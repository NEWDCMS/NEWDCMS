using DCMS.Core;
using DCMS.Core.Domain.Products;
using DCMS.Core.Domain.Report;
using DCMS.Services.ExportImport;
using DCMS.Services.Logging;
using DCMS.Services.Messages;
using DCMS.Services.Products;
using DCMS.Services.Report;
using DCMS.Services.Users;
using DCMS.Services.WareHouses;
using DCMS.ViewModel.Models.Report;
using DCMS.Web.Framework.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DCMS.Services.Census;

namespace DCMS.Web.Controllers
{
    /// <summary>
    /// 报表 员工报表
    /// </summary>
    public class StaffReportController : BasePublicController
    {
        private readonly IUserService _userService;
        private readonly IWareHouseService _wareHouseService;
        private readonly IProductService _productService;
        private readonly ICategoryService _productCategoryService;
        private readonly IStaffReportService _staffReportService;
        private readonly IExportManager _exportManager;
        private readonly IVisitStoreService _visitStoreService;


        public StaffReportController(
            IStoreContext storeContext,
            IWorkContext workContext,
            IUserService userService,
            IWareHouseService wareHouseService,
            IProductService productService,
            ICategoryService productCategoryService,
            IStaffReportService staffReportService,
            INotificationService notificationService,
            IExportManager exportManager,
            IVisitStoreService visitStoreService,
            ILogger loggerService) : base(workContext, loggerService, storeContext, notificationService)
        {
            _userService = userService;
            _wareHouseService = wareHouseService;
            _productService = productService;
            _productCategoryService = productCategoryService;
            _staffReportService = staffReportService;
            _exportManager = exportManager;
            _visitStoreService = visitStoreService;
        }

        #region 业务员业绩

        /// <summary>
        /// 业务员业绩
        /// </summary>
        /// <param name="categoryId">商品类别Id</param>
        /// <param name="wareHouseId">仓库Id</param>
        /// <param name="terminalId">客户Id</param>
        /// <param name="terminalName">客户名称</param>
        /// <param name="topNumber">统计前</param>
        /// <param name="startTime">开始日期</param>
        /// <param name="endTime">结束日期</param>
        /// <param name="pagenumber"></param>
        /// <returns></returns>
        [HttpGet]
        [AuthCode((int)AccessGranularityEnum.SalespersonPerformanceView)]
        public IActionResult StaffReportBusinessUserAchievement(int? categoryId, int? wareHouseId, int? terminalId, string terminalName, int? topNumber, DateTime? startTime, DateTime? endTime, int pagenumber = 0)
        {


            var model = new StaffReportBusinessUserAchievementListModel();

            #region 绑定数据源

            //商品类别
            model.Categories = BindCategorySelection(new Func<int?, IList<Category>>(_productCategoryService.GetAllCategoriesDisplayed), curStore);
            model.CategoryId = categoryId ?? null;

            //仓库
            model.WareHouses = BindWareHouseSelection(_wareHouseService.BindWareHouseList, curStore, null, 0);
            model.WareHouseId = wareHouseId ?? null;

            //客户
            model.TerminalId = terminalId;
            model.TerminalName = terminalName;

            //统计前
            model.TopNumber = (topNumber ?? null);

            model.StartTime = startTime ?? DateTime.Parse(DateTime.Now.ToString("yyyy-MM-01"));
            model.EndTime = endTime ?? DateTime.Now.AddDays(1);

            #endregion

            if (pagenumber > 0)
            {
                pagenumber -= 1;
            }

            var sqlDatas = _staffReportService.GetStaffReportBusinessUserAchievement(curStore?.Id ?? 0,
                categoryId, wareHouseId, terminalId, topNumber,
                model.StartTime, //?? DateTime.Parse(DateTime.Now.ToString("yyyy-MM-dd")),
                model.EndTime //?? DateTime.Now.AddDays(1),
                ).Select(sd =>
                {
                    return sd == null ? new StaffReportBusinessUserAchievement() : sd;

                }).AsQueryable();

            var items = new PagedList<StaffReportBusinessUserAchievement>(sqlDatas, pagenumber, 30);

            model.Items = items;

            #region 汇总

            if (sqlDatas != null && sqlDatas.ToList().Count > 0)
            {

                // 销售金额（总）
                model.TotalSumSaleAmount = model.Items.Sum(m => m.SaleAmount ?? 0);

                // 退货金额（总）
                model.TotalSumReturnAmount = model.Items.Sum(m => m.ReturnAmount ?? 0);

                // 净销金额（总）
                model.TotalSumNetAmount = model.TotalSumSaleAmount - model.TotalSumReturnAmount;

            }

            #endregion

            model.PagingFilteringContext.LoadPagedList(items);

            //设置图表数据
            model.Charts = "";
            if (model.Items != null && model.Items.Count > 0)
            {
                foreach (var item in model.Items.OrderByDescending(m => m.NetAmount).ToList())
                {
                    model.Charts = model.Charts + (model.Charts == "" ? "" : ",") + item.BusinessUserName + "|" + item.SaleAmount + "|" + item.ReturnAmount + "|" + item.NetAmount;
                }
            }

            return View(model);
        }

        //业务员业绩导出
        [AuthCode((int)AccessGranularityEnum.SalespersonPerformanceExport)]
        public FileResult ExportStaffReportBusinessUserAchievement(int? categoryId, int? wareHouseId, int? terminalId, string terminalName, int? topNumber, DateTime? startTime, DateTime? endTime)
        {

            #region 查询导出数据

            var sqlDatas = _staffReportService.GetStaffReportBusinessUserAchievement(curStore?.Id ?? 0,
          categoryId, wareHouseId, terminalId, topNumber,
          (startTime == null) ? DateTime.Parse(DateTime.Now.ToString("yyyy-MM-01")) : startTime,
          (endTime == null) ? DateTime.Now.AddDays(1) : endTime
          );
            #endregion

            #region 导出
            var ms = _exportManager.ExportStaffReportBusinessUserAchievementToXlsx(sqlDatas);
            if (ms != null)
            {
                return File(ms, "application/vnd.ms-excel", "业务员业绩.xlsx");
            }
            else
            {
                return File(new MemoryStream(), "application/vnd.ms-excel", "业务员业绩.xlsx");
            }
            #endregion
        }

        #endregion

        #region 员工提成汇总表
        /// <summary>
        /// 员工提成汇总表
        /// </summary>
        /// <param name="startTime">开始日期</param>
        /// <param name="endTime">结束日期</param>
        /// <param name="staffUserId">员工Id</param>
        /// <param name="categoryId">商品类别Id</param>
        /// <param name="productId">商品Id</param>
        /// <param name="productName">商品名称</param>
        /// <param name="pagenumber"></param>
        /// <returns></returns>
        [HttpGet]
        [AuthCode((int)AccessGranularityEnum.RoyaltySummaryView)]
        public IActionResult StaffReportPercentageSummary(DateTime? startTime, DateTime? endTime, int? staffUserId, int? categoryId, int? productId, string productName, int pagenumber = 0)
        {


            var model = new StaffReportPercentageSummaryListModel();

            #region 绑定数据源
            model.StartTime = startTime ?? DateTime.Now.AddDays(-100);
            model.EndTime = endTime ?? DateTime.Now.AddDays(1);

            //员工
            model.StaffUsers = BindUserSelection(_userService.BindUserList, curStore, "");
            model.StaffUserId = staffUserId ?? null;

            //商品类别
            model.Categories = BindCategorySelection(new Func<int?, IList<Category>>(_productCategoryService.GetAllCategoriesDisplayed), curStore);
            model.CategoryId = categoryId ?? null;

            //商品
            model.ProductId = productId;
            model.ProductName = productName;

            #endregion

            if (pagenumber > 0)
            {
                pagenumber -= 1;
            }

            var sqlDatas = _staffReportService.GetStaffReportPercentageSummary(curStore?.Id ?? 0,
                 model.StartTime,
                model.EndTime,
                staffUserId, categoryId, productId
                ).Select(sd =>
                {
                    return sd == null ? new StaffReportPercentageSummary() : sd;

                }).AsQueryable();

            var items = new PagedList<StaffReportPercentageSummary>(sqlDatas, pagenumber, 30);

            model.Items = items;

            #region 汇总

            if (sqlDatas != null && sqlDatas.ToList().Count > 0)
            {

                /// 业务提成（总）
                model.TotalSumBusinessPercentage = model.Items.Sum(m => m.BusinessPercentage ?? 0);

                /// 送货提成（总）
                model.TotalSumDeliveryPercentage = model.Items.Sum(m => m.DeliveryPercentage ?? 0);

                /// 提成合计（总）
                model.TotalSumPercentageTotal = model.Items.Sum(m => m.PercentageTotal ?? 0);

            }

            #endregion

            model.PagingFilteringContext.LoadPagedList(items);

            return View(model);
        }

        //员工提成汇总表导出
        [HttpGet]
        [AuthCode((int)AccessGranularityEnum.RoyaltySummaryExport)]
        public FileResult ExportStaffReportPercentageSummary(DateTime? startTime, DateTime? endTime, int? staffUserId, int? categoryId, int? productId, string productName)
        {

            #region 查询导出数据

            var sqlDatas = _staffReportService.GetStaffReportPercentageSummary(curStore?.Id ?? 0,
                (startTime == null) ? DateTime.Now.AddDays(-100) : startTime,
                (endTime == null) ? DateTime.Now.AddDays(1) : endTime,
                staffUserId, categoryId, productId
                );
            #endregion

            #region 导出
            var ms = _exportManager.ExportStaffReportPercentageSummaryToXlsx(sqlDatas);
            if (ms != null)
            {
                return File(ms, "application/vnd.ms-excel", "员工提成汇总表.xlsx");
            }
            else
            {
                return File(new MemoryStream(), "application/vnd.ms-excel", "员工提成汇总表.xlsx");
            }
            #endregion

        }

        #endregion

        /// <summary>
        /// 员工提成明细表
        /// </summary>
        /// <param name="userType">用户类型（1业务员、2送货员）</param>
        /// <param name="startTime">开始日期</param>
        /// <param name="endTime">结束日期</param>
        /// <param name="staffUserId">员工Id</param>
        /// <param name="categoryId">商品类别Id</param>
        /// <param name="productId">商品Id</param>
        /// <param name="pagenumber"></param>
        /// <returns></returns>
        [HttpGet]
        [ClearCache("GETSTAFFREPORT_PERCENTAGE_PK.{0}")]
        public IActionResult StaffReportPercentageItem(int userType, DateTime? startTime, DateTime? endTime, int? staffUserId, int? categoryId, int? productId, int pagenumber = 0)
        {
            var model = new StaffReportPercentageItemListModel();

            if (pagenumber > 0)
                pagenumber -= 1;

            model.StaffUserName = _userService.GetUserName(curStore.Id, staffUserId ?? 0);
            model.StartTime = (startTime == null) ? DateTime.Parse(DateTime.Now.ToString("yyyy-MM-01")) : (DateTime)startTime;
            model.EndTime = (endTime == null) ? DateTime.Now.AddDays(1) : (DateTime)endTime;
            model.CategoryName = _productCategoryService.GetCategoryName(curStore.Id, categoryId ?? 0);
            model.ProductName = _productService.GetProductName(curStore.Id, productId ?? 0);

            var percentageItems = _staffReportService.GetStaffReportPercentageItem(curStore?.Id ?? 0,
                userType, model.StartTime,
                model.EndTime,
                staffUserId,
                categoryId,
                productId);

            var items = new PagedList<StaffReportPercentageItem>(percentageItems, pagenumber, 30);

            model.Items = items;
            model.PagingFilteringContext.LoadPagedList(items);

            return View(model);
        }


        /// <summary>
        /// 员工销量提成导出
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public IActionResult StaffExport(int? staffUserId, DateTime? startTime = null, DateTime? endTime = null)
        {
            if (!staffUserId.HasValue)
            {
                return RedirectToAction("List");
            }

            #region 查询导出数据

            var report = _staffReportService.StaffSaleQuery(curStore.Id, staffUserId, startTime, endTime);
            var user = _userService.GetUserById(curStore.Id, staffUserId ?? 0);

            #endregion

            #region 导出

            var ms = _exportManager.ExportStaffSaleQueryToXlsx(report, curStore.Id);
            if (ms != null)
            {
                return File(ms, "application/vnd.ms-excel", $"{user.UserRealName}_提成销量.xlsx");
            }
            else
            {
                return File(new MemoryStream(), "application/vnd.ms-excel", "销售单.xlsx");
            }

            #endregion

        }


        /// <summary>
        /// 拜访汇总
        /// </summary>
        /// <param name="type"></param>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <param name="staffUserId"></param>
        /// <returns></returns>
        [HttpGet]
        public IActionResult VisitSummery(int? type, DateTime? startTime, DateTime? endTime, int? staffUserId)
        {
            var model = new VisitSummeryListModel();

            try
            {
                model.StartTime = (startTime == null) ? DateTime.Parse(DateTime.Now.ToString("yyyy-MM-01")) : (DateTime)startTime;
                model.EndTime = (endTime == null) ? DateTime.Now.AddDays(1) : (DateTime)endTime;

                var visitSummery = _staffReportService.GetVisitSummeryQuery(type ?? 0, curStore?.Id ?? 0, staffUserId ?? 0);
                model.Items = visitSummery;
            }
            catch (Exception)
            { 
            
            }

            return View(model);
        }

        /// <summary>
        /// 异步获取拜访汇总
        /// </summary>
        /// <param name="type"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public async Task<JsonResult> AsyncVisitSummery(int? type, DateTime? start = null, DateTime? end = null, int pageIndex = 0, int pageSize = 50)
        {
            return await Task.Run(() =>
            {
                var visitSummery = _staffReportService.GetVisitSummeryQuery(type ?? 0, curStore?.Id ?? 0, null, start, end);
                visitSummery.ToList().ForEach(v=> 
                {
                    var rst = _visitStoreService.GetAllVisitReacheds(curStore?.Id ?? 0, v.UserId, null, start, end).Select(s=>s.OnStoreStopSeconds).Sum();
                    v.TotalDuration = GetTime(rst ?? 0);
                });
                return Json(new
                {
                    Success = true,
                    total = visitSummery.Count(),
                    rows = visitSummery.ToList()
                });

            });
        }


        /// <summary>
        /// 拜访汇总导出
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public IActionResult VisitExport(int? type, int? staffUserId, DateTime? start = null, DateTime? end = null)
        {
            if (!staffUserId.HasValue)
            {
                return RedirectToAction("List");
            }

            #region 查询导出数据

            var report = _staffReportService.GetVisitSummeryQuery(type ?? 8, curStore?.Id ?? 0, staffUserId, start, end);
            var user = _userService.GetUserById(curStore.Id, staffUserId ?? 0);

            #endregion

            #region 导出

            var ms = _exportManager.ExportVisitSummeryQueryToXlsx(report, curStore.Id);
            if (ms != null)
            {
                return File(ms, "application/vnd.ms-excel", $"{user.UserRealName}_拜访量汇总.xlsx");
            }
            else
            {
                return File(new MemoryStream(), "application/vnd.ms-excel", "销售单.xlsx");
            }

            #endregion
        }

        [HttpGet]
        public IActionResult VisitExportAll(int? type, DateTime? start = null, DateTime? end = null)
        {

            #region 查询导出数据

            var report = _staffReportService.GetVisitSummeryQuery(type ?? 0, curStore?.Id ?? 0, null, start, end);

            #endregion

            #region 导出

            var ms = _exportManager.ExportVisitSummeryQueryToXlsx(report, curStore.Id);
            if (ms != null)
            {
                return File(ms, "application/vnd.ms-excel", $"月拜访量汇总.xlsx");
            }
            else
            {
                return File(new MemoryStream(), "application/vnd.ms-excel", "销售单.xlsx");
            }

            #endregion
        }


        /// <summary>
        /// 业务员每日拜访明细
        /// </summary>
        /// <param name="storeId"></param>
        /// <param name="year"></param>
        /// <param name="month"></param>
        /// <returns></returns>
        [HttpGet]
        public IActionResult BusinessUserVisitOfYear(int? storeId, int year,int month)
        {
            var model = new BusinessUserVisitOfYearListModel();

            try
            {
                model.Year = (year == 0) ? DateTime.Now.Year : year;
                model.Month = (month == 0) ? DateTime.Now.Month : month;
            }
            catch (Exception)
            {

            }

            return View(model);
        }
        /// <summary>
        /// 获取业务员每日拜访明细
        /// </summary>
        /// <param name="year"></param>
        /// <param name="month"></param>
        /// <returns></returns>
        public async Task<JsonResult> GetBusinessUserVisitOfYearList(int year, int month)
        {
            return await Task.Run(() =>
            {
                var userVisitOfYear = _staffReportService.GetBusinessUserVisitOfYearList(curStore?.Id ?? 0, year, month);
                return Json(new
                {
                    Success = true,
                    total = userVisitOfYear.Count(),
                    rows = userVisitOfYear.ToList()
                });

            });
        }

        /// <summary>
        /// 导出
        /// </summary>
        /// <param name="terminalId"></param>
        /// <param name="billSourceType"></param>
        /// <returns></returns>
        [HttpGet]
        public FileResult ExportBusinessUserVisitOfYear(int year,int month)
        {

            #region 查询导出数据
            var userVisitOfYear = _staffReportService.GetBusinessUserVisitOfYearList(curStore?.Id ?? 0, year, month);
            #endregion

            #region 导出
            var ms = _exportManager.ExportBusinessUserVisitOfYear(userVisitOfYear,year,month);
            if (ms != null)
            {
                return File(ms, "application/vnd.ms-excel", $"业务员拜访统计{year}年{month}月.xlsx");
            }
            else
            {
                return File(new MemoryStream(), "application/vnd.ms-excel", "业务员拜访统计.xlsx");
            }
            #endregion

        }
        private string GetTime(int time)
        {
            //小时计算
            int hours = (time) % (24 * 3600) / 3600;
            //分钟计算
            int minutes = (time) % 3600 / 60;

            return hours + "时" + minutes + "分";
        }
    }
}