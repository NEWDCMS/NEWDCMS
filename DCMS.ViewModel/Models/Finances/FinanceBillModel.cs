using DCMS.Web.Framework.Models;
using System;


namespace DCMS.ViewModel.Models.Finances
{
    /// <summary>
    /// 财务项表
    /// </summary>
    public class FinanceBillModel : BaseEntityModel
    {
        /// <summary>
        /// 经销商Id
        /// </summary>
        //移除 = 0;

        /// <summary>
        /// 单据类型
        /// </summary>
        public string BillType { get; set; }

        /// <summary>
        /// 单据表主键
        /// </summary>
        public int? BillId { get; set; } = 0;

        /// <summary>
        /// 单据编号
        /// </summary>
        public string BillNumber { get; set; }

        /// <summary>
        /// 财务科目编号
        /// </summary>
        public string FinanceSubjectCode { get; set; }

        /// <summary>
        /// 实际金额
        /// </summary>
        public decimal? ActualAmount { get; set; } = 0;

        /// <summary>
        /// 状态
        /// </summary>
        public string Status { get; set; }

        /// <summary>
        /// 创建人员
        /// </summary>
        public int? AddUserId { get; set; } = 0;

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime? AddDate { get; set; }

        /// <summary>
        /// 修改人员
        /// </summary>
        public int? EditUserId { get; set; } = 0;

        /// <summary>
        /// 修改时间
        /// </summary>
        public DateTime? EditDate { get; set; }



    }
}
