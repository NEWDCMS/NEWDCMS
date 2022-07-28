using DCMS.Web.Framework.Models;
using System;


namespace DCMS.ViewModel.Models.Sales
{
    /// <summary>
    /// 所有单据主表
    /// </summary>
    public class MainModel : BaseEntityModel
    {

        /// <summary>
        /// 经销商Id
        /// </summary>
        //移除 = 0;

        /// <summary>
        /// 单据编号
        /// </summary>
        public string BillNumber { get; set; }

        /// <summary>
        /// 单据类型
        /// </summary>
        public string BillType { get; set; }

        /// <summary>
        /// 操作人员
        /// </summary>
        public string OperatorUserId { get; set; }

        /// <summary>
        /// 终端客户Id
        /// </summary>
        public string TerminalId { get; set; }

        /// <summary>
        /// 付款方式
        /// </summary>
        public string PayType { get; set; }

        /// <summary>
        /// 优惠
        /// </summary>
        public decimal? PreferentialAmount { get; set; } = 0;

        /// <summary>
        /// 实收金额
        /// </summary>
        public decimal? RealReceiveAmount { get; set; } = 0;

        /// <summary>
        /// 是否按最小单位销售类型
        /// </summary>
        public int? IsMinUnitSale { get; set; } = 0;

        /// <summary>
        /// 状态
        /// </summary>
        public string Status { get; set; }

        /// <summary>
        /// 单据打印数据
        /// </summary>
        public string BillPrintData { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        public string Remark { get; set; }

        /// <summary>
        /// 创建人员Id
        /// </summary>
        public int? AddUserId { get; set; } = 0;

        /// <summary>
        /// 创建人员名称
        /// </summary>
        public int? AddUserName { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime? AddDate { get; set; }

        /// <summary>
        /// 修改人员Id
        /// </summary>
        public int? EditUserId { get; set; } = 0;

        /// <summary>
        /// 修改人员名称
        /// </summary>
        public int? EditUserName { get; set; } = 0;

        /// <summary>
        /// 修改时间
        /// </summary>
        public DateTime? EditDate { get; set; }

    }
}
