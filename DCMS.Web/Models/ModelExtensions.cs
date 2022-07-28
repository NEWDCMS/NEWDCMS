using DCMS.Core;
using DCMS.Core.Domain.Products;
using DCMS.Services.Products;
using DCMS.ViewModel.Models.Products;
using DCMS.Web.Infrastructure.Mapper.Extensions;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DCMS.Web.Models
{

    public static class ProductExtensions
    {
        /// <summary>
        /// 泛型扩展:初始商品模型基础数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="product"></param>
        /// <param name="m"></param>
        /// <param name="wareHouseId">如果不指定仓库,则不显示实时库存量</param>
        /// <param name="allOptions"></param>
        /// <param name="_productService"></param>
        /// <returns></returns>
        public static T InitBaseModel<T>(this Product product, T m,
            int? wareHouseId,
            IList<SpecificationAttributeOption> allOptions,
            IList<ProductPrice> allProductPrices,
            IList<ProductTierPrice> allProductTierPrices,
     
            IProductService _productService,
            int terminalId = 0) where T : ProductBaseModel
        {

            if (product == null)
            {
                throw new ArgumentNullException("product");
            }

            try
            {

                //获取规格 SmallProductPrices
                var option = product.GetProductUnit(allOptions, allProductPrices);

                option.smallOption.ConvertedQuantity = 1;
                option.strokOption.ConvertedQuantity = product.StrokeQuantity;
                option.bigOption.ConvertedQuantity = product.BigQuantity;

                option.smallOption.UnitConversion = string.Format("1{0} = {1}{2}", option.bigOption != null ? option.bigOption.Name : "/", product.BigQuantity, option.smallOption != null ? option.smallOption.Name : "/");
                option.strokOption.UnitConversion = string.Format("1{0} = {1}{2}", option.strokOption != null ? option.strokOption.Name : "/", product.StrokeQuantity, option.smallOption != null ? option.smallOption.Name : "/");
                option.bigOption.UnitConversion = string.Format("1{0} = {1}{2}", option.bigOption != null ? option.bigOption.Name : "/", product.BigQuantity, option.smallOption != null ? option.smallOption.Name : "/");

                var smallOption = option.smallOption;
                var strokOption = option.strokOption;
                var bigOption = option.bigOption;

                //大单位换算数
                m.BigQuantity = product.BigQuantity ?? 0;

                //中单位换算数
                m.StrokeQuantity = product.StrokeQuantity ?? 0;

                //商品名称
                m.ProductName = product != null ? (string.IsNullOrWhiteSpace(product.MnemonicCode) ? product.Name: product.MnemonicCode) : "";

                //商品编码(SKU)
                m.ProductSKU = product != null ? product.Sku : "";

                //条码
                m.BarCode = product != null ? product.SmallBarCode : "";

                //单位转换
                m.UnitConversion = product.GetProductUnitConversion(allOptions);

                //单位
                m.Units = product.GetProductUnits(allOptions);

                //是否允许调价
                m.IsAdjustPrice = product.IsAdjustPrice;

                //单位名(默认)
                if (m.UnitId == 0)
                {
                    m.UnitId = smallOption.Id;
                    var op = allOptions.Where(ao => ao.Id == product.SmallUnitId).FirstOrDefault();
                    m.UnitName = op == null ? "" : op.Name;
                }
                else
                {
                    var op = allOptions.Where(ao => ao.Id == m.UnitId).FirstOrDefault();
                    m.UnitName = op == null ? "" : op.Name;
                }

                //实时库存量(可用)
                m.StockQty = product.GetProductUsableQuantity(wareHouseId ?? 0);

                //可用库存
                m.UsableQuantity = product.GetProductUsableQuantity(wareHouseId ?? 0);
                if (m.UsableQuantity > 0)
                {
    
                    m.UsableQuantityConversion = product.GetConversionFormat(allOptions, product.SmallUnitId, m.UsableQuantity ?? 0);
                }
                //现货库存
                m.CurrentQuantity = product.GetProductCurrentQuantity(wareHouseId ?? 0);
                if (m.CurrentQuantity > 0)
                {
                    m.CurrentQuantityConversion = product.GetConversionFormat(allOptions, product.SmallUnitId, m.CurrentQuantity ?? 0);
                }
                //预占库存
                m.OrderQuantity = product.GetProductOrderQuantity(wareHouseId ?? 0);
                if (m.OrderQuantity > 0)
                {
                    m.OrderQuantityConversion = product.GetConversionFormat(allOptions, product.SmallUnitId, m.OrderQuantity ?? 0);
                }

                //获取单位
                var smallProductPrices = allProductPrices.Where(p => p.ProductId == product.Id && p.UnitId == product.SmallUnitId).FirstOrDefault();
                var strokeProductPrices = allProductPrices.Where(p => p.ProductId == product.Id && p.UnitId == (product.StrokeUnitId ?? 0)).FirstOrDefault();
                var bigProductPrices = allProductPrices.Where(p => p.ProductId == product.Id && p.UnitId == (product.BigUnitId ?? 0)).FirstOrDefault();

                m.SmallProductPrices = smallProductPrices != null ? smallProductPrices.ToModel<ProductPriceModel>() : new ProductPriceModel();
                m.StrokeProductPrices = strokeProductPrices != null ? strokeProductPrices.ToModel<ProductPriceModel>() : new ProductPriceModel();
                m.BigProductPrices = bigProductPrices != null ? bigProductPrices.ToModel<ProductPriceModel>() : new ProductPriceModel();
                m.smallOption = smallOption.ToModel<SpecificationAttributeOptionModel>();
                m.strokeOption = strokOption.ToModel<SpecificationAttributeOptionModel>();
                m.bigOption = bigOption.ToModel<SpecificationAttributeOptionModel>();


                //单位价格
                m.Prices = new List<UnitPricesModel>()
                {
                    new UnitPricesModel() { UnitId = smallOption.Id, ProductPrice = option.smallPrice.ToModel<ProductPriceModel>(),UnitConversion= option.smallOption.UnitConversion },
                    new UnitPricesModel() { UnitId = strokOption.Id, ProductPrice = option.strokPrice.ToModel<ProductPriceModel>(),UnitConversion= option.strokOption.UnitConversion},
                    new UnitPricesModel() { UnitId = bigOption.Id, ProductPrice = option.bigPrice.ToModel<ProductPriceModel>(),UnitConversion = option.bigOption.UnitConversion}
                };

                #region 商品价格体系（层次价格 + 方案价格）

                //当前商品的层次价格(过滤掉 上次售价)
                var curProductAPTPrices = allProductTierPrices
                    .Where(s => s.ProductId == product.Id && s.PriceTypeId != (int)PriceType.LastedPrice).ToList();

                var productTierPriceModels = new List<ProductTierPriceModel>();

                var productTierPrices1 = curProductAPTPrices
                    .Where(s => s.ProductId == product.Id && s.PriceTypeId == (int)PriceType.ProductCost)
                    .FirstOrDefault() ?? new ProductTierPrice();

                var productTierPrices2 = curProductAPTPrices
                   .Where(s => s.ProductId == product.Id && s.PriceTypeId == (int)PriceType.WholesalePrice)
                   .FirstOrDefault() ?? new ProductTierPrice();

                var productTierPrices3 = curProductAPTPrices
                   .Where(s => s.ProductId == product.Id && s.PriceTypeId == (int)PriceType.RetailPrice)
                   .FirstOrDefault() ?? new ProductTierPrice();

                var productTierPrices4 = curProductAPTPrices
                   .Where(s => s.ProductId == product.Id && s.PriceTypeId == (int)PriceType.LowestPrice)
                   .FirstOrDefault() ?? new ProductTierPrice();

                var productTierPrices5 = curProductAPTPrices
                   .Where(s => s.ProductId == product.Id && s.PriceTypeId == (int)PriceType.CostPrice)
                   .FirstOrDefault() ?? new ProductTierPrice();

                var tierPrices = new List<ProductTierPrice>
                    {
                        //进价
                        new ProductTierPrice()
                        {
                            Id= productTierPrices1?.Id??0,
                            StoreId = product.StoreId,
                            ProductId = product.Id,
                            PricesPlanId = 0,
                            PriceTypeId = (int)PriceType.ProductCost,
                            SmallUnitPrice = productTierPrices1?.SmallUnitPrice??0,
                            StrokeUnitPrice = productTierPrices1?.StrokeUnitPrice??0,
                            BigUnitPrice = productTierPrices1?.BigUnitPrice??0,
                        },
                        //批发
                        new ProductTierPrice()
                        {
                            Id= productTierPrices2?.Id??0,
                            StoreId = product.StoreId,
                            ProductId = product.Id,
                            PricesPlanId = 0,
                            PriceTypeId = (int)PriceType.WholesalePrice,
                            SmallUnitPrice = productTierPrices2?.SmallUnitPrice??0,
                            StrokeUnitPrice = productTierPrices2?.StrokeUnitPrice??0,
                            BigUnitPrice = productTierPrices2?.BigUnitPrice??0,
                        },
                        //零售
                        new ProductTierPrice()
                        {
                            Id= productTierPrices3?.Id??0,
                            StoreId = product.StoreId,
                            ProductId = product.Id,
                            PricesPlanId = 0,
                            PriceTypeId = (int)PriceType.RetailPrice,
                            SmallUnitPrice = productTierPrices3?.SmallUnitPrice??0,
                            StrokeUnitPrice = productTierPrices3?.StrokeUnitPrice??0,
                            BigUnitPrice = productTierPrices3?.BigUnitPrice??0,
                        },
                        //最低售价
                        new ProductTierPrice()
                        {
                            Id= productTierPrices4?.Id??0,
                            StoreId = product.StoreId,
                            ProductId = product.Id,
                            PricesPlanId = 0,
                            PriceTypeId = (int)PriceType.LowestPrice,
                            SmallUnitPrice = productTierPrices4?.SmallUnitPrice??0,
                            StrokeUnitPrice = productTierPrices4?.StrokeUnitPrice??0,
                            BigUnitPrice = productTierPrices4?.BigUnitPrice??0,
                        },
                        //成本价
                        new ProductTierPrice()
                        {
                             Id= productTierPrices5?.Id??0,
                            StoreId = product.StoreId,
                            ProductId = product.Id,
                            PricesPlanId = 0,
                            PriceTypeId = (int)PriceType.CostPrice,
                            SmallUnitPrice = productTierPrices5?.SmallUnitPrice??0,
                            StrokeUnitPrice = productTierPrices5?.StrokeUnitPrice??0,
                            BigUnitPrice = productTierPrices5?.BigUnitPrice??0,
                        }
                    };

                foreach (var p in tierPrices)
                {
                    var model = p.ToModel<ProductTierPriceModel>();


                    model.StoreId = p.StoreId;
                    model.ProductId = p.ProductId;
                    model.PricesPlanId = p.PricesPlanId;
                    model.PriceTypeId = p.PriceTypeId;
                    model.PriceType = p.PriceType;
                    model.PriceTypeName = CommonHelper.GetEnumDescription<PriceType>(p.PriceType);
                    model.SmallUnitPrice = p.SmallUnitPrice;
                    model.StrokeUnitPrice = p.StrokeUnitPrice;
                    model.BigUnitPrice = p.BigUnitPrice;

                    productTierPriceModels.Add(model);
                }


                //自定义方案:当前商品的方案价格
                var curProductPlanPrices = allProductTierPrices
                    .Where(s => s.ProductId == product.Id && s.PriceTypeId == (int)PriceType.CustomPlan).ToList();

                var plans = _productService.GetAllPlansByStore(product.StoreId);
                foreach (var p in plans)
                {
                    var planPrice = curProductPlanPrices
                        .Where(s => s.ProductId == product.Id && s.PriceTypeId == (int)PriceType.CustomPlan && s.PricesPlanId == p.Id)
                        .FirstOrDefault() ?? new ProductTierPrice();

                    productTierPriceModels.Add(new ProductTierPriceModel()
                    {
                        StoreId = product.StoreId,
                        ProductId = product.Id,
                        PricesPlanId = p.Id,
                        PriceTypeId = (int)PriceType.CustomPlan,
                        PriceTypeName = p.Name,
                        SmallUnitPrice = planPrice?.SmallUnitPrice ?? 0,
                        StrokeUnitPrice = planPrice?.StrokeUnitPrice ?? 0,
                        BigUnitPrice = planPrice?.BigUnitPrice ?? 0,
                        Id = planPrice?.Id ?? 0
                    });
                }

                #endregion



                //上次售价单独取值 RecentPrice 表
                var recentPrice = _productService.GetRecentPrice(product.StoreId, terminalId, product.Id);
                ProductTierPriceModel lastedPriceModel = new ProductTierPriceModel
                {
                    StoreId = product.StoreId,
                    ProductId = product.Id,
                    PricesPlanId = 0,
                    PriceTypeId = (int)PriceType.LastedPrice,
                    SmallUnitPrice = recentPrice == null ? 0 : recentPrice.SmallUnitPrice,
                    StrokeUnitPrice = recentPrice == null ? 0 : recentPrice.StrokeUnitPrice,
                    BigUnitPrice = recentPrice == null ? 0 : recentPrice.BigUnitPrice
                };
                productTierPriceModels.Add(lastedPriceModel);
            
                m.ProductTierPrices = productTierPriceModels;

                //所有可用库存量
                m.StockQuantities = product.Stocks.Select(s =>
                {
                    return new StockQuantityModel
                    {
                        UsableQuantity = s.UsableQuantity ?? 0,
                        CurrentQuantity = s.CurrentQuantity ?? 0,
                        WareHouseId = s.WareHouseId
                    };
                }).ToList();

                return m;
            }
            catch (Exception ex)
            {
                throw new ArgumentNullException(ex.Message);
            }
        }

    }
}
