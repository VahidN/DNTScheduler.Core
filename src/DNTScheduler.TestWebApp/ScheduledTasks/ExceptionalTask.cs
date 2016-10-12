using System.IO;
using System.Threading.Tasks;
using DNTScheduler.Core.Contracts;

namespace DNTScheduler.TestWebApp.ScheduledTasks
{
    public class ExceptionalTask : IScheduledTask
    {
        public bool IsShuttingDown { get; set; }

        public Task RunAsync()
        {
            throw new FileNotFoundException("Couldn't find the xyz.abc file.");
        }
    }
}