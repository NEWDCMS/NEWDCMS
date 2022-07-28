using DCMS.Core;
using DCMS.Core.Domain.Products;
using DCMS.ViewModel.Models.Products;
using DCMS.Web.Framework;
using DCMS.Web.Framework.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace DCMS.ViewModel.Models.Purchases
{

    public partial class PurchaseReturnBillListModel : BaseModel
    {

        public PurchaseReturnBillListModel()
        {
            PagingFilteringContext = new PagingFilteringModel();
            Lists = new List<PurchaseReturnBillModel>();
            DynamicColumns = new List<string>();
        }

        public PagingFilteringModel PagingFilteringContext { get; set; }
        public IList<PurchaseReturnBillModel> Lists { get; set; }
        public List<string> DynamicColumns { get; set; }


        [HintDisplayName("员工", "员工")]
        public int? BusinessUserId { get; set; } = 0;
        public SelectList BusinessUsers { get; set; }

        [HintDisplayName("供应商", "供应商")]
        public int? ManufacturerId { get; set; } = 0;
        public SelectList Manufacturers { get; set; }


        [HintDisplayName("仓库", "仓库")]
        public int? WareHouseId { get; set; } = 0;
        public SelectList WareHouses { get; set; }

        [HintDisplayName("单据编号", "单据编号")]
        public string BillNumber { get; set; }

        [HintDisplayName("状态(打印)", "状态(打印)")]
        public bool? PrintStatus { get; set; }

        [DisplayName("开始时间")]

        [UIHint("DateTime")] public DateTime StartTime { get; set; }

        [DisplayName("结束时间")]

        [UIHint("DateTime")] public DateTime EndTime { get; set; }

        [HintDisplayName("状态(审核)", "状态(审核)")]
        public bool? AuditedStatus { get; set; }

        [HintDisplayName("备注", "备注")]
        public string Remark { get; set; }

        [HintDisplayName("按审核时间", " 按审核时间")]
        public bool? SortByAuditedTime { get; set; }

        [HintDisplayName("显示红冲的数据", " 显示红冲的数据")]
        public bool? ShowReverse { get; set; }

        /// <summary>
        /// 付款状态
        /// </summary>
        public int PayStatus { get; set; }
        public PayStatus PaymentStatus
        {
            get { return (PayStatus)PayStatus; }
            set { PayStatus = (int)value; }
        }


    }


    /// <summary>
    /// 采购退货单
    /// </summary>
   // [Validator(typeof(PurchaseReturnValidator))]
    public class PurchaseReturnBillModel : BaseBillModel
    {

        public PurchaseReturnBillModel()
        {
            Items = new List<PurchaseReturnItemModel>();
            PurchaseReturnBillAccountings = new List<PurchaseReturnBillAccountingModel>();
        }

        public string BillBarCode { get; set; }

        [HintDisplayName("供应商", "供应商")]
        public int ManufacturerId { get; set; } = 0;
        [HintDisplayName("供应商", "供应商")]
        public string ManufacturerName { get; set; }
        public SelectList Manufacturers { get; set; }

        [HintDisplayName("业务员", "业务员")]
        public int? BusinessUserId { get; set; } = 0;
        [HintDisplayName("业务员", "业务员")]
        public string BusinessUserName { get; set; }
        public SelectList BusinessUsers { get; set; }

        [HintDisplayName("仓库", "仓库")]
        public int WareHouseId { get; set; } = 0;
        [HintDisplayName("仓库", "仓库")]
        public string WareHouseName { get; set; }
        public SelectList WareHouses { get; set; }

        [HintDisplayName("交易日期", "交易日期")]
        [UIHint("DateTimeNullable")] public DateTime? TransactionDate { get; set; }

        [HintDisplayName("按最小单位采购退货", "按最小单位采购退货")]
        public bool IsMinUnitPurchase { get; set; }

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


        [HintDisplayName("现金", "默认收款账户")]
        public int CollectionAccount { get; set; } = 0;
        [HintDisplayName("收款金额", "默认收款金额")]
        public decimal CollectionAmount { get; set; } = 0;

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


        [HintDisplayName("打印数", "打印数")]
        public int PrintNum { get; set; } = 0;


        [HintDisplayName("已付款", "是否已经开具付款单（已付款）")]
        public bool Paymented { get; set; }
        [HintDisplayName("数据来源", "数据来源")]
        public int Operation { get; set; } = 0;
        [HintDisplayName("付款状态", "付款状态")]
        public int PayStatus { get; set; } = 0;
        /// <summary>
        /// 项目
        /// </summary>
        public IList<PurchaseReturnItemModel> Items { get; set; }

        //收款账户
        public IList<PurchaseReturnBillAccountingModel> PurchaseReturnBillAccountings { get; set; }

        /// <summary>
        /// 默认进价
        /// </summary>
        public int DefaultPurchasePrice { get; set; } = 0;

        /// <summary>
        /// 是否显示生产日期
        /// </summary>
        public bool IsShowCreateDate { get; set; }

        /// <summary>
        /// 单据合计精度
        /// </summary>
        public int AccuracyRounding { get; set; } = 0;

        /// <summary>
        /// 供应商余额
        /// </summary>
        public ManufacturerBalance TBalance { get; set; } = new ManufacturerBalance();
    }


    /// <summary>
    /// 采购退货单明细
    /// </summary>
    public class PurchaseReturnItemModel : ProductBaseModel
    {



        /// <summary>
        /// 采购退货单Id
        /// </summary>
        public int PurchaseReturnBillId { get; set; } = 0;

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
        /// 备注
        /// </summary>
        [HintDisplayName("备注", "备注")]
        public string Remark { get; set; }

        /// <summary>
        /// 剩余还款数量
        /// </summary>
        public int RemainderQty { get; set; } = 0;

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

        public List<string> ProductTimes { get; set; }
        #endregion

        /// <summary>
        /// 税率%
        /// </summary>
        [HintDisplayName("税率%", "税率%")]
        public decimal TaxRate { get; set; } = 0;

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

    }


    /// <summary>
    ///  收款账户（采购退货单科目映射表）
    /// </summary>
    public class PurchaseReturnBillAccountingModel : BaseAccountModel
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
    public class PurchaseReturnBillUpdateModel : BaseBalance
    {
        public string BillNumber { get; set; }
        /// <summary>
        /// 供应商
        /// </summary>
        public int ManufacturerId { get; set; } = 0;

        /// <summary>
        /// 业务员
        /// </summary>
        public int BusinessUserId { get; set; } = 0;

        /// <summary>
        /// 仓库
        /// </summary>
        public int WareHouseId { get; set; } = 0;

        /// <summary>
        /// 交易日期
        /// </summary>
        public DateTime TransactionDate { get; set; }

        /// <summary>
        /// 按最小单位采购
        /// </summary>
        public bool IsMinUnitPurchase { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        public string Remark { get; set; }

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
        public List<PurchaseReturnItemModel> Items { get; set; }

        /// <summary>
        /// 收款账户
        /// </summary>
        public List<PurchaseReturnBillAccountingModel> Accounting { get; set; }

    }


}
