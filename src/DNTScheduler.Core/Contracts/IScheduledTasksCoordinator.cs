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
        /// Starts the scheduler.
        /// </summary>
        void Start();

        /// <summary>
        /// Stops the scheduler.
        /// </summary>
        Task Stop();
    }
}