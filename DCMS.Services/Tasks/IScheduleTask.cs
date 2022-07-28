namespace DCMS.Services.Tasks
{
    /// <summary>
    /// 计划任务应实现的接口
    /// </summary>
    public partial interface IScheduleTask
    {
        /// <summary>
        /// 执行任务
        /// </summary>
        void Execute();
    }
}
