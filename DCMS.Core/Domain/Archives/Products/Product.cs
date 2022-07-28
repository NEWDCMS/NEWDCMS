using DCMS.Core.Domain.WareHouses;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace DCMS.Core.Domain.Products
{

    public class UnitQuantityPart
    {
        /// <summary>
        /// 小单位量
        /// </summary>
        public int Small { get; set; }
        /// <summary>
        /// 中单位量
        /// </summary>
        public int Stroke { get; set; }
        /// <summary>
        /// 大单位量
        /// </summary>
        public int Big { get; set; }

    }

    public class ProductUnitOptions
    {
        public Product Product { get; set; } = new Product();
        public ProductUnitOption Option { get; set; } = new ProductUnitOption();
    }

    /// <summary>
    /// 产品单位规格
    /// </summary>
    public class ProductUnitOption
    {
        public ProductUnitOption()
        {
            smallOption = new SpecificationAttributeOption();
            strokOption = new SpecificationAttributeOption();
            bigOption = new SpecificationAttributeOption();
            smallPrice = new ProductPrice();
            strokPrice = new ProductPrice();
            bigPrice = new ProductPrice();
        }


        /// <summary>
        /// 小单位
        /// </summary>
        public SpecificationAttributeOption smallOption { get; set; }
        public ProductPrice smallPrice { get; set; }

        /// <summary>
        /// 中单位
        /// </summary>
        public SpecificationAttributeOption strokOption { get; set; }
        public ProductPrice strokPrice { get; set; }

        /// <summary>
        /// 大单位
        /// </summary>
        public SpecificationAttributeOption bigOption { get; set; }
        public ProductPrice bigPrice { get; set; }
    }

    /// <summary>
    /// 产品排序枚举
    /// </summary>
    public enum ProductSortingEnum
    {
        /// <summary>
        /// 位置
        /// </summary>
        Position = 0,
        /// <summary>
        /// 按名称从 A 到 Z
        /// </summary>
        NameAsc = 5,
        /// <summary>
        /// 按名称从: Z 到 A
        /// </summary>
        NameDesc = 6,
        /// <summary>
        /// 按价格从: 低 到 高
        /// </summary>
        PriceAsc = 10,
        /// <summary>
        /// 按价格从: 高 到 低
        /// </summary>
        PriceDesc = 11,
        /// <summary>
        /// 按创建时间
        /// </summary>
        CreatedOn = 15,
    }


    public class ProductView
    {
        public int Id { get; set; }
        public int StoreId { get; set; }

        /// <summary>
        /// 商品名称
        /// </summary>
        public string Name { get; set; } = "";
        /// <summary>
        /// 商品助记码
        /// </summary>
        public string MnemonicCode { get; set; } = CommonHelper.GenerateStr(5);
        /// <summary>
        /// 商品类型Id
        /// </summary>
        public int CategoryId { get; set; } = 0;
        /// <summary>
        /// 小单位
        /// </summary>
        public int SmallUnitId { get; set; } = 0;
        /// <summary>
        /// 中单位
        /// </summary>
        public int? StrokeUnitId { get; set; } = 0;
        /// <summary>
        /// 大单位
        /// </summary>
        public int? BigUnitId { get; set; } = 0;
        /// <summary>
        /// 是否允许优惠（开销售单，销售订单时，允许对销售商品进行改价）
        /// </summary>
        [Column(TypeName = "BIT(1)")]
        public bool IsAdjustPrice { get; set; } = false;
        /// <summary>
        /// 状态 1、正常 0、停用
        /// </summary>
        [Column(TypeName = "BIT(1)")]
        public bool Status { get; set; } = false;
        /// <summary>
        /// 是否启用生产日期 1、启用(包括全部启用和只启用销售单生产日期) 2、禁用
        /// </summary>
        [Column(TypeName = "BIT(1)")]
        public bool IsManufactureDete { get; set; } = false;

        /// <summary>
        /// 商品编码
        /// </summary>
        public string ProductCode { get; set; } = "";
        /// <summary>
        /// 规格
        /// </summary>
        public string Specification { get; set; } = "";

        /// <summary>
        /// 原产地
        /// </summary>
        public string CountryOrigin { get; set; } = "";
        /// <summary>
        /// 主供应商
        /// </summary>
        public int? Supplier { get; set; } = 0;
        /// <summary>
        /// 其他条码
        /// </summary>
        public string OtherBarCode { get; set; } = "";
        /// <summary>
        /// 其他条码1
        /// </summary>
        public string OtherBarCode1 { get; set; } = "";
        /// <summary>
        /// 其他条码2
        /// </summary>
        public string OtherBarCode2 { get; set; } = "";
        /// <summary>
        /// 统计类别
        /// </summary>
        public int? StatisticalType { get; set; } = 0;
        /// <summary>
        /// 保质期（天）
        /// </summary>
        public int? ExpirationDays { get; set; } = 0;
        /// <summary>
        /// 临期预警（天）
        /// </summary>
        public int? AdventDays { get; set; } = 0;
        /// <summary>
        /// 商品图片
        /// </summary>
        public string ProductImages { get; set; } = "";
        /// <summary>
        /// 是否分口味核算库存
        /// </summary>
        [Column(TypeName = "BIT(1)")]
        public bool IsFlavor { get; set; } = false;
        /// <summary>
        /// 大单位换算数
        /// </summary>
        public int? BigQuantity { get; set; } = 0;
        /// <summary>
        /// 中单位换算数
        /// </summary>
        public int? StrokeQuantity { get; set; } = 0;
        /// <summary>
        /// 小单位条码
        /// </summary>
        public string SmallBarCode { get; set; } = "";
        /// <summary>
        /// 中单位条码
        /// </summary>
        public string StrokeBarCode { get; set; } = "";
        /// <summary>
        /// 大单位条码
        /// </summary>
        public string BigBarCode { get; set; } = "";

        /// <summary>
        /// 品牌
        /// </summary>
        public int BrandId { get; set; } = 0;
        //======================================
        //@mschen_ 补充  2018-10-19 14:37:12
        //======================================

        /// <summary>
        /// 商品SKU
        /// </summary>
        public string Sku { get; set; } = "";

        /// <summary>
        /// 提供商编号
        /// </summary>
        public string ManufacturerPartNumber { get; set; } = "";

        /// <summary>
        /// 是否移除（数据库保留）
        /// </summary>
        [Column(TypeName = "BIT(1)")]
        public bool Deleted { get; set; } = false;

        /// <summary>
        /// 是否允许发布
        /// </summary>
        [Column(TypeName = "BIT(1)")]
        public bool Published { get; set; } = false;

        /// <summary>
        /// 在下单时（是否允许，在改商品的基础上添加其他商品（主要是用于商品的组合））
        /// </summary>
        [Column(TypeName = "BIT(1)")]
        public bool RequireOtherProducts { get; set; } = false;

        /// <summary>
        /// 允许包含产品标识（用“，”号分割）
        /// </summary>
        public string RequiredProductIds { get; set; } = "";

        /// <summary>
        ///库存量
        /// </summary>
        public int StockQuantity { get; set; } = 0;

        /// <summary>
        /// 显示库存
        /// </summary>
        [Column(TypeName = "BIT(1)")]
        public bool DisplayStockAvailability { get; set; } = false;

        /// <summary>
        /// 显示库存量
        /// </summary>
        [Column(TypeName = "BIT(1)")]
        public bool DisplayStockQuantity { get; set; } = false;

        /// <summary>
        /// 最小库存
        /// </summary>
        public int MinStockQuantity { get; set; } = 0;

        /// <summary>
        /// 低库存时处理
        /// </summary>
        public int LowStockActivityId { get; set; } = 0;

        /// <summary>
        /// 低库存通知管理员时数量
        /// </summary>
        public int NotifyAdminForQuantityBelow { get; set; } = 0;

        /// <summary>
        ///低库存时订单模式
        /// </summary>
        public int BackorderModeId { get; set; } = 0;

        /// <summary>
        /// 低库存是否允许下单
        /// </summary>
        [Column(TypeName = "BIT(1)")]
        public bool AllowBackInStockSubscriptions { get; set; } = false;

        /// <summary>
        /// 禁用下单
        /// </summary>
        [Column(TypeName = "BIT(1)")]
        public bool DisablePlaceButton { get; set; } = false;

        /// <summary>
        /// 如何管理库存
        /// </summary>
        public int ManageInventoryMethodId { get; set; } = 0;

        /// <summary>
        /// 更新时间
        /// </summary>
        public DateTime UpdatedOnUtc { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreatedOnUtc { get; set; }


        /// <summary>
        /// 排序
        /// </summary>
        public int DisplayOrder { get; set; } = 0;


        /// <summary>
        /// 提成方案
        /// </summary>
        public int? PercentageId { get; set; } = 0;


        /// <summary>
        /// 已经销售(开单)
        /// </summary>
        [Column(TypeName = "BIT(1)")]
        public bool HasSold { get; set; } = false;

        /// <summary>
        /// 获取或设置一个值，该值指示产品是否标记为免税
        /// </summary>
        [Column(TypeName = "BIT(1)")]
        public bool IsTaxExempt { get; set; }

        /// <summary>
        /// 获取或设置税务类别标识符
        /// </summary>
        public int TaxCategoryId { get; set; }

        /// <summary>
        /// 获取或设置一个值，该值指示此产品是否应用折扣
        /// <remarks>就像我们运行 AppliedDiscounts.Count > 0
        /// 我们将此属性用于性能优化：如果此属性设置为false，则不需要加载应用折扣导航属性
        /// </remarks>
        /// </summary>
        [Column(TypeName = "BIT(1)")]
        public bool HasDiscountsApplied { get; set; }


        /// <summary>
        /// 是否预设
        /// </summary>
        [Column(TypeName = "BIT(1)")]
        public bool IsPreset { get; set; }

        public IList<ProductVariantAttribute> ProductVariantAttributes
        { get; set; }
        public IList<ProductVariantAttributeCombination> ProductVariantAttributeCombinations
        { get; set; }
        public IList<ProductManufacturer> ProductManufacturers
        { get; set; }
        public IList<ProductCategory> ProductCategories
        { get; set; }
        public IList<ProductSpecificationAttribute> ProductSpecificationAttributes
        { get; set; }
        public IList<ProductPrice> ProductPrices
        { get; set; }
        public IList<ProductTierPrice> ProductTierPrices
        { get; set; }
        public IList<ProductPicture> ProductPictures
        { get; set; }
        public IList<Stock> Stocks
        { get; set; }
        public IList<ProductFlavor> ProductFlavors
        { get; set; }

    }
    /// <summary>
    /// 用于表示商品实体
    /// </summary>
    public class Product : BaseEntity
    {
        private ICollection<ProductCategory> _productCategories;
        private ICollection<ProductManufacturer> _productManufacturers;
        private ICollection<ProductSpecificationAttribute> _productSpecificationAttributes;
        private ICollection<ProductVariantAttribute> _productVariantAttributes;
        private ICollection<ProductVariantAttributeCombination> _productVariantAttributeCombinations;
        private ICollection<ProductPrice> _productPrices;
        private ICollection<ProductTierPrice> _productTierPrices;
        private ICollection<ProductPicture> _productPictures;
        private ICollection<Stock> _stocks;
        //口味
        private ICollection<ProductFlavor> _productFlavors;

        /// <summary>
        /// ERP 编码
        /// </summary>
        public int ERPProductId { get; set; } = 0;
        /// <summary>
        /// 商品名称
        /// </summary>
        public string Name { get; set; } = "";
        /// <summary>
        /// 商品助记码
        /// </summary>
        public string MnemonicCode { get; set; } = CommonHelper.GenerateStr(5);
        /// <summary>
        /// 商品类型Id
        /// </summary>
        public int CategoryId { get; set; } = 0;
        /// <summary>
        /// 小单位
        /// </summary>
        public int SmallUnitId { get; set; } = 0;
        /// <summary>
        /// 中单位
        /// </summary>
        public int? StrokeUnitId { get; set; } = 0;
        /// <summary>
        /// 大单位
        /// </summary>
        public int? BigUnitId { get; set; } = 0;
        /// <summary>
        /// 是否允许优惠（开销售单，销售订单时，允许对销售商品进行改价）
        /// </summary>
        [Column(TypeName = "BIT(1)")]
        public bool IsAdjustPrice { get; set; } = false;
        /// <summary>
        /// 状态 1、正常 0、停用
        /// </summary>
        [Column(TypeName = "BIT(1)")]
        public bool Status { get; set; } = false;
        /// <summary>
        /// 是否启用生产日期 1、启用(包括全部启用和只启用销售单生产日期) 2、禁用
        /// </summary>
        [Column(TypeName = "BIT(1)")]
        public bool IsManufactureDete { get; set; } = false;

        /// <summary>
        /// 商品编码
        /// </summary>
        public string ProductCode { get; set; } = "";
        /// <summary>
        /// 规格
        /// </summary>
        public string Specification { get; set; } = "";

        /// <summary>
        /// 原产地
        /// </summary>
        public string CountryOrigin { get; set; } = "";
        /// <summary>
        /// 主供应商
        /// </summary>
        public int? Supplier { get; set; } = 0;
        /// <summary>
        /// 其他条码
        /// </summary>
        public string OtherBarCode { get; set; } = "";
        /// <summary>
        /// 其他条码1
        /// </summary>
        public string OtherBarCode1 { get; set; } = "";
        /// <summary>
        /// 其他条码2
        /// </summary>
        public string OtherBarCode2 { get; set; } = "";
        /// <summary>
        /// 统计类别
        /// </summary>
        public int? StatisticalType { get; set; } = 0;
        /// <summary>
        /// 保质期（天）
        /// </summary>
        public int? ExpirationDays { get; set; } = 0;
        /// <summary>
        /// 临期预警（天）
        /// </summary>
        public int? AdventDays { get; set; } = 0;
        /// <summary>
        /// 商品图片
        /// </summary>
        public string ProductImages { get; set; } = "";
        /// <summary>
        /// 是否分口味核算库存
        /// </summary>
        [Column(TypeName = "BIT(1)")]
        public bool IsFlavor { get; set; } = false;
        /// <summary>
        /// 大单位换算数
        /// </summary>
        public int? BigQuantity { get; set; } = 0;
        /// <summary>
        /// 中单位换算数
        /// </summary>
        public int? StrokeQuantity { get; set; } = 0;
        /// <summary>
        /// 小单位条码
        /// </summary>
        public string SmallBarCode { get; set; } = "";
        /// <summary>
        /// 中单位条码
        /// </summary>
        public string StrokeBarCode { get; set; } = "";
        /// <summary>
        /// 大单位条码
        /// </summary>
        public string BigBarCode { get; set; } = "";

        /// <summary>
        /// 品牌
        /// </summary>
        public int BrandId { get; set; } = 0;

        //======================================
        //@mschen_ 补充  2018-10-19 14:37:12
        //======================================

        /// <summary>
        /// 商品SKU
        /// </summary>
        public string Sku { get; set; } = "";

        /// <summary>
        /// 提供商编号
        /// </summary>
        public string ManufacturerPartNumber { get; set; } = "";

        /// <summary>
        /// 是否移除（数据库保留）
        /// </summary>
        [Column(TypeName = "BIT(1)")]
        public bool Deleted { get; set; } = false;

        /// <summary>
        /// 是否允许发布
        /// </summary>
        [Column(TypeName = "BIT(1)")]
        public bool Published { get; set; } = false;

        /// <summary>
        /// 在下单时（是否允许，在改商品的基础上添加其他商品（主要是用于商品的组合））
        /// </summary>
        [Column(TypeName = "BIT(1)")]
        public bool RequireOtherProducts { get; set; } = false;

        /// <summary>
        /// 允许包含产品标识（用“，”号分割）
        /// </summary>
        public string RequiredProductIds { get; set; } = "";

        /// <summary>
        ///库存量
        /// </summary>
        public int StockQuantity { get; set; } = 0;

        /// <summary>
        /// 显示库存
        /// </summary>
        [Column(TypeName = "BIT(1)")]
        public bool DisplayStockAvailability { get; set; } = false;

        /// <summary>
        /// 显示库存量
        /// </summary>
        [Column(TypeName = "BIT(1)")]
        public bool DisplayStockQuantity { get; set; } = false;

        /// <summary>
        /// 最小库存
        /// </summary>
        public int MinStockQuantity { get; set; } = 0;

        /// <summary>
        /// 低库存时处理
        /// </summary>
        public int LowStockActivityId { get; set; } = 0;

        /// <summary>
        /// 低库存通知管理员时数量
        /// </summary>
        public int NotifyAdminForQuantityBelow { get; set; } = 0;

        /// <summary>
        ///低库存时订单模式
        /// </summary>
        public int BackorderModeId { get; set; } = 0;

        /// <summary>
        /// 低库存是否允许下单
        /// </summary>
        [Column(TypeName = "BIT(1)")]
        public bool AllowBackInStockSubscriptions { get; set; } = false;

        /// <summary>
        /// 禁用下单
        /// </summary>
        [Column(TypeName = "BIT(1)")]
        public bool DisablePlaceButton { get; set; } = false;

        /// <summary>
        /// 如何管理库存
        /// </summary>
        public int ManageInventoryMethodId { get; set; } = 0;

        /// <summary>
        /// 更新时间
        /// </summary>
        public DateTime UpdatedOnUtc { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreatedOnUtc { get; set; }


        /// <summary>
        /// 排序
        /// </summary>
        public int DisplayOrder { get; set; } = 0;


        /// <summary>
        /// 提成方案
        /// </summary>
        public int? PercentageId { get; set; } = 0;


        /// <summary>
        /// 已经销售(开单)
        /// </summary>
        [Column(TypeName = "BIT(1)")]
        public bool HasSold { get; set; } = false;

        /// <summary>
        /// 获取或设置一个值，该值指示产品是否标记为免税
        /// </summary>
        [Column(TypeName = "BIT(1)")]
        public bool IsTaxExempt { get; set; }

        /// <summary>
        /// 获取或设置税务类别标识符
        /// </summary>
        public int TaxCategoryId { get; set; }

        /// <summary>
        /// 获取或设置一个值，该值指示此产品是否应用折扣
        /// <remarks>就像我们运行 AppliedDiscounts.Count > 0
        /// 我们将此属性用于性能优化：如果此属性设置为false，则不需要加载应用折扣导航属性
        /// </remarks>
        /// </summary>
        [Column(TypeName = "BIT(1)")]
        public bool HasDiscountsApplied { get; set; }


        /// <summary>
        /// 是否预设
        /// </summary>
        [Column(TypeName = "BIT(1)")]
        public bool IsPreset { get; set; }


        /// <summary>
        /// 获取/设置管理库存
        /// </summary>
        public ManageInventoryMethod ManageInventoryMethod
        {
            get
            {
                return (ManageInventoryMethod)ManageInventoryMethodId;
            }
            set
            {
                ManageInventoryMethodId = (int)value;
            }
        }

        /// <summary>
        /// 低库存时的处理方法
        /// </summary>
        public LowStockActivity LowStockActivity
        {
            get
            {
                return (LowStockActivity)LowStockActivityId;
            }
            set
            {
                LowStockActivityId = (int)value;
            }
        }


        /// <summary>
        /// (导航)商品属性
        /// </summary>
        public virtual ICollection<ProductVariantAttribute> ProductVariantAttributes
        {
            get { return _productVariantAttributes ?? (_productVariantAttributes = new List<ProductVariantAttribute>()); }
            set { _productVariantAttributes = value; }
        }

        /// <summary>
        /// (导航)商品变体组合属性
        /// </summary>
        public virtual ICollection<ProductVariantAttributeCombination> ProductVariantAttributeCombinations
        {
            get { return _productVariantAttributeCombinations ?? (_productVariantAttributeCombinations = new List<ProductVariantAttributeCombination>()); }
            set { _productVariantAttributeCombinations = value; }
        }


        /// <summary>
        /// (导航)提供商
        /// </summary>
        public virtual ICollection<ProductManufacturer> ProductManufacturers
        {
            get { return _productManufacturers ?? (_productManufacturers = new List<ProductManufacturer>()); }
            set { _productManufacturers = value; }
        }

        /// <summary>
        /// (导航)商品类别
        /// </summary>
        public virtual ICollection<ProductCategory> ProductCategories
        {
            get { return _productCategories ?? (_productCategories = new List<ProductCategory>()); }
            set { _productCategories = value; }
        }

        /// <summary>
        ///  (导航)规格属性
        /// </summary>
        public virtual ICollection<ProductSpecificationAttribute> ProductSpecificationAttributes
        {
            get { return _productSpecificationAttributes ?? (_productSpecificationAttributes = new List<ProductSpecificationAttribute>()); }
            set { _productSpecificationAttributes = value; }
        }

        /// <summary>
        ///  (导航)商品价格
        /// </summary>
        public virtual ICollection<ProductPrice> ProductPrices
        {
            get { return _productPrices ?? (_productPrices = new List<ProductPrice>()); }
            set { _productPrices = value; }
        }

        /// <summary>
        ///  (导航)商品层次价格
        /// </summary>
        public virtual ICollection<ProductTierPrice> ProductTierPrices
        {
            get { return _productTierPrices ?? (_productTierPrices = new List<ProductTierPrice>()); }
            set { _productTierPrices = value; }
        }

        /// <summary>
        /// (导航)商品图片
        /// </summary>
        public virtual ICollection<ProductPicture> ProductPictures
        {
            get { return _productPictures ?? (_productPictures = new List<ProductPicture>()); }
            set { _productPictures = value; }
        }

        /// <summary>
        ///  (导航)库存
        /// </summary>

        public virtual ICollection<Stock> Stocks
        {
            get { return _stocks ?? (_stocks = new List<Stock>()); }
            set { _stocks = value; }
        }

        public virtual ICollection<ProductFlavor> ProductFlavors
        {
            get { return _productFlavors ?? (_productFlavors = new List<ProductFlavor>()); }
            set { _productFlavors = value; }
        }


        public Brand Brand { get; set; }
    }


    /// <summary>
    /// 拥有记录商品单位、价格
    /// </summary>
    public class RecordProductPrice
    {
        /// <summary>
        /// 经销商Id
        /// </summary>
        public int StoreId { get; set; }

        /// <summary>
        /// 客户Id
        /// </summary>
        public int TerminalId { get; set; }

        /// <summary>
        /// 商品Id
        /// </summary>
        public int ProductId { get; set; }

        /// <summary>
        /// 单位Id
        /// </summary>
        public int UnitId { get; set; }

        /// <summary>
        /// 价格
        /// </summary>
        public decimal Price { get; set; }

    }

    public class ProductUpdate : BaseEntity
    {



        public int CombinationId { get; set; } = 0;



        public string Name { get; set; }

        public string MnemonicCode { get; set; }

        public int CategoryId { get; set; } = 0;
        public SelectList ParentList { get; set; }

        public int BrandId { get; set; } = 0;
        public string BrandName { get; set; }

        public int SmallUnitId { get; set; } = 0;
        public SelectList SmallUnits { get; set; }


        public int? StrokeUnitId { get; set; } = 0;
        public SelectList StrokeUnits { get; set; }


        public int? BigUnitId { get; set; } = 0;
        public SelectList BigUnits { get; set; }

        //[HintDisplayName("单位换算", "单位换算名（用于数据列表）")]
        //public string UnitName { get; set; }
        //public Dictionary<string, int> Units { get; set; }

        public bool Status { get; set; }

        public bool IsManufactureDete { get; set; }

        public DateTime? ManufactureDete { get; set; }

        //是否启用生产日期配置
        public bool IsShowCreateDate { get; set; }


        public string ProductCode { get; set; }

        public string Specification { get; set; }

        public int? Number { get; set; } = 0;

        public string CountryOrigin { get; set; }

        public int? Supplier { get; set; } = 0;
        public string SupplierName { get; set; }

        public string OtherBarCode { get; set; }

        public string OtherBarCode1 { get; set; }

        public string OtherBarCode2 { get; set; }

        public int? StatisticalType { get; set; } = 0;
        public SelectList StatisticalTypes { get; set; }

        public int? ExpirationDays { get; set; } = 0;

        public int? AdventDays { get; set; } = 0;

        public string ProductImages { get; set; }

        public bool IsFlavor { get; set; }


        public string SmallBarCode { get; set; }

        public string StrokeBarCode { get; set; }

        public string BigBarCode { get; set; }



        //======================================
        //@mschen_ 补充  2018-10-19 14:37:12
        //======================================


        public string Sku { get; set; }





        public int DisplayOrder { get; set; }


        public int? PercentageId { get; set; } = 0;


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

        public List<string> ProductTimes { get; set; }



    }



}
