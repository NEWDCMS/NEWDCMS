using DCMS.Web.Framework.Models;
using System;

namespace DCMS.ViewModel.Models.Finances
{
    /// <summary>
    /// 用于单据汇总（用于单据的收付款汇总）
    /// </summary>
    public class BillSummaryModel : BaseEntityModel
    {
        /// <summary>
        /// 单据Id
        /// </summary>
        public int BillId { get; set; } = 0;
        /// <summary>
        /// 单据编号
        /// </summary>
        public string BillNumber { get; set; }
        /// <summary>
        /// 单据类型枚举名
        /// </summary>
        public string BillTypeName { get; set; }
        /// <summary>
        /// 单据类型
        /// </summary>
        public int BillTypeId { get; set; } = 0;

        public string BillLink { get; set; }


        //客户", "客户")]
        public int CustomerId { get; set; } = 0;
        //客户", "客户")]
        public string CustomerName { get; set; }
        //终端编号", "终端编号")]
        public string CustomerPointCode { get; set; }
        public int DistrictId { get; set; } = 0;


        /// <summary>
        /// 开单时间
        /// </summary>
        public DateTime MakeBillDate { get; set; }
        /// <summary>
        /// 单据金额
        /// </summary>
        public decimal? Amount { get; set; } = 0;
        /// <summary>
        /// 优惠金额
        /// </summary>
        public decimal? DiscountAmount { get; set; } = 0;
        /// <summary>
        /// 已收/已付金额
        /// </summary>
        public decimal? PaymentedAmount { get; set; } = 0;
        /// <summary>
        /// 尚欠金额
        /// </summary>
        public decimal? ArrearsAmount { get; set; } = 0;
        /// <summary>
        /// 备注
        /// </summary>
        public string Remark { get; set; }
    }
}