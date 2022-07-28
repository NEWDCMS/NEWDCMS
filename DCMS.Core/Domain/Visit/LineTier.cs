using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;

namespace DCMS.Core.Domain.Visit
{
    /// <summary>
    /// 用于表示拜访线路类别
    /// </summary>
    public partial class LineTier : BaseEntity
    {
        private ICollection<LineTierOption> _lineTierOptions;


        /// <summary>
        /// 线路类别名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 是否启用
        /// </summary>
        [Column(TypeName = "BIT(1)")]
        public bool Enabled { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        public string Remark { get; set; }

        [JsonIgnore]
        public virtual ICollection<LineTierOption> LineTierOptions
        {
            get { return _lineTierOptions ?? (_lineTierOptions = new List<LineTierOption>()); }
            protected set { _lineTierOptions = value; }
        }
    }


    /// <summary>
    /// 用于表示线路访问
    /// </summary>

    public partial class LineTierOption : BaseEntity
    {
        /// <summary>
        /// 类别
        /// </summary>

        public int LineTierId { get; set; }

        /// <summary>
        /// 拜访顺序
        /// </summary>
        public int Order { get; set; }

        /// <summary>
        /// 客户(终端)
        /// </summary>
        public int TerminalId { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreatedOnUtc { get; set; }



        public virtual LineTier LineTier { get; set; }
    }



    /// <summary>
    /// 用于表示业务员线路指派（该表是映射表，存放业务员和线路的分配关系）
    /// </summary>
    public partial class UserLineTierAssign : BaseEntity
    {
        /// <summary>
        /// 线路顺序
        /// </summary>
        public int Order { get; set; }

        /// <summary>
        ///业务员
        /// </summary>
        public int UserId { get; set; }

        /// <summary>
        /// 线路
        /// </summary>
        public int LineTierId { get; set; }

        ///// <summary>
        ///// 客户数量
        ///// </summary>
        //public int Quantity { get; set; }


        public virtual LineTier LineTier { get; set; }

        //public virtual User User { get; set; }

    }


}
