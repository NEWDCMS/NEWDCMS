using DCMS.ViewModel.Models.Plan;
using DCMS.Web.Framework;
using DCMS.Web.Framework.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace DCMS.ViewModel.Models.Products
{
    public partial class ProductListModel : BaseModel
    {
        public ProductListModel()
        {
            PagingFilteringContext = new PagingFilteringModel();
            ExcludeIds = new List<int>();
            TargetDoms = new List<string>();
            PercentagePlans = new List<PercentagePlanModel>();
        }

        public int CurrentPercentagePlan { get; set; }
        public List<int> ExcludeIds { get; set; }
        public string TargetForm { get; set; }
        public List<string> TargetDoms { get; set; }
        public int RowIndex { get; set; }
        public string Target { get; set; }

        public PagingFilteringModel PagingFilteringContext { get; set; }
        public IList<ProductModel> Items { get; set; }
        public int ChannelId { get; set; } = 0;
        public int TerminalId { get; set; } = 0;
        public int BusinessUserId { get; set; } = 0;
        public int? BillType { get; set; } = 0;

        /// <summary>
        /// 提成方案
        /// </summary>
        public List<PercentagePlanModel> PercentagePlans { get; set; }

        #region 弹出框库存数量条件

        /// <summary>
        /// 仓库Id
        /// </summary>
        public int WareHouseId { get; set; } = 0;

        /// <summary>
        /// 库存数量大于
        /// </summary>
        public bool StockQtyMoreThan { get; set; } = false;

        /// <summary>
        /// 查询时是否包含商品详情 0:不包含，1:包含
        /// </summary>
        public bool IncludeProductDetail { get; set; } = true;
        public bool SingleSelect { get; set; } = false;

        #endregion

        /// <summary>
        /// 商品状态
        /// </summary>
        public bool? Status { get; set; }

        public int Storeid { get; set; }

    }



    //[Validator(typeof(ProductValidator))]
    public partial class ProductModel : ProductBaseModel, IParentList
    {
        public int CombinationId { get; set; } = 0;




        [HintDisplayName("商品名称", "商品名称")]
        public string Name { get; set; }

        [HintDisplayName("商品助记码", "商品助记码")]
        public string MnemonicCode { get; set; }

        [HintDisplayName("商品类型", "商品类型")]
        public int CategoryId { get; set; } = 0;
        [JsonIgnore]
        public SelectList ParentList { get; set; }

        [HintDisplayName("品牌", "品牌")]
        public int BrandId { get; set; } = 0;
        [HintDisplayName("品牌", "品牌")]
        public string BrandName { get; set; }

        [HintDisplayName("小单位", "规格属性小单位")]
        public int SmallUnitId { get; set; } = 0;
        public SelectList SmallUnits { get; set; }


        [HintDisplayName("中单位", "规格属性中单位")]
        public int? StrokeUnitId { get; set; } = 0;
        public SelectList StrokeUnits { get; set; }


        [HintDisplayName("大单位", "规格属性大单位")]
        public int? BigUnitId { get; set; } = 0;
        public SelectList BigUnits { get; set; }

        //[HintDisplayName("单位换算", "单位换算名（用于数据列表）")]
        //public string UnitName { get; set; }
        //public Dictionary<string, int> Units { get; set; }

        [HintDisplayName("状态", "如果停用，开单时无法选该商品")]
        public bool Status { get; set; }

        [HintDisplayName("是否启用生产日期", "是否启用生产日期")]
        public bool IsManufactureDete { get; set; }

        [HintDisplayName("生产日期", "生产日期")]
        public DateTime? ManufactureDete { get; set; }

        //是否启用生产日期配置
        public bool IsShowCreateDate { get; set; }


        [HintDisplayName("商品编码", "商品编码")]
        public string ProductCode { get; set; }

        [HintDisplayName("规格", "规格")]
        public string Specification { get; set; }

        [HintDisplayName("顺序号", "顺序号")]
        public int? Number { get; set; } = 0;

        [HintDisplayName("原产地", "原产地")]
        public string CountryOrigin { get; set; }

        [HintDisplayName("主供应商", "主供应商")]
        public int? Supplier { get; set; } = 0;
        public string SupplierName { get; set; }

        [HintDisplayName("其他条码", "其他条码")]
        public string OtherBarCode { get; set; }

        [HintDisplayName("其他条码1", "其他条码1")]
        public string OtherBarCode1 { get; set; }

        [HintDisplayName("其他条码2", "其他条码2")]
        public string OtherBarCode2 { get; set; }

        [HintDisplayName("统计类别", "统计类别")]
        public int? StatisticalType { get; set; } = 0;
        [HintDisplayName("统计类别", "统计类别")]
        public SelectList StatisticalTypes { get; set; }

        [HintDisplayName("保质期（天）", "保质期（天）")]
        public int? ExpirationDays { get; set; } = 0;

        [HintDisplayName("临期预警（天）", "临期预警（天）")]
        public int? AdventDays { get; set; } = 0;

        [HintDisplayName("商品图片", "商品图片")]
        public string ProductImages { get; set; }

        [HintDisplayName("是否分口味核算库存", "保存后,分口味核算库存,将不再允许改变")]
        public bool IsFlavor { get; set; }


        [HintDisplayName("条码", "小单位条码")]
        public string SmallBarCode { get; set; }

        [HintDisplayName("条码", "中单位条码")]
        public string StrokeBarCode { get; set; }

        [HintDisplayName("条码", "大单位条码")]
        public string BigBarCode { get; set; }



        //======================================
        //@mschen_ 补充  2018-10-19 14:37:12
        //======================================


        [HintDisplayName("商品SKU", "商品SKU")]
        public string Sku { get; set; }


        [HintDisplayName("排序", "排序")]
        public int DisplayOrder { get; set; }


        [HintDisplayName("提成方案", "提成方案")]
        public int? PercentageId { get; set; } = 0;


        [HintDisplayName("已经销售(开单)", "已经销售(开单)")]
        public bool HasSold { get; set; }


        public string PercentageCalCulateMethods { get; set; }
        public string PercentageSales { get; set; }
        public string PercentageReturns { get; set; }

        public Dictionary<string, string> UnitPriceDicts { get; set; } = new Dictionary<string, string>();

        /// <summary>
        /// 商品数量
        /// </summary>
        public int? Quantity { get; set; } = 0;

        /// <summary>
        /// 当前库存数量
        /// </summary>
        public int CurrentStock { get; set; }

        [HintDisplayName("生产日期列表", "生产日期列表")]
        public List<string> ProductTimes { get; set; }

    }





    public class StockQuantityModel
    {
        /// <summary>
        /// 可用库存数量
        /// </summary>
        public int UsableQuantity { get; set; } = 0;
        /// <summary>
        /// 现货库存数量
        /// </summary>
        public int CurrentQuantity { get; set; } = 0;

        public int WareHouseId { get; set; } = 0;
    }


    public class ProductPriceModel : BaseEntityModel
    {
        /// <summary>
        /// 商品Id
        /// </summary>
        public int ProductId { get; set; }
        /// <summary>
        /// 单位Id
        /// </summary>
        public int UnitId { get; set; } = 0;

        /// <summary>
        /// 单位名称
        /// </summary>
        public string UnitName { get; set; }

        /// <summary>
        /// 批发价
        /// </summary>
        public decimal? TradePrice { get; set; } = 0;
        /// <summary>
        /// 零售价
        /// </summary>
        public decimal? RetailPrice { get; set; } = 0;
        /// <summary>
        /// 最低售价
        /// </summary>
        public decimal? FloorPrice { get; set; } = 0;
        /// <summary>
        /// 进价
        /// </summary>
        public decimal? PurchasePrice { get; set; } = 0;
        /// <summary>
        /// 成本价
        /// </summary>
        public decimal? CostPrice { get; set; } = 0;
        /// <summary>
        /// 特价1
        /// </summary>
        public decimal? SALE1 { get; set; } = 0;
        /// <summary>
        /// 特价2
        /// </summary>
        public decimal? SALE2 { get; set; } = 0;
        /// <summary>
        /// 特价3
        /// </summary>
        public decimal? SALE3 { get; set; } = 0;
    }


    public class UnitPricesModel
    {

        /// <summary>
        /// 单位Id
        /// </summary>
        public int UnitId { get; set; } = 0;

        public ProductPriceModel ProductPrice { get; set; }

        /// <summary>
        /// 单位换算
        /// </summary>
        public string UnitConversion { get; set; }
    }


}