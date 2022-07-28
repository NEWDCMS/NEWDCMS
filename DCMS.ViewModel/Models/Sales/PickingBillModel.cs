using DCMS.Core;
using DCMS.Web.Framework;
using DCMS.Web.Framework.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
namespace DCMS.ViewModel.Models.Sales
{

    public partial class PickingBillListModel : BaseModel
    {


        [HintDisplayName("业务员", "业务员")]
        public int? BusinessUserId { get; set; } = 0;
        public SelectList BusinessUsers { get; set; }

        [HintDisplayName("送货员", "送货员")]
        public int? DeliveryUserId { get; set; } = 0;
        public SelectList DeliveryUsers { get; set; }

        [HintDisplayName("备注", "备注")]
        public string Remark { get; set; }

        [DisplayName("开始时间")]

        public DateTime? StartTime { get; set; }

        [DisplayName("结束时间")]

        public DateTime? EndTime { get; set; }


        [HintDisplayName("整箱拆零状态", "整箱拆零状态")]
        public int? WholeScrapStatus { get; set; } = 0;
        public PickingWholeScrapStatus PickingWholeScrapStatus { get; set; }

        [HintDisplayName("拆零状态", "拆零状态")]
        public int? ScrapStatus { get; set; } = 0;
        public PickingScrapStatus PickingScrapStatus { get; set; }


        [HintDisplayName("过滤", "过滤")]
        public int[] PickingFilterSelectedIds { get; set; }
        public IEnumerable<SelectListItem> PickingFilters { get; set; }

    }


    /// <summary>
    /// 仓库分拣单
    /// </summary>
    public class PickingBillModel : BaseEntityModel
    {



        public int BillId { get; set; } = 0;

        [HintDisplayName("单据编号", "单据编号")]
        public string BillNumber { get; set; }
        public string BillBarCode { get; set; }

        [HintDisplayName("单据类型Id", "单据类型Id")]
        public int BillType { get; set; } = 0;

        [HintDisplayName("单据类型名称", "单据类型名称")]
        public string BillTypeName { get; set; }

        [HintDisplayName("交易日期", "交易日期")]
        [UIHint("DateTimeNullable")] public DateTime? TransactionDate { get; set; }

        [HintDisplayName("业务员", "业务员")]
        public int BusinessUserId { get; set; } = 0;
        [HintDisplayName("业务员", "业务员")]
        public string BusinessUserName { get; set; }
        public SelectList BusinessUsers { get; set; }

        [HintDisplayName("客户Id", "客户Id")]
        public int TerminalId { get; set; } = 0;
        [HintDisplayName("客户", "客户")]
        public string TerminalName { get; set; }
        [HintDisplayName("客户编码", "客户编码")]
        public string TerminalPointCode { get; set; }

        [HintDisplayName("订单金额", "订单金额")]
        public decimal OrderAmount { get; set; } = 0;

        [HintDisplayName("备注", "备注")]
        public string Remark { get; set; }

        [HintDisplayName("送货员", "送货员")]
        public int DeliveryUserId { get; set; } = 0;
        [HintDisplayName("送货员", "送货员")]
        public string DeliveryUserName { get; set; }
        public SelectList DeliveryUsers { get; set; }

        [HintDisplayName("打印次数", "打印次数")]
        public int PrintNum { get; set; } = 0;
        [HintDisplayName("打印时间", "打印时间")]
        public DateTime? PrintData { get; set; }


    }

    /// <summary>
    /// 仓库分拣打印
    /// </summary>
    public class PickingPrintData
    {
        [HintDisplayName("经销商", "经销商")]
        public int StoreId { get; set; } = 0;

        public int BillId { get; set; } = 0;

        [HintDisplayName("单据编号", "单据编号")]
        public string BillNumber { get; set; }
        public string BillBarCode { get; set; }

        public int CarId { get; set; } = 0;
        public string CarNo { get; set; }

        [HintDisplayName("业务员", "业务员")]
        public int BusinessUserId { get; set; } = 0;
        [HintDisplayName("业务员", "业务员")]
        public string BusinessUserName { get; set; }
        public SelectList BusinessUsers { get; set; }

        private ICollection<PickingPrintProductItem> _pickingPrintProductItems;
        public virtual ICollection<PickingPrintProductItem> PickingPrintProductItems
        {
            get { return _pickingPrintProductItems ?? (_pickingPrintProductItems = new List<PickingPrintProductItem>()); }
            protected set { _pickingPrintProductItems = value; }
        }

        [HintDisplayName("合计", "合计")]
        public string Total { get; set; }
    }

    public class PickingPrintProductItem
    {
        public int ProductId { get; set; } = 0;
        public string ProductName { get; set; }

        public int Quantity { get; set; } = 0;

        public string QuantityChange { get; set; }

    }





}
