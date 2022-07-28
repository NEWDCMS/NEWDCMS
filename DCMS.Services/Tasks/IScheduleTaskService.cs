using DCMS.Core.Domain.Tasks;
using System.Collections.Generic;

namespace DCMS.Services.Tasks
{
    /// <summary>
    /// 任务实体操作接口
    /// </summary>
    public partial interface IScheduleTaskService
    {

        void DeleteTask(ScheduleTask task);


        ScheduleTask GetTaskById(int taskId);


        ScheduleTask GetTaskByType(string type);


        IList<ScheduleTask> GetAllTasks(bool showHidden = false);

        void InsertTask(ScheduleTask task);


        void UpdateTask(ScheduleTask task);
    }
}
