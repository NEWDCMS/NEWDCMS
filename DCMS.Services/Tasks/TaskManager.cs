using DCMS.Core.Infrastructure;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace DCMS.Services.Tasks
{
    /// <summary>
    /// 表示任务管理器
    /// </summary>
    public partial class TaskManager
    {
        #region Fields

        private readonly List<TaskThread> _taskThreads = new List<TaskThread>();

        #endregion

        #region Ctor

        private TaskManager()
        {
        }

        #endregion

        #region Methods

        /// <summary>
        /// 初始任务管理器
        /// </summary>
        public void Initialize()
        {
            _taskThreads.Clear();

            var taskService = EngineContext.Current.Resolve<IScheduleTaskService>();

            var scheduleTasks = taskService
                .GetAllTasks()
                .OrderBy(x => x.Seconds)
                .ToList();

            foreach (var scheduleTask in scheduleTasks)
            {
                var taskThread = new TaskThread
                {
                    Seconds = scheduleTask.Seconds
                };

                //有时任务周期可以设置为几个小时（甚至几天）
                //在这种情况下，它被运行的可能性很小（应用程序可以重新启动），在启动被中断的任务之前计算时间
                if (scheduleTask.LastStartUtc.HasValue)
                {
                    //距离上次启动还有几秒
                    var secondsLeft = (DateTime.UtcNow - scheduleTask.LastStartUtc).Value.TotalSeconds;

                    if (secondsLeft >= scheduleTask.Seconds)
                    {
                        //立即运行
                        taskThread.InitSeconds = 0;
                    }
                    else
                    {
                        //计算开始时间并取整它（EnsureRunncePerPeriod）
                        taskThread.InitSeconds = (int)(scheduleTask.Seconds - secondsLeft) + 1;
                    }
                }
                else
                {
                    //第一次开始的任务
                    taskThread.InitSeconds = scheduleTask.Seconds;
                }

                taskThread.AddTask(scheduleTask);
                _taskThreads.Add(taskThread);
            }
        }

        /// <summary>
        /// 开始任务管理器
        /// </summary>
        public void Start()
        {
            foreach (var taskThread in _taskThreads)
            {
                taskThread.InitTimer();
            }
        }

        /// <summary>
        /// 停止任务管理器
        /// </summary>
        public void Stop()
        {
            foreach (var taskThread in _taskThreads)
            {
                taskThread.Dispose();
            }
            _taskThreads.Clear();
        }

        #endregion

        #region Properties

        /// <summary>
        /// 获取任务管理器实例
        /// </summary>
        public static TaskManager Instance { get; } = new TaskManager();


        /// <summary>
        /// 获取此任务管理器的任务线程列表
        /// </summary>
        public IList<TaskThread> TaskThreads => new ReadOnlyCollection<TaskThread>(_taskThreads);

        #endregion
    }
}
