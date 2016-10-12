using DNTScheduler.Core;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace DNTScheduler.TestWebApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly IOptions<ScheduledTasksStorage> _tasksStorage;

        public HomeController(IOptions<ScheduledTasksStorage> tasksStorage)
        {
            _tasksStorage = tasksStorage;
        }

        public IActionResult Index()
        {
            return View(model: _tasksStorage.Value.Tasks);
        }
    }
}