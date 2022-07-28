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
    /// 用于表示盘点盈亏单列表
    /// </summary>
    public partial class InventoryProfitLossBillListModel : BaseModel
    {
        public InventoryProfitLossBillListModel()
        {
            PagingFilteringContext = new PagingFilteringModel();

        }

        public PagingFilteringModel PagingFilteringContext { get; set; }
        public IList<InventoryProfitLossBillModel> Items { get; set; }

        [HintDisplayName("经办人", "经办人")]
        public int? ChargePerson { get; set; } = 0;
        public string ChargePersonName { get; set; }
        public SelectList ChargePersons { get; set; }



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


        [HintDisplayName("备注", "备注")]
        public string Remark { get; set; }

    }



    /// <summary>
    /// 用于表示盘点盈亏单
    /// </summary>
    public class InventoryProfitLossBillModel : BaseEntityModel
    {
        public InventoryProfitLossBillModel()
        {
            Items = new List<InventoryProfitLossItemModel>();
        }
        public IList<InventoryProfitLossItemModel> Items { get; set; }


        [HintDisplayName("单据编号", "单据编号")]
        public string BillNumber { get; set; }
        public string BillBarCode { get; set; }



        [HintDisplayName("经办人", "经办人")]
        public int ChargePerson { get; set; } = 0;
        public string ChargePersonName { get; set; }
        public SelectList ChargePersons { get; set; }


        [HintDisplayName("仓库", "仓库")]
        public int WareHouseId { get; set; } = 0;
        public string WareHouseName { get; set; }
        public SelectList WareHouses { get; set; }


        [HintDisplayName("盘点时间", "盘点时间")]
        [UIHint("DateTime")]
        public DateTime InventoryDate { get; set; }
        [HintDisplayName("盘点日期（显示）", "盘点日期（显示）")]
        public string InventoryDateView { get; set; }


        [HintDisplayName("是否按最小单位盘点", "是否按最小单位盘点")]
        public bool InventoryByMinUnit { get; set; }


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
        [HintDisplayName("状态(审核)（显示）", " 状态(审核)（显示）")]
        public string AuditedStatusName { get; set; }


        [HintDisplayName("审核时间", "审核时间")]
        public DateTime? AuditedDate { get; set; }
        [HintDisplayName("调拨日期（显示）", "调拨日期（显示）")]
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

        /// <summary>
        /// 是否显示生产日期
        /// </summary>
        public bool IsShowCreateDate { get; set; }
        [HintDisplayName("数据来源", "数据来源")]
        public int Operation { get; set; } = 0;


    }


    /// <summary>
    /// 盘点盈亏单项目
    /// </summary>

    public class InventoryProfitLossItemModel : ProductBaseModel
    {


        [HintDisplayName("盘点盈亏单", "盘点盈亏单")]
        public int InventoryProfitLossBillId { get; set; } = 0;


        [HintDisplayName("数量", "数量")]
        public int Quantity { get; set; } = 0;


        [HintDisplayName("成本价", "成本价")]
        public decimal? CostPrice { get; set; } = 0;

        [HintDisplayName("成本金额", "成本金额")]
        public decimal? CostAmount { get; set; } = 0;


        //[HintDisplayName("库存", "库存")]
        //public int Stock { get; set; }

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

        public bool IsManufactureDete { get; set; }

        [HintDisplayName("生产日期", "生产日期")]
        public DateTime? ManufactureDete { get; set; }

    }




    /// <summary>
    /// 项目保存或者编辑
    /// </summary>
    public class InventoryProfitLossUpdateModel : BaseEntityModel
    {
        public InventoryProfitLossUpdateModel()
        {
            Items = new List<InventoryProfitLossItemModel>();
        }


        [HintDisplayName("经办人", "经办人")]
        public int ChargePerson { get; set; } = 0;


        [HintDisplayName("仓库", "仓库")]
        public int WareHouseId { get; set; } = 0;


        [HintDisplayName("盘点时间", "盘点时间")]
        public DateTime InventoryDate { get; set; }


        [HintDisplayName("是否按最小单位盘点", "是否按最小单位盘点")]
        public bool InventoryByMinUnit { get; set; }


        [HintDisplayName("备注", "备注")]
        public string Remark { get; set; }

        /// <summary>
        /// 操作源
        /// </summary>
        public int? Operation { get; set; }

        /// <summary>
        /// 项目
        /// </summary>
        public List<InventoryProfitLossItemModel> Items { get; set; }

    }
}
