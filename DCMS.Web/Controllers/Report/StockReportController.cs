using DCMS.Core;
using DCMS.Core.Caching;
using DCMS.Core.Domain.Products;
using DCMS.Core.Domain.Terminals;
using DCMS.Core.Domain.WareHouses;
using DCMS.Services.Common;
using DCMS.Services.ExportImport;
using DCMS.Services.Finances;
using DCMS.Services.Logging;
using DCMS.Services.Messages;
using DCMS.Services.Products;
using DCMS.Services.Report;
using DCMS.Services.Terminals;
using DCMS.Services.Users;
using DCMS.Services.WareHouses;
using DCMS.ViewModel.Models.Finances;
using DCMS.ViewModel.Models.Products;
using DCMS.ViewModel.Models.WareHouses;
using DCMS.Web.Framework.Mvc.Filters;
using DCMS.Web.Infrastructure.Mapper.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace DCMS.Web.Controllers
{
    /// <summary>
    /// 用于库存表管理
    /// </summary>
    public class StockReportController : BasePublicController
    {
        private readonly IUserService _userService;
        private readonly IWareHouseService _wareHouseService;
        private readonly IBillConvertService _billConvertService;
        
        private readonly IProductService _productService;
        private readonly ISpecificationAttributeService _specificationAttributeService;
        private readonly ICategoryService _productCategoryService;
        private readonly IBrandService _brandService;
        private readonly IStockReportService _stockReportService;
        private readonly IStockService _stockService;
        private readonly IDistrictService _districtService;
        private readonly IChannelService _channelService;
        private readonly IRankService _rankService;
        private readonly IExportManager _exportManager;
        private readonly IClosingAccountsService _closingAccountsService;

        public StockReportController(
            IStoreContext storeContext,
            INotificationService notificationService,
            IWareHouseService wareHouseService,
            IBillConvertService billConvertService,
            IProductService productService,
            ISpecificationAttributeService specificationAttributeService,
            ICategoryService productCategoryService,
           
            IBrandService brandService,
            IStockReportService stockReportService,
            IStockService stockService,
            IUserService userService,
            IChannelService channelService,
            IDistrictService districtService,
            IRankService rankService,
            IExportManager exportManager,
            ILogger loggerService,
            IClosingAccountsService closingAccountsService,
            IWorkContext workContext
            ) : base(workContext, loggerService, storeContext, notificationService)
        {
            _userService = userService;
            _wareHouseService = wareHouseService;
            _billConvertService = billConvertService;
            
            _productService = productService;
            _specificationAttributeService = specificationAttributeService;
            _productCategoryService = productCategoryService;
            _brandService = brandService;
            _stockReportService = stockReportService;
            _stockService = stockService;
            _channelService = channelService;
            _districtService = districtService;
            _rankService = rankService;
            _exportManager = exportManager;
            _closingAccountsService = closingAccountsService;
        }

        public IActionResult Index()
        {
            return RedirectToAction("List");
        }

        #region 库存表

        /// <summary>
        /// 库存表
        /// </summary>
        [HttpGet]
        [AuthCode((int)AccessGranularityEnum.StockReportingView)]
        public IActionResult List(int? wareHouseId, int? categoryId, int? productId, string productName, int? brandId, bool? status, int? maxStock, bool? showZeroStack, int pagenumber = 0)
        {
            if (pagenumber > 0)
            {
                pagenumber -= 1;
            }

            var categories = _productCategoryService.BindCategoryList(curStore.Id).ToList();
            //var wareHouses = _wareHouseService.BindWareHouseList(curStore.Id).ToList();
            var brands = _brandService.BindBrandList(curStore.Id).ToList();

            var model = new StockReportListModel
            {
                //仓库
                WareHouses = BindWareHouseSelection(_wareHouseService.BindWareHouseList, curStore, null, curUser.Id),
                WareHouseId = wareHouseId ?? null,

                Categories = BindCategorySelection(categories),
                CategoryId = categoryId ?? null,

                ProductId = productId ?? 0,
                ProductName = productName,

                Brands = BindBrandSelection(brands),
                BrandId = brandId ?? null,

                Status = status,
                MaxStock = maxStock,
                ShowZeroStack = showZeroStack
            };


            //获取商品库存信息
            var productStocks = _stockReportService.GetStockReportProduct(curStore.Id,
                wareHouseId,
                categoryId,
                productId,
                productName,
                brandId,
                status,
                maxStock,
                showZeroStack, 
                pagenumber, 
                50);


            foreach (var item in productStocks)
            {
                item.CurrentQuantityPart = Pexts.StockQuantityFormat(item.CurrentQuantity, item.StrokeQuantity ?? 0, item.BigQuantity ?? 0);
                item.UsableQuantityPart = Pexts.StockQuantityFormat(item.UsableQuantity, item.StrokeQuantity ?? 0, item.BigQuantity ?? 0);
                item.OrderQuantityPart = Pexts.StockQuantityFormat(item.OrderQuantity, item.StrokeQuantity ?? 0, item.BigQuantity ?? 0);
            }

            model.PagingFilteringContext.LoadPagedList(productStocks);
            model.Items = productStocks;

            return View(model);
        }


        [HttpGet]
        public JsonResult AsyncPopupWindow(int categoryId, int? productId)
        {
            var model = new StockProductModel();
            model.ProductId = productId ?? 0;
            model.CategoryId = categoryId;
            return Json(new
            {
                Success = true,
                RenderHtml = RenderPartialViewToString("AsyncPopupWindow", model)
            });
        }

        /// <summary>
        /// 获取商品出入库记录
        /// </summary>
        /// <param name="store"></param>
        /// <param name="productId"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<JsonResult> AsyncStockInOutRecords(int? store, int? productId, int pageIndex = 0, int pageSize = 20)
        {
            return await Task.Run(() =>
            {

                var reports = _stockReportService.AsyncStockInOutRecords(store ?? curStore.Id, productId, pageIndex, pageSize);
                return Json(new
                {
                    Success = true,
                    total = reports.Item2,
                    rows = reports.Item1
                });
            });
        }


        /// <summary>
        /// 获取商品出入库流水
        /// </summary>
        /// <param name="store"></param>
        /// <param name="productId"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<JsonResult> AsyncStockFlows(int? store, int? productId, int pageIndex = 0, int pageSize = 20)
        {
            return await Task.Run(() =>
            {
                var reports = _stockReportService.AsyncStockFlows(store ?? curStore.Id, productId, pageIndex, pageSize);
                return Json(new
                {
                    Success = true,
                    total = reports.Item2,
                    rows = reports.Item1
                });
            });
        }


        [NonAction]
        private List<StockCategoryTree> GetCategoryTreeV2(int? store, int? wareHouseId, int Id, IList<StockReportProduct> productStocks, List<int> categories, IList<Category> allPerentList)
        {
            List<StockCategoryTree> trees = new List<StockCategoryTree>();
            var perentList = allPerentList.Where(ap => ap.ParentId == Id).ToList();
            if (perentList != null && perentList.Count > 0)
            {
                foreach (var b in perentList)
                {
                    List<StockCategoryTree> tempList = GetCategoryTreeV2(store.Value, wareHouseId, b.Id, productStocks, categories, allPerentList);
                    var node = new StockCategoryTree
                    {
                        Visible = categories.Contains(b.Id),
                        StockCategory = new StockCategoryModel()
                        {
                            Selected = categories != null ? categories.Contains(b.Id) : false,
                            Id = b.Id,
                            Name = b.Name,
                            Status = b.Status,
                            OrderNo = b.OrderNo,
                            BrandId = b.BrandId ?? 0,
                            BrandName = b.BrandName,
                            Products = productStocks.Select(s =>
                            {
                                return new ProductModel()
                                {
                                    Name = s.ProductName,
                                    ProductName = s.ProductName,
                                    CategoryId = s.CategoryId,
                                    CategoryName = s.CategoryName,
                                    BrandId = s.BrandId,
                                    BrandName = s.BrandName
                                };
                            }).ToList()
                        },
                        Children = new List<StockCategoryTree>()
                    };

                    if (tempList != null && tempList.Count > 0)
                    {
                        node.Children = tempList;
                    }

                    trees.Add(node);

                }
            }
            return trees;
        }

        /// <summary>
        /// 库存表导出
        /// </summary>
        /// <param name="wareHouseId"></param>
        /// <param name="categoryId"></param>
        /// <param name="productId"></param>
        /// <param name="productName"></param>
        /// <param name="brandId"></param>
        /// <param name="status"></param>
        /// <param name="maxStock"></param>
        /// <param name="showZeroStack"></param>
        /// <param name="pagenumber"></param>
        /// <returns></returns>
        [AuthCode((int)AccessGranularityEnum.StockReportingExport)]
        public FileResult ExportList(int? wareHouseId, int? categoryId, int? productId, string productName, int? brandId, bool? status, int? maxStock, bool? showZeroStack, int pagenumber = 0)
        {

            #region 查询导出数据

            var sqlDatas = _stockReportService.GetStockReportProduct(curStore?.Id ?? 0,
                wareHouseId,
                categoryId,
                productId,
                productName,
                brandId,
                status,
                maxStock,
                showZeroStack, 
                pagenumber,
                50);
            #endregion

            #region 导出
            var ms = _exportManager.ExportStockListToXlsx(sqlDatas);
            if (ms != null)
            {
                return File(ms, "application/vnd.ms-excel", "库存表.xlsx");
            }
            else
            {
                return File(new MemoryStream(), "application/vnd.ms-excel", "库存表.xlsx");
            }
            #endregion
        }

        /// <summary>
        /// 生成期初导入模板
        /// </summary>
        /// <param name="wareHouseId"></param>
        /// <param name="categoryId"></param>
        /// <param name="productId"></param>
        /// <param name="productName"></param>
        /// <param name="brandId"></param>
        /// <param name="status"></param>
        /// <param name="maxStock"></param>
        /// <param name="showZeroStack"></param>
        /// <param name="pagenumber"></param>
        /// <returns></returns>
        [AuthCode((int)AccessGranularityEnum.StockReportingExport)]
        public FileResult ExportGenerateList(int? wareHouseId, int? categoryId, int? productId, string productName, int? brandId, bool? status, int? maxStock, bool? showZeroStack, int pagenumber = 0)
        {

            #region 查询导出数据

            var sqlDatas = _stockReportService.GetStockReportProduct(curStore?.Id ?? 0,
                wareHouseId,
                categoryId,
                productId,
                productName,
                brandId,
                status,
                maxStock,
                showZeroStack, pagenumber, 50);
            #endregion

            #region 导出
            var ms = _exportManager.ExportGenerateStockListToXlsx(sqlDatas);
            if (ms != null)
            {
                return File(ms, "application/vnd.ms-excel", "库存信息.xlsx");
            }
            else
            {
                return File(new MemoryStream(), "application/vnd.ms-excel", "库存信息.xlsx");
            }
            #endregion
        }

        #endregion

        #region 库存变化汇总表(汇总)
        /// <summary>
        /// 库存变化汇总表(汇总)
        /// </summary>
        /// <param name="productId"></param>
        /// <param name="brandId"></param>
        /// <param name="wareHouseId"></param>
        /// <param name="priceType"></param>
        /// <param name="unitType"></param>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <param name="pagenumber"></param>
        /// <returns></returns>
        [HttpGet]
        [AuthCode((int)AccessGranularityEnum.StockChangeView)]
        public IActionResult ChangeSummary(int? productId, string productName, int? brandId, int? wareHouseId, int? priceType, int? unitType, DateTime? startTime = null, DateTime? endTime = null, int pagenumber = 0)
        {

            var model = new StockChangeSummaryListModel
            {
                ProductId = productId ?? 0,
                ProductName = productName,

                //仓库
                WareHouses = BindWareHouseSelection(_wareHouseService.BindWareHouseList, curStore,null,0),
                WareHouseId = wareHouseId ?? null,

                Brands = BindBrandSelection(_brandService.BindBrandList, curStore),
                BrandId = brandId ?? null,

                StartTime = startTime ?? DateTime.Parse(DateTime.Now.ToString("yyyy-MM-01 00:00:00")),
                EndTime = endTime ?? DateTime.Now.AddDays(1),
                PriceType = priceType ?? null,
                UnitType = unitType ?? null
            };

            if (pagenumber > 0)
            {
                pagenumber -= 1;
            }

            var products = _stockReportService.GetAllStockChangeSummary(curStore?.Id ?? 0,
               wareHouseId,
               productId,
               productName,
               brandId,
               model.StartTime,
               model.EndTime);

            #region 查询需要关联其他表的数据
            var allProducts = _productService.GetProductsByIds(curStore.Id, products.Select(sd => sd.ProductId).Distinct().ToArray());
            var allOptions = _specificationAttributeService.GetSpecificationAttributeOptionByIds(curStore.Id, allProducts.GetProductBigStrokeSmallUnitIds());
            var allProductPrices = _productService.GetProductPricesByProductIds(curStore.Id, products.Select(sd => sd.ProductId).Distinct().ToArray());
            var allProductStocks = _stockService.GetAllStocksByProductIds(curStore == null ? 0 : curStore.Id, products.Select(sd => sd.ProductId).Distinct().ToArray());
            #endregion

            products.ToList().ForEach(change =>
            {
                var p = allProducts.Where(ap => ap.Id == change.ProductId).FirstOrDefault();
                if (p != null)
                {
                    //获取规格
                    var option = p.GetProductUnit(allOptions, allProductPrices);

                    var stock = allProductStocks.Where(s => s.WareHouseId == wareHouseId && s.ProductId == p.Id).FirstOrDefault();
                    if (priceType != null)
                    {
                        if (priceType.Value == 0)
                        {
                            //按照商品档案中预设的批发价进行计算。
                            change.Price = option.smallPrice == null ? 0 : option.smallPrice.TradePrice ?? 0;
                        }
                        else if (priceType.Value == 1)
                        {
                            //按照现在此商品在库存中的平均价格计算。
                            if (stock != null)
                            {
                                change.Price = (stock.UsableQuantity * (option.smallPrice == null ? 0 : option.smallPrice.CostPrice)) / stock.UsableQuantity ?? 1;
                            }
                        }
                        else if (priceType.Value == 2)
                        {
                            //按照商品档案中预设的进价进行计算。
                            change.Price = option.smallPrice == null ? 0 : option.smallPrice.PurchasePrice ?? 0;
                        }
                    }

                    //change.PriceName = change.Price + "/" + option.smallOption.Name;
                    //change.InitialAmount = change.Price * change.InitialQuantity;
                    //change.EndAmount = change.Price * change.EndQuantity;
                    //change.UnitName = option.smallOption.Name;
                    //if (unitType != null)
                    //{
                    decimal price = 0;
                    var smailPrice = _productService.GetProductPriceByProductIdAndUnitId(curStore.Id, p.Id, p.SmallUnitId);
                    if (smailPrice != null) 
                    {
                        price = smailPrice.CostPrice ?? 0;
                    }
                    var initialAmount = price * change.InitialQuantity;
                    var endAmount = price * change.EndQuantity;
                    change.InitialAmount = price * change.InitialQuantity;
                    change.EndAmount = price * change.EndQuantity;
                    change.GiftAmount = price * change.GiftQuantity ?? 0;
                    change.CurrentVolumeAmount = price * change.CurrentVolumeQuantity;
                    change.CurrentLossesAmount = price * change.CurrentLossesQuantity;

                    if (unitType == null || unitType.Value == 0)
                    {
                                    
                        //基本单位
                        change.PriceName = smailPrice.CostPrice + "/" + option.smallOption.Name;
                        //单位转换
                        change.UnitConversion = option.smallOption.UnitConversion;
                        //单位名称
                        change.UnitName = option.smallOption.Name;
                    }
                    else if (unitType.Value == 1)
                    {
                        var bigPrice = _productService.GetProductPriceByProductIdAndUnitId(curStore.Id, p.Id, p.BigUnitId??0);
                        if (bigPrice != null)
                        {
                            price = bigPrice.CostPrice ?? 0;
                        }
                        //大包单位
                        change.PriceName = price + "/" + option.bigOption.Name;
                        //单位转换
                        change.UnitConversion = option.bigOption.UnitConversion;
                        //单位名称
                        change.UnitName = option.bigOption.Name;
                        change.InitialQuantity = change.InitialQuantity / change.BigQuantity ?? 1;
                        change.EndQuantity = change.EndQuantity / change.BigQuantity ?? 1;
                        change.CurrentPurchaseQuantity = change.CurrentPurchaseQuantity / change.BigQuantity ?? 1;
                        change.CurrentReturnQuantity = change.CurrentReturnQuantity / change.BigQuantity ?? 1;
                        change.CurrentAllocationInQuantity = change.CurrentAllocationInQuantity / change.BigQuantity ?? 1;
                        change.CurrentAllocationOutQuantity = change.CurrentAllocationOutQuantity / change.BigQuantity ?? 1;
                        change.CurrentSaleQuantity = change.CurrentSaleQuantity / change.BigQuantity ?? 1;
                        change.CurrentSaleReturnQuantity = change.CurrentSaleReturnQuantity / change.BigQuantity ?? 1;
                        change.CurrentCombinationQuantity = change.CurrentCombinationQuantity / change.BigQuantity ?? 1;
                        change.CurrentSplitReturnQuantity = change.CurrentSplitReturnQuantity / change.BigQuantity ?? 1;
                        change.CurrentWasteQuantity = change.CurrentWasteQuantity / change.BigQuantity ?? 1;
                        change.CurrentVolumeQuantity = change.CurrentVolumeQuantity / change.BigQuantity ?? 1;
                        change.CurrentLossesQuantity = change.CurrentLossesQuantity / change.BigQuantity ?? 1;
                        change.GiftQuantity = change.EndQuantity / change.BigQuantity ?? 1;
                    }
                    //}


                    //商品编码(SKU)
                    change.ProductSKU = p != null ? p.Sku : "";
                    //条码
                    change.BarCode = p != null ? p.SmallBarCode : "";
                    change.ProductName = p.Name;
                    change.BrandId = p.BrandId;
                }
            });

            var summaries = new PagedList<StockChangeSummary>(products, pagenumber, 30);

            model.Items = summaries;
            model.PagingFilteringContext.LoadPagedList(summaries);

            return View(model);
        }

        //库存变化汇总表(汇总)导出
        [HttpGet]
        [AuthCode((int)AccessGranularityEnum.StockChangeExport)]
        public FileResult ExportChangeSummary(int? productId = 0, string productName = null, int? brandId = 0, int? wareHouseId = 0, int? priceType = 0, int? unitType = 0, DateTime? startTime = null, DateTime? endTime = null)
        {

            #region 查询导出数据

            var products = _stockReportService.GetAllStockChangeSummary(curStore?.Id ?? 0,
                wareHouseId,
                productId,
                productName,
                brandId,
                (startTime == null) ? DateTime.Parse(DateTime.Now.ToString("yyyy-MM-01 00:00:00")) : startTime,
                (endTime == null) ? DateTime.Now.AddDays(1) : endTime);

            #region 查询需要关联其他表的数据
            var allProducts = _productService.GetProductsByIds(curStore.Id, products.Select(sd => sd.ProductId).Distinct().ToArray());
            var allOptions = _specificationAttributeService.GetSpecificationAttributeOptionByIds(curStore.Id, allProducts.GetProductBigStrokeSmallUnitIds());
            var allProductPrices = _productService.GetProductPricesByProductIds(curStore.Id, products.Select(sd => sd.ProductId).Distinct().ToArray());
            var allProductStocks = _stockService.GetAllStocksByProductIds(curStore == null ? 0 : curStore.Id, products.Select(sd => sd.ProductId).Distinct().ToArray());
            #endregion

            var products2=products.ToList().Select(change =>
            {
                var p = allProducts.Where(ap => ap.Id == change.ProductId).FirstOrDefault();
                if (p != null)
                {
                    //获取规格
                    var option = p.GetProductUnit(allOptions, allProductPrices);

                    var stock = allProductStocks.Where(s => s.WareHouseId == wareHouseId && s.ProductId == p.Id).FirstOrDefault();
                    if (priceType != null)
                    {
                        if (priceType.Value == 0)
                        {
                            //按照商品档案中预设的批发价进行计算。
                            change.Price = option.smallPrice == null ? 0 : option.smallPrice.TradePrice ?? 0;
                        }
                        else if (priceType.Value == 1)
                        {
                            //按照现在此商品在库存中的平均价格计算。
                            if (stock != null)
                            {
                                change.Price = (stock.UsableQuantity * (option.smallPrice == null ? 0 : option.smallPrice.CostPrice)) / stock.UsableQuantity ?? 1;
                            }
                        }
                        else if (priceType.Value == 2)
                        {
                            //按照商品档案中预设的进价进行计算。
                            change.Price = option.smallPrice == null ? 0 : option.smallPrice.PurchasePrice ?? 0;
                        }
                    }

                    change.PriceName = change.Price + "/" + option.smallOption.Name;
                    change.InitialAmount = change.Price * change.InitialQuantity;
                    change.EndAmount = change.Price * change.EndQuantity;
                    change.UnitName = option.smallOption.Name;
                    if (unitType != null)
                    {
                        if (unitType.Value == 0)
                        {
                            //基本单位
                            change.PriceName = change.Price + "/" + option.smallOption.Name;
                            //单位转换
                            change.UnitConversion = option.smallOption.UnitConversion;
                            //单位名称
                            change.UnitName = option.smallOption.Name;
                        }
                        else if (unitType.Value == 1)
                        {
                            //大包单位
                            change.PriceName = change.Price + "/" + option.bigOption.Name;
                            //单位转换
                            change.UnitConversion = option.bigOption.UnitConversion;
                            //单位名称
                            change.UnitName = option.bigOption.Name;
                            change.InitialQuantity = change.InitialQuantity / change.BigQuantity ?? 1;
                            change.EndQuantity = change.EndQuantity / change.BigQuantity ?? 1;
                            change.CurrentPurchaseQuantity = change.CurrentPurchaseQuantity / change.BigQuantity ?? 1;
                            change.CurrentReturnQuantity = change.CurrentReturnQuantity / change.BigQuantity ?? 1;
                            change.CurrentAllocationInQuantity = change.CurrentAllocationInQuantity / change.BigQuantity ?? 1;
                            change.CurrentAllocationOutQuantity = change.CurrentAllocationOutQuantity / change.BigQuantity ?? 1;
                            change.CurrentSaleQuantity = change.CurrentSaleQuantity / change.BigQuantity ?? 1;
                            change.CurrentSaleReturnQuantity = change.CurrentSaleReturnQuantity / change.BigQuantity ?? 1;
                            change.CurrentCombinationQuantity = change.CurrentCombinationQuantity / change.BigQuantity ?? 1;
                            change.CurrentSplitReturnQuantity = change.CurrentSplitReturnQuantity / change.BigQuantity ?? 1;
                            change.CurrentWasteQuantity = change.CurrentWasteQuantity / change.BigQuantity ?? 1;
                            change.CurrentVolumeQuantity = change.CurrentVolumeQuantity / change.BigQuantity ?? 1;
                            change.CurrentLossesQuantity = change.CurrentLossesQuantity / change.BigQuantity ?? 1;
                        }
                    }


                    //商品编码(SKU)
                    change.ProductSKU = p != null ? p.Sku : "";
                    //条码
                    change.BarCode = p != null ? p.SmallBarCode : "";
                    change.ProductName = p.Name;
                    change.BrandId = p.BrandId;
                }
                return change;
            });

            #endregion

            #region 导出
            var ms = _exportManager.ExportChangeSummaryToXlsx(products2.ToList());
            if (ms != null)
            {
                return File(ms, "application/vnd.ms-excel", "库存变化汇总表(汇总).xlsx");
            }
            else
            {
                return File(new MemoryStream(), "application/vnd.ms-excel", "库存变化汇总表(汇总).xlsx");
            }
            #endregion

        }
        #endregion


        #region 库存变化汇总表(按单据)
        /// <summary>
        /// 库存变化汇总表(按单据)
        /// </summary>
        /// <param name="productId"></param>
        /// <param name="brandId"></param>
        /// <param name="wareHouseId"></param>
        /// <param name="priceType"></param>
        /// <param name="unitType"></param>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <param name="pagenumber"></param>
        /// <returns></returns>
        [HttpGet]
        [AuthCode((int)AccessGranularityEnum.StockChangeByBillsView)]
        public IActionResult ChangeByOrder(int? productId, string productName, int? categoryId, int? billType, int? wareHouseId, string billCode = "", DateTime? startTime = null, DateTime? endTime = null, bool crossMonth = true, int pagenumber = 0)
        {

            var model = new StockChangeSummaryOrderListModel
            {
                ProductId = productId ?? 0,
                ProductName = productName,
                //仓库
                WareHouses = BindWareHouseSelection(_wareHouseService.BindWareHouseList, curStore, null, 0),
                WareHouseId = wareHouseId ?? null,

                BillTypes = BindBillTypeSelection<BillTypeReportEnum>(),
                BillType = billType ?? null,

                Categories = BindCategorySelection(new Func<int?, IList<Category>>(_productCategoryService.GetAllCategoriesDisplayed), curStore),
                CategoryId = categoryId ?? null,
                StartTime = startTime ?? DateTime.Parse(DateTime.Now.ToString("yyyy-MM-01")),
                EndTime = endTime ?? DateTime.Now.AddDays(1),
                BillCode = billCode,
                CrossMonth = crossMonth
            };

            if (pagenumber > 0)
            {
                pagenumber -= 1;
            }

            var products = _stockReportService.GetStockChangeSummaryByOrder(curStore?.Id ?? 0,
                productId ?? 0,
                productName,
                categoryId ?? 0,
                billType ?? 0,
                wareHouseId ?? 0,
                billCode,
                model.StartTime,
                model.EndTime,
                crossMonth);

            #region 查询需要关联其他表的数据

            var allProducts = _productService.GetProductsByIds(curStore.Id, products.Select(sd => sd.ProductId).Distinct().ToArray());
            var allOptions = _specificationAttributeService.GetSpecificationAttributeOptionByIds(curStore.Id, allProducts.GetProductBigStrokeSmallUnitIds());
            var allProductPrices = _productService.GetProductPricesByProductIds(curStore.Id, products.Select(sd => sd.ProductId).Distinct().ToArray());
            var allProductStocks = _stockService.GetAllStocksByProductIds(curStore == null ? 0 : curStore.Id, products.Select(sd => sd.ProductId).Distinct().ToArray());
            #endregion

            var whs = _wareHouseService.GetWareHouseByIds(curStore?.Id ?? 0, products.Select(s => s.WareHouseId).ToArray());

            var products2 = products.Select(change =>
            {
                var p = allProducts.Where(ap => ap.Id == change.ProductId).FirstOrDefault();
                if (p != null)
                {
                    //获取规格
                    var option = p.GetProductUnit(allOptions, allProductPrices);

                    //商品编码(SKU)
                    change.ProductSKU = p != null ? p.Sku : "";

                    //条码
                    change.BarCode = p != null ? p.SmallBarCode : "";
                    change.ProductName = p.Name;
                    change.BrandId = p.BrandId;
                    change.WareHouseName = whs.Where(s => s.Id == change.WareHouseId).FirstOrDefault()?.Name;

                    //基本单位
                    change.PriceName = change.Price + "/" + option.smallOption.Name;
                    //单位转换
                    change.UnitConversion = option.smallOption.UnitConversion;
                    //单位名称
                    change.UnitName = option.smallOption.Name;

                
                    if (change.BillType == 31 && change.Direction == 1)
                    {
                        change.BillType = 311;
                    }
                    else if (change.BillType == 31 && change.Direction == 2)
                    {
                        change.BillType = 312;

                    }
                    else if (change.BillType == 32 && change.Direction == 1)
                    {
                        change.BillType = 321;
                    }
                    else if (change.BillType == 32 && change.Direction == 2)
                    {
                        change.BillType = 322;
                    }

                    change.BillTypeName = CommonHelper.GetEnumDescription<BillTypeReportEnum>((BillTypeReportEnum)change.BillType);
                    change.LinkUrl = _billConvertService.GenerateBillUrl(change.BillType, change.BillId);

                    //可用改变量（数量转换）
                    change.UsableQuantityChangeConversion = p.GetConversionFormat(allOptions, p.SmallUnitId, change.UsableQuantityChange);

                    //当前可用（数量转换）
                    change.UsableQuantityAfterConversion = p.GetConversionFormat(allOptions, p.SmallUnitId, change.UsableQuantityAfter);

                    change.UsableQuantityChangePart = Pexts.StockQuantityFormat(change.UsableQuantityChange, p.StrokeQuantity ?? 0, p.BigQuantity ?? 0);

                    change.UsableQuantityChangeAfterPart = Pexts.StockQuantityFormat(change.UsableQuantityAfter, p.StrokeQuantity ?? 0, p.BigQuantity ?? 0);
                    //商品差异
                    change.Difference = _billConvertService.Cy(change.BillType, curStore?.Id ?? 0, change.BillId, change.ProductId);

                }

                return change == null ? new StockChangeSummaryOrder() : change;

            }).AsQueryable();


            if (productId.HasValue && productId > 0)
            {
                products2 = products2.Where(p => p.ProductId == productId.Value);
            }

            var summaries = new PagedList<StockChangeSummaryOrder>(products2, pagenumber, 30);

            model.Items = summaries;
            model.PagingFilteringContext.LoadPagedList(summaries);

            return View(model);
        }

        //库存变化表（按单据）导出
        [AuthCode((int)AccessGranularityEnum.StockChangeByBillsExport)]
        public FileResult ExportChangeByOrder(int? productId, string productName, int? categoryId, int? billType, int? wareHouseId, string billCode = "", DateTime? startTime = null, DateTime? endTime = null)
        {

            #region 查询导出数据

            var products = _stockReportService.GetStockChangeSummaryByOrder(curStore?.Id ?? 0,
                productId ?? 0,
                productName,
                categoryId ?? 0,
                billType ?? 0,
                wareHouseId ?? 0,
                billCode,
                startTime ?? DateTime.Now,
                endTime ?? DateTime.Now.AddDays(1));

            #region 查询需要关联其他表的数据
            var allProducts = _productService.GetProductsByIds(curStore.Id, products.Select(sd => sd.ProductId).Distinct().ToArray());
            var allOptions = _specificationAttributeService.GetSpecificationAttributeOptionByIds(curStore.Id, allProducts.GetProductBigStrokeSmallUnitIds());
            var allProductPrices = _productService.GetProductPricesByProductIds(curStore.Id, products.Select(sd => sd.ProductId).Distinct().ToArray());
            var allProductStocks = _stockService.GetAllStocksByProductIds(curStore == null ? 0 : curStore.Id, products.Select(sd => sd.ProductId).Distinct().ToArray());
            #endregion

            var products2 = products.Select(change =>
            {
                var p = allProducts.Where(ap => ap.Id == change.ProductId).FirstOrDefault();
                if (p != null)
                {
                    //获取规格
                    var option = p.GetProductUnit(allOptions, allProductPrices);

                    //商品编码(SKU)
                    change.ProductSKU = p != null ? p.Sku : "";
                    //条码
                    change.BarCode = p != null ? p.SmallBarCode : "";
                    change.ProductName = p.Name;
                    change.BrandId = p.BrandId;

                    //基本单位
                    change.PriceName = change.Price + "/" + option.smallOption.Name;
                    //单位转换
                    change.UnitConversion = option.smallOption.UnitConversion;
                    //单位名称
                    change.UnitName = option.smallOption.Name;

                    if (change.BillType == 31 && change.Direction == 1)
                    {
                        change.BillType = 311;
                    }
                    else if (change.BillType == 31 && change.Direction == 2)
                    {
                        change.BillType = 312;
                    }
                    else if (change.BillType == 32 && change.Direction == 1)
                    {
                        change.BillType = 321;
                    }
                    else if (change.BillType == 32 && change.Direction == 2)
                    {
                        change.BillType = 322;
                    }

                    change.BillTypeName = CommonHelper.GetEnumDescription<BillTypeReportEnum>((BillTypeReportEnum)change.BillType);
                    change.LinkUrl = _billConvertService.GenerateBillUrl(change.BillType, change.BillId);

                    //可用改变量（数量转换）
                    change.UsableQuantityChangeConversion = p.GetConversionFormat(allOptions, p.SmallUnitId, change.UsableQuantityChange);
                    //当前可用（数量转换）
                    change.UsableQuantityAfterConversion = p.GetConversionFormat(allOptions, p.SmallUnitId, change.UsableQuantityAfter);
                }

                return change == null ? new StockChangeSummaryOrder() : change;

            }).AsQueryable();
            #endregion

            var datas = new List<StockChangeSummaryOrder>(products2);

            #region 导出
            var ms = _exportManager.ExportChangeByOrderToXlsx(datas);

            if (ms != null)
            {
                return File(ms, "application/vnd.ms-excel", "库存变化表（按单据）.xlsx");
            }
            else
            {
                return File(new MemoryStream(), "application/vnd.ms-excel", "库存变化表（按单据）.xlsx");
            }
            #endregion
        }

        #endregion

        #region 门店库存上报表
        /// <summary>
        /// 门店库存上报表
        /// </summary>
        /// <param name="store"></param>
        /// <param name="businessUserId"></param>
        /// <param name="terminalId"></param>
        /// <param name="channelId"></param>
        /// <param name="rankId"></param>
        /// <param name="districtId"></param>
        /// <param name="productId"></param>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <param name="pagenumber"></param>
        /// <returns></returns>
        [HttpGet]
        [AuthCode((int)AccessGranularityEnum.StoreStockReportingView)]
        public IActionResult StockReportList(int? store, int? businessUserId, int? terminalId, string TerminalName, int? channelId, int? rankId, int? districtId, int? productId, string productName, DateTime? startTime = null, DateTime? endTime = null, int pagenumber = 0)
        {

            var model = new InventoryReportListModel
            {
                //业务员
                BusinessUsers = BindUserSelection(_userService.BindUserList, curStore, DCMSDefaults.Salesmans,curUser.Id,true, _userService.IsAdmin(curStore.Id, curUser.Id))
            };
            model.BusinessUserId = (businessUserId ?? null);

            //客户渠道
            model.Channels = BindChanneSelection(new Func<int?, IList<Channel>>(_channelService.GetAll), curStore);
            model.ChannelId = (channelId ?? null);

            //客户等级
            model.Ranks = BindRankSelection(new Func<int?, IList<Rank>>(_rankService.GetAll), curStore);
            model.RankId = (rankId ?? null);

            //客户片区
            model.Districts = BindDistrictSelection(new Func<int?, IList<District>>(_districtService.GetAll), curStore);
            model.DistrictId = (districtId ?? null);

            model.TerminalId = terminalId;
            model.ProductId = productId;

            model.StartTime = startTime ?? DateTime.Parse(DateTime.Now.ToString("yyyy-01-01"));
            model.EndTime = endTime ?? DateTime.Now.AddDays(1);


            if (pagenumber > 0)
            {
                pagenumber -= 1;
            }

            var lists = _stockReportService.GetInventoryReportList(curStore?.Id ?? 0,
                businessUserId ?? 0,
                terminalId ?? 0,
                TerminalName,
                channelId ?? 0,
                rankId ?? 0,
                districtId ?? 0,
                productId ?? 0,
                productName,
                model.StartTime,
                model.EndTime);

            var stores = new PagedList<InventoryReportList>(lists, pagenumber, 30);

            model.Items = stores;
            model.PagingFilteringContext.LoadPagedList(stores);

            return View(model);
        }

        //门店库存上报导出
        [HttpGet]
        [AuthCode((int)AccessGranularityEnum.StoreStockReportingExport)]
        public FileResult StockReportListExport(int? businessUserId, int? terminalId, string terminalName, int? channelId, int? rankId, int? districtId, int? productId, string productName, DateTime? startTime = null, DateTime? endTime = null)
        {

            if (endTime != null)
            {
                endTime = DateTime.Parse(DateTime.Parse(endTime.Value.ToString()).ToShortDateString()).AddDays(1);
            }

            #region 需要导出的数据

            var sqlDatas = _stockReportService.GetInventoryReportList(curStore?.Id ?? 0,
                businessUserId ?? 0,
                terminalId ?? 0,
                terminalName,
                channelId ?? 0,
                rankId ?? 0,
                districtId ?? 0,
                productId ?? 0,
                productName,
                startTime ?? DateTime.Now,
                endTime ?? DateTime.Now.AddDays(1)).ToList();

            #endregion

            #region 导出

            var ms = _exportManager.ExportStockReportListToXlsx(sqlDatas);

            if (ms != null)
            {
                return File(ms, "application/vnd.ms-excel", "门店库存上报表.xlsx");
            }
            else
            {
                return File(new MemoryStream(), "application/vnd.ms-excel", "门店库存上报表.xlsx");
            }

            #endregion
        }
        #endregion

        #region 门店库存上报汇总表
        /// <summary>
        /// 门店库存上报汇总表
        /// </summary>
        /// <param name="store"></param>
        /// <param name="businessUserId"></param>
        /// <param name="terminalId"></param>
        /// <param name="channelId"></param>
        /// <param name="rankId"></param>
        /// <param name="districtId"></param>
        /// <param name="productId"></param>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <param name="pagenumber"></param>
        /// <returns></returns>
        [HttpGet]
        [AuthCode((int)AccessGranularityEnum.StoreStockSummeryReportingView)]
        public IActionResult StockReportAllList(int? store, int? channelId, int? rankId, int? districtId, int? productId, string productName = "", DateTime? startTime = null, DateTime? endTime = null, int pagenumber = 0)
        {


            var model = new InventoryReportListModel
            {
                //客户渠道
                Channels = BindChanneSelection(new Func<int?, IList<Channel>>(_channelService.GetAll), curStore)
            };

            model.ChannelId = (channelId ?? null);

            //客户等级
            model.Ranks = BindRankSelection(new Func<int?, IList<Rank>>(_rankService.GetAll), curStore);
            model.RankId = (rankId ?? null);

            //客户片区
            model.Districts = BindDistrictSelection(new Func<int?, IList<District>>(_districtService.GetAll), curStore);
            model.DistrictId = (districtId ?? null);

            model.ProductId = productId;
            model.StartTime = startTime ?? DateTime.Parse(DateTime.Now.ToString("yyyy-MM-01"));
            model.EndTime = endTime ?? DateTime.Now.AddDays(1);

            if (pagenumber > 0)
            {
                pagenumber -= 1;
            }

            var inventoryReportSummaryLists = _stockReportService.GetInventoryReportSummaryList(curStore?.Id ?? 0,
                channelId ?? 0,
                rankId ?? 0,
                districtId ?? 0,
                productId ?? 0, productName, model.StartTime, model.EndTime);

            var items = new PagedList<InventoryReportList>(inventoryReportSummaryLists, pagenumber, 30);

            model.Items = items;
            model.PagingFilteringContext.LoadPagedList(items);

            return View(model);
        }

        //门店库存上报汇总表导出
        [AuthCode((int)AccessGranularityEnum.StoreStockSummeryReportingExport)]
        public FileResult StockReportAllExport(int? channelId, int? rankId, int? districtId, int? productId, string productName, DateTime? startTime = null, DateTime? endTime = null)
        {


            if (endTime != null)
            {
                endTime = DateTime.Parse(DateTime.Parse(endTime.Value.ToString()).ToShortDateString()).AddDays(1);
            }

            #region 需要导出的数据

            var sqlDatas = _stockReportService.GetInventoryReportSummaryList(curStore?.Id ?? 0,
                channelId ?? 0,
                rankId ?? 0,
                districtId ?? 0,
                productId ?? 0,
                productName,
                startTime, endTime).ToList();


            #endregion

            var ms = _exportManager.ExportStockReportAllToXlsx(sqlDatas);

            if (ms != null)
            {
                return File(ms, "application/vnd.ms-excel", "门店库存上报汇总表.xlsx");
            }
            else
            {
                return File(new MemoryStream(), "application/vnd.ms-excel", "门店库存上报汇总表.xlsx");
            }
        }

        #endregion

        #region 调拨明细表
        /// <summary>
        /// 调拨明细表
        /// </summary>
        /// <param name="shipmentWareHouseId"></param>
        /// <param name="incomeWareHouseId"></param>
        /// <param name="productId"></param>
        /// <param name="categoryId"></param>
        /// <param name="billNumber"></param>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <param name="pagenumber"></param>
        /// <returns></returns>
        [HttpGet]
        [AuthCode((int)AccessGranularityEnum.AllocatingDetailsView)]
        public IActionResult AllocationDetails(int? shipmentWareHouseId, int? incomeWareHouseId, int? productId, string productName, int? categoryId, string BillNumber = "", DateTime? startTime = null, DateTime? endTime = null, int? StatusId = null, int pagenumber = 0)
        {

            var model = new AllocationDetailsListModel();
            model.ShipmentWareHouseId = shipmentWareHouseId ?? null;
            //入货仓库
            model.WareHouses = BindWareHouseSelection(_wareHouseService.BindWareHouseList, curStore,null,0);
            model.IncomeWareHouseId = incomeWareHouseId ?? null;

            //状态
            model.Status = new SelectList(from a in Enum.GetValues(typeof(StatusEnum)).Cast<StatusEnum>()
                                          select new SelectListItem
                                          {
                                              Text = CommonHelper.GetEnumDescription(a),
                                              Value = ((int)a).ToString()
                                          }, "Value", "Text");
            model.StatusId = (StatusId ?? null);

            model.Categories = BindCategorySelection(new Func<int?, IList<Category>>(_productCategoryService.GetAllCategoriesDisplayed), curStore);
            model.CategoryId = categoryId ?? null;

            #region 储存查询条件的值到模型对应的字段
            model.ShipmentWareHouseId = shipmentWareHouseId;
            model.IncomeWareHouseId = incomeWareHouseId;
            model.ProductId = productId;
            model.CategoryId = categoryId;
            model.BillNumber = BillNumber;
            model.StatusId = StatusId;
            model.StartTime = startTime ?? DateTime.Parse(DateTime.Now.ToString("yyyy-MM-01"));
            model.EndTime = endTime ?? DateTime.Now.AddDays(1);
            #endregion

            if (pagenumber > 0)
            {
                pagenumber -= 1;
            }

            var bills = _stockReportService.GetAllocationDetails(curStore?.Id ?? 0,
                shipmentWareHouseId ?? 0,
                incomeWareHouseId ?? 0,
                productId ?? 0,
                productName,
                categoryId ?? 0,
                BillNumber,
                StatusId,
                model.StartTime,
                model.EndTime);

            var details = new PagedList<AllocationDetailsList>(bills, pagenumber, 30);

            foreach (var b in details)
            {
                b.LinkUrl = _billConvertService.GenerateBillUrl(31, b.Id);
            }


            model.Items = details;
            model.PagingFilteringContext.LoadPagedList(details);

            return View(model);
        }

        //调拨明细表导出
        [AuthCode((int)AccessGranularityEnum.AllocatingDetailsExport)]
        public FileResult AllocationDetailsExport(int? shipmentWareHouseId, int? incomeWareHouseId, int? productId, string productName, int? categoryId, string BillNumber = "", DateTime? startTime = null, DateTime? endTime = null, int? StatusId = null)
        {


            if (endTime != null)
            {
                endTime = DateTime.Parse(DateTime.Parse(endTime.Value.ToString()).ToShortDateString()).AddDays(1);
            }

            #region 需要导出的数据

            var sqlDatas = _stockReportService.GetAllocationDetails(curStore?.Id ?? 0,
                shipmentWareHouseId ?? 0,
                incomeWareHouseId ?? 0,
                productId ?? 0,
                productName,
                categoryId ?? 0,
                BillNumber,
                StatusId,
                startTime ?? DateTime.Now,
                endTime ?? DateTime.Now.AddDays(1)).ToList();

            #endregion

            var ms = _exportManager.ExportAllocationDetailsToXlsx(sqlDatas);

            if (ms != null)
            {
                return File(ms, "application/vnd.ms-excel", "调拨明细表.xlsx");
            }
            else
            {
                return File(new MemoryStream(), "application/vnd.ms-excel", "调拨明细表.xlsx");
            }
        }

        #endregion

        #region 调拨汇总表按商品
        /// <summary>
        /// 调拨汇总表按商品
        /// </summary>
        /// <param name="store"></param>
        /// <param name="shipmentWareHouseId"></param>
        /// <param name="incomeWareHouseId"></param>
        /// <param name="productId"></param>
        /// <param name="categoryId"></param>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <param name="pagenumber"></param>
        /// <returns></returns>
        [HttpGet]
        [AuthCode((int)AccessGranularityEnum.AllocatingSummeryByProductView)]
        public IActionResult AllocationDetailsByProduct(int? store, int? shipmentWareHouseId, int? incomeWareHouseId, int? productId, string productName, int? categoryId, DateTime? startTime = null, DateTime? endTime = null, int pagenumber = 0)
        {


            var model = new AllocationDetailsListModel();
            model.ShipmentWareHouseId = shipmentWareHouseId ?? null;
            //入货仓库
            model.WareHouses = BindWareHouseSelection(_wareHouseService.BindWareHouseList, curStore, null, 0);
            model.IncomeWareHouseId = incomeWareHouseId ?? null;

            //商品类别
            model.Categories = BindCategorySelection(new Func<int?, IList<Category>>(_productCategoryService.GetAllCategoriesDisplayed), curStore);
            model.CategoryId = categoryId ?? null;

            model.ProductId = productId;
            model.StartTime = startTime ?? DateTime.Parse(DateTime.Now.ToString("yyyy-MM-01"));
            model.EndTime = endTime ?? DateTime.Now.AddDays(1);

            if (pagenumber > 0)
            {
                pagenumber -= 1;
            }

            var bills = _stockReportService.GetAllocationDetailsByProducts(curStore?.Id ?? 0,
                shipmentWareHouseId ?? 0,
                incomeWareHouseId ?? 0,
                productId ?? 0,
                productName,
                categoryId ?? 0,
                model.StartTime,
                model.EndTime);

            #region 查询需要关联其他表的数据
            var allProducts = _productService.GetProductsByIds(curStore.Id, bills.Select(sd => sd.ProductId).Distinct().ToArray());
            var allOptions = _specificationAttributeService.GetSpecificationAttributeOptionByIds(curStore.Id, allProducts.GetProductBigStrokeSmallUnitIds());
            var allProductPrices = _productService.GetProductPricesByProductIds(curStore.Id, bills.Select(sd => sd.ProductId).Distinct().ToArray());
            var allProductStocks = _stockService.GetAllStocksByProductIds(curStore == null ? 0 : curStore.Id, bills.Select(sd => sd.ProductId).Distinct().ToArray());
            #endregion

            var bills2 = bills.Select(change =>
                  {
                      var p = allProducts.Where(ap => ap.Id == change.ProductId).FirstOrDefault();
                      if (p != null)
                      {
                          //获取规格
                          var option = p.GetProductUnit(allOptions, allProductPrices);

                          //商品编码(SKU)
                          change.ProductSKU = p != null ? p.Sku : "";
                          //条码
                          change.BarCode = p != null ? p.SmallBarCode : "";
                          change.ProductName = p.Name;
                          change.BrandId = p.BrandId;

                          //基本单位
                          change.PriceName = change.Price + "/" + option.smallOption.Name;
                          //单位转换
                          change.UnitConversion = option.smallOption.UnitConversion;
                          //单位名称
                          change.UnitName = _specificationAttributeService.GetSpecificationAttributeOptionName(curStore.Id, change.UnitId);
                      }

                      return change == null ? new AllocationDetailsList() : change;

                  }).AsQueryable();


            if (productId.HasValue && productId > 0)
            {
                bills2 = bills2.Where(p => p.ProductId == productId.Value);
            }
            var bills3 = bills2.GroupBy(s => new { s.ProductId, s.ProductName, s.BarCode, s.UnitConversion, s.ShipmentWareHouseName, s.IncomeWareHouseName, s.UnitName }).Select(m => new AllocationDetailsList
            {
                //ProductId = m.Key.ProductId,
                ProductName = m.Key.ProductName,
                BarCode = m.Key.BarCode,
                UnitConversion = m.Key.UnitConversion,
                ShipmentWareHouseName = m.Key.ShipmentWareHouseName,
                IncomeWareHouseName = m.Key.IncomeWareHouseName,
                Quantity = m.Sum(p => p.Quantity),
                UnitName=m.Key.UnitName
            }).ToList();
            var stores = new PagedList<AllocationDetailsList>(bills3, pagenumber, 30);

            model.Items = stores;
            model.PagingFilteringContext.LoadPagedList(stores);

            return View(model);
        }

        //调拨明细表（按商品）导出
        [AuthCode((int)AccessGranularityEnum.AllocatingSummeryByProductExport)]
        public FileResult AllocationDetailsByProductExport(int? shipmentWareHouseId, int? incomeWareHouseId, int? productId, string productName, int? categoryId, DateTime? startTime = null, DateTime? endTime = null)
        {

            #region 需要导出的数据

            var bills = _stockReportService.GetAllocationDetailsByProducts(curStore?.Id ?? 0,
                shipmentWareHouseId ?? 0,
                incomeWareHouseId ?? 0,
                productId ?? 0,
                productName,
                categoryId ?? 0,
                startTime ?? DateTime.Parse(DateTime.Now.ToString("yyyy-MM-01")),
                endTime ?? DateTime.Now.AddDays(1));

            #region 查询需要关联其他表的数据
            var allProducts = _productService.GetProductsByIds(curStore.Id, bills.Select(sd => sd.ProductId).Distinct().ToArray());
            var allOptions = _specificationAttributeService.GetSpecificationAttributeOptionByIds(curStore.Id, allProducts.GetProductBigStrokeSmallUnitIds());
            var allProductPrices = _productService.GetProductPricesByProductIds(curStore.Id, bills.Select(sd => sd.ProductId).Distinct().ToArray());
            var allProductStocks = _stockService.GetAllStocksByProductIds(curStore == null ? 0 : curStore.Id, bills.Select(sd => sd.ProductId).Distinct().ToArray());
            #endregion

            var bills2 = bills.Select(change =>
            {
                var p = allProducts.Where(ap => ap.Id == change.ProductId).FirstOrDefault();
                if (p != null)
                {
                    //获取规格
                    var option = p.GetProductUnit(allOptions, allProductPrices);

                    //商品编码(SKU)
                    change.ProductSKU = p != null ? p.Sku : "";
                    //条码
                    change.BarCode = p != null ? p.SmallBarCode : "";
                    change.ProductName = p.Name;
                    change.BrandId = p.BrandId;

                    //基本单位
                    change.PriceName = change.Price + "/" + option.smallOption.Name;
                    //单位转换
                    change.UnitConversion = option.smallOption.UnitConversion;
                    //单位名称
                    change.UnitName = _specificationAttributeService.GetSpecificationAttributeOptionName(curStore.Id, change.UnitId);
                }

                return change ?? new AllocationDetailsList();

            }).AsQueryable();


            if (productId.HasValue && productId > 0)
            {
                bills2 = bills2.Where(p => p.ProductId == productId.Value);
            }
            var bills3 = bills2.GroupBy(s => new { s.ProductId, s.ProductName, s.BarCode, s.UnitConversion, s.ShipmentWareHouseName, s.IncomeWareHouseName }).Select(m => new AllocationDetailsList
            {
                //ProductId = m.Key.ProductId,
                ProductName = m.Key.ProductName,
                BarCode = m.Key.BarCode,
                UnitConversion = m.Key.UnitConversion,
                ShipmentWareHouseName = m.Key.ShipmentWareHouseName,
                IncomeWareHouseName = m.Key.IncomeWareHouseName,
                Quantity = m.Sum(p => p.Quantity)
            }).ToList();
            #endregion

            var ms = _exportManager.ExportAllocationDetailsByProductToXlsx(bills3);

            if (ms != null)
            {
                return File(ms, "application/vnd.ms-excel", "调拨明细表(按商品).xlsx");
            }
            else
            {
                return File(new MemoryStream(), "application/vnd.ms-excel", "调拨明细表(按商品).xlsx");
            }
        }
        #endregion

        #region 库存滞销报表
        /// <summary>
        /// 库存滞销报表
        /// </summary>
        /// <param name="productId"></param>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <param name="wareHouseId"></param>
        /// <param name="brandId"></param>
        /// <param name="categoryId"></param>
        /// <param name="lessNetSaleQuantity"></param>
        /// <param name="pagenumber"></param>
        /// <returns></returns>
        [HttpGet]
        [AuthCode((int)AccessGranularityEnum.InventoryUnsalableReportView)]
        public IActionResult StockUnsalable(int? productId, string productName, DateTime? startTime, DateTime? endTime, int? wareHouseId, int? brandId, int? categoryId,
            int? lessNetSaleQuantity, int pagenumber = 0)
        {


            var model = new StockUnsalableListModel();

            #region 绑定数据源

            //商品
            model.ProductId = productId;
            model.ProductName = productName;

            model.StartTime = startTime ?? DateTime.Parse(DateTime.Now.ToString("yyyy-MM-01 00:00:00"));
            model.EndTime = endTime ?? DateTime.Now.AddDays(1);

            //仓库
            model.WareHouses = BindWareHouseSelection(_wareHouseService.BindWareHouseList, curStore,null,0);
            model.WareHouseId = (wareHouseId ?? null);

            //品牌
            model.Brands = BindBrandSelection(new Func<int?, IList<Brand>>(_brandService.GetAllBrands), curStore);
            model.BrandId = (brandId ?? null);

            //商品类别
            model.Categories = BindCategorySelection(new Func<int?, IList<Category>>(_productCategoryService.GetAllCategoriesDisplayed), curStore);
            model.CategoryId = (categoryId ?? null);

            //净销数量小于
            model.LessNetSaleQuantity = lessNetSaleQuantity ?? 150;

            #endregion

            if (pagenumber > 0)
            {
                pagenumber -= 1;
            }

            var sqlDatas = _stockReportService.GetStockUnsalable(curStore?.Id ?? 0,
                productId, productName, model.StartTime, //?? DateTime.Parse(DateTime.Now.ToString("yyyy-MM-dd")),
                model.EndTime, //?? DateTime.Now.AddDays(1),
                wareHouseId, brandId,
                categoryId, model.LessNetSaleQuantity
                );

            var items = new PagedList<StockUnsalable>(sqlDatas, pagenumber, 30);

            model.Items = items;

            #region 汇总

            if (sqlDatas != null && sqlDatas.ToList().Count > 0)
            {

                //销售金额
                model.TotalSumSaleAmount = sqlDatas.Sum(a => a.SaleAmount ?? 0);
                //退货金额
                model.TotalSumReturnAmount = sqlDatas.Sum(a => a.ReturnAmount ?? 0);
                //净销金额
                model.TotalSumNetAmount = sqlDatas.Sum(a => a.NetAmount ?? 0);

            }

            #endregion

            model.PagingFilteringContext.LoadPagedList(items);

            return View(model);
        }

        //库存滞销报表导出
        [AuthCode((int)AccessGranularityEnum.InventoryUnsalableReportExport)]
        public FileResult ExportStockUnsalable(int? productId, string productName, DateTime? startTime, DateTime? endTime, int? wareHouseId, int? brandId, int? categoryId,
            int? lessNetSaleQuantity)
        {

            #region 查询导出数据

            var sqlDatas = _stockReportService.GetStockUnsalable(curStore?.Id ?? 0,
                 productId,
                 productName,
                 (startTime == null) ? DateTime.Parse(DateTime.Now.ToString("yyyy-MM-01 00:00:00")) : startTime,
                 (endTime == null) ? DateTime.Now.AddDays(1) : endTime,
                 wareHouseId, brandId,
                 categoryId, lessNetSaleQuantity
                 ).ToList();

            #endregion

            #region 导出
            var ms = _exportManager.ExportStockUnsalableToXlsx(sqlDatas);
            if (ms != null)
            {
                return File(ms, "application/vnd.ms-excel", "库存滞销报表.xlsx");
            }
            else
            {
                return File(new MemoryStream(), "application/vnd.ms-excel", "库存滞销报表.xlsx");
            }
            #endregion
        }


        #endregion

        #region 库存预警表
        /// <summary>
        /// 库存预警表
        /// </summary>
        /// <param name="wareHouseId"></param>
        /// <param name="categoryId"></param>
        /// <param name="brandId"></param>
        /// <param name="unitShowTypeId"></param>
        /// <param name="pagenumber"></param>
        /// <returns></returns>
        [HttpGet]
        [AuthCode((int)AccessGranularityEnum.StockWaringReportView)]
        public IActionResult EarlyWarning(int? wareHouseId, int? categoryId, int? brandId, int? unitShowTypeId, int pagenumber = 0)
        {


            var model = new EarlyWarningListModel();

            #region 绑定数据源

            //仓库
            model.WareHouses = BindWareHouseSelection(_wareHouseService.BindWareHouseList, curStore, null, 0);
            model.WareHouseId = (wareHouseId ?? null);

            //商品类别
            model.Categories = BindCategorySelection(new Func<int?, IList<Category>>(_productCategoryService.GetAllCategoriesDisplayed), curStore);
            model.CategoryId = (categoryId ?? null);

            //品牌
            model.Brands = BindBrandSelection(new Func<int?, IList<Brand>>(_brandService.GetAllBrands), curStore);
            model.BrandId = (brandId ?? null);

            //单位类型
            model.UnitShowTypes = new SelectList(from a in Enum.GetValues(typeof(UnitShowType)).Cast<UnitShowType>()
                                                 select new SelectListItem
                                                 {
                                                     Text = CommonHelper.GetEnumDescription(a),
                                                     Value = ((int)a).ToString()
                                                 }, "Value", "Text");
            model.UnitShowTypeId = (unitShowTypeId ?? null);

            #endregion

            if (pagenumber > 0)
            {
                pagenumber -= 1;
            }

            var lists = _stockReportService.GetEarlyWarning(curStore?.Id ?? 0, wareHouseId, categoryId, brandId, unitShowTypeId ?? 0);
            var items = new PagedList<EarlyWarning>(lists, pagenumber, 30);
            model.Items = items;
            model.PagingFilteringContext.LoadPagedList(items);

            return View(model);
        }

        //库存预警表导出
        [AuthCode((int)AccessGranularityEnum.EarlyWarningView)]
        public FileResult ExportEarlyWarning(int? wareHouseId, int? categoryId, int? brandId, int? unitShowTypeId)
        {

            #region 查询导出数据

            var sqlDatas = _stockReportService.GetEarlyWarning(curStore?.Id ?? 0, wareHouseId, categoryId, brandId, unitShowTypeId??0).ToList();

            #endregion

            #region 导出
            var ms = _exportManager.ExportEarlyWarningToXlsx(sqlDatas);
            if (ms != null)
            {
                return File(ms, "application/vnd.ms-excel", "库存预警表.xlsx");
            }
            else
            {
                return File(new MemoryStream(), "application/vnd.ms-excel", "库存预警表.xlsx");
            }
            #endregion
        }

        #endregion

        #region 临期预警表
        /// <summary>
        /// 临期预警表
        /// </summary>
        /// <param name="wareHouseId">仓库Id</param>
        /// <param name="categoryId">商品类别Id</param>
        /// <param name="productId">商品Id</param>
        /// <param name="pagenumber"></param>
        /// <returns></returns>
        [HttpGet]
        [AuthCode((int)AccessGranularityEnum.EarlyWarningView)]
        public IActionResult ExpirationWarning(int? wareHouseId, int? categoryId, int? productId, string productName, int pagenumber = 0)
        {
            var model = new ExpirationWarningListModel();

            #region 绑定数据源

            //仓库
            model.WareHouses = BindWareHouseSelection(_wareHouseService.BindWareHouseList, curStore,null,0);
            model.WareHouseId = (wareHouseId ?? null);

            //商品类别
            model.Categories = BindCategorySelection(new Func<int?, IList<Category>>(_productCategoryService.GetAllCategoriesDisplayed), curStore);
            model.CategoryId = (categoryId ?? null);

            #endregion

            if (pagenumber > 0)
            {
                pagenumber -= 1;
            }

            var lists = _stockReportService.GetExpirationWarning(curStore?.Id ?? 0, wareHouseId, categoryId, productId, productName);
            var items = new PagedList<ExpirationWarning>(lists, pagenumber, 30);
            model.Items = items;
            model.PagingFilteringContext.LoadPagedList(items);

            return View(model);
        }

        //临期预警表导出
        [AuthCode((int)AccessGranularityEnum.SaleSummaryByProductExport)]
        public FileResult ExportExpirationWarning(int? wareHouseId, int? categoryId, int? productId, string productName)
        {

            #region 查询导出数据

            var sqlDatas = _stockReportService.GetExpirationWarning(curStore?.Id ?? 0, wareHouseId, categoryId, productId, productName);

            #endregion

            #region 导出
            var ms = _exportManager.ExportExpirationWarningToXlsx(sqlDatas);
            if (ms != null)
            {
                return File(ms, "application/vnd.ms-excel", "临期预警表.xlsx");
            }
            else
            {
                return File(new MemoryStream(), "application/vnd.ms-excel", "临期预警表.xlsx");
            }
            #endregion
        }

        #endregion


        #region 成本变化汇总

        /// <summary>
        /// 成本变化汇总
        /// </summary>
        /// <param name="date">结转日期</param>
        /// <returns></returns>
        public IActionResult CostPriceSummery(int productId, string productName, string RecordTime, int pagenumber = 0)
        {
            var model = new CostPriceSummeryListModel
            {
                Dates = BindDatesSelection(_closingAccountsService.GetAll, curStore?.Id)
            };

            if (pagenumber > 0)
            {
                pagenumber -= 1;
            };
            model.RecordTime = RecordTime ?? DateTime.Parse(DateTime.Parse(DateTime.Now.ToString("yyyy-MM-01")).AddDays(-1).ToString("yyyy-MM-dd")).ToShortDateString();
            DateTime.TryParse(model.RecordTime, out DateTime cdate);


            var lists = _closingAccountsService.GetCostPriceSummeries(curStore?.Id, productId, productName, cdate, pagenumber, 50);
            model.Items = lists.Select(s => s.ToModel<CostPriceSummeryModel>()).ToList();
            model.PagingFilteringContext.LoadPagedList(lists);
            return View(model);
        }

        [HttpGet]
        [AuthCode((int)AccessGranularityEnum.StockChangeExport)]
        public FileResult ExportCostPriceSummery(int productId, string productName, string RecordTime)
        {

            DateTime.TryParse(RecordTime, out DateTime cdate);
            var lists = _closingAccountsService.ExportCostPriceSummery(curStore?.Id, productId, productName, cdate);
            var ms = _exportManager.ExportCostPriceSummeryToXlsx(lists);
            if (ms != null)
            {
                return File(ms, "application/vnd.ms-excel", "成本汇总表.xlsx");
            }
            else
            {
                return File(new MemoryStream(), "application/vnd.ms-excel", "成本汇总表.xlsx");
            }

        }
        /// <summary>
        /// 成本变化明细表
        /// </summary>
        /// <param name="date">结转日期</param>
        /// <param name="costPriceSummeryId"></param>
        /// <returns></returns>
        public IActionResult CostPriceChangeRecords(string date, int costPriceSummeryId = 0, int pagenumber = 0)
        {
            var model = new CostPriceChangeRecordsListModel();
            if (pagenumber > 0)
            {
                pagenumber -= 1;
            }

            DateTime.TryParse(date, out DateTime cdate);

            model.DateName = cdate.ToString("yyyy 年第 MM 期");
            var lists = _closingAccountsService.GetCostPriceChangeRecordss(curStore?.Id, costPriceSummeryId, cdate, pagenumber, 50);
            model.Items = lists.Select(s => s.ToModel<CostPriceChangeRecordsModel>()).ToList();
            model.PagingFilteringContext.LoadPagedList(lists);
            return View(model);
        }
        [HttpGet]
        [AuthCode((int)AccessGranularityEnum.StockChangeExport)]
        public FileResult ExportCostPriceChangeRecords(string date, int costPriceSummeryId = 0, int pagenumber = 0)
        {

            DateTime.TryParse(date, out DateTime cdate);
            var lists = _closingAccountsService.ExportGetCostPriceChangeRecordss(curStore?.Id, costPriceSummeryId, cdate);
            var ms = _exportManager.ExportCostPriceChangeRecordsToXlsx(lists);
            if (ms != null)
            {
                return File(ms, "application/vnd.ms-excel", "成本汇总明细表.xlsx");
            }
            else
            {
                return File(new MemoryStream(), "application/vnd.ms-excel", "成本汇总明细表.xlsx");
            }

        }


        #endregion
    }
}