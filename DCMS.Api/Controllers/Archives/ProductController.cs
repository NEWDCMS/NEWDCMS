using DCMS.Api.Infrastructure.Mapper.Extensions;
using DCMS.Api.Models;
using DCMS.Core;
using DCMS.Core.Caching;
using DCMS.Core.Domain.Configuration;
using DCMS.Core.Domain.Products;
using DCMS.Services.Configuration;
using DCMS.Services.Products;
using DCMS.Services.Purchases;
using DCMS.Services.Users;
using DCMS.Services.WareHouses;
using DCMS.ViewModel.Models.Products;
using DCMS.Web.Framework.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Swashbuckle.AspNetCore.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace DCMS.Api.Controllers
{

    /// <summary>
    /// 用于商品信息管理
    /// </summary>
    [Authorize]
    [Route("api/v{version:apiVersion}/dcms/archives")]
    public class ProductController : BaseAPIController
    {
        private readonly ICategoryService _categoryService;
        private readonly IProductService _productService;
        private readonly ISpecificationAttributeService _specificationAttributeService;
        private readonly IBrandService _brandService;

        private readonly IPurchaseBillService _purchaseBillService;
        private readonly IRedLocker _locker;
        private readonly IUserService _userService;
        private readonly ISettingService _settingService;
        private readonly IStockService _stockService;

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="categoryService"></param>
        /// <param name="productService"></param>
        /// <param name="brandService"></param>
        /// <param name="specificationAttributeService"></param>
        /// <param name="purchaseBillService"></param>
        /// <param name="locker"></param>
        /// <param name="stockService"></param>
        /// <param name="userService"></param>
        /// <param name="settingService"></param>
        /// <param name="logger"></param>
        public ProductController(ICategoryService categoryService,
            IProductService productService,
            IBrandService brandService,
            ISpecificationAttributeService specificationAttributeService,
            IPurchaseBillService purchaseBillService,
            IRedLocker locker,
            IStockService stockService,
            IUserService userService,
            ISettingService settingService,
            ILogger<BaseAPIController> logger) : base(logger)
        {
            _categoryService = categoryService;
            _purchaseBillService = purchaseBillService;
            _productService = productService;
            _brandService = brandService;

            _specificationAttributeService = specificationAttributeService;
            _locker = locker;
            _userService = userService;
            _settingService = settingService;
            _stockService = stockService;
        }


        /// <summary>
        /// 获取经销商商品列表
        /// </summary>
        /// <param name="store"></param>
        /// <param name="excludeIds"></param>
        /// <param name="key"></param>
        /// <param name="categoryIds"></param>
        /// <param name="terminalid"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        [HttpGet("product/getProducts/{store}")]
        [SwaggerOperation("getProducts")]
        //[ValidateActionParameters] PageData<TOutput>
        //[AuthBaseFilter]
        public async Task<APIResult<PageData<ProductModel>>> GetProducts(int? store, [FromQuery] int[] excludeIds, string key, [FromQuery] int[] categoryIds = null, int? terminalid = 0, int wareHouseId = 0, int pageIndex = 0, int pageSize = 20, bool? usablequantity = false, int brandId = 0)
        {
            if (!store.HasValue || store.Value == 0)
                return this.Error3<PageData<ProductModel>>("参数错误！");

            return await Task.Run(() =>
            {
                try
                {
                    var cates = _categoryService.GetAllCategories(store ?? 0, "全部").FirstOrDefault(); //排除全部
                    if (categoryIds == null || categoryIds.Length == 0 || (cates != null && categoryIds.Contains(cates.Id)))
                        categoryIds = new int[] { };

                    var gridModel = _productService.SearchProducts(store,
                                  excludeIds,
                                  categoryIds,
                                  usablequantity,
                                  true,
                                  null,
                                  key: key,
                                  includeSpecificationAttributes: true,
                                  includeVariantAttributes: false,
                                  includeTierPrices: true,
                                  includeVariantAttributeCombinations: false,
                                  includePictures: false,
                                  wareHouseId: wareHouseId,
                                  productStatus: 1,
                                  pageIndex: pageIndex,
                                  pageSize: pageSize,
                                  brandId: brandId);


                    var allOptions = new List<SpecificationAttributeOption>();
                    var allProductPrices = new List<ProductPrice>();
                    var allProductTierPrices = new List<ProductTierPrice>();

                    foreach (var p in gridModel)
                    {
                        foreach (var sap in p.ProductSpecificationAttributes)
                        {
                            if (sap.SpecificationAttributeOption != null)
                                allOptions.Add(sap.SpecificationAttributeOption);
                        }

                        foreach (var pp in p.ProductPrices)
                        {
                            if (pp != null)
                                allProductPrices.Add(pp);
                        }

                        foreach (var ptp in p.ProductTierPrices)
                        {
                            if (ptp != null)
                                allProductTierPrices.Add(ptp);
                        }
                    }

                    var products = gridModel.Select(m =>
                    {

                        var p = m.ToModel<ProductModel>();
                        p.Name = string.IsNullOrWhiteSpace(p.MnemonicCode) ? p.Name : p.MnemonicCode;
                        var cat = m.ProductCategories.FirstOrDefault(ca => ca.Id == m.CategoryId);

                        p.ProductId = m.Id;
                        p.CategoryName = cat != null ? cat.Category?.Name : "";
                        p.BrandName = m.Brand?.Name;

                        p = m.InitBaseModel<ProductModel>(p,
                            wareHouseId,
                            allOptions,
                            allProductPrices,
                            allProductTierPrices,
                            _productService,
                            terminalid ?? 0);

                        p.ProductTimes = _productService.GetProductDates(store ?? 0, p.Id, wareHouseId);

                        var currentQuantity = _stockService.GetProductCurrentQuantity(store ?? 0, p.ProductId, wareHouseId);
                        var usableQuantity = _stockService.GetProductUsableQuantity(store ?? 0, p.ProductId, wareHouseId);

                        //现货库存数量
                        p.CurrentQuantity = currentQuantity;
                        if (p.CurrentQuantity > 0)
                        {
                            p.CurrentQuantityConversion = m.GetConversionFormat(allOptions, m.SmallUnitId, p.CurrentQuantity ?? 0);
                        }
                        else
                        {
                            p.CurrentQuantity = _stockService.GetProductCurrentQuantity(store ?? 0, m.Id, wareHouseId);
                            if (p.CurrentQuantity > 0)
                            {
                                p.CurrentQuantityConversion = m.GetConversionFormat(allOptions, m.SmallUnitId, p.CurrentQuantity ?? 0);
                            }
                        }

                        //可用库存数量
                        p.UsableQuantity = usableQuantity;
                        if (p.UsableQuantity > 0)
                        {
                            p.UsableQuantityConversion = m.GetConversionFormat(allOptions, m.SmallUnitId, p.UsableQuantity ?? 0);
                        }
                        else
                        {
                            p.UsableQuantity = _stockService.GetProductUsableQuantity(store ?? 0, m.Id, wareHouseId);
                            if (p.UsableQuantity > 0)
                            {
                                p.UsableQuantityConversion = m.GetConversionFormat(allOptions, m.SmallUnitId, p.UsableQuantity ?? 0);
                            }
                        }

                        //成本价
                        p.CostPrices = _purchaseBillService.GetReferenceCostPrice(m);

                        return p;
                    }).ToList();

                    var newPages = new PageData<ProductModel>(products, gridModel.TotalCount);

                    return this.Successful3("", newPages);
                }
                catch (Exception ex)
                {
                    return this.Error3<PageData<ProductModel>>(ex.Message);
                }

            });
        }


        /// <summary>
        /// 获取经销商商品信息
        /// </summary>
        /// <param name="store"></param>
        /// <param name="productId"></param>
        /// <returns></returns>
        [HttpGet("product/getProductById/{store}/{productId}")]
        [SwaggerOperation("getProductById")]
        //[ValidateActionParameters]
        //[AuthBaseFilter]
        public async Task<APIResult<ProductModel>> GetProductById(int? store, int productId)
        {
            if (!store.HasValue || store.Value == 0)
                return this.Error3<ProductModel>("参数错误！");

            return await Task.Run(() =>
            {
                try
                {
                    var product = ParseProduct(store, 0,productId);
                    if (product != null)
                    {
                        return this.Successful("", product);
                    }
                    else
                    {
                        return this.Error3<ProductModel>("产品不存在");
                    }
                }
                catch (Exception ex)
                {
                    return this.Error3<ProductModel>(ex.Message);
                }
            });
        }

        private ProductModel ParseProduct(int? store,int wareHouseId, int productId)
        {
            var m = _productService.GetProductById(store, productId);
            if (m != null)
            {

                var allOptions = _specificationAttributeService.GetSpecificationAttributeOptionByIds(store, new List<int>() { m == null ? 0 : m.SmallUnitId, m == null ? 0 : m.StrokeUnitId ?? 0, m == null ? 0 : m.BigUnitId ?? 0 });

                var allProductTierPrices = _productService.GetProductTierPriceByProductIds(store, new[] { productId });
                var allProductPrices = _productService.GetProductPricesByProductIds(store, new int[] { m.Id });

                var cat = _categoryService.GetCategoryById(store, m.CategoryId); ;
                var brand = _brandService.GetBrandById(store, m.BrandId);
                var p = m.ToModel<ProductModel>();

                p.ProductId = m.Id;
                p.CategoryName = cat != null ? cat.Name : "";
                p.BrandName = brand != null ? brand.Name : "";
                
                //这里替换成高级用法
                p = m.InitBaseModel(p, wareHouseId, allOptions, allProductPrices, allProductTierPrices, _productService);

                //获取单位  2021-11-4 mu 添加//////////////////////////////////////////////////////////////
                var smallProductPrices = allProductPrices.Where(w => w.ProductId == p.Id && w.UnitId == p.SmallUnitId).FirstOrDefault();
                var strokeProductPrices = allProductPrices.Where(w => w.ProductId == p.Id && w.UnitId == (p.StrokeUnitId ?? 0)).FirstOrDefault();
                var bigProductPrices = allProductPrices.Where(w => w.ProductId == p.Id && w.UnitId == (p.BigUnitId ?? 0)).FirstOrDefault();

                p.SmallProductPrices = smallProductPrices != null ? smallProductPrices.ToModel<ProductPriceModel>() : new ProductPriceModel();
                p.StrokeProductPrices = strokeProductPrices != null ? strokeProductPrices.ToModel<ProductPriceModel>() : new ProductPriceModel();
                p.BigProductPrices = bigProductPrices != null ? bigProductPrices.ToModel<ProductPriceModel>() : new ProductPriceModel();
                //////////////////////////////////////////////////////////////////////////////////////////
                var currentQuantity = _stockService.GetProductCurrentQuantity(store ?? 0, p.ProductId, wareHouseId);
                var usableQuantity = _stockService.GetProductUsableQuantity(store ?? 0, p.ProductId, wareHouseId);

                //现货库存数量
                p.CurrentQuantity = currentQuantity;
                if (p.CurrentQuantity > 0)
                {
                    p.CurrentQuantityConversion = m.GetConversionFormat(allOptions, m.SmallUnitId, p.CurrentQuantity ?? 0);
                }
                else
                {
                    p.CurrentQuantity = _stockService.GetProductCurrentQuantity(store ?? 0, m.Id, wareHouseId);
                    if (p.CurrentQuantity > 0)
                    {
                        p.CurrentQuantityConversion = m.GetConversionFormat(allOptions, m.SmallUnitId, p.CurrentQuantity ?? 0);
                    }
                }

                //可用库存数量
                p.UsableQuantity = usableQuantity;
                if (p.UsableQuantity > 0)
                {
                    p.UsableQuantityConversion = m.GetConversionFormat(allOptions, m.SmallUnitId, p.UsableQuantity ?? 0);
                }
                else
                {
                    p.UsableQuantity = _stockService.GetProductUsableQuantity(store ?? 0, m.Id, wareHouseId);
                    if (p.UsableQuantity > 0)
                    {
                        p.UsableQuantityConversion = m.GetConversionFormat(allOptions, m.SmallUnitId, p.UsableQuantity ?? 0);
                    }
                }

                //成本价
                p.CostPrices = _purchaseBillService.GetReferenceCostPrice(m);

                return p;
            }
            else
            {
                return new ProductModel();
            }
        }



        /// <summary>
        /// 获取经销商商品信息
        /// </summary>
        /// <param name="store"></param>
        /// <param name="productIds"></param>
        /// <returns></returns>
        [HttpGet("product/getProductByIds/{store}/{wareHouseId}")]
        [SwaggerOperation("getProductByIds")]
        public async Task<APIResult<IList<ProductModel>>> GetProductByIds(int? store, int wareHouseId,[FromQuery] int[] productIds)
        {
            if (!store.HasValue || store.Value == 0)
                return this.Error<ProductModel>(Resources.ParameterError);

            return await Task.Run(() =>
            {
                try
                {
                    var ps = new List<ProductModel>();
                    if (productIds != null && productIds.Any())
                    {
                        foreach (var p in productIds)
                        {
                            var product = ParseProduct(store, wareHouseId, p);
                            ps.Add(product);
                        }
                    }
                    return this.Successful2("", ps);
                }
                catch (Exception ex)
                {
                    return this.Error2<ProductModel>(ex.Message);

                }
            });
        }
        /// <summary>
        /// 创建经销商商品档案
        /// </summary>
        /// <param name="model">商品数据</param>
        /// <param name="store">经销商</param>
        /// <returns></returns>
        [HttpPost("product/createOrUpdateProduct/{store}/{userId}")]
        [SwaggerOperation("createOrUpdateProduct")]
        //[ValidateActionParameters]
        //[AuthBaseFilter]
        public async Task<APIResult<dynamic>> CreateOrUpdateProduct(ProductModel model, int? store, int? userId)
        {
            if (!store.HasValue || store.Value == 0)
                return this.Error("参数错误！");

            return await Task.Run(async () =>
            {

                var user = _userService.GetUserById(store ?? 0, userId ?? 0);

                try
                {

                    var companySetting = _settingService.LoadSetting<CompanySetting>(store ?? 0);
                    var products = _productService.GetAllProducts(store ?? 0);

                    if (string.IsNullOrEmpty(model.Name))
                    {
                        return this.Error("商品名称不能为空");

                    }

                    if (string.IsNullOrEmpty(model.MnemonicCode))
                    {
                        return this.Error("商品助记码不能为空");

                    }

                    if (model.SmallUnitId == 0)
                    {
                        return this.Error("请选择商品最小单位");

                    }

                    Product product = model.ToEntity<Product>();
                    #region 基本信息
                    //添加商品
                    try
                    {
                        //商品
                        product.StoreId = store ?? 0;
                        product.MnemonicCode = CommonHelper.GenerateStrchar(8) + "_" + model.MnemonicCode;
                        product.BigQuantity = model.BigQuantity ?? 0;
                        product.StrokeQuantity = model.StrokeQuantity ?? 0;
                        product.UpdatedOnUtc = DateTime.Now;
                        product.CreatedOnUtc = DateTime.Now;
                        product.Deleted = false;
                        product.Published = true;
                        product.DisplayOrder = model.DisplayOrder;
                        product.Name = model.Name;
                        product.CategoryId = model.CategoryId;
                        product.BrandId = model.BrandId;
                        //没有开单，可修改商品单位
                        if (!product.HasSold)
                        {
                            product.SmallUnitId = model.SmallUnitId;
                            product.StrokeUnitId = model.StrokeUnitId;
                            product.BigUnitId = model.BigUnitId;
                        }
                        product.IsAdjustPrice = model.IsAdjustPrice;
                        product.Status = model.Status;
                        //是否启用生产日期
                        product.IsManufactureDete = model.IsManufactureDete;

                        //其他信息
                        product.Sku = model.Sku;
                        product.Specification = model.Specification;
                        //product.Number = model.Number;
                        product.CountryOrigin = model.CountryOrigin;
                        product.Supplier = model.Supplier;
                        product.OtherBarCode = model.OtherBarCode;
                        product.OtherBarCode1 = model.OtherBarCode1;
                        product.OtherBarCode2 = model.OtherBarCode2;
                        product.StatisticalType = model.StatisticalType;
                        product.ExpirationDays = model.ExpirationDays;
                        product.AdventDays = model.AdventDays;
                        product.IsFlavor = model.IsFlavor;
                        product.SmallBarCode = model.SmallBarCode;
                        product.StrokeBarCode = model.StrokeBarCode;
                        product.BigBarCode = model.BigBarCode;
                        product.ERPProductId = 0;
                    }
                    catch (Exception ex)
                    {
                        return this.Error(ex.Message);
                    }

                    #endregion

                    #region 商品价格
                    //小单位
                    model.SmallProductPrices.UnitId = int.Parse(model.UnitPriceDicts["Small_UnitId"] == null ? "0" : model.UnitPriceDicts["Small_UnitId"]);
                    model.SmallProductPrices.TradePrice = decimal.Parse(model.UnitPriceDicts["Small_TradePrice"] == null ? "0" : model.UnitPriceDicts["Small_TradePrice"]);
                    model.SmallProductPrices.RetailPrice = decimal.Parse(model.UnitPriceDicts["Small_RetailPrice"] == null ? "0" : model.UnitPriceDicts["Small_RetailPrice"]);
                    model.SmallProductPrices.FloorPrice = decimal.Parse(model.UnitPriceDicts["Small_FloorPrice"] == null ? "0" : model.UnitPriceDicts["Small_FloorPrice"]);
                    model.SmallProductPrices.PurchasePrice = decimal.Parse(model.UnitPriceDicts["Small_PurchasePrice"] == null ? "0" : model.UnitPriceDicts["Small_PurchasePrice"]);
                    model.SmallProductPrices.CostPrice = decimal.Parse(model.UnitPriceDicts["Small_CostPrice"] == null ? "0" : model.UnitPriceDicts["Small_CostPrice"]);
                    model.SmallProductPrices.SALE1 = decimal.Parse(model.UnitPriceDicts["Small_SALE1"] == null ? "0" : model.UnitPriceDicts["Small_SALE1"]);
                    model.SmallProductPrices.SALE2 = decimal.Parse(model.UnitPriceDicts["Small_SALE2"] == null ? "0" : model.UnitPriceDicts["Small_SALE2"]);
                    model.SmallProductPrices.SALE3 = decimal.Parse(model.UnitPriceDicts["Small_SALE3"] == null ? "0" : model.UnitPriceDicts["Small_SALE3"]);
                    //中单位
                    model.StrokeProductPrices.UnitId = int.Parse(model.UnitPriceDicts["Stroke_UnitId"] == null ? "0" : model.UnitPriceDicts["Stroke_UnitId"]);
                    model.StrokeProductPrices.TradePrice = decimal.Parse(model.UnitPriceDicts["Stroke_TradePrice"] == null ? "0" : model.UnitPriceDicts["Stroke_TradePrice"]);
                    model.StrokeProductPrices.RetailPrice = decimal.Parse(model.UnitPriceDicts["Stroke_RetailPrice"] == null ? "0" : model.UnitPriceDicts["Stroke_RetailPrice"]);
                    model.StrokeProductPrices.FloorPrice = decimal.Parse(model.UnitPriceDicts["Stroke_FloorPrice"] == null ? "0" : model.UnitPriceDicts["Stroke_FloorPrice"]);
                    model.StrokeProductPrices.PurchasePrice = decimal.Parse(model.UnitPriceDicts["Stroke_PurchasePrice"] == null ? "0" : model.UnitPriceDicts["Stroke_PurchasePrice"]);
                    model.StrokeProductPrices.CostPrice = decimal.Parse(model.UnitPriceDicts["Stroke_CostPrice"] == null ? "0" : model.UnitPriceDicts["Stroke_CostPrice"]);
                    model.SmallProductPrices.SALE1 = decimal.Parse(model.UnitPriceDicts["Stroke_SALE1"] == null ? "0" : model.UnitPriceDicts["Stroke_SALE1"]);
                    model.StrokeProductPrices.SALE2 = decimal.Parse(model.UnitPriceDicts["Stroke_SALE2"] == null ? "0" : model.UnitPriceDicts["Stroke_SALE2"]);
                    model.StrokeProductPrices.SALE3 = decimal.Parse(model.UnitPriceDicts["Stroke_SALE3"] == null ? "0" : model.UnitPriceDicts["Stroke_SALE3"]);
                    //大单位
                    model.BigProductPrices.UnitId = int.Parse(model.UnitPriceDicts["Big_UnitId"] == null ? "0" : model.UnitPriceDicts["Big_UnitId"]);
                    model.BigProductPrices.TradePrice = decimal.Parse(model.UnitPriceDicts["Big_TradePrice"] == null ? "0" : model.UnitPriceDicts["Big_TradePrice"]);
                    model.BigProductPrices.RetailPrice = decimal.Parse(model.UnitPriceDicts["Big_RetailPrice"] == null ? "0" : model.UnitPriceDicts["Big_RetailPrice"]);
                    model.BigProductPrices.FloorPrice = decimal.Parse(model.UnitPriceDicts["Big_FloorPrice"] == null ? "0" : model.UnitPriceDicts["Big_FloorPrice"]);
                    model.BigProductPrices.PurchasePrice = decimal.Parse(model.UnitPriceDicts["Big_PurchasePrice"] == null ? "0" : model.UnitPriceDicts["Big_PurchasePrice"]);
                    model.BigProductPrices.CostPrice = decimal.Parse(model.UnitPriceDicts["Big_CostPrice"] == null ? "0" : model.UnitPriceDicts["Big_CostPrice"]);
                    model.SmallProductPrices.SALE1 = decimal.Parse(model.UnitPriceDicts["Big_SALE1"] == null ? "0" : model.UnitPriceDicts["Big_SALE1"]);
                    model.BigProductPrices.SALE2 = decimal.Parse(model.UnitPriceDicts["Big_SALE2"] == null ? "0" : model.UnitPriceDicts["Big_SALE2"]);
                    model.BigProductPrices.SALE3 = decimal.Parse(model.UnitPriceDicts["Big_SALE3"] == null ? "0" : model.UnitPriceDicts["Big_SALE3"]);

                    var productPrices = new List<ProductPrice>();
                    try
                    {
                        var spp = model.SmallProductPrices.ToEntity<ProductPrice>();
                        spp.ProductId = product.Id;
                        if (spp.UnitId != 0)
                        {
                            productPrices.Add(spp);
                        }
                        var tpp = model.StrokeProductPrices.ToEntity<ProductPrice>();
                        tpp.ProductId = product.Id;
                        if (tpp.UnitId != 0)
                        {
                            productPrices.Add(tpp);
                        }
                        var bpp = model.BigProductPrices.ToEntity<ProductPrice>();
                        bpp.ProductId = product.Id;
                        if (bpp.UnitId != 0)
                        {
                            productPrices.Add(bpp);
                        }
                    }
                    catch (Exception ex)
                    {
                        return this.Error(ex.Message);
                    }
                    #endregion

                    #region  规格属性

                    var productSpecificationAttributes = new List<ProductSpecificationAttribute>();
                    try
                    {
                        if (product.SmallUnitId > 0)
                        {
                            var smallPSpec = new ProductSpecificationAttribute()
                            {
                                StoreId = store ?? 0,
                                ProductId = product.Id,
                                SpecificationAttributeOptionId = product.SmallUnitId,
                                CustomValue = "小",
                                AllowFiltering = true,
                                ShowOnProductPage = true,
                                DisplayOrder = 0
                            };
                            productSpecificationAttributes.Add(smallPSpec);
                        }

                        if (product.StrokeUnitId.HasValue && product.StrokeUnitId.Value > 0)
                        {
                            var strokePSpec = new ProductSpecificationAttribute()
                            {
                                StoreId = store ?? 0,
                                ProductId = product.Id,
                                SpecificationAttributeOptionId = product.StrokeUnitId ?? _specificationAttributeService.GetSpecificationAttributeOptionsByStore(store).FirstOrDefault().Id,
                                CustomValue = "中",
                                AllowFiltering = true,
                                ShowOnProductPage = true,
                                DisplayOrder = 0
                            };
                            productSpecificationAttributes.Add(strokePSpec);
                        }

                        if (product.BigUnitId.HasValue && product.BigUnitId.Value > 0)
                        {
                            var bigSpec = new ProductSpecificationAttribute()
                            {
                                StoreId = store ?? 0,
                                ProductId = product.Id,
                                SpecificationAttributeOptionId = product.BigUnitId ?? _specificationAttributeService.GetSpecificationAttributeOptionsByStore(store).FirstOrDefault().Id,
                                CustomValue = "大",
                                AllowFiltering = true,
                                ShowOnProductPage = true,
                                DisplayOrder = 0
                            };
                            productSpecificationAttributes.Add(bigSpec);
                        }

                    }
                    catch (Exception ex)
                    {
                        return this.Error(ex.Message);
                    }

                    #endregion

                    #region 组合商品
                    var combination = new Combination();
                    try
                    {

                        combination.DisplayOrder = 0;
                        combination.ProductId = product.Id;
                        combination.ProductName = product.Name;
                        combination.StoreId = store ?? 0;
                        combination.Enabled = true;
                    }
                    catch (Exception ex)
                    {
                        return this.Error(ex.Message);
                    }
                    #endregion


                    //RedLock
                    string lockKey = string.Format(DCMSCachingDefaults.RedisDataReSubmitKey, Request.GetUrl(), store, userId, CommonHelper.MD5(JsonConvert.SerializeObject(model)));
                    var result = await _locker.PerformActionWithLockAsync(lockKey,
                        TimeSpan.FromSeconds(30),
                        TimeSpan.FromSeconds(10),
                        TimeSpan.FromSeconds(1),
                        () => _productService.ProductCreateOrUpdate(store ?? 0, userId ?? 0, model.ProductId, product, productSpecificationAttributes, productPrices, null, null, combination, null));


                    return this.Successful("", result);

                }
                catch (Exception ex)
                {
                    return this.Error(ex.Message);
                }
            });
        }

        /// <summary>
        /// 获取调拨商品
        /// </summary>
        /// <param name="store">经销商</param>
        /// <param name="type">
        /// 0: 获取仓库今天销售的商品,
        /// 1：获取仓库昨天的销售商品,
        /// 2：获取仓库近2天的销售商品,
        /// 3：获取仓库上次调拨的销售商品,
        /// 4：获取仓库今天退货的商品,
        /// 5：获取仓库昨天退货的商品,
        /// 6：获取仓库前天退货的商品,
        /// 7: 获取库存所在指定类别的商品</param>
        /// <param name="categoryIds">商品类别</param>
        /// <param name="wareHouseId">仓库</param>
        /// <returns></returns>
        [HttpGet("product/getAllocationProducts/{store}")]
        [SwaggerOperation("getAllocationProducts")]
        //[ValidateActionParameters]
        //[AuthBaseFilter]
        public async Task<APIResult<IList<ProductModel>>> GetAllocationProducts(int? store, int? type, [FromQuery] int[] categoryIds, int wareHouseId = 0)
        {
            if (!store.HasValue || store.Value == 0)
                return this.Error<ProductModel>(Resources.ParameterError);

            return await Task.Run(() =>
            {
                try
                {
                    if (!type.HasValue)
                        return this.Error<ProductModel>(Resources.ParameterError);

                    var products = new List<ProductModel>();
                    if (type == 0 || type == 1 || type == 2 || type == 3 || type == 4 || type == 5 || type == 6 || type == 7)
                    {
                        var gridModel = _productService.GetAllocationProducts(store: store ?? 0, type: type ?? 0, categoryIds: categoryIds, wareHouseId: wareHouseId);
                        if (gridModel != null && gridModel.Count > 0)
                        {
                            var allCatagories = _categoryService.GetCategoriesByCategoryIds(store, gridModel.Select(p => p.CategoryId).Distinct().ToArray());
                            var allBrands = _brandService.GetBrandsByBrandIds(store, gridModel.Select(p => p.BrandId).Distinct().ToArray());
                            var allOptions = _specificationAttributeService.GetSpecificationAttributeOptionByIds(store, gridModel.GetProductBigStrokeSmallUnitIds());
                            var allProductTierPrices = _productService.GetProductTierPriceByProductIds(store, gridModel.Select(p => p.Id).Distinct().ToArray());
                            var allProductPrices = _productService.GetProductPricesByProductIds(store, gridModel.Select(gm => gm.Id).Distinct().ToArray());

                            products = gridModel.Select(m =>
                            {
                                var cat = allCatagories.Where(ca => ca.Id == m.CategoryId).FirstOrDefault();
                                var brand = allBrands.Where(br => br.Id == m.BrandId).FirstOrDefault();
                                var p = m.ToModel<ProductModel>();
                                //这里替换成高级用法
                                p = m.InitBaseModel<ProductModel>(p, wareHouseId, allOptions, allProductPrices, allProductTierPrices, _productService);

                                p.ProductId = m.Id;
                                p.CategoryName = cat != null ? cat.Name : "";
                                p.BrandName = brand != null ? brand.Name : "";

                                return p;
                            }).ToList();
                        }
                    }

                    return this.Successful<ProductModel>("", products);
                }
                catch (Exception ex)
                {
                    return this.Error2<ProductModel>(ex.Message);
                }
            });
        }


    }
}
