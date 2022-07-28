using DCMS.Core;
using DCMS.Core.Domain.Configuration;
using DCMS.Web.Framework;
using DCMS.Web.Framework.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace DCMS.ViewModel.Models.Sales
{

    public partial class ChangeReservationListModel : BaseModel, IParentList
    {
        #region Serch
        [HintDisplayName("单据类型", "单据类型")]
        public int BillType { get; set; } = 0;
        [HintDisplayName("单据类型", "单据类型")]
        public BillTypeEnum BillTypeEnum { get; set; }

        [HintDisplayName("客户Id", "客户Id")]
        public string TerminalId { get; set; }

        [HintDisplayName("客户", "客户")]
        public string TerminalName { get; set; }

        [HintDisplayName("单据编号", "单据编号")]
        public string BillNumber { get; set; }

        [HintDisplayName("业务员", "业务员")]
        public int? BusinessUserId { get; set; } = 0;
        public SelectList BusinessUsers { get; set; }

        [HintDisplayName("送货员", "送货员")]
        public int? DeliveryUserId { get; set; } = 0;
        public SelectList DeliveryUsers { get; set; }

        [HintDisplayName("备注", "备注")]
        public string Remark { get; set; }

        [DisplayName("开始时间")]

        [UIHint("DateTime")] public DateTime StartTime { get; set; }

        [DisplayName("开始时间")]

        [UIHint("DateTime")] public DateTime EndTime { get; set; }

        public SelectList ParentList { get; set; }

        [HintDisplayName("状态(转单)", "状态(转单)")]
        public bool? ChangedStatus { get; set; }

        [HintDisplayName("状态(调度)", "状态(调度)")]
        public bool? DispatchedStatus { get; set; }

        [HintDisplayName("过滤", "过滤")]
        public int[] ChangeReservationFilterSelectedIds { get; set; }
        public IEnumerable<SelectListItem> ChangeReservationFilters { get; set; }
        #endregion


    }


    /// <summary>
    /// 订单转销售单
    /// </summary>
    public class ChangeReservationModel : BaseEntityModel
    {
        public ChangeReservationModel()
        {
            //查询
            SaleReservationBillAccountings = new List<SaleReservationBillAccountingModel>();
            ReturnReservationBillAccountings = new List<ReturnReservationBillAccountingModel>();

            //弹出框
            SaleBillAccountings = new List<AccountingOption>();
            ReturnBillAccountings = new List<AccountingOption>();

        }

        //收款账户
        public IList<SaleReservationBillAccountingModel> SaleReservationBillAccountings { get; set; }
        public IList<ReturnReservationBillAccountingModel> ReturnReservationBillAccountings { get; set; }

        #region 转单弹出框 下拉会计科目数据源

        public IList<AccountingOption> SaleBillAccountings { get; set; }
        public IList<AccountingOption> ReturnBillAccountings { get; set; }

        /// <summary>
        /// 收款方式
        /// </summary>
        public int AccountingId { get; set; } = 0;
        public SelectList SaleBillAccountingSelectLists { get; set; }
        public SelectList ReturnBillAccountingSelectLists { get; set; }

        #endregion

        public int BillType { get; set; } = 0;
        public string BillTypeName { get; set; }
        public BillTypeEnum BillTypeEnum
        {
            get { return (BillTypeEnum)BillType; }
            set { BillType = (int)value; }
        }
        public string BillLink { get; set; }

        public int Operation { get; set; }
        public int BillId { get; set; } = 0;
        public int SaleBillId { get; set; } = 0;
        public int ReturnBillId { get; set; } = 0;

        public string SaleBillNumber { get; set; }
        public string ReturnBillNumber { get; set; }


        public string BillNumber { get; set; }
        public string BillBarCode { get; set; }

        public int TerminalId { get; set; } = 0;
        public string TerminalName { get; set; }
        public string TerminalPointCode { get; set; }


        public int BusinessUserId { get; set; } = 0;
        public string BusinessUserName { get; set; }

        public int? DeliveryUserId { get; set; } = 0;
        public string DeliveryUserName { get; set; }


        public int DepartmentId { get; set; } = 0;
        public string DepartmentName { get; set; }

        public int DistrictId { get; set; } = 0;
        public string DistrictName { get; set; }

        public int? WareHouseId { get; set; } = 0;
        public string WareHouseName { get; set; }


        public int PayTypeId { get; set; } = 0;

        [UIHint("DateTimeNullable")] public DateTime? TransactionDate { get; set; }


        public bool IsMinUnitSale { get; set; }

        public string DefaultAmountId { get; set; }
        public string DefaultAmountName { get; set; }
        public List<SelectListItem> SaleDefaultAmounts { get; set; }

        /// <summary>
        /// 总金额
        /// </summary>
        public decimal SumAmount { get; set; } = 0;

        /// <summary>
        /// 应收金额
        /// </summary>
        public decimal ReceivableAmount { get; set; } = 0;

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
        /// 备注
        /// </summary>
        public string Remark { get; set; }


        /// <summary>
        /// 现金科目
        /// </summary>
        public int CollectionAccount { get; set; } = 0;
        /// <summary>
        /// 现金收款金额
        /// </summary>
        public decimal CollectionAmount { get; set; } = 0;

        /// <summary>
        /// 制单人
        /// </summary>
        public int MakeUserId { get; set; } = 0;
        public string MakeUserName { get; set; }

        public DateTime? CreatedOnUtc { get; set; }

        /// <summary>
        /// 审核人
        /// </summary>
        public int? AuditedUserId { get; set; } = 0;
        public string AuditedUserName { get; set; }
        public bool AuditedStatus { get; set; }
        public DateTime? AuditedDate { get; set; }

        /// <summary>
        /// 红冲人
        /// </summary>
        public int? ReversedUserId { get; set; } = 0;
        public int? ReversedUserName { get; set; } = 0;
        public bool ReversedStatus { get; set; }
        public DateTime? ReversedDate { get; set; }

        /// <summary>
        /// 调度人
        /// </summary>
        public int? DispatchedUserId { get; set; } = 0;
        public int? DispatchedUserName { get; set; } = 0;
        public bool DispatchedStatus { get; set; }
        public DateTime? DispatchedDate { get; set; }

        /// <summary>
        /// 转单人
        /// </summary>
        public int? ChangedUserId { get; set; } = 0;
        public int? ChangedUserName { get; set; } = 0;
        public bool ChangedStatus { get; set; }
        public DateTime? ChangedDate { get; set; }

        /// <summary>
        /// 打印数
        /// </summary>
        public int PrintNum { get; set; } = 0;

        /// <summary>
        /// 是否已经开具收款单（已收账）
        /// </summary>
        public bool Receipted { get; set; }

    }


    /// <summary>
    /// 项目保存或者编辑
    /// </summary>
    //[Validator(typeof(ChangeReservationValidatorUpdate))]
    public class ChangeReservationUpdateModel : BaseEntityModel
    {
        public ChangeReservationUpdateModel()
        {
            //查询
            SaleReservationBillAccountings = new List<SaleReservationBillAccountingModel>();
            ReturnReservationBillAccountings = new List<ReturnReservationBillAccountingModel>();

            //弹出框
            SaleBillAccountings = new List<AccountingOption>();
            ReturnBillAccountings = new List<AccountingOption>();

        }

        //收款账户
        public IList<SaleReservationBillAccountingModel> SaleReservationBillAccountings { get; set; }
        public IList<ReturnReservationBillAccountingModel> ReturnReservationBillAccountings { get; set; }

        #region 转单弹出框 下拉会计科目数据源
        //收款账户（这里需要转成销售单、退货单所以去销售单、退货单科目）
        //销售单科目
        public IList<AccountingOption> SaleBillAccountings { get; set; }
        //退货单科目
        public IList<AccountingOption> ReturnBillAccountings { get; set; }

        [HintDisplayName("收款方式", "收款方式")]
        public int AccountingId { get; set; } = 0;
        public SelectList SaleBillAccountingSelectLists { get; set; }
        public SelectList ReturnBillAccountingSelectLists { get; set; }
        #endregion

        [HintDisplayName("仓库", "仓库")]
        public int WareHouseId { get; set; } = 0;
        [HintDisplayName("仓库", "仓库")]
        public string WareHouseName { get; set; }
        public SelectList WareHouses { get; set; }

        [HintDisplayName("送货员", "送货员")]
        public int DeliveryUserId { get; set; } = 0;
        public SelectList DeliveryUsers { get; set; }

        [HintDisplayName("交易日期", "交易日期")]
        [UIHint("DateTimeNullable")] public DateTime? TransactionDate { get; set; }

        public new int Id { get; set; } = 0;

        public int BillType { get; set; } = 0;
        public string Ids { get; set; }

        /// <summary>
        /// 操作源
        /// </summary>
        public int? Operation { get; set; }

    }


}
