using DCMS.Core.Caching;
using DCMS.Core.Domain.Tasks;
using DCMS.Core.Infrastructure.DependencyManagement;
using DCMS.Services.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using DCMS.Services.Caching;



namespace DCMS.Services.Tasks
{

    /// <summary>
    /// 用于计划任务管理服务
    /// </summary>
    public partial class ScheduleTaskService : BaseService, IScheduleTaskService
    {
        public ScheduleTaskService(IServiceGetter getter,
                  IStaticCacheManager cacheManager,
                  IEventPublisher eventPublisher) : base(getter, cacheManager, eventPublisher)
        {
        }

        /// <summary>
        /// 删除计划任务
        /// </summary>
        /// <param name="task"></param>
        public virtual void DeleteTask(ScheduleTask task)
        {
            if (task == null)
            {
                throw new ArgumentNullException("task");
            }

            var uow = ScheduleTaskRepository.UnitOfWork;
            ScheduleTaskRepository.Delete(task);
            uow.SaveChanges();
        }

        /// <summary>
        /// 获取计划任务
        /// </summary>
        /// <param name="taskId"></param>
        /// <returns></returns>
        public virtual ScheduleTask GetTaskById(int taskId)
        {
            if (taskId == 0)
            {
                return null;
            }

            return ScheduleTaskRepository.ToCachedGetById(taskId);
        }

        /// <summary>
        /// 根据类型获取计划任务
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public virtual ScheduleTask GetTaskByType(string type)
        {
            if (string.IsNullOrWhiteSpace(type))
            {
                return null;
            }

            var query = ScheduleTaskRepository.Table;
            query = query.Where(st => st.Type == type);
            query = query.OrderByDescending(t => t.Id);

            var task = query.FirstOrDefault();
            return task;
        }

        /// <summary>
        /// 获取全部计划任务
        /// </summary>
        /// <param name="showHidden"></param>
        /// <returns></returns>
        public virtual IList<ScheduleTask> GetAllTasks(bool showHidden = false)
        {
            var query = ScheduleTaskRepository.Table;
            if (!showHidden)
            {
                query = query.Where(t => t.Enabled);
            }
            query = query.OrderByDescending(t => t.Seconds);
            var tasks = query.ToList();
            //var tasks2 = ScheduleTaskRepository.QueryFromSql<ScheduleTask>("select * from ScheduleTask").ToList();
            return tasks;
        }

        /// <summary>
        /// 添加计划任务
        /// </summary>
        /// <param name="task"></param>
        public virtual void InsertTask(ScheduleTask task)
        {
            if (task == null)
            {
                throw new ArgumentNullException("task");
            }

            var uow = ScheduleTaskRepository.UnitOfWork;
            ScheduleTaskRepository.Insert(task);
            uow.SaveChanges();

        }

        /// <summary>
        /// 更新计划任务
        /// </summary>
        /// <param name="task"></param>
        public virtual void UpdateTask(ScheduleTask task)
        {
            if (task == null)
            {
                throw new ArgumentNullException("task");
            }

            var uow = ScheduleTaskRepository.UnitOfWork;
            ScheduleTaskRepository.Update(task);
            uow.SaveChanges();
        }
    }
}
