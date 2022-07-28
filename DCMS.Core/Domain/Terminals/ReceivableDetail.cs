using System;
namespace DCMS.Core.Domain.Terminals
{
    /// <summary>
    ///应收款历史记录表
    /// </summary>
    public class ReceivableDetail : BaseEntity
    {
        /// <summary>
        /// 应收款Id
        /// </summary>
        public int ReceivableId { get; set; }


        /// <summary>
        /// 终端Id
        /// </summary>
        public int TerminalId { get; set; }

        /// <summary>
        /// （原）欠款
        /// </summary>
        public decimal OldOweCash { get; set; }

        /// <summary>
        /// （原）收款
        /// </summary>
        public decimal OldAdvanceCash { get; set; }

        /// <summary>
        /// （新）欠款
        /// </summary>
        public decimal NewOweCash { get; set; }

        /// <summary>
        /// （新）收款
        /// </summary>
        public decimal NewAdvanceCash { get; set; }

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
        public byte Status { get; set; }

        /// <summary>
        /// 欠款时间
        /// </summary>
        public DateTime BalanceDate { get; set; }
        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreatedOnUtc { get; set; }
    }
}
