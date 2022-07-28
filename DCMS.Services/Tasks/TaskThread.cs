using DCMS.Core;
using DCMS.Core.Domain.Common;
using DCMS.Core.Domain.Tasks;
using DCMS.Core.Infrastructure;
using DCMS.Services.Logging;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace DCMS.Services.Tasks
{
    /// <summary>
    /// 表示任务线程
    /// </summary>
    public partial class TaskThread : IDisposable
    {
        #region Fields

        private static readonly string _scheduleTaskUrl;
        private static readonly int? _timeout;

        private readonly Dictionary<string, string> _tasks;
        private Timer _timer;
        private bool _disposed;

        #endregion

        #region Ctor

        static TaskThread()
        {
            _scheduleTaskUrl = $"{EngineContext.Current.Resolve<IStoreContext>().CurrentStore.Url}{DCMSTaskDefaults.ScheduleTaskPath}";
            _timeout = EngineContext.Current.Resolve<CommonSettings>().ScheduleTaskRunTimeout;
        }

        internal TaskThread()
        {
            _tasks = new Dictionary<string, string>();
            Seconds = 10 * 60;
        }

        #endregion

        #region Utilities

        private void Run()
        {
            if (Seconds <= 0)
            {
                return;
            }

            StartedUtc = DateTime.UtcNow;
            IsRunning = true;

            foreach (var taskName in _tasks.Keys)
            {
                var taskType = _tasks[taskName];
                try
                {
                    /*
                    //创建和配置客户端
                    var client = EngineContext.Current.Resolve<IHttpClientFactory>().CreateClient(DCMSHttpDefaults.DefaultHttpClient);
                    if (_timeout.HasValue)
                        client.Timeout = TimeSpan.FromMilliseconds(_timeout.Value);
                    //发送数据
                    //"http://192.168.1.42:9999/scheduletask/runtask"
                    var data = new FormUrlEncodedContent(new[] { new KeyValuePair<string, string>(nameof(taskType), taskType) });
                    client.PostAsync(_scheduleTaskUrl, data).Wait();
                    */
                    var scheduleTaskService = EngineContext.Current.Resolve<IScheduleTaskService>();
                    var scheduleTask = scheduleTaskService.GetTaskByType(taskType);
                    if (scheduleTask != null)
                    {
                        var task = new Task(scheduleTask);
                        task.Execute(false, false);
                    }
                }
                catch (Exception ex)
                {
                    var _serviceScopeFactory = EngineContext.Current.Resolve<IServiceScopeFactory>();
                    using (var scope = _serviceScopeFactory.CreateScope())
                    {
                        // 解析
                        var logger = scope.ServiceProvider.GetRequiredService<ILogger>();

                        var storeContext = scope.ServiceProvider.GetRequiredService<IStoreContext>();

                        var message = ex.InnerException?.GetType() == typeof(TaskCanceledException) ? "TimeoutError" : ex.Message;

                        message = string.Format("Error:{0}{1}{2}{3}{4}", taskName, message, taskType, storeContext.CurrentStore.Name, _scheduleTaskUrl);

                        logger.Error(message, ex);
                    }
                }
            }

            IsRunning = false;
        }

        private void TimerHandler(object state)
        {
            try
            {
                _timer.Change(-1, -1);
                Run();

                if (RunOnlyOnce)
                {
                    Dispose();
                }
                else
                {
                    _timer.Change(Interval, Interval);
                }
            }
            catch
            {
                // ignore
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// 释放实例
        /// </summary>
        public void Dispose()
        {
            if (_timer == null || _disposed)
            {
                return;
            }

            lock (this)
            {
                _timer.Dispose();
                _timer = null;
                _disposed = true;
            }
        }

        /// <summary>
        /// 初始定时器
        /// </summary>
        public void InitTimer()
        {
            if (_timer == null)
            {
                _timer = new Timer(TimerHandler, null, InitInterval, Interval);
            }
        }

        /// <summary>
        /// 添加任务到任务线程
        /// </summary>
        /// <param name="task">The task to be added</param>
        public void AddTask(ScheduleTask task)
        {
            if (!_tasks.ContainsKey(task.Name))
            {
                _tasks.Add(task.Name, task.Type);
            }
        }

        #endregion

        #region Properties

        /// <summary>
        /// 获取或设置运行任务的间隔（秒）
        /// </summary>
        public int Seconds { get; set; }

        /// <summary>
        /// 获取或设置计时器首次启动前的间隔
        /// </summary>
        public int InitSeconds { get; set; }

        /// <summary>
        /// 获取或设置线程启动的日期时间
        /// </summary>
        public DateTime StartedUtc { get; private set; }

        /// <summary>
        /// 获取或设置一个值，该值指示线程是否正在运行
        /// </summary>
        public bool IsRunning { get; private set; }

        /// <summary>
        /// 获取运行任务的间隔（毫秒）
        /// </summary>
        public int Interval
        {
            get
            {
                //如果某人输入的时间超过“2147483”秒，则可能引发异常（超过int.MaxValue）
                var interval = Seconds * 1000;
                if (interval <= 0)
                {
                    interval = int.MaxValue;
                }

                return interval;
            }
        }

        /// <summary>
        /// 获取开始任务的到期时间间隔（以毫秒为单位）
        /// </summary>
        public int InitInterval
        {
            get
            {
                //如果某人输入的时间少于“0”秒，则可能引发异常
                var interval = InitSeconds * 1000;
                if (interval <= 0)
                {
                    interval = 0;
                }

                return interval;
            }
        }

        /// <summary>
        /// 获取或设置一个值，该值指示线程是否只运行一次（在应用程序启动时）
        /// </summary>
        public bool RunOnlyOnce { get; set; }

        #endregion

    }
}
