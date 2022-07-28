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
    /// 用于表示库存商品组合单列表
    /// </summary>
    public partial class CombinationProductBillListModel : BaseModel
    {
        public CombinationProductBillListModel()
        {
            PagingFilteringContext = new PagingFilteringModel();

        }

        public PagingFilteringModel PagingFilteringContext { get; set; }
        public IList<CombinationProductBillModel> Items { get; set; }

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
    /// 用于表示库存商品组合单
    /// </summary>
    public class CombinationProductBillModel : BaseEntityModel
    {

        public CombinationProductBillModel()
        {
            Items = new List<CombinationProductItemModel>();
        }
        public IList<CombinationProductItemModel> Items { get; set; }

        /// <summary>
        /// 业务员
        /// </summary>
        public string SalesmanName { get; set; }

        [HintDisplayName("单据编号", "单据编号")]
        public string BillNumber { get; set; }
        public string BillBarCode { get; set; }

        [HintDisplayName("仓库", "仓库")]
        public int WareHouseId { get; set; } = 0;
        public string WareHouseName { get; set; }
        public SelectList WareHouses { get; set; }

        [HintDisplayName("主商品", "主商品")]
        public int ProductId { get; set; } = 0;
        public string ProductName { get; set; }

        [HintDisplayName("主商品数量", "主商品数量")]
        public int? Quantity { get; set; } = 0;

        [HintDisplayName("主商品成本", "主商品成本")]
        public decimal? ProductCost { get; set; } = 0;

        [HintDisplayName("主商品成本金额", "主商品成本金额")]
        public decimal? ProductCostAmount { get; set; } = 0;

        [HintDisplayName("备注", "备注")]
        public string Remark { get; set; }

        [HintDisplayName("成本差额", "成本差额")]
        public decimal? CostDifference { get; set; } = 0;

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
    /// 库存商品组合单项目
    /// </summary>

    public class CombinationProductItemModel : ProductBaseModel
    {

        [HintDisplayName("库存商品组合单", "库存商品组合单")]
        public int CombinationProductBillId { get; set; } = 0;

        [HintDisplayName("子商品单位", "子商品单位")]
        public int? SubProductUnitId { get; set; } = 0;
        public string SubProductUnitName { get; set; }

        [HintDisplayName("子商品数量", "子商品数量")]
        public int? SubProductQuantity { get; set; } = 0;

        [HintDisplayName("数量", "数量")]
        public int? Quantity { get; set; } = 0;

        [HintDisplayName("单位成本", "单位成本")]
        public decimal? CostPrice { get; set; } = 0;

        [HintDisplayName("成本金额", "成本金额")]
        public decimal? CostAmount { get; set; } = 0;

        [HintDisplayName("库存", "库存")]
        public int? Stock { get; set; } = 0;

        [HintDisplayName("创建时间", "创建时间")]
        public DateTime CreatedOnUtc { get; set; }

        [HintDisplayName("备注", "备注")]
        public string Remark { get; set; }

    }

    /// <summary>
    /// 项目保存或者编辑
    /// </summary>
    public class CombinationProductUpdateModel : BaseEntityModel
    {

        public CombinationProductUpdateModel()
        {
            Items = new List<CombinationProductItemModel>();
        }

        [HintDisplayName("仓库", "仓库")]
        public int WareHouseId { get; set; } = 0;

        [HintDisplayName("主商品", "主商品")]
        public int ProductId { get; set; } = 0;

        [HintDisplayName("主商品数量", "主商品数量")]
        public int? Quantity { get; set; }

        [HintDisplayName("主商品成本", "主商品成本")]
        public decimal? ProductCost { get; set; } = 0;

        [HintDisplayName("主商品成本金额", "主商品成本金额")]
        public decimal? ProductCostAmount { get; set; } = 0;

        [HintDisplayName("备注", "备注")]
        public string Remark { get; set; }

        [HintDisplayName("成本差额", "成本差额")]
        public decimal? CostDifference { get; set; } = 0;

        /// <summary>
        /// 操作源
        /// </summary>
        public int? Operation { get; set; }

        /// <summary>
        /// 项目
        /// </summary>
        public List<CombinationProductItemModel> Items { get; set; }

    }
}
