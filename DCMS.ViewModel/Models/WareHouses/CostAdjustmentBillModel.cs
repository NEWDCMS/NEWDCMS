using DCMS.ViewModel.Models.Products;
using DCMS.Web.Framework;
using DCMS.Web.Framework.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DCMS.ViewModel.Models.WareHouses
{


    /// <summary>
    /// 用于表示成本调价单列表
    /// </summary>
    public partial class CostAdjustmentBillListModel : BaseModel
    {
        public CostAdjustmentBillListModel()
        {
            PagingFilteringContext = new PagingFilteringModel();

        }

        public PagingFilteringModel PagingFilteringContext { get; set; }
        public IList<CostAdjustmentBillModel> Items { get; set; }

        [HintDisplayName("操作员", "操作员")]
        public int? OperatorId { get; set; } = 0;
        public string OperatorName { get; set; }
        public SelectList Operators { get; set; }

        [HintDisplayName("单据编号", "单据编号")]
        public string BillNumber { get; set; }

        [HintDisplayName("状态(审核)", "状态(审核)")]
        public bool? AuditedStatus { get; set; }

        [HintDisplayName("打印状态", "打印状态")]
        public bool? PrintedStatus { get; set; }

        [HintDisplayName("开始日期", "开始日期")]
        [UIHint("DateTimeNullable")]
        public DateTime? StartTime { get; set; }

        [HintDisplayName("截止日期", "截止日期")]
        [UIHint("DateTimeNullable")]
        public DateTime? EndTime { get; set; }

        [HintDisplayName(" 显示红冲的数据", " 显示红冲的数据")]
        public bool? ShowReverse { get; set; }

        [HintDisplayName("按审核时间", " 按审核时间")]
        public bool? SortByAuditedTime { get; set; }

        [HintDisplayName("备注", "备注")]
        public string Remark { get; set; }

    }


    /// <summary>
    /// 用于表示成本调价单
    /// </summary>
    public class CostAdjustmentBillModel : BaseEntityModel
    {
        public CostAdjustmentBillModel()
        {
            Items = new List<CostAdjustmentItemModel>();
        }
        public IList<CostAdjustmentItemModel> Items { get; set; }

        [HintDisplayName("单据编号", "单据编号")]
        public string BillNumber { get; set; }
        public string BillBarCode { get; set; }

        [HintDisplayName("调价日期", "调价日期")]
        [UIHint("DateTime")]
        public DateTime AdjustmentDate { get; set; }
        [HintDisplayName("调价日期显示", "调价日期显示")]
        public string AdjustmentDateView { get; set; }

        [HintDisplayName("是否按最小单位调价", "是否按最小单位调价")]
        public bool AdjustmentByMinUnit { get; set; }

        [HintDisplayName("备注", "备注")]
        public string Remark { get; set; }

        [HintDisplayName("制单人", "制单人")]
        public int MakeUserId { get; set; } = 0;
        public string MakeUserName { get; set; }

        [HintDisplayName("审核人", "审核人")]
        public int? AuditedUserId { get; set; } = 0;
        public string AuditedUserName { get; set; }

        [HintDisplayName("状态(审核)", " 状态(审核)")]
        public bool AuditedStatus { get; set; }
        [HintDisplayName("状态(审核)显示", " 状态(审核)显示")]
        public string AuditedStatusName { get; set; }

        [HintDisplayName("审核时间", "审核时间")]
        public DateTime? AuditedDate { get; set; }
        [HintDisplayName("审核时间显示", "审核时间显示")]
        public string AuditedDateName { get; set; }

        [HintDisplayName("红冲人", "红冲人")]
        public int? ReversedUserId { get; set; } = 0;

        [HintDisplayName("红冲状态", "红冲状态")]
        public bool ReversedStatus { get; set; }

        [HintDisplayName("红冲时间", "红冲时间")]
        public DateTime? ReversedDate { get; set; }

        [HintDisplayName("打印数", "打印数")]
        public int? PrintNum { get; set; } = 0;

        public DateTime CreatedOnUtc { get; set; }
        [HintDisplayName("数据来源", "数据来源")]
        public int Operation { get; set; } = 0;

    }

    /// <summary>
    /// 成本调价单项目
    /// </summary>

    public class CostAdjustmentItemModel : ProductBaseModel
    {

        [HintDisplayName("成本调价单", "成本调价单")]
        public int CostAdjustmentBillId { get; set; } = 0;

        [HintDisplayName("数量", "数量")]
        public int Quantity { get; set; } = 0;

        [HintDisplayName("调整前价格", "调整前价格")]
        public decimal? AdjustmentPriceBefore { get; set; } = 0;

        [HintDisplayName("调整后价格", "调整后价格")]
        public decimal? AdjustedPrice { get; set; } = 0;

        [HintDisplayName("创建时间", "创建时间")]
        public DateTime CreatedOnUtc { get; set; }

    }

    /// <summary>
    /// 项目保存或者编辑
    /// </summary>
    public class CostAdjustmentUpdateModel : BaseEntityModel
    {
        public CostAdjustmentUpdateModel()
        {
            Items = new List<CostAdjustmentItemModel>();
        }

        [HintDisplayName("调价日期", "调价日期")]
        public DateTime AdjustmentDate { get; set; }

        [HintDisplayName("是否按最小单位调价", "是否按最小单位调价")]
        public bool AdjustmentByMinUnit { get; set; }

        [HintDisplayName("备注", "备注")]
        public string Remark { get; set; }

        /// <summary>
        /// 操作源
        /// </summary>
        public int? Operation { get; set; }

        /// <summary>
        /// 项目
        /// </summary>
        public List<CostAdjustmentItemModel> Items { get; set; }

    }
}
