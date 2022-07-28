using DCMS.Core;
using DCMS.Core.Caching;
using DCMS.Core.Domain.Configuration;
using DCMS.Core.Domain.Products;
using DCMS.Services.Configuration;
using DCMS.Services.ExportImport;
using DCMS.Services.Logging;
using DCMS.Services.Messages;
using DCMS.Services.Products;
using DCMS.Services.Purchases;
using DCMS.Services.WareHouses;
using DCMS.ViewModel.Models.Products;
using DCMS.Web.Framework.Extensions;
using DCMS.Web.Framework.Mvc.Filters;
using DCMS.Web.Infrastructure.Mapper.Extensions;
using DCMS.Web.Models;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;
using NUglify.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Task = System.Threading.Tasks.Task;

namespace DCMS.Web.Controllers
{

    /// <summary>
    /// 用于商品信息管理
    /// </summary>
    public class ProductController : BasePublicController
    {
        private readonly ICategoryService _categoryService;
        private readonly IProductService _productService;
        private readonly IUserActivityService _userActivityService;
        private readonly ISpecificationAttributeService _specificationAttributeService;
        private readonly ISettingService _settingService;
        private readonly IBrandService _brandService;
        private readonly IStatisticalTypeService _statisticalTypeService;
        private readonly IProductFlavorService _productFlavorService;
        private readonly IPurchaseBillService _purchaseBillService;
        private readonly IManufacturerService _manufacturerService;
        private readonly IStaticCacheManager _cacheManager;
        private readonly IStockService _stockService;
        private readonly IImportManager _importManager;
        private readonly IExportManager _exportManager;
        private readonly IRedLocker _locker;

        public ProductController(ICategoryService categoryService,
            IProductService productService,
            IUserActivityService userActivityService,
            IWorkContext workContext,
            ISettingService settingService,
            IBrandService brandService,
            IStaticCacheManager cacheManager,
            ISpecificationAttributeService specificationAttributeService,
            IStatisticalTypeService statisticalTypeService,
            IManufacturerService manufacturerService,
            IProductFlavorService productFlavorService,
            IPurchaseBillService purchaseBillService,
            IStoreContext storeContext,
            IStockService stockService,
            ILogger loggerService,
            IImportManager importManager,
            IExportManager exportManager,
            INotificationService notificationService,
           
            IRedLocker locker) : base(workContext, loggerService, storeContext, notificationService)
        {
            _categoryService = categoryService;
            _userActivityService = userActivityService;
            _productService = productService;
            _settingService = settingService;
            _brandService = brandService;
            _cacheManager = cacheManager;
            _specificationAttributeService = specificationAttributeService;
            _statisticalTypeService = statisticalTypeService;
            _productFlavorService = productFlavorService;
            _purchaseBillService = purchaseBillService;
            _manufacturerService = manufacturerService;
            _stockService = stockService;
            _importManager = importManager;
            _exportManager = exportManager;
            _locker = locker;
        }


        public IActionResult Index()
        {
            return RedirectToAction("List");
        }


        /// <summary>
        /// 商品列表
        /// </summary>
        /// <param name="pagenumber"></param>
        /// <returns></returns>
        [AuthCode((int)AccessGranularityEnum.ProductArchivesView)]
        public IActionResult List()
        {
            var model = new ProductListModel();
            return View(model);
        }


        /// <summary>
        /// 异步获取商品列表
        /// </summary>
        /// <param name="storeId"></param>
        /// <param name="excludeIds"></param>
        /// <param name="key"></param>
        /// <param name="categoryId"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="wareHouseId"></param>
        /// <param name="stockQtyMoreThan"></param>
        /// <param name="includeProductDetail"></param>
        /// <param name="terminalId"></param>
        /// <returns></returns>
        public async Task<JsonResult> AsyncList(int? storeId, int[] excludeIds, string key = "", int categoryId = 0, int pageIndex = 0, int pageSize = 10, int wareHouseId = 0, bool stockQtyMoreThan = true, int includeProductDetail = 1, int terminalId = 0, int productStatus = 2)
        {
            if (!storeId.HasValue)
            {
                storeId = curStore?.Id;
            }

            //if (key != "" && categoryId > 0)
            //{
            //    key = "";
            //}

            if (_categoryService.GetCategoriesName(curStore.Id, categoryId) == "全部")
            {
                categoryId = 0;
            }

            return await Task.Run(() =>
            {
                int totalCount = 0;
                var gridModel = _productService.SearchProducts(curStore?.Id ?? 0,
                              excludeIds,
                              categoryId == 0 ? new int[] { } : new int[] { categoryId },
                              stockQtyMoreThan,
                              null,
                              null,
                              key: key,
                              includeSpecificationAttributes: true,
                              includeVariantAttributes: false,
                              includeVariantAttributeCombinations: false,
                              includePictures: false,
                              includeTierPrices: true,
                              productStatus: productStatus,
                              pageIndex: pageIndex, pageSize: pageSize);


                var allOptions = new List<SpecificationAttributeOption>();
                var allProductPrices = new List<ProductPrice>();
                var allProductTierPrices = new List<ProductTierPrice>();
                var plans = _productService.GetAllPlansByStore(curStore?.Id ?? 0);
                var allCategories = _categoryService.GetAllCategoriesByIds(curStore.Id, gridModel.Select(c => c.CategoryId).Distinct().ToArray());

                foreach (var p in gridModel)
                {
                    foreach (var sap in p.ProductSpecificationAttributes)
                    {
                        if (sap.SpecificationAttributeOption != null)
                        {
                            allOptions.Add(sap.SpecificationAttributeOption);
                        }
                    }

                    foreach (var pp in p.ProductPrices)
                    {
                        if (pp != null)
                        {
                            allProductPrices.Add(pp);
                        }
                    }

                    foreach (var ptp in p.ProductTierPrices)
                    {
                        if (ptp != null)
                        {
                            allProductTierPrices.Add(ptp);
                        }
                    }

                }

                //
                totalCount = gridModel.TotalCount;

                var products = gridModel.Select(m =>
                {

                    var p = m.ToModel<ProductModel>();
                    var cat = allCategories.FirstOrDefault(ca => ca.Id == m.CategoryId); //m.ProductCategories.FirstOrDefault(ca => ca.Id == m.CategoryId); //m.ProductCategories始终不会有数据

                    p.ProductId = m.Id;
                    p.CategoryName = cat != null ? cat.Name : "";
                    p.BrandName = m.Brand?.Name;

                    //这里替换成高级用法
                    //1.查询商品详情
                    if (includeProductDetail == 1)
                    {
                        p = m.InitBaseModel<ProductModel>(p, 
                            wareHouseId, 
                            allOptions, 
                            allProductPrices, 
                            allProductTierPrices,
                            _productService, 
                            terminalId);
                        p.ProductTimes = _productService.GetProductDates(curStore.Id, p.Id, wareHouseId);
                    }
                    //2.仅查询单位，和库存的详情
                    else
                    {
                        p = m.InitBaseModel<ProductModel>(p, wareHouseId, allOptions, allProductPrices, allProductTierPrices,_productService, terminalId);

                        p.ProductTimes = _productService.GetProductDates(curStore.Id, p.Id, wareHouseId);

                        //增加界面显示的数量，单位
                        if (m.BigUnitId != null && m.BigUnitId > 0)
                        {
                            p.UnitName = allOptions.Where(al => al.Id == m.BigUnitId).Select(al => al.Name).FirstOrDefault();
                        }
                        else if (m.StrokeUnitId != null && m.StrokeUnitId > 0)
                        {
                            p.UnitName = allOptions.Where(al => al.Id == m.StrokeUnitId).Select(al => al.Name).FirstOrDefault();
                        }
                        else
                        {
                            p.UnitName = allOptions.Where(al => al.Id == m.SmallUnitId).Select(al => al.Name).FirstOrDefault();
                        }

                        //现货库存数量
                        p.CurrentQuantity = m.GetProductCurrentQuantity(wareHouseId);
                        if (p.CurrentQuantity > 0)
                        {
                            p.CurrentQuantityConversion = m.GetConversionFormat(allOptions, m.SmallUnitId, p.CurrentQuantity ?? 0);
                        }
                        else
                        {
                            p.CurrentQuantity = _stockService.GetProductCurrentQuantity(curStore.Id, m.Id, wareHouseId);
                            if (p.CurrentQuantity > 0)
                            {
                                p.CurrentQuantityConversion = m.GetConversionFormat(allOptions, m.SmallUnitId, p.CurrentQuantity ?? 0);
                            }
                        }

                        //可用库存数量
                        p.UsableQuantity = m.GetProductUsableQuantity(wareHouseId);
                        if (p.UsableQuantity > 0)
                        {
                            p.UsableQuantityConversion = m.GetConversionFormat(allOptions, m.SmallUnitId, p.UsableQuantity ?? 0);
                        }
                        else
                        {
                            p.UsableQuantity = _stockService.GetProductUsableQuantity(curStore.Id, m.Id, wareHouseId);
                            if (p.UsableQuantity > 0)
                            {
                                p.UsableQuantityConversion = m.GetConversionFormat(allOptions, m.SmallUnitId, p.UsableQuantity ?? 0);
                            }
                        }

                    }
                    //成本价
                    p.CostPrices = _purchaseBillService.GetReferenceCostPrice(m);

                    return p;
                }).ToList();

                return Json(new
                {
                    Success = true,
                    Total = totalCount,
                    Rows = products
                });
            });
        }

        /// <summary>
        /// 获取商品详情
        /// </summary>
        /// <param name="productId">商品Id</param>
        /// <param name="wareHouseId">仓库Id</param>
        /// <returns></returns>
        public async Task<JsonResult> GetProductModel(int productId = 0, int wareHouseId = 0, int terminalId = 0)
        {

            return await Task.Run(() =>
            {
                Product product = _productService.GetProductById(curStore.Id, productId);
                if (product != null)
                {
                    ProductModel p = product.ToModel<ProductModel>();
                    p.CategoryName = _categoryService.GetCategoryName(curStore.Id, product.CategoryId);
                    p.BrandName = _brandService.GetBrandName(curStore.Id, product.BrandId);
                    var allOptions = _specificationAttributeService.GetSpecificationAttributeOptionByIds(curStore.Id, product.GetProductBigStrokeSmallUnitIds());
                    var allProductPrices = _productService.GetProductPricesByProductIds(curStore.Id, new int[] { product.Id });
                    var allProductTierPrices = _productService.GetProductTierPriceByProductIds(curStore.Id, new int[] { product.Id });

                    //这里替换成高级用法
                    p = product.InitBaseModel<ProductModel>(p, wareHouseId, allOptions, allProductPrices, allProductTierPrices, _productService, terminalId);
                    p.ProductTimes = _productService.GetProductDates(curStore.Id, p.Id, wareHouseId);

                    //成本价
                    p.CostPrices = _purchaseBillService.GetReferenceCostPrice(product);

                    return Json(new { Flag = true, Row = p });
                }
                else
                {
                    return Json(new { Flag = false });
                }
            });
        }


        /// <summary>
        /// 异步搜索商品
        /// </summary>
        /// <param name="productId">商品Id</param>
        /// <param name="targetDoms">目标节点</param>
        /// <param name="index">如果在表格行内选择搜索商品，则需要获取当前行索引值</param>
        /// <param name="target"></param>
        /// <param name="wareHouseId">仓库Id</param>
        /// <param name="stockQtyMoreThan">库存数量大于</param>
        /// <param name="includeProductDetail">是否包含商品详情</param>
        /// <param name="terminalId">用户选择终端Id</param>
        /// <returns></returns>
        public JsonResult AsyncSearchSelectPopup(int? productId, string[] targetDoms = null, int index = 0, string target = "", int wareHouseId = 0, bool stockQtyMoreThan = false, bool includeProductDetail = true, int terminalId = 0, string targetForm = "", int? billType = 0, bool singleSelect = false)
        {
            var model = new ProductListModel();

            if (productId.HasValue)
            {
                model.ExcludeIds.Add(productId.Value);
            }
            else
            {
                model.ExcludeIds.Add(0);
            }

            model.TargetForm = targetForm;
            if (targetDoms != null)
            {
                model.TargetDoms = targetDoms.ToList();
            }

            //需要获取当前行索引值
            model.RowIndex = index;
            model.Target = target;

            //库存过滤
            model.WareHouseId = wareHouseId;
            model.StockQtyMoreThan = stockQtyMoreThan;
            model.IncludeProductDetail = includeProductDetail;
            model.SingleSelect = singleSelect;

            //终端条件
            model.TerminalId = terminalId;

            model.BillType = billType;

            return Json(new
            {
                Success = true,
                RenderHtml = RenderPartialViewToString("AsyncSearch", model)
            });
        }


        /// <summary>
        /// 添加商品
        /// </summary>
        /// <returns></returns>
        [AuthCode((int)AccessGranularityEnum.ProductArchivesSave)]
        public IActionResult Create()
        {

            var model = new ProductModel();

            //获取配置
            var companySetting = _settingService.LoadSetting<CompanySetting>(curStore?.Id ?? 0);
            var productSetting = _settingService.LoadSetting<ProductSetting>(curStore?.Id ?? 0);

            var smalllists = new List<SelectListItem> { new SelectListItem() { Text = "-无-", Value = "0" } };
            var stroklists = new List<SelectListItem> { new SelectListItem() { Text = "-无-", Value = "0" } };
            var biglists = new List<SelectListItem> { new SelectListItem() { Text = "-无-", Value = "0" } };

            //分类
            model = BindCategoryDropDownList<ProductModel>(model, new Func<int?, int, bool, IList<Category>>(_categoryService.GetAllCategoriesByParentCategoryId), curStore?.Id ?? 0, 0);


            //规格属性
            var smallOptions = _specificationAttributeService.GetSpecificationAttributeOptionsBySpecificationAttribute(curStore?.Id ?? 0, productSetting.SmallUnitSpecificationAttributeOptionsMapping).ToList();
            var strokOptions = _specificationAttributeService.GetSpecificationAttributeOptionsBySpecificationAttribute(curStore?.Id ?? 0, productSetting.StrokeUnitSpecificationAttributeOptionsMapping).ToList();
            var bigOptions = _specificationAttributeService.GetSpecificationAttributeOptionsBySpecificationAttribute(curStore?.Id ?? 0, productSetting.BigUnitSpecificationAttributeOptionsMapping).ToList();

            smallOptions.ForEach(o =>
            {
                smalllists.Add(new SelectListItem() { Text = o.Name, Value = o.Id.ToString() });
            });
            strokOptions.ForEach(o =>
            {
                stroklists.Add(new SelectListItem() { Text = o.Name, Value = o.Id.ToString() });
            });
            bigOptions.ForEach(o =>
            {
                biglists.Add(new SelectListItem() { Text = o.Name, Value = o.Id.ToString() });
            });

            model.SmallUnits = new SelectList(smalllists, "Value", "Text");
            model.StrokeUnits = new SelectList(stroklists, "Value", "Text");
            model.BigUnits = new SelectList(biglists, "Value", "Text");
            model.SmallProductPrices = new ProductPriceModel();
            model.StrokeProductPrices = new ProductPriceModel();
            model.BigProductPrices = new ProductPriceModel();
            model.MnemonicCode = CommonHelper.Str_char(6, true);
            model.Status = true;
            //是否显示生产日期设置列
            model.IsShowCreateDate = companySetting.OpenBillMakeDate == 2;

            return View(model);
        }


        /// <summary>
        /// 添加商品保存
        /// </summary>
        /// <param name="model"></param>
        /// <param name="continueEditing"> save-newcreate </param>
        /// <returns></returns>
        [HttpPost, ParameterBasedOnFormNameAttribute("save-continue", "continueEditing")]
        [HttpPost, ParameterBasedOnFormNameAttribute("save-newcreate", "newCreate")]
        [AuthCode((int)AccessGranularityEnum.ProductArchivesSave)]
        public async Task<IActionResult> Create(ProductModel model, IFormCollection form, bool continueEditing, bool newCreate)
        {
            //获取配置
            var companySetting = _settingService.LoadSetting<CompanySetting>(curStore?.Id ?? 0);
            var products = _productService.GetAllProducts(curStore?.Id ?? 0);

            if (string.IsNullOrEmpty(model.Name))
            {
                ModelState.AddModelError("", "商品名称不能为空");
            }

            if (string.IsNullOrEmpty(model.MnemonicCode))
            {
                ModelState.AddModelError("", "商品助记码不能为空");
            }
            if (model.MnemonicCode.Length > 100)
            {
                ModelState.AddModelError("", "商品助记码最大长度为100");
            }

            if (model.SmallUnitId == 0)
            {
                ModelState.AddModelError("", "请选择商品最小单位");
            }

            if (model.CategoryId == 0)
            {
                ModelState.AddModelError("", "请选择商品类别");
            }

            if (model.BrandId == 0)
            {
                ModelState.AddModelError("", "请选择品牌");
            }

            if (model.BigUnitId > 0 && (model.BigQuantity ?? 0) <= 0)
            {
                ModelState.AddModelError("", "请输入大单位转换数量");
            }
            if (model.StrokeUnitId > 0 && (model.StrokeQuantity ?? 0) <= 0)
            {
                ModelState.AddModelError("", "请输入中单位转换数量");
            }

            if (ModelState.IsValid)
            {
                var product = model.ToEntity<Product>();

                #region 基本信息
                //添加商品
                try
                {
                    //商品
                    product.StoreId = curStore?.Id ?? 0;
                    //product.MnemonicCode = CommonHelper.GenerateStrchar(8) + "_" + model.MnemonicCode;
                    product.MnemonicCode = model.MnemonicCode;
                    product.BigQuantity = model.BigQuantity ?? 0;
                    product.StrokeQuantity = model.StrokeQuantity ?? 0;
                    product.UpdatedOnUtc = DateTime.Now;
                    product.CreatedOnUtc = DateTime.Now;
                    product.Deleted = false;
                    product.Published = true;
                    product.DisplayOrder = model.DisplayOrder;
                    product.Name = model.Name;
                    product.MnemonicCode = model.MnemonicCode;
                    product.CategoryId = model.CategoryId;
                    product.BrandId = model.BrandId;
                    product.SmallUnitId = model.SmallUnitId;
                    product.StrokeUnitId = model.StrokeUnitId;
                    product.BigUnitId = model.BigUnitId;
                    product.IsAdjustPrice = model.IsAdjustPrice;
                    product.Status = model.Status;
                    //是否启用生产日期
                    product.IsManufactureDete = model.IsManufactureDete;

                    //其他信息
                    product.Sku = model.Sku;
                    product.ProductCode = model.Sku;
                    product.Specification = model.Specification;
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
                    _userActivityService.InsertActivity("InsertProduct", ex.Message, curUser, model.Name);
                }

                #endregion

                #region 商品价格
                List<ProductPrice> productPrices = new List<ProductPrice>();

                try
                {
                    //添加商品价格
                    var names = new string[] { "Small", "Stroke", "Big" };
                    foreach (var name in names)
                    {


                        var _unitId = string.IsNullOrEmpty(form[name + "_UnitId"]) ? "0" : form[name + "_UnitId"].ToString();
                        var _tradePrice = string.IsNullOrEmpty(form[name + "_TradePrice"]) ? "0" : form[name + "_TradePrice"].ToString();
                        var _retailPrice = string.IsNullOrEmpty(form[name + "_RetailPrice"]) ? "0" : form[name + "_RetailPrice"].ToString();
                        var _floorPrice = string.IsNullOrEmpty(form[name + "_FloorPrice"]) ? "0" : form[name + "_FloorPrice"].ToString();
                        var _purchasePrice = string.IsNullOrEmpty(form[name + "_PurchasePrice"]) ? "0" : form[name + "_PurchasePrice"].ToString();
                        var _costPrice = string.IsNullOrEmpty(form[name + "_CostPrice"]) ? "0" : form[name + "_CostPrice"].ToString();
                        var _sALE1 = string.IsNullOrEmpty(form[name + "_SALE1"]) ? "0" : form[name + "_SALE1"].ToString();
                        var _sALE2 = string.IsNullOrEmpty(form[name + "_SALE2"]) ? "0" : form[name + "_SALE2"].ToString();
                        var _sALE3 = string.IsNullOrEmpty(form[name + "_SALE3"]) ? "0" : form[name + "_SALE3"].ToString();

                        int.TryParse(_unitId, out int unitId);
                        decimal.TryParse(_tradePrice, out decimal tradePrice);
                        decimal.TryParse(_retailPrice, out decimal retailPrice);
                        decimal.TryParse(_floorPrice, out decimal floorPrice);
                        decimal.TryParse(_purchasePrice, out decimal purchasePrice);
                        decimal.TryParse(_costPrice, out decimal costPrice);
                        decimal.TryParse(_sALE1, out decimal sALE1);
                        decimal.TryParse(_sALE2, out decimal sALE2);
                        decimal.TryParse(_sALE3, out decimal sALE3);

                        switch (name.ToUpper())
                        {
                            case "SMALL":
                                model.SmallProductPrices.ProductId = product.Id;
                                model.SmallProductPrices.UnitId = unitId;
                                model.SmallProductPrices.TradePrice = tradePrice;
                                model.SmallProductPrices.RetailPrice = retailPrice;
                                model.SmallProductPrices.FloorPrice = floorPrice;
                                model.SmallProductPrices.PurchasePrice = purchasePrice;
                                model.SmallProductPrices.CostPrice = costPrice;
                                model.SmallProductPrices.SALE1 = sALE1;
                                model.SmallProductPrices.SALE2 = sALE2;
                                model.SmallProductPrices.SALE3 = sALE3;
                                break;
                            case "STROKE":
                                model.StrokeProductPrices.ProductId = product.Id;
                                model.StrokeProductPrices.UnitId = unitId;
                                model.StrokeProductPrices.TradePrice = tradePrice;
                                model.StrokeProductPrices.RetailPrice = retailPrice;
                                model.StrokeProductPrices.FloorPrice = floorPrice;
                                model.StrokeProductPrices.PurchasePrice = purchasePrice;
                                model.StrokeProductPrices.CostPrice = costPrice;
                                model.StrokeProductPrices.SALE1 = sALE1;
                                model.StrokeProductPrices.SALE2 = sALE2;
                                model.StrokeProductPrices.SALE3 = sALE3;
                                break;
                            case "BIG":
                                model.BigProductPrices.ProductId = product.Id;
                                model.BigProductPrices.UnitId = unitId;
                                model.BigProductPrices.TradePrice = tradePrice;
                                model.BigProductPrices.RetailPrice = retailPrice;
                                model.BigProductPrices.FloorPrice = floorPrice;
                                model.BigProductPrices.PurchasePrice = purchasePrice;
                                model.BigProductPrices.CostPrice = costPrice;
                                model.BigProductPrices.SALE1 = sALE1;
                                model.BigProductPrices.SALE2 = sALE2;
                                model.BigProductPrices.SALE3 = sALE3;
                                break;
                        }
                    }

                    var spp = model.SmallProductPrices.ToEntity<ProductPrice>();
                    spp.ProductId = product.Id;
                    spp.StoreId = curStore?.Id ?? 0;

                    if (spp.UnitId != 0)
                    {
                        productPrices.Add(spp);
                    }
                    var tpp = model.StrokeProductPrices.ToEntity<ProductPrice>();
                    tpp.ProductId = product.Id;
                    tpp.StoreId = curStore?.Id ?? 0;
                    if (tpp.UnitId != 0)
                    {
                        productPrices.Add(tpp);
                    }
                    var bpp = model.BigProductPrices.ToEntity<ProductPrice>();
                    bpp.ProductId = product.Id;
                    bpp.StoreId = curStore?.Id ?? 0;
                    if (bpp.UnitId != 0)
                    {
                        productPrices.Add(bpp);
                    }
                }
                catch (Exception)
                {
                    _userActivityService.InsertActivity("InsertProduct", "添加商品价格失败", curUser, model.Name);
                }
                #endregion

                #region  规格属性
                List<ProductSpecificationAttribute> productSpecificationAttributes = new List<ProductSpecificationAttribute>();

                try
                {
                    var smallPSpec = new ProductSpecificationAttribute()
                    {
                        StoreId = curStore?.Id ?? 0,
                        ProductId = product.Id,
                        SpecificationAttributeOptionId = product.SmallUnitId,
                        CustomValue = "小",
                        AllowFiltering = true,
                        ShowOnProductPage = true,
                        DisplayOrder = 0
                    };
                    productSpecificationAttributes.Add(smallPSpec);

                    if (product.StrokeUnitId > 0)
                    {
                        var strokePSpec = new ProductSpecificationAttribute()
                        {
                            StoreId = curStore?.Id ?? 0,
                            ProductId = product.Id,
                            SpecificationAttributeOptionId = product.StrokeUnitId ?? _specificationAttributeService.GetSpecificationAttributeOptionsByStore(curStore.Id).FirstOrDefault().Id,
                            CustomValue = "中",
                            AllowFiltering = true,
                            ShowOnProductPage = true,
                            DisplayOrder = 0
                        };
                        productSpecificationAttributes.Add(strokePSpec);
                    }

                    if (product.BigUnitId > 0)
                    {
                        var bigSpec = new ProductSpecificationAttribute()
                        {
                            StoreId = curStore?.Id ?? 0,
                            ProductId = product.Id,
                            SpecificationAttributeOptionId = product.BigUnitId ?? _specificationAttributeService.GetSpecificationAttributeOptionsByStore(curStore.Id).FirstOrDefault().Id,
                            CustomValue = "大",
                            AllowFiltering = true,
                            ShowOnProductPage = true,
                            DisplayOrder = 0
                        };
                        productSpecificationAttributes.Add(bigSpec);
                    }
                }
                catch (Exception)
                {
                    _userActivityService.InsertActivity("InsertProduct", "添加商品规格属性失败", curUser, model.Name);
                }
                #endregion

                #region 组合商品
                var combination = new Combination();
                try
                {
                    combination.StoreId = curStore?.Id ?? 0;
                    combination.DisplayOrder = 0;
                    combination.ProductId = product.Id;
                    combination.ProductName = product.Name;
                    combination.StoreId = curStore?.Id ?? 0;
                    combination.Enabled = true;
                }
                catch (Exception)
                {
                    _userActivityService.InsertActivity("InsertProduct", "添加组合商品失败", curUser, model.Name);
                }
                #endregion

                //RedLock
                string lockKey = string.Format(DCMSCachingDefaults.RedisDataReSubmitKey, Request.GetUrl(), curStore.Id, curUser.Id, CommonHelper.MD5(JsonConvert.SerializeObject(model)));
                var result = await _locker.PerformActionWithLockAsync(lockKey,
                    TimeSpan.FromSeconds(30),
                    TimeSpan.FromSeconds(10),
                    TimeSpan.FromSeconds(1),
                    () => _productService.ProductCreateOrUpdate(curStore.Id, curUser.Id, 0, product, productSpecificationAttributes, productPrices, null, null, combination, null));


                //activity log
                _userActivityService.InsertActivity("InsertProduct", "添加商品成功", curUser, product.Name);
                //_notificationService.SuccessNotification("添加商品成功");

                if (newCreate)
                    return RedirectToAction("Create");
                else
                    return continueEditing ? RedirectToAction("Edit", new { id = product.Id }) : RedirectToAction("List");
              
            }

            //获取配置
            var productSetting = _settingService.LoadSetting<ProductSetting>(curStore?.Id ?? 0);

            var smalllists = new List<SelectListItem> { new SelectListItem() { Text = "-无-", Value = "0" } };
            var stroklists = new List<SelectListItem> { new SelectListItem() { Text = "-无-", Value = "0" } };
            var biglists = new List<SelectListItem> { new SelectListItem() { Text = "-无-", Value = "0" } };

            //分类
            model = BindCategoryDropDownList<ProductModel>(model, new Func<int?, int, bool, IList<Category>>(_categoryService.GetAllCategoriesByParentCategoryId), curStore?.Id ?? 0, 0);

            //规格属性
            var smallOptions = _specificationAttributeService.GetSpecificationAttributeOptionsBySpecificationAttribute(curStore?.Id ?? 0, productSetting.SmallUnitSpecificationAttributeOptionsMapping).ToList();
            var strokOptions = _specificationAttributeService.GetSpecificationAttributeOptionsBySpecificationAttribute(curStore?.Id ?? 0, productSetting.StrokeUnitSpecificationAttributeOptionsMapping).ToList();
            var bigOptions = _specificationAttributeService.GetSpecificationAttributeOptionsBySpecificationAttribute(curStore?.Id ?? 0, productSetting.BigUnitSpecificationAttributeOptionsMapping).ToList();

            smallOptions.ForEach(o =>
            {
                smalllists.Add(new SelectListItem() { Text = o.Name, Value = o.Id.ToString() });
            });
            strokOptions.ForEach(o =>
            {
                stroklists.Add(new SelectListItem() { Text = o.Name, Value = o.Id.ToString() });
            });
            bigOptions.ForEach(o =>
            {
                biglists.Add(new SelectListItem() { Text = o.Name, Value = o.Id.ToString() });
            });

            model.SmallUnits = new SelectList(smalllists, "Value", "Text");
            model.StrokeUnits = new SelectList(stroklists, "Value", "Text");
            model.BigUnits = new SelectList(biglists, "Value", "Text");

            return View(model);
        }


        [HttpPost, ParameterBasedOnFormNameAttribute("save-newcreate", "newCreate")]
        [AuthCode((int)AccessGranularityEnum.ProductArchivesSave)]
        public async Task<IActionResult> NewCreate(ProductModel model, IFormCollection form, bool newCreate)
        {
            //获取配置
            var companySetting = _settingService.LoadSetting<CompanySetting>(curStore?.Id ?? 0);
            var products = _productService.GetAllProducts(curStore?.Id ?? 0);

            if (string.IsNullOrEmpty(model.Name))
            {
                ModelState.AddModelError("", "商品名称不能为空");
            }

            if (string.IsNullOrEmpty(model.MnemonicCode))
            {
                ModelState.AddModelError("", "商品助记码不能为空");
            }
            if (model.MnemonicCode.Length > 100)
            {
                ModelState.AddModelError("", "商品助记码最大长度为100");
            }

            if (model.SmallUnitId == 0)
            {
                ModelState.AddModelError("", "请选择商品最小单位");
            }

            if (model.CategoryId == 0)
            {
                ModelState.AddModelError("", "请选择商品类别");
            }

            if (model.BrandId == 0)
            {
                ModelState.AddModelError("", "请选择品牌");
            }

            if (model.BigUnitId > 0 && (model.BigQuantity ?? 0) <= 0)
            {
                ModelState.AddModelError("", "请输入大单位转换数量");
            }
            if (model.StrokeUnitId > 0 && (model.StrokeQuantity ?? 0) <= 0)
            {
                ModelState.AddModelError("", "请输入中单位转换数量");
            }

            if (ModelState.IsValid)
            {
                var product = model.ToEntity<Product>();

                #region 基本信息
                //添加商品
                try
                {
                    //商品
                    product.StoreId = curStore?.Id ?? 0;
                    //product.MnemonicCode = CommonHelper.GenerateStrchar(8) + "_" + model.MnemonicCode;
                    product.MnemonicCode = model.MnemonicCode;
                    product.BigQuantity = model.BigQuantity ?? 0;
                    product.StrokeQuantity = model.StrokeQuantity ?? 0;
                    product.UpdatedOnUtc = DateTime.Now;
                    product.CreatedOnUtc = DateTime.Now;
                    product.Deleted = false;
                    product.Published = true;
                    product.DisplayOrder = model.DisplayOrder;
                    product.Name = model.Name;
                    product.MnemonicCode = model.MnemonicCode;
                    product.CategoryId = model.CategoryId;
                    product.BrandId = model.BrandId;
                    product.SmallUnitId = model.SmallUnitId;
                    product.StrokeUnitId = model.StrokeUnitId;
                    product.BigUnitId = model.BigUnitId;
                    product.IsAdjustPrice = model.IsAdjustPrice;
                    product.Status = model.Status;
                    //是否启用生产日期
                    product.IsManufactureDete = model.IsManufactureDete;

                    //其他信息
                    product.Sku = model.Sku;
                    product.Specification = model.Specification;
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
                }
                catch (Exception ex)
                {
                    _userActivityService.InsertActivity("InsertProduct", ex.Message, curUser, model.Name);
                }

                #endregion

                #region 商品价格
                List<ProductPrice> productPrices = new List<ProductPrice>();

                try
                {
                    //添加商品价格
                    var names = new string[] { "Small", "Stroke", "Big" };
                    foreach (var name in names)
                    {


                        var _unitId = string.IsNullOrEmpty(form[name + "_UnitId"]) ? "0" : form[name + "_UnitId"].ToString();
                        var _tradePrice = string.IsNullOrEmpty(form[name + "_TradePrice"]) ? "0" : form[name + "_TradePrice"].ToString();
                        var _retailPrice = string.IsNullOrEmpty(form[name + "_RetailPrice"]) ? "0" : form[name + "_RetailPrice"].ToString();
                        var _floorPrice = string.IsNullOrEmpty(form[name + "_FloorPrice"]) ? "0" : form[name + "_FloorPrice"].ToString();
                        var _purchasePrice = string.IsNullOrEmpty(form[name + "_PurchasePrice"]) ? "0" : form[name + "_PurchasePrice"].ToString();
                        var _costPrice = string.IsNullOrEmpty(form[name + "_CostPrice"]) ? "0" : form[name + "_CostPrice"].ToString();
                        var _sALE1 = string.IsNullOrEmpty(form[name + "_SALE1"]) ? "0" : form[name + "_SALE1"].ToString();
                        var _sALE2 = string.IsNullOrEmpty(form[name + "_SALE2"]) ? "0" : form[name + "_SALE2"].ToString();
                        var _sALE3 = string.IsNullOrEmpty(form[name + "_SALE3"]) ? "0" : form[name + "_SALE3"].ToString();

                        int.TryParse(_unitId, out int unitId);
                        decimal.TryParse(_tradePrice, out decimal tradePrice);
                        decimal.TryParse(_retailPrice, out decimal retailPrice);
                        decimal.TryParse(_floorPrice, out decimal floorPrice);
                        decimal.TryParse(_purchasePrice, out decimal purchasePrice);
                        decimal.TryParse(_costPrice, out decimal costPrice);
                        decimal.TryParse(_sALE1, out decimal sALE1);
                        decimal.TryParse(_sALE2, out decimal sALE2);
                        decimal.TryParse(_sALE3, out decimal sALE3);

                        switch (name.ToUpper())
                        {
                            case "SMALL":
                                model.SmallProductPrices.ProductId = product.Id;
                                model.SmallProductPrices.UnitId = unitId;
                                model.SmallProductPrices.TradePrice = tradePrice;
                                model.SmallProductPrices.RetailPrice = retailPrice;
                                model.SmallProductPrices.FloorPrice = floorPrice;
                                model.SmallProductPrices.PurchasePrice = purchasePrice;
                                model.SmallProductPrices.CostPrice = costPrice;
                                model.SmallProductPrices.SALE1 = sALE1;
                                model.SmallProductPrices.SALE2 = sALE2;
                                model.SmallProductPrices.SALE3 = sALE3;
                                break;
                            case "STROKE":
                                model.StrokeProductPrices.ProductId = product.Id;
                                model.StrokeProductPrices.UnitId = unitId;
                                model.StrokeProductPrices.TradePrice = tradePrice;
                                model.StrokeProductPrices.RetailPrice = retailPrice;
                                model.StrokeProductPrices.FloorPrice = floorPrice;
                                model.StrokeProductPrices.PurchasePrice = purchasePrice;
                                model.StrokeProductPrices.CostPrice = costPrice;
                                model.StrokeProductPrices.SALE1 = sALE1;
                                model.StrokeProductPrices.SALE2 = sALE2;
                                model.StrokeProductPrices.SALE3 = sALE3;
                                break;
                            case "BIG":
                                model.BigProductPrices.ProductId = product.Id;
                                model.BigProductPrices.UnitId = unitId;
                                model.BigProductPrices.TradePrice = tradePrice;
                                model.BigProductPrices.RetailPrice = retailPrice;
                                model.BigProductPrices.FloorPrice = floorPrice;
                                model.BigProductPrices.PurchasePrice = purchasePrice;
                                model.BigProductPrices.CostPrice = costPrice;
                                model.BigProductPrices.SALE1 = sALE1;
                                model.BigProductPrices.SALE2 = sALE2;
                                model.BigProductPrices.SALE3 = sALE3;
                                break;
                        }
                    }

                    var spp = model.SmallProductPrices.ToEntity<ProductPrice>();
                    spp.ProductId = product.Id;
                    spp.StoreId = curStore?.Id ?? 0;

                    if (spp.UnitId != 0)
                    {
                        productPrices.Add(spp);
                    }
                    var tpp = model.StrokeProductPrices.ToEntity<ProductPrice>();
                    tpp.ProductId = product.Id;
                    tpp.StoreId = curStore?.Id ?? 0;
                    if (tpp.UnitId != 0)
                    {
                        productPrices.Add(tpp);
                    }
                    var bpp = model.BigProductPrices.ToEntity<ProductPrice>();
                    bpp.ProductId = product.Id;
                    bpp.StoreId = curStore?.Id ?? 0;
                    if (bpp.UnitId != 0)
                    {
                        productPrices.Add(bpp);
                    }
                }
                catch (Exception)
                {
                    _userActivityService.InsertActivity("InsertProduct", "添加商品价格失败", curUser, model.Name);
                }
                #endregion

                #region  规格属性
                List<ProductSpecificationAttribute> productSpecificationAttributes = new List<ProductSpecificationAttribute>();

                try
                {
                    var smallPSpec = new ProductSpecificationAttribute()
                    {
                        StoreId = curStore?.Id ?? 0,
                        ProductId = product.Id,
                        SpecificationAttributeOptionId = product.SmallUnitId,
                        CustomValue = "小",
                        AllowFiltering = true,
                        ShowOnProductPage = true,
                        DisplayOrder = 0
                    };
                    productSpecificationAttributes.Add(smallPSpec);

                    if (product.StrokeUnitId > 0)
                    {
                        var strokePSpec = new ProductSpecificationAttribute()
                        {
                            StoreId = curStore?.Id ?? 0,
                            ProductId = product.Id,
                            SpecificationAttributeOptionId = product.StrokeUnitId ?? _specificationAttributeService.GetSpecificationAttributeOptionsByStore(curStore.Id).FirstOrDefault().Id,
                            CustomValue = "中",
                            AllowFiltering = true,
                            ShowOnProductPage = true,
                            DisplayOrder = 0
                        };
                        productSpecificationAttributes.Add(strokePSpec);
                    }

                    if (product.BigUnitId > 0)
                    {
                        var bigSpec = new ProductSpecificationAttribute()
                        {
                            StoreId = curStore?.Id ?? 0,
                            ProductId = product.Id,
                            SpecificationAttributeOptionId = product.BigUnitId ?? _specificationAttributeService.GetSpecificationAttributeOptionsByStore(curStore.Id).FirstOrDefault().Id,
                            CustomValue = "大",
                            AllowFiltering = true,
                            ShowOnProductPage = true,
                            DisplayOrder = 0
                        };
                        productSpecificationAttributes.Add(bigSpec);
                    }
                }
                catch (Exception)
                {
                    _userActivityService.InsertActivity("InsertProduct", "添加商品规格属性失败", curUser, model.Name);
                }
                #endregion

                #region 组合商品
                var combination = new Combination();
                try
                {
                    combination.StoreId = curStore?.Id ?? 0;
                    combination.DisplayOrder = 0;
                    combination.ProductId = product.Id;
                    combination.ProductName = product.Name;
                    combination.StoreId = curStore?.Id ?? 0;
                    combination.Enabled = true;
                }
                catch (Exception)
                {
                    _userActivityService.InsertActivity("InsertProduct", "添加组合商品失败", curUser, model.Name);
                }
                #endregion

                //RedLock
                string lockKey = string.Format(DCMSCachingDefaults.RedisDataReSubmitKey, Request.GetUrl(), curStore.Id, curUser.Id, CommonHelper.MD5(JsonConvert.SerializeObject(model)));
                var result = await _locker.PerformActionWithLockAsync(lockKey,
                    TimeSpan.FromSeconds(30),
                    TimeSpan.FromSeconds(10),
                    TimeSpan.FromSeconds(1),
                    () => _productService.ProductCreateOrUpdate(curStore.Id, curUser.Id, 0, product, productSpecificationAttributes, productPrices, null, null, combination, null));


                //activity log
                _userActivityService.InsertActivity("InsertProduct", "添加商品成功", curUser, product.Name);

                //_notificationService.SuccessNotification("添加商品成功");
                //return newCreate ? RedirectToAction("Edit", new { id = product.Id }) : RedirectToAction("List");
            }

            return RedirectToAction("Create");
        }


        /// <summary>
        /// 编辑商品
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [AuthCode((int)AccessGranularityEnum.ProductArchivesView)]
        public IActionResult Edit(int? id, bool reset = false)
        {
            if (!id.HasValue)
            {
                return RedirectToAction("List");
            }

            var model = new ProductModel();

            var product = _productService.GetProductById(curStore.Id, id.Value);

            if (product != null)
            {
                model = product.ToModel<ProductModel>();
                //只能看到当前经销商的商品
                if (product.StoreId != curStore.Id)
                {
                    return RedirectToAction("List");
                }
            }
            else
            {
                return RedirectToAction("List");
            }

            if (reset) //是否重置方案
            {
                var tiers = _productService.GetProductTierPriceByProductId(product.Id);

                if (tiers != null && tiers.Count > 0)
                {
                    _productService.DeleteProductTierPrice(tiers.ToList());
                }

            }

            var combination = _productService.GetCombinationByProductId(id.Value);
            if (combination != null)
            {
                model.CombinationId = combination.Id;
            }

            //获取配置
            var companySetting = _settingService.LoadSetting<CompanySetting>(curStore?.Id ?? 0);
            var productSetting = _settingService.LoadSetting<ProductSetting>(curStore?.Id ?? 0);

            var smalllists = new List<SelectListItem> { new SelectListItem() { Text = "-无-", Value = "0" } };
            var stroklists = new List<SelectListItem> { new SelectListItem() { Text = "-无-", Value = "0" } };
            var biglists = new List<SelectListItem> { new SelectListItem() { Text = "-无-", Value = "0" } };

            //分类
            model = BindCategoryDropDownList<ProductModel>(model, new Func<int?, int, bool, IList<Category>>(_categoryService.GetAllCategoriesByParentCategoryId), curStore?.Id ?? 0, 0);

            //统计类别 
            var statisticalTypes = _statisticalTypeService.GetAllStatisticalTypess(curStore?.Id ?? 0);
            var types = new List<SelectListItem>();
            statisticalTypes.ToList().ForEach(t =>
            {
                types.Add(new SelectListItem
                {
                    Text = t.Name,
                    Value = t.Id.ToString(),
                });
            });

            model.StatisticalTypes = new SelectList(types, "Value", "Text");
            if (model.StatisticalType == 0)
            {
                model.StatisticalType = null;
            }

            //规格属性
            var smallOptions = _specificationAttributeService.GetSpecificationAttributeOptionsBySpecificationAttribute(curStore?.Id ?? 0, productSetting.SmallUnitSpecificationAttributeOptionsMapping).ToList();
            var strokOptions = _specificationAttributeService.GetSpecificationAttributeOptionsBySpecificationAttribute(curStore?.Id ?? 0, productSetting.StrokeUnitSpecificationAttributeOptionsMapping).ToList();
            var bigOptions = _specificationAttributeService.GetSpecificationAttributeOptionsBySpecificationAttribute(curStore?.Id ?? 0, productSetting.BigUnitSpecificationAttributeOptionsMapping).ToList();

            smallOptions.ForEach(o =>
            {
                smalllists.Add(new SelectListItem() { Text = o.Name, Value = o.Id.ToString() });
            });
            strokOptions.ForEach(o =>
            {
                stroklists.Add(new SelectListItem() { Text = o.Name, Value = o.Id.ToString() });
            });
            bigOptions.ForEach(o =>
            {
                biglists.Add(new SelectListItem() { Text = o.Name, Value = o.Id.ToString() });
            });

            model.SmallUnits = new SelectList(smalllists, "Value", "Text");
            model.StrokeUnits = new SelectList(stroklists, "Value", "Text");
            model.BigUnits = new SelectList(biglists, "Value", "Text");

            //获取单位
            var smallProductPrices = _productService.GetProductPriceByProductIdAndUnitId(curStore.Id, product.Id, product.SmallUnitId);
            var strokeProductPrices = _productService.GetProductPriceByProductIdAndUnitId(curStore.Id, product.Id, product.StrokeUnitId ?? 0);
            var bigProductPrices = _productService.GetProductPriceByProductIdAndUnitId(curStore.Id, product.Id, product.BigUnitId ?? 0);


            model.SmallProductPrices = smallProductPrices != null ? smallProductPrices.ToModel<ProductPriceModel>() : new ProductPriceModel();
            model.StrokeProductPrices = strokeProductPrices != null ? strokeProductPrices.ToModel<ProductPriceModel>() : new ProductPriceModel();
            model.BigProductPrices = bigProductPrices != null ? bigProductPrices.ToModel<ProductPriceModel>() : new ProductPriceModel();

            var supplier = _manufacturerService.GetManufacturerById(curStore.Id, model.Supplier ?? 0);
            model.SupplierName = supplier != null ? supplier.Name : "";

            var smallOption = _specificationAttributeService.GetSpecificationAttributeOptionById(product.SmallUnitId);
            var strokeOption = _specificationAttributeService.GetSpecificationAttributeOptionById(product.StrokeUnitId ?? 0);
            var bigOption = _specificationAttributeService.GetSpecificationAttributeOptionById(product.BigUnitId ?? 0);

            model.smallOption = smallOption.ToModel<SpecificationAttributeOptionModel>();
            model.strokeOption = strokeOption.ToModel<SpecificationAttributeOptionModel>();
            model.bigOption = bigOption.ToModel<SpecificationAttributeOptionModel>();

            model.UnitName = "";
            model.UnitName = string.Format("1{0} = {1}{2}", bigOption != null ? bigOption.Name : "/", product.BigQuantity ?? 0, smallOption != null ? smallOption.Name : "/");
            model.BrandName = _brandService.GetBrandName(curStore.Id, model.BrandId);

            //层次价格
            var productTierPrices1 = new ProductTierPrice();
            var productTierPrices2 = new ProductTierPrice();
            var productTierPrices3 = new ProductTierPrice();
            var productTierPrices4 = new ProductTierPrice();
            //var productTierPrices5 = new ProductTierPrice();
            var productTierPrices6 = new ProductTierPrice();
            var tierPricesOlds = _productService.GetProductTierPriceByProductId(id.Value);
            if (tierPricesOlds != null && tierPricesOlds.Count > 0)
            {
                productTierPrices1 = tierPricesOlds.Where(p => p.PricesPlanId == 0 && p.PriceTypeId == (int)PriceType.ProductCost).FirstOrDefault();
                productTierPrices2 = tierPricesOlds.Where(p => p.PricesPlanId == 0 && p.PriceTypeId == (int)PriceType.WholesalePrice).FirstOrDefault();
                productTierPrices3 = tierPricesOlds.Where(p => p.PricesPlanId == 0 && p.PriceTypeId == (int)PriceType.RetailPrice).FirstOrDefault();
                productTierPrices4 = tierPricesOlds.Where(p => p.PricesPlanId == 0 && p.PriceTypeId == (int)PriceType.LowestPrice).FirstOrDefault();
                //productTierPrices5 = tierPricesOlds.Where(p => p.PricesPlanId == 0 && p.PriceTypeId == (int)PriceType.LastedPrice).FirstOrDefault();
                productTierPrices6 = tierPricesOlds.Where(p => p.PricesPlanId == 0 && p.PriceTypeId == (int)PriceType.CostPrice).FirstOrDefault();
            }

            var tierPrices = new List<ProductTierPrice>
            {
                //进价
                new ProductTierPrice()
                {
                    StoreId = curStore?.Id??0,
                    ProductId = id.Value,
                    PricesPlanId = 0,
                    PriceTypeId = (int)PriceType.ProductCost,
                    SmallUnitPrice = productTierPrices1 == null || productTierPrices1.SmallUnitPrice==0 ? model.SmallProductPrices.PurchasePrice==0? 0 : model.SmallProductPrices.PurchasePrice : productTierPrices1.SmallUnitPrice,
                    StrokeUnitPrice = productTierPrices1 == null || productTierPrices1.StrokeUnitPrice==0 ? model.StrokeProductPrices.PurchasePrice==0? 0 : model.StrokeProductPrices.PurchasePrice : productTierPrices1.StrokeUnitPrice,
                    BigUnitPrice = productTierPrices1 == null || productTierPrices1.BigUnitPrice==0 ? model.BigProductPrices.PurchasePrice==0? 0 : model.BigProductPrices.PurchasePrice : productTierPrices1.BigUnitPrice,
                },
                //批发
                new ProductTierPrice()
                {
                    StoreId = curStore?.Id??0,
                    ProductId = id.Value,
                    PricesPlanId = 0,
                    PriceTypeId = (int)PriceType.WholesalePrice,
                    SmallUnitPrice = productTierPrices2 == null || productTierPrices2.SmallUnitPrice==0 ? model.SmallProductPrices.TradePrice==0? 0 : model.SmallProductPrices.TradePrice : productTierPrices2.SmallUnitPrice,
                    StrokeUnitPrice = productTierPrices2 == null || productTierPrices2.StrokeUnitPrice==0 ? model.StrokeProductPrices.TradePrice==0? 0 : model.StrokeProductPrices.TradePrice : productTierPrices2.StrokeUnitPrice,
                    BigUnitPrice = productTierPrices2 == null || productTierPrices2.BigUnitPrice==0 ? model.BigProductPrices.TradePrice==0? 0 : model.BigProductPrices.TradePrice : productTierPrices2.BigUnitPrice,
                },
                //零售
                new ProductTierPrice()
                {
                    StoreId = curStore?.Id??0,
                    ProductId = id.Value,
                    PricesPlanId = 0,
                    PriceTypeId = (int)PriceType.RetailPrice,
                    SmallUnitPrice = productTierPrices3 == null || productTierPrices3.SmallUnitPrice==0 ? model.SmallProductPrices.RetailPrice==0? 0 : model.SmallProductPrices.RetailPrice : productTierPrices3.SmallUnitPrice,
                    StrokeUnitPrice = productTierPrices3 == null || productTierPrices3.StrokeUnitPrice==0 ? model.StrokeProductPrices.RetailPrice==0? 0 : model.StrokeProductPrices.RetailPrice : productTierPrices3.StrokeUnitPrice,
                    BigUnitPrice = productTierPrices3 == null || productTierPrices3.BigUnitPrice==0 ? model.BigProductPrices.RetailPrice==0? 0 : model.BigProductPrices.RetailPrice : productTierPrices3.BigUnitPrice,
                },
                //最低售价
                new ProductTierPrice()
                {
                    StoreId = curStore?.Id??0,
                    ProductId = id.Value,
                    PricesPlanId = 0,
                    PriceTypeId = (int)PriceType.LowestPrice,
                    SmallUnitPrice = productTierPrices4 == null || productTierPrices4.SmallUnitPrice==0 ? model.SmallProductPrices.FloorPrice==0? 0 : model.SmallProductPrices.FloorPrice : productTierPrices4.SmallUnitPrice,
                    StrokeUnitPrice = productTierPrices4 == null || productTierPrices4.StrokeUnitPrice==0 ? model.StrokeProductPrices.FloorPrice==0? 0 : model.StrokeProductPrices.FloorPrice : productTierPrices4.StrokeUnitPrice,
                    BigUnitPrice = productTierPrices4 == null || productTierPrices4.BigUnitPrice==0 ? model.BigProductPrices.FloorPrice==0? 0 : model.BigProductPrices.FloorPrice : productTierPrices4.BigUnitPrice,
                },
                //上次售价
                //tierPrices.Add(new ProductTierPrice()
                //{
                //    StoreId = curStore?.Id??0,
                //    ProductId = id.Value,
                //    PricesPlanId = 0,
                //    PriceTypeId = (int)PriceType.LastedPrice,
                //    SmallUnitPrice = productTierPrices5 == null ? 0 : productTierPrices5.SmallUnitPrice,
                //    StrokeUnitPrice = productTierPrices5 == null ? 0 : productTierPrices5.StrokeUnitPrice,
                //    BigUnitPrice = productTierPrices5 == null ? 0 : productTierPrices5.BigUnitPrice,
                //});
                //成本价
                new ProductTierPrice()
                {
                    StoreId = curStore?.Id??0,
                    ProductId = id.Value,
                    PricesPlanId = 0,
                    PriceTypeId = (int)PriceType.CostPrice,
                    SmallUnitPrice = productTierPrices6 == null || productTierPrices6.SmallUnitPrice==0 ? model.SmallProductPrices.CostPrice==0? 0 : model.SmallProductPrices.CostPrice : productTierPrices6.SmallUnitPrice,
                    StrokeUnitPrice = productTierPrices6 == null || productTierPrices6.StrokeUnitPrice==0 ? model.StrokeProductPrices.CostPrice==0? 0 : model.StrokeProductPrices.CostPrice : productTierPrices6.StrokeUnitPrice,
                    BigUnitPrice = productTierPrices6 == null || productTierPrices6.BigUnitPrice==0 ? model.BigProductPrices.CostPrice==0? 0 : model.BigProductPrices.CostPrice : productTierPrices6.BigUnitPrice,
                }
            };

            model.ProductTierPrices.Clear();

            foreach (var p in tierPrices)
            {
                model.ProductTierPrices.Add(new ProductTierPriceModel()
                {
                    StoreId = p.StoreId,
                    ProductId = p.ProductId,
                    PricesPlanId = p.PricesPlanId,
                    PriceTypeId = p.PriceTypeId,
                    PriceType = p.PriceType,
                    PriceTypeName = CommonHelper.GetEnumDescription<PriceType>(p.PriceType),
                    SmallUnitPrice = p.SmallUnitPrice,
                    StrokeUnitPrice = p.StrokeUnitPrice,
                    BigUnitPrice = p.BigUnitPrice,

                });

            }

            //自定义方案
            var plans = _productService.GetAllPlansByStore(curStore?.Id ?? 0);
            foreach (var p in plans)
            {
                var productTierPrices = new ProductTierPrice();
                if (tierPricesOlds != null && tierPricesOlds.Count > 0)
                {
                    productTierPrices = tierPricesOlds.Where(tp => tp.PricesPlanId == p.Id && tp.PriceTypeId == (int)PriceType.CustomPlan).FirstOrDefault();
                }

                try
                {
                    model.ProductTierPrices.Add(new ProductTierPriceModel()
                    {
                        StoreId = p.StoreId,
                        ProductId = model.Id,
                        PricesPlanId = p.Id,
                        PriceTypeId = (int)PriceType.CustomPlan,
                        PriceTypeName = p.Name,
                        //PriceType = (PriceType)Enum.Parse(typeof(PriceType), p.Name),
                        SmallUnitPrice = productTierPrices == null ? 0 : productTierPrices.SmallUnitPrice,
                        StrokeUnitPrice = productTierPrices == null ? 0 : productTierPrices.StrokeUnitPrice,
                        BigUnitPrice = productTierPrices == null ? 0 : productTierPrices.BigUnitPrice,
                    });
                }
                catch (Exception)
                {
                }

            }

            //是否显示生产日期设置列
            model.IsShowCreateDate = companySetting.OpenBillMakeDate == 2;


            return View(model);
        }

        [HttpPost, ParameterBasedOnFormNameAttribute("save-continue", "continueEditing")]
        [AuthCode((int)AccessGranularityEnum.ProductArchivesSave)]
        public async Task<IActionResult> Edit(ProductModel model, IFormCollection form, bool continueEditing)
        {

            //获取配置
            var companySetting = _settingService.LoadSetting<CompanySetting>(curStore.Id);
            var products = _productService.GetAllProducts(curStore.Id);
            var ProductFlavors = _productFlavorService.GetProductFlavorsByParentId(model.Id);
            if (ProductFlavors.Count == 0 && model.IsFlavor == true)
            {
                ModelState.AddModelError("", "没有口味信息");
            }

            if (string.IsNullOrEmpty(model.Name))
            {
                ModelState.AddModelError("", "商品名称不能为空");
            }

            if (string.IsNullOrEmpty(model.MnemonicCode))
            {
                ModelState.AddModelError("", "商品助记码不能为空");
            }
            if (model.MnemonicCode.Length > 100)
            {
                ModelState.AddModelError("", "助记码最大长度为100");
            }

            if (model.SmallUnitId == 0)
            {
                ModelState.AddModelError("", "请选择商品最小单位");
            }

            if (model.CategoryId == 0)
            {
                ModelState.AddModelError("", "请选择商品类别");
            }

            if (model.BrandId == 0)
            {
                ModelState.AddModelError("", "请选择品牌");
            }


            if (model.BigUnitId > 0 && (model.BigQuantity ?? 0) <= 0)
            {
                ModelState.AddModelError("", "请输入大单位转换数量");
            }
            if (model.StrokeUnitId > 0 && (model.StrokeQuantity ?? 0) <= 0)
            {
                ModelState.AddModelError("", "请输入中单位转换数量");
            }

            if (model.OtherBarCode != null || model.OtherBarCode1 != null || model.OtherBarCode2 != null)
            {
                if (companySetting.AllowCreateMulSameBarcode)//允许创建多个相同条码的商品
                {
                    if (products.Count(x => (x.OtherBarCode == model.OtherBarCode && model.OtherBarCode != null) || (x.OtherBarCode1 == model.OtherBarCode1 && x.OtherBarCode1 != null) || (x.OtherBarCode2 == model.OtherBarCode2 && model.OtherBarCode2 != null)) > 1)
                    {
                        ModelState.AddModelError("", "不能添加条码相同的商品");
                    }
                }
            }

            var smallProductPrices = new ProductPrice();
            var strokeProductPrices = new ProductPrice();
            var bigProductPrices = new ProductPrice();

            if (ModelState.IsValid)
            {
                var product = _productService.GetProductById(curStore.Id, model.Id);

                #region 基本信息
                try
                {
                    //基本信息
                    product.StoreId = curStore?.Id ?? 0;
                    //已开单只能修改 商品名称和商品助记码
                    if (product.HasSold)
                    {
                        product.MnemonicCode = model.MnemonicCode;
                        product.Name = model.Name;
                        product.IsAdjustPrice = model.IsAdjustPrice;  //是否要允许调价
                        product.SmallBarCode = model.SmallBarCode;    //小单位条码
                        product.StrokeBarCode = model.StrokeBarCode;  //中单位条码
                        product.BigBarCode = model.BigBarCode;        //大单位条码
                        product.OtherBarCode = model.OtherBarCode;    //条码
                        product.OtherBarCode1 = model.OtherBarCode1;  //条码
                        product.OtherBarCode2 = model.OtherBarCode2;  //条码
                    }
                    else {
                        product.MnemonicCode = model.MnemonicCode;
                        product.BigQuantity = model.BigQuantity ?? 0;
                        product.StrokeQuantity = model.StrokeQuantity ?? 0;
                        product.CreatedOnUtc = DateTime.Now;
                        product.UpdatedOnUtc = DateTime.Now;
                        product.Published = true;
                        product.DisplayOrder = model.DisplayOrder;
                        product.Name = model.Name;
                        product.CategoryId = model.CategoryId;
                        product.BrandId = model.BrandId;
                        product.IsAdjustPrice = model.IsAdjustPrice;
                        product.Status = model.Status;
                        //是否启用生产日期
                        product.IsManufactureDete = model.IsManufactureDete;
                        product.SmallBarCode = model.SmallBarCode;    
                        product.StrokeBarCode = model.StrokeBarCode;  
                        product.BigBarCode = model.BigBarCode;        
                        product.OtherBarCode = model.OtherBarCode;    
                        product.OtherBarCode1 = model.OtherBarCode1;  
                        product.OtherBarCode2 = model.OtherBarCode2;
                        //其他信息
                        product.Sku = model.Sku;
                        product.ProductCode = model.Sku;  //2021-09-01 mu 添加
                        product.Specification = model.Specification;
                        product.CountryOrigin = model.CountryOrigin;
                        product.Supplier = model.Supplier;
                        
                        product.StatisticalType = model.StatisticalType;
                        product.ExpirationDays = model.ExpirationDays;
                        product.AdventDays = model.AdventDays;
                        product.IsFlavor = model.IsFlavor;
                    }
                    product.SmallUnitId = model.SmallUnitId;
                    product.StrokeUnitId = model.StrokeUnitId;
                    product.BigUnitId = model.BigUnitId;
                }
                catch (Exception)
                {
                    _userActivityService.InsertActivity("UpdateProduct", "商品信息更新失败", curUser, model.Name);
                }

                #endregion

                #region 商品属性
                List<ProductSpecificationAttribute> productSpecificationAttributes = new List<ProductSpecificationAttribute>();

                try
                {
                    #region 规格属性
                    var _smallPSpec = _specificationAttributeService.GetProductSpecificationAttributesByProductId(curStore.Id,product.Id).Where(p => p.SpecificationAttributeOptionId == product.SmallUnitId).FirstOrDefault();
                    var _strokePSpec = _specificationAttributeService.GetProductSpecificationAttributesByProductId(curStore.Id,product.Id).Where(p => p.SpecificationAttributeOptionId == product.StrokeUnitId).FirstOrDefault();
                    var _bigSpec = _specificationAttributeService.GetProductSpecificationAttributesByProductId(curStore.Id,product.Id).Where(p => p.SpecificationAttributeOptionId == product.BigUnitId).FirstOrDefault();

                    var smallPSpec = new ProductSpecificationAttribute()
                    {
                        Id = _smallPSpec?.Id ?? 0,
                        ProductId = product.Id,
                        SpecificationAttributeOptionId = product.SmallUnitId,
                        CustomValue = "小",
                        AllowFiltering = true,
                        ShowOnProductPage = true,
                        DisplayOrder = 0
                    };
                    var strokePSpec = new ProductSpecificationAttribute()
                    {
                        Id = _strokePSpec?.Id ?? 0,
                        ProductId = product.Id,
                        SpecificationAttributeOptionId = product.StrokeUnitId ?? _specificationAttributeService.GetSpecificationAttributeOptionsByStore(curStore.Id).FirstOrDefault().Id,
                        CustomValue = "中",
                        AllowFiltering = true,
                        ShowOnProductPage = true,
                        DisplayOrder = 0
                    };
                    var bigSpec = new ProductSpecificationAttribute()
                    {
                        Id = _bigSpec?.Id ?? 0,
                        ProductId = product.Id,
                        SpecificationAttributeOptionId = product.BigUnitId ?? _specificationAttributeService.GetSpecificationAttributeOptionsByStore(curStore.Id).FirstOrDefault().Id,
                        CustomValue = "大",
                        AllowFiltering = true,
                        ShowOnProductPage = true,
                        DisplayOrder = 0
                    };

                    if (_smallPSpec != null)
                    {
                        productSpecificationAttributes.Add(smallPSpec);
                    }
                    else
                    {
                        if (smallPSpec.SpecificationAttributeOptionId != 0)
                        {
                            productSpecificationAttributes.Add(smallPSpec);
                        }
                    }

                    if (_strokePSpec != null)
                    {
                        productSpecificationAttributes.Add(strokePSpec);
                    }
                    else
                    {
                        if (strokePSpec.SpecificationAttributeOptionId != 0)
                        {
                            productSpecificationAttributes.Add(strokePSpec);
                        }
                    }

                    if (_bigSpec != null)
                    {
                        productSpecificationAttributes.Add(bigSpec);
                    }
                    else
                    {
                        if (bigSpec.SpecificationAttributeOptionId != 0)
                        {
                            productSpecificationAttributes.Add(bigSpec);
                        }
                    }

                    #endregion
                }
                catch (Exception ex)
                {
                    _userActivityService.InsertActivity("UpdateProduct", "编辑商品属性失败", curUser, model.Name);
                    _notificationService.SuccessNotification("编辑商品属性失败:" + ex.Message);
                }
                #endregion

                #region 价格信息
                List<ProductPrice> productPricess = new List<ProductPrice>();
                try
                {
                    //更新商品价格
                    var names = new string[] { "Small", "Stroke", "Big" };
                    foreach (var name in names)
                    {

                        var _unitId = string.IsNullOrEmpty(form[name + "_UnitId"]) ? "0" : form[name + "_UnitId"].ToString();
                        if (_unitId == "0") continue;
                        var _tradePrice = string.IsNullOrEmpty(form[name + "_TradePrice"]) ? "0" : form[name + "_TradePrice"].ToString();
                        var _retailPrice = string.IsNullOrEmpty(form[name + "_RetailPrice"]) ? "0" : form[name + "_RetailPrice"].ToString();
                        var _floorPrice = string.IsNullOrEmpty(form[name + "_FloorPrice"]) ? "0" : form[name + "_FloorPrice"].ToString();
                        var _purchasePrice = string.IsNullOrEmpty(form[name + "_PurchasePrice"]) ? "0" : form[name + "_PurchasePrice"].ToString();
                        var _costPrice = string.IsNullOrEmpty(form[name + "_CostPrice"]) ? "0" : form[name + "_CostPrice"].ToString();
                        var _sALE1 = string.IsNullOrEmpty(form[name + "_SALE1"]) ? "0" : form[name + "_SALE1"].ToString();
                        var _sALE2 = string.IsNullOrEmpty(form[name + "_SALE2"]) ? "0" : form[name + "_SALE2"].ToString();
                        var _sALE3 = string.IsNullOrEmpty(form[name + "_SALE3"]) ? "0" : form[name + "_SALE3"].ToString();

                        int.TryParse(_unitId, out int unitId);
                        decimal.TryParse(_tradePrice, out decimal tradePrice);
                        decimal.TryParse(_retailPrice, out decimal retailPrice);
                        decimal.TryParse(_floorPrice, out decimal floorPrice);
                        decimal.TryParse(_purchasePrice, out decimal purchasePrice);
                        decimal.TryParse(_costPrice, out decimal costPrice);
                        decimal.TryParse(_sALE1, out decimal sALE1);
                        decimal.TryParse(_sALE2, out decimal sALE2);
                        decimal.TryParse(_sALE3, out decimal sALE3);

                        switch (name.ToUpper())
                        {
                            case "SMALL":
                                model.SmallProductPrices.ProductId = product.Id;
                                model.SmallProductPrices.UnitId = unitId;
                                model.SmallProductPrices.TradePrice = tradePrice;
                                model.SmallProductPrices.RetailPrice = retailPrice;
                                model.SmallProductPrices.FloorPrice = floorPrice;
                                model.SmallProductPrices.PurchasePrice = purchasePrice;
                                model.SmallProductPrices.CostPrice = costPrice;
                                model.SmallProductPrices.SALE1 = sALE1;
                                model.SmallProductPrices.SALE2 = sALE2;
                                model.SmallProductPrices.SALE3 = sALE3;
                                break;
                            case "STROKE":
                                model.StrokeProductPrices.ProductId = product.Id;
                                model.StrokeProductPrices.UnitId = unitId;
                                model.StrokeProductPrices.TradePrice = tradePrice;
                                model.StrokeProductPrices.RetailPrice = retailPrice;
                                model.StrokeProductPrices.FloorPrice = floorPrice;
                                model.StrokeProductPrices.PurchasePrice = purchasePrice;
                                model.StrokeProductPrices.CostPrice = costPrice;
                                model.StrokeProductPrices.SALE1 = sALE1;
                                model.StrokeProductPrices.SALE2 = sALE2;
                                model.StrokeProductPrices.SALE3 = sALE3;
                                break;
                            case "BIG":
                                model.BigProductPrices.ProductId = product.Id;
                                model.BigProductPrices.UnitId = unitId;
                                model.BigProductPrices.TradePrice = tradePrice;
                                model.BigProductPrices.RetailPrice = retailPrice;
                                model.BigProductPrices.FloorPrice = floorPrice;
                                model.BigProductPrices.PurchasePrice = purchasePrice;
                                model.BigProductPrices.CostPrice = costPrice;
                                model.BigProductPrices.SALE1 = sALE1;
                                model.BigProductPrices.SALE2 = sALE2;
                                model.BigProductPrices.SALE3 = sALE3;
                                break;
                        }
                    }

                    //获取单位
                    var productPrices = _productService.GetProductPriceByUnit(product);
                    smallProductPrices = productPrices.Item1;
                    strokeProductPrices = productPrices.Item2;
                    bigProductPrices = productPrices.Item3;

                    if (smallProductPrices != null)
                    {
                        model.SmallProductPrices.Id = smallProductPrices.Id;
                        productPricess.Add(model.SmallProductPrices.ToEntity(smallProductPrices));
                    }
                    else
                    {
                        var spp = model.SmallProductPrices.ToEntity<ProductPrice>();
                        if (spp.UnitId != 0)
                        {
                            productPricess.Add(spp);
                        }
                    }

                    if (strokeProductPrices != null)
                    {
                        model.StrokeProductPrices.Id = strokeProductPrices.Id;
                        productPricess.Add(model.StrokeProductPrices.ToEntity(strokeProductPrices));
                    }
                    else
                    {
                        var tpp = model.StrokeProductPrices.ToEntity<ProductPrice>();
                        if (tpp.UnitId != 0)
                        {
                            productPricess.Add(tpp);
                        }
                    }

                    if (bigProductPrices != null)
                    {
                        model.BigProductPrices.Id = bigProductPrices.Id;
                        productPricess.Add(model.BigProductPrices.ToEntity(bigProductPrices));
                    }
                    else
                    {
                        var bpp = model.BigProductPrices.ToEntity<ProductPrice>();
                        if (bpp.UnitId != 0)
                        {
                            productPricess.Add(bpp);
                        }
                    }
                }
                catch (Exception)
                {
                    _userActivityService.InsertActivity("UpdateProduct", "商品价格更新失败", curUser, model.Name);
                }

                #endregion

                #region 层次价格

                var productTierPrices = new List<ProductTierPrice>();
                try
                {
                    //层次价格
                    var tierPricesOlds = _productService.GetProductTierPriceByProductId(model.Id);
 
                    //价格方案
                    var productTierPrices1 = new ProductTierPrice();
                    var productTierPrices2 = new ProductTierPrice();
                    var productTierPrices3 = new ProductTierPrice();
                    var productTierPrices4 = new ProductTierPrice();
                    var productTierPrices6 = new ProductTierPrice();

                    if (tierPricesOlds != null && tierPricesOlds.Count > 0)
                    {
                        //进价
                        productTierPrices1 = tierPricesOlds.Where(p => p.PricesPlanId == 0 && p.PriceTypeId == (int)PriceType.ProductCost).FirstOrDefault();
                        if (productTierPrices1 == null)
                        {
                            productTierPrices1 = new ProductTierPrice();
                        }

                        //批发价格
                        productTierPrices2 = tierPricesOlds.Where(p => p.PricesPlanId == 0 && p.PriceTypeId == (int)PriceType.WholesalePrice).FirstOrDefault();
                        if (productTierPrices2 == null)
                        {
                            productTierPrices2 = new ProductTierPrice();
                        }

                        //零售价格
                        productTierPrices3 = tierPricesOlds.Where(p => p.PricesPlanId == 0 && p.PriceTypeId == (int)PriceType.RetailPrice).FirstOrDefault();
                        if (productTierPrices3 == null)
                        {
                            productTierPrices3 = new ProductTierPrice();
                        }

                        //最低售价
                        productTierPrices4 = tierPricesOlds.Where(p => p.PricesPlanId == 0 && p.PriceTypeId == (int)PriceType.LowestPrice).FirstOrDefault();
                        if (productTierPrices4 == null)
                        {
                            productTierPrices4 = new ProductTierPrice();
                        }

                        //最低售价
                        productTierPrices6 = tierPricesOlds.Where(p => p.PricesPlanId == 0 && p.PriceTypeId == (int)PriceType.CostPrice).FirstOrDefault();
                        if (productTierPrices6 == null)
                        {
                            productTierPrices6 = new ProductTierPrice();
                        }
                    }

                    productTierPrices1.StoreId = curStore.Id;
                    productTierPrices1.ProductId = product.Id;
                    productTierPrices1.PricesPlanId = 0;
                    productTierPrices1.PriceTypeId = (int)PriceType.ProductCost;
                    productTierPrices.Add(productTierPrices1);

                    productTierPrices2.StoreId = curStore.Id;
                    productTierPrices2.ProductId = product.Id;
                    productTierPrices2.PricesPlanId = 0;
                    productTierPrices2.PriceTypeId = (int)PriceType.WholesalePrice;
                    productTierPrices.Add(productTierPrices2);

                    productTierPrices3.StoreId = curStore.Id;
                    productTierPrices3.ProductId = product.Id;
                    productTierPrices3.PricesPlanId = 0;
                    productTierPrices3.PriceTypeId = (int)PriceType.RetailPrice;
                    productTierPrices.Add(productTierPrices3);

                    productTierPrices4.StoreId = curStore.Id;
                    productTierPrices4.ProductId = product.Id;
                    productTierPrices4.PricesPlanId = 0;
                    productTierPrices4.PriceTypeId = (int)PriceType.LowestPrice;
                    productTierPrices.Add(productTierPrices4);

                    productTierPrices6.StoreId = curStore.Id;
                    productTierPrices6.ProductId = product.Id;
                    productTierPrices6.PricesPlanId = 0;
                    productTierPrices6.PriceTypeId = (int)PriceType.CostPrice;
                    productTierPrices.Add(productTierPrices6);

                    foreach (var tierprice in productTierPrices)
                    {
                        var smallUnitPrice = string.IsNullOrEmpty(form[(PriceType)tierprice.PriceTypeId + "_" + tierprice.PricesPlanId + "_SmallUnitPrice"]) ? "0" : form[(PriceType)tierprice.PriceTypeId + "_" + tierprice.PricesPlanId + "_SmallUnitPrice"].ToString();
                        var strokeUnitPrice = string.IsNullOrEmpty(form[(PriceType)tierprice.PriceTypeId + "_" + tierprice.PricesPlanId + "_StrokeUnitPrice"]) ? "0" : form[(PriceType)tierprice.PriceTypeId + "_" + tierprice.PricesPlanId + "_StrokeUnitPrice"].ToString();
                        var bigUnitPrice = string.IsNullOrEmpty(form[(PriceType)tierprice.PriceTypeId + "_" + tierprice.PricesPlanId + "_BigUnitPrice"]) ? "0" : form[(PriceType)tierprice.PriceTypeId + "_" + tierprice.PricesPlanId + "_BigUnitPrice"].ToString();
                        tierprice.SmallUnitPrice = Convert.ToDecimal(smallUnitPrice);
                        tierprice.StrokeUnitPrice = Convert.ToDecimal(strokeUnitPrice);
                        tierprice.BigUnitPrice = Convert.ToDecimal(bigUnitPrice);

                    }

                    //自定义方案
                    var plans = _productService.GetAllPlansByStore(curStore?.Id ?? 0);
                    foreach (var plan in plans)
                    {
                        var productTierPrice = new ProductTierPrice();
                        if (tierPricesOlds != null && tierPricesOlds.Count > 0)
                        {
                            productTierPrice = tierPricesOlds.Where(tp => tp.PricesPlanId == plan.Id && tp.PriceTypeId == (int)PriceType.CustomPlan).FirstOrDefault();
                            if (productTierPrice == null)
                            {
                                productTierPrice = new ProductTierPrice();
                            }
                        }

                        var smallUnitPrice = string.IsNullOrEmpty(form["CustomPlan_" + plan.Id + "_SmallUnitPrice"]) ? "0" : form[PriceType.CustomPlan + "_" + plan.Id + "_SmallUnitPrice"].ToString();
                        if (form["CustomPlan_" + plan.Id + "_SmallUnitPrice"].ToString().Split(',').Length > 1)
                        {
                            smallUnitPrice = smallUnitPrice.Split(',')[1];
                        }

                        var strokeUnitPrice = string.IsNullOrEmpty(form[PriceType.CustomPlan + "_" + plan.Id + "_StrokeUnitPrice"]) ? "0" : form[PriceType.CustomPlan + "_" + plan.Id + "_StrokeUnitPrice"].ToString();
                        if (form[PriceType.CustomPlan + "_" + plan.Id + "_StrokeUnitPrice"].ToString().Split(',').Length > 1)
                        {
                            strokeUnitPrice = strokeUnitPrice.Split(',')[1];
                        }

                        var bigUnitPrice = string.IsNullOrEmpty(form["CustomPlan_" + plan.Id + "_BigUnitPrice"]) ? "0" : form[PriceType.CustomPlan + "_" + plan.Id + "_BigUnitPrice"].ToString();
                        if (form[PriceType.CustomPlan + "_" + plan.Id + "_BigUnitPrice"].ToString().Split(',').Length > 1)
                        {
                            bigUnitPrice = bigUnitPrice.Split(',')[1];
                        }

                        if (productTierPrice != null)
                        {
                            productTierPrice.StoreId = curStore.Id;
                            productTierPrice.ProductId = model.Id;
                            productTierPrice.PricesPlanId = plan.Id;
                            productTierPrice.PriceTypeId = 88;

                            productTierPrice.SmallUnitPrice = Convert.ToDecimal(smallUnitPrice);
                            productTierPrice.StrokeUnitPrice = Convert.ToDecimal(strokeUnitPrice);
                            productTierPrice.BigUnitPrice = Convert.ToDecimal(bigUnitPrice);

                            productTierPrices.Add(productTierPrice);
                        }

                    }

                    model.ProductTierPrices.Clear();

                    foreach (var p in productTierPrices)
                    {
                        model.ProductTierPrices.Add(new ProductTierPriceModel()
                        {
                            Id = p.Id,
                            StoreId = p.StoreId,
                            ProductId = p.ProductId,
                            PricesPlanId = p.PricesPlanId,
                            PriceTypeId = p.PriceTypeId,
                            PriceType = p.PriceType,
                            SmallUnitPrice = p.SmallUnitPrice,
                            StrokeUnitPrice = p.StrokeUnitPrice,
                            BigUnitPrice = p.BigUnitPrice,
                        });
                    }

                }
                catch /*(Exception ex)*/
                {
                    _userActivityService.InsertActivity("UpdateProduct", "层次价格更新失败", curUser, model.Name);
                    //_notificationService.SuccessNotification("层次价格更新失败:" + ex.Message);
                }

                #endregion

                #region 组合商品
                var combination = _productService.GetCombinationByProductId(model.Id);
                try
                {
                    #region 组合

                    if (combination != null)
                    {
                        model.CombinationId = combination.Id;

                        combination.DisplayOrder = 0;
                        combination.ProductId = product.Id;
                        combination.ProductName = product.Name;
                        combination.StoreId = curStore?.Id ?? 0;
                        combination.Enabled = true;
                    }
                    else
                    {
                        combination = new Combination
                        {
                            StoreId = curStore?.Id ?? 0,
                            DisplayOrder = 0,
                            ProductId = product.Id,
                            ProductName = product.Name
                        };
                        combination.StoreId = curStore?.Id ?? 0;
                        combination.Enabled = true;
                    }

                    #endregion
                }
                catch /*(Exception ex)*/
                {
                    _userActivityService.InsertActivity("UpdateProduct", "编辑组合商品失败", curUser, model.Name);
                }
                #endregion

                //RedLock
                string lockKey = string.Format(DCMSCachingDefaults.RedisDataReSubmitKey, Request.GetUrl(), curStore.Id, curUser.Id, CommonHelper.MD5(JsonConvert.SerializeObject(model)));
                var result = await _locker.PerformActionWithLockAsync(lockKey,
                    TimeSpan.FromSeconds(30),
                    TimeSpan.FromSeconds(10),
                    TimeSpan.FromSeconds(1),
                    () => _productService.ProductCreateOrUpdate(curStore.Id, curUser.Id, product.Id, product, productSpecificationAttributes, productPricess, productTierPrices, null, combination, null));

                //activity log
                _userActivityService.InsertActivity("UpdateProduct", "编辑商品成功", curUser, product.Name);
                //_notificationService.SuccessNotification("编辑商品成功");
                return continueEditing ? RedirectToAction("Edit", new { id = product.Id }) : RedirectToAction("List");
            }

            //如果出错了，一下可以继续呈现
            //统计类别 
            var statisticalTypes = _statisticalTypeService.GetAllStatisticalTypess(curStore?.Id ?? 0);
            var types = new List<SelectListItem>();
            statisticalTypes.ToList().ForEach(t =>
            {
                types.Add(new SelectListItem
                {
                    Text = t.Name,
                    Value = t.Id.ToString(),
                });
            });

            model.StatisticalTypes = new SelectList(types, "Value", "Text");

            //获取配置
            var productSetting = _settingService.LoadSetting<ProductSetting>(curStore?.Id ?? 0);

            var smalllists = new List<SelectListItem> { new SelectListItem() { Text = "-无-", Value = "0" } };
            var stroklists = new List<SelectListItem> { new SelectListItem() { Text = "-无-", Value = "0" } };
            var biglists = new List<SelectListItem> { new SelectListItem() { Text = "-无-", Value = "0" } };

            //分类
            model = BindCategoryDropDownList<ProductModel>(model, new Func<int?, int, bool, IList<Category>>(_categoryService.GetAllCategoriesByParentCategoryId), curStore?.Id ?? 0, 0);

            //规格属性
            var smallOptions = _specificationAttributeService.GetSpecificationAttributeOptionsBySpecificationAttribute(curStore?.Id ?? 0, productSetting.SmallUnitSpecificationAttributeOptionsMapping).ToList();
            var strokOptions = _specificationAttributeService.GetSpecificationAttributeOptionsBySpecificationAttribute(curStore?.Id ?? 0, productSetting.StrokeUnitSpecificationAttributeOptionsMapping).ToList();
            var bigOptions = _specificationAttributeService.GetSpecificationAttributeOptionsBySpecificationAttribute(curStore?.Id ?? 0, productSetting.BigUnitSpecificationAttributeOptionsMapping).ToList();

            smallOptions.ForEach(o =>
            {
                smalllists.Add(new SelectListItem() { Text = o.Name, Value = o.Id.ToString() });
            });
            strokOptions.ForEach(o =>
            {
                stroklists.Add(new SelectListItem() { Text = o.Name, Value = o.Id.ToString() });
            });
            bigOptions.ForEach(o =>
            {
                biglists.Add(new SelectListItem() { Text = o.Name, Value = o.Id.ToString() });
            });

            model.SmallUnits = new SelectList(smalllists, "Value", "Text");
            model.StrokeUnits = new SelectList(stroklists, "Value", "Text");
            model.BigUnits = new SelectList(biglists, "Value", "Text");

            //获取单位
             smallProductPrices = _productService.GetProductPriceByProductIdAndUnitId(curStore.Id, model.Id, model.SmallUnitId);
             strokeProductPrices = _productService.GetProductPriceByProductIdAndUnitId(curStore.Id, model.Id, model.StrokeUnitId ?? 0);
             bigProductPrices = _productService.GetProductPriceByProductIdAndUnitId(curStore.Id, model.Id, model.BigUnitId ?? 0);


            model.SmallProductPrices = smallProductPrices != null ? smallProductPrices.ToModel<ProductPriceModel>() : new ProductPriceModel();
            model.StrokeProductPrices = strokeProductPrices != null ? strokeProductPrices.ToModel<ProductPriceModel>() : new ProductPriceModel();
            model.BigProductPrices = bigProductPrices != null ? bigProductPrices.ToModel<ProductPriceModel>() : new ProductPriceModel();

            var smallOption = _specificationAttributeService.GetSpecificationAttributeOptionById(model.SmallUnitId);
            var strokeOption = _specificationAttributeService.GetSpecificationAttributeOptionById(model.StrokeUnitId ?? 0);
            var bigOption = _specificationAttributeService.GetSpecificationAttributeOptionById(model.BigUnitId ?? 0);

            model.smallOption = smallOption.ToModel<SpecificationAttributeOptionModel>();
            model.strokeOption = strokeOption.ToModel<SpecificationAttributeOptionModel>();
            model.bigOption = bigOption.ToModel<SpecificationAttributeOptionModel>();

            model.UnitName = "";
            model.UnitName = string.Format("1{0} = {1}{2}", bigOption != null ? bigOption.Name : "/", model.BigQuantity ?? 0, smallOption != null ? smallOption.Name : "/");
            model.BrandName = _brandService.GetBrandName(curStore.Id, model.BrandId);

            return View(model);
        }

        /// <summary>
        /// 批量删除商品
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        [AuthCode((int)AccessGranularityEnum.ProductArchivesSave)]
        public JsonResult Delete(string ids)
        {

            try
            {
                if (!string.IsNullOrEmpty(ids))
                {
                    int[] sids = ids.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(u => int.Parse(u)).ToArray();
                    var list = _productService.GetProductsByIds(curStore.Id, sids);
                    var HasSoldProductIds = _productService.GetHasSoldProductIds(curStore.Id, int.Parse(ids));
                    foreach (var product in list)
                    {
                        if (product == null)
                        {
                            return Warning("商品信息不存在!");
                        }
                        if (product.StockQuantity > 0)
                        {
                            return Warning("库存数量为空!");
                        }
                        if (HasSoldProductIds != null)
                        {
                            return Warning("该商品已开单!");
                        }

                        product.Status = false;
                        _productService.UpdateProduct(product);

                        //activity log
                        _userActivityService.InsertActivity("DeleteProduct", "商品删除成功", curUser, product.Name);
                    }
                }

                return Successful("商品删除成功");
            }
            catch (Exception ex)
            {
                return Error(ex.Message);
            }

        }
        /// <summary>
        /// 批量匹配商品简称
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult BatchSetMnemonicCode()
        {
            try
            {
                _productService.BatchSetMnemonicCode(curStore.Id);
                return Successful("操作成功");
            }
            catch (Exception ex)
            {
                return Error(ex.Message);
            }
        }

        /// <summary>
        /// 批量禁用商品
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult BatchDisableProducts(int[] ids)
        {
            try
            {
                if (ids.Length > 0)
                {
                    var products = _productService.GetProductsByIds(curStore.Id, ids);
                    products.ForEach(s => { s.Status = false; });
                    _productService.UpdateProducts(products?.ToList());
                }
                return Successful("操作成功");
            }
            catch (Exception ex)
            {
                return Error(ex.Message);
            }
        }

        /// <summary>
        /// 批量解禁商品
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult BatchEnableProducts(int[] ids)
        {
            try
            {
                if (ids.Length > 0)
                {
                    var products = _productService.GetProductsByIds(curStore.Id, ids);
                    products.ForEach(s => { s.Status = true; });
                    _productService.UpdateProducts(products?.ToList());
                }
                return Successful("操作成功");
            }
            catch (Exception ex)
            {
                return Error(ex.Message);
            }
        }

        #region 口味

        [HttpGet]
        public JsonResult FlavorList(int productId)
        {
            var flavors = _productFlavorService.GetProductFlavorsByParentId(productId);
            var gridModel = flavors.Select(x =>
            {
                var model = x.ToModel<ProductFlavorModel>();
                return model;
            }).ToList();

            return Json(new
            {
                total = gridModel.Count(),
                rows = gridModel
            });
        }


        public JsonResult FlavorCreatePopup(int parentId)
        {
            var model = new ProductFlavorModel
            {
                //ProductId = productId
                //注意这里是父商品Id
                ParentId = parentId
            };

            return Json(new
            {
                Success = true,
                RenderHtml = RenderPartialViewToString("CreateFlavor", model)
            });
        }

        [HttpPost]
        public JsonResult FlavorCreatePopup(IFormCollection form)
        {

            //var productId = string.IsNullOrEmpty(form["productId"].ToString()) ? "0" : form["productId"].ToString();
            var parentId = string.IsNullOrEmpty(form["parentId"].ToString()) ? "0" : form["parentId"].ToString();

            var model = new ProductFlavorModel()
            {
                ParentId = int.Parse(parentId),
                Name = string.IsNullOrEmpty(form["Name"].ToString()) ? "0" : form["Name"].ToString(),
                SmallUnitBarCode = string.IsNullOrEmpty(form["SmallUnitBarCode"].ToString()) ? "0" : form["SmallUnitBarCode"].ToString(),
                StrokeUnitBarCode = string.IsNullOrEmpty(form["StrokeUnitBarCode"].ToString()) ? "0" : form["StrokeUnitBarCode"].ToString(),
                BigUnitBarCode = string.IsNullOrEmpty(form["BigUnitBarCode"].ToString()) ? "0" : form["BigUnitBarCode"].ToString(),
                Quantity = int.Parse(string.IsNullOrEmpty(form["Quantity"].ToString()) ? "0" : form["Quantity"].ToString()),
                ProductId = int.Parse(string.IsNullOrEmpty(form["ProductId"].ToString()) ? "0" : form["ProductId"].ToString())
            };

            if (ModelState.IsValid)
            {
                var flavor = model.ToEntity<ProductFlavor>();
                _productFlavorService.InsertProductFlavor(flavor);
                //如果用户输入口味，没有选择商品，则 productId=当前表自增Id
                if (flavor.ProductId == 0)
                {
                    flavor.ProductId = flavor.Id;
                    flavor.StoreId = curStore.Id;
                    flavor.AddType = 0;
                    _productFlavorService.UpdateProductFlavor(flavor);
                }
                else if (flavor.ProductId > 0)
                {
                    flavor.AddType = 1;
                    flavor.StoreId = curStore.Id;
                    _productFlavorService.UpdateProductFlavor(flavor);
                }
                return Successful("添加成功");
            }

            return Warning("添加成功");
        }

        public JsonResult FlavorEditPopup(int id)
        {
            var flavor = _productFlavorService.GetProductFlavorById(id);
            if (flavor == null)
            {
                return Warning("口味信息不存在!");
            }

            var model = flavor.ToModel<ProductFlavorModel>();
            if (flavor.AddType == 1)
            {
                var product = _productService.GetProductById(curStore.Id, flavor.ProductId);
                model.ProductName = product == null ? "" : product.Name;
            }

            return Json(new
            {
                Success = true,
                RenderHtml = RenderPartialViewToString("EditFlavor", model)
            });
        }

        [HttpPost]
        public JsonResult FlavorEditPopup(IFormCollection form)
        {
            var id = string.IsNullOrEmpty(form["Id"].ToString()) ? "0" : form["Id"].ToString();
            //var productId = string.IsNullOrEmpty(form["ProductId"].ToString()) ? "0" : form["ProductId"].ToString();

            var flavor = _productFlavorService.GetProductFlavorById(int.Parse(id));
            if (flavor == null)
            {
                return Warning("口味信息不存在!");
            }

            if (ModelState.IsValid)
            {
                flavor.Name = string.IsNullOrEmpty(form["Name"].ToString()) ? "" : form["Name"].ToString();
                flavor.SmallUnitBarCode = string.IsNullOrEmpty(form["SmallUnitBarCode"].ToString()) ? "0" : form["SmallUnitBarCode"].ToString();
                flavor.StrokeUnitBarCode = string.IsNullOrEmpty(form["StrokeUnitBarCode"].ToString()) ? "0" : form["StrokeUnitBarCode"].ToString();
                flavor.BigUnitBarCode = string.IsNullOrEmpty(form["BigUnitBarCode"].ToString()) ? "0" : form["BigUnitBarCode"].ToString();
                flavor.Quantity = int.Parse(string.IsNullOrEmpty(form["Quantity"].ToString()) ? "0" : form["Quantity"].ToString());

                _productFlavorService.UpdateProductFlavor(flavor);

                return Successful("编辑成功");
            }
            return Warning("编辑失败");
        }


        public JsonResult FlavorDelete(int flavorId)
        {

            var flavor = _productFlavorService.GetProductFlavorById(flavorId);
            if (flavor == null)
            {
                throw new ArgumentException("No flavor attr found with the flavor id");
            }

            _productFlavorService.DeleteProductFlavor(flavor);

            return Successful("删除成功");
        }



        public JsonResult GetFlavorByProductId(string prductId)
        {

            if (string.IsNullOrEmpty(prductId))
            {
                throw new ArgumentNullException("prductId");
            }

            var flavors = _productFlavorService.GetProductFlavorsByProductId(Convert.ToInt32(prductId));
            var result = (from o in flavors
                          select new { id = o.Id, name = o.Name }).ToList();
            return Json(result);
        }

        #endregion

        #region 组合商品

        [HttpGet]
        public JsonResult CombinationsList(int? productId, int? combinationId)
        {
            var productCombinations = _productService.GetAllProductCombinationsByCombinationId(curStore?.Id ?? 0, combinationId);

            var gridModel = productCombinations.Select(x =>
            {
                var model = x.ToModel<ProductCombinationModel>();
                var option = _specificationAttributeService.GetSpecificationAttributeOptionById(x.UnitId);
                model.UnitName = option != null ? option.Name : x.UnitId.ToString();
                model.Stock = 0;//库存
                return model;
            }).ToList();

            return Json(new
            {
                total = gridModel.Count(),
                rows = gridModel
            });
        }

        public JsonResult CombinationsCreatePopup(int productId)
        {
            var model = new ProductCombinationModel
            {
                ProductId = productId
            };

            var combination = _productService.GetCombinationByProductId(productId);
            if (combination != null)
            {
                model.CombinationId = combination.Id;
            }

            //获取配置
            var productSetting = _settingService.LoadSetting<ProductSetting>(curStore?.Id ?? 0);
            var optionlists = new List<SelectListItem>();

            //规格属性
            var smallOptions = _specificationAttributeService.GetSpecificationAttributeOptionsBySpecificationAttribute(curStore?.Id ?? 0, productSetting.SmallUnitSpecificationAttributeOptionsMapping).ToList();
            var strokOptions = _specificationAttributeService.GetSpecificationAttributeOptionsBySpecificationAttribute(curStore?.Id ?? 0, productSetting.StrokeUnitSpecificationAttributeOptionsMapping).ToList();
            var bigOptions = _specificationAttributeService.GetSpecificationAttributeOptionsBySpecificationAttribute(curStore?.Id ?? 0, productSetting.BigUnitSpecificationAttributeOptionsMapping).ToList();

            smallOptions.ForEach(o =>
            {
                optionlists.Add(new SelectListItem() { Text = o.Name, Value = o.Id.ToString() });
            });
            strokOptions.ForEach(o =>
            {
                optionlists.Add(new SelectListItem() { Text = o.Name, Value = o.Id.ToString() });
            });
            bigOptions.ForEach(o =>
            {
                optionlists.Add(new SelectListItem() { Text = o.Name, Value = o.Id.ToString() });
            });

            List<SelectListItem> units = new List<SelectListItem>
            {
                new SelectListItem() { Text = "请选择", Value = "0" }
            };
            model.Units = new SelectList(units, "Value", "Text");

            return Json(new
            {
                Success = true,
                RenderHtml = RenderPartialViewToString("CreateCombination", model)
            });
        }

        [HttpPost]
        public JsonResult CombinationsCreatePopup(IFormCollection form)
        {

            var productId = string.IsNullOrEmpty(form["ProductId"].ToString()) ? "0" : form["ProductId"].ToString();
            var productName = string.IsNullOrEmpty(form["ProductName"].ToString()) ? "" : form["ProductName"].ToString();
            var combinationId = string.IsNullOrEmpty(form["CombinationId"].ToString()) ? "0" : form["CombinationId"].ToString();
            var displayOrder = string.IsNullOrEmpty(form["DisplayOrder"].ToString()) ? "0" : form["DisplayOrder"].ToString();
            var quantity = string.IsNullOrEmpty(form["Quantity"].ToString()) ? "0" : form["Quantity"].ToString();
            var unitId = string.IsNullOrEmpty(form["UnitId"].ToString()) ? "0" : form["UnitId"].ToString();
            if (ModelState.IsValid)
            {

                var model = new ProductCombinationModel()
                {
                    ProductId = int.Parse(productId),
                    ProductName = productName,
                    CombinationId = int.Parse(combinationId),
                    DisplayOrder = int.Parse(displayOrder),
                    Quantity = int.Parse(quantity),
                    StoreId = curStore?.Id ?? 0,
                    UnitId = int.Parse(unitId)
                };
                var productCombination = model.ToEntity<ProductCombination>();
                _productService.InsertProductCombination(productCombination);

                return Successful("添加成功");
            }
            return Warning("添加成功");
        }


        public JsonResult CombinationsEditPopup(int id)
        {

            var model = new ProductCombinationModel();

            var productCombination = _productService.GetProductCombinationById(id);

            if (productCombination != null)
            {
                var combination = _productService.GetCombinationById(productCombination.CombinationId);
                model = productCombination.ToModel<ProductCombinationModel>();
                model.CombinationId = productCombination != null ? productCombination.CombinationId : 0;
            }

            //获取配置
            var productSetting = _settingService.LoadSetting<ProductSetting>(curStore?.Id ?? 0);
            var optionlists = new List<SelectListItem>();

            //规格属性
            var smallOptions = _specificationAttributeService.GetSpecificationAttributeOptionsBySpecificationAttribute(curStore?.Id ?? 0, productSetting.SmallUnitSpecificationAttributeOptionsMapping).ToList();
            var strokOptions = _specificationAttributeService.GetSpecificationAttributeOptionsBySpecificationAttribute(curStore?.Id ?? 0, productSetting.StrokeUnitSpecificationAttributeOptionsMapping).ToList();
            var bigOptions = _specificationAttributeService.GetSpecificationAttributeOptionsBySpecificationAttribute(curStore?.Id ?? 0, productSetting.BigUnitSpecificationAttributeOptionsMapping).ToList();

            smallOptions.ForEach(o =>
            {
                optionlists.Add(new SelectListItem() { Text = o.Name, Value = o.Id.ToString() });
            });
            strokOptions.ForEach(o =>
            {
                optionlists.Add(new SelectListItem() { Text = o.Name, Value = o.Id.ToString() });
            });
            bigOptions.ForEach(o =>
            {
                optionlists.Add(new SelectListItem() { Text = o.Name, Value = o.Id.ToString() });
            });

            //根据用户选择的商品，加载当前选择商品的单位
            //var distincts = from c in optionlists
            //                group c by c.Text
            //                into cGroup
            //                orderby cGroup.Key
            //                select cGroup.FirstOrDefault();
            //model.Units = new SelectList(distincts, "Value", "Text");


            if (productCombination != null)
            {
                Product product = _productService.GetProductById(curStore.Id, productCombination.ProductId);
                if (product != null)
                {
                    Dictionary<string, int> dic = product.GetProductUnits(_productService, _specificationAttributeService);
                    if (dic != null && dic.Count > 0)
                    {
                        List<SelectListItem> selectListItems = new List<SelectListItem>();
                        dic.ToList().ForEach(d =>
                        {
                            selectListItems.Add(new SelectListItem() { Text = d.Key.ToString(), Value = d.Value.ToString() });
                        });
                        model.Units = new SelectList(selectListItems, "Value", "Text");
                    }
                }
            }
            if (model.Units == null)
            {
                List<SelectListItem> units = new List<SelectListItem>
                {
                    new SelectListItem() { Text = "请选择", Value = "0" }
                };
                model.Units = new SelectList(units, "Value", "Text");
            }

            return Json(new
            {
                Success = true,
                RenderHtml = RenderPartialViewToString("EditCombination", model)
            });
        }

        [HttpPost]
        public JsonResult CombinationsEditPopup(IFormCollection form)
        {

            var cId = string.IsNullOrEmpty(form["Id"].ToString()) ? "0" : form["Id"].ToString();
            var productId = string.IsNullOrEmpty(form["ProductId"].ToString()) ? "0" : form["ProductId"].ToString();
            var productName = string.IsNullOrEmpty(form["ProductName"].ToString()) ? "" : form["ProductName"].ToString();
            var combinationId = string.IsNullOrEmpty(form["CombinationId"].ToString()) ? "0" : form["CombinationId"].ToString();
            var displayOrder = string.IsNullOrEmpty(form["DisplayOrder"].ToString()) ? "0" : form["DisplayOrder"].ToString();
            var quantity = string.IsNullOrEmpty(form["Quantity"].ToString()) ? "0" : form["Quantity"].ToString();
            var unitId = string.IsNullOrEmpty(form["UnitId"].ToString()) ? "0" : form["UnitId"].ToString();

            if (ModelState.IsValid)
            {
                var productCombination = _productService.GetProductCombinationById(int.Parse(cId));
                productCombination.ProductId = int.Parse(productId);
                productCombination.ProductName = productName;
                productCombination.CombinationId = int.Parse(combinationId);
                productCombination.DisplayOrder = int.Parse(displayOrder);
                productCombination.Quantity = int.Parse(quantity);
                productCombination.StoreId = curStore?.Id ?? 0;
                productCombination.UnitId = int.Parse(unitId);
                _productService.UpdateProductCombination(productCombination);

                return Successful("编辑成功");
            }
            return Warning("编辑成功");
        }

        public JsonResult CombinationsDelete(int comId)
        {
            var productCombination = _productService.GetProductCombinationById(comId);
            if (productCombination == null)
            {
                throw new ArgumentException("No productCombination attr found with the productCombination id");
            }

            _productService.DeleteProductCombination(productCombination);

            return Successful("删除成功");
        }

        /// <summary>
        /// 异步搜索组合商品
        /// </summary>
        /// <param name="productId">商品Id</param>
        /// <param name="targetDoms">目标节点</param>
        /// <param name="index">如果在表格行内选择搜索商品，则需要获取当前行索引值</param>
        /// <returns></returns>
        public JsonResult AsyncSearchComSelectPopup(int? pageIndex = 0, int? wareHouseId = null)
        {
            var model = new CombinationListModel();
            try
            {
                var combinations = _productService.GetAllHaveSubProductsCombinationsByStore(curStore?.Id ?? 0);

                var comLists = combinations.Select(c =>
                {
                    var m = c.ToModel<CombinationModel>();
                    var allProducts = _productService.GetProductsByIds(curStore.Id, c.ProductCombinations.Select(pr => pr.ProductId).Distinct().ToArray());
                    var allOptions = _specificationAttributeService.GetSpecificationAttributeOptionByIds(curStore.Id, allProducts.GetProductBigStrokeSmallUnitIds());

                    m.SubProducts = c.ProductCombinations.Select(s =>
                    {
                        var sm = s.ToModel<ProductCombinationModel>();
                        var option = _specificationAttributeService.GetSpecificationAttributeOptionById(s.UnitId);
                        sm.UnitName = option != null ? option.Name : s.UnitId.ToString();

                        var product = allProducts.Where(ap => ap.Id == sm.ProductId).FirstOrDefault();
                        if (product != null)
                        {
                            //成本价
                            sm.CostPrice = _purchaseBillService.GetReferenceCostPrice(product.StoreId, sm.ProductId, sm.UnitId);

                            //库存
                            if (wareHouseId > 0)
                            {
                                sm.Stock = _stockService.GetCurrentStock(curStore.Id, wareHouseId, product.Id)?.UsableQuantity??0;
                            }
                            else
                            {
                                sm.Stock = _stockService.GetCurrentStock(curStore.Id, 0, product.Id)?.UsableQuantity ?? 0;
                            }
                           
                            //库存显示
                            sm.StockQuantityConversion = product.GetConversionFormat(allOptions, product.SmallUnitId, sm.Stock);

                        }
                        return sm;
                    }).ToList();

                    //将主商品的信息放到后面查询，如果没有子商品，则不用查询
                    if (m.SubProducts != null && m.SubProducts.Count > 0)
                    {
                        //主商品成本
                        Product mainProduct = _productService.GetProductById(curStore.Id, m.ProductId);
                        //m.ProductCost = _purchaseBillService.GetReferenceCostPrice(mainProduct.StoreId, mainProduct.Id, mainProduct.SmallUnitId);
                        if (mainProduct != null)
                        {
                            m.ProductCost = _purchaseBillService.GetReferenceCostPrice(mainProduct.StoreId, mainProduct.Id, mainProduct.SmallUnitId);
                        }
                    }

                    m.JSONData = JsonConvert.SerializeObject(m.SubProducts);

                    return m;
                }).Where(c => c.SubProducts.Count > 0).ToList();

                model.Items = comLists;
                model.PagingFilteringContext.LoadPagedList(new PagedList<CombinationModel>(comLists, pageIndex.Value, 30));

                return Json(new
                {
                    Success = true,
                    RenderHtml = RenderPartialViewToString("AsyncSearchCom", model)
                });
            }
            catch (Exception)
            {
                return null;
            }


        }


        #endregion

        #region 导入数据
        //DCMS.Services.ExportImport  下 

        [HttpPost]
        public IActionResult Import(IFormCollection form)
        {
            var file = form.Files["file"];
            var categoryId = form["hidCategoryId"];
            string path = "App_Data/TempUploads/" + file.FileName;

            using (var fs = new FileStream(path, FileMode.Create))
            {
                file.CopyToAsync(fs);
                int.TryParse(categoryId, out int a);
                _importManager.ImportProductsFromXlsx(fs, curStore.Id, a);
                fs.Flush();
            }

            return RedirectToAction("List");
        }

        /// <summary>
        /// 导出
        /// </summary>
        /// <param name="terminalId"></param>
        /// <param name="billSourceType"></param>
        /// <returns></returns>
        [HttpGet]
        [AuthCode((int)AccessGranularityEnum.ProductArchivesExport)]
        public FileResult Export(int? storeId, int[] excludeIds, string key = "", int categoryId = 0, int pageIndex = 0, int pageSize = 10, int wareHouseId = 0, bool stockQtyMoreThan = true, int includeProductDetail = 1, int terminalId = 0, int productStatus = 2)
        {

            #region 查询导出数据

            IList<Product> products = new List<Product>();
            products = _productService.SearchProducts(curStore?.Id ?? 0,
                              excludeIds,
                              categoryId == 0 ? new int[] { } : new int[] { categoryId },
                              stockQtyMoreThan,
                              null,
                              null,
                              key: key,
                              includeSpecificationAttributes: true,
                              includeVariantAttributes: false,
                              includeVariantAttributeCombinations: false,
                              includePictures: false,
                              includeTierPrices: true,
                              productStatus: productStatus);
            #endregion

            #region 导出
            var ms = _exportManager.ExportProductsToXlsx(products);
            if (ms != null)
            {
                return File(ms, "application/vnd.ms-excel", "商品档案.xlsx");
            }
            else
            {
                return File(new MemoryStream(), "application/vnd.ms-excel", "商品档案.xlsx");
            }
            #endregion

        }

        //public IActionResult DownLoad()
        //{
        //    var addrUrl = "App_Data/TempUploads/Product.xlsx";
        //    var stream = System.IO.File.OpenRead(addrUrl);
        //    //string fileExt = GetFileExt(file);
        //    //获取文件的ContentType
        //    var provider = new FileExtensionContentTypeProvider();
        //    var memi = provider.Mappings["Product.xlsx"];
        //    return File(stream, memi, Path.GetFileName(addrUrl));
        //}
        #endregion

        /// <summary>
        /// 赠品选择
        /// </summary>
        /// <returns></returns>
        public JsonResult AsyncGiveQuotaSelectPopup(int? channelId, int? terminalId, int? businessUserId, int? wareHouseId)
        {
            var model = new ProductListModel
            {
                ChannelId = channelId ?? 0,
                TerminalId = terminalId ?? 0,
                BusinessUserId = businessUserId ?? 0,
                WareHouseId = wareHouseId ?? 0
            };
            return Json(new
            {
                Success = true,
                RenderHtml = RenderPartialViewToString("AsyncGiveQuotaSelect", model)
            });
        }

        public JsonResult AsyncSearchSelectFlavorPopup(int index = 0, string target = "", string targetForm = "", int? productId = null)
        {
            var model = new ProductFlavorListModel();

            if (productId.HasValue)
            {
                model.ParentId = productId ?? 0;
            }

            model.TargetForm = targetForm;
            //需要获取当前行索引值
            model.RowIndex = index;
            model.Target = target;

            return Json(new
            {
                Success = true,
                RenderHtml = RenderPartialViewToString("AsyncFlavorSelect", model)
            });
        }

        /// <summary>
        /// 异步获商品口味
        /// </summary>
        /// <param name="campaignId"></param>
        /// <returns></returns>
        public async Task<JsonResult> AsyncProductFlavors(string key, int parentId = 0, int pageSize = 10, int pageIndex = 0)
        {
            return await Task.Run(() =>
            {
                ProductFlavorListModel model = new ProductFlavorListModel();
                var list = _productFlavorService.GetProductFlavors(key, parentId, pageIndex, pageSize);
                model.PagingFilteringContext.LoadPagedList(list);
                model.Items = list.Select(s =>
                {
                    var m = s.ToModel<ProductFlavorModel>();
                    return m;
                }).ToList();

                return Json(new
                {
                    total = model.Items.Count(),
                    rows = model.Items
                });
            });
        }

        #region 修改商品档案的 名称及助记码
        private void UpdateProductNameAndMnemonicCode(Product entity, ProductModel model) 
        {
            try
            {
                entity.Name = model.Name;
                entity.MnemonicCode = model.MnemonicCode;
                _productService.UpdateProduct(entity);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion
    }
}
