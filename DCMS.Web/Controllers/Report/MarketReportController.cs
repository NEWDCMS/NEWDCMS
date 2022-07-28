using DCMS.Core;
using DCMS.Core.Domain.Report;
using DCMS.Services.ExportImport;
using DCMS.Services.Global.Common;
using DCMS.Services.Logging;
using DCMS.Services.Messages;
using DCMS.Services.Products;
using DCMS.Services.Report;
using DCMS.Services.Terminals;
using DCMS.Services.Users;
using DCMS.ViewModel.Models.Report;
using DCMS.Web.Framework.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using System;
using System.IO;
using System.Linq;

namespace DCMS.Web.Controllers
{
    /// <summary>
    /// 报表 市场报表
    /// </summary>
    public class MarketReportController : BasePublicController
    {
        private readonly IUserService _userService;
        private readonly IExportService _exportService;
        private readonly IDistrictService _districtService;
        private readonly ICategoryService _productCategoryService;
        private readonly IBrandService _brandService;
        private readonly IMarketReportService _marketReportService;
        private readonly IExportManager _exportManager;

        public MarketReportController(
            IStoreContext storeContext,
            IWorkContext workContext,
            IUserService userService,
            IExportService exportService,
            IDistrictService districtService,
            ICategoryService productCategoryService,
            IBrandService brandService,
            IMarketReportService marketReportService,
            INotificationService notificationService,
            ILogger loggerService,
            IExportManager exportManager
            ) : base(workContext, loggerService, storeContext, notificationService)
        {
            _userService = userService;
            _exportService = exportService;
            _districtService = districtService;
            _productCategoryService = productCategoryService;
            _brandService = brandService;
            _marketReportService = marketReportService;
            _exportManager = exportManager;
        }

        #region 客户活跃度
        /// <summary>
        /// 客户活跃度
        /// </summary>
        /// <param name="noVisitDayMore">无拜访天数大于</param>
        /// <param name="terminalId">客户Id</param>
        /// <param name="terminalName">客户名称</param>
        /// <param name="noSaleDayMore">无销售天数大于</param>
        /// <param name="districtId">客户片区Id</param>
        /// <param name="staffUserId">员工Id</param>
        /// <param name="pagenumber"></param>
        /// <returns></returns>
        [HttpGet]
        [AuthCode((int)AccessGranularityEnum.CustomerActivityView)]
        public IActionResult MarketReportTerminalActive(int? noVisitDayMore, int? terminalId, string terminalName, int? noSaleDayMore, int? districtId, int? staffUserId,
            int pagenumber = 0)
        {

            var model = new MarketReportTerminalActiveListModel();

            #region 绑定数据源

            //无拜访天数大于
            model.NoVisitDayMore = (noVisitDayMore ?? null);

            //客户
            model.TerminalId = terminalId;
            model.TerminalName = terminalName;

            //无销售天数大于
            model.NoSaleDayMore = (noSaleDayMore ?? null);

            //客户片区
            model.Districts = BindDistrictSelection(_districtService.BindDistrictList, curStore);
            model.DistrictId = (districtId ?? null);

            //员工
            model.StaffUsers = BindUserSelection(_userService.BindUserList, curStore, DCMSDefaults.Salesmans);
            model.StaffUserId = (staffUserId ?? null);

            #endregion

            if (pagenumber > 0)
            {
                pagenumber -= 1;
            }

            var sqlDatas = _marketReportService.GetMarketReportTerminalActive(curStore?.Id ?? 0,
                 noVisitDayMore, terminalId, terminalName, noSaleDayMore, districtId, staffUserId
                ).Select(sd =>
                {
                    return sd ?? new MarketReportTerminalActive();

                }).AsQueryable();

            var items = new PagedList<MarketReportTerminalActive>(sqlDatas, pagenumber, 30);

            model.Items = items;

            #region 汇总
            #endregion

            model.PagingFilteringContext.LoadPagedList(items);

            return View(model);
        }

        //客户活跃度导出
        [HttpGet]
        [AuthCode((int)AccessGranularityEnum.CustomerActivityExport)]
        public FileResult ExportMarketReportTerminalActive(int? noVisitDayMore, int? terminalId, string terminalName, int? noSaleDayMore, int? districtId, int? staffUserId)
        {

            #region 查询导出数据

            var sqlDatas = _marketReportService.GetMarketReportTerminalActive(curStore?.Id ?? 0,
                 noVisitDayMore, terminalId, terminalName, noSaleDayMore, districtId, staffUserId);
            #endregion

            #region 导出
            var ms = _exportManager.ExportMarketReportTerminalActiveToXlsx(sqlDatas);
            if (ms != null)
            {
                return File(ms, "application/vnd.ms-excel", "客户活跃度.xlsx");
            }
            else
            {
                return File(new MemoryStream(), "application/vnd.ms-excel", "客户活跃度.xlsx");
            }
            #endregion

        }

        #endregion

        #region 客户价值分析
        /// <summary>
        /// 客户价值分析
        /// </summary>
        /// <param name="terminalId">客户Id</param>
        /// <param name="terminalName">客户名称</param>
        /// <param name="districtId">客户片区Id</param>
        /// <param name="type">客户价值类型</param>
        /// <param name="pagenumber"></param>
        /// <returns></returns>
        [HttpGet]
        [AuthCode((int)AccessGranularityEnum.CustomerAnalysisView)]
        public IActionResult MarketReportTerminalValueAnalysis(int? terminalId, string terminalName, int? districtId, int? type, int pagenumber = 0)
        {

            var model = new MarketReportTerminalValueAnalysisListModel();

            #region 绑定数据源

            //客户
            model.TerminalId = terminalId;
            model.TerminalName = terminalName;

            //客户片区
            model.Districts = BindDistrictSelection(_districtService.BindDistrictList, curStore);
            model.DistrictId = (districtId ?? null);

            #endregion

            if (pagenumber > 0)
            {
                pagenumber -= 1;
            }

            model.TerminalValueId = type;

            model.Groups.Add(new ValueAnalysisGroupModel { TerminalValueId = 8, TerminalValueName = $" 高价值客户 0" });
            model.Groups.Add(new ValueAnalysisGroupModel { TerminalValueId = 7, TerminalValueName = $" 高重点保护客户 0" });
            model.Groups.Add(new ValueAnalysisGroupModel { TerminalValueId = 6, TerminalValueName = $" 重点发展客户 0" });
            model.Groups.Add(new ValueAnalysisGroupModel { TerminalValueId = 5, TerminalValueName = $" 重点挽留客户 0" });
            model.Groups.Add(new ValueAnalysisGroupModel { TerminalValueId = 4, TerminalValueName = $" 一般价值客户 0" });
            model.Groups.Add(new ValueAnalysisGroupModel { TerminalValueId = 3, TerminalValueName = $" 一般保持客户 0" });
            model.Groups.Add(new ValueAnalysisGroupModel { TerminalValueId = 2, TerminalValueName = $" 一般发展客户 0" });
            model.Groups.Add(new ValueAnalysisGroupModel { TerminalValueId = 1, TerminalValueName = $" 潜在客户 0" });
            var lists = _marketReportService.GetMarketReportTerminalValueAnalysis(curStore?.Id ?? 0, terminalId, terminalName, districtId).OrderByDescending(o => o.RFMScore).ToList();
            foreach (IGrouping<int, MarketReportTerminalValueAnalysis> group in lists.GroupBy(s => s.TerminalTypeId))
            {
                var grp = model.Groups.Where(s => s.TerminalValueId == group.Key).First();
                grp.TerminalValueName = $" {group.First().TerminalTypeName} {group.Count()}";
            }

            if (type.HasValue && type.Value > 0)
            {
                lists = lists.Where(s => s.TerminalTypeId == type).ToList();
            }

            var items = new PagedList<MarketReportTerminalValueAnalysis>(lists, pagenumber, 30);

            model.Items = items;
            model.PagingFilteringContext.LoadPagedList(items);

            return View(model);
        }

        //客户价值分析导出
        [AuthCode((int)AccessGranularityEnum.CustomerAnalysisExport)]
        public FileResult ExportMarketReportTerminalValueAnalysis(int? terminalId, string terminalName, int? districtId, int? type)
        {
            #region 查询导出数据

            var sqlDatas = _marketReportService.GetMarketReportTerminalValueAnalysis(curStore?.Id ?? 0, terminalId, terminalName, districtId);

            if (type.HasValue && type.Value > 0)
            {
                sqlDatas = sqlDatas.Where(s => s.TerminalTypeId == type).ToList();
            }

            #endregion

            #region 导出
            var ms = _exportManager.ExportMarketReportTerminalValueAnalysisToXlsx(sqlDatas);
            if (ms != null)
            {
                return File(ms, "application/vnd.ms-excel", "客户价值分析.xlsx");
            }
            else
            {
                return File(new MemoryStream(), "application/vnd.ms-excel", "客户价值分析.xlsx");
            }
            #endregion
        }

        #endregion

        #region 客户流失预警
        /// <summary>
        /// 客户流失预警
        /// </summary>
        /// <param name="terminalId">客户Id</param>
        /// <param name="terminalName">客户名称</param>
        /// <param name="districtId">客户片区Id</param>
        /// <param name="terminalValueId">客户价值Id</param>
        /// <param name="pagenumber"></param>
        /// <returns></returns>
        [HttpGet]
        [AuthCode((int)AccessGranularityEnum.LossWarningView)]
        public IActionResult MarketReportTerminalLossWarning(int? terminalId, string terminalName, int? districtId, int? type, int pagenumber = 0)
        {

            var model = new MarketReportTerminalValueAnalysisListModel
            {

                //客户
                TerminalId = terminalId,
                TerminalName = terminalName,

                //客户片区
                Districts = BindDistrictSelection(_districtService.BindDistrictList, curStore),
                DistrictId = (districtId ?? null),

                TerminalValueId = type
            };

            model.Groups.Add(new ValueAnalysisGroupModel { TerminalValueId = 8, TerminalValueName = $" 高价值客户 0" });
            model.Groups.Add(new ValueAnalysisGroupModel { TerminalValueId = 7, TerminalValueName = $" 高重点保护客户 0" });
            model.Groups.Add(new ValueAnalysisGroupModel { TerminalValueId = 6, TerminalValueName = $" 重点发展客户 0" });
            model.Groups.Add(new ValueAnalysisGroupModel { TerminalValueId = 5, TerminalValueName = $" 重点挽留客户 0" });
            model.Groups.Add(new ValueAnalysisGroupModel { TerminalValueId = 4, TerminalValueName = $" 一般价值客户 0" });
            model.Groups.Add(new ValueAnalysisGroupModel { TerminalValueId = 3, TerminalValueName = $" 一般保持客户 0" });
            model.Groups.Add(new ValueAnalysisGroupModel { TerminalValueId = 2, TerminalValueName = $" 一般发展客户 0" });
            model.Groups.Add(new ValueAnalysisGroupModel { TerminalValueId = 1, TerminalValueName = $" 潜在客户 0" });
            var lists = _marketReportService.GetMarketReportTerminalValueAnalysis(curStore?.Id ?? 0, terminalId, terminalName, districtId).OrderByDescending(o=>o.RFMScore).ToList();
            foreach (IGrouping<int, MarketReportTerminalValueAnalysis> group in lists.GroupBy(s => s.TerminalTypeId))
            {
                var grp = model.Groups.Where(s => s.TerminalValueId == group.Key).First();
                grp.TerminalValueName = $" {group.First().TerminalTypeName} {group.Count()}";
            }

            if (type.HasValue && type.Value > 0)
            {
                lists = lists.Where(s => s.TerminalTypeId == type).ToList();
            }

            var items = new PagedList<MarketReportTerminalValueAnalysis>(lists, pagenumber, 30);
            model.Items = items;
            model.PagingFilteringContext.LoadPagedList(items);

            return View(model);
        }

        //客户流失预警导出
        [AuthCode((int)AccessGranularityEnum.LossWarningExport)]
        public FileResult ExportMarketReportTerminalLossWarning(int? terminalId, string terminalName, int? districtId, int? type)
        {

            #region 查询导出数据

            var sqlDatas = _marketReportService.GetMarketReportTerminalValueAnalysis(curStore?.Id ?? 0, terminalId, terminalName, districtId);
            if (type.HasValue && type.Value > 0)
            {
                sqlDatas = sqlDatas.Where(s => s.TerminalTypeId == type).ToList();
            }

            #endregion

            #region 导出
            var ms = _exportManager.ExportMarketReportTerminalLossWarningToXlsx(sqlDatas);
            if (ms != null)
            {
                return File(ms, "application/vnd.ms-excel", "客户流失预警.xlsx");
            }
            else
            {
                return File(new MemoryStream(), "application/vnd.ms-excel", "客户流失预警.xlsx");
            }
            #endregion
        }

        #endregion

        #region 铺市率报表

        /// <summary>
        /// 铺市率报表
        /// </summary>
        /// <param name="productId">商品Id</param>
        /// <param name="productName">商品名称</param>
        /// <param name="categoryId">商品类别Id</param>
        /// <param name="brandId">品牌Id</param>
        /// <param name="districtId">客户片区Id</param>
        /// <param name="startTime">开始日期</param>
        /// <param name="endTime">结束日期</param>
        /// <param name="bussinessUserId">业务员Id</param>
        /// <param name="pagenumber"></param>
        /// <returns></returns>
        [HttpGet]
        [AuthCode((int)AccessGranularityEnum.MarketRateReportView)]
        public IActionResult MarketReportShopRate(int? productId, string productName, int? categoryId, int? brandId, int? districtId, DateTime? startTime, DateTime? endTime, int? bussinessUserId, int pagenumber = 0)
        {


            var model = new MarketReportShopRateListModel();

            #region 绑定数据源

            //商品
            model.ProductId = productId;
            model.ProductName = productName;

            //商品类别
            model.Categories = BindCategorySelection(_productCategoryService.BindCategoryList, curStore);
            model.CategoryId = categoryId ?? null;

            //品牌
            model.Brands = BindBrandSelection(_brandService.BindBrandList, curStore);
            model.BrandId = brandId ?? null;

            //客户片区
            model.Districts = BindDistrictSelection(_districtService.BindDistrictList, curStore);
            model.DistrictId = districtId ?? null;

            //业务员
            model.BusinessUsers = BindUserSelection(_userService.BindUserList, curStore, DCMSDefaults.Salesmans,curUser.Id,true, _userService.IsAdmin(curStore.Id, curUser.Id));
            model.BusinessUserId = (bussinessUserId ?? -1);

            model.StartTime = startTime ?? DateTime.Now.AddMonths(-1);
            model.EndTime = endTime ?? DateTime.Now.AddDays(1);

            #endregion

            if (pagenumber > 0)
            {
                pagenumber -= 1;
            }

            var lists = _marketReportService.GetMarketReportShopRate(curStore?.Id ?? 0, productId, productName, categoryId, brandId, districtId, model.StartTime, model.EndTime, bussinessUserId);

            var items = new PagedList<MarketReportShopRate>(lists, pagenumber, 30);
            model.Items = items;
            model.PagingFilteringContext.LoadPagedList(items);

            return View(model);
        }

        //铺市率报表导出
        [AuthCode((int)AccessGranularityEnum.MarketRateReportExport)]
        public FileResult ExportMarketReportShopRate(int? productId, string productName, int? categoryId, int? brandId, int? districtId, DateTime? startTime, DateTime? endTime, int? bussinessUserId)
        {

            #region 查询导出数据

            var sqlDatas = _marketReportService.GetMarketReportShopRate(curStore?.Id ?? 0, productId, productName, categoryId, brandId, districtId, (startTime == null) ? DateTime.Now.AddMonths(-1) : startTime, (endTime == null) ? DateTime.Now.AddDays(1) : endTime, bussinessUserId);

            #endregion

            #region 导出
            var ms = _exportManager.ExportMarketReportShopRateToXlsx(sqlDatas);
            if (ms != null)
            {
                return File(ms, "application/vnd.ms-excel", "铺市率报表.xlsx");
            }
            else
            {
                return File(new MemoryStream(), "application/vnd.ms-excel", "铺市率报表.xlsx");
            }
            #endregion
        }

        #endregion


    }
}