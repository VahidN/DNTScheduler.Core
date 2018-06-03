using System.IO;
using DNTScheduler.Core;
using DNTScheduler.TestWebApp.ScheduledTasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;

namespace DNTScheduler.TestWebApp
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
            services.AddDirectoryBrowser();

            services.AddDNTScheduler(options =>
            {
                // DNTScheduler needs a ping service to keep it alive. Set it to false if you don't need it.
                // options.AddPingTask = true;

                options.AddScheduledTask<DoBackupTask>(
                    runAt: utcNow =>
                    {
                        var now = utcNow.AddHours(3.5);
                        return now.Day % 3 == 0 && now.Hour == 0 && now.Minute == 1 && now.Second == 1;
                    },
                    order: 2);

                options.AddScheduledTask<SendEmailsTask>(
                    runAt: utcNow =>
                    {
                        var now = utcNow.AddHours(3.5);
                        return now.Minute % 2 == 0 && now.Second == 1;
                    },
                    order: 1);

                options.AddScheduledTask<ExceptionalTask>(utcNow => utcNow.Second == 1);
                options.AddScheduledTask<LongRunningTask>(utcNow => utcNow.Second == 1);
            });
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddDebug(minLevel: LogLevel.Debug);

            var logger = loggerFactory.CreateLogger(typeof(Startup));
            app.UseDNTScheduler(onUnexpectedException: (ex, name) =>
            {
                logger.LogError(0, ex, $"Failed running {name}");
            });

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            // Serve wwwroot as root
            app.UseFileServer();

            app.UseFileServer(new FileServerOptions
            {
                // Set root of file server
                FileProvider = new PhysicalFileProvider(Path.Combine(Directory.GetCurrentDirectory(), "bower_components")),
                // Only react to requests that match this path
                RequestPath = "/bower_components",
                // Don't expose file system
                EnableDirectoryBrowsing = false
            });


            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}