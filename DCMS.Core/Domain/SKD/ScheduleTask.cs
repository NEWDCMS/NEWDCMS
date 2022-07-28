
using System;
using System.ComponentModel.DataAnnotations.Schema;


namespace DCMS.Core.Domain.Tasks
{

    /// <summary>
    /// 用于计划任务
    /// </summary>
    public class ScheduleTask : BaseEntity
    {


        /// <summary>
        /// 获取设置任务名
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 获取或设置运行周期（以秒为单位）
        /// </summary>
        public int Seconds { get; set; }

        /// <summary>
        /// 获取或设置运行 ITask 类
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// 是否启用
        /// </summary>
        [Column(TypeName = "BIT(1)")]
        public bool Enabled { get; set; }

        /// <summary>
        /// 获取或设置指示在某个错误上是否应停止任务
        /// </summary>
        [Column(TypeName = "BIT(1)")]
        public bool StopOnError { get; set; }

        /// <summary>
        /// 最后一次开始时间
        /// </summary>

        public DateTime? LastStartUtc { get; set; }

        /// <summary>
        /// 最后一次结束时间
        /// </summary>

        public DateTime? LastEndUtc { get; set; }

        /// <summary>
        /// 最近一次成功运行时间
        /// </summary>

        public DateTime? LastSuccessUtc { get; set; }
    }
}
