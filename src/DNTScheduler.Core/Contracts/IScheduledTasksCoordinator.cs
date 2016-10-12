using System;
using System.Threading.Tasks;

namespace DNTScheduler.Core.Contracts
{
    /// <summary>
    /// Scheduled Tasks Manager
    /// </summary>
    public interface IScheduledTasksCoordinator
    {
        /// <summary>
        /// Fires on unhandled exceptions.
        /// </summary>
        Action<Exception, string> OnUnexpectedException { set; get; }

        /// <summary>
        /// Starts the scheduler.
        /// </summary>
        void Start();

        /// <summary>
        /// Stops the scheduler.
        /// </summary>
        Task Stop();
    }
}