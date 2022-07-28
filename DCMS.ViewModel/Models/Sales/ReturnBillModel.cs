using DCMS.Core;
using DCMS.Core.Domain.Terminals;
using DCMS.ViewModel.Models.Products;
using DCMS.Web.Framework;
using DCMS.Web.Framework.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace DCMS.ViewModel.Models.Sales
{

    public partial class ReturnBillListModel : BaseModel, IParentList
    {

        public ReturnBillListModel()
        {
            PagingFilteringContext = new PagingFilteringModel();
            Lists = new List<ReturnBillModel>();
            DynamicColumns = new List<string>();
        }

        public PagingFilteringModel PagingFilteringContext { get; set; }
        public IList<ReturnBillModel> Lists { get; set; }
        public List<string> DynamicColumns { get; set; }

        [HintDisplayName("客户Id", "客户Id")]
        public int? TerminalId { get; set; } = 0;
        [HintDisplayName("客户", "客户")]
        public string TerminalName { get; set; }

        [HintDisplayName("单据编号", "单据编号")]
        public string BillNumber { get; set; }

        [HintDisplayName("业务员", "业务员")]
        public int? BusinessUserId { get; set; } = 0;
        public SelectList BusinessUsers { get; set; }

        [HintDisplayName("部门", "部门名称")]
        public int? DepartmentId { get; set; } = 0;
        public SelectList ParentList { get; set; }

        [HintDisplayName("仓库", "仓库")]
        public int? WareHouseId { get; set; } = 0;
        public SelectList WareHouses { get; set; }

        [HintDisplayName("送货员", "送货员")]
        public int? DeliveryUserId { get; set; } = 0;
        public SelectList DeliveryUsers { get; set; }

        [HintDisplayName("片区", "片区")]
        public int? DistrictId { get; set; } = 0;
        public SelectList Districts { get; set; }

        [HintDisplayName("备注", "备注")]
        public string Remark { get; set; }

        [DisplayName("开始时间")]

        [UIHint("DateTime")] public DateTime StartTime { get; set; }

        [DisplayName("结束时间")]

        [UIHint("DateTime")] public DateTime EndTime { get; set; }

        /// <summary>
        /// 默认售价下拉
        /// </summary>
        public IEnumerable<SelectListItem> DefaultAmounts { get; set; }


        [HintDisplayName("状态", "状态")]
        public int? Status { get; set; } = 0;
        //public ReturnStatus ReturnStatus { get; set; }


        [HintDisplayName("过滤", "过滤")]
        public int[] ReturnFilterSelectedIds { get; set; }
        public IEnumerable<SelectListItem> ReturnFilters { get; set; }

        [HintDisplayName("支付方式", "支付方式")]
        public int PaymentMethodType { get; set; } = 0;
        public ReturnBillPaymentMethodType ReturnBillPaymentMethodType { get; set; }

        [HintDisplayName("单据来源", "单据来源")]
        public int? BillSourceType { get; set; } = 0;
        public ReturnBillSourceType ReturnBillSourceType { get; set; }

        [HintDisplayName("状态(审核)", "状态(审核)")]
        public bool? AuditedStatus { get; set; }

        [HintDisplayName("显示红冲的数据", " 显示红冲的数据")]
        public bool? ShowReverse { get; set; }

        [HintDisplayName("显示退货单", " 显示退货单")]
        public bool? ShowReturn { get; set; }

        [HintDisplayName("按审核时间", " 按审核时间")]
        public bool? SortByAuditedTime { get; set; }

        [HintDisplayName("商品", "商品")]
        public int ProductId { get; set; } = 0;
        public string ProductName { get; set; }
    }


    /// <summary>
    /// 退货单
    /// </summary>
    //[Validator(typeof(ReturnValidator))]
    public class ReturnBillModel : BaseBillModel, IParentList
    {

        public ReturnBillModel()
        {
            Items = new List<ReturnItemModel>();
            ReturnBillAccountings = new List<ReturnBillAccountingModel>();
        }

        public string BillBarCode { get; set; }

        [HintDisplayName("退货订单Id", "退货订单Id")]
        public int ReturnReservationBillId { get; set; } = 0;
        [HintDisplayName("退货订单编号", "退货订单编号")]
        public string ReturnReservationBillNumber { get; set; }

        [HintDisplayName("客户Id", "客户Id")]
        public int TerminalId { get; set; } = 0;
        [HintDisplayName("客户", "客户")]
        public string TerminalName { get; set; }
        [HintDisplayName("终端编号", "终端编号")]
        public string TerminalPointCode { get; set; }

        [HintDisplayName("业务员", "业务员")]
        public int? BusinessUserId { get; set; } = 0;
        [HintDisplayName("业务员", "业务员")]
        public string BusinessUserName { get; set; }
        public SelectList BusinessUsers { get; set; }

        [HintDisplayName("送货员", "送货员")]
        public int? DeliveryUserId { get; set; } = 0;
        [HintDisplayName("送货员", "送货员")]
        public string DeliveryUserName { get; set; }
        public SelectList DeliveryUsers { get; set; }

        [HintDisplayName("部门名称", "部门名称")]
        public int DepartmentId { get; set; } = 0;
        public string DepartmentName { get; set; }
        public SelectList ParentList { get; set; }

        [HintDisplayName("片区名称", "片区名称")]
        public int DistrictId { get; set; } = 0;
        public string DistrictName { get; set; }
        public SelectList Districts { get; set; }

        [HintDisplayName("仓库", "仓库")]
        public int WareHouseId { get; set; } = 0;
        [HintDisplayName("仓库", "仓库")]
        public string WareHouseName { get; set; }
        public SelectList WareHouses { get; set; }

        [HintDisplayName("付款方式", "付款方式")]
        public int PayTypeId { get; set; } = 0;

        [HintDisplayName("付款方式", "付款方式")]
        public string PayTypeName { get; set; }

        [HintDisplayName("交易日期", "交易日期")]
        [UIHint("DateTimeNullable")] public DateTime? TransactionDate { get; set; }

        [HintDisplayName("最小单位", "按最小单位退货")]
        public bool IsMinUnitSale { get; set; }

        [HintDisplayName("默认售价", "默认售价类型")]
        public string DefaultAmountId { get; set; }
        [HintDisplayName("默认售价", "默认售价名称")]
        public string DefaultAmountName { get; set; }
        public SelectList ReturnDefaultAmounts { get; set; }

        [HintDisplayName("总金额", "总金额")]
        public decimal SumAmount { get; set; } = 0;

        [HintDisplayName("应收金额", "应收金额")]
        public decimal ReceivableAmount { get; set; } = 0;

        [HintDisplayName("优惠金额", "优惠金额")]
        public decimal PreferentialAmount { get; set; } = 0;

        [HintDisplayName("优惠后金额", "优惠后金额")]
        public decimal PreferentialEndAmount { get; set; } = 0;

        [HintDisplayName("欠款金额", "欠款金额")]
        public decimal OweCash { get; set; } = 0;
        [HintDisplayName("数据来源", "数据来源")]
        public int Operation { get; set; } = 0;


        [HintDisplayName("利润", "利润")]
        public decimal SumProfit { get; set; } = 0;

        [HintDisplayName("成本利润率", "成本利润率")]
        public decimal SumCostProfitRate { get; set; } = 0;

        [HintDisplayName("状态", "状态")]
        public string Status { get; set; }


        [HintDisplayName("现金", "默认收款账户")]
        public int CollectionAccount { get; set; } = 0;
        [HintDisplayName("收款金额", "默认收款金额")]
        public decimal? CollectionAmount { get; set; } = 0;


        [HintDisplayName("制单人", "制单人")]
        public string MakeUserName { get; set; }


        [HintDisplayName("审核人", "审核人")]
        public int? AuditedUserId { get; set; } = 0;
        [HintDisplayName("审核人", "审核人")]
        public string AuditedUserName { get; set; }

        [HintDisplayName("状态(审核)", "状态(审核)")]
        public bool AuditedStatus { get; set; }

        [HintDisplayName("审核时间", "审核时间")]
        public DateTime? AuditedDate { get; set; }

        [HintDisplayName("红冲人", "红冲人")]
        public int? ReversedUserId { get; set; } = 0;

        [HintDisplayName("红冲状态", "红冲状态")]
        public bool ReversedStatus { get; set; }

        [HintDisplayName("红冲时间", "红冲时间")]
        public DateTime? ReversedDate { get; set; }

        [HintDisplayName("上交状态", "上交状态")]
        public bool? HandInStatus { get; set; }

        [HintDisplayName("上交时间", "上交时间")]
        public DateTime? HandInDate { get; set; }

        [HintDisplayName("打印数", "打印数")]
        public int PrintNum { get; set; } = 0;

        /// <summary>
        /// 收款状态
        /// </summary>
        public int ReceiptStatus { get; set; }
        public ReceiptStatus ReceivedStatus
        {
            get { return (ReceiptStatus)ReceiptStatus; }
            set { ReceiptStatus = (int)value; }
        }


        public List<ReturnItemModel> Items { get; set; }

        /// <summary>
        /// 关联订单
        /// </summary>
        public int OrderId { get; set; }
        public string OrderNumber { get; set; }

        //收款账户
        public IList<ReturnBillAccountingModel> ReturnBillAccountings { get; set; }

        /// <summary>
        /// 是否显示生产日期
        /// </summary>
        public bool IsShowCreateDate { get; set; }

        /// <summary>
        /// 商品变价参考标准
        /// </summary>
        public int VariablePriceCommodity { get; set; } = 0;

        /// <summary>
        /// 单据合计精度
        /// </summary>
        public int AccuracyRounding { get; set; } = 0;

        /// <summary>
        /// 交易日期可选范围
        /// </summary>
        public int AllowSelectionDateRange { get; set; } = 0;

        /// <summary>
        /// 允许预收款支付成负数
        /// </summary>
        public bool AllowAdvancePaymentsNegative { get; set; }


        #region 终端、员工欠款


        /// <summary>
        /// 终端余额
        /// </summary>
        public TerminalBalance TBalance { get; set; } = new TerminalBalance();

        /// <summary>
        /// 员工最大欠款额度
        /// </summary>
        public decimal? UserMaxAmount { get; set; } = 0;

        /// <summary>
        /// 员工使用欠款
        /// </summary>
        public decimal? UserUsedAmount { get; set; } = 0;

        /// <summary>
        /// 员工可用欠款
        /// </summary>
        public decimal? UserAvailableAmount { get; set; } = 0;

        #endregion

    }

    /// <summary>
    /// 退货单明细
    /// </summary>
    public class ReturnItemModel : ProductBaseModel
    {


        /// <summary>
        /// 退货订单Id
        /// </summary>
        public int ReturnBillId { get; set; } = 0;

        /// <summary>
        /// 单据编号
        /// </summary>
        public string BillNumber { get; set; }


        /// <summary>
        /// 数量
        /// </summary>
        [HintDisplayName("数量", "数量")]
        public int Quantity { get; set; } = 0;

        /// <summary>
        /// 价格
        /// </summary>
        [HintDisplayName("价格", "价格")]
        public decimal Price { get; set; } = 0;

        /// <summary>
        /// 金额
        /// </summary>
        [HintDisplayName("金额", "金额")]
        public decimal Amount { get; set; } = 0;

        /// <summary>
        /// 订货单编号
        /// </summary>
        public string OrderCode { get; set; }

        /// <summary>
        /// 剩余还款数量
        /// </summary>
        public int RemainderQty { get; set; } = 0;


        //(注意：结转成本后，已审核业务单据中的成本价，将会被替换成结转后的全月平均价!)
        /// <summary>
        /// 成本价
        /// </summary>
        [HintDisplayName("成本价", "成本价")]
        public decimal CostPrice { get; set; } = 0;
        /// <summary>
        /// 成本金额
        /// </summary>
        [HintDisplayName("成本金额", "成本金额")]
        public decimal CostAmount { get; set; } = 0;

        /// <summary>
        /// 利润
        /// </summary>
        [HintDisplayName("利润", "利润")]
        public decimal Profit { get; set; } = 0;

        /// <summary>
        /// 成本利润率
        /// </summary>
        [HintDisplayName("成本利润率", "成本利润率")]
        public decimal CostProfitRate { get; set; } = 0;

        /// <summary>
        /// 备注
        /// </summary>
        [HintDisplayName("备注", "备注")]
        public string Remark { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreatedOnUtc { get; set; }

        #region 商品信息
        [HintDisplayName("小单位", "规格属性小单位")]
        public int SmallUnitId { get; set; } = 0;

        [HintDisplayName("中单位", "规格属性中单位")]
        public int? StrokeUnitId { get; set; } = 0;

        [HintDisplayName("大单位", "规格属性大单位")]
        public int? BigUnitId { get; set; } = 0;

        public bool IsManufactureDete { get; set; }

        [HintDisplayName("生产日期", "生产日期")]
        public DateTime? ManufactureDete { get; set; }

        public IList<string> ProductTimes { get; set; } = new List<string>();
        #endregion

        /// <summary>
        /// 是否赠品 2019-07-24
        /// </summary>
        public bool IsGifts { get; set; }

        /// <summary>
        /// 大单位赠送量 2019-07-24
        /// </summary>
        public int? BigGiftQuantity { get; set; } = 0;

        /// <summary>
        /// 小单位赠送量 2019-07-24
        /// </summary>
        public int? SmallGiftQuantity { get; set; } = 0;

        /// <summary>
        /// 税率%
        /// </summary>
        public decimal TaxRate { get; set; }

        /// <summary>
        /// 税额
        /// </summary>
        public decimal TaxPrice { get; set; } = 0;

        /// <summary>
        /// 含税价格
        /// </summary>
        public decimal ContainTaxPrice { get; set; } = 0;

        /// <summary>
        /// 税价总计
        /// </summary>
        public decimal TaxPriceAmount { get; set; } = 0;

        ///// <summary>
        ///// 税率金额
        ///// </summary>
        //public decimal TaxRateAmount { get; set; }


        public int? SortIndex { get; set; }
    }


    /// <summary>
    ///  收款账户（退货单科目映射表）
    /// </summary>
    public class ReturnBillAccountingModel : BaseAccountModel
    {

        public string Name { get; set; }


        /// <summary>
        /// 是否默认
        /// </summary>
        public bool IsDefault { get; set; }

        public int AccountCodeTypeId { get; set; }

    }


    /// <summary>
    /// 项目保存或者编辑
    /// </summary>
    public class ReturnBillUpdateModel : BaseEntityModel
    {
        public string BillNumber { get; set; }
        /// <summary>
        /// 客户
        /// </summary>
        public int TerminalId { get; set; } = 0;

        /// <summary>
        /// 业务员
        /// </summary>
        public int BusinessUserId { get; set; } = 0;

        /// <summary>
        /// 仓库
        /// </summary>
        public int WareHouseId { get; set; } = 0;

        /// <summary>
        /// 送货员
        /// </summary>
        public int DeliveryUserId { get; set; } = 0;

        /// <summary>
        /// 交易日期
        /// </summary>
        public DateTime TransactionDate { get; set; }

        /// <summary>
        /// 按最小单位销售
        /// </summary>
        public bool IsMinUnitSale { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        public string Remark { get; set; }

        /// <summary>
        /// 价格体系
        /// </summary>
        public string DefaultAmountId { get; set; }

        /// <summary>
        /// 优惠金额
        /// </summary>
        public decimal PreferentialAmount { get; set; } = 0;
        /// <summary>
        /// 优惠后金额
        /// </summary>
        public decimal PreferentialEndAmount { get; set; } = 0;

        /// <summary>
        /// 欠款金额
        /// </summary>
        public decimal OweCash { get; set; } = 0;

        /// <summary>
        /// 操作源
        /// </summary>
        public int? Operation { get; set; }

        /// <summary>
        /// 项目
        /// </summary>
        public List<ReturnItemModel> Items { get; set; }

        /// <summary>
        /// 收款账户
        /// </summary>
        public List<ReturnBillAccountingModel> Accounting { get; set; }

        /// <summary>
        /// 订单ID
        /// </summary>
        public int OrderId { get; set; }
        public int DispatchItemId { get; set; }
        /// <summary>
        /// 终端纬度坐标
        /// </summary>
        public double? Latitude { get; set; } = 0;
        /// <summary>
        /// 终端经度坐标
        /// </summary>
        public double? Longitude { get; set; } = 0;


    }


}
