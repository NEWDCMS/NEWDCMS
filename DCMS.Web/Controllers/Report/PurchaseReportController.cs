using DCMS.Core;
using DCMS.Core.Domain.Purchases;
using DCMS.Services.ExportImport;
using DCMS.Services.Logging;
using DCMS.Services.Messages;
using DCMS.Services.Products;
using DCMS.Services.Report;
using DCMS.Services.WareHouses;
using DCMS.ViewModel.Models.Purchases;
using DCMS.Web.Framework.Mvc.Filters;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace DCMS.Web.Controllers
{
    /// <summary>
    /// 采购报表
    /// </summary>
    public class PurchaseReportController : BasePublicController
    {
        private readonly IWareHouseService _wareHouseService;
        private readonly ICategoryService _productCategoryService;
        private readonly IStatisticalTypeService _statisticalTypeService;
        private readonly IManufacturerService _manufacturerService;
        private readonly IPurchaseReportService _purchaseReportService;
        private readonly IExportManager _exportManager;

        public PurchaseReportController(
            IWorkContext workContext,
            IStoreContext storeContext,
            IWareHouseService wareHouseService,
            ICategoryService productCategoryService,
            IStatisticalTypeService statisticalTypeService,
            IManufacturerService manufacturerService,
            IPurchaseReportService purchaseReportService,
            ILogger loggerService,
            INotificationService notificationService,
            IExportManager exportManager) : base(workContext, loggerService, storeContext, notificationService)
        {
            _wareHouseService = wareHouseService;
            _productCategoryService = productCategoryService;
            _statisticalTypeService = statisticalTypeService;
            _manufacturerService = manufacturerService;
            _purchaseReportService = purchaseReportService;
            _exportManager = exportManager;
        }


        #region 采购明细表
        /// <summary>
        /// 采购明细表
        /// </summary>
        /// <param name="productId">商品Id</param>
        /// <param name="categoryId">商品类别Id</param>
        /// <param name="manufacturerId">供应商Id</param>
        /// <param name="wareHouseId">仓库Id</param>
        /// <param name="billNumber">单据编号</param>
        /// <param name="purchaseTypeId">单据类型Id</param>
        /// <param name="startTime">开始日期</param>
        /// <param name="endTime">结束日期</param>
        /// <param name="remark">明细备注</param>
        /// <param name="pagenumber"></param>
        /// <returns></returns>
        [HttpGet]
        [AuthCode((int)AccessGranularityEnum.PurchaseReportView)]
        public IActionResult PurchaseReportItem(int? productId, string productName, int? categoryId, int? manufacturerId, int? wareHouseId, string billNumber, int? purchaseTypeId, DateTime? startTime, DateTime? endTime, string remark, int pagenumber = 0)
        {


            var model = new PurchaseReportItemListModel();

            #region 绑定数据源

            //商品
            model.ProductId = productId;
            model.ProductName = productName;

            //商品类别
            model.Categories = BindCategorySelection(_productCategoryService.BindCategoryList, curStore);
            model.CategoryId = (categoryId ?? null);

            //供应商
            model.Manufacturers = BindManufacturerSelection(_manufacturerService.BindManufacturerList, curStore);
            model.ManufacturerId = (manufacturerId ?? null);

            //仓库
            model.WareHouses = BindWareHouseSelection(_wareHouseService.BindWareHouseList, curStore,null,0);
            model.WareHouseId = (wareHouseId ?? null);

            //单据类型
            model.PurchaseTypes = new SelectList(new List<SelectListItem>() { new SelectListItem
                                                 {
                                                     Text = CommonHelper.GetEnumDescription(BillTypeEnum.PurchaseBill),
                                                     Value = ((int)BillTypeEnum.PurchaseBill).ToString()
                                                 },new SelectListItem
                                                 {
                                                     Text = CommonHelper.GetEnumDescription(BillTypeEnum.PurchaseReturnBill),
                                                     Value = ((int)BillTypeEnum.PurchaseReturnBill).ToString()
                                                 }}, "Value", "Text");
            model.PurchaseTypeId = (purchaseTypeId ?? null);

            model.StartTime = startTime ?? DateTime.Parse(DateTime.Now.ToString("yyyy-MM-01"));
            model.EndTime = endTime ?? DateTime.Now.AddDays(1);

            //备注
            model.Remark = remark;
            model.BillNumber = billNumber;

            #endregion

            if (pagenumber > 0)
            {
                pagenumber -= 1;
            }

            var sqlDatas = _purchaseReportService.GetPurchaseReportItem(curStore?.Id ?? 0,
                 curUser.Id,
                productId, productName, categoryId, manufacturerId, wareHouseId,
                billNumber, purchaseTypeId,
                model.StartTime, //?? DateTime.Parse(DateTime.Now.ToString("yyyy-MM-dd")),
                model.EndTime, //?? DateTime.Now.AddDays(1),
                remark
                );

            var items = new PagedList<PurchaseReportItem>(sqlDatas, pagenumber, 30);

            model.Items = items;

            #region 汇总

            if (items != null && items.Count > 0)
            {
                model.PageSumAmount = items.Sum(a => a.Amount ?? 0);
            }

            if (sqlDatas != null && sqlDatas.ToList().Count > 0)
            {
                model.TotalSumAmount = sqlDatas.Sum(a => a.Amount ?? 0);
            }

            #endregion

            model.PagingFilteringContext.LoadPagedList(items);

            return View(model);
        }

        //采购明细表导出
        [AuthCode((int)AccessGranularityEnum.PurchaseReportExport)]
        public FileResult ExportPurchaseReportItem(int? productId, string productName, int? categoryId, int? manufacturerId, int? wareHouseId, string billNumber, int? purchaseTypeId, DateTime? startTime, DateTime? endTime, string remark, int pagenumber = 0)
        {


            startTime = startTime ?? DateTime.Parse(DateTime.Now.ToString("yyyy-MM-01"));
            endTime = endTime ?? DateTime.Now.AddDays(1);

            #region 查询导出数据
            var sqlDatas = _purchaseReportService.GetPurchaseReportItem(curStore?.Id ?? 0,
                 curUser.Id,
                productId, productName, categoryId, manufacturerId, wareHouseId,
                billNumber, purchaseTypeId,
                startTime, //?? DateTime.Parse(DateTime.Now.ToString("yyyy-MM-dd")),
                endTime, //?? DateTime.Now.AddDays(1),
                remark
                );

            #endregion

            var datas = new List<PurchaseReportItem>(sqlDatas);

            #region 导出
            var ms = _exportManager.ExportPurchaseReportItemToXlsx(datas);
            if (ms != null)
            {
                return File(ms, "application/vnd.ms-excel", "采购明细表.xlsx");
            }
            else
            {
                return File(new MemoryStream(), "application/vnd.ms-excel", "采购明细表.xlsx");
            }
            #endregion
        }

        #endregion

        #region 采购汇总（按商品）
        /// <summary>
        /// 采购汇总（按商品）
        /// </summary>
        /// <param name="categoryId">商品类别Id</param>
        /// <param name="productId">商品Id</param>
        /// <param name="manufacturerId">供应商Id</param>
        /// <param name="wareHouseId">仓库Id</param>
        /// <param name="startTime">开始日期</param>
        /// <param name="endTime">结束日期</param>
        /// <param name="pagenumber"></param>
        /// <returns></returns>
        [HttpGet]
        [AuthCode((int)AccessGranularityEnum.PurchaseSummaryByProductView)]
        public IActionResult PurchaseReportSummaryProduct(int? categoryId, int? productId, string productName, int? manufacturerId, int? wareHouseId, DateTime? startTime, DateTime? endTime, int pagenumber = 0)
        {


            var model = new PurchaseReportSummaryProductListModel();

            #region 绑定数据源

            //商品类别
            model.Categories = BindCategorySelection(_productCategoryService.BindCategoryList, curStore);
            model.CategoryId = (categoryId ?? -1);

            //商品
            model.ProductId = productId;
            model.ProductName = productName;

            //供应商
            model.Manufacturers = BindManufacturerSelection(_manufacturerService.BindManufacturerList, curStore);
            model.ManufacturerId = (manufacturerId ?? null);

            //仓库
            model.WareHouses = BindWareHouseSelection(_wareHouseService.BindWareHouseList, curStore,null,0);
            model.WareHouseId = (wareHouseId ?? null);

            model.StartTime = startTime ?? DateTime.Parse(DateTime.Now.ToString("yyyy-MM-01"));
            model.EndTime = endTime ?? DateTime.Now.AddDays(1);

            #endregion

            if (pagenumber > 0)
            {
                pagenumber -= 1;
            }

            var sqlDatas = _purchaseReportService.GetPurchaseReportSummaryProduct(curStore?.Id ?? 0,
                categoryId, productId, productName, manufacturerId, wareHouseId,
                model.StartTime, model.EndTime
                ).Select(sd =>
                {
                    return sd ?? new PurchaseReportSummaryProduct();

                }).AsQueryable();

            var items = new PagedList<PurchaseReportSummaryProduct>(sqlDatas, pagenumber, 30);

            model.Items = items;

            #region 汇总

            if (sqlDatas != null && sqlDatas.ToList().Count > 0)
            {

                //采购数量
                model.TotalSumPurchaseQuantityConversion = sqlDatas.Sum(a => a.PurchaseBigQuantity ?? 0) + "大" + sqlDatas.Sum(a => a.PurchaseStrokeQuantity ?? 0) + "中" + sqlDatas.Sum(a => a.PurchaseSmallQuantity ?? 0) + "小";
                //采购金额
                model.TotalSumPurchaseAmount = items.Sum(a => a.PurchaseAmount ?? 0);

                //赠送数量
                model.TotalSumGiftQuantityConversion = sqlDatas.Sum(a => a.GiftBigQuantity ?? 0) + "大" + sqlDatas.Sum(a => a.GiftStrokeQuantity ?? 0) + "中" + sqlDatas.Sum(a => a.GiftSmallQuantity ?? 0) + "小";

                //退购数量
                model.TotalSumPurchaseReturnQuantityConversion = sqlDatas.Sum(a => a.PurchaseReturnBigQuantity ?? 0) + "大" + sqlDatas.Sum(a => a.PurchaseReturnStrokeQuantity ?? 0) + "中" + sqlDatas.Sum(a => a.PurchaseReturnSmallQuantity ?? 0) + "小";
                //退购金额
                model.TotalSumPurchaseReturnAmount = sqlDatas.Sum(a => a.PurchaseReturnAmount ?? 0);

                //数量小计
                model.TotalSumQuantityConversion = (sqlDatas.Sum(a => a.PurchaseBigQuantity ?? 0) + sqlDatas.Sum(a => a.GiftBigQuantity ?? 0) - sqlDatas.Sum(a => a.PurchaseReturnBigQuantity ?? 0)) + "大" +
                                                   (sqlDatas.Sum(a => a.PurchaseStrokeQuantity ?? 0) + sqlDatas.Sum(a => a.GiftStrokeQuantity ?? 0) - sqlDatas.Sum(a => a.PurchaseReturnStrokeQuantity ?? 0)) + "中" +
                                                   (sqlDatas.Sum(a => a.PurchaseSmallQuantity ?? 0) + sqlDatas.Sum(a => a.GiftSmallQuantity ?? 0) - sqlDatas.Sum(a => a.PurchaseReturnSmallQuantity ?? 0)) + "小";
                //金额小计
                model.TotalSumAmount = sqlDatas.Sum(a => a.SumAmount ?? 0);

            }

            #endregion

            model.PagingFilteringContext.LoadPagedList(items);

            return View(model);
        }

        //采购汇总（按商品）导出
        [AuthCode((int)AccessGranularityEnum.PurchaseSummaryByProductExport)]
        public FileResult ExportPurchaseReportSummaryProduct(int? categoryId, int? productId, string productName, int? manufacturerId, int? wareHouseId, DateTime? startTime, DateTime? endTime)
        {

            #region 查询导出数据

            var sqlDatas = _purchaseReportService.GetPurchaseReportSummaryProduct(curStore?.Id ?? 0,
                categoryId, productId, productName, manufacturerId, wareHouseId,
                startTime ?? DateTime.Now, endTime ?? DateTime.Now.AddDays(1)
                ).Select(sd =>
                {
                    return sd ?? new PurchaseReportSummaryProduct();

                }).AsQueryable();
            #endregion

            var datas = new List<PurchaseReportSummaryProduct>(sqlDatas);

            #region 导出
            var ms = _exportManager.ExportPurchaseReportSummaryProductToXlsx(datas);
            if (ms != null)
            {
                return File(ms, "application/vnd.ms-excel", "采购汇总（按商品）.xlsx");
            }
            else
            {
                return File(new MemoryStream(), "application/vnd.ms-excel", "采购汇总（按商品）.xlsx");
            }
            #endregion
        }

        #endregion

        #region 采购汇总（按供应商）
        /// <summary>
        /// 采购汇总（按供应商）
        /// </summary>
        /// <param name="startTime">开始日期</param>
        /// <param name="endTime">结束日期</param>
        /// <param name="manufacturerId">供应商Id</param>
        /// <param name="pagenumber"></param>
        /// <returns></returns>
        [HttpGet]
        [AuthCode((int)AccessGranularityEnum.PurchaseSummaryBySupplierView)]
        public IActionResult PurchaseReportSummaryManufacturer(DateTime? startTime, DateTime? endTime, int? manufacturerId, int pagenumber = 0)
        {


            var model = new PurchaseReportSummaryManufacturerListModel();

            #region 绑定数据源

            //商品统计类别动态列
            var statisticalTypes = _statisticalTypeService.GetAllStatisticalTypess(curStore?.Id ?? 0);
            Dictionary<int, string> dic = new Dictionary<int, string>();
            if (statisticalTypes != null && statisticalTypes.Count > 0)
            {
                //model.DynamicColumns = statisticalTypes.OrderBy(a => a.Id).Select(a => a.Name).ToList();
                foreach (var c in statisticalTypes)
                {
                    model.DynamicColumns.Add(c.Name);
                    dic.Add(c.Id, c.Name);
                }
            }
            model.DynamicColumns.Add("其他");
            dic.Add((int)StatisticalTypeEnum.OtherTypeId, "其他");

            model.StartTime = startTime ?? DateTime.Parse(DateTime.Now.ToString("yyyy-MM-01"));
            model.EndTime = endTime ?? DateTime.Now.AddDays(1);

            //供应商
            model.Manufacturers = BindManufacturerSelection(_manufacturerService.BindManufacturerList, curStore);
            model.ManufacturerId = (manufacturerId ?? null);

            #endregion

            if (pagenumber > 0)
            {
                pagenumber -= 1;
            }

            var sqlDatas = _purchaseReportService.GetPurchaseReportSummaryManufacturer(curStore?.Id ?? 0, model.StartTime, model.EndTime, manufacturerId, dic
                ).Select(sd =>
                {
                    return sd ?? new PurchaseReportSummaryManufacturer();

                }).AsQueryable();

            var items = new PagedList<PurchaseReportSummaryManufacturer>(sqlDatas, pagenumber, 30);

            model.Items = items;

            #region 汇总

            //动态列汇总
            if (model.DynamicColumns != null && model.DynamicColumns.Count > 0 && dic.Keys != null && dic.Keys.Count > 0)
            {
                foreach (var k in dic.Keys)
                {
                    model.TotalDynamicDatas.Add(new PurchaseReportSumStatisticalType()
                    {
                        StatisticalTypeId = k
                    });
                }
            }
            //动态列
            if (model.Items != null && model.Items.Count > 0)
            {
                model.Items.ToList().ForEach(a =>
                {
                    if (a.PurchaseReportStatisticalTypes != null && a.PurchaseReportStatisticalTypes.Count > 0)
                    {
                        a.PurchaseReportStatisticalTypes.ToList().ForEach(b =>
                        {
                            PurchaseReportSumStatisticalType purchaseReportSumStatisticalType = model.TotalDynamicDatas.Where(c => c.StatisticalTypeId == b.StatisticalTypeId).FirstOrDefault();
                            if (purchaseReportSumStatisticalType != null)
                            {
                                purchaseReportSumStatisticalType.PurchaseSmallQuantity = (purchaseReportSumStatisticalType.PurchaseSmallQuantity ?? 0) + (b.SmallQuantity ?? 0);
                                //purchaseReportSumStatisticalType.SumPurchaseSmallUnitConversion = (purchaseReportSumStatisticalType.PurchaseSmallQuantity ?? 0) + (b.SmallQuantity ?? 0) + a.SumPurchaseSmallUnitConversion;
                                purchaseReportSumStatisticalType.OrderAmount = (purchaseReportSumStatisticalType.OrderAmount ?? 0) + (b.OrderAmount ?? 0);
                            }
                        });
                    }
                });
            }
            //主列
            if (model.Items != null && model.Items.Count > 0)
            {
                //采购数量
                model.TotalSumPurchaseSmallQuantity = model.Items.Sum(m => m.SumPurchaseSmallQuantity ?? 0);
                //单据金额
                model.TotalSumOrderAmount = model.Items.Sum(m => m.SumOrderAmount ?? 0);
            }

            #endregion

            model.PagingFilteringContext.LoadPagedList(items);

            return View(model);
        }

        //采购汇总（按供应商）导出
        [AuthCode((int)AccessGranularityEnum.PurchaseSummaryBySupplierExport)]
        public FileResult ExportPurchaseReportSummaryManufacturer(DateTime? startTime, DateTime? endTime, int? manufacturerId)
        {
            //商品统计类别动态列
            var statisticalTypes = _statisticalTypeService.GetAllStatisticalTypess(curStore?.Id ?? 0);
            Dictionary<int, string> dic = new Dictionary<int, string>();
            if (statisticalTypes != null && statisticalTypes.Count > 0)
            {
                foreach (var c in statisticalTypes)
                {
                    dic.Add(c.Id, c.Name);
                }
            }
            dic.Add((int)StatisticalTypeEnum.OtherTypeId, "其他");

            var sqlDatas = _purchaseReportService.GetPurchaseReportSummaryManufacturer(curStore?.Id ?? 0, startTime ?? DateTime.Now, endTime ?? DateTime.Now.AddDays(1), manufacturerId, dic);

            #region 导出
            var ms = _exportManager.ExportPurchaseReportSummaryManufacturerToXlsx(sqlDatas, curStore.Id);
            if (ms != null)
            {
                return File(ms, "application/vnd.ms-excel", "采购汇总（按供应商）.xlsx");
            }
            else
            {
                return File(new MemoryStream(), "application/vnd.ms-excel", "采购汇总（按供应商）.xlsx");
            }
            #endregion
        }

        #endregion


    }
}