using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using DNTScheduler.Core.Contracts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Hosting;

namespace DNTScheduler.Core
{
    /// <summary>
    /// Scheduled Tasks Manager
    /// </summary>
    public sealed class ScheduledTasksCoordinator : IScheduledTasksCoordinator
    {
        // the 30 seconds is for the entire app to tie up what it's doing.
        private const int TimeToFinish = 30 * 1000;

        private readonly IJobsRunnerTimer _jobsRunnerTimer;
        private readonly ILogger<ScheduledTasksCoordinator> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly IOptions<ScheduledTasksStorage> _tasksStorage;
        private bool _isShuttingDown;

        /// <summary>
        /// Scheduled Tasks Manager
        /// </summary>
        public ScheduledTasksCoordinator(
            ILogger<ScheduledTasksCoordinator> logger,
            IHostApplicationLifetime applicationLifetime,
            IOptions<ScheduledTasksStorage> tasksStorage,
            IJobsRunnerTimer jobsRunnerTimer,
            IServiceProvider serviceProvider)
        {
            _logger = logger;
            _tasksStorage = tasksStorage;
            _jobsRunnerTimer = jobsRunnerTimer;
            _serviceProvider = serviceProvider;
            applicationLifetime.ApplicationStopping.Register(() =>
            {
                _logger.LogInformation("Application is stopping ... .");
                disposeResources().Wait();
            });
        }

        /// <summary>
        /// Starts the scheduler.
        /// </summary>
        public void Start()
        {
            if (_jobsRunnerTimer.IsRunning)
            {
                return;
            }

            _jobsRunnerTimer.OnThreadPoolTimerCallback = () =>
            {
                var now = DateTime.UtcNow;

                var tasks = new List<Task>();
                foreach (var taskStatus in _tasksStorage.Value.Tasks
                                                                .Where(x => x.RunAt(now))
                                                                .OrderBy(x => x.Order))
                {
                    if (_isShuttingDown)
                    {
                        return;
                    }

                    if (taskStatus.IsRunning)
                    {
                        _logger.LogInformation($"Ignoring `{taskStatus}` task. It's still running.");
                        continue;
                    }

                    tasks.Add(Task.Run(() => runTask(taskStatus, now)));
                }

                if (tasks.Any())
                {
                    Task.WaitAll(tasks.ToArray());
                }
            };
            _jobsRunnerTimer.Start();
        }

        /// <summary>
        /// Stops the scheduler.
        /// </summary>
        public Task Stop()
        {
            return disposeResources();
        }

        private async Task disposeResources()
        {
            if (_isShuttingDown)
            {
                return;
            }

            try
            {
                _isShuttingDown = true;

                foreach (var task in _tasksStorage.Value.Tasks.Where(x => x.TaskInstance != null))
                {
                    task.TaskInstance.IsShuttingDown = true;
                }

                var timeOut = TimeToFinish;
                while (_tasksStorage.Value.Tasks.Any(x => x.IsRunning) && timeOut >= 0)
                {
                    // still running ...
                    await Task.Delay(50);
                    timeOut -= 50;
                }
            }
            finally
            {
                _jobsRunnerTimer.Stop();
                await _serviceProvider.GetService<MySitePingClient>()?.WakeUp("/");
            }
        }

        private void runTask(ScheduledTaskStatus taskStatus, DateTime now)
        {
            using (var serviceScope = _serviceProvider.GetRequiredService<IServiceScopeFactory>().CreateScope())
            {
                var scheduledTask = (IScheduledTask)serviceScope.ServiceProvider.GetRequiredService(taskStatus.TaskType);
                taskStatus.TaskInstance = scheduledTask;
                var name = scheduledTask.GetType().GetTypeInfo().FullName;

                try
                {
                    if (_isShuttingDown)
                    {
                        return;
                    }

                    taskStatus.IsRunning = true;
                    taskStatus.LastRun = now;

                    _logger.LogInformation($"Start running `{name}` task @ {now}.");
                    scheduledTask.RunAsync().Wait();

                    _logger.LogInformation($"Finished running `{name}` task @ {now}.");
                    taskStatus.IsLastRunSuccessful = true;
                }
                catch (Exception ex)
                {
                    _logger.LogCritical(0, ex, $"Failed running {name}");
                    taskStatus.IsLastRunSuccessful = false;
                    taskStatus.LastException = ex;
                }
                finally
                {
                    taskStatus.IsRunning = false;
                    taskStatus.TaskInstance = null;
                }
            }
        }
    }
}