using DNTScheduler.Core.Contracts;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace DNTScheduler.Core
{
    /// <summary>
    /// Application Builder Extensions
    /// </summary>
    public static class ApplicationBuilderExtensions
    {
        /// <summary>
        /// Enables DNTScheduler.Core to access ApplicationServices.
        /// </summary>
        public static IApplicationBuilder UseDNTScheduler(this IApplicationBuilder app)
        {
            var scheduledTasksCoordinator = app.ApplicationServices.GetService<IScheduledTasksCoordinator>();
            scheduledTasksCoordinator.Start();

            return app;
        }
    }
}