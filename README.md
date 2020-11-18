# DNTScheduler.Core

DNTScheduler.Core is a lightweight ASP.NET Core's background tasks runner and scheduler.

## Install via NuGet

To install DNTScheduler, run the following command in the Package Manager Console:

```
PM> Install-Package DNTScheduler.Core
```

You can also view the [package page](http://www.nuget.org/packages/DNTScheduler.Core/) on NuGet.

## Usage

- After installing the DNTScheduler.Core package, to define a new task, [create a new class](/src/DNTScheduler.TestWebApp/ScheduledTasks/) that implements the `IScheduledTask` interface:

```csharp
namespace DNTScheduler.TestWebApp.ScheduledTasks
{
    public class DoBackupTask : IScheduledTask
    {
        private readonly ILogger<DoBackupTask> _logger;

        public DoBackupTask(ILogger<DoBackupTask> logger)
        {
            _logger = logger;
        }

        public bool IsShuttingDown { get; set; }

        public async Task RunAsync()
        {
            if (this.IsShuttingDown)
            {
                return;
            }

            _logger.LogInformation("Running Do Backup");

            await Task.Delay(TimeSpan.FromMinutes(3));
        }
    }
}
```

The `RunAsync` method represents the task's logic.

- To register this new task, call `services.AddDNTScheduler();` method in your [Startup class](/src/DNTScheduler.TestWebApp/Startup.cs):

```csharp
namespace DNTScheduler.TestWebApp
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();
            services.AddDirectoryBrowser();

            services.AddDNTScheduler(options =>
            {
                // DNTScheduler needs a ping service to keep it alive.
                // If you don't need it, don't add it!
                options.AddPingTask(siteRootUrl: "https://localhost:5001");

                options.AddScheduledTask<DoBackupTask>(
                    runAt: utcNow =>
                    {
                        var now = utcNow.AddHours(3.5);
                        return now.Day % 3 == 0 && now.Hour == 0 && now.Minute == 1 && now.Second == 1;
                    },
                    order: 2);
            });
        }
```

`AddDNTScheduler` method, adds this new task to the list of the defined tasks. Also its first parameter defines the custom logic of the running intervals of this task. It's a callback method that will be called every second and provides the utcNow value. If it returns true, the job will be executed.
If you have multiple jobs at the same time, the `order` parameter's value indicates the order of their execution.

- Finally to start running the registered tasks, call `app.UseDNTScheduler()` method in your [Startup class](/src/DNTScheduler.TestWebApp/Startup.cs):

```csharp
public void Configure(IApplicationBuilder app)
{
    app.UseDNTScheduler();
```

Please follow the [DNTScheduler.TestWebApp](/src/DNTScheduler.TestWebApp) sample for more details.
