using DCMS.Core;
using DCMS.Core.Caching;
using DCMS.Core.Domain.Tasks;
using DCMS.Core.Infrastructure;
using DCMS.Services.Logging;
using System;
using System.Linq;

namespace DCMS.Services.Tasks
{
    /// <summary>
    /// 用于表示计划任务
    /// </summary>
    public partial class Task
    {

        private bool? _enabled;

        public Task(ScheduleTask task)
        {
            ScheduleTask = task;
        }


        /// <summary>
        /// 初始化并执行任务
        /// </summary>
        private void ExecuteTask()
        {
            var scheduleTaskService = EngineContext.Current.Resolve<IScheduleTaskService>();

            if (!Enabled)
            {
                return;
            }

            var type = Type.GetType(ScheduleTask.Type) ??
                //确保仅指定类型名时它工作正常（不需要完全限定名）
                AppDomain.CurrentDomain.GetAssemblies()
                .Select(a => a.GetType(ScheduleTask.Type))
                .FirstOrDefault(t => t != null);
            if (type == null)
            {
                throw new Exception($"Schedule task ({ScheduleTask.Type}) cannot by instantiated");
            }

            object instance = null;
            try
            {
                instance = EngineContext.Current.Resolve(type);
            }
            catch
            {
                //try resolve
            }

            if (instance == null)
            {
                //not resolved
                instance = EngineContext.Current.ResolveUnregistered(type);
            }

            var task = instance as IScheduleTask;
            if (task == null)
            {
                return;
            }

            ScheduleTask.LastStartUtc = DateTime.UtcNow;
            //更新适当的日期时间属性
            scheduleTaskService.UpdateTask(ScheduleTask);
            task.Execute();
            ScheduleTask.LastEndUtc = ScheduleTask.LastSuccessUtc = DateTime.UtcNow;
            //更新适当的日期时间属性
            scheduleTaskService.UpdateTask(ScheduleTask);
        }

        /// <summary>
        /// 任务是否已经运行?
        /// </summary>
        /// <param name="scheduleTask">Schedule task</param>
        /// <returns>Result</returns>
        protected virtual bool IsTaskAlreadyRunning(ScheduleTask scheduleTask)
        {
            //首次运行任务
            if (!scheduleTask.LastStartUtc.HasValue && !scheduleTask.LastEndUtc.HasValue)
            {
                return false;
            }

            var lastStartUtc = scheduleTask.LastStartUtc ?? DateTime.UtcNow;

            //任务已经完成
            if (scheduleTask.LastEndUtc.HasValue && lastStartUtc < scheduleTask.LastEndUtc)
            {
                return false;
            }

            //上次任务未完成
            if (lastStartUtc.AddSeconds(scheduleTask.Seconds) <= DateTime.UtcNow)
            {
                return false;
            }

            return true;
        }


        /// <summary>
        /// 执行任务
        /// </summary>
        /// <param name="throwException">在发生错误时是否应引发异常</param>
        /// <param name="ensureRunOncePerPeriod">是否应确保此任务在每个运行期间运行一次</param>
        public void Execute(bool throwException = false, bool ensureRunOncePerPeriod = true)
        {
            if (ScheduleTask == null || !Enabled)
            {
                return;
            }

            if (ensureRunOncePerPeriod)
            {
                //任务已经运行
                if (IsTaskAlreadyRunning(ScheduleTask))
                {
                    return;
                }

                //验证
                if (ScheduleTask.LastStartUtc.HasValue && (DateTime.UtcNow - ScheduleTask.LastStartUtc).Value.TotalSeconds < ScheduleTask.Seconds)
                {
                    //太早
                    return;
                }
            }

            try
            {
                //获取过期时间
                var expirationInSeconds = Math.Min(ScheduleTask.Seconds, 300) - 1;
                var expiration = TimeSpan.FromSeconds(expirationInSeconds);

                //用锁执行任务
                var locker = EngineContext.Current.Resolve<IRedLocker>();
                locker.PerformActionWithLock(ScheduleTask.Type, expiration, ExecuteTask);
            }
            catch (Exception exc)
            {
                var scheduleTaskService = EngineContext.Current.Resolve<IScheduleTaskService>();
                var storeContext = EngineContext.Current.Resolve<IStoreContext>();
                var scheduleTaskUrl = $"{storeContext.CurrentStore.Url}{DCMSTaskDefaults.ScheduleTaskPath}";

                //出错时禁用自动任务，并发送告警
                ScheduleTask.Enabled = !ScheduleTask.StopOnError;
                ScheduleTask.LastEndUtc = DateTime.UtcNow;
                scheduleTaskService.UpdateTask(ScheduleTask);

                var message = string.Format("执行计划任务 \"{0}\" 时出错:{1} ，来自：{2}，经销商：{3}，地址：{4}", ScheduleTask.Name,
                    exc.Message, ScheduleTask.Type, storeContext.CurrentStore.Name, scheduleTaskUrl);

                //记录错误
                var logger = EngineContext.Current.Resolve<ILogger>();
                logger.Error(message, exc);

                //启用异常则抛出
                if (throwException)
                {
                    throw;
                }
            }
        }


        /// <summary>
        /// 计划任务
        /// </summary>
        public ScheduleTask ScheduleTask { get; }

        /// <summary>
        /// 任务是否已启用
        /// </summary>
        public bool Enabled
        {
            get
            {
                if (!_enabled.HasValue)
                {
                    _enabled = ScheduleTask?.Enabled;
                }

                return _enabled.HasValue && _enabled.Value;
            }

            set => _enabled = value;
        }


    }
}
