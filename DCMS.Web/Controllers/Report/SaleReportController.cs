using DCMS.Core;
using DCMS.Core.Domain.Configuration;
using DCMS.Core.Domain.Sales;
using DCMS.Services.Configuration;
using DCMS.Services.ExportImport;
using DCMS.Services.Finances;
using DCMS.Services.Global.Common;
using DCMS.Services.Logging;
using DCMS.Services.Messages;
using DCMS.Services.Products;
using DCMS.Services.Report;
using DCMS.Services.Terminals;
using DCMS.Services.Users;
using DCMS.Services.WareHouses;
using DCMS.ViewModel.Models.Sales;
using DCMS.Web.Framework.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using NUglify.Helpers;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;


namespace DCMS.Web.Controllers
{
    /// <summary>
    /// 销售报表
    /// </summary>
    public class SaleReportController : BasePublicController
    {
        private readonly IUserService _userService;
        private readonly IDistrictService _districtService;
        private readonly IChannelService _channelService;
        private readonly IRankService _rankService;
        private readonly IWareHouseService _wareHouseService;
        private readonly ICategoryService _productCategoryService;
        private readonly IBrandService _brandService;
        private readonly IStatisticalTypeService _statisticalTypeService;
        private readonly ISaleReportService _saleReportService;
        private readonly IExportService _exportService;
        private readonly IGiveQuotaService _giveQuotaService;
        private readonly IExportManager _exportManager;
        private readonly IRemarkConfigService _remarkConfigService;
        private readonly ICashReceiptBillService _cashReceiptBillService;

        public SaleReportController(
            IStoreContext storeContext,
            IWorkContext workContext,
            IUserService userService,
            IDistrictService districtService,
            IChannelService channelService,
            IRankService rankService,
            IWareHouseService wareHouseService,
            ICategoryService productCategoryService,
            IBrandService brandService,
            IStatisticalTypeService statisticalTypeService,
            IGiveQuotaService giveQuotaService,
            ISaleReportService saleReportService,
            IExportService exportService,
            INotificationService notificationService,
            ILogger loggerService,
            IExportManager exportManager,
            IRemarkConfigService remarkConfigService,
            ICashReceiptBillService cashReceiptBillService
            ) : base(workContext, loggerService, storeContext, notificationService)
        {
            _userService = userService;
            _districtService = districtService;
            _channelService = channelService;
            _rankService = rankService;
            _wareHouseService = wareHouseService;
            _productCategoryService = productCategoryService;
            _brandService = brandService;
            _statisticalTypeService = statisticalTypeService;
            _giveQuotaService = giveQuotaService;
            _saleReportService = saleReportService;
            _exportService = exportService;
            _exportManager = exportManager;
            _remarkConfigService = remarkConfigService;
            _cashReceiptBillService = cashReceiptBillService;
        }

        #region 销售明细表
        /// <summary>
        /// 销售明细表
        /// </summary>
        /// <param name="productId">商品Id</param>
        /// <param name="productName">商品名称</param>
        /// <param name="categoryId">商品类别Id</param>
        /// <param name="brandId">品牌Id</param>
        /// <param name="terminalId">客户Id</param>
        /// <param name="terminalName">客户名称</param>
        /// <param name="billNumber">单据编号</param>
        /// <param name="saleTypeId">销售类型Id</param>
        /// <param name="bussinessUserId">业务员Id</param>
        /// <param name="wareHouseId">仓库Id</param>
        /// <param name="payTypeId">支付方式Id</param>
        /// <param name="deliveryUserId">送货员Id</param>
        /// <param name="rankId">客户等级Id</param>
        /// <param name="remark">备注</param>
        /// <param name="channelId">客户渠道Id</param>
        /// <param name="startTime">开始日期</param>
        /// <param name="endTime">结束日期</param>
        /// <param name="costContractProduct">费用合同兑现商品</param>
        /// <param name="districtId">客户片区Id</param>
        /// <param name="pagenumber"></param>
        /// <returns></returns>
        [HttpGet]
        [AuthCode((int)AccessGranularityEnum.SaleDetailsView)]
        public IActionResult SaleReportItem(int? productId, string productName, int? categoryId, int? brandId, int? terminalId, string terminalName, string billNumber, int? saleTypeId, int? businessUserId, int? wareHouseId, int? payTypeId, int? deliveryUserId, int? rankId, string remark, int? channelId, DateTime? startTime, DateTime? endTime, bool? costContractProduct, int? districtId, int pagenumber = 0, bool? auditedStatus = null)
        {

            var model = new SaleReportItemListModel();

            #region 绑定数据源

            //商品
            model.ProductId = productId;
            model.ProductName = productName;

            //商品类别
            model.Categories = BindCategorySelection(_productCategoryService.BindCategoryList, curStore);
            model.CategoryId = (categoryId ?? null);

            //品牌
            model.Brands = BindBrandSelection(_brandService.BindBrandList, curStore);
            model.BrandId = (brandId ?? null);

            //客户
            model.TerminalId = terminalId;
            model.TerminalName = terminalName;

            //单据编号
            model.BillNumber = billNumber;

            //销售类型
            model.SaleTypes = new SelectList(from a in Enum.GetValues(typeof(SaleReportItemSaleTypeEnum)).Cast<SaleReportItemSaleTypeEnum>()
                                             select new SelectListItem
                                             {
                                                 Text = CommonHelper.GetEnumDescription(a),
                                                 Value = ((int)a).ToString()
                                             }, "Value", "Text");
            model.SaleTypeId = (saleTypeId ?? null);

            //业务员
            model.BusinessUsers = BindUserSelection(_userService.BindUserList, curStore, DCMSDefaults.Salesmans,curUser.Id,true, _userService.IsAdmin(curStore.Id, curUser.Id));
            model.BusinessUserId = (businessUserId ?? null);

            //仓库
            model.WareHouses = BindWareHouseSelection(_wareHouseService.BindWareHouseList, curStore,null,0);
            model.WareHouseId = (wareHouseId ?? null);

            //支付方式
            model.PayTypes = new SelectList(from a in Enum.GetValues(typeof(SaleReportItemPayTypeEnum)).Cast<SaleReportItemPayTypeEnum>()
                                            select new SelectListItem
                                            {
                                                Text = CommonHelper.GetEnumDescription(a),
                                                Value = ((int)a).ToString()
                                            }, "Value", "Text");
            model.PayTypeId = (payTypeId ?? null);

            //送货员
            model.DeliveryUsers = BindUserSelection(_userService.BindUserList, curStore, DCMSDefaults.Delivers, curUser.Id, true, _userService.IsAdmin(curStore.Id, curUser.Id));
            model.DeliveryUserId = (deliveryUserId ?? null);

            //客户等级
            model.Ranks = BindRankSelection(_rankService.BindRankList, curStore);
            model.RankId = (rankId ?? null);

            //备注
            model.Remark = remark;

            //客户渠道
            model.Channels = BindChanneSelection(_channelService.BindChannelList, curStore);
            model.ChannelId = (channelId ?? null);

            model.StartTime = startTime ?? DateTime.Parse(DateTime.Now.ToString("yyyy-MM-01 00:00:00"));
            model.EndTime = endTime ?? DateTime.Now.AddDays(1);
            if (model.StartTime.AddMonths(3)< model.EndTime) 
            {
                model.EndTime = model.StartTime.AddMonths(3);
            }

            //费用合同兑现商品
            model.CostContractProduct = costContractProduct ?? null;

            //客户片区
            model.Districts = BindDistrictSelection(_districtService.BindDistrictList, curStore);
            model.DistrictId = districtId ?? null;

            model.AuditedStatus = auditedStatus;
            #endregion

            if (pagenumber > 0)
            {
                pagenumber -= 1;
            }

            var items = _saleReportService.GetSaleReportItem(curStore?.Id ?? 0,
                productId,
                productName,
                categoryId,
                brandId,
                terminalId,
                terminalName,
                billNumber,
                saleTypeId,
                businessUserId,
                wareHouseId,
                payTypeId,
                deliveryUserId,
                rankId,
                remark,
                channelId,
                model.StartTime,
                model.EndTime,
                costContractProduct ?? false,
                districtId,
                pageIndex: pagenumber,
                pageSize: 30,
                auditedStatus:auditedStatus);


            #region 汇总

            if (items != null && items.Any())
            {

                model.Items = items.ToList();
                //数量
                model.PageSumQuantityConversion = items.Sum(a => a.SaleReturnBigQuantity ?? 0) + "大" + items.Sum(a => a.SaleReturnStrokeQuantity ?? 0) + "中" + items.Sum(a => a.SaleReturnSmallQuantity) + "小";
                //金额
                model.PageSumAmount = items.Sum(a => a.Amount);
                //成本金额
                model.PageSumCostAmount = items.Sum(a => a.CostAmount);
                //利润
                model.PageSumProfit = items.Sum(a => a.Profit);
                //成本利润率
                if (model.PageSumCostAmount == null || model.PageSumCostAmount == 0)
                {
                    model.PageSumCostProfitRate = 100;
                }
                else
                {
                    model.PageSumCostProfitRate = (model.PageSumProfit / model.PageSumCostAmount) * 100;
                }
                //变动差额
                model.PageSumChangeDifference = items.Sum(a => a.ChangeDifference);
        

                //数量
                model.TotalSumQuantityConversion = items.Sum(a => a.SaleReturnBigQuantity ?? 0) + "大" + items.Sum(a => a.SaleReturnStrokeQuantity ?? 0) + "中" + items.Sum(a => a.SaleReturnSmallQuantity) + "小";
                //金额
                model.TotalSumAmount = items.Sum(a => a.Amount ?? 0);
                //成本金额
                model.TotalSumCostAmount = items.Sum(a => a.CostAmount ?? 0);
                //利润
                model.TotalSumProfit = items.Sum(a => a.Profit ?? 0);
                //成本利润率
                if (model.TotalSumCostAmount == null || model.TotalSumCostAmount == 0)
                {
                    model.TotalSumCostProfitRate = 100;
                }
                else
                {
                    model.TotalSumCostProfitRate = (model.TotalSumProfit / model.TotalSumCostAmount) * 100;
                }

                //变动差额
                model.TotalSumChangeDifference = items.Sum(a => a.ChangeDifference ?? 0);
            }

            #endregion

            model.PagingFilteringContext.LoadPagedList(items);

            return View(model);
        }

        //销售明细表导出
        [HttpGet]
        //[AuthCode((int)AccessGranularityEnum.SaleDetailsExport)]
        public FileResult ExportSaleReportItem(int? productId, string productName, int? categoryId, int? brandId, int? terminalId, string terminalName, string billNumber, int? saleTypeId, int? businessUserId, int? wareHouseId, int? payTypeId, int? deliveryUserId, int? rankId, string remark, int? channelId, DateTime? startTime, DateTime? endTime, bool? costContractProduct, int? districtId, int pagenumber = 0,bool? auditedStatus = null)
        {
            try
            {

                #region 查询导出数据

                if (pagenumber > 0)
                {
                    pagenumber -= 1;
                }

                startTime = startTime ?? DateTime.Parse(DateTime.Now.ToString("yyyy-MM-01 00:00:00"));
                endTime = endTime ?? DateTime.Now.AddDays(1);
                var items = _saleReportService.GetSaleReportData(curStore?.Id ?? 0,
                    productId ?? 0,
                    productName,
                    categoryId ?? 0,
                    brandId ?? 0,
                    terminalId ?? 0,
                    terminalName,
                    billNumber,
                    saleTypeId ?? 0,
                    businessUserId ?? 0,
                    wareHouseId ?? 0,
                    payTypeId ?? 0,
                    deliveryUserId ?? 0,
                    rankId ?? 0,
                    remark,
                    channelId ?? 0,
                    startTime,
                    endTime,
                    costContractProduct ?? false,
                    districtId ?? 0,
                    auditedStatus:auditedStatus);
                //var items = _saleReportService.GetSaleReportItem(curStore?.Id ?? 0,
                //    productId ?? 0,
                //    productName,
                //    categoryId ?? 0,
                //    brandId ?? 0,
                //    terminalId ?? 0,
                //    terminalName,
                //    billNumber,
                //    saleTypeId ?? 0,
                //    businessUserId ?? 0,
                //    wareHouseId ?? 0,
                //    payTypeId ?? 0,
                //    deliveryUserId ?? 0,
                //    rankId ?? 0,
                //    remark,
                //    channelId ?? 0,
                //    startTime,
                //    endTime,
                //    costContractProduct ?? false,
                //    districtId ?? 0,
                //    pageIndex: pagenumber,
                //    pageSize: 30);

                #endregion

                #region 导出
                var ms = _exportManager.ExportSaleReportItemToXlsx(items);
                if (ms != null)
                {
                    return File(ms, "application/vnd.ms-excel", "销售明细表.xlsx");
                }
                else
                {
                    return File(new MemoryStream(), "application/vnd.ms-excel", "销售明细表.xlsx");
                }
                #endregion
            }
            catch (Exception)
            {
                return File(new MemoryStream(), "application/vnd.ms-excel", "销售明细表-无数据.xlsx");
            }

        }
        #endregion

        #region 销售汇总（按商品）
        /// <summary>
        /// 销售汇总（按商品）
        /// </summary>
        /// <param name="productId">商品Id</param>
        /// <param name="productName">商品名称</param>
        /// <param name="categoryId">商品类别Id</param>
        /// <param name="bussinessUserId">业务员Id</param>
        /// <param name="wareHouseId">仓库Id</param>
        /// <param name="terminalId">客户Id</param>
        /// <param name="terminalName">客户名称</param>
        /// <param name="deliveryUserId">送货员Id</param>
        /// <param name="rankId">客户等级Id</param>
        /// <param name="remark">备注</param>
        /// <param name="payTypeId">支付方式Id</param>
        /// <param name="startTime">开始日期</param>
        /// <param name="endTime">结束日期</param>
        /// <param name="costContractProduct">费用合同兑现商品</param>
        /// <param name="channelId">客户渠道Id</param>
        /// <param name="districtId">客户片区Id</param>
        /// <param name="brandId">品牌Id</param>
        /// <param name="billNumber">单据编号</param>
        /// <param name="saleTypeId">销售类型Id</param>
        /// <param name="pagenumber"></param>
        /// <returns></returns>
        [HttpGet]
        [AuthCode((int)AccessGranularityEnum.SaleSummaryByProductView)]
        public IActionResult SaleReportSummaryProduct(int? productId, string productName, int? categoryId, int? businessUserId, int? wareHouseId, int? terminalId, string terminalName, int? deliveryUserId, int? rankId, string remark, int? payTypeId, DateTime? startTime, DateTime? endTime, bool? costContractProduct, int? channelId, int? districtId, int pagenumber = 0, bool? auditedStatus = null)
        {


            var model = new SaleReportSummaryProductListModel();

            #region 绑定数据源

            //商品
            model.ProductId = productId;
            model.ProductName = productName;

            //商品类别
            model.Categories = BindCategorySelection(_productCategoryService.BindCategoryList, curStore);
            model.CategoryId = (categoryId ?? null);

            //业务员
            model.BusinessUsers = BindUserSelection(_userService.BindUserList, curStore, DCMSDefaults.Salesmans,curUser.Id,true, _userService.IsAdmin(curStore.Id, curUser.Id));
            model.BusinessUserId = (businessUserId ?? null);

            //仓库
            model.WareHouses = BindWareHouseSelection(_wareHouseService.BindWareHouseList, curStore,null,0);
            model.WareHouseId = (wareHouseId ?? null);

            //客户
            model.TerminalId = terminalId;
            model.TerminalName = terminalName;

            //送货员
            model.DeliveryUsers = BindUserSelection(_userService.BindUserList, curStore, DCMSDefaults.Delivers, curUser.Id, true, _userService.IsAdmin(curStore.Id, curUser.Id));
            model.DeliveryUserId = (deliveryUserId ?? null);

            //客户等级
            model.Ranks = BindRankSelection(_rankService.BindRankList, curStore);
            model.RankId = (rankId ?? null);

            //支付方式
            model.PayTypes = new SelectList(from a in Enum.GetValues(typeof(SaleReportSummaryProductPayTypeEnum)).Cast<SaleReportSummaryProductPayTypeEnum>()
                                            select new SelectListItem
                                            {
                                                Text = CommonHelper.GetEnumDescription(a),
                                                Value = ((int)a).ToString()
                                            }, "Value", "Text");
            model.PayTypeId = (payTypeId ?? null);

            //费用合同兑现商品
            model.CostContractProduct = costContractProduct ?? null;

            //客户渠道
            model.Channels = BindChanneSelection(_channelService.BindChannelList, curStore);
            model.ChannelId = (channelId ?? null);

            model.StartTime = startTime ?? DateTime.Parse(DateTime.Now.ToString("yyyy-MM-01 00:00:00"));
            model.EndTime = endTime ?? DateTime.Now.AddDays(1);

            //客户片区
            model.Districts = BindDistrictSelection(_districtService.BindDistrictList, curStore);
            model.DistrictId = (districtId ?? null);

            //审核状态
            model.AuditedStatus = auditedStatus;
            #endregion

            if (pagenumber > 0)
            {
                pagenumber -= 1;
            }

            var sqlDatas = _saleReportService.GetSaleReportSummaryProduct(curStore?.Id ?? 0,
                productId, productName, categoryId, businessUserId, wareHouseId,
                terminalId, terminalName, deliveryUserId, rankId, remark,
                payTypeId,
                model.StartTime, //?? DateTime.Parse(DateTime.Now.ToString("yyyy-MM-dd")),
                model.EndTime, //?? DateTime.Now.AddDays(1),
                costContractProduct,
                channelId, districtId,
                auditedStatus: auditedStatus
                );

            var items = new PagedList<SaleReportSummaryProduct>(sqlDatas, pagenumber, 30);

            model.Items = items;

            #region 汇总

            if (sqlDatas != null && sqlDatas.ToList().Count > 0)
            {

                //销售数量
                model.TotalSumSaleQuantityConversion = sqlDatas.Sum(a => a.SaleBigQuantity ?? 0) + "大" + sqlDatas.Sum(a => a.SaleStrokeQuantity ?? 0) + "中" + sqlDatas.Sum(a => a.SaleSmallQuantity ?? 0) + "小";
                //销售金额
                model.TotalSumSaleAmount = items.Sum(a => a.SaleAmount ?? 0);
                //赠送数量
                model.TotalSumGiftQuantityConversion = sqlDatas.Sum(a => a.GiftBigQuantity ?? 0) + "大" + sqlDatas.Sum(a => a.GiftStrokeQuantity ?? 0) + "中" + sqlDatas.Sum(a => a.GiftSmallQuantity ?? 0) + "小";
                //退货数量
                model.TotalSumReturnQuantityConversion = sqlDatas.Sum(a => a.ReturnBigQuantity ?? 0) + "大" + sqlDatas.Sum(a => a.ReturnStrokeQuantity ?? 0) + "中" + sqlDatas.Sum(a => a.ReturnSmallQuantity ?? 0) + "小";
                //退货金额
                model.TotalSumReturnAmount = sqlDatas.Sum(a => a.ReturnAmount ?? 0);
                //净销数量
                model.TotalSumNetQuantityConversion = sqlDatas.Sum(a => a.NetBigQuantity ?? 0) + "大" + sqlDatas.Sum(a => a.NetStrokeQuantity ?? 0) + "中" + sqlDatas.Sum(a => a.NetSmallQuantity ?? 0) + "小";
                //净销金额
                model.TotalSumNetAmount = model.TotalSumSaleAmount - model.TotalSumReturnAmount;
                //成本金额
                model.TotalSumCostAmount = sqlDatas.Sum(a => a.CostAmount ?? 0);
                //利润
                model.TotalSumProfit = sqlDatas.Sum(a => a.Profit ?? 0);
                //成本利润率
                if (model.TotalSumCostAmount == null || model.TotalSumCostAmount == 0)
                {
                    model.TotalSumCostProfitRate = 100;
                }
                else
                {
                    model.TotalSumCostProfitRate = (model.TotalSumProfit / model.TotalSumCostAmount) * 100;
                }

            }

            #endregion

            model.PagingFilteringContext.LoadPagedList(items);

            return View(model);
        }

        //销售汇总（按商品）导出
        [AuthCode((int)AccessGranularityEnum.SaleSummaryByProductExport)]
        public FileResult ExportSaleReportSummaryProduct(int? productId, string productName, int? categoryId, int? businessUserId, int? wareHouseId, int? terminalId, string terminalName, int? deliveryUserId, int? rankId, string remark, int? payTypeId, DateTime? startTime, DateTime? endTime, bool? costContractProduct, int? channelId, int? districtId,bool? auditedStatus = null)
        {

            var sqlDatas = _saleReportService.GetSaleReportSummaryProduct(curStore?.Id ?? 0,
                productId, productName, categoryId, businessUserId, wareHouseId,
                terminalId, terminalName, deliveryUserId, rankId, remark,
                payTypeId, startTime ?? DateTime.Parse(DateTime.Now.ToString("yyyy-MM-01 00:00:00")), endTime ?? DateTime.Now.AddDays(1), costContractProduct,
                channelId, districtId,auditedStatus: auditedStatus
                );

            #region 导出
            var ms = _exportManager.ExportSaleReportSummaryProductToXlsx(sqlDatas);
            if (ms != null)
            {
                return File(ms, "application/vnd.ms-excel", "销售汇总表（按商品）.xlsx");
            }
            else
            {
                return File(new MemoryStream(), "application/vnd.ms-excel", "销售汇总表（按商品）.xlsx");
            }
            #endregion
        }
        #endregion

        #region 销售汇总（按客户）
        /// <summary>
        /// 销售汇总（按客户）
        /// </summary>
        /// <param name="terminalId">客户Id</param>
        /// <param name="terminalName">客户名称</param>
        /// <param name="startTime">开始日期</param>
        /// <param name="endTime">结束日期</param>
        /// <param name="brandId">品牌Id</param>
        /// <param name="productId">商品Id</param>
        /// <param name="productName">商品名称</param>
        /// <param name="categoryId">商品类别Id</param>
        /// <param name="districtId">客户片区Id</param>
        /// <param name="channelId">客户渠道Id</param>
        /// <param name="rankId">客户等级Id</param>
        /// <param name="remark">备注</param>
        /// <param name="bussinessUserId">业务员Id</param>
        /// <param name="wareHouseId">仓库Id</param>
        /// <param name="pagenumber"></param>
        /// <returns></returns>
        [HttpGet]
        [AuthCode((int)AccessGranularityEnum.SaleSummaryByCustomerView)]
        public IActionResult SaleReportSummaryCustomer(int? terminalId, string terminalName, DateTime? startTime, DateTime? endTime, int? brandId, int? productId, string productName, int? categoryId, int? districtId, int? channelId, int? rankId, string remark, int? businessUserId, int? wareHouseId, int pagenumber = 0, bool? auditedStatus = null)
        {


            var model = new SaleReportSummaryCustomerListModel();

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

            //客户
            model.TerminalId = terminalId;
            model.TerminalName = terminalName;

            //品牌
            model.Brands = BindBrandSelection(_brandService.BindBrandList, curStore);
            model.BrandId = (brandId ?? null);

            //商品
            model.ProductId = productId;
            model.ProductName = productName;

            //商品类别
            model.Categories = BindCategorySelection(_productCategoryService.BindCategoryList, curStore);
            model.CategoryId = (categoryId ?? null);

            //客户片区
            model.Districts = BindDistrictSelection(_districtService.BindDistrictList, curStore);
            model.DistrictId = (districtId ?? null);

            //客户渠道
            model.Channels = BindChanneSelection(_channelService.BindChannelList, curStore);
            model.ChannelId = (channelId ?? null);

            //客户等级
            model.Ranks = BindRankSelection(_rankService.BindRankList, curStore);
            model.RankId = (rankId ?? null);

            //业务员
            model.BusinessUsers = BindUserSelection(_userService.BindUserList, curStore, DCMSDefaults.Salesmans,curUser.Id,true, _userService.IsAdmin(curStore.Id, curUser.Id));
            model.BusinessUserId = (businessUserId ?? null);

            //仓库
            model.WareHouses = BindWareHouseSelection(_wareHouseService.BindWareHouseList, curStore,null,0);
            model.WareHouseId = (wareHouseId ?? null);

            //model.StartTime = DateTime.Now;
            //model.EndTime = DateTime.Now.AddDays(1);
            model.StartTime = startTime ?? DateTime.Parse(DateTime.Now.ToString("yyyy-MM-01 00:00:00"));
            model.EndTime = endTime ?? DateTime.Now.AddDays(1);

            //审核状态
            model.AuditedStatus = auditedStatus;
            #endregion

            if (pagenumber > 0)
            {
                pagenumber -= 1;
            }

            var sqlDatas = _saleReportService.GetSaleReportSummaryCustomer(curStore?.Id ?? 0,
                terminalId,
                terminalName,
                model.StartTime, //?? DateTime.Parse(DateTime.Now.ToString("yyyy-MM-dd")),
                model.EndTime, //?? DateTime.Now.AddDays(1),
                brandId,
                productId, productName, categoryId, districtId, channelId,
                rankId, remark, businessUserId, wareHouseId, dic,auditedStatus: auditedStatus
                ).Select(sd =>
                {
                    //利润 = 销售金额 - 退货金额 - 优惠 -成本
                    sd.Profit = (sd.SaleAmount ?? 0) - (sd.ReturnAmount ?? 0) - (sd.DiscountAmount ?? 0) - (sd.CostAmount ?? 0);
                    return sd ?? new SaleReportSummaryCustomer();

                }).AsQueryable();

            var items = new PagedList<SaleReportSummaryCustomer>(sqlDatas, pagenumber, 30);

            model.Items = items;

            #region 汇总

            //动态列汇总
            if (model.DynamicColumns != null && model.DynamicColumns.Count > 0 && dic.Keys != null && dic.Keys.Count > 0)
            {
                foreach (var k in dic.Keys)
                {
                    model.TotalDynamicDatas.Add(new SaleReportSumStatisticalType()
                    {
                        StatisticalTypeId = k
                    });
                }
            }
            if (model.Items != null && model.Items.Count > 0)
            {
                model.Items.ToList().ForEach(a =>
                {
                    if (a.SaleReportStatisticalTypes != null && a.SaleReportStatisticalTypes.Count > 0)
                    {
                        a.SaleReportStatisticalTypes.ToList().ForEach(b =>
                        {
                            SaleReportSumStatisticalType saleReportSumStatisticalType = model.TotalDynamicDatas.Where(c => c.StatisticalTypeId == b.StatisticalTypeId).FirstOrDefault();
                            if (saleReportSumStatisticalType != null)
                            {
                                saleReportSumStatisticalType.NetSmallQuantity = (saleReportSumStatisticalType.NetSmallQuantity ?? 0) + (b.NetSmallQuantity ?? 0);
                                saleReportSumStatisticalType.NetAmount = (saleReportSumStatisticalType.NetAmount ?? 0) + (b.NetAmount ?? 0);
                                saleReportSumStatisticalType.CostAmount = (saleReportSumStatisticalType.CostAmount ?? 0) + (b.CostAmount ?? 0);
                                saleReportSumStatisticalType.Profit = (saleReportSumStatisticalType.Profit ?? 0) + (b.Profit ?? 0);
                                if (saleReportSumStatisticalType.CostAmount == null || saleReportSumStatisticalType.CostAmount == 0)
                                {
                                    saleReportSumStatisticalType.CostProfitRate = 100;
                                }
                                else
                                {
                                    saleReportSumStatisticalType.CostProfitRate = ((saleReportSumStatisticalType.Profit ?? 0) / saleReportSumStatisticalType.CostAmount) * 100;
                                }

                            }
                        });
                    }
                });
            }

            if (model.Items != null && model.Items.Count > 0)
            {
                //销售数量
                model.TotalSumSaleSmallQuantity = model.Items.Sum(m => m.SaleSmallQuantity ?? 0);
                //退货数量
                model.TotalSumReturnSmallQuantity = model.Items.Sum(m => m.ReturnSmallQuantity ?? 0);
                //赠送数量
                model.TotalSumGiftQuantity = model.Items.Sum(m => m.GiftQuantity ?? 0);
                //净销数量 = 销售数量 - 退货数量
                model.TotalSumNetSmallQuantity = model.TotalSumSaleSmallQuantity - model.TotalSumReturnSmallQuantity + model.TotalSumGiftQuantity;
                //销售金额
                model.TotalSumSaleAmount = model.Items.Sum(m => m.SaleAmount ?? 0);
                //退货金额
                model.TotalSumReturnAmount = model.Items.Sum(m => m.ReturnAmount ?? 0);
                //净销金额 = 销售金额 - 退货金额
                model.TotalSumNetAmount = model.TotalSumSaleAmount - model.TotalSumReturnAmount;
                //优惠金额
                model.TotalSumDiscountAmount = model.Items.Sum(m => m.DiscountAmount ?? 0);
                //成本金额
                model.TotalSumCostAmount = model.Items.Sum(m => m.CostAmount ?? 0);
                //利润
                model.TotalSumProfit = model.Items.Sum(m => m.Profit ?? 0);
                //成本利润率
                if (model.TotalSumCostAmount == null || model.TotalSumCostAmount == 0)
                {
                    model.TotalSumCostProfitRate = 100;
                }
                else
                {
                    model.TotalSumCostProfitRate = (model.TotalSumProfit / model.TotalSumCostAmount) * 100;
                }

            }

            #endregion

            model.PagingFilteringContext.LoadPagedList(items);

            return View(model);
        }

        //销售汇总（按客户）导出
        [AuthCode((int)AccessGranularityEnum.SaleSummaryByCustomerExport)]
        public FileResult ExportSaleReportSummaryCustomer(int? terminalId, string terminalName, DateTime? startTime, DateTime? endTime, int? brandId, int? productId, string productName, int? categoryId, int? districtId, int? channelId, int? rankId, string remark, int? businessUserId, int? wareHouseId,bool? auditedStatus = null)
        {


            //商品统计类别动态列
            var statisticalTypes = _statisticalTypeService.GetAllStatisticalTypess(curStore?.Id ?? 0);
            Dictionary<int, string> dic = new Dictionary<int, string>();
            if (statisticalTypes != null && statisticalTypes.Count > 0)
            {
                //model.DynamicColumns = statisticalTypes.OrderBy(a => a.Id).Select(a => a.Name).ToList();
                foreach (var c in statisticalTypes)
                {
                    dic.Add(c.Id, c.Name);
                }
            }
            dic.Add((int)StatisticalTypeEnum.OtherTypeId, "其他");

            #region 查询导出数据

            var sqlDatas = _saleReportService.GetSaleReportSummaryCustomer(curStore?.Id ?? 0,
                terminalId, terminalName,
                (startTime == null) ? DateTime.Parse(DateTime.Now.ToString("yyyy-MM-01 00:00:00")) : startTime,
                (endTime == null) ? DateTime.Now.AddDays(1) : endTime,
                brandId,
                productId, productName, categoryId, districtId, channelId,
                rankId, remark, businessUserId, wareHouseId, dic,auditedStatus: auditedStatus
                );

            #endregion

            #region 导出
            var ms = _exportManager.ExportSaleReportSummaryCustomerToXlsx(sqlDatas, curStore.Id);
            if (ms != null)
            {
                return File(ms, "application/vnd.ms-excel", "销售汇总（按客户）.xlsx");
            }
            else
            {
                return File(new MemoryStream(), "application/vnd.ms-excel", "销售汇总（按客户）.xlsx");
            }
            #endregion
        }


        #endregion

        #region 销售汇总（按业务员）
        /// <summary>
        /// 销售汇总（按业务员）
        /// </summary>
        /// <param name="bussinessUserId">业务员Id</param>
        /// <param name="startTime">开始日期</param>
        /// <param name="endTime">结束日期</param>
        /// <param name="brandId">品牌Id</param>
        /// <param name="pagenumber"></param>
        /// <returns></returns>
        [HttpGet]
        [AuthCode((int)AccessGranularityEnum.SaleSummaryByBUserView)]
        public IActionResult SaleReportSummaryBusinessUser(int? businessUserId, DateTime? startTime, DateTime? endTime, int? brandId, int pagenumber = 0, bool? auditedStatus = null)
        {


            var model = new SaleReportSummaryBusinessUserListModel();

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

            //业务员
            model.BusinessUsers = BindUserSelection(_userService.BindUserList, curStore, DCMSDefaults.Salesmans,curUser.Id,true, _userService.IsAdmin(curStore.Id, curUser.Id));
            model.BusinessUserId = (businessUserId ?? null);

            model.StartTime = startTime ?? DateTime.Parse(DateTime.Now.ToString("yyyy-MM-01 00:00:00"));
            model.EndTime = endTime ?? DateTime.Now.AddDays(1);

            //品牌
            model.Brands = BindBrandSelection(_brandService.BindBrandList, curStore);
            model.BrandId = (brandId ?? null);

            //审核状态
            model.AuditedStatus = auditedStatus;
            #endregion

            if (pagenumber > 0)
            {
                pagenumber -= 1;
            }

            var sqlDatas = _saleReportService.GetSaleReportSummaryBusinessUser(curStore?.Id ?? 0,
                businessUserId,
                model.StartTime, //?? DateTime.Parse(DateTime.Now.ToString("yyyy-MM-dd")),
                model.EndTime, //?? DateTime.Now.AddDays(1),
                brandId, dic,auditedStatus:auditedStatus
                ).Select(sd =>
                {

                    //利润 = 销售金额 - 退货金额 - 优惠 -成本
                    sd.Profit = (sd.SaleAmount ?? 0) - (sd.ReturnAmount ?? 0) - (sd.DiscountAmount ?? 0) - (sd.CostAmount ?? 0);

                    sd.NetAmount = (sd.SaleAmount ?? 0) - (sd.ReturnAmount ?? 0);
                    sd.NetSmallQuantity = sd.SaleSmallQuantity ?? 0;

                    return sd ?? new SaleReportSummaryBusinessUser();

                }).AsQueryable();

            var items = new PagedList<SaleReportSummaryBusinessUser>(sqlDatas, pagenumber, 30);

            model.Items = items;

            #region 汇总

            //动态列汇总
            if (model.DynamicColumns != null && model.DynamicColumns.Count > 0 && dic.Keys != null && dic.Keys.Count > 0)
            {
                foreach (var k in dic.Keys)
                {
                    model.TotalDynamicDatas.Add(new SaleReportSumStatisticalType()
                    {
                        StatisticalTypeId = k
                    });
                }
            }
            if (model.Items != null && model.Items.Count > 0)
            {
                model.Items.ToList().ForEach(a =>
                {
                    if (a.SaleReportStatisticalTypes != null && a.SaleReportStatisticalTypes.Count > 0)
                    {
                        a.SaleReportStatisticalTypes.ToList().ForEach(b =>
                        {
                            SaleReportSumStatisticalType saleReportSumStatisticalType = model.TotalDynamicDatas.Where(c => c.StatisticalTypeId == b.StatisticalTypeId).FirstOrDefault();
                            if (saleReportSumStatisticalType != null)
                            {
                                saleReportSumStatisticalType.NetSmallQuantity = (saleReportSumStatisticalType.NetSmallQuantity ?? 0) + (b.NetSmallQuantity ?? 0);
                                saleReportSumStatisticalType.NetAmount = (saleReportSumStatisticalType.NetAmount ?? 0) + (b.NetAmount ?? 0);
                                saleReportSumStatisticalType.CostAmount = (saleReportSumStatisticalType.CostAmount ?? 0) + (b.CostAmount ?? 0);
                                saleReportSumStatisticalType.Profit = (saleReportSumStatisticalType.Profit ?? 0) + (b.Profit ?? 0);
                                if (saleReportSumStatisticalType.CostAmount == null || saleReportSumStatisticalType.CostAmount == 0)
                                {
                                    saleReportSumStatisticalType.CostProfitRate = 100;
                                }
                                else
                                {
                                    saleReportSumStatisticalType.CostProfitRate = ((saleReportSumStatisticalType.Profit ?? 0) / saleReportSumStatisticalType.CostAmount) * 100;
                                }

                            }
                        });
                    }
                });
            }

            if (model.Items != null && model.Items.Count > 0)
            {
                //销售数量
                model.TotalSumSaleSmallQuantity = model.Items.Sum(m => m.SaleSmallQuantity ?? 0);
                //退货数量
                model.TotalSumReturnSmallQuantity = model.Items.Sum(m => m.ReturnSmallQuantity ?? 0);
                //赠送数量
                model.TotalSumGiftQuantity = model.Items.Sum(m => m.GiftQuantity ?? 0);
                //净销数量 = 销售数量 - 退货数量
                model.TotalSumNetSmallQuantity = model.TotalSumSaleSmallQuantity - model.TotalSumReturnSmallQuantity;
                //销售金额
                model.TotalSumSaleAmount = model.Items.Sum(m => m.SaleAmount ?? 0);
                //退货金额
                model.TotalSumReturnAmount = model.Items.Sum(m => m.ReturnAmount ?? 0);
                //净销金额 = 销售金额 - 退货金额
                model.TotalSumNetAmount = model.TotalSumSaleAmount - model.TotalSumReturnAmount;
                //优惠金额
                model.TotalSumDiscountAmount = model.Items.Sum(m => m.DiscountAmount ?? 0);
                //成本金额
                model.TotalSumCostAmount = model.Items.Sum(m => m.CostAmount ?? 0);
                //利润
                model.TotalSumProfit = model.Items.Sum(m => m.Profit ?? 0);
                //成本利润率
                if (model.TotalSumCostAmount == null || model.TotalSumCostAmount == 0)
                {
                    model.TotalSumCostProfitRate = 100;
                }
                else
                {
                    model.TotalSumCostProfitRate = (model.TotalSumProfit / model.TotalSumCostAmount) * 100;
                }

            }

            #endregion

            model.PagingFilteringContext.LoadPagedList(items);

            return View(model);
        }

        //销售汇总（按业务员）导出
        [AuthCode((int)AccessGranularityEnum.SaleSummaryByBUserExport)]
        public FileResult ExportSaleReportSummaryBusinessUser(int? businessUserId, DateTime? startTime, DateTime? endTime, int? brandId,bool? auditedStatus = null)
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

            #region 查询导出数据

            var sqlDatas = _saleReportService.GetSaleReportSummaryBusinessUser(curStore?.Id ?? 0, businessUserId,
                (startTime == null) ? DateTime.Parse(DateTime.Now.ToString("yyyy-MM-01 00:00:00")) : startTime,
                (endTime == null) ? DateTime.Now.AddDays(1) : endTime,
                brandId, dic,auditedStatus: auditedStatus);

            #endregion

            #region 导出
            var ms = _exportManager.ExportSaleReportSummaryBusinessUserToXlsx(sqlDatas, curStore.Id);
            if (ms != null)
            {
                return File(ms, "application/vnd.ms-excel", "销售汇总（按业务员）.xlsx");
            }
            else
            {
                return File(new MemoryStream(), "application/vnd.ms-excel", "销售汇总（按业务员）.xlsx");
            }
            #endregion
        }

        #endregion

        #region 销售汇总（按客户/商品）
        /// <summary>
        /// 销售汇总（按客户/商品）
        /// </summary>
        /// <param name="wareHouseId">仓库Id</param>
        /// <param name="productId">商品Id</param>
        /// <param name="productName">商品名称</param>
        /// <param name="categoryId">商品类别Id</param>
        /// <param name="brandId">品牌Id</param>
        /// <param name="channelId">客户渠道Id</param>
        /// <param name="rankId">客户等级Id</param>
        /// <param name="bussinessUserId">业务员Id</param>
        /// <param name="deliveryUserId">送货员Id</param>
        /// <param name="terminalId">客户Id</param>
        /// <param name="terminalName">客户名称</param>
        /// <param name="remark">备注</param>
        /// <param name="startTime">开始日期</param>
        /// <param name="endTime">结束日期</param>
        /// <param name="pagenumber"></param>
        /// <returns></returns>
        [HttpGet]
        [AuthCode((int)AccessGranularityEnum.SaleSummaryByCPView)]
        public IActionResult SaleReportSummaryCustomerProduct(int? wareHouseId, int? productId, string productName, int? categoryId, int? brandId, int? channelId, int? rankId, int? businessUserId, int? deliveryUserId, int? terminalId, string terminalName, string remark, DateTime? startTime, DateTime? endTime, int pagenumber = 0, bool? auditedStatus = null)
        {

            var model = new SaleReportSummaryCustomerProductListModel();

            #region 绑定数据源

            //仓库
            model.WareHouses = BindWareHouseSelection(_wareHouseService.BindWareHouseList, curStore,null,0);
            model.WareHouseId = (wareHouseId ?? null);

            //商品
            model.ProductId = productId;
            model.ProductName = productName;

            //商品类别
            model.Categories = BindCategorySelection(_productCategoryService.BindCategoryList, curStore);
            model.CategoryId = (categoryId ?? null);

            //品牌
            model.Brands = BindBrandSelection(_brandService.BindBrandList, curStore);
            model.BrandId = (brandId ?? null);

            //客户渠道
            model.Channels = BindChanneSelection(_channelService.BindChannelList, curStore);
            model.ChannelId = (channelId ?? null);

            //客户等级
            model.Ranks = BindRankSelection(_rankService.BindRankList, curStore);
            model.RankId = (rankId ?? null);

            //业务员
            model.BusinessUsers = BindUserSelection(_userService.BindUserList, curStore, DCMSDefaults.Salesmans,curUser.Id,true, _userService.IsAdmin(curStore.Id, curUser.Id));
            model.BusinessUserId = (businessUserId ?? null);

            //送货员
            model.DeliveryUsers = BindUserSelection(_userService.BindUserList, curStore, DCMSDefaults.Delivers, curUser.Id, true, _userService.IsAdmin(curStore.Id, curUser.Id));
            model.DeliveryUserId = (deliveryUserId ?? null);

            //客户
            model.TerminalId = terminalId;
            model.TerminalName = terminalName;

            //备注
            model.Remark = remark;

            model.StartTime = startTime ?? DateTime.Parse(DateTime.Now.ToString("yyyy-MM-01 00:00:00"));
            model.EndTime = endTime ?? DateTime.Now.AddDays(1);

            //审核状态
            model.AuditedStatus = auditedStatus;
            #endregion

            if (pagenumber > 0)
            {
                pagenumber -= 1;
            }

            var items = _saleReportService.GetSaleReportSummaryCustomerProduct(curStore?.Id ?? 0,
                wareHouseId, 
                productId, 
                productName, 
                categoryId, 
                brandId,
                channelId,
                rankId, 
                businessUserId,
                deliveryUserId,
                terminalId, 
                terminalName, 
                remark,
                model.StartTime,
                model.EndTime,
                pageIndex: pagenumber,
                pageSize: 100,
                auditedStatus:auditedStatus);


            #region 汇总

            if (items != null && items.ToList().Count > 0)
            {

                //销售数量
                model.TotalSumSaleQuantityConversion = items.Where(a => a.SumRowType == false).Sum(a => a.SaleBigQuantity ?? 0) + "大" +
                                                       items.Where(a => a.SumRowType == false).Sum(a => a.SaleStrokeQuantity ?? 0) + "中" +
                                                       items.Where(a => a.SumRowType == false).Sum(a => a.SaleSmallQuantity ?? 0) + "小";
                //赠送数量
                model.TotalSumGiftQuantityConversion = items.Where(a => a.SumRowType == false).Sum(a => a.GiftBigQuantity ?? 0) + "大" +
                                                       items.Where(a => a.SumRowType == false).Sum(a => a.GiftStrokeQuantity ?? 0) + "中" +
                                                       items.Where(a => a.SumRowType == false).Sum(a => a.GiftSmallQuantity ?? 0) + "小";

                //销售金额
                model.TotalSumSaleAmount = items.Where(a => a.SumRowType == false).Sum(a => a.SaleAmount ?? 0);
                //退货数量
                model.TotalSumReturnQuantityConversion = items.Where(a => a.SumRowType == false).Sum(a => a.ReturnBigQuantity ?? 0) + "大" +
                                                         items.Where(a => a.SumRowType == false).Sum(a => a.ReturnStrokeQuantity ?? 0) + "中" +
                                                         items.Where(a => a.SumRowType == false).Sum(a => a.ReturnSmallQuantity ?? 0) + "小";
                //退货金额
                model.TotalSumReturnAmount = items.Where(a => a.SumRowType == false).Sum(a => a.ReturnAmount ?? 0);

                //还货数量
                model.TotalSumRepaymentQuantityConversion = items.Where(a => a.SumRowType == false).Sum(a => a.RepaymentBigQuantity ?? 0) + "大" +
                                                         items.Where(a => a.SumRowType == false).Sum(a => a.RepaymentStrokeQuantity ?? 0) + "中" +
                                                         items.Where(a => a.SumRowType == false).Sum(a => a.RepaymentSmallQuantity ?? 0) + "小";
                //还货金额
                model.TotalSumRepaymentAmount = items.Where(a => a.SumRowType == false).Sum(a => a.RepaymentAmount ?? 0);
                //总数量
                model.TotalSumQuantityConversion = (items.Where(a => a.SumRowType == false).Sum(a => a.SaleBigQuantity ?? 0) -
                                                   items.Where(a => a.SumRowType == false).Sum(a => a.ReturnBigQuantity ?? 0) +
                                                   items.Where(a => a.SumRowType == false).Sum(a => a.GiftBigQuantity ?? 0) +
                                                   items.Where(a => a.SumRowType == false).Sum(a => a.RepaymentBigQuantity ?? 0)) + "大" +
                                                   (items.Where(a => a.SumRowType == false).Sum(a => a.SaleStrokeQuantity ?? 0) +
                                                   items.Where(a => a.SumRowType == false).Sum(a => a.ReturnStrokeQuantity ?? 0) +
                                                   items.Where(a => a.SumRowType == false).Sum(a => a.GiftStrokeQuantity ?? 0) +
                                                   items.Where(a => a.SumRowType == false).Sum(a => a.RepaymentStrokeQuantity ?? 0)) + "中" +
                                                   (items.Where(a => a.SumRowType == false).Sum(a => a.SaleSmallQuantity ?? 0) -
                                                   items.Where(a => a.SumRowType == false).Sum(a => a.ReturnSmallQuantity ?? 0) +
                                                   items.Where(a => a.SumRowType == false).Sum(a => a.GiftSmallQuantity ?? 0) +
                                                   items.Where(a => a.SumRowType == false).Sum(a => a.RepaymentSmallQuantity ?? 0)) + "小";

                //总金额
                model.TotalSumAmount = model.TotalSumSaleAmount + model.TotalSumReturnAmount + model.TotalSumRepaymentAmount;

                //成本金额
                model.TotalSumAmount = items.Where(a => a.SumRowType == false).Sum(a => a.CostAmount ?? 0);
                //利润
                model.TotalSumProfit = model.TotalSumAmount - model.TotalSumAmount;
                //成本利润率
                if (model.TotalSumAmount == null || model.TotalSumAmount == 0)
                {
                    model.TotalSumCostProfitRate = 100;
                }
                else
                {
                    model.TotalSumCostProfitRate = (model.TotalSumProfit / model.TotalSumAmount) * 100;
                }

            }

            #endregion

            model.PagingFilteringContext.LoadPagedList(items);
            model.Items = items.Skip(pagenumber*100).Take(100).ToList();

            return View(model);
        }

        //销售汇总（按客户/商品）导出
        [AuthCode((int)AccessGranularityEnum.SaleSummaryByCPExport)]
        public FileResult ExportSaleReportSummaryCustomerProduct(SaleReportSummaryCustomerProductListModel model, int pagenumber = 0,bool? auditedStatus = null)
        {
            if (pagenumber > 0)
            {
                pagenumber -= 1;
            }
            //var datas = model?.Items;
            var items = _saleReportService.GetSaleReportSummaryCustomerProductData(curStore?.Id ?? 0,
                model.WareHouseId,
                model.ProductId,
                model.ProductName,
                model.CategoryId,
                model.BrandId,
                model.ChannelId,
                model.RankId,
                model.BusinessUserId,
                model.DeliveryUserId,
                model.TerminalId,
                model.TerminalName,
                model.Remark,
                model.StartTime,
                model.EndTime,
                auditedStatus:auditedStatus);
            #region 导出

            if (items != null && items.Any())
            {
                var ms = _exportManager.ExportSaleReportSummaryCustomerProductToXlsx(items.ToList());
                if (ms != null)
                {
                    return File(ms, "application/vnd.ms-excel", "销售汇总表（客户/商品）.xlsx");
                }
                else
                {
                    return File(new MemoryStream(), "application/vnd.ms-excel", "销售汇总表（客户/商品）.xlsx");
                }
            }
            else
            {
                return File(new MemoryStream(), "application/vnd.ms-excel", "销售汇总表（客户/商品）.xlsx");
            }

            #endregion
        }

        #endregion

        #region 销售汇总（按仓库）
        /// <summary>
        /// 销售汇总（按仓库）
        /// </summary>
        /// <param name="wareHouseId">仓库Id</param>
        /// <param name="startTime">开始日期</param>
        /// <param name="endTime">结束日期</param>
        /// <param name="pagenumber"></param>
        /// <returns></returns>
        [HttpGet]
        [AuthCode((int)AccessGranularityEnum.SaleSummaryByStockView)]
        public IActionResult SaleReportSummaryWareHouse(int? wareHouseId, DateTime? startTime, DateTime? endTime, int pagenumber = 0, bool? auditedStatus = null)
        {


            var model = new SaleReportSummaryWareHouseListModel();

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

            //仓库
            model.WareHouses = BindWareHouseSelection(_wareHouseService.BindWareHouseList, curStore,null,0);
            model.WareHouseId = (wareHouseId ?? null);

            model.StartTime = startTime ?? DateTime.Parse(DateTime.Now.ToString("yyyy-MM-01 00:00:00"));
            model.EndTime = endTime ?? DateTime.Now.AddDays(1);

            //审核状态
            model.AuditedStatus = auditedStatus;
            #endregion

            if (pagenumber > 0)
            {
                pagenumber -= 1;
            }

            var sqlDatas = _saleReportService.GetSaleReportSummaryWareHouse(curStore?.Id ?? 0,
                wareHouseId,
                model.StartTime, //?? DateTime.Parse(DateTime.Now.ToString("yyyy-MM-dd")),
                model.EndTime, //?? DateTime.Now.AddDays(1),
                dic,auditedStatus:auditedStatus
                ).Select(sd =>
                {

                    //利润 = 销售金额 - 退货金额 - 优惠 -成本
                    sd.Profit = (sd.SaleAmount ?? 0) - (sd.ReturnAmount ?? 0) - (sd.DiscountAmount ?? 0) - (sd.CostAmount ?? 0);

                    sd.NetAmount = (sd.SaleAmount ?? 0) - (sd.ReturnAmount ?? 0);
                    sd.NetSmallQuantity = (sd.SaleSmallQuantity ?? 0) - (sd.ReturnSmallQuantity ?? 0) - (sd.GiftQuantity ?? 0);

                    return sd ?? new SaleReportSummaryWareHouse();

                }).AsQueryable();

            var items = new PagedList<SaleReportSummaryWareHouse>(sqlDatas, pagenumber, 30);

            model.Items = items;

            #region 汇总

            //动态列汇总
            if (model.DynamicColumns != null && model.DynamicColumns.Count > 0 && dic.Keys != null && dic.Keys.Count > 0)
            {
                foreach (var k in dic.Keys)
                {
                    model.TotalDynamicDatas.Add(new SaleReportSumStatisticalType()
                    {
                        StatisticalTypeId = k
                    });
                }
            }
            if (model.Items != null && model.Items.Count > 0)
            {
                model.Items.ToList().ForEach(a =>
                {
                    if (a.SaleReportStatisticalTypes != null && a.SaleReportStatisticalTypes.Count > 0)
                    {
                        a.SaleReportStatisticalTypes.ToList().ForEach(b =>
                        {
                            SaleReportSumStatisticalType saleReportSumStatisticalType = model.TotalDynamicDatas.Where(c => c.StatisticalTypeId == b.StatisticalTypeId).FirstOrDefault();
                            if (saleReportSumStatisticalType != null)
                            {
                                saleReportSumStatisticalType.NetSmallQuantity = (saleReportSumStatisticalType.NetSmallQuantity ?? 0) + (b.NetSmallQuantity ?? 0) - (b.GiftQuantity ?? 0);
                                saleReportSumStatisticalType.NetAmount = (saleReportSumStatisticalType.NetAmount ?? 0) + (b.NetAmount ?? 0);
                                saleReportSumStatisticalType.CostAmount = (saleReportSumStatisticalType.CostAmount ?? 0) + (b.CostAmount ?? 0);
                                saleReportSumStatisticalType.Profit = (saleReportSumStatisticalType.Profit ?? 0) + (b.Profit ?? 0);
                                if (saleReportSumStatisticalType.CostAmount == null || saleReportSumStatisticalType.CostAmount == 0)
                                {
                                    saleReportSumStatisticalType.CostProfitRate = 100;
                                }
                                else
                                {
                                    saleReportSumStatisticalType.CostProfitRate = ((saleReportSumStatisticalType.Profit ?? 0) / saleReportSumStatisticalType.CostAmount) * 100;
                                }

                            }
                        });
                    }
                });
            }

            if (model.Items != null && model.Items.Count > 0)
            {
                //销售数量
                model.TotalSumSaleSmallQuantity = model.Items.Sum(m => m.SaleSmallQuantity ?? 0);
                //退货数量
                model.TotalSumReturnSmallQuantity = model.Items.Sum(m => m.ReturnSmallQuantity ?? 0);
                //赠送数量
                model.TotalSumGiftSmallQuantity = model.Items.Sum(m => m.GiftQuantity ?? 0);
                //净销数量 = 销售数量 - 退货数量
                model.TotalSumNetSmallQuantity = model.TotalSumSaleSmallQuantity - model.TotalSumReturnSmallQuantity - model.TotalSumGiftSmallQuantity;
                //销售金额
                model.TotalSumSaleAmount = model.Items.Sum(m => m.SaleAmount ?? 0);
                //退货金额
                model.TotalSumReturnAmount = model.Items.Sum(m => m.ReturnAmount ?? 0);
                //净销金额 = 销售金额 - 退货金额
                model.TotalSumNetAmount = model.TotalSumSaleAmount - model.TotalSumReturnAmount;
                //优惠金额
                model.TotalSumDiscountAmount = model.Items.Sum(m => m.DiscountAmount ?? 0);
                //成本金额
                model.TotalSumCostAmount = model.Items.Sum(m => m.CostAmount ?? 0);
                //利润
                model.TotalSumProfit = model.Items.Sum(m => m.Profit ?? 0);
                //成本利润率
                if (model.TotalSumCostAmount == null || model.TotalSumCostAmount == 0)
                {
                    model.TotalSumCostProfitRate = 100;
                }
                else
                {
                    model.TotalSumCostProfitRate = (model.TotalSumProfit / model.TotalSumCostAmount) * 100;
                }

            }

            #endregion

            model.PagingFilteringContext.LoadPagedList(items);

            return View(model);
        }

        //销售汇总（按仓库）导出
        [AuthCode((int)AccessGranularityEnum.SaleSummaryByStockExport)]
        public FileResult ExportSaleReportSummaryWareHouse(int? terminalId, DateTime? startTime, DateTime? endTime, int? brandId,
            int? productId, int? categoryId, int? districtId, int? channelId,
            int? rankId, string remark, int? businessUserId, int? wareHouseId, bool? auditedStatus = null)
        {


            //商品统计类别动态列
            var statisticalTypes = _statisticalTypeService.GetAllStatisticalTypess(curStore?.Id ?? 0);
            Dictionary<int, string> dic = new Dictionary<int, string>();
            if (statisticalTypes != null && statisticalTypes.Count > 0)
            {
                //model.DynamicColumns = statisticalTypes.OrderBy(a => a.Id).Select(a => a.Name).ToList();
                foreach (var c in statisticalTypes)
                {
                    dic.Add(c.Id, c.Name);
                }
            }
            dic.Add((int)StatisticalTypeEnum.OtherTypeId, "其他");

            #region 查询导出数据

            var sqlDatas = _saleReportService.GetSaleReportSummaryWareHouse(curStore?.Id ?? 0,
                wareHouseId,
                (startTime == null) ? DateTime.Parse(DateTime.Now.ToString("yyyy-MM-01 00:00:00")) : startTime,
                (endTime == null) ? DateTime.Now.AddDays(1) : endTime,
                dic,auditedStatus: auditedStatus
                );

            #endregion

            #region 导出
            var ms = _exportManager.ExportSaleReportSummaryWareHouseToXlsx(sqlDatas, curStore.Id);
            if (ms != null)
            {
                return File(ms, "application/vnd.ms-excel", "销售汇总（按仓库）.xlsx");
            }
            else
            {
                return File(new MemoryStream(), "application/vnd.ms-excel", "销售汇总（按仓库）.xlsx");
            }
            #endregion
        }

        #endregion


        #region 销售汇总（按品牌）
        /// <summary>
        /// 销售汇总（按品牌）
        /// </summary>
        /// <param name="brandId">品牌Id</param>
        /// <param name="districtId">客户片区Id</param>
        /// <param name="channelId">客户渠道Id</param>
        /// <param name="bussinessUserId">业务员Id</param>
        /// <param name="deliveryUserId">送货员Id</param>
        /// <param name="startTime">开始日期</param>
        /// <param name="endTime">结束日期</param>
        /// <param name="pagenumber"></param>
        /// <returns></returns>
        [HttpGet]
        [AuthCode((int)AccessGranularityEnum.SaleSummaryByBrandView)]
        public IActionResult SaleReportSummaryBrand(int? brandId, int? districtId, int? channelId, int? businessUserId, int? deliveryUserId, DateTime? startTime, DateTime? endTime,
            int pagenumber = 0,bool? auditedStatus = null)
        {

            var model = new SaleReportSummaryBrandListModel();

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

            //品牌
            model.Brands = BindBrandSelection(_brandService.BindBrandList, curStore);
            model.BrandId = (brandId ?? null);

            //客户片区
            model.Districts = BindDistrictSelection(_districtService.BindDistrictList, curStore);
            model.DistrictId = (districtId ?? null);

            //客户渠道
            model.Channels = BindChanneSelection(_channelService.BindChannelList, curStore);
            model.ChannelId = (channelId ?? null);

            //业务员
            model.BusinessUsers = BindUserSelection(_userService.BindUserList, curStore, DCMSDefaults.Salesmans,curUser.Id,true, _userService.IsAdmin(curStore.Id, curUser.Id));
            model.BusinessUserId = (businessUserId ?? null);

            //送货员
            model.DeliveryUsers = BindUserSelection(_userService.BindUserList, curStore, DCMSDefaults.Delivers, curUser.Id, true, _userService.IsAdmin(curStore.Id, curUser.Id));
            model.DeliveryUserId = (deliveryUserId ?? null);

            model.StartTime = startTime ?? DateTime.Parse(DateTime.Now.ToString("yyyy-MM-01 00:00:00"));
            model.EndTime = endTime ?? DateTime.Now.AddDays(1);

            //审核状态
            model.AuditedStatus = auditedStatus;
            #endregion

            if (pagenumber > 0)
            {
                pagenumber -= 1;
            }

            var sqlDatas = _saleReportService.GetSaleReportSummaryBrand(curStore?.Id ?? 0,
                new int[] { brandId ?? 0 }, districtId, channelId,
                businessUserId, deliveryUserId,
                startTime ?? DateTime.Parse(DateTime.Now.ToString("yyyy-MM-01 00:00:00")),
                endTime ?? DateTime.Now.AddDays(1), dic,auditedStatus:auditedStatus
                ).Select(sd =>
                {

                    //利润 = 销售金额 - 退货金额 - 优惠 -成本
                    sd.Profit = (sd.SaleAmount ?? 0) - (sd.ReturnAmount ?? 0) - (sd.DiscountAmount ?? 0) - (sd.CostAmount ?? 0);

                    sd.NetAmount = (sd.SaleAmount ?? 0) - (sd.ReturnAmount ?? 0);
                    sd.NetSmallQuantity = (sd.SaleSmallQuantity ?? 0) - (sd.ReturnSmallQuantity ?? 0);

                    return sd ?? new SaleReportSummaryBrand();

                }).AsQueryable();

            var items = new PagedList<SaleReportSummaryBrand>(sqlDatas, pagenumber, 30);

            model.Items = items;

            #region 汇总

            //动态列汇总
            if (model.DynamicColumns != null && model.DynamicColumns.Count > 0 && dic.Keys != null && dic.Keys.Count > 0)
            {
                foreach (var k in dic.Keys)
                {
                    model.TotalDynamicDatas.Add(new SaleReportSumStatisticalType()
                    {
                        StatisticalTypeId = k
                    });
                }
            }
            if (model.Items != null && model.Items.Count > 0)
            {
                model.Items.ToList().ForEach(a =>
                {
                    if (a.SaleReportStatisticalTypes != null && a.SaleReportStatisticalTypes.Count > 0)
                    {
                        a.SaleReportStatisticalTypes.ToList().ForEach(b =>
                        {
                            SaleReportSumStatisticalType saleReportSumStatisticalType = model.TotalDynamicDatas.Where(c => c.StatisticalTypeId == b.StatisticalTypeId).FirstOrDefault();
                            if (saleReportSumStatisticalType != null)
                            {
                                saleReportSumStatisticalType.NetSmallQuantity = (saleReportSumStatisticalType.NetSmallQuantity ?? 0) + (b.NetSmallQuantity ?? 0);
                                saleReportSumStatisticalType.NetAmount = (saleReportSumStatisticalType.NetAmount ?? 0) + (b.NetAmount ?? 0);
                                saleReportSumStatisticalType.CostAmount = (saleReportSumStatisticalType.CostAmount ?? 0) + (b.CostAmount ?? 0);
                                saleReportSumStatisticalType.Profit = (saleReportSumStatisticalType.Profit ?? 0) + (b.Profit ?? 0);
                                if (saleReportSumStatisticalType.CostAmount == null || saleReportSumStatisticalType.CostAmount == 0)
                                {
                                    saleReportSumStatisticalType.CostProfitRate = 100;
                                }
                                else
                                {
                                    saleReportSumStatisticalType.CostProfitRate = ((saleReportSumStatisticalType.Profit ?? 0) / saleReportSumStatisticalType.CostAmount) * 100;
                                }

                            }
                        });
                    }
                });
            }

            if (model.Items != null && model.Items.Count > 0)
            {
                //销售数量
                model.TotalSumSaleSmallQuantity = model.Items.Sum(m => m.SaleSmallQuantity ?? 0);
                //退货数量
                model.TotalSumReturnSmallQuantity = model.Items.Sum(m => m.ReturnSmallQuantity ?? 0);
                //赠送数量
                model.TotalSumGiftSmallQuantity = model.Items.Sum(m => m.GiftSmallQuantity ?? 0);
                //净销数量 = 销售数量 - 退货数量
                model.TotalSumNetSmallQuantity = model.TotalSumSaleSmallQuantity - model.TotalSumReturnSmallQuantity;
                //销售金额
                model.TotalSumSaleAmount = model.Items.Sum(m => m.SaleAmount ?? 0);
                //退货金额
                model.TotalSumReturnAmount = model.Items.Sum(m => m.ReturnAmount ?? 0);
                //净销金额 = 销售金额 - 退货金额
                model.TotalSumNetAmount = model.TotalSumSaleAmount - model.TotalSumReturnAmount;
                //优惠金额
                model.TotalSumDiscountAmount = model.Items.Sum(m => m.DiscountAmount ?? 0);
                //成本金额
                model.TotalSumCostAmount = model.Items.Sum(m => m.CostAmount ?? 0);
                //利润
                model.TotalSumProfit = model.Items.Sum(m => m.Profit ?? 0);
                //成本利润率
                if (model.TotalSumCostAmount == null || model.TotalSumCostAmount == 0)
                {
                    model.TotalSumCostProfitRate = 100;
                }
                else
                {
                    model.TotalSumCostProfitRate = (model.TotalSumProfit / model.TotalSumCostAmount) * 100;
                }

            }

            #endregion

            model.PagingFilteringContext.LoadPagedList(items);

            //设置图表数据
            model.Charts = "";
            if (model.Items != null && model.Items.Count > 0)
            {
                foreach (var item in model.Items.OrderBy(m => m.NetAmount).ToList())
                {
                    model.Charts = model.Charts + (model.Charts == "" ? "" : ",") + item.BrandName + "|" + item.NetAmount;
                }
            }

            return View(model);
        }

        //销售汇总（按品牌）导出
        [AuthCode((int)AccessGranularityEnum.SaleSummaryByStockExport)]
        public FileResult ExportSaleReportSummaryBrand(int? brandId, int? districtId, int? channelId,
            int? businessUserId, int? deliveryUserId, DateTime? startTime, DateTime? endTime, bool? auditedStatus = null)
        {


            //商品统计类别动态列
            var statisticalTypes = _statisticalTypeService.GetAllStatisticalTypess(curStore?.Id ?? 0);
            Dictionary<int, string> dic = new Dictionary<int, string>();
            if (statisticalTypes != null && statisticalTypes.Count > 0)
            {
                //model.DynamicColumns = statisticalTypes.OrderBy(a => a.Id).Select(a => a.Name).ToList();
                foreach (var c in statisticalTypes)
                {
                    dic.Add(c.Id, c.Name);
                }
            }
            dic.Add((int)StatisticalTypeEnum.OtherTypeId, "其他");

            #region 查询导出数据

            var sqlDatas = _saleReportService.GetSaleReportSummaryBrand(curStore?.Id ?? 0,
                new int[] { brandId ?? 0 }, districtId, channelId,
                businessUserId, deliveryUserId,
                startTime ?? DateTime.Parse(DateTime.Now.ToString("yyyy-MM-01 00:00:00")),
                endTime ?? DateTime.Now.AddDays(1), dic, auditedStatus: auditedStatus
                );

            #endregion

            #region 导出
            var ms = _exportManager.ExportSaleReportSummaryBrandToXlsx(sqlDatas, curStore.Id);
            if (ms != null)
            {
                return File(ms, "application/vnd.ms-excel", "销售汇总（按品牌）.xlsx");
            }
            else
            {
                return File(new MemoryStream(), "application/vnd.ms-excel", "销售汇总（按品牌）.xlsx");
            }
            #endregion
        }

        #endregion


        #region 订单明细
        /// <summary>
        /// 订单明细
        /// </summary>
        /// <param name="productId">商品Id</param>
        /// <param name="productName">商品名称</param>
        /// <param name="categoryId">商品类别Id</param>
        /// <param name="brandId">品牌Id</param>
        /// <param name="terminalId">客户Id</param>
        /// <param name="terminalName">客户名称</param>
        /// <param name="billNumber">单据编号</param>
        /// <param name="rankId">客户等级Id</param>
        /// <param name="bussinessUserId">业务员Id</param>
        /// <param name="wareHouseId">仓库Id</param>
        /// <param name="saleTypeId">销售类型Id</param>
        /// <param name="channelId">客户渠道Id</param>
        /// <param name="startTime">开始日期</param>
        /// <param name="endTime">结束日期</param>
        /// <param name="districtId">客户片区Id</param>
        /// <param name="remark">备注</param>
        /// <param name="costContractProduct">费用合同兑现商品</param>
        /// <param name="occupyStock">只展示占用库存商品</param>
        /// <param name="pagenumber"></param>
        /// <returns></returns>
        [HttpGet]
        [AuthCode((int)AccessGranularityEnum.OrderDetailsView)]
        public IActionResult SaleReportOrderItem(int? productId, string productName, int? categoryId, int? brandId, int? terminalId, string terminalName, string billNumber, int? rankId, int? businessUserId, int? wareHouseId, int? saleTypeId, int? channelId, DateTime? startTime, DateTime? endTime, int? districtId, string remark, bool? costContractProduct, bool? occupyStock, int pagenumber = 0)
        {

            var model = new SaleReportOrderItemListModel();

            #region 绑定数据源

            //商品
            model.ProductId = productId;
            model.ProductName = productName;

            //商品类别
            model.Categories = BindCategorySelection(_productCategoryService.BindCategoryList, curStore);
            model.CategoryId = (categoryId ?? null);

            //品牌
            model.Brands = BindBrandSelection(_brandService.BindBrandList, curStore);
            model.BrandId = (brandId ?? null);

            //客户
            model.TerminalId = terminalId;
            model.TerminalName = terminalName;

            //客户等级
            model.Ranks = BindRankSelection(_rankService.BindRankList, curStore);
            model.RankId = (rankId ?? null);

            //业务员
            model.BusinessUsers = BindUserSelection(_userService.BindUserList, curStore, DCMSDefaults.Salesmans,curUser.Id,true, _userService.IsAdmin(curStore.Id, curUser.Id));
            model.BusinessUserId = businessUserId ?? null;

            //仓库
            model.WareHouses = BindWareHouseSelection(_wareHouseService.BindWareHouseList, curStore,null,0);
            model.WareHouseId = (wareHouseId ?? null);

            //销售类型
            model.SaleTypes = new SelectList(from a in Enum.GetValues(typeof(SaleReportOrderItemSaleTypeEnum)).Cast<SaleReportOrderItemSaleTypeEnum>()
                                             select new SelectListItem
                                             {
                                                 Text = CommonHelper.GetEnumDescription(a),
                                                 Value = ((int)a).ToString()
                                             }, "Value", "Text");
            model.SaleTypeId = (saleTypeId ?? null);

            //客户渠道
            model.Channels = BindChanneSelection(_channelService.BindChannelList, curStore);
            model.ChannelId = (channelId ?? null);

            model.StartTime = startTime ?? DateTime.Parse(DateTime.Now.ToString("yyyy-MM-01 00:00:00"));
            model.EndTime = endTime ?? DateTime.Now.AddDays(1);

            //客户片区
            model.Districts = BindDistrictSelection(_districtService.BindDistrictList, curStore);
            model.DistrictId = (districtId ?? null);

            //备注
            model.Remark = remark;

            //费用合同兑现商品
            model.CostContractProduct = costContractProduct ?? null;

            //只展示占用库存商品
            model.OccupyStock = occupyStock ?? null;

            #endregion

            if (pagenumber > 0)
            {
                pagenumber -= 1;
            }

            var sqlDatas = _saleReportService.GetSaleReportOrderItem(curStore?.Id ?? 0,
                productId, productName, categoryId, brandId, terminalId, terminalName,
                billNumber, rankId, businessUserId, wareHouseId,
                saleTypeId, channelId,
               model.StartTime, //?? DateTime.Parse(DateTime.Now.ToString("yyyy-MM-dd")),
                model.EndTime, //?? DateTime.Now.AddDays(1),
                districtId, remark, costContractProduct, occupyStock
                );

            var items = new PagedList<SaleReportOrderItem>(sqlDatas, pagenumber, 30);

            model.Items = items;

            #region 汇总

            if (items != null && items.Count > 0)
            {

                //数量
                model.PageSumQuantityConversion = items.Sum(a => a.SaleReturnBigQuantity ?? 0) + "大" + items.Sum(a => a.SaleReturnStrokeQuantity ?? 0) + "中" + items.Sum(a => a.SaleReturnSmallQuantity) + "小";
                //金额
                model.PageSumAmount = items.Sum(a => a.Amount);
                //成本金额
                model.PageSumCostAmount = items.Sum(a => a.CostAmount);
                //利润
                model.PageSumProfit = items.Sum(a => a.Profit);
                //成本利润率
                model.PageSumCostProfitRate = items.Sum(a => a.CostProfitRate) / items.Count();
            }

            if (sqlDatas != null && sqlDatas.ToList().Count > 0)
            {

                //数量
                model.TotalSumQuantityConversion = //数量
                model.PageSumQuantityConversion = sqlDatas.Sum(a => a.SaleReturnBigQuantity ?? 0) + "大" + sqlDatas.Sum(a => a.SaleReturnStrokeQuantity ?? 0) + "中" + sqlDatas.Sum(a => a.SaleReturnSmallQuantity) + "小";
                //金额
                model.TotalSumAmount = sqlDatas.Sum(a => a.Amount ?? 0);
                //成本金额
                model.TotalSumCostAmount = sqlDatas.Sum(a => a.CostAmount ?? 0);
                //利润
                model.TotalSumProfit = sqlDatas.Sum(a => a.Profit ?? 0);
                //成本利润率
                if (model.TotalSumCostAmount == null || model.TotalSumCostAmount == 0)
                {
                    model.TotalSumCostProfitRate = 100;
                }
                else
                {
                    model.TotalSumCostProfitRate = (model.TotalSumProfit / model.TotalSumCostAmount) * 100;
                }
            }

            #endregion

            model.PagingFilteringContext.LoadPagedList(items);

            return View(model);
        }

        //订单明细表导出
        [AuthCode((int)AccessGranularityEnum.OrderDetailsExport)]
        public FileResult ExportSaleReportOrderItem(int? productId, string productName, int? categoryId, int? brandId, int? terminalId, string terminalName, string billNumber, int? rankId, int? bussinessUserId, int? wareHouseId, int? saleTypeId, int? channelId, DateTime? startTime, DateTime? endTime, int? districtId, string remark, bool? costContractProduct, bool? occupyStock)
        {

            #region 查询导出数据

            var sqlDatas = _saleReportService.GetSaleReportOrderItem(curStore?.Id ?? 0,
                productId, productName, categoryId, brandId, terminalId, terminalName,
                billNumber, rankId, bussinessUserId, wareHouseId,
                saleTypeId, channelId, startTime ?? DateTime.Parse(DateTime.Now.ToString("yyyy-MM-01 00:00:00")), endTime ?? DateTime.Now.AddDays(1),
                districtId, remark, costContractProduct, occupyStock
                );

            #endregion

            #region 导出
            var ms = _exportManager.ExportSaleReportOrderItemToXlsx(sqlDatas);
            if (ms != null)
            {
                return File(ms, "application/vnd.ms-excel", "订单明细表.xlsx");
            }
            else
            {
                return File(new MemoryStream(), "application/vnd.ms-excel", "订单明细表.xlsx");
            }
            #endregion
        }

        #endregion

        #region 订单汇总（按商品）
        /// <summary>
        /// 订单汇总（按商品）
        /// </summary>
        /// <param name="productId">商品Id</param>
        /// <param name="productName">商品名称</param>
        /// <param name="categoryId">商品类别Id</param>
        /// <param name="bussinessUserId">业务员Id</param>
        /// <param name="wareHouseId">仓库Id</param>
        /// <param name="districtId">客户片区Id</param>
        /// <param name="terminalId">客户Id</param>
        /// <param name="terminalName">客户名称</param>
        /// <param name="channelId">客户渠道Id</param>
        /// <param name="remark">备注</param>
        /// <param name="startTime">开始日期</param>
        /// <param name="endTime">结束日期</param>
        /// <param name="deliveryUserId">送货员Id</param>
        /// <param name="rankId">客户等级Id</param>
        /// <param name="costContractProduct">费用合同兑现商品</param>
        /// <param name="pagenumber"></param>
        /// <returns></returns>
        [HttpGet]
        [AuthCode((int)AccessGranularityEnum.OrderSummaryByProductView)]
        public IActionResult SaleReportSummaryOrderProduct(int? productId, string productName, int? categoryId, int? businessUserId, int? wareHouseId, int? districtId, int? terminalId, string terminalName, int? channelId, string remark, DateTime? startTime, DateTime? endTime, int? deliveryUserId, int? rankId, bool? costContractProduct, int pagenumber = 0)
        {


            var model = new SaleReportSummaryOrderProductListModel();

            #region 绑定数据源

            //商品
            model.ProductId = productId;
            model.ProductName = productName;

            //商品类别
            model.Categories = BindCategorySelection(_productCategoryService.BindCategoryList, curStore);
            model.CategoryId = (categoryId ?? null);

            //业务员
            model.BusinessUsers = BindUserSelection(_userService.BindUserList, curStore, DCMSDefaults.Salesmans,curUser.Id,true, _userService.IsAdmin(curStore.Id, curUser.Id));
            model.BusinessUserId = (businessUserId ?? null);

            //仓库
            model.WareHouses = BindWareHouseSelection(_wareHouseService.BindWareHouseList, curStore,null,0);
            model.WareHouseId = (wareHouseId ?? null);

            //客户片区
            model.Districts = BindDistrictSelection(_districtService.BindDistrictList, curStore);
            model.DistrictId = (districtId ?? null);

            //客户
            model.TerminalId = terminalId;
            model.TerminalName = terminalName;

            //客户渠道
            model.Channels = BindChanneSelection(_channelService.BindChannelList, curStore);
            model.ChannelId = (channelId ?? null);

            //备注
            model.Remark = remark;

            model.StartTime = startTime ?? DateTime.Parse(DateTime.Now.ToString("yyyy-MM-01 00:00:00"));
            model.EndTime = endTime ?? DateTime.Now.AddDays(1);

            //送货员
            model.DeliveryUsers = BindUserSelection(_userService.BindUserList, curStore, DCMSDefaults.Delivers, curUser.Id, true, _userService.IsAdmin(curStore.Id, curUser.Id));
            model.DeliveryUserId = (deliveryUserId ?? null);

            //客户等级
            model.Ranks = BindRankSelection(_rankService.BindRankList, curStore);
            model.RankId = (rankId ?? null);

            //过滤
            model.CostContractProduct = costContractProduct ?? null;

            #endregion

            if (pagenumber > 0)
            {
                pagenumber -= 1;
            }

            var sqlDatas = _saleReportService.GetSaleReportSummaryOrderProduct(curStore?.Id ?? 0,
                productId, productName, categoryId, businessUserId, wareHouseId,
                districtId, terminalId, terminalName, channelId, remark,
                model.StartTime, //?? DateTime.Parse(DateTime.Now.ToString("yyyy-MM-dd")),
                model.EndTime, //?? DateTime.Now.AddDays(1),
                deliveryUserId, rankId,
                costContractProduct
                );

            var items = new PagedList<SaleReportSummaryOrderProduct>(sqlDatas, pagenumber, 30);

            model.Items = items;

            #region 汇总

            if (sqlDatas != null && sqlDatas.ToList().Count > 0)
            {

                //销售数量
                model.TotalSumSaleQuantityConversion = sqlDatas.Sum(a => a.SaleBigQuantity ?? 0) + "大" + sqlDatas.Sum(a => a.SaleStrokeQuantity ?? 0) + "中" + sqlDatas.Sum(a => a.SaleSmallQuantity ?? 0) + "小";
                //销售金额
                model.TotalSumSaleAmount = items.Sum(a => a.SaleAmount ?? 0);
                //赠送数量
                model.TotalSumGiftQuantityConversion = sqlDatas.Sum(a => a.GiftBigQuantity ?? 0) + "大" + sqlDatas.Sum(a => a.GiftStrokeQuantity ?? 0) + "中" + sqlDatas.Sum(a => a.GiftSmallQuantity ?? 0) + "小";
                //退货数量
                model.TotalSumReturnQuantityConversion = sqlDatas.Sum(a => a.ReturnBigQuantity ?? 0) + "大" + sqlDatas.Sum(a => a.ReturnStrokeQuantity ?? 0) + "中" + sqlDatas.Sum(a => a.ReturnSmallQuantity ?? 0) + "小";
                //退货金额
                model.TotalSumReturnAmount = sqlDatas.Sum(a => a.ReturnAmount ?? 0);
                //净销数量
                model.TotalSumNetQuantityConversion = (sqlDatas.Sum(a => a.SaleBigQuantity ?? 0) - sqlDatas.Sum(a => a.ReturnBigQuantity ?? 0)) + "大" +
                                                      (sqlDatas.Sum(a => a.SaleStrokeQuantity ?? 0) - sqlDatas.Sum(a => a.ReturnStrokeQuantity ?? 0)) + "中" +
                                                      (sqlDatas.Sum(a => a.SaleSmallQuantity ?? 0) - sqlDatas.Sum(a => a.ReturnSmallQuantity ?? 0)) + "小";
                //净销金额
                model.TotalSumNetAmount = model.TotalSumSaleAmount - model.TotalSumReturnAmount;
                //成本金额
                model.TotalSumCostAmount = sqlDatas.Sum(a => a.CostAmount ?? 0);
                //利润
                model.TotalSumProfit = sqlDatas.Sum(a => a.Profit ?? 0);
                //成本利润率
                if (model.TotalSumCostAmount == null || model.TotalSumCostAmount == 0)
                {
                    model.TotalSumCostProfitRate = 100;
                }
                else
                {
                    model.TotalSumCostProfitRate = (model.TotalSumProfit / model.TotalSumCostAmount) * 100;
                }

            }

            #endregion

            model.PagingFilteringContext.LoadPagedList(items);

            return View(model);
        }

        //订单汇总（按商品）导出
        [AuthCode((int)AccessGranularityEnum.OrderSummaryByProductExport)]
        public FileResult ExportSaleReportSummaryOrderProduct(int? productId, string productName, int? categoryId, int? businessUserId, int? wareHouseId, int? districtId, int? terminalId, string terminalName, int? channelId, string remark, DateTime? startTime, DateTime? endTime, int? deliveryUserId, int? rankId, bool? costContractProduct)
        {

            #region 查询导出数据

            var sqlDatas = _saleReportService.GetSaleReportSummaryOrderProduct(curStore?.Id ?? 0,
                productId, productName, categoryId, businessUserId, wareHouseId,
                districtId, terminalId, terminalName, channelId, remark,
                startTime ?? DateTime.Parse(DateTime.Now.ToString("yyyy-MM-01 00:00:00")), endTime ?? DateTime.Now.AddDays(1), deliveryUserId, rankId,
                costContractProduct
                );
            #endregion

            #region 导出
            var ms = _exportManager.ExportSaleReportSummaryOrderProductToXlsx(sqlDatas);
            if (ms != null)
            {
                return File(ms, "application/vnd.ms-excel", "订单汇总（按商品）.xlsx");
            }
            else
            {
                return File(new MemoryStream(), "application/vnd.ms-excel", "订单汇总（按商品）.xlsx");
            }
            #endregion
        }

        #endregion

        #region 费用合同明细表
        /// <summary>
        /// 费用合同明细表
        /// </summary>
        /// <param name="terminalId">客户Id</param>
        /// <param name="terminalName">客户名称</param>
        /// <param name="productId">商品Id</param>
        /// <param name="productName">商品名称</param>
        /// <param name="bussinessUserId">业务员Id</param>
        /// <param name="accountingOptionId">费用类别Id</param>
        /// <param name="accountingOptionName">费用类别名称</param>
        /// <param name="billNumber">单据编号</param>
        /// <param name="categoryId">商品类别Id</param>
        /// <param name="cashTypeId">兑现方式Id</param>
        /// <param name="remark">备注</param>
        /// <param name="statusTypeId">状态Id</param>
        /// <param name="startTime">开始日期</param>
        /// <param name="endTime">结束日期</param>
        /// <param name="pagenumber"></param>
        /// <returns></returns>
        [HttpGet]
        [AuthCode((int)AccessGranularityEnum.ExpenseDetailsView)]
        public IActionResult SaleReportCostContractItem(int? terminalId, string terminalName, int? productId, string productName, int? businessUserId, int? accountingOptionId, string accountingOptionName,
            string billNumber, int? categoryId, int? cashTypeId, string remark,
            int? statusTypeId, DateTime? startTime, DateTime? endTime, int pagenumber = 0)
        {


            
            var model = new SaleReportCostContractItemListModel();

            #region 绑定数据源

            //客户
            model.TerminalId = terminalId;
            model.TerminalName = terminalName;

            //商品
            model.ProductId = productId;
            model.ProductName = productName;

            //业务员
            model.BusinessUsers = BindUserSelection(_userService.BindUserList, curStore, DCMSDefaults.Salesmans,curUser.Id,true, _userService.IsAdmin(curStore.Id, curUser.Id));
            model.BusinessUserId = businessUserId ?? null;

            //费用类别
            model.AccountingOptionId = accountingOptionId;
            model.AccountingOptionName = accountingOptionName;

            //商品类别
            model.Categories = BindCategorySelection(_productCategoryService.BindCategoryList, curStore);
            model.CategoryId = (categoryId ?? null);

            //兑现方式
            model.CashTypes = new SelectList(from a in Enum.GetValues(typeof(CostContractItemCashTypeEnum)).Cast<CostContractItemCashTypeEnum>()
                                             select new SelectListItem
                                             {
                                                 Text = CommonHelper.GetEnumDescription(a),
                                                 Value = ((int)a).ToString()
                                             }, "Value", "Text");
            model.CashTypeId = (cashTypeId ?? null);

            //状态
            model.StatusTypes = new SelectList(from a in Enum.GetValues(typeof(CostContractStatusEnum)).Cast<CostContractStatusEnum>()
                                               select new SelectListItem
                                               {
                                                   Text = CommonHelper.GetEnumDescription(a),
                                                   Value = ((int)a).ToString()
                                               }, "Value", "Text");
            model.StatusTypeId = (statusTypeId ?? null);

            model.StartTime = startTime ?? DateTime.Parse(DateTime.Now.ToString("yyyy-MM-01 00:00:00"));
            model.EndTime = endTime ?? DateTime.Now.AddDays(1);

            #endregion

            if (pagenumber > 0)
            {
                pagenumber -= 1;
            }

            var sqlDatas = _saleReportService.GetSaleReportCostContractItem(curStore?.Id ?? 0,
                terminalId, terminalName, productId, productName, businessUserId, accountingOptionId,
                billNumber, categoryId, cashTypeId, remark,
                statusTypeId,
                model.StartTime, //?? DateTime.Parse(DateTime.Now.ToString("yyyy-MM-dd")),
                model.EndTime //?? DateTime.Now.AddDays(1),
                );

            var items = new PagedList<SaleReportCostContractItem>(sqlDatas, pagenumber, 30);

            model.Items = items;

            #region 汇总

            decimal TotalBigQuantity = 0;
            decimal TotalStrokeQuantity = 0;
            decimal TotalSmallQuantity = 0;
            if (sqlDatas != null && sqlDatas.ToList().Count > 0)
            {
                foreach (var it in sqlDatas)
                {
                    switch (it.UnitBigStrokeSmall)
                    {
                        case "大":
                            TotalBigQuantity += it.Total ?? 0;
                            break;
                        case "中":
                            TotalStrokeQuantity += it.Total ?? 0;
                            break;
                        case "小":
                            TotalSmallQuantity += it.Total ?? 0;
                            break;
                        default:
                            break;
                    }
                }
                //数量
                model.TotalSumQuantityConversion = TotalBigQuantity + "大" + TotalStrokeQuantity + "中" + TotalSmallQuantity + "小";
            }

            #endregion

            model.PagingFilteringContext.LoadPagedList(items);

            return View(model);
        }
        #endregion

        #region 赠品汇总
        /// <summary>
        /// 赠品汇总
        /// </summary>
        /// <param name="productId">商品Id</param>
        /// <param name="productName">商品名称</param>
        /// <param name="terminalId">客户Id</param>
        /// <param name="terminalName">客户名称</param>
        /// <param name="categoryId">商品类别Id</param>
        /// <param name="brandId">品牌Id</param>
        /// <param name="remark">备注</param>
        /// <param name="startTime">开始日期</param>
        /// <param name="endTime">结束日期</param>
        /// <param name="bussinessUserId">业务员Id</param>
        /// <param name="pagenumber"></param>
        /// <returns></returns>
        [HttpGet]
        [AuthCode((int)AccessGranularityEnum.GiftSummaryView)]
        public IActionResult SaleReportSummaryGiveQuota(int? businessUserId, int? productId, string productName, int? terminalId, string terminalName, int? categoryId, int? costingCalCulateMethodId, int? giveTypeId, DateTime? startTime, DateTime? endTime, string remark = "", int pagenumber = 0)
        {


            var model = new SaleReportSummaryGiveQuotaListModel();

            #region 绑定数据源

            //商品
            model.ProductId = productId;
            model.ProductName = productName;

            //客户
            model.TerminalId = terminalId;
            model.TerminalName = terminalName;

            //商品类别
            model.Categories = BindCategorySelection(_productCategoryService.BindCategoryList, curStore);
            model.CategoryId = (categoryId ?? null);

            //备注
            model.Remark = remark;

            //业务员
            model.BusinessUsers = BindUserSelection(_userService.BindUserList, curStore, DCMSDefaults.Salesmans, curUser.Id, true, _userService.IsAdmin(curStore.Id, curUser.Id));
            model.BusinessUserId = (businessUserId ?? null);


            model.StartTime = startTime ?? DateTime.Parse(DateTime.Now.ToString("yyyy-MM-01 00:00:00"));
            model.EndTime = endTime ?? DateTime.Now.AddDays(1);

            //获取RemarkConfig
            var remarkConfigs = _remarkConfigService.GetAllRemarkConfigs(curStore.Id);
            
            #endregion

            if (pagenumber > 0)
                pagenumber -= 1;

            var summeries = _giveQuotaService.GetAllGiveQuotaRecordsSummeries(curStore?.Id ?? 0,
                businessUserId,
                productId,
                productName,
                terminalId,
                terminalName,
                categoryId,
                costingCalCulateMethodId,
                giveTypeId,
                model.StartTime,
                model.EndTime,
                pagenumber,
                30);

            model.Items = summeries;
            model.PagingFilteringContext.LoadPagedList(summeries);

            model.TotalGeneralQuantity = $"{summeries.Sum(c => c.GeneralQuantityTuple.Item1)}大{summeries.Sum(c => c.GeneralQuantityTuple.Item2)}中{summeries.Sum(c => c.GeneralQuantityTuple.Item3)}小";
            model.TotalGeneralCostAmount = summeries.Sum(c => c.GeneralQuantityTuple.Item4);

            model.TotalOrderQuantity = "0大0中0小";
            model.TotalOrderCostAmount = 0;
       
            model.TotalPromotionalQuantity = $"{summeries.Sum(c => c.PromotionalQuantityTuple.Item1)}大{summeries.Sum(c => c.PromotionalQuantityTuple.Item2)}中{summeries.Sum(c => c.PromotionalQuantityTuple.Item3)}小";
            model.TotalPromotionalCostAmount = summeries.Sum(c => c.PromotionalQuantityTuple.Item4);

            model.TotalContractQuantity = $"{summeries.Sum(c => c.ContractQuantityTuple.Item1)}大{summeries.Sum(c => c.ContractQuantityTuple.Item2)}中{summeries.Sum(c => c.ContractQuantityTuple.Item3)}小";
            model.TotalContractCostAmount = summeries.Sum(c => c.ContractQuantityTuple.Item4);

            var lst = new List<TotalOrdinaryGiftSummery>();
            var lst1 = new List<RemarkConfig>();
            foreach (var item in remarkConfigs)
            {
                var entity = new TotalOrdinaryGiftSummery();
                entity.RemarkConfigId = item.Id;
                foreach (var item1 in summeries)
                {
                    var entity1 = item1.OrdinaryGiftSummerys.Where(w=>w.RemarkConfigId == item.Id).FirstOrDefault();
                    if (entity1 != null) 
                    {
                        entity.TotalBigQuantity += entity1.QuantityTuple.Item1;
                        entity.TotalStockQuantity += entity1.QuantityTuple.Item2;
                        entity.TotalSmailQuantity += entity1.QuantityTuple.Item3;
                        entity.TotalCostAmount += entity1.QuantityTuple.Item4;
                    }
                    //
                    if (item.Name == "搭赠")
                    {
                        var entity2 = item1.OrdinaryGiftSummerys.Where(w => w.RemarkConfigId == 0).FirstOrDefault();
                        if (entity2 != null) 
                        {
                            entity.TotalBigQuantity += entity2.QuantityTuple.Item1;
                            entity.TotalStockQuantity += entity2.QuantityTuple.Item2;
                            entity.TotalSmailQuantity += entity2.QuantityTuple.Item3;
                            entity.TotalCostAmount += entity2.QuantityTuple.Item4;
                        }
                    }
                    
                }
                if (entity.TotalBigQuantity > 0 || entity.TotalStockQuantity >0 || entity.TotalSmailQuantity>0) 
                {
                    lst1.Add(item);
                }
                lst.Add(entity);
            }
            model.RemarkConfigs = lst1;
            model.TotalOrdinaryGiftSummerys = lst;

            return View(model);
        }


        //赠品汇总导出
        [HttpPost]
        [AuthCode((int)AccessGranularityEnum.GiftSummaryExport)]
        public FileResult ExportSaleReportSummaryGiveQuota(SaleReportSummaryGiveQuotaListModel model)
        {
            #region 导出

            var ms = _exportManager.ExportSaleReportSummaryGiveQuotaToXlsx(model.Items);
            if (ms != null)
            {
                return File(ms, "application/vnd.ms-excel", "赠品汇总.xlsx");
            }
            else
            {
                return File(new MemoryStream(), "application/vnd.ms-excel", "赠品汇总.xlsx");
            }

            #endregion
        }

        #endregion

        #region 热销排行榜
        /// <summary>
        /// 热销排行榜
        /// </summary>
        /// <param name="productId">商品Id</param>
        /// <param name="productName">商品名称</param>
        /// <param name="wareHouseId">仓库Id</param>
        /// <param name="terminalId">客户Id</param>
        /// <param name="terminalName">客户名称</param>
        /// <param name="brandId">品牌Id</param>
        /// <param name="categoryId">商品类别Id</param>
        /// <param name="startTime">开始日期</param>
        /// <param name="endTime">结束日期</param>
        /// <param name="topNumber">统计前</param>
        /// <param name="pagenumber"></param>
        /// <returns></returns>
        [HttpGet]
        [AuthCode((int)AccessGranularityEnum.HotRatingView)]
        public IActionResult SaleReportHotSale(int? productId, string productName, int? wareHouseId, int? terminalId, string terminalName, int? brandId,
            int? categoryId, DateTime? startTime, DateTime? endTime, int? topNumber, int pagenumber = 0)
        {

            var model = new SaleReportHotSaleListModel();

            #region 绑定数据源

            //商品
            model.ProductId = productId;
            model.ProductName = productName;

            //仓库
            model.WareHouses = BindWareHouseSelection(_wareHouseService.BindWareHouseList, curStore,null,0);
            model.WareHouseId = (wareHouseId ?? null);

            //客户
            model.TerminalId = terminalId;
            model.TerminalName = terminalName;

            //品牌
            model.Brands = BindBrandSelection(_brandService.BindBrandList, curStore);
            model.BrandId = (brandId ?? null);

            //商品类别
            model.Categories = BindCategorySelection(_productCategoryService.BindCategoryList, curStore);
            model.CategoryId = (categoryId ?? null);

            model.StartTime = startTime ?? DateTime.Parse(DateTime.Now.ToString("yyyy-MM-01"));
            model.EndTime = endTime ?? DateTime.Now.AddDays(1);

            model.TopNumber = topNumber ?? null;

            #endregion

            if (pagenumber > 0)
            {
                pagenumber -= 1;
            }

            var sqlDatas = _saleReportService.GetSaleReportHotSale(curStore?.Id ?? 0,
                productId, productName, null, wareHouseId, terminalId, terminalName, brandId,
                categoryId,
                model.StartTime, //?? DateTime.Parse(DateTime.Now.ToString("yyyy-MM-dd")),
                model.EndTime, //?? DateTime.Now.AddDays(1),
                topNumber
                );

            var items = new PagedList<SaleReportHotSale>(sqlDatas, pagenumber, 30);

            model.Items = items;

            #region 汇总

            if (sqlDatas != null && sqlDatas.ToList().Count > 0)
            {

                //销售数量
                model.TotalSumSaleQuantityConversion = sqlDatas.Sum(a => a.SaleBigQuantity ?? 0) + "大" + sqlDatas.Sum(a => a.SaleStrokeQuantity ?? 0) + "中" + sqlDatas.Sum(a => a.SaleSmallQuantity ?? 0) + "小";
                //销售金额
                model.TotalSumSaleAmount = items.Sum(a => a.SaleAmount ?? 0);
                //退货数量
                model.TotalSumReturnQuantityConversion = sqlDatas.Sum(a => a.ReturnBigQuantity ?? 0) + "大" + sqlDatas.Sum(a => a.ReturnStrokeQuantity ?? 0) + "中" + sqlDatas.Sum(a => a.ReturnSmallQuantity ?? 0) + "小";
                //退货金额
                model.TotalSumReturnAmount = sqlDatas.Sum(a => a.ReturnAmount ?? 0);
                //净销数量
                model.TotalSumNetQuantityConversion = (sqlDatas.Sum(a => a.SaleBigQuantity ?? 0) - sqlDatas.Sum(a => a.ReturnBigQuantity ?? 0)) + "大" +
                                                      (sqlDatas.Sum(a => a.SaleStrokeQuantity ?? 0) - sqlDatas.Sum(a => a.ReturnStrokeQuantity ?? 0)) + "中" +
                                                      (sqlDatas.Sum(a => a.SaleSmallQuantity ?? 0) - sqlDatas.Sum(a => a.ReturnSmallQuantity ?? 0)) + "小";
                //净销金额
                model.TotalSumNetAmount = model.TotalSumSaleAmount - model.TotalSumReturnAmount;
                //成本金额
                model.TotalSumCostAmount = sqlDatas.Sum(a => a.CostAmount ?? 0);
                //利润
                model.TotalSumProfit = sqlDatas.Sum(a => a.Profit ?? 0);
                //成本利润率
                if (model.TotalSumCostAmount == null || model.TotalSumCostAmount == 0)
                {
                    model.TotalSumCostProfitRate = 100;
                }
                else
                {
                    model.TotalSumCostProfitRate = (model.TotalSumProfit / model.TotalSumCostAmount) * 100;
                }

            }

            #endregion

            model.PagingFilteringContext.LoadPagedList(items);

            //设置图表数据
            model.Charts = "";
            if (model.Items != null && model.Items.Count > 0)
            {
                foreach (var item in model.Items.OrderByDescending(m => m.NetAmount).ToList())
                {
                    //转换成最小数量
                    int quantity = ((item.NetBigQuantity ?? 0) * (item.BigQuantity ?? 0)) + ((item.NetStrokeQuantity ?? 0) * (item.StrokeQuantity ?? 0)) + (item.NetSmallQuantity ?? 0);
                    model.Charts = model.Charts + (model.Charts == "" ? "" : ",") + item.ProductName + "|" + quantity + "|" + item.NetAmount;
                }
            }

            return View(model);
        }

        //热销排行榜导出
        public FileResult ExportSaleReportHotSale(int? productId, string productName, int? wareHouseId, int? terminalId, string terminalName, int? brandId,
            int? categoryId, DateTime? startTime, DateTime? endTime, int? topNumber)
        {

            #region 查询导出数据
            var sqlDatas = _saleReportService.GetSaleReportHotSale(curStore?.Id ?? 0,
                productId, productName, null, wareHouseId, terminalId, terminalName, brandId,
                categoryId,
                (startTime == null) ? DateTime.Parse(DateTime.Now.ToString("yyyy-MM-01 00:00:00")) : startTime,
                (endTime == null) ? DateTime.Now.AddDays(1) : endTime,
                topNumber
                );

            #endregion

            #region 导出
            var ms = _exportManager.ExportSaleReportHotSaleToXlsx(sqlDatas);
            if (ms != null)
            {
                return File(ms, "application/vnd.ms-excel", "热销排行榜.xlsx");
            }
            else
            {
                return File(new MemoryStream(), "application/vnd.ms-excel", "热销排行榜.xlsx");
            }
            #endregion
        }

        #endregion


        #region 销量走势图
        /// <summary>
        /// 销量走势图
        /// </summary>
        /// <param name="startTime">开始日期</param>
        /// <param name="endTime">结束日期</param>
        /// <param name="topNumber">统计方式</param>
        /// <param name="pagenumber"></param>
        /// <returns></returns>
        [HttpGet]
        [AuthCode((int)AccessGranularityEnum.SaleChartView)]
        public IActionResult SaleReportSaleQuantityTrend(DateTime? startTime, DateTime? endTime, int? groupByTypeId, int pagenumber = 0)
        {


            var model = new SaleReportSaleQuantityTrendListModel();

            //开始日期 默认 当前日期
            if (startTime == null)
            {
                startTime =DateTime.Parse(DateTime.Now.ToString("yyyy-MM-01"));
            }

            //结束日期 默认 当前日期 +1
            if (endTime == null)
            {
                endTime = DateTime.Now.AddDays(1);
            }
            //统计类型 默认 日
            if (groupByTypeId == null || groupByTypeId == 0)
            {
                groupByTypeId = 1;
            }

            #region 绑定数据源
            model.StartTime = startTime ?? DateTime.Parse(DateTime.Now.ToString("yyyy-MM-01"));
            model.EndTime = endTime ?? DateTime.Now.AddDays(1);

            //统计方式
            model.GroupByTypes = new SelectList(from a in Enum.GetValues(typeof(SaleReportSaleQuantityTrendGroupByTypeEnum)).Cast<SaleReportSaleQuantityTrendGroupByTypeEnum>()
                                                select new SelectListItem
                                                {
                                                    Text = CommonHelper.GetEnumDescription(a),
                                                    Value = ((int)a).ToString()
                                                }, "Value", "Text");
            model.GroupByTypeId ??= 1;

            #endregion

            //if (pagenumber > 0)
            //    pagenumber -= 1;

            var sqlDatas = _saleReportService.GetSaleReportSaleQuantityTrend(curStore?.Id ?? 0,
                model.StartTime,
                model.EndTime,
                groupByTypeId
                ).Select(sd =>
                {
                    return sd ?? new SaleReportSaleQuantityTrend();

                }).AsQueryable();

            var items = new PagedList<SaleReportSaleQuantityTrend>(sqlDatas, pagenumber, 30);

            model.Items = items;

            #region 汇总

            #endregion

            model.PagingFilteringContext.LoadPagedList(items);

            //设置图表数据
            model.Charts = "";
            if (model.Items != null && model.Items.Count > 0)
            {
                //foreach (var item in model.Items)
                //{
                //    model.Charts = model.Charts + (model.Charts == "" ? "" : ",") + DateTime.Parse(item.ShowDate).ToString("yyyy-MM-dd")  + "|" + item.NetAmount;
                //}
                model.Items.GroupBy(s => DateTime.Parse(s.ShowDate).ToString("yyyy-MM-dd")).ForEach(h =>
                {
                    var totalAmount = model.Items.Where(o => DateTime.Parse(o.ShowDate).ToString("yyyy-MM-dd") == h.Key).Sum(a => a.NetAmount);

                    model.Charts = model.Charts + (model.Charts == "" ? "" : ",") + h.Key + "|" + totalAmount;
                });
            }

            return View(model);
        }

        //销量走势图
        public FileResult ExportSaleReportSaleQuantityTrend(DateTime? startTime, DateTime? endTime, int? groupByTypeId)
        {

            //统计类型 默认 日
            if (groupByTypeId == null || groupByTypeId == 0)
            {
                groupByTypeId = 1;
            }

            #region 表头
            DataTable dt = new DataTable("ExportSaleReportHotSale");
            dt.Columns.Add("日期", typeof(string));
            dt.Columns.Add("净销金额", typeof(string));

            #endregion

            #region 查询导出数据
            var sqlDatas = _saleReportService.GetSaleReportSaleQuantityTrend(curStore?.Id ?? 0,
                (startTime == null) ? DateTime.Parse(DateTime.Now.ToString("yyyy-MM-01 00:00:00")) : startTime,
                (endTime == null) ? DateTime.Now.AddDays(1) : endTime,
                groupByTypeId
                );

            #region 格式化
            sqlDatas.ToList().ForEach(s =>
            {
                DataRow dr = dt.NewRow();
                dr["日期"] = s.ShowDate;
                dr["净销金额"] = s.NetAmount;
                dt.Rows.Add(dr);
            });
            #endregion

            #endregion

            #region 导出
            var ms = _exportService.ExportTableToExcel("sheet1", dt);
            if (ms != null)
            {
                return File(ms, "application/vnd.ms-excel", "销量走势图.xls");
            }
            else
            {
                return File(new MemoryStream(), "application/vnd.ms-excel", "销量走势图.xls");
            }
            #endregion
        }


        #endregion


        #region 销售商品成本利润
        /// <summary>
        /// 销售商品成本利润
        /// </summary>
        /// <param name="productId">商品Id</param>
        /// <param name="productName">商品名称</param>
        /// <param name="categoryId">商品类别Id</param>
        /// <param name="brandId">品牌Id</param>
        /// <param name="channelId">客户渠道Id</param>
        /// <param name="terminalId">客户Id</param>
        /// <param name="terminalName">客户名称</param>
        /// <param name="bussinessUserId">业务员Id</param>
        /// <param name="wareHouseId">仓库Id</param>
        /// <param name="startTime">开始日期</param>
        /// <param name="endTime">结束日期</param>
        /// <param name="pagenumber"></param>
        /// <returns></returns>
        [HttpGet]
        [AuthCode((int)AccessGranularityEnum.CostProfitView)]
        public IActionResult SaleReportProductCostProfit(int? productId, string productName, int? categoryId, int? brandId, int? channelId, int? terminalId, string terminalName, int? businessUserId, int? wareHouseId, DateTime? startTime, DateTime? endTime, int pagenumber = 0)
        {


            var model = new SaleReportProductCostProfitListModel();

            #region 绑定数据源

            //商品
            model.ProductId = productId;
            model.ProductName = productName;

            //商品类别
            model.Categories = BindCategorySelection(_productCategoryService.BindCategoryList, curStore);
            model.CategoryId = (categoryId ?? null);

            //品牌
            model.Brands = BindBrandSelection(_brandService.BindBrandList, curStore);
            model.BrandId = (brandId ?? null);

            //客户渠道
            model.Channels = BindChanneSelection(_channelService.BindChannelList, curStore);
            model.ChannelId = (channelId ?? null);

            //客户
            model.TerminalId = terminalId;
            model.TerminalName = terminalName;

            //业务员
            model.BusinessUsers = BindUserSelection(_userService.BindUserList, curStore, DCMSDefaults.Salesmans,curUser.Id,true, _userService.IsAdmin(curStore.Id, curUser.Id));
            model.BusinessUserId = (businessUserId ?? null);

            //仓库
            model.WareHouses = BindWareHouseSelection(_wareHouseService.BindWareHouseList, curStore,null,0);
            model.WareHouseId = (wareHouseId ?? null);

            model.StartTime = startTime ?? DateTime.Parse(DateTime.Now.ToString("yyyy-MM-01 00:00:00"));
            model.EndTime = endTime ?? DateTime.Now.AddDays(1);

            #endregion

            if (pagenumber > 0)
            {
                pagenumber -= 1;
            }

            var sqlDatas = _saleReportService.GetSaleReportProductCostProfit(curStore?.Id ?? 0,
                productId, productName, categoryId, brandId, channelId,
                terminalId, terminalName, businessUserId, wareHouseId,
                model.StartTime, //?? DateTime.Parse(DateTime.Now.ToString("yyyy-MM-dd")),
                model.EndTime //?? DateTime.Now.AddDays(1),
                ).Select(sd =>
                {

                    return sd ?? new SaleReportProductCostProfit();

                }).AsQueryable();

            var items = new PagedList<SaleReportProductCostProfit>(sqlDatas, pagenumber, 30);

            model.Items = items;

            #region 汇总

            if (sqlDatas != null && sqlDatas.ToList().Count > 0)
            {

                //销售数量
                model.TotalSumSaleQuantityConversion = sqlDatas.Sum(a => a.SaleBigQuantity ?? 0) + "大" + sqlDatas.Sum(a => a.SaleStrokeQuantity ?? 0) + "中" + sqlDatas.Sum(a => a.SaleSmallQuantity ?? 0) + "小";
                //销售金额
                model.TotalSumSaleAmount = items.Sum(a => a.SaleAmount ?? 0);
                //退货数量
                model.TotalSumReturnQuantityConversion = sqlDatas.Sum(a => a.ReturnBigQuantity ?? 0) + "大" + sqlDatas.Sum(a => a.ReturnStrokeQuantity ?? 0) + "中" + sqlDatas.Sum(a => a.ReturnSmallQuantity ?? 0) + "小";
                //退货金额
                model.TotalSumReturnAmount = sqlDatas.Sum(a => a.ReturnAmount ?? 0);
                //净销数量
                model.TotalSumNetQuantityConversion = (sqlDatas.Sum(a => a.SaleBigQuantity ?? 0) - sqlDatas.Sum(a => a.ReturnBigQuantity ?? 0)) + "大" +
                                                      (sqlDatas.Sum(a => a.SaleStrokeQuantity ?? 0) - sqlDatas.Sum(a => a.ReturnStrokeQuantity ?? 0)) + "中" +
                                                      (sqlDatas.Sum(a => a.SaleSmallQuantity ?? 0) - sqlDatas.Sum(a => a.ReturnSmallQuantity ?? 0)) + "小";
                //净销金额
                model.TotalSumNetAmount = model.TotalSumSaleAmount - model.TotalSumReturnAmount;
                //成本金额
                model.TotalSumCostAmount = sqlDatas.Sum(a => a.CostAmount ?? 0);
                //利润
                model.TotalSumProfit = sqlDatas.Sum(a => a.Profit ?? 0);
                //销售利润率
                if (model.TotalSumSaleAmount == null || model.TotalSumSaleAmount == 0)
                {
                    model.TotalSumSaleProfitRate = 100;
                }
                else
                {
                    model.TotalSumSaleProfitRate = (model.TotalSumProfit / model.TotalSumSaleAmount) * 100;
                }

                //成本利润率
                if (model.TotalSumCostAmount == null || model.TotalSumCostAmount == 0)
                {
                    model.TotalSumCostProfitRate = 100;
                }
                else
                {
                    model.TotalSumCostProfitRate = (model.TotalSumProfit / model.TotalSumCostAmount) * 100;
                }

            }

            #endregion

            model.PagingFilteringContext.LoadPagedList(items);

            return View(model);
        }

        //销售商品成本利润导出
        [AuthCode((int)AccessGranularityEnum.CostProfitView)]
        public FileResult ExportSaleReportProductCostProfit(int? productId, string productName, int? categoryId, int? brandId, int? channelId, int? terminalId, string terminalName, int? businessUserId, int? wareHouseId, DateTime? startTime, DateTime? endTime)
        {

            #region 查询导出数据
            var sqlDatas = _saleReportService.GetSaleReportProductCostProfit(curStore?.Id ?? 0,
                productId, productName, categoryId, brandId, channelId,
                terminalId, terminalName, businessUserId, wareHouseId, startTime ?? DateTime.Parse(DateTime.Now.ToString("yyyy-MM-01 00:00:00")), endTime ?? DateTime.Now.AddDays(1)
                ).Select(sd =>
                {

                    return sd ?? new SaleReportProductCostProfit();

                }).AsQueryable();

            #endregion

            var datas = new List<SaleReportProductCostProfit>(sqlDatas);

            #region 导出
            var ms = _exportManager.ExportSaleReportProductCostProfitToXlsx(datas);
            if (ms != null)
            {
                return File(ms, "application/vnd.ms-excel", "销售成本利润表.xlsx");
            }
            else
            {
                return File(new MemoryStream(), "application/vnd.ms-excel", "销售成本利润表.xlsx");
            }
            #endregion
        }

        #endregion



        #region 导出

        //费用合同明细表导出
        [AuthCode((int)AccessGranularityEnum.ExpenseDetailsExport)]
        public FileResult ExportSaleReportCostContractItem(int? terminalId, string terminalName, int? productId, string productName, int? bussinessUserId,
            int? accountingOptionId, string billNumber, int? categoryId, int? cashTypeId, string remark,
            int? statusTypeId, DateTime? startTime, DateTime? endTime)
        {

            #region 查询导出数据

            var sqlDatas = _saleReportService.GetSaleReportCostContractItem(curStore?.Id ?? 0,
                terminalId, terminalName, productId, productName, bussinessUserId, accountingOptionId,
                billNumber, categoryId, cashTypeId, remark,
                statusTypeId, startTime ?? DateTime.Parse(DateTime.Now.ToString("yyyy-MM-01 00:00:00")), endTime ?? DateTime.Now.AddDays(1)
                );

            #endregion

            var datas = new List<SaleReportCostContractItem>(sqlDatas);

            #region 导出
            var ms = _exportManager.ExportSaleReportCostContractItemToXlsx(datas);
            if (ms != null)
            {
                return File(ms, "application/vnd.ms-excel", "费用合同明细表.xlsx");
            }
            else
            {
                return File(new MemoryStream(), "application/vnd.ms-excel", "费用合同明细表.xlsx");
            }
            #endregion
        }


        #endregion

        #region 经营报表
        /// <summary>
        /// 经营日报 
        /// </summary>
        /// <param name="businessUserId"></param>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <param name="pagenumber"></param>
        /// <param name="auditedStatus"></param>
        /// <returns></returns>
        [HttpGet]
        //[AuthCode((int)AccessGranularityEnum.GiftSummaryView)]
        public IActionResult SaleReportSummaryBusinessDaily(int? businessUserId, DateTime? startTime, DateTime? endTime,
            int pagenumber = 0, bool? auditedStatus = null)
        {
            var model = new SaleReportSummaryBusinessDailyListModel();
            if (endTime.HasValue) 
            {
                endTime = DateTime.Parse(endTime?.ToString("yyyy-MM-dd 23:59:59"));
            }
            model.StartTime = startTime ?? DateTime.Parse(DateTime.Now.ToString("yyyy-MM-01"));
            model.EndTime = endTime?? DateTime.Parse(DateTime.Now.ToString("yyyy-MM-dd 23:59:59"));
            //业务员
            model.BusinessUsers = BindUserSelection(_userService.BindUserList, curStore, DCMSDefaults.Salesmans, curUser.Id, true, _userService.IsAdmin(curStore.Id, curUser.Id));
            model.BusinessUserId = (businessUserId ?? null);
            var rst = _saleReportService.GetSaleReportSummaryBusinessDaily(curStore.Id, model.StartTime, model.EndTime, businessUserId ?? 0);
            if (rst?.Count > 0)
            {
                var lst = new List<SaleReportSummaryBusinessDaily>();
                var groupDays = rst.GroupBy(g => g.CreatedOnUtc.ToString("yyyy-MM-dd")).ToList();
                groupDays.ForEach(day =>
                {
                    var groupBusiness = rst.Where(w => w.CreatedOnUtc.ToString("yyyy-MM-dd") == day.Key).ToList();
                    groupBusiness.GroupBy(g => g.BusinessUserId).ToList().ForEach(u =>
                    {
                        var entity = new SaleReportSummaryBusinessDaily();
                        entity.DateName = day.Key;
                        entity.BusinessName = _userService.GetUserName(curStore.Id, u.Key);
                        entity.SaleQuantity = groupBusiness.Where(w => w.BusinessUserId == u.Key).Sum(s => s.SaleQuantity);
                        entity.SaleAmount = groupBusiness.Where(w=>w.BusinessUserId == u.Key).Sum(s=>s.SumAmount);
                        entity.GiftQuantity = groupBusiness.Where(w => w.BusinessUserId == u.Key).Sum(s => s.GiftQuantity);
                        entity.GiftCostAmount = groupBusiness.Where(w => w.BusinessUserId == u.Key).Sum(s => s.GiftCostAmount);
                        entity.ReturnQuantity = groupBusiness.Where(w => w.BusinessUserId == u.Key).Sum(s => s.ReturnQuantity);
                        entity.ReturnAmount = groupBusiness.Where(w => w.BusinessUserId == u.Key).Sum(s => s.ReturnAmount);
                        entity.PreferentialAmount = groupBusiness.Where(w => w.BusinessUserId == u.Key).Sum(s => s.PreferentialAmount);
                        entity.ProceedsAmount = groupBusiness.Where(w=>w.BusinessUserId == u.Key).Sum(s=>s.ProceedsAmount);
                        entity.BalanceAmount = groupBusiness.Where(w => w.BusinessUserId == u.Key).Sum(s => s.BalanceAmount);
                        //获取单据收款记录
                        foreach (var item in groupBusiness)
                        {
                            var lst1 = _cashReceiptBillService.GetCashReceiptItemListByBillId(curStore.Id, item.Id);
                            if (lst1 != null && lst1.Count > 0) 
                            {
                                var preferentialAmount = lst1.Sum(s => s.DiscountAmountOnce) ?? 0;
                                var proceedsAmount = lst1.Sum(s => s.ReceivableAmountOnce) ?? 0;
                                if (item.TypeId != 2)
                                {
                                    entity.PreferentialAmount += preferentialAmount;
                                    entity.ProceedsAmount += proceedsAmount;
                                    entity.BalanceAmount = entity.BalanceAmount - preferentialAmount - proceedsAmount;
                                }
                                else 
                                {
                                    entity.BalanceAmount = entity.BalanceAmount + preferentialAmount + proceedsAmount;
                                }
                            }
                        }
                        lst.Add(entity);
                    });
                });
                var items = new PagedList<SaleReportSummaryBusinessDaily>(lst, pagenumber, 30);
                model.PagingFilteringContext.LoadPagedList(items);
                model.Items = items;
            }
            return View(model);
        }
        /// <summary>
        /// 经营月报
        /// </summary>
        /// <param name="businessUserId"></param>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <param name="pagenumber"></param>
        /// <param name="auditedStatus"></param>
        /// <returns></returns>
        [HttpGet]
        //[AuthCode((int)AccessGranularityEnum.GiftSummaryView)]
        public IActionResult SaleReportSummaryBusinessMonthly(int? businessUserId, int year, int month,
            int pagenumber = 0, bool? auditedStatus = null)
        {
            try
            {
                var model = new SaleReportSummaryBusinessMonthlyListModel();
                model.Year = (year == 0) ? DateTime.Now.Year : year;
                model.Month = (month == 0) ? DateTime.Now.Month : month;
                //业务员
                model.StartTime = Convert.ToDateTime($"{model.Year}-{model.Month}-01");
                model.EndTime = Convert.ToDateTime($"{model.Year}-{model.Month}-{DateTime.DaysInMonth(model.Year, model.Month)} 23:59:59");
                model.BusinessUsers = BindUserSelection(_userService.BindUserList, curStore, DCMSDefaults.Salesmans, curUser.Id, true, _userService.IsAdmin(curStore.Id, curUser.Id));
                model.BusinessUserId = (businessUserId ?? null);
                var rst = _saleReportService.GetSaleReportSummaryBusinessDaily(curStore.Id, model.StartTime, model.EndTime, businessUserId ?? 0);
                if (rst?.Count > 0)
                {
                    var lst = new List<SaleReportSummaryBusinessDaily>();
                    var groupDays = rst.GroupBy(g => g.CreatedOnUtc.ToString("yyyy-MM-dd")).ToList();
                    groupDays.ForEach(day =>
                    {
                        var groupBusiness = rst.Where(w => w.CreatedOnUtc.ToString("yyyy-MM-dd") == day.Key).ToList();
                        groupBusiness.GroupBy(g => g.BusinessUserId).ToList().ForEach(u =>
                        {
                            var entity = new SaleReportSummaryBusinessDaily();
                            entity.DateName = day.Key;
                            entity.BusinessName = _userService.GetUserName(curStore.Id, u.Key);
                            entity.SaleQuantity = groupBusiness.Where(w => w.BusinessUserId == u.Key).Sum(s => s.SaleQuantity);
                            entity.SaleAmount = groupBusiness.Where(w => w.BusinessUserId == u.Key).Sum(s => s.SumAmount);
                            entity.GiftQuantity = groupBusiness.Where(w => w.BusinessUserId == u.Key).Sum(s => s.GiftQuantity);
                            entity.GiftCostAmount = groupBusiness.Where(w => w.BusinessUserId == u.Key).Sum(s => s.GiftCostAmount);
                            entity.ReturnQuantity = groupBusiness.Where(w => w.BusinessUserId == u.Key).Sum(s => s.ReturnQuantity);
                            entity.ReturnAmount = groupBusiness.Where(w => w.BusinessUserId == u.Key).Sum(s => s.ReturnAmount);
                            entity.PreferentialAmount = groupBusiness.Where(w => w.BusinessUserId == u.Key).Sum(s => s.PreferentialAmount);
                            entity.ProceedsAmount = groupBusiness.Where(w => w.BusinessUserId == u.Key).Sum(s => s.ProceedsAmount);
                            entity.BalanceAmount = groupBusiness.Where(w => w.BusinessUserId == u.Key).Sum(s => s.BalanceAmount);
                            //获取单据收款记录
                            foreach (var item in groupBusiness)
                            {
                                var lst1 = _cashReceiptBillService.GetCashReceiptItemListByBillId(curStore.Id, item.Id);
                                if (lst1 != null && lst1.Count > 0)
                                {
                                    var preferentialAmount = lst1.Sum(s => s.DiscountAmountOnce) ?? 0;
                                    var proceedsAmount = lst1.Sum(s => s.ReceivableAmountOnce) ?? 0;
                                    if (item.TypeId != 2)
                                    {
                                        entity.PreferentialAmount += preferentialAmount;
                                        entity.ProceedsAmount += proceedsAmount;
                                        entity.BalanceAmount = entity.BalanceAmount - preferentialAmount - proceedsAmount;
                                    }
                                    else
                                    {
                                        entity.BalanceAmount = entity.BalanceAmount + preferentialAmount + proceedsAmount;
                                    }
                                }
                            }
                            lst.Add(entity);
                        });
                    });
                    var items = new PagedList<SaleReportSummaryBusinessDaily>(lst, pagenumber, 30);
                    model.PagingFilteringContext.LoadPagedList(items);
                    model.Items = items;
                }
                return View(model);
            }
            catch (Exception ex)
            {
                return View();
            }
            
        }
        /// <summary>
        /// 经营月报
        /// </summary>
        /// <param name="businessUserId"></param>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <param name="pagenumber"></param>
        /// <param name="auditedStatus"></param>
        /// <returns></returns>
        [HttpGet]
        //[AuthCode((int)AccessGranularityEnum.GiftSummaryView)]
        public IActionResult SaleReportSummaryBusinessYearly(int? businessUserId, int year, int pagenumber = 0, bool? auditedStatus = null)
        {
            try
            {
                var model = new SaleReportSummaryBusinessYearlyListModel();
                model.Year = (year == 0) ? DateTime.Now.Year : year;
                //业务员
                model.BusinessUsers = BindUserSelection(_userService.BindUserList, curStore, DCMSDefaults.Salesmans, curUser.Id, true, _userService.IsAdmin(curStore.Id, curUser.Id));
                model.BusinessUserId = (businessUserId ?? null);
                var rst = _saleReportService.GetSaleReportSummaryBusinessYearly(curStore.Id, model.Year, businessUserId ?? 0);
                if (rst?.Count > 0)
                {
                    var lst = new List<SaleReportSummaryBusinessDaily>();
                    var groupDays = rst.GroupBy(g => g.CreatedOnUtc.ToString("yyyy-MM")).ToList();
                    groupDays.ForEach(day =>
                    {
                        var groupBusiness = rst.Where(w => w.CreatedOnUtc.ToString("yyyy-MM") == day.Key).ToList();
                        groupBusiness.GroupBy(g => g.BusinessUserId).ToList().ForEach(u =>
                        {
                            var entity = new SaleReportSummaryBusinessDaily();
                            entity.DateName = day.Key;
                            entity.BusinessName = _userService.GetUserName(curStore.Id, u.Key);
                            entity.SaleQuantity = groupBusiness.Where(w => w.BusinessUserId == u.Key).Sum(s => s.SaleQuantity);
                            entity.SaleAmount = groupBusiness.Where(w => w.BusinessUserId == u.Key).Sum(s => s.SumAmount);
                            entity.GiftQuantity = groupBusiness.Where(w => w.BusinessUserId == u.Key).Sum(s => s.GiftQuantity);
                            entity.GiftCostAmount = groupBusiness.Where(w => w.BusinessUserId == u.Key).Sum(s => s.GiftCostAmount);
                            entity.ReturnQuantity = groupBusiness.Where(w => w.BusinessUserId == u.Key).Sum(s => s.ReturnQuantity);
                            entity.ReturnAmount = groupBusiness.Where(w => w.BusinessUserId == u.Key).Sum(s => s.ReturnAmount);
                            entity.PreferentialAmount = groupBusiness.Where(w => w.BusinessUserId == u.Key).Sum(s => s.PreferentialAmount);
                            entity.ProceedsAmount = groupBusiness.Where(w => w.BusinessUserId == u.Key).Sum(s => s.ProceedsAmount);
                            entity.BalanceAmount = groupBusiness.Where(w => w.BusinessUserId == u.Key).Sum(s => s.BalanceAmount);
                            //获取单据收款记录
                            foreach (var item in groupBusiness)
                            {
                                var lst1 = _cashReceiptBillService.GetCashReceiptItemListByBillId(curStore.Id, item.Id);
                                if (lst1 != null && lst1.Count > 0)
                                {
                                    var preferentialAmount = lst1.Sum(s => s.DiscountAmountOnce) ?? 0;
                                    var proceedsAmount = lst1.Sum(s => s.ReceivableAmountOnce) ?? 0;
                                    if (item.TypeId != 2)
                                    {
                                        entity.PreferentialAmount += preferentialAmount;
                                        entity.ProceedsAmount += proceedsAmount;
                                        entity.BalanceAmount = entity.BalanceAmount - preferentialAmount - proceedsAmount;
                                    }
                                    else
                                    {
                                        entity.BalanceAmount = entity.BalanceAmount + preferentialAmount + proceedsAmount;
                                    }
                                }
                            }
                            lst.Add(entity);
                        });
                    });
                    var items = new PagedList<SaleReportSummaryBusinessDaily>(lst, pagenumber, 30);
                    model.PagingFilteringContext.LoadPagedList(items);
                    model.Items = items;
                }
                return View(model);
            }
            catch (Exception ex)
            {
                return View();
            }

        }
        #endregion
    }
}