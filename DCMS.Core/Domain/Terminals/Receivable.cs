using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace DCMS.Core.Domain.Terminals
{
    /// <summary>
    ///应收款
    /// </summary>
    public class Receivable : BaseEntity
    {


        /// <summary>
        /// 终端Id
        /// </summary>
        public int TerminalId { get; set; }

        /// <summary>
        /// 初始欠款
        /// </summary>
        public decimal OweCash { get; set; }

        /// <summary>
        /// 收款
        /// </summary>
        public decimal AdvanceCash { get; set; }

        /// <summary>
        /// 业务员
        /// </summary>
        public int OperationUserId { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        public string Remark { get; set; }

        /// <summary>
        /// 状态
        /// </summary>
        [Column(TypeName = "BIT(1)")]
        public bool Status { get; set; }

        /// <summary>
        /// 是否初始
        /// </summary>
        [Column(TypeName = "BIT(1)")]
        public bool Inited { get; set; }

        /// <summary>
        /// 是否删除
        /// </summary>
        [Column(TypeName = "BIT(1)")]
        public bool Deleted { get; set; }

        /// <summary>
        /// 欠款时间
        /// </summary>
        public DateTime BalanceDate { get; set; }

        /// <summary>
        /// 创建人
        /// </summary>
        public int CreatedUserId { get; set; }
        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreatedOnUtc { get; set; }
    }
}
