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
    /// 用于表示盘点任务(部分)列表
    /// </summary>
    public partial class InventoryPartTaskBillListModel : BaseModel
    {
        public InventoryPartTaskBillListModel()
        {
            PagingFilteringContext = new PagingFilteringModel();

        }

        [HintDisplayName("盘点状态", "盘点状态(1进行中，2已结束,3已取消)")]
        public int? InventoryStatus { get; set; }
        public SelectList InventoryStatuss { get; set; }

        public PagingFilteringModel PagingFilteringContext { get; set; }
        public IList<InventoryPartTaskBillModel> Items { get; set; }

        [HintDisplayName("盘点人", "盘点人")]
        public int? InventoryPerson { get; set; } = 0;
        public string InventoryPersonName { get; set; }
        public SelectList InventoryPersons { get; set; }

        [HintDisplayName("仓库", "仓库")]
        public int? WareHouseId { get; set; } = 0;
        public string WareHouseName { get; set; }
        public SelectList WareHouses { get; set; }

        [HintDisplayName("单据编号", "单据编号")]
        public string BillNumber { get; set; }

        [HintDisplayName("状态(审核)", "状态(审核)")]
        public bool? AuditedStatus { get; set; }

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

        [HintDisplayName("按完成时间", " 按完成时间")]
        public bool? SortByCompletedTime { get; set; }

        [HintDisplayName("备注", "备注")]
        public string Remark { get; set; }

    }

    /// <summary>
    /// 用于表示盘点任务(部分)
    /// </summary>
    public class InventoryPartTaskBillModel : BaseEntityModel
    {
        public InventoryPartTaskBillModel()
        {
            Items = new List<InventoryPartTaskItemModel>();
        }
        public IList<InventoryPartTaskItemModel> Items { get; set; }

        public int ProductId { get; set; } = 0;
        public string ProductName { get; set; }

        [HintDisplayName("单据编号", "单据编号")]
        public string BillNumber { get; set; }
        public string BillBarCode { get; set; }

        [HintDisplayName("盘点人", "盘点人")]
        public int InventoryPerson { get; set; } = 0;
        public string InventoryPersonName { get; set; }
        public SelectList InventoryPersons { get; set; }

        [HintDisplayName("仓库", "仓库")]
        public int WareHouseId { get; set; } = 0;
        public string WareHouseName { get; set; }
        public SelectList WareHouses { get; set; }

        [HintDisplayName("盘点时间", "盘点时间")]
        [UIHint("DateTime")]
        public DateTime InventoryDate { get; set; }

        [HintDisplayName("盘点时间显示", "盘点时间显示")]
        public string InventoryDateView { get; set; }

        [HintDisplayName("关联盈亏单", "关联盈亏单")]
        public int? InventoryProfitLossBillId { get; set; } = 0;

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

        [HintDisplayName("盘点状态", "盘点状态(1进行中，2已结束,3已取消)")]
        public int InventoryStatus { get; set; } = 0;
        public IEnumerable<SelectListItem> InventoryStatuss { get; set; }
        public string InventoryStatusName { get; set; }

        /// <summary>
        /// 完成人
        /// </summary>
        [HintDisplayName("完成人", "完成人")]
        public int? CompletedUserId { get; set; } = 0;

        /// <summary>
        /// 完成时间
        /// </summary>
        [HintDisplayName("完成时间", "完成时间")]
        public DateTime? CompletedDate { get; set; }

        [HintDisplayName("打印数", "打印数")]
        public int? PrintNum { get; set; } = 0;

        public DateTime CreatedOnUtc { get; set; }
        [HintDisplayName("数据来源", "数据来源")]
        public int Operation { get; set; } = 0;

    }

    /// <summary>
    /// 盘点任务项目
    /// </summary>

    public class InventoryPartTaskItemModel : ProductBaseModel
    {

        [HintDisplayName("盘点任务单", "盘点任务单")]
        public int InventoryPartTaskBillId { get; set; } = 0;

        [HintDisplayName("数量", "数量")]
        public int Quantity { get; set; } = 0;

        [HintDisplayName("当前库存数量", "当前库存数量")]
        public int? CurrentStock { get; set; } = 0;

        [HintDisplayName("大单位数量", "大单位数量")]
        public int? BigUnitQuantity { get; set; } = 0;

        [HintDisplayName("中单位数量", "中单位数量")]
        public int? AmongUnitQuantity { get; set; } = 0;

        [HintDisplayName("小单位数量", "小单位数量")]
        public int? SmallUnitQuantity { get; set; } = 0;

        [HintDisplayName("盘盈数量", "盘盈数量")]
        public int? VolumeQuantity { get; set; } = 0;

        [HintDisplayName("盘亏数量", "盘亏数量")]
        public int? LossesQuantity { get; set; } = 0;

        [HintDisplayName("盘盈批发金额", "盘盈批发金额")]
        public decimal? VolumeWholesaleAmount { get; set; } = 0;

        [HintDisplayName("盘亏批发金额", "盘亏批发金额")]
        public decimal? LossesWholesaleAmount { get; set; } = 0;

        [HintDisplayName("盘盈成本金额", "盘盈成本金额")]
        public decimal? VolumeCostAmount { get; set; } = 0;

        [HintDisplayName("盘亏成本金额", "盘亏成本金额")]
        public decimal? LossesCostAmount { get; set; } = 0;

        [HintDisplayName("创建时间", "创建时间")]
        public DateTime CreatedOnUtc { get; set; }

        #region 商品信息
        [HintDisplayName("小单位", "规格属性小单位")]
        public int SmallUnitId { get; set; } = 0;

        [HintDisplayName("中单位", "规格属性中单位")]
        public int? StrokeUnitId { get; set; } = 0;

        [HintDisplayName("大单位", "规格属性大单位")]
        public int? BigUnitId { get; set; } = 0;

        #endregion

    }

    /// <summary>
    /// 项目保存或者编辑
    /// </summary>
    public class InventoryPartTaskUpdateModel : BaseEntityModel
    {

        public InventoryPartTaskUpdateModel()
        {
            Items = new List<InventoryPartTaskItemModel>();
        }

        [HintDisplayName("盘点人", "盘点人")]
        public int InventoryPerson { get; set; } = 0;

        [HintDisplayName("仓库", "仓库")]
        public int WareHouseId { get; set; } = 0;

        [HintDisplayName("盘点时间", "盘点时间")]
        public DateTime InventoryDate { get; set; }

        [HintDisplayName("备注", "备注")]
        public string Remark { get; set; }

        /// <summary>
        /// 操作源
        /// </summary>
        public int? Operation { get; set; }

        /// <summary>
        /// 项目
        /// </summary>
        public List<InventoryPartTaskItemModel> Items { get; set; }

    }
}
