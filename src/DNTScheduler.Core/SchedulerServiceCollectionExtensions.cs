using System;
using DNTScheduler.Core.Contracts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace DNTScheduler.Core
{
    /// <summary>
    ///  DNTScheduler ServiceCollection Extensions
    /// </summary>
    public static class SchedulerServiceCollectionExtensions
    {
        /// <summary>
        /// Adds default DNTScheduler providers.
        /// </summary>
        public static void AddDNTScheduler(this IServiceCollection services, Action<ScheduledTasksStorage> options)
        {
            services.CheckArgumentNull(nameof(services));
            options.CheckArgumentNull(nameof(options));

            services.TryAddSingleton<IThisApplication, ThisApplication>();
            services.TryAddSingleton<IJobsRunnerTimer, JobsRunnerTimer>();
            services.TryAddSingleton<IScheduledTasksCoordinator, ScheduledTasksCoordinator>();

            configTasks(services, options);
        }

        private static void configTasks(IServiceCollection services, Action<ScheduledTasksStorage> options)
        {
            var storage = new ScheduledTasksStorage();
            options(storage);

            foreach (var task in storage.Tasks)
            {
                services.TryAddTransient(task.TaskType);
            }

            if (storage.AddPingTask)
            {
                storage.AddScheduledTask<PingTask>(runAt: utcNow => utcNow.Second == 1);
                services.TryAddSingleton<PingTask>();
            }

            services.TryAddSingleton(Options.Create(storage));
        }
    }
}