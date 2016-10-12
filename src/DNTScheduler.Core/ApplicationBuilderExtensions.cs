using System;
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
        public static IApplicationBuilder UseDNTScheduler(
            this IApplicationBuilder app,
            Action<Exception, string> onUnexpectedException = null)
        {
            app.CheckArgumentNull(nameof(app));

            app.UseMiddleware<SiteRootUrlMiddleware>();

            var scheduledTasksCoordinator = app.ApplicationServices.GetService<IScheduledTasksCoordinator>();
            scheduledTasksCoordinator.OnUnexpectedException = onUnexpectedException;
            scheduledTasksCoordinator.Start();

            return app;
        }
    }
}