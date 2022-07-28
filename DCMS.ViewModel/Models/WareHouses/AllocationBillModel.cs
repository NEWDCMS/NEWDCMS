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
    /// 用于表示调拨单列表
    /// </summary>
    public partial class AllocationBillListModel : BaseModel
    {
        public AllocationBillListModel()
        {
            PagingFilteringContext = new PagingFilteringModel();
            Lists = new List<AllocationBillModel>();

        }

        public PagingFilteringModel PagingFilteringContext { get; set; }
        public IList<AllocationBillModel> Lists { get; set; }

        [HintDisplayName("出货仓库", "出货仓库")]
        public int? ShipmentWareHouseId { get; set; } = 0;
        public string ShipmentWareHouseName { get; set; }
        public SelectList ShipmentWareHouses { get; set; }

        [HintDisplayName("入货仓库", "入货仓库")]
        public int? IncomeWareHouseId { get; set; } = 0;
        public string IncomeWareHouseName { get; set; }
        public SelectList IncomeWareHouses { get; set; }

        [HintDisplayName("单据编号", "单据编号")]
        public string BillNumber { get; set; }
        public string BillBarCode { get; set; }

        [HintDisplayName("状态(审核)", "状态(审核)")]
        public bool? AuditedStatus { get; set; }

        [HintDisplayName("开始日期", "开始日期")]
        [UIHint("DateTime")]
        public DateTime StartTime { get; set; }

        [HintDisplayName("截止日期", "截止日期")]
        [UIHint("DateTime")]
        public DateTime EndTime { get; set; }

        [HintDisplayName(" 显示红冲的数据", " 显示红冲的数据")]
        public bool? ShowReverse { get; set; }

        [HintDisplayName("按审核时间", " 按审核时间")]
        public bool? SortByAuditedTime { get; set; }

        [HintDisplayName("备注", "备注")]
        public string Remark { get; set; }

        /// <summary>
        /// 调度单
        /// </summary>
        public string DispatchBillNumber { get; set; }
        public int DispatchBillId { get; set; }


        [HintDisplayName("商品", "商品")]
        public int ProductId { get; set; } = 0;
        public string ProductName { get; set; }
    }

    /// <summary>
    /// 用于表示调拨单
    /// </summary>
    public class AllocationBillModel : BaseEntityModel
    {
        public AllocationBillModel()
        {
            Items = new List<AllocationItemModel>();
        }

        public IList<AllocationItemModel> Items { get; set; }

        [HintDisplayName("单据编号", "单据编号")]
        public string BillNumber { get; set; }
        public string BillBarCode { get; set; }

        [HintDisplayName("出货仓库", "出货仓库")]
        public int ShipmentWareHouseId { get; set; } = 0;
        public string ShipmentWareHouseName { get; set; }
        public SelectList ShipmentWareHouses { get; set; }

        [HintDisplayName("入货仓库", "入货仓库")]
        public int IncomeWareHouseId { get; set; } = 0;
        public string IncomeWareHouseName { get; set; }
        public SelectList IncomeWareHouses { get; set; }

        [HintDisplayName("调拨日期", "调拨日期")]
        [UIHint("DateTime")]
        public DateTime CreatedOnUtc { get; set; }

        [HintDisplayName("调拨日期（显示）", "调拨日期（显示）")]
        public string CreatedOnUtcView { get; set; }

        [HintDisplayName("是否按最小单位调拨", "是否按最小单位调拨")]
        public bool AllocationByMinUnit { get; set; }

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

        [HintDisplayName("审核时间（显示）", "审核时间（显示）")]
        public string AuditedDateName { get; set; }

        [HintDisplayName("红冲人", "红冲人")]
        public int? ReversedUserId { get; set; } = 0;

        [HintDisplayName("红冲状态", "红冲状态")]
        public bool ReversedStatus { get; set; }

        [HintDisplayName("红冲时间", "红冲时间")]
        public DateTime? ReversedDate { get; set; }

        [HintDisplayName("打印数", "打印数")]
        public int? PrintNum { get; set; } = 0;

        [HintDisplayName("加载类型", "加载类型")]
        public int ModelLoadType { get; set; } = 0;

        [HintDisplayName("加载数据", "加载数据")]
        public string ModelLoadData { get; set; }

        /// <summary>
        /// 是否显示生产日期
        /// </summary>
        public bool IsShowCreateDate { get; set; }

        /// <summary>
        /// 显示订单占用库存
        /// </summary>
        public bool APPShowOrderStock { get; set; }
        [HintDisplayName("数据来源", "数据来源")]
        public int Operation { get; set; } = 0;
        /// <summary>
        /// 调度单
        /// </summary>
        public string DispatchBillNumber { get; set; }
        public int DispatchBillId { get; set; }

    }

    /// <summary>
    /// 调拨单项目
    /// </summary>
    public class AllocationItemModel : ProductBaseModel
    {

        [HintDisplayName("调拨单", "调拨单Id")]
        public int AllocationBillId { get; set; } = 0;

        [HintDisplayName("数量", "数量")]
        public int Quantity { get; set; } = 0;

        [HintDisplayName("批发价", "批发价")]
        public decimal? TradePrice { get; set; } = 0;

        [HintDisplayName("批发金额", "批发金额")]
        public decimal? WholesaleAmount { get; set; } = 0;

        [HintDisplayName("出库库存", "出库库存")]
        public int OutgoingStock { get; set; } = 0;

        [HintDisplayName("入库库存", "入库库存")]
        public int WarehousingStock { get; set; } = 0;

        [HintDisplayName("备注", "备注")]
        public string Remark { get; set; }

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

        public int? SortIndex { get; set; }

    }

    /// <summary>
    /// 项目保存或者编辑
    /// </summary>
    public class AllocationUpdateModel : BaseEntityModel
    {

        public AllocationUpdateModel()
        {
            Items = new List<AllocationItemModel>();
        }
        public string BillNumber { get; set; }

        [HintDisplayName("出货仓库", "出货仓库")]
        public int ShipmentWareHouseId { get; set; } = 0;

        [HintDisplayName("入货仓库", "入货仓库")]
        public int IncomeWareHouseId { get; set; } = 0;

        [HintDisplayName("调拨日期", "调拨日期")]
        public DateTime CreatedOnUtc { get; set; }

        [HintDisplayName("是否按最小单位调拨", "是否按最小单位调拨")]
        public bool AllocationByMinUnit { get; set; }

        [HintDisplayName("备注", "备注")]
        public string Remark { get; set; }

        /// <summary>
        /// 操作源
        /// </summary>
        public int? Operation { get; set; }

        /// <summary>
        /// 项目
        /// </summary>
        public List<AllocationItemModel> Items { get; set; }

    }

    /// <summary>
    /// 快速调拨
    /// </summary>
    public class QuickAllocationModel
    {
        /// <summary>
        /// 调拨类型
        /// </summary>
        public int AllocationTypeId { get; set; } = 0;


        [HintDisplayName("送货员", "送货员")]
        public int DeliveryUserId { get; set; } = 0;
        [HintDisplayName("送货员", "送货员")]
        public int DeliveryUserName { get; set; } = 0;
        public SelectList DeliveryUsers { get; set; }

        [HintDisplayName("商品类别", "商品类别")]
        public int? CategoryId { get; set; } = 0;
        [HintDisplayName("商品类别", "商品类别")]
        public string CategoryName { get; set; }
        public SelectList Categories { get; set; }

        [HintDisplayName("加载数据", "加载数据")]
        public int? LoadDataId { get; set; } = 0;
        [HintDisplayName("加载数据", "加载数据")]
        public string LoadDataName { get; set; }
        public SelectList LoadDatas { get; set; }

    }



}
