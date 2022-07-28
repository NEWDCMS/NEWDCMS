using DCMS.Web.Framework;
using DCMS.Web.Framework.Models;
using System.Collections.Generic;

namespace DCMS.ViewModel.Models.Products
{

    public class ProductBaseModel : BaseEntityModel
    {
        public ProductBaseModel()
        {
            BigProductPrices = new ProductPriceModel();
            StrokeProductPrices = new ProductPriceModel();
            SmallProductPrices = new ProductPriceModel();
            Prices = new List<UnitPricesModel>();
            Units = new Dictionary<string, int>();
            ProductTierPrices = new List<ProductTierPriceModel>();
            StockQuantities = new List<StockQuantityModel>();
            CostPrices = new Dictionary<int, decimal>();
        }

        [HintDisplayName("商品", "商品")]
        public int ProductId { get; set; } = 0;

        [HintDisplayName("商品名称", "商品名称")]
        public string ProductName { get; set; }

        public string CategoryName { get; set; }

        [HintDisplayName("商品编码", "商品编码")]
        public string ProductSKU { get; set; }


        [HintDisplayName("条形码", "条形码")]
        public string BarCode { get; set; }

        [HintDisplayName("单位", "单位")]
        public int UnitId { get; set; } = 0;
        public Dictionary<string, int> Units { get; set; }

        [HintDisplayName("单位名称", "单位名称")]
        public string UnitName { get; set; }

        [HintDisplayName("单位换算", "单位换算")]
        public string UnitConversion { get; set; }

        /// <summary>
        /// 是否允许优惠（开销售单，销售订单时，允许对销售商品进行改价）
        /// </summary>
        [HintDisplayName("是否允许调价", "是否允许调价")]
        public bool IsAdjustPrice { get; set; }

        [HintDisplayName("库存数量", "实时库存数量")]
        public int? StockQty { get; set; } = 0;

        /// <summary>
        /// 可用库存数量
        /// </summary>
        public int? UsableQuantity { get; set; } = 0;
        public string UsableQuantityConversion;

        /// <summary>
        /// 预占库存数量
        /// </summary>
        public int? CurrentQuantity { get; set; } = 0;
        public string CurrentQuantityConversion;

        /// <summary>
        ///预占库存数量 
        /// </summary>
        public int? OrderQuantity { get; set; } = 0;
        public string OrderQuantityConversion;

        /// <summary>
        /// 锁定库存数量
        /// </summary>
        public int? LockQuantity { get; set; } = 0;
        public string LockQuantityConversion;

        [HintDisplayName("大单位换算数", "大单位换算数,已开单商品则不能修改量")]
        public int? BigQuantity { get; set; } = 0;

        [HintDisplayName("中单位换算数", "中单位换算数,已开单商品则不能修改量")]
        public int? StrokeQuantity { get; set; } = 0;

        public SpecificationAttributeOptionModel smallOption { get; set; }
        public SpecificationAttributeOptionModel strokeOption { get; set; }
        public SpecificationAttributeOptionModel bigOption { get; set; }


        public ProductPriceModel BigProductPrices { get; set; }
        public ProductPriceModel StrokeProductPrices { get; set; }
        public ProductPriceModel SmallProductPrices { get; set; }

        /// <summary>
        /// 单位价格
        /// </summary>
        public List<UnitPricesModel> Prices { get; set; }

        /// <summary>
        /// 层次价格
        /// </summary>
        public List<ProductTierPriceModel> ProductTierPrices { get; set; }

        /// <summary>
        /// 成本价（预设进价、平均进价）
        /// </summary>
        public Dictionary<int, decimal> CostPrices { get; set; }

        /// <summary>
        /// 商品所有库存量
        /// </summary>
        public List<StockQuantityModel> StockQuantities { get; set; }

    }





}
