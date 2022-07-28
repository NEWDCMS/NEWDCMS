using DCMS.Services.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace DCMS.Web.Controllers
{
    public partial class ScheduleTaskController : Controller
    {
        private readonly IScheduleTaskService _scheduleTaskService;

        public ScheduleTaskController(IScheduleTaskService scheduleTaskService)
        {
            _scheduleTaskService = scheduleTaskService;
        }

        [HttpPost]
        public virtual IActionResult RunTask(string taskType)
        {
            var scheduleTask = _scheduleTaskService.GetTaskByType(taskType);
            if (scheduleTask == null)
            {
                //schedule task cannot be loaded
                return NoContent();
            }

            var task = new Task(scheduleTask);
            task.Execute();

            return NoContent();
        }
    }
}