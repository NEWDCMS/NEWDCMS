using DCMS.Core;
using DCMS.Core.Caching;
using DCMS.Core.Domain.Configuration;
using DCMS.Core.Domain.Products;
using DCMS.Core.Domain.Sales;
using DCMS.Core.Domain.Terminals;
using DCMS.Core.Domain.Users;
using DCMS.Core.Domain.WareHouses;
using DCMS.Services.Common;
using DCMS.Services.Configuration;
using DCMS.Services.ExportImport;
using DCMS.Services.Finances;
using DCMS.Services.Logging;
using DCMS.Services.Messages;
using DCMS.Services.Products;
using DCMS.Services.Purchases;
using DCMS.Services.Sales;
using DCMS.Services.Settings;
using DCMS.Services.Terminals;
using DCMS.Services.Users;
using DCMS.Services.WareHouses;
using DCMS.ViewModel.Models.Sales;
using DCMS.Web.Framework.Mvc.Filters;
using DCMS.Web.Infrastructure.Mapper.Extensions;
using DCMS.Web.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Task = System.Threading.Tasks.Task;

namespace DCMS.Web.Controllers
{
    /// <summary>
    /// 销售订单
    /// </summary>
    public class SaleReservationBillController : BasePublicController
    {
        private readonly IPrintTemplateService _printTemplateService;
        private readonly IUserActivityService _userActivityService;
        private readonly ISaleReservationBillService _saleReservationBillService;
        private readonly IDistrictService _districtService;
        private readonly IWareHouseService _wareHouseService;
        private readonly IProductService _productService;
        private readonly IStockService _stockService;
        private readonly ISettingService _settingService;
        private readonly ISpecificationAttributeService _specificationAttributeService;
        private readonly IProductTierPricePlanService _productTierPricePlanService;
        private readonly IBranchService _branchService;
        private readonly IUserService _userService;
        private readonly ITerminalService _terminalService;
        private readonly IAccountingService _accountingService;
        private readonly IMediaService _mediaService;
        private readonly ISaleBillService _saleBillService;
        private readonly IDispatchBillService _dispatchBillService;
        private readonly ICostContractBillService _costContractBillService;
        private readonly IRedLocker _locker;
        private readonly IExportManager _exportManager;
        private readonly ICommonBillService _commonBillService;
        private readonly IPurchaseBillService _purchaseBillService;

        public SaleReservationBillController(
            IWorkContext workContext,
            ILogger loggerService,
            IPrintTemplateService printTemplateService,
            IUserActivityService userActivityService,
            ISaleReservationBillService saleReservationBillService,
            IDistrictService districtService,
            IWareHouseService wareHouseService,
            IProductService productService,
            IStockService stockService,
            ISettingService settingService,
            ISpecificationAttributeService specificationAttributeService,
            IProductTierPricePlanService productTierPricePlanService,
            IBranchService branchService,
            IUserService userService,
            ITerminalService terminalService,
            IAccountingService accountingService,
            IMediaService mediaService,
            ISaleBillService saleBillService,
            IDispatchBillService dispatchBillService,
            ICostContractBillService costContractBillService,
            IStoreContext storeContext,
            INotificationService notificationService,
            IRedLocker locker,
            IExportManager exportManager,
            ICommonBillService commonBillService,
            IPurchaseBillService purchaseBillService
            ) : base(workContext, loggerService, storeContext, notificationService)
        {
            _printTemplateService = printTemplateService;
            _userActivityService = userActivityService;
            _saleReservationBillService = saleReservationBillService;
            _districtService = districtService;
            _wareHouseService = wareHouseService;
            _productService = productService;
            _stockService = stockService;
            _specificationAttributeService = specificationAttributeService;
            _productTierPricePlanService = productTierPricePlanService;
            _settingService = settingService;
            _branchService = branchService;
            _userService = userService;
            _terminalService = terminalService;
            _accountingService = accountingService;
            _mediaService = mediaService;
            _saleBillService = saleBillService;
            _dispatchBillService = dispatchBillService;
            _costContractBillService = costContractBillService;
            _locker = locker;
            _exportManager = exportManager;
            _commonBillService = commonBillService;
            _purchaseBillService = purchaseBillService;
        }

        public IActionResult Index()
        {
            return RedirectToAction("List");
        }

        [AuthCode((int)AccessGranularityEnum.SaleReservationBillListView)]
        public IActionResult List(int? terminalId, string terminalName, int? businessUserId, int? deliveryUserId, string billNumber, int? wareHouseId, string remark, int? districtId, DateTime? startTime = null, DateTime? endTime = null, bool? auditedStatus = null, bool? sortByAuditedTime = null, bool? showReverse = null, bool? showReturn = null, bool? alreadyChange = null, int pagenumber = 0)
        {
            var model = new SaleReservationBillListModel();

            if (pagenumber > 0)
            {
                pagenumber -= 1;
            }

            #region 绑定数据源

            model = PrepareSaleReservationBillListModel(model);

            model.TerminalId = terminalId ?? null;
            model.TerminalName = terminalName;
            model.BusinessUserId = businessUserId ?? null;
            model.DeliveryUserId = deliveryUserId ?? null;
            model.BillNumber = billNumber;
            model.WareHouseId = wareHouseId ?? null;
            model.Remark = remark;
            model.StartTime = startTime ?? DateTime.Parse(DateTime.Now.ToString("yyyy-MM-01"));
            model.EndTime = endTime ?? DateTime.Now.AddDays(1);
            model.DistrictId = districtId ?? null;
            model.AuditedStatus = auditedStatus;
            model.SortByAuditedTime = sortByAuditedTime;
            model.ShowReverse = showReverse;
            model.ShowReturn = showReturn;
            model.AlreadyChange = alreadyChange;

            #endregion

            var saleReservations = _saleReservationBillService.GetSaleReservationBillList(curStore?.Id ?? 0,
                 curUser.Id,
                terminalId,
                terminalName,
                businessUserId,
                deliveryUserId,
                billNumber,
                wareHouseId,
                remark,
                model.StartTime,
                model.EndTime,
                districtId,
                auditedStatus,
                sortByAuditedTime,
                showReverse,
                showReturn,
                alreadyChange,
                false,
                pagenumber,
                pageSize: 30);


            model.PagingFilteringContext.LoadPagedList(saleReservations);

            //默认收款账户动态列
            var defaultAcc = _accountingService.GetDefaultAccounting(curStore?.Id ?? 0, BillTypeEnum.SaleReservationBill);
            model.DynamicColumns = defaultAcc?.Item4?.OrderBy(s => s.Key).Select(s => s.Value).ToList();

            #region 查询需要关联其他表的数据
            List<int> userIds = new List<int>();

            userIds.AddRange(saleReservations.Select(b => b.BusinessUserId).Distinct().ToArray());
            userIds.AddRange(saleReservations.Select(b => b.DeliveryUserId).Distinct().ToArray());

            var allUsers = _userService.GetUsersDictsByIds(curStore.Id, userIds.Distinct().ToArray());

            var allTerminal = _terminalService.GetTerminalsByIds(curStore.Id, saleReservations.Select(b => b.TerminalId).Distinct().ToArray(), true);
            //var allWareHouses = _wareHouseService.GetWareHouseByIds(curStore.Id, saleReservations.Select(b => b.WareHouseId).Distinct().ToArray(), true);


            //商品价格
            List<int> productIds = new List<int>();

            //所有关联销售单
            var allSaleBills = _saleBillService.GetSaleBillsBySaleReservationIds(curStore.Id, saleReservations.Select(b => b.Id).Distinct().ToArray());
            //销售单中的商品
            if (allSaleBills != null && allSaleBills.Count > 0)
            {
                allSaleBills.ToList().ForEach(sale =>
                {
                    if (sale != null && sale.Items != null && sale.Items.Count > 0)
                    {
                        productIds.AddRange(sale.Items.Select(it => it.ProductId).Distinct().ToArray());
                    }
                });
            }
            //销售订单中的商品
            if (saleReservations != null && saleReservations.Count > 0)
            {
                saleReservations.ToList().ForEach(sale =>
                {
                    if (sale != null && sale.Items != null && sale.Items.Count > 0)
                    {
                        productIds.AddRange(sale.Items.Select(it => it.ProductId).Distinct().ToArray());
                    }
                });
            }
            productIds = productIds.Distinct().ToList();
            //商品价格
            var allProductPrices = _productService.GetProductPricesByProductIds(curStore.Id, productIds.ToArray(), true);

            #endregion

            model.Lists = saleReservations.Select(s =>
            {
                var m = s.ToModel<SaleReservationBillModel>();

                //业务员名称
                m.BusinessUserName = allUsers.Where(aw => aw.Key == m.BusinessUserId).Select(aw => aw.Value).FirstOrDefault();
                m.MakeUserName = allUsers.Where(aw => aw.Key == m.MakeUserId).Select(aw => aw.Value).FirstOrDefault();
                //送货员名称
                m.DeliveryUserName = allUsers.Where(aw => aw.Key == m.DeliveryUserId).Select(aw => aw.Value).FirstOrDefault();

                //客户名称
                var terminal = allTerminal.FirstOrDefault(at => at.Id == m.TerminalId);
                m.TerminalName = terminal == null ? "" : terminal.Name;
                m.TerminalPointCode = terminal == null ? "" : terminal.Code;
                //仓库名称
                var warehouse = model.WareHouses.Where(s => s.Value == m.WareHouseId.ToString()).FirstOrDefault();
                //var warehouse = allWareHouses.FirstOrDefault(aw => aw.Id == m.WareHouseId);
                m.WareHouseName = warehouse == null ? "" : warehouse.Text;

                //应收金额	
                m.ReceivableAmount = (s.ReceivableAmount == 0) ? ((s.OweCash != 0) ? s.OweCash : s.SaleReservationBillAccountings.Sum(sa => sa.CollectionAmount)) : s.ReceivableAmount;

                //收款账户(只有允许预收款)
                m.SaleReservationBillAccountings = defaultAcc?.Item4?.OrderBy(sb => sb.Key).Select(sb =>
                {
                    var acc = s.SaleReservationBillAccountings.Where(a => a?.AccountingOption?.ParentId == sb.Key).FirstOrDefault();
                    return new SaleReservationBillAccountingModel()
                    {
                        Name = acc?.AccountingOption?.Name,
                        AccountingOptionId = acc?.AccountingOptionId ?? 0,
                        CollectionAmount = acc?.CollectionAmount ?? 0
                    };
                }).ToList();


                //查询销售订单 关联的销售单
                bool saleChangePrice = false;
                SaleBill saleBill = allSaleBills.FirstOrDefault(ab => ab.SaleReservationBillId == m.Id);
                if (saleBill != null)
                {
                    m.SaleBillId = saleBill.Id;
                    m.SaleBillNumber = saleBill.BillNumber;
                    if (saleBill.Items != null && saleBill.Items.Count > 0)
                    {
                        saleBill.Items.ToList().ForEach(it =>
                        {
                            ProductPrice productPrice = allProductPrices.FirstOrDefault(ap => ap.ProductId == it.ProductId && ap.UnitId == it.UnitId);
                            if (productPrice != null && it.Price != productPrice.TradePrice)
                            {
                                saleChangePrice = true;
                            }
                        });
                    }
                }
                m.SaleChangePrice = saleChangePrice;

                //是否变价
                bool saleReservationChangePrice = false;
                if (s.Items != null && s.Items.Count > 0)
                {
                    s.Items.ToList().ForEach(it =>
                    {
                        ProductPrice productPrice = allProductPrices.FirstOrDefault(ap => ap.ProductId == it.ProductId && ap.UnitId == it.UnitId);
                        if (productPrice != null && it.Price != productPrice.TradePrice)
                        {
                            saleReservationChangePrice = true;
                        }
                    });
                }
                m.SaleReservationChangePrice = saleReservationChangePrice;

                return m;
            }).ToList();

            return View(model);
        }

        /// <summary>
        /// 获取销售订单列表
        /// </summary>
        /// <param name="terminalId">客户</param>
        /// <param name="businessUserId">业务员</param>
        /// <param name="billNumber">单据号</param>
        /// <param name="wareHouseId">仓库</param>
        /// <param name="remark">备注</param>
        /// <param name="start">开始时间</param>
        /// <param name="end">结束时间</param>
        /// <param name="districtId">片区</param>
        /// <param name="auditedStatus">审核状态</param>
        /// <param name="sortByAuditedTime">按审核时间</param>
        /// <param name="showReverse">显示红冲的数据</param>
        /// <param name="showReturn">显示退货单</param>
        /// <param name="paymentMethodType">支付方式</param>
        /// <param name="billSourceType">单据来源</param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        [AuthCode((int)AccessGranularityEnum.SaleReservationBillListView)]
        public async Task<JsonResult> AsyncList(int? terminalId, int? businessUserId, int? deliveryUserId, string billNumber = "", int? wareHouseId = null, string remark = "", DateTime? start = null, DateTime? end = null, int? districtId = null, bool? auditedStatus = null, bool? sortByAuditedTime = null, bool? showReverse = null, bool? showReturn = null, bool? alreadyChange = null, int pageIndex = 0, int pageSize = 20)
        {
            return await Task.Run(() =>
            {
                var gridModel = _saleReservationBillService.GetSaleReservationBillList(curStore?.Id ?? 0,
                     curUser.Id, terminalId, "", businessUserId, deliveryUserId, billNumber, wareHouseId, remark, start, end, districtId, auditedStatus, sortByAuditedTime, showReverse, showReturn, alreadyChange, false, pageIndex, pageSize);

                //默认账户设置
                var defaultAcc = _accountingService.GetDefaultAccounting(curStore?.Id ?? 0, BillTypeEnum.SaleReservationBill);

                return Json(new
                {
                    Success = true,
                    total = gridModel.TotalCount,
                    rows = gridModel.Select(s =>
                    {

                        var m = s.ToModel<SaleReservationBillModel>();

                        //业务员名称
                        m.BusinessUserName = _userService.GetUserName(curStore.Id, m.BusinessUserId ?? 0);
                        //客户名称
                        var terminal = _terminalService.GetTerminalById(curStore.Id, m.TerminalId);
                        m.TerminalName = terminal == null ? "" : terminal.Name;
                        m.TerminalPointCode = terminal == null ? "" : terminal.Code;
                        //仓库名称
                        m.WareHouseName = _wareHouseService.GetWareHouseName(curStore.Id, m.WareHouseId);

                        //应收金额	
                        m.ReceivableAmount = (s.ReceivableAmount == 0) ? ((s.OweCash != 0) ? s.OweCash : s.SaleReservationBillAccountings.Sum(sa => sa.CollectionAmount)) : s.ReceivableAmount;

                        //优惠金额
                        m.PreferentialAmount = s.PreferentialAmount;

                        //收款账户(只有允许预收款)
                        m.SaleReservationBillAccountings = defaultAcc?.Item4?.OrderBy(sb => sb.Key).Select(sb =>
                        {
                            var acc = s.SaleReservationBillAccountings.Where(a => a?.AccountingOption?.ParentId == sb.Key).FirstOrDefault();
                            return new SaleReservationBillAccountingModel()
                            {
                                Name = acc?.AccountingOption?.Name,
                                AccountingOptionId = acc?.AccountingOptionId ?? 0,
                                CollectionAmount = acc?.CollectionAmount ?? 0
                            };
                        }).ToList();

                        //欠款金额	
                        m.OweCash = s.OweCash;

                        return m;

                    }).ToList()
                });

            });
        }

        /// <summary>
        /// 添加销售订单
        /// </summary>
        /// <returns></returns>
        [AuthCode((int)AccessGranularityEnum.SaleReservationBillSave)]
        public IActionResult Create()
        {
            var companySetting = _settingService.LoadSetting<CompanySetting>(curStore.Id);
            var model = new SaleReservationBillModel();

            model = PrepareSaleReservationModel(model);

            //付款方式默认挂账
            model.PayTypeId = (int)SaleReservationBillPayType.OnAccount;

            model.PreferentialAmount = 0;
            model.PreferentialEndAmount = 0;
            model.OweCash = 0;
            model.TransactionDate = DateTime.Now;


            //单号
            model.BillNumber = CommonHelper.GetBillNumber(CommonHelper.GetEnumDescription(BillTypeEnum.SaleReservationBill).Split(',')[1], curStore.Id);
            model.BillBarCode = _mediaService.GenerateBarCodeForBase64(model.BillNumber, 150, 50);
            //制单人
            var mu = _userService.GetUserById(curStore.Id, curUser.Id);
            model.MakeUserName = mu != null ? (mu.UserRealName + " " + DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss")) : "";

            //销售订单默认收款账户：(预收款)24

            //默认账户设置
            var defaultAcc = _accountingService.GetDefaultAccounting(curStore?.Id ?? 0, BillTypeEnum.SaleReservationBill);
            model.SaleReservationBillAccountings.Add(new SaleReservationBillAccountingModel()
            {
                AccountingOptionId = defaultAcc?.Item1?.Id ?? 0,
                CollectionAmount = 0,
                Name = defaultAcc.Item1?.Name,
                AccountCodeTypeId = defaultAcc?.Item1?.AccountCodeTypeId ?? 0
            });



            model.IsShowCreateDate = companySetting.OpenBillMakeDate == 0 ? false : true;
            //商品变价参考
            model.VariablePriceCommodity = companySetting.VariablePriceCommodity;
            //单据合计精度
            model.AccuracyRounding = companySetting.AccuracyRounding;
            //交易日期可选范围
            model.AllowSelectionDateRange = companySetting.AllowSelectionDateRange;
            //允许预收款支付成负数
            model.AllowAdvancePaymentsNegative = companySetting.AllowAdvancePaymentsNegative;
            //显示订单占用库存
            model.APPShowOrderStock = companySetting.APPShowOrderStock;


            model.UserMaxAmount = 0;
            model.UserUsedAmount = 0;
            model.UserAvailableAmount = 0;

            //启用税务功能
            model.EnableTaxRate = companySetting.EnableTaxRate;
            model.TaxRate = companySetting.TaxRate;
            return View(model);
        }

        /// <summary>
        /// 编辑销售订单
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [AuthCode((int)AccessGranularityEnum.SaleReservationBillListView)]
        public IActionResult Edit(int? id)
        {
            //没有值跳转到列表
            if (id == null)
            {
                return RedirectToAction("List");
            }
            var companySetting = _settingService.LoadSetting<CompanySetting>(curStore.Id);
            var model = new SaleReservationBillModel();

            SaleReservationBill saleReservation = _saleReservationBillService.GetSaleReservationBillById(curStore.Id, id.Value, true);

            //没有值跳转到列表
            if (saleReservation == null)
            {
                return RedirectToAction("List");
            }
            if (saleReservation.StoreId != curStore.Id)
            {
                return RedirectToAction("List");
            }

            model = saleReservation.ToModel<SaleReservationBillModel>();
            model.BillBarCode = _mediaService.GenerateBarCodeForBase64(saleReservation.BillNumber, 150, 50);


            //获取默认收款账户
            var defaultAcc = _accountingService.GetDefaultAccounting(curStore?.Id ?? 0, BillTypeEnum.SaleReservationBill);
            model.CollectionAccount = defaultAcc?.Item1?.Id ?? 0;
            model.CollectionAmount = saleReservation.SaleReservationBillAccountings.Where(sa => sa.AccountingOptionId == defaultAcc?.Item1?.Id).Sum(sa => sa.CollectionAmount);
            model.SaleReservationBillAccountings = saleReservation.SaleReservationBillAccountings.Select(s =>
            {
                var m = s.ToAccountModel<SaleReservationBillAccountingModel>();
                m.Name = _accountingService.GetAccountingOptionName(curStore.Id, s.AccountingOptionId);
                return m;
            }).ToList();

            //取单据项目
            model.Items = saleReservation.Items.Select(s => s.ToModel<SaleReservationItemModel>()).ToList();

            //获取客户名称
            var terminal = _terminalService.GetTerminalById(curStore.Id, model.TerminalId);
            model.TerminalName = terminal == null ? "" : terminal.Name;
            model.TerminalPointCode = terminal == null ? "" : terminal.Code;

            #region 绑定数据源

            model = PrepareSaleReservationModel(model);

            //制单人
            //var mu = _userService.GetUserById(curStore.Id, saleReservation.MakeUserId);
            //model.MakeUserName = mu != null ? (mu.UserRealName + " " + saleReservation.CreatedOnUtc.ToString("yyyy/MM/dd HH:mm:ss")) : "";
            var mu = string.Empty;
            if (saleReservation.MakeUserId > 0)
            {
                mu = _userService.GetUserName(curStore.Id, saleReservation.MakeUserId);
            }
            model.MakeUserName = mu + " " + saleReservation.CreatedOnUtc.ToString("yyyy/MM/dd HH:mm:ss");

            //审核人
            //var au = _userService.GetUserById(curStore.Id, saleReservation.AuditedUserId ?? 0);
            //model.AuditedUserName = au != null ? (au.UserRealName + " " + (saleReservation.AuditedDate.HasValue ? saleReservation.AuditedDate.Value.ToString("yyyy/MM/dd HH:mm:ss") : "")) : "";
            var au = string.Empty;
            if (saleReservation.AuditedUserId != null && saleReservation.AuditedUserId > 0)
            {
                au = _userService.GetUserName(curStore.Id, saleReservation.AuditedUserId ?? 0);
            }
            model.AuditedUserName = au + " " + (saleReservation.AuditedDate.HasValue ? saleReservation.AuditedDate.Value.ToString("yyyy/MM/dd HH:mm:ss") : "");

            #endregion

            //是否显示生产日期
            model.IsShowCreateDate = companySetting.OpenBillMakeDate == 0 ? false : true;
            //优惠后金额
            //model.PreferentialEndAmount = model.ReceivableAmount - model.PreferentialAmount;
            model.PreferentialEndAmount = model.SumAmount - model.PreferentialAmount;
            //商品变价参考
            model.VariablePriceCommodity = companySetting.VariablePriceCommodity;
            //单据合计精度
            model.AccuracyRounding = companySetting.AccuracyRounding;
            //允许预收款支付成负数
            model.AllowAdvancePaymentsNegative = companySetting.AllowAdvancePaymentsNegative;
            //显示订单占用库存
            model.APPShowOrderStock = companySetting.APPShowOrderStock;

            ////终端、员工欠款
            //model.TerminalMaxAmount = terminal != null ? (terminal.MaxAmountOwed ?? 0) : 0;
            ////model.TerminalUsedAmount = _commonBillService.GetTerminalBalance(model.StoreId, model.TerminalId);
            //model.TerminalAvailableAmount = (model.TerminalMaxAmount - model.TerminalUsedAmount) < 0 ? 0 : model.TerminalMaxAmount - model.TerminalUsedAmount;

            //model.UserMaxAmount = _userService.GetUserMaxAmountOfArrears(model.BusinessUserId ?? 0);
            //model.UserUsedAmount = _commonBillService.GetUserUsedAmount(model.StoreId, model.BusinessUserId ?? 0);
            //model.UserAvailableAmount = (model.UserMaxAmount - model.UserUsedAmount) < 0 ? 0 : model.UserMaxAmount - model.UserUsedAmount;

            //启用税务功能
            model.EnableTaxRate = companySetting.EnableTaxRate;
            model.TaxRate = companySetting.TaxRate;
            return View(model);
        }

        /// <summary>
        /// 异步获取销售订单项目
        /// </summary>
        /// <param name="saleReservationId"></param>
        /// <returns></returns>
        public JsonResult AsyncSaleReservationItems(int saleReservationBillId)
        {
            try
            {
                var gridModel = _saleReservationBillService.GetSaleReservationItemList(saleReservationBillId);
                var allProductPrices = _productService.GetProductPricesByProductIds(curStore.Id, gridModel.Select(p => p.ProductId).ToArray());
                var allProducts = _productService.GetProductsByIds(curStore.Id, gridModel.Select(pr => pr.ProductId).Distinct().ToArray());
                var allOptions = _specificationAttributeService.GetSpecificationAttributeOptionByIds(curStore.Id, allProducts.GetProductBigStrokeSmallUnitIds());
                var allProductTierPrices = _productService.GetProductTierPriceByProductIds(curStore.Id, gridModel.Select(pr => pr.ProductId).Distinct().ToArray());

                var details = gridModel.Select(o =>
                {
                    var m = o.ToModel<SaleReservationItemModel>();
                    var product = allProducts.Where(ap => ap.Id == m.ProductId).FirstOrDefault();
                    if (product != null)
                    {
                        //这里替换成高级用法
                        m = product.InitBaseModel(m, o.SaleReservationBill?.WareHouseId ?? 0, allOptions, allProductPrices, allProductTierPrices, _productService);

                        //商品信息
                        m.BigUnitId = product.BigUnitId;
                        m.StrokeUnitId = product.StrokeUnitId;
                        m.SmallUnitId = product.SmallUnitId;
                        m.IsManufactureDete = product.IsManufactureDete; //是否开启生产日期功能
                        m.ProductTimes = _productService.GetProductDates(curStore.Id, product.Id, o.SaleReservationBill?.WareHouseId??0);

                        m.SaleProductTypeName = CommonHelper.GetEnumDescription<SaleProductTypeEnum>(m.SaleProductTypeId ?? 0);

                        //税价总计
                        m.TaxPriceAmount = m.Amount;

                        if (o.SaleReservationBill.TaxAmount > 0 && m.TaxRate>0)
                        {
                            //含税价格
                            m.ContainTaxPrice = m.Price;
                            //税额
                            m.TaxPrice = m.Amount - m.Amount / (1 + m.TaxRate / 100);

                            m.Price /= (1 + m.TaxRate / 100);
                            m.Amount /= (1 + m.TaxRate / 100);
                        }

                    }

                    return m;

                }).ToList();

                return Json(new
                {
                    total = details.Count,
                    hidden = details.Count(c => c.TaxRate > 0) > 0,
                    rows = details
                });
            }
            catch (Exception)
            {
                return null;
            }

        }


        /// <summary>
        /// 更新/编辑销售订单项目 V2 (事务处理)
        /// </summary>
        /// <param name="data"></param>
        /// <param name="saleId"></param>
        /// <returns></returns>
        [HttpPost]
        [AuthCode((int)AccessGranularityEnum.SaleReservationBillSave)]
        public async Task<JsonResult> CreateOrUpdate(SaleReservationBillUpdateModel data, int? billId,bool doAudit = true)
        {

            try
            {
                var bill = new SaleReservationBill();

                #region 单据验证

                if (data == null || data.Items == null)
                {
                    return Warning("请录入数据.");
                }

                if (PeriodLocked(DateTime.Now))
                {
                    return Warning("锁账期间,禁止业务操作.");
                }

                if (PeriodClosed(DateTime.Now))
                {
                    return Warning("会计期间已结账,禁止业务操作.");
                }

                if (billId.HasValue && billId.Value != 0)
                {
                    bill = _saleReservationBillService.GetSaleReservationBillById(curStore.Id, billId.Value);

                    //公共单据验证
                    var commonBillChecking = BillChecking<SaleReservationBill, SaleReservationItem>(bill, BillStates.Draft, ((int)AccessGranularityEnum.SaleReservationBillSave).ToString());
                    if (commonBillChecking.Value != null)
                    {
                        return commonBillChecking;
                    }
                }
                #endregion

                #region 验证赠品
                if (data != null && data.Items != null && data.Items.Count > 0)
                {
                    List<SaleReservationItem> items = data.Items.Where(it => it.SaleProductTypeId > 0).Select(it =>
                    {
                        SaleReservationItem saleItem = it.ToEntity<SaleReservationItem>();
                        return saleItem;
                    }).ToList();

                    if (!_costContractBillService.CheckGift(curStore.Id, data.TerminalId, items, out string errMsg2))
                    {
                        return Warning(errMsg2);
                    }
                }

                #endregion

                #region 预收款 验证
                //预收款 验证
                if (data.Accounting != null)
                {
                    //剩余预收款金额(预收账款科目下的所有子类科目)：
                    //var advanceAmountBalance = _commonBillService.CalcTerminalBalance(curStore.Id, data.TerminalId);
                    if (data.AdvanceAmount > 0 && data.AdvanceAmount > data.AdvanceAmountBalance)
                    {
                        return this.Warning("预收款余额不足!");
                    }
                }
                #endregion

                #region 验证盘点

                if (data.Items.Any())
                {
                    if (_wareHouseService.CheckProductInventory(curStore.Id, data.WareHouseId, bill.Items?.Select(it => it.ProductId).Distinct().ToArray(), out string thisMsg))
                    {
                        return Warning(thisMsg);
                    }
                }

                #endregion

                #region 验证库存

                IList<Product> allProducts = new List<Product>();
                string errMsg = "";
               
                //当前数据
                List<ProductStockItem> productStockItemNews = new List<ProductStockItem>();
                if (data.Items != null && data.Items.Count > 0)
                {
                    allProducts = _productService.GetProductsByIds(curStore.Id, data.Items.Select(pr => pr.ProductId).Distinct().ToArray());
                    foreach (SaleReservationItemModel item in data.Items)
                    {
                        if (item.ProductId != 0)
                        {
                            var product = allProducts.Where(ap => ap.Id == item.ProductId).FirstOrDefault();
                            if (product != null)
                            {
                                ProductStockItem productStockItem = productStockItemNews.Where(a => a.ProductId == item.ProductId).FirstOrDefault();
                                //商品转化量
                                var conversionQuantity = product.GetConversionQuantity(item.UnitId, _specificationAttributeService, _productService);
                                //库存量增量 = 单位转化量 * 数量
                                int thisQuantity = item.Quantity * conversionQuantity;
                                if (productStockItem != null)
                                {
                                    productStockItem.Quantity += thisQuantity;
                                }
                                else
                                {
                                    productStockItem = new ProductStockItem
                                    {
                                        ProductId = item.ProductId,
                                        //当期选择单位
                                        UnitId = product.SmallUnitId,
                                        //转化单位
                                        SmallUnitId = product.SmallUnitId,
                                        //转化单位
                                        BigUnitId = product.BigUnitId ?? 0,
                                        ProductName = allProducts.Where(s => s.Id == item.ProductId).FirstOrDefault()?.Name,
                                        ProductCode = allProducts.Where(s => s.Id == item.ProductId).FirstOrDefault()?.ProductCode,
                                        Quantity = thisQuantity
                                    };
                                    productStockItemNews.Add(productStockItem);
                                }
                            }
                        }
                    }
                }

                /*
               //验证库存
               if (!_stockService.CheckStockQty(_productService, _specificationAttributeService, curStore.Id, data.WareHouseId, productStockItemNews, out string errMsg))
               {
                   return Warning(errMsg);
               }
               */
                #endregion

                #region 验证0元开单备注
                if (data != null && data.Items != null && data.Items.Count > 0)
                {
                    var items = data.Items.Where(it => it.ProductId > 0);
                    if (items != null && items.Count() > 0)
                    {
                        items.ToList().ForEach(it =>
                        {
                            if (it.Price == 0 && string.IsNullOrEmpty(it.Remark))
                            {
                                errMsg += $"0元开单，必须选择备注.";
                            }
                        });
                    }
                }
                if (!string.IsNullOrEmpty(errMsg))
                {
                    return Warning(errMsg);
                }
                #endregion

                //业务逻辑
                var accountings = _accountingService.GetAllAccountingOptions(curStore.Id, 0, true);
                var dataTo = data.ToEntity<SaleReservationBillUpdate>();
                dataTo.Operation = (int)OperationEnum.PC;
                if (data.Accounting == null)
                {
                    return Warning("没有默认的付款账号");
                }
                dataTo.Accounting = data.Accounting.Select(ac =>
                {
                    return ac.ToAccountEntity<SaleReservationBillAccounting>();
                }).ToList();
                dataTo.Items = data.Items.Select(it =>
                {
                    //成本价（此处计算成本价防止web、api成本价未带出,web、api的controller都要单独计算（取消service计算，防止其他service都引用 pruchasebillservice））
                    var item = it.ToEntity<SaleReservationItem>();
                    item.CostPrice = _purchaseBillService.GetReferenceCostPrice(curStore.Id, item.ProductId, item.UnitId);
                    item.CostAmount = item.CostPrice * item.Quantity;
                    return item;
                }).ToList();

                //RedLock
                var result = await _locker.PerformActionWithLockAsync(LockKey(data),
                    TimeSpan.FromSeconds(30),
                    TimeSpan.FromSeconds(10),
                    TimeSpan.FromSeconds(1),
                    () => _saleReservationBillService.BillCreateOrUpdate(curStore.Id,
                    curUser.Id,
                    billId,
                    bill,
                    dataTo.Accounting,
                    accountings,
                    dataTo,
                    dataTo.Items,
                    productStockItemNews,
                    _userService.IsAdmin(curStore.Id, curUser.Id), doAudit));
                return Json(result);

            }
            catch (Exception ex)
            {
                //活动日志
                _userActivityService.InsertActivity("CreateOrUpdate", Resources.Bill_CreateOrUpdateFailed, curUser.Id);
                _notificationService.ErrorNotification(Resources.Bill_CreateOrUpdateFailed);
                return Error(ex.Message);
            }

        }


        /// <summary>
        /// 审核
        /// </summary>
        /// <returns></returns>
        [AuthCode((int)AccessGranularityEnum.SaleReservationBillApproved)]
        public async Task<JsonResult> Auditing(int? id)
        {
            try
            {
                var bill = new SaleReservationBill();

                #region 验证

                if (!id.HasValue)
                {
                    return Warning("参数错误.");
                }
                else
                {
                    bill = _saleReservationBillService.GetSaleReservationBillById(curStore.Id, id.Value, true);
                    if (bill.AuditedStatus)
                    {
                        return Warning("单据已审核，请刷新页面.");
                    }
                }

                //公共单据验证
                var commonBillChecking = BillChecking<SaleReservationBill, SaleReservationItem>(bill, BillStates.Audited, ((int)AccessGranularityEnum.SaleReservationBillApproved).ToString());
                if (commonBillChecking.Value != null)
                {
                    return commonBillChecking;
                }

                if (_wareHouseService.CheckProductInventory(curStore.Id, bill.WareHouseId, bill.Items?.Select(it => it.ProductId).Distinct().ToArray(), out string thisMsg))
                {
                    return Warning("仓库正在盘点中，拒绝操作.");
                }


                #region 验证赠品

                var items = bill.Items.Where(it => it.SaleProductTypeId > 0).ToList();
                if (!_costContractBillService.CheckGift(curStore.Id, bill.TerminalId, items, out string errMsg))
                {
                    return Warning(errMsg);
                }

                #endregion

                #region 预收款 验证

                //预收款 验证
                if (bill.SaleReservationBillAccountings != null)
                {
                    //1.获取当前经销商 预收款科目Id
                    int accountingOptionId = 0;
                    AccountingOption accountingOption = _accountingService.GetAccountingOptionByAccountCodeTypeId(curStore.Id, (int)AccountingCodeEnum.AdvancesReceived);
                    if (accountingOption != null)
                    {
                        accountingOptionId = (accountingOption == null) ? 0 : accountingOption.Id;
                    }
                    //获取用户输入 预收款金额
                    var advancePaymentReceipt = bill.SaleReservationBillAccountings.Where(ac => ac.AccountingOptionId == accountingOptionId).Sum(ac => ac.CollectionAmount);

                    //用户可用 预收款金额
                    var useAdvanceReceiptAmount = _commonBillService.CalcTerminalBalance(curStore.Id, bill.TerminalId);
                    //如果输入预收款大于用户可用预收款
                    if (advancePaymentReceipt > 0 && advancePaymentReceipt > useAdvanceReceiptAmount.AdvanceAmountBalance)
                    {
                        return Warning("预收款余额不足!");
                    }
                }
                #endregion

                #region 验证库存
                IList<Product> allProducts = new List<Product>();

                //当前数据
                List<ProductStockItem> productStockItemNews = new List<ProductStockItem>();
                if (bill.Items != null && bill.Items.Count > 0)
                {
                    allProducts = _productService.GetProductsByIds(curStore.Id, bill.Items.Select(pr => pr.ProductId).Distinct().ToArray());
                    var allOptions = _specificationAttributeService.GetSpecificationAttributeOptionByIds(curStore.Id, allProducts.GetProductBigStrokeSmallUnitIds());

                    foreach (SaleReservationItem item in bill.Items)
                    {
                        if (item.ProductId != 0)
                        {
                            var product = allProducts.Where(ap => ap.Id == item.ProductId).FirstOrDefault();
                            if (product != null)
                            {
                                ProductStockItem productStockItem = productStockItemNews.Where(a => a.ProductId == item.ProductId).FirstOrDefault();
                                //商品转化量
                                var conversionQuantity = product.GetConversionQuantity(allOptions, item.UnitId);
                                //库存量增量 = 单位转化量 * 数量
                                int thisQuantity = item.Quantity * conversionQuantity;
                                if (productStockItem != null)
                                {
                                    productStockItem.Quantity += thisQuantity;
                                }
                                else
                                {
                                    productStockItem = new ProductStockItem
                                    {
                                        ProductId = item.ProductId,
                                        //当期选择单位
                                        UnitId = product.SmallUnitId,
                                        //转化单位
                                        SmallUnitId = product.SmallUnitId,
                                        //转化单位
                                        BigUnitId = product.BigUnitId ?? 0,
                                        ProductName = allProducts.Where(s => s.Id == item.ProductId).FirstOrDefault()?.Name,
                                        ProductCode = allProducts.Where(s => s.Id == item.ProductId).FirstOrDefault()?.ProductCode,
                                        Quantity = thisQuantity
                                    };

                                    productStockItemNews.Add(productStockItem);
                                }
                            }
                        }

                    }
                }

                //验证库存
                if (!_stockService.CheckStockQty(_productService, _specificationAttributeService, curStore.Id, bill.WareHouseId, productStockItemNews, out string errMsg2))
                {
                    return Warning(errMsg2);
                }

                #endregion

                #endregion

                //RedLock
                var result = await _locker.PerformActionWithLockAsync(RedLockKey(bill),
                      TimeSpan.FromSeconds(30),
                      TimeSpan.FromSeconds(10),
                      TimeSpan.FromSeconds(1),
                      () => _saleReservationBillService.Auditing(curStore.Id, curUser.Id, bill));
                return Json(result);

            }
            catch (Exception ex)
            {
                //活动日志
                _userActivityService.InsertActivity("Auditing", "单据审核失败", curUser.Id);
                _notificationService.SuccessNotification("单据审核失败");
                return Error(ex.Message);
            }

        }

        /// <summary>
        /// 红冲
        /// </summary>
        /// <returns></returns>
        [AuthCode((int)AccessGranularityEnum.SaleReservationBillReverse)]
        public async Task<JsonResult> Reverse(int? id)
        {
            try
            {
                var bill = new SaleReservationBill() { StoreId = curStore?.Id ?? 0 };

                if (PeriodClosed(DateTime.Now))
                {
                    return Warning("系统当月已经结转，不允许红冲");
                }

                if (!id.HasValue)
                {
                    return Warning("参数错误.");
                }
                else
                {
                    bill = _saleReservationBillService.GetSaleReservationBillById(curStore.Id, id.Value, true);
                    if (bill.ReversedStatus)
                    {
                        return Warning("单据已红冲，请刷新页面.");
                    }
                }

                //公共单据验证
                var commonBillChecking = BillChecking<SaleReservationBill, SaleReservationItem>(bill, BillStates.Reversed, ((int)AccessGranularityEnum.SaleReservationBillReverse).ToString());
                if (commonBillChecking.Value != null)
                {
                    return commonBillChecking;
                }

                if (_wareHouseService.CheckProductInventory(curStore.Id, bill.WareHouseId, bill.Items?.Select(it => it.ProductId).Distinct().ToArray(), out string thisMsg))
                {
                    return Warning("仓库正在盘点中，拒绝操作.");
                }

                if (bill.ChangedStatus == true)
                {
                    var saleBill = _saleBillService.GetSaleBillBySaleReservationBillId(curStore.Id, bill.Id);
                    if(saleBill.ReversedStatus==false)
                        return Warning("订单已转为销售单，不能红冲.");
                }

                #region 验证送货签收

                if (_dispatchBillService.CheckSign(bill))
                {
                    return Warning("当前销售订单已经签收，不能红冲.");
                }

                #endregion

                #region 验证流程下个节点是否已红冲或不存在
                if (!CheckNextNodeReversed(curStore, bill.Id, BillTypeEnum.SaleReservationBill))
                {
                    return Warning("订单已装车度，相关调度单未红冲，本订单不能红冲.");
                }
                #endregion

                //RedLock
                var result = await _locker.PerformActionWithLockAsync(RedLockKey(bill),
                      TimeSpan.FromSeconds(30),
                      TimeSpan.FromSeconds(10),
                      TimeSpan.FromSeconds(1),
                      () => _saleReservationBillService.Reverse(curUser.Id, bill));
                return Json(result);

            }
            catch (Exception ex)
            {
                //活动日志
                _userActivityService.InsertActivity("Reverse", "单据红冲失败", curUser.Id);
                _notificationService.SuccessNotification("单据红冲失败");
                return Error(ex.Message);
            }
        }

        /// 作废
        /// </summary>
        /// <returns></returns>
        [AuthCode((int)AccessGranularityEnum.SaleReservationBillReverse)]
        public JsonResult Delete(int? id)
        {
            try
            {
                var bill = new SaleReservationBill() { StoreId = curStore?.Id ?? 0 };

                #region 验证
                if (!id.HasValue)
                {
                    return Warning("参数错误.");
                }
                else
                {
                    bill = _saleReservationBillService.GetSaleReservationBillById(curStore.Id, id.Value, true);
                    if (bill.AuditedStatus || bill.ReversedStatus)
                    {
                        return Warning("单据已审核或红冲，不能作废.");
                    }
                    if (bill.Deleted)
                    {
                        return Warning("单据已作废，请刷新页面.");
                    }
                    if (bill != null)
                    {
                        var rs = _saleReservationBillService.Delete(curUser.Id, bill);
                        if (!rs.Success)
                        {
                            return Error(rs.Message);
                        }
                    }
                }
                #endregion
                return Successful("作废成功");

            }
            catch (Exception ex)
            {
                //活动日志
                _userActivityService.InsertActivity("Delete", "单据作废失败", curUser.Id);
                _notificationService.SuccessNotification("单据作废失败");
                return Error(ex.Message);
            }
        }

        /// <summary>
        /// 导出
        /// </summary>
        /// <param name="terminalId"></param>
        /// <param name="businessUserId"></param>
        /// <param name="billNumber"></param>
        /// <param name="wareHouseId"></param>
        /// <param name="remark"></param>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <param name="districtId"></param>
        /// <param name="auditedStatus"></param>
        /// <param name="sortByAuditedTime"></param>
        /// <param name="showReverse"></param>
        /// <param name="showReturn"></param>
        /// <param name="alreadyChange"></param>
        /// <returns></returns>
        [HttpGet]
        [AuthCode((int)AccessGranularityEnum.SaleReservationBillExport)]
        public FileResult Export(int type, string selectData, int? terminalId, string terminalName, int? businessUserId, int? deliveryUserId, string billNumber = "", int? wareHouseId = null, string remark = "", DateTime? startTime = null, DateTime? endTime = null, int? districtId = null, bool? auditedStatus = null, bool? sortByAuditedTime = null, bool? showReverse = null, bool? showReturn = null, bool? alreadyChange = false)
        {

            #region 查询导出数据

            IList<SaleReservationBill> saleReservationBills = new List<SaleReservationBill>();
            if (type == 1)
            {
                if (!string.IsNullOrEmpty(selectData))
                {
                    List<string> ids = selectData.Split(',').ToList();
                    foreach (var id in ids)
                    {
                        SaleReservationBill saleReservationBill = _saleReservationBillService.GetSaleReservationBillById(curStore.Id, int.Parse(id));
                        if (saleReservationBill != null)
                        {
                            saleReservationBills.Add(saleReservationBill);
                        }
                    }
                }

            }
            else if (type == 2)
            {
                saleReservationBills = _saleReservationBillService.GetSaleReservationBillList(curStore?.Id ?? 0,
                     curUser.Id,
                terminalId,
                terminalName,
                businessUserId,
                deliveryUserId,
                billNumber,
                wareHouseId,
                remark,
                startTime,
                endTime,
                districtId,
                auditedStatus,
                sortByAuditedTime,
                showReverse,
                showReturn,
                alreadyChange,
                false,
                0);
            }

            #endregion

            #region 导出
            var ms = _exportManager.ExportSaleReservationBillToXlsx(saleReservationBills, curStore.Id);
            if (ms != null)
            {
                return File(ms, "application/vnd.ms-excel", "销售订单.xlsx");
            }
            else
            {
                return File(new MemoryStream(), "application/vnd.ms-excel", "销售订单.xlsx");
            }
            #endregion
        }

        [AuthCode((int)AccessGranularityEnum.SaleReservationPrint)]
        public JsonResult PrintSetting()
        {
            var printTemplates = _printTemplateService.GetAllPrintTemplates(curStore.Id).ToList();
            var printTemplate = printTemplates.Where(a => a.BillType == (int)BillTypeEnum.SaleReservationBill).FirstOrDefault();
            //获取打印设置
            var pCPrintSetting = _settingService.LoadSetting<PCPrintSetting>(_storeContext.CurrentStore.Id);
            var settings = new object();
            if (pCPrintSetting != null)
            {
                settings = new
                {
                    PaperWidth = (printTemplate?.PaperWidth == 0 || printTemplate?.PaperHeight == 0) ? pCPrintSetting.PaperWidth : printTemplate.PaperWidth,
                    PaperHeight = (printTemplate?.PaperWidth == 0 || printTemplate?.PaperHeight == 0) ? pCPrintSetting.PaperHeight : printTemplate.PaperHeight,
                    BorderType = pCPrintSetting.BorderType,
                    MarginTop = pCPrintSetting.MarginTop,
                    MarginBottom = pCPrintSetting.MarginBottom,
                    MarginLeft = pCPrintSetting.MarginLeft,
                    MarginRight = pCPrintSetting.MarginRight,
                    IsPrintPageNumber = pCPrintSetting.IsPrintPageNumber,
                    PrintHeader = pCPrintSetting.PrintHeader,
                    PrintFooter = pCPrintSetting.PrintFooter,
                    FixedRowNumber = pCPrintSetting.FixedRowNumber,
                    PrintSubtotal = pCPrintSetting.PrintSubtotal,
                    PrintPort = pCPrintSetting.PrintPort
                };
                return Successful("", settings);
            }
            return Successful("", null);

        }
        /// <summary>
        /// 打印
        /// </summary>
        /// <param name="type">1;打印选择，2 打印全部</param>
        /// <param name="selectData"></param>
        /// <returns></returns>
        [AuthCode((int)AccessGranularityEnum.SaleReservationPrint)]
        public JsonResult Print(int? type, string selectData, int? terminalId, string terminalName, int? businessUserId, int? deliveryUserId, string billNumber = "", int? wareHouseId = null, string remark = "", DateTime? startTime = null, DateTime? endTime = null, int? districtId = null, bool? auditedStatus = null, bool? sortByAuditedTime = null, bool? showReverse = null, bool? showReturn = null, bool? alreadyChange = false)
        {
            try
            {
                bool fg = true;
                string errMsg = string.Empty;

                #region 查询打印数据

                var bills = new List<SaleReservationBill>();
                var datas = new List<string>();
                List<string> ids = selectData.Split(',').ToList();
                //默认选择
                type ??= 1;
                if (type == 1)
                {
                    foreach (var id in ids)
                    {
                        var bill = _saleReservationBillService.GetSaleReservationBillById(curStore.Id, int.Parse(id), true);
                        if (bill != null)
                        {
                            bills.Add(bill);
                        }
                    }
                }
                else if (type == 2)
                {
                    bills = _saleReservationBillService.GetSaleReservationBillList(curStore?.Id ?? 0,
                         curUser.Id,
                            terminalId,
                            terminalName,
                            businessUserId,
                            deliveryUserId,
                            billNumber,
                            wareHouseId,
                            remark,
                            startTime,
                            endTime,
                            districtId,
                            auditedStatus,
                            sortByAuditedTime,
                            showReverse,
                            showReturn,
                            alreadyChange,
                            false,
                            0).ToList();
                }

                #endregion

                #region 修改数据
                if (bills != null && bills.Count > 0)
                {
                    #region 修改单据表打印数
                    foreach (var d in bills)
                    {
                        //获取默认收款账户
                        d.SaleReservationBillAccountings.Select(s =>
                        {
                            s.AccountingOptionName = _accountingService.GetAccountingOptionName(curStore.Id, s.AccountingOptionId);
                            return s;
                        }).ToList();

                        d.PrintNum += 1;
                        _saleReservationBillService.UpdateSaleReservationBill(d);
                    }
                    #endregion
                }

                //获取打印模板
                var printTemplates = _printTemplateService.GetAllPrintTemplates(curStore.Id).ToList();
                var content = printTemplates.Where(a => a.BillType == (int)BillTypeEnum.SaleReservationBill).Select(a => a.Content).FirstOrDefault();

                //获取打印设置
                var pCPrintSetting = _settingService.LoadSetting<PCPrintSetting>(_storeContext.CurrentStore.Id);

                //填充打印数据
                foreach (var d in bills)
                {

                    StringBuilder sb = new StringBuilder();
                    sb.Append(content);

                    #region theadid

                    if (pCPrintSetting != null)
                    {
                        sb.Replace("@商铺名称", string.IsNullOrWhiteSpace(pCPrintSetting.StoreName) ? "&nbsp;": pCPrintSetting.StoreName);
                    }

                    var terminal = _terminalService.GetTerminalById(curStore.Id, d.TerminalId);
                    if (terminal != null)
                    {
                        sb.Replace("@客户名称", terminal.Name);
                        sb.Replace("@客户电话", terminal.BossCall);
                        sb.Replace("@老板姓名", terminal.BossName);
                        sb.Replace("@客户地址", terminal.Address);
                    }
                    sb.Replace("@单据编号", d.BillNumber);
                    User makeUser = _userService.GetUserById(curStore.Id, d.MakeUserId);
                    if (makeUser != null)
                    {
                        sb.Replace("@制单", makeUser.UserRealName);
                    }

                    sb.Replace("@日期", d.TransactionDate == null ? "" : ((DateTime)d.TransactionDate).ToString("yyyy/MM/dd HH:mm:ss"));
                    sb.Replace("@交易日期", d.TransactionDate == null ? "" : ((DateTime)d.TransactionDate).ToString("yyyy/MM/dd HH:mm:ss"));
                    sb.Replace("@打印日期", DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss"));
                    User businessUser = _userService.GetUserById(curStore.Id, d.BusinessUserId);
                    if (businessUser != null)
                    {
                        sb.Replace("@业务员", businessUser.UserRealName);
                        sb.Replace("@业务电话", businessUser.MobileNumber);
                    }
                    WareHouse wareHouse = _wareHouseService.GetWareHouseById(curStore.Id, d.WareHouseId);
                    if (wareHouse != null)
                    {
                        sb.Replace("@仓库", wareHouse.Name);
                        sb.Replace("@库管员", "***");
                    }
                    #endregion

                    #region tbodyid
                    //明细
                    //获取 tbody 中的行
                    int beginTbody = sb.ToString().IndexOf(@"<tbody id=""tbody"">") + @"<tbody id=""tbody"">".Length;
                    if (beginTbody == 17)
                    {
                        beginTbody = sb.ToString().IndexOf(@"<tbody id='tbody'>") + @"<tbody id='tbody'>".Length;
                    }
                    int endTbody = sb.ToString().IndexOf("</tbody>", beginTbody);
                    string tbodytr = sb.ToString()[beginTbody..endTbody];
                    var sumItem1 = 0;
                    var sumItem2 = 0;
                    var sumItem3 = 0;

                    if (d.Items != null && d.Items.Count > 0)
                    {
                        //1.先删除明细第一行
                        sb.Remove(beginTbody, endTbody - beginTbody);
                        int i = 0;
                        var allProducts = _productService.GetProductsByIds(curStore.Id, d.Items.Select(pr => pr.ProductId).Distinct().ToArray());
                        var allOptions = _specificationAttributeService.GetSpecificationAttributeOptionByIds(curStore.Id, allProducts.GetProductBigStrokeSmallUnitIds());
                        foreach (var item in d.Items)
                        {
                            int index = sb.ToString().IndexOf("</tbody>", beginTbody);
                            i++;
                            StringBuilder sb2 = new StringBuilder();
                            sb2.Append(tbodytr);
                            sb2.Replace("#序号", i.ToString());
                            var product = allProducts.Where(ap => ap.Id == item.ProductId).FirstOrDefault();
                            if (product != null)
                            {
                                sb2.Replace("#商品名称", product.Name);
                                ProductUnitOption productUnitOption = product.GetProductUnit(_specificationAttributeService, _productService);
                                if (item.UnitId == product.SmallUnitId)
                                {
                                    sb2.Replace("#条形码", product.SmallBarCode);
                                    if (productUnitOption != null && productUnitOption.smallOption != null)
                                    {
                                        sb2.Replace("#商品单位", productUnitOption.smallOption.Name);
                                    }
                                }
                                else if (item.UnitId == product.StrokeUnitId)
                                {
                                    sb2.Replace("#条形码", product.StrokeBarCode);
                                    if (productUnitOption != null && productUnitOption.strokOption != null)
                                    {
                                        sb2.Replace("#商品单位", productUnitOption.strokOption.Name);
                                    }
                                }
                                else if (item.UnitId == product.BigUnitId)
                                {
                                    sb2.Replace("#条形码", product.BigBarCode);
                                    if (productUnitOption != null && productUnitOption.bigOption != null)
                                    {
                                        sb2.Replace("#商品单位", productUnitOption.bigOption.Name);
                                    }
                                }
                                sb2.Replace("#单位换算", product.GetProductUnitConversion(allOptions));
                                sb2.Replace("#保质期", (product.ExpirationDays ?? 0).ToString());
                                int conversionQuantity = product.GetConversionQuantity(item.UnitId, _specificationAttributeService, _productService);
                                var qty = Pexts.StockQuantityFormat(conversionQuantity * item.Quantity, product.StrokeQuantity ?? 0, product.BigQuantity ?? 0);
                                sb2.Replace("#辅助数量", qty.Item1 + "大" + qty.Item2 + "中" + qty.Item3 + "小");
                                sumItem1 += qty.Item1;
                                sumItem2 += qty.Item2;
                                sumItem3 += qty.Item3;
                            }
                            sb2.Replace("#数量", item.Quantity.ToString());
                            sb2.Replace("#价格", item.Price.ToString("0.00"));
                            sb2.Replace("#金额", item.Amount.ToString("0.00"));
                            sb2.Replace("#备注", item.Remark);

                            sb.Insert(index, sb2);
                        }

                        sb.Replace("辅助数量:###", sumItem1 + "大" + sumItem2 + "中" + sumItem3 + "小");
                        sb.Replace("数量:###", d.Items.Sum(s => s.Quantity).ToString());
                        sb.Replace("金额:###", d.Items.Sum(s => s.Amount).ToString("0.00"));
                    }
                    #endregion

                    #region tfootid

                    sb.Replace("@公司地址", pCPrintSetting?.Address);
                    sb.Replace("@订货电话", pCPrintSetting?.PlaceOrderTelphone);

                    //收/付款方式
                    var accounts = new StringBuilder();
                    foreach (var acc in d?.SaleReservationBillAccountings)
                    {
                        accounts.Append($"{acc?.AccountingOptionName}[{acc?.CollectionAmount ?? 0}]&nbsp;&nbsp;");
                    }
                    sb.Replace("@收付款方式", accounts.ToString());
                    #endregion

                    datas.Add(sb.ToString());
                }

                if (fg)
                {
                    return Successful("打印成功", datas);
                }
                else
                {
                    return Warning(errMsg);
                }
                #endregion

            }
            catch (Exception ex)
            {
                return Warning(ex.ToString());
            }
        }


        [NonAction]
        private SaleReservationBillListModel PrepareSaleReservationBillListModel(SaleReservationBillListModel model)
        {

            var isAdmin = _userService.IsAdmin(curStore.Id, curUser.Id);

            //默认收款账户动态列
            var alls = _accountingService.GetAllAccounts(curStore?.Id ?? 0);
            var defaultAccounting = _accountingService
                .GetDefaultAccounting(curStore?.Id ?? 0, BillTypeEnum.SaleReservationBill, 0, alls?.ToList()).FirstOrDefault();
            model.DynamicColumns.Add(defaultAccounting.Name);

            //业务员
            model.BusinessUsers = BindUserSelection(_userService.BindUserList, curStore, DCMSDefaults.Salesmans, curUser.Id,true, isAdmin);

            //仓库
            model.WareHouses = BindWareHouseSelection(_wareHouseService.BindWareHouseList, curStore, WHAEnum.SaleReservationBill, curUser.Id, 0);

            //送货员
            model.DeliveryUsers = BindUserSelection(_userService.BindUserList, curStore, DCMSDefaults.Delivers, curUser.Id, true, isAdmin);


            //部门
            model = BindDropDownList<SaleReservationBillListModel>(model, _branchService.BindBranchsByParentId, curStore?.Id ?? 0, 0);

            //片区
            model.Districts = BindDistrictSelection(_districtService.BindDistrictList, curStore);

            return model;
        }

        [NonAction]
        private SaleReservationBillModel PrepareSaleReservationModel(SaleReservationBillModel model)
        {
            model.BillTypeEnumId = (int)BillTypeEnum.SaleReservationBill;

            //业务员
            model.BusinessUsers = BindUserSelection(_userService.BindUserList, curStore, DCMSDefaults.Salesmans, curUser.Id,true, _userService.IsAdmin(curStore.Id, curUser.Id));

            //当前用户为业务员时默认绑定
            if (curUser.IsSalesman() && !(model.Id  > 0))
            {
                model.BusinessUserId = curUser.Id;
            }
            else
            {
                model.BusinessUserId = model.BusinessUserId == 0 ? null : model.BusinessUserId;
            }

            //仓库
            model.WareHouses = BindWareHouseSelection(_wareHouseService.BindWareHouseList, curStore, WHAEnum.SaleReservationBill, curUser.Id, model.WareHouseId);


            //默认售价类型 
            model.SaleReservationBillDefaultAmounts = BindPricePlanSelection(_productTierPricePlanService.GetAllPricePlan, curStore);
            //默认
            model.DefaultAmountId = "0_5";
            var companySetting = _settingService.LoadSetting<CompanySetting>(curStore.Id);
            if (companySetting != null)
            {
                if (!string.IsNullOrEmpty(companySetting.DefaultPricePlan))
                {
                    //分析配置 格式："{PricesPlanId}_{PriceTypeId}"
                    var settingDefault = model.SaleReservationBillDefaultAmounts?.Where(s => s.Value.EndsWith(companySetting.DefaultPricePlan.ToString())).FirstOrDefault();
                    //这里取默认（不选择时），否启手动下拉选择
                    model.DefaultAmountId = settingDefault?.Value; //如：0_0
                }
            }

            //用户可用 预收款金额
            decimal useAdvanceReceiptAmount = 0;
            //增加条件，避免不必要查询
            if (curStore.Id > 0 && model.TerminalId > 0)
            {
                useAdvanceReceiptAmount = _commonBillService.CalcTerminalBalance(curStore.Id, model.TerminalId).AdvanceAmountBalance;
            }

            //预收款
            model.AdvanceReceiptAmount = useAdvanceReceiptAmount;

            return model;
        }
    }
}