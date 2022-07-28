using System.ComponentModel;

namespace DCMS.Core.Domain.Products
{

    public enum PriceType : int
    {
        /// <summary>
        /// 进价
        /// </summary>
        [Description("进价")]
        ProductCost = 0,

        /// <summary>
        /// 批发价格
        /// </summary>
        [Description("批发价格")]
        WholesalePrice = 1,

        /// <summary>
        /// 零售价格
        /// </summary>
        [Description("零售价格")]
        RetailPrice = 2,

        /// <summary>
        /// 最低售价
        /// </summary>
        [Description("最低售价")]
        LowestPrice = 3,

        /// <summary>
        /// 上次售价
        /// </summary>
        [Description("上次售价")]
        LastedPrice = 4,

        /// <summary>
        /// 成本价
        /// </summary>
        [Description("成本价")]
        CostPrice = 5,

        [Description("自定义方案")]
        CustomPlan = 88
    }

    /// <summary>
    ///  用于表示商品的层次价格方案
    /// </summary>
    public class ProductTierPrice : BaseEntity
    {

        /// <summary>
        /// 商品
        /// </summary>
        public int ProductId { get; set; } = 0;

        /// <summary>
        /// 自定义方案
        /// </summary>
        public int PricesPlanId { get; set; } = 0;

        /// <summary>
        /// 表示价格类型
        /// </summary>
        public int PriceTypeId { get; set; } = 0;


        public PriceType PriceType
        {
            get { return (PriceType)PriceTypeId; }
            set { PriceTypeId = (byte)value; }
        }

        /// <summary>
        /// 小单位价格
        /// </summary>
        public decimal? SmallUnitPrice { get; set; } = 0;

        /// <summary>
        /// 中单位价格
        /// </summary>
        public decimal? StrokeUnitPrice { get; set; } = 0;

        /// <summary>
        /// 大单位价格
        /// </summary>
        public decimal? BigUnitPrice { get; set; } = 0;
        public virtual Product Product { get; set; }

    }


    /// <summary>
    /// 用于表示商品的层次价格的自定义方案
    /// </summary>
    public class ProductTierPricePlan : BaseEntity
    {

        /// 方案名
        /// </summary>
        public string Name { get; set; }
    }


    /// <summary>
    /// 表示当前经销商价格方案
    /// </summary>
    public class ProductPricePlan
    {
        /// <summary>
        /// 经销商
        /// </summary>
        public int StoreId { get; set; }
        /// <summary>
        /// 方案名
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 自定义方案
        /// </summary>
        public int PricesPlanId { get; set; }

        /// <summary>
        /// 表示价格类型
        /// </summary>
        public int PriceTypeId { get; set; }
    }


    /// <summary>
    /// 表示商品层次价格
    /// </summary>
    public class TierPrice
    {
        // <summary>
        /// 小单位
        /// </summary>
        public SpecificationAttributeOption SmallOption { get; set; }

        /// <summary>
        /// 中单位
        /// </summary>
        public SpecificationAttributeOption StrokOption { get; set; }


        /// <summary>
        /// 大单位
        /// </summary>
        public SpecificationAttributeOption BigOption { get; set; }


        /// <summary>
        /// 小单位价格
        /// </summary>
        public decimal? SmallUnitPrice { get; set; }

        /// <summary>
        /// 中单位价格
        /// </summary>
        public decimal? StrokeUnitPrice { get; set; }

        /// <summary>
        /// 大单位价格
        /// </summary>
        public decimal? BigUnitPrice { get; set; }


    }

}
