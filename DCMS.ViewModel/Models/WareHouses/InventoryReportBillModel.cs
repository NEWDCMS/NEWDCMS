using DCMS.Web.Framework;
using DCMS.Web.Framework.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace DCMS.ViewModel.Models.Sales
{

    public partial class InventoryReportBillListModel : BaseModel, IParentList
    {

        public InventoryReportBillListModel()
        {
            PagingFilteringContext = new PagingFilteringModel();
            Lists = new List<InventoryReportBillModel>();
            DynamicColumns = new List<string>();
        }

        public PagingFilteringModel PagingFilteringContext { get; set; }
        public IList<InventoryReportBillModel> Lists { get; set; }
        public List<string> DynamicColumns { get; set; }


        [HintDisplayName("客户Id", "客户Id")]
        public int TerminalId { get; set; } = 0;
        [HintDisplayName("客户", "客户")]
        public string TerminalName { get; set; }

        [HintDisplayName("单据编号", "单据编号")]
        public string BillNumber { get; set; }

        [HintDisplayName("业务员", "业务员")]
        public int? BusinessUserId { get; set; } = 0;
        public SelectList BusinessUsers { get; set; }

        [HintDisplayName("部门", "部门名称")]
        public int DepartmentId { get; set; } = 0;
        public SelectList ParentList { get; set; }

        [HintDisplayName("片区", "片区")]
        public int DistrictId { get; set; } = 0;
        public SelectList Districts { get; set; }

        [HintDisplayName("备注", "备注")]
        public string Remark { get; set; }

        [DisplayName("开始时间")]
        [UIHint("DateTime")]
        public DateTime StartTime { get; set; }

        [DisplayName("结束时间")]
        [UIHint("DateTime")]
        public DateTime EndTime { get; set; }


    }


    /// <summary>
    /// 用于门店库存上报
    /// </summary>
    public class InventoryReportBillModel : BaseEntityModel
    {

        public InventoryReportBillModel()
        {
            Items = new List<InventoryReportItemModel>();
        }



        [HintDisplayName("单据编号", "单据编号")]
        public string BillNumber { get; set; }
        public string BillBarCode { get; set; }

        [HintDisplayName("客户Id", "客户Id")]
        public int TerminalId { get; set; } = 0;
        [HintDisplayName("客户", "客户")]
        public string TerminalName { get; set; }
        [HintDisplayName("客户编码", "客户编码")]
        public string TerminalPointCode { get; set; }

        [HintDisplayName("业务员", "业务员")]
        public int BusinessUserId { get; set; } = 0;
        [HintDisplayName("业务员", "业务员")]
        public string BusinessUserName { get; set; }
        public SelectList BusinessUsers { get; set; }

        [HintDisplayName("红冲人", "红冲人")]
        public int? ReversedUserId { get; set; } = 0;
        [HintDisplayName("红冲人", "红冲人")]
        public string ReversedUserName { get; set; }
        [HintDisplayName("红冲状态", "红冲状态")]
        public bool ReversedStatus { get; set; }
        [HintDisplayName("红冲时间", "红冲时间")]
        public DateTime? ReversedDate { get; set; }

        /// <summary>
        /// 项目
        /// </summary>
        public IList<InventoryReportItemModel> Items { get; set; }

    }


    /// <summary>
    /// 门店上报商品明细
    /// </summary>
    public class InventoryReportItemModel : BaseEntityModel
    {

        public InventoryReportItemModel()
        {
            InventoryReportStoreQuantities = new List<InventoryReportStoreQuantityModel>();
        }

        public int InventoryReportBillId { get; set; } = 0;



        /// <summary>
        /// 商品Id
        /// </summary>
        public int ProductId { get; set; } = 0;

        /// <summary>
        /// 商品名称
        /// </summary>
        public string ProductName { get; set; }

        /// <summary>
        /// 采购大单位
        /// </summary>
        public int BigUnitId { get; set; } = 0;

        /// <summary>
        /// 采购大单位名称
        /// </summary>
        public string BigUnitName { get; set; }

        /// <summary>
        /// 采购大单位数量
        /// </summary>
        public int BigQuantity { get; set; } = 0;


        /// <summary>
        /// 采购小单位
        /// </summary>
        public int SmallUnitId { get; set; } = 0;

        /// <summary>
        /// 采购小单位名称
        /// </summary>
        public string SmallUnitName { get; set; }

        /// <summary>
        /// 采购小单位数量
        /// </summary>
        public int SmallQuantity { get; set; } = 0;

        /// <summary>
        /// 项目
        /// </summary>
        public IList<InventoryReportStoreQuantityModel> InventoryReportStoreQuantities { get; set; }


    }

    public class InventoryReportStoreQuantityModel : BaseEntityModel
    {
        /// <summary>
        /// 门店上报商品明细Id
        /// </summary>
        public int InventoryReportItemId { get; set; } = 0;



        /// <summary>
        /// 大单位库存量
        /// </summary>
        public int BigStoreQuantity { get; set; } = 0;

        /// <summary>
        /// 小单位库存量
        /// </summary>
        public int SmallStoreQuantity { get; set; } = 0;

        /// <summary>
        /// 生产日期
        /// </summary>
        public DateTime? ManufactureDete { get; set; }

    }

    /// <summary>
    /// 上报汇总表
    /// </summary>
    public class InventoryReportSummaryModel : BaseEntityModel
    {


        /// <summary>
        /// 客户
        /// </summary>
        public int TerminalId { get; set; } = 0;
        public string TerminalName { get; set; }

        /// <summary>
        /// 业务员
        /// </summary>
        public int BusinessUserId { get; set; } = 0;
        public string BusinessUserName { get; set; }

        /// <summary>
        /// 商品Id
        /// </summary>
        public int? ProductId { get; set; } = 0;
        /// <summary>
        /// 商品编号
        /// </summary>
        public string ProductCode { get; set; }
        /// <summary>
        /// 商品名称
        /// </summary>
        public string ProductName { get; set; }

        /// <summary>
        /// 条形码（小）
        /// </summary>
        public string SmallBarCode { get; set; }
        /// <summary>
        /// 条形码（中）
        /// </summary>
        public string StrokeBarCode { get; set; }
        /// <summary>
        /// 条形码（大）
        /// </summary>
        public string BigBarCode { get; set; }

        /// <summary>
        /// 单位Id（小）
        /// </summary>
        public int? SmallUnitId { get; set; } = 0;
        /// <summary>
        /// 单位名称（小）
        /// </summary>
        public string SmallUnitName { get; set; }
        /// <summary>
        /// 单位Id（中）
        /// </summary>
        public int? StrokeUnitId { get; set; } = 0;
        /// <summary>
        /// 单位名称（中）
        /// </summary>
        public string StrokeUnitName { get; set; }
        /// <summary>
        /// 单位Id（大）
        /// </summary>
        public int? BigUnitId { get; set; } = 0;
        /// <summary>
        /// 单位名称（大）
        /// </summary>
        public string BigUnitName { get; set; }

        /// <summary>
        /// 大转小
        /// </summary>
        public int? BigQuantity { get; set; } = 0;
        /// <summary>
        /// 中转小
        /// </summary>
        public int? StrokeQuantity { get; set; } = 0;

        /// <summary>
        /// 单位换算
        /// </summary>
        public string UnitConversion { get; set; }

        /// <summary>
        /// 期初数量
        /// </summary>
        public int BeginStoreQuantity { get; set; } = 0;
        public string BeginStoreQuantityConversion { get; set; }
        /// <summary>
        /// 期初时间
        /// </summary>
        public DateTime BeginDate { get; set; }

        /// <summary>
        /// 期末数量
        /// </summary>
        public int EndStoreQuantity { get; set; } = 0;
        public string EndStoreQuantityConversion { get; set; }
        /// <summary>
        /// 期末时间
        /// </summary>
        public DateTime? EndDate { get; set; }


        /// <summary>
        /// 采购量
        /// </summary>
        public int PurchaseQuantity { get; set; } = 0;
        public string PurchaseQuantityConversion { get; set; }

        /// <summary>
        /// 销售量
        /// </summary>
        public int SaleQuantity { get; set; } = 0;
        public string SaleQuantityConversion { get; set; }

    }

    public class InventoryReportSummaryListModel
    {
        public InventoryReportSummaryListModel()
        {
            PagingFilteringContext = new PagingFilteringModel();
            Lists = new List<InventoryReportSummaryModel>();
            DynamicColumns = new List<string>();
        }

        public PagingFilteringModel PagingFilteringContext { get; set; }
        public IList<InventoryReportSummaryModel> Lists { get; set; }
        public List<string> DynamicColumns { get; set; }


        #region  用于条件检索

        [HintDisplayName("商品", "商品")]
        public int? ProductId { get; set; } = 0;
        public string ProductName { get; set; }

        [HintDisplayName("客户", "客户")]
        public int? TerminalId { get; set; } = 0;
        public string TerminalName { get; set; }

        [HintDisplayName("业务员", "业务员")]
        public int? BusinessUserId { get; set; } = 0;
        public string BusinessUserName { get; set; }
        public SelectList BusinessUsers { get; set; }


        [HintDisplayName("客户渠道", "客户渠道")]
        public int? ChannelId { get; set; } = 0;
        public string ChannelName { get; set; }
        public SelectList Channels { get; set; }


        [HintDisplayName("客户等级", "客户等级")]
        public int? RankId { get; set; } = 0;
        public string RankName { get; set; }
        public SelectList Ranks { get; set; }

        [HintDisplayName("客户片区", "客户片区")]
        public int? DistrictId { get; set; } = 0;
        public string DistrictName { get; set; }
        public SelectList Districts { get; set; }


        [HintDisplayName("开始日期", "开始日期")]
        [UIHint("DateTime")]
        public DateTime StartTime { get; set; }

        [HintDisplayName("截止日期", "截止日期")]
        [UIHint("DateTime")]
        public DateTime EndTime { get; set; }

        #endregion 



    }




    /// <summary>
    /// 项目保存或者编辑
    /// </summary>
    public class InventoryReportBillUpdateModel
    {
        /// <summary>
        /// 经销商
        /// </summary>
        //移除 = 0;
        /// <summary>
        /// 单据编号
        /// </summary>
        public string BillNumber { get; set; }

        /// <summary>
        /// 仓库
        /// </summary>
        public int WareHouseId { get; set; } = 0;

        /// <summary>
        /// 业务员
        /// </summary>
        public int BusinessUserId { get; set; } = 0;
        /// <summary>
        /// 客户
        /// </summary>
        public int TerminalId { get; set; } = 0;
        /// <summary>
        /// 红冲人
        /// </summary>
        public int? ReversedUserId { get; set; } = 0;
        /// <summary>
        /// 红冲状态
        /// </summary>
        public bool ReversedStatus { get; set; }
        /// <summary>
        /// 红冲时间
        /// </summary>
        public DateTime? ReversedDate { get; set; }
        /// <summary>
        /// 来源
        /// </summary>
        public int Operation { get; set; } = 0;

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreatedOnUtc { get; set; }
        /// <summary>
        /// 备注
        /// </summary>
        public string Remark { get; set; }

        /// <summary>
        /// 项目
        /// </summary>
        public List<InventoryReportItemModel> Items { get; set; }



    }

}
