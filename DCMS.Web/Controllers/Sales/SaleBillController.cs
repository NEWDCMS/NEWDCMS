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
using Newtonsoft.Json;
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
    /// 销售单
    /// </summary>
    public class SaleBillController : BasePublicController
    {
        private readonly IPrintTemplateService _printTemplateService;
        private readonly IUserActivityService _userActivityService;
        private readonly ISaleBillService _saleBillService;
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
        private readonly ISaleReservationBillService _saleReservationBillService;
        private readonly ICostContractBillService _costContractBillService;
        private readonly IRedLocker _locker;
        private readonly IExportManager _exportManager;
        private readonly ICommonBillService _commonBillService;
        private readonly IPurchaseBillService _purchaseBillService;
        private readonly ICashReceiptBillService _cashReceiptBillService;

        public SaleBillController(
            IWorkContext workContext,
            IStoreContext storeContext,
            ILogger loggerService,
            IPrintTemplateService printTemplateService,
            IUserActivityService userActivityService,
            ISaleBillService saleBillService,
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
            ISaleReservationBillService saleReservationBillService,
            ICostContractBillService costContractBillService,
            INotificationService notificationService,
            IRedLocker locker,
            IExportManager exportManager,
            ICommonBillService commonBillService,
            IPurchaseBillService purchaseBillService,
            ICashReceiptBillService cashReceiptBillService
            ) : base(workContext, loggerService, storeContext, notificationService)
        {
            _printTemplateService = printTemplateService;
            _userActivityService = userActivityService;
            _saleBillService = saleBillService;
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
            _saleReservationBillService = saleReservationBillService;
            _costContractBillService = costContractBillService;
            _locker = locker;
            _exportManager = exportManager;
            _commonBillService = commonBillService;
            _purchaseBillService = purchaseBillService;
            _cashReceiptBillService = cashReceiptBillService;
        }

        public IActionResult Index()
        {
            return RedirectToAction("List");
        }

        [AuthCode((int)AccessGranularityEnum.SaleBillListView)]
        public IActionResult List(int? terminalId, string terminalName, int? businessUserId, int? districtId, int? deliveryUserId, int? wareHouseId, string billNumber = "", string remark = "", DateTime? startTime = null, DateTime? endTime = null, bool? auditedStatus = null, bool? sortByAuditedTime = null, bool? showReverse = null, bool? showReturn = null, int? paymentMethodType = null, int? billSourceType = null, int? productId = 0,string productName="", int pagenumber = 0)
        {


            var model = new SaleBillListModel();

            if (pagenumber > 0)
            {
                pagenumber -= 1;
            }

            #region 绑定数据源

            model = PrepareSaleBillListModel(model);

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
            model.PaymentMethodType = paymentMethodType ?? null;
            model.BillSourceType = billSourceType ?? null;
            model.ProductId = productId ?? 0;
            model.ProductName = productName;
            #endregion

            var sales = _saleBillService.GetSaleBillList(curStore?.Id ?? 0,
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
                paymentMethodType,
                billSourceType,
                null,
                false,//未删除单据
                null,
                null,
                productId,
                pagenumber,
                pageSize: 30);

            //默认收款账户动态列
            var defaultAcc = _accountingService.GetDefaultAccounting(curStore?.Id ?? 0, BillTypeEnum.SaleBill);
            model.DynamicColumns = defaultAcc?.Item4?.OrderBy(s => s.Key).Select(s => s.Value).ToList();

            model.PagingFilteringContext.LoadPagedList(sales);

            #region 查询需要关联其他表的数据
            List<int> userIds = new List<int>();
            userIds.AddRange(sales.Select(b => b.BusinessUserId).Distinct().ToArray());
            userIds.AddRange(sales.Select(b => b.DeliveryUserId).Distinct().ToArray());
            var allUsers = _userService.GetUsersDictsByIds(curStore.Id, userIds.Distinct().ToArray());

            var allTerminal = _terminalService.GetTerminalsByIds(curStore.Id, sales.Select(b => b.TerminalId).Distinct().ToArray());
            //var allWareHouses = _wareHouseService.GetWareHouseByIds(curStore.Id, sales.Select(b => b.WareHouseId).Distinct().ToArray());

            //商品价格
            List<int> productIds = new List<int>();

            //所有关联销售订单
            var allSaleReservationBills = _saleReservationBillService.GetSaleReservationBillsByIds(sales.Select(b => b.SaleReservationBillId ?? 0).Distinct().ToArray());
            //销售订单中的商品
            if (allSaleReservationBills != null && allSaleReservationBills.Count > 0)
            {
                allSaleReservationBills.ToList().ForEach(saleReservation =>
                {
                    if (saleReservation != null && saleReservation.Items != null && saleReservation.Items.Count > 0)
                    {
                        productIds.AddRange(saleReservation.Items.Select(it => it.ProductId).Distinct().ToArray());
                    }
                });
            }

            //销售单中的商品
            if (sales != null && sales.Count > 0)
            {
                sales.ToList().ForEach(sale =>
                {
                    if (sale != null && sale.Items != null && sale.Items.Count > 0)
                    {
                        productIds.AddRange(sale.Items.Select(it => it.ProductId).Distinct().ToArray());
                    }

                });
            }
            productIds = productIds.Distinct().ToList();
            //商品价格
            var allProductPrices = _productService.GetProductPricesByProductIds(curStore.Id, productIds.Distinct().ToArray());
            #endregion

            model.Lists = sales.Select(s =>
            {
                var m = s.ToModel<SaleBillModel>();

                //业务员名称
                m.BusinessUserName = allUsers.Where(au => au.Key == s.BusinessUserId).Select(au => au.Value).FirstOrDefault();
                m.MakeUserName = allUsers.Where(au => au.Key == s.MakeUserId).Select(au => au.Value).FirstOrDefault();
                //送货员名称
                m.DeliveryUserName = allUsers.Where(au => au.Key == s.DeliveryUserId).Select(au => au.Value).FirstOrDefault();

                //客户名称
                var terminal = allTerminal.Where(at => at.Id == m.TerminalId).FirstOrDefault();
                m.TerminalName = terminal == null ? "" : terminal.Name;
                m.TerminalPointCode = terminal == null ? "" : terminal.Code;
                //仓库名称
                var warehouse = model.WareHouses.Where(s => s.Value == m.WareHouseId.ToString()).FirstOrDefault();
                m.WareHouseName = warehouse == null ? "" : warehouse.Text;

                //应收金额	
                m.ReceivableAmount = (s.ReceivableAmount == 0) ? ((s.OweCash != 0) ? s.OweCash : s.SaleBillAccountings.Sum(sa => sa.CollectionAmount)) : s.ReceivableAmount;

                //收款账户
                m.SaleBillAccountings = defaultAcc?.Item4?.OrderBy(sb => sb.Key).Select(sb =>
                {
                    var acc = s.SaleBillAccountings.Where(a => a?.AccountingOption?.ParentId == sb.Key).FirstOrDefault();
                    return new SaleBillAccountingModel()
                    {
                        AccountingOptionId = acc?.AccountingOptionId ?? 0,
                        CollectionAmount = acc?.CollectionAmount ?? 0
                    };
                }).ToList();



                //查询销售单 关联的销售订单
                bool saleReservationChangePrice = false;
                SaleReservationBill saleReservationBill = allSaleReservationBills.Where(ar => ar.Id == m.SaleReservationBillId).FirstOrDefault();
                if (saleReservationBill != null)
                {
                    m.SaleReservationBillId = saleReservationBill.Id;
                    m.SaleReservationBillNumber = saleReservationBill.BillNumber;
                    if (saleReservationBill.Items != null && saleReservationBill.Items.Count > 0)
                    {
                        saleReservationBill.Items.ToList().ForEach(it =>
                        {
                            ProductPrice productPrice = allProductPrices.Where(ap => ap.ProductId == it.ProductId && ap.UnitId == it.UnitId).FirstOrDefault();
                            if (productPrice != null && it.Price != productPrice.TradePrice)
                            {
                                saleReservationChangePrice = true;
                            }
                        });
                    }
                }
                m.SaleReservationChangePrice = saleReservationChangePrice;

                //是否变价
                bool saleChangePrice = false;
                if (s.Items != null && s.Items.Count > 0)
                {
                    s.Items.ToList().ForEach(it =>
                    {
                        ProductPrice productPrice = allProductPrices.Where(ap => ap.ProductId == it.ProductId && ap.UnitId == it.UnitId).FirstOrDefault();
                        if (productPrice != null && it.Price != productPrice.TradePrice)
                        {
                            saleChangePrice = true;
                        }
                    });
                }
                m.SaleChangePrice = saleChangePrice;

                return m;
            }).ToList();

            return View(model);
        }

        /// <summary>
        /// 获取销售单列表
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
        [AuthCode((int)AccessGranularityEnum.SaleBillListView)]
        public async Task<JsonResult> AsyncList(int? terminalId, int? businessUserId, string billNumber = "", int? wareHouseId = null, string remark = "", DateTime? start = null, DateTime? end = null, int? districtId = null, int? deliveryUserId = null, bool? auditedStatus = null, bool? sortByAuditedTime = null, bool? showReverse = null, bool? showReturn = null, int? paymentMethodType = null, int? billSourceType = null, bool? receipted = null, int? productId = 0, int pageIndex = 0, int pageSize = 20)
        {
            return await Task.Run(() =>
            {

                var gridModel = _saleBillService.GetSaleBillList(curStore?.Id ?? 0,
                    curUser.Id, 
                    terminalId, 
                    "", 
                    businessUserId, 
                    deliveryUserId, 
                    billNumber, 
                    wareHouseId, 
                    remark,
                    start, 
                    end, 
                    districtId, 
                    auditedStatus, 
                    sortByAuditedTime, 
                    showReverse, 
                    showReturn, 
                    paymentMethodType,
                    billSourceType,
                    receipted, 
                    false, 
                    null,
                    null,
                    productId,
                    pageIndex, 
                    pageSize);

                //默认收款账户动态列
                var defaultAcc = _accountingService.GetDefaultAccounting(curStore?.Id ?? 0, BillTypeEnum.SaleBill);

                return Json(new
                {
                    Success = true,
                    total = gridModel.TotalCount,
                    rows = gridModel.Select(s =>
                    {

                        var m = s.ToModel<SaleBillModel>();

                        //业务员名称
                        m.BusinessUserName = _userService.GetUserName(curStore.Id, m.BusinessUserId ?? 0);
                        //客户名称
                        var terminal = _terminalService.GetTerminalById(curStore.Id, m.TerminalId);
                        m.TerminalName = terminal == null ? "" : terminal.Name;
                        m.TerminalPointCode = terminal == null ? "" : terminal.Code;
                        //仓库名称
                        m.WareHouseName = _wareHouseService.GetWareHouseName(curStore.Id, m.WareHouseId);

                        //应收金额	
                        m.ReceivableAmount = (s.ReceivableAmount == 0) ? ((s.OweCash != 0) ? s.OweCash : s.SaleBillAccountings.Sum(sa => sa.CollectionAmount)) : s.ReceivableAmount;

                        //优惠金额
                        m.PreferentialAmount = s.PreferentialAmount;

                        //收款账户
                        m.SaleBillAccountings = defaultAcc?.Item4?.OrderBy(sb => sb.Key).Select(sb =>
                        {
                            var acc = s.SaleBillAccountings.Where(a => a?.AccountingOption?.ParentId == sb.Key).FirstOrDefault();
                            return new SaleBillAccountingModel()
                            {
                                AccountingOptionId = acc?.AccountingOptionId ?? 0,
                                CollectionAmount = acc?.CollectionAmount ?? 0
                            };
                        }).ToList();

                        return m;

                    }).ToList()
                });

            });
        }

        /// <summary>
        /// 添加销售单
        /// </summary>
        /// <returns></returns>
        [AuthCode((int)AccessGranularityEnum.SaleBillSave)]
        public IActionResult Create(int? orderId = 0,bool isCopy=false)
        {
            var companySetting = _settingService.LoadSetting<CompanySetting>(curStore.Id);

            var model = new SaleBillModel();

            model = PrepareSaleBillModel(model);
            model.PreferentialAmount = 0;
            model.PreferentialEndAmount = 0;
            model.OweCash = 0;
            model.TransactionDate = DateTime.Now;
            
            //单号
            model.BillNumber = CommonHelper.GetBillNumber(CommonHelper.GetEnumDescription(BillTypeEnum.SaleBill).Split(',')[1], curStore.Id);
            model.BillBarCode = _mediaService.GenerateBarCodeForBase64(model.BillNumber, 150, 50);
            //制单人
            var mu = _userService.GetUserById(curStore.Id, curUser.Id);
            model.MakeUserName = mu != null ? (mu.UserRealName + " " + DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss")) : "";


            //默认账户设置
            var defaultAcc = _accountingService.GetDefaultAccounting(curStore?.Id ?? 0, BillTypeEnum.SaleBill);
            model.SaleBillAccountings.Add(new SaleBillAccountingModel()
            {
                AccountingOptionId = defaultAcc?.Item1?.Id ?? 0,
                CollectionAmount = 0,
                Name = defaultAcc.Item1?.Name,
                AccountCodeTypeId = defaultAcc?.Item1?.AccountCodeTypeId ?? 0
            });


            model.IsShowCreateDate = (companySetting?.OpenBillMakeDate) != 0;
            //变价参考
            model.VariablePriceCommodity = companySetting?.VariablePriceCommodity ?? 0;
            //单据合计精度
            model.AccuracyRounding = companySetting?.AccuracyRounding ?? 0;
            //交易日期可选范围
            model.AllowSelectionDateRange = companySetting?.AllowSelectionDateRange ?? 0;
            //允许预收款支付成负数
            model.AllowAdvancePaymentsNegative = companySetting?.AllowAdvancePaymentsNegative ?? false;
            //显示订单占用库存
            model.APPShowOrderStock = companySetting?.APPShowOrderStock ?? false;

            model.UserMaxAmount = 0;
            model.UserUsedAmount = 0;
            model.UserAvailableAmount = 0;

            //启用税务功能
            model.EnableTaxRate = companySetting.EnableTaxRate;
            model.TaxRate = companySetting.TaxRate;

            //转单
            if (orderId.HasValue && orderId.Value > 0&&!isCopy)
            {
                var order = _saleReservationBillService.GetSaleReservationBillById(curStore.Id, orderId.Value, true);
                model.OrderId = orderId.Value;
                model.OrderNumber = order?.BillNumber;
                model.TerminalId = order?.TerminalId ?? 0;
                model.TerminalName = _terminalService.GetTerminalName(curStore.Id, order?.TerminalId ?? 0);
                model.BusinessUserId = order?.BusinessUserId;
                model.WareHouseId = order?.WareHouseId ?? 0;
               

                //收款账户
                model.SaleBillAccountings = order.SaleReservationBillAccountings.Select(s =>
                {
                    return new SaleBillAccountingModel()
                    {
                        Name = s?.AccountingOption?.Name,
                        CollectionAmount = s?.CollectionAmount ?? 0,
                        AccountingOptionId = s?.AccountingOptionId ?? 0
                    };
                }).ToList();

                //追加欠款
                model.SaleBillAccountings.Add(new SaleBillAccountingModel()
                {
                    Name = defaultAcc?.Item1?.Name,
                    CollectionAmount = order?.OweCash ?? 0,
                    AccountingOptionId = defaultAcc?.Item1?.Id ?? 0
                });

                model.SaleBillAccountings.OrderBy(a => a.AccountingOptionId);
            }
            else if(orderId.HasValue && orderId.Value > 0 && isCopy)
            {//复制
                var bill = _saleBillService.GetSaleBillById(curStore.Id, orderId.Value, true);

                //没有值跳转到列表
                if (bill == null || bill.StoreId != curStore.Id)
                {
                    return RedirectToAction("List");
                }

                //获取默认收款账户
                model.CollectionAccount = defaultAcc?.Item1?.Id ?? 0;
                model.CollectionAmount = bill.SaleBillAccountings.Where(sa => sa.AccountingOptionId == defaultAcc?.Item1?.Id).Sum(sa => sa.CollectionAmount);

               
                model.SaleBillAccountings = bill.SaleBillAccountings.Select(s =>
                {
                    var m = s.ToAccountModel<SaleBillAccountingModel>();
                    m.Name = _accountingService.GetAccountingOptionName(curStore.Id, s.AccountingOptionId);
                    return m;
                }).ToList();

                //取单据项目
                model.Items = bill.Items.Select(s => s.ToModel<SaleItemModel>()).ToList();

                model.DeliveryUserId = bill.DeliveryUserId;
                model.BusinessUserId = bill.BusinessUserId;
                model.WareHouseId = bill.WareHouseId;

                //获取客户名称
                model.TerminalId = bill.TerminalId;
                var terminal = _terminalService.GetTerminalById(curStore.Id, model.TerminalId);
                model.TerminalName = terminal == null ? "" : terminal.Name;
                model.TerminalPointCode = terminal == null ? "" : terminal.Code;
                model.TargetBillIdByCopy = bill.Id;

                //优惠后金额
                //model.PreferentialEndAmount = model.ReceivableAmount - model.PreferentialAmount;
                //model.PreferentialEndAmount = model.SumAmount - model.PreferentialAmount;
                //model.IsShowCreateDate = companySetting.OpenBillMakeDate != 0;


                //model.UserMaxAmount = _userService.GetUserMaxAmountOfArrears(curStore.Id, model.BusinessUserId ?? 0);
                //model.UserUsedAmount = _commonBillService.GetUserUsedAmount(model.StoreId, model.BusinessUserId ?? 0);
                //model.UserAvailableAmount = (model.UserMaxAmount - model.UserUsedAmount) < 0 ? 0 : model.UserMaxAmount - model.UserUsedAmount;

            }
            return View(model);
        }

        /// <summary>
        /// 编辑销售单
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [AuthCode((int)AccessGranularityEnum.SaleBillListView)]
        public IActionResult Edit(int? id, bool fix = false)
        {

            //没有值跳转到列表
            if (id == null)
            {
                return RedirectToAction("List");
            }

            var model = new SaleBillModel();
            var companySetting = _settingService.LoadSetting<CompanySetting>(curStore.Id);

            var bill = _saleBillService.GetSaleBillById(curStore.Id, id.Value, true);

            //没有值跳转到列表
            if (bill == null || bill.StoreId != curStore.Id)
            {
                return RedirectToAction("List");
            }

            model = bill.ToModel<SaleBillModel>();
            model.BillBarCode = _mediaService.GenerateBarCodeForBase64(bill.BillNumber, 150, 50);

            //获取默认收款账户
            var defaultAcc = _accountingService.GetDefaultAccounting(curStore?.Id ?? 0, BillTypeEnum.SaleBill);
            model.CollectionAccount = defaultAcc?.Item1?.Id ?? 0;
            model.CollectionAmount = bill.SaleBillAccountings.Where(sa => sa.AccountingOptionId == defaultAcc?.Item1?.Id).Sum(sa => sa.CollectionAmount);
            model.SaleBillAccountings = bill.SaleBillAccountings.Select(s =>
            {
                var m = s.ToAccountModel<SaleBillAccountingModel>();
                m.Name = _accountingService.GetAccountingOptionName(curStore.Id, s.AccountingOptionId);
                return m;
            }).ToList();

            //取单据项目
            model.Items = bill.Items.Select(s => s.ToModel<SaleItemModel>()).ToList();

            //获取客户名称
            var terminal = _terminalService.GetTerminalById(curStore.Id, model.TerminalId);
            model.TerminalName = terminal == null ? "" : terminal.Name;
            model.TerminalPointCode = terminal == null ? "" : terminal.Code;
            #region 绑定数据源

            model = PrepareSaleBillModel(model);

            //制单人
            var mu = _userService.GetUserById(curStore.Id, bill.MakeUserId);
            model.MakeUserName = mu != null ? (mu.UserRealName + " " + bill.CreatedOnUtc.ToString("yyyy/MM/dd HH:mm:ss")) : "";

            //审核人
            var au = _userService.GetUserById(curStore.Id, bill.AuditedUserId ?? 0);
            model.AuditedUserName = au != null ? (au.UserRealName + " " + (bill.AuditedDate.HasValue ? bill.AuditedDate.Value.ToString("yyyy/MM/dd HH:mm:ss") : "")) : "";

            #endregion

            //优惠后金额
            //model.PreferentialEndAmount = model.ReceivableAmount - model.PreferentialAmount;
            model.PreferentialEndAmount = model.SumAmount - model.PreferentialAmount;
            model.IsShowCreateDate = companySetting.OpenBillMakeDate != 0;
            //变价参考
            model.VariablePriceCommodity = companySetting.VariablePriceCommodity;
            //单据合计精度
            model.AccuracyRounding = companySetting.AccuracyRounding;
            //交易日期可选范围
            model.AllowSelectionDateRange = companySetting.AllowSelectionDateRange;
            //允许预收款支付成负数
            model.AllowAdvancePaymentsNegative = companySetting.AllowAdvancePaymentsNegative;
            //显示订单占用库存
            model.APPShowOrderStock = companySetting.APPShowOrderStock;

            //终端、员工欠款
            //model.TerminalMaxAmount = terminal != null ? (terminal.MaxAmountOwed ?? 0) : 0;
            ////model.TerminalUsedAmount = _commonBillService.GetTerminalBalance(model.StoreId, model.TerminalId);
            //model.TerminalAvailableAmount = (model.TerminalMaxAmount - model.TerminalUsedAmount) < 0 ? 0 : model.TerminalMaxAmount - model.TerminalUsedAmount;

            model.UserMaxAmount = _userService.GetUserMaxAmountOfArrears(curStore.Id, model.BusinessUserId ?? 0);
            model.UserUsedAmount = _commonBillService.GetUserUsedAmount(model.StoreId, model.BusinessUserId ?? 0);
            model.UserAvailableAmount = (model.UserMaxAmount - model.UserUsedAmount) < 0 ? 0 : model.UserMaxAmount - model.UserUsedAmount;

            //启用税务功能
            model.EnableTaxRate = companySetting.EnableTaxRate;
            model.TaxRate = companySetting.TaxRate;
            model.Fix = fix;

            return View(model);
        }

        #region 单据项目

        /// <summary>
        /// 异步获取销售单项目
        /// </summary>
        /// <param name="saleId"></param>
        /// <returns></returns>
        public JsonResult AsyncSaleItems(int saleBillId, int targetBillId = 0)
        {
            if (targetBillId != 0 && saleBillId==0)
            {
                saleBillId = targetBillId;
            }

            var gridModel = _saleBillService.GetSaleItemList(saleBillId);
            var allProducts = _productService.GetProductsByIds(curStore.Id, gridModel.Select(pr => pr.ProductId).Distinct().ToArray());
            var allOptions = _specificationAttributeService.GetSpecificationAttributeOptionByIds(curStore.Id, allProducts.GetProductBigStrokeSmallUnitIds());
            var allProductPrices = _productService.GetProductPricesByProductIds(curStore.Id, gridModel.Select(gm => gm.ProductId).Distinct().ToArray());
            var allProductTierPrices = _productService.GetProductTierPriceByProductIds(curStore.Id, gridModel.Select(pr => pr.ProductId).Distinct().ToArray());

            var details = gridModel.Select(o =>
            {
                var m = o.ToModel<SaleItemModel>();
                var product = allProducts.Where(ap => ap.Id == m.ProductId).FirstOrDefault();

                if (product != null)
                {
                    //这里替换成高级用法
                    m = product.InitBaseModel<SaleItemModel>(m, o.SaleBill.WareHouseId, allOptions, allProductPrices, allProductTierPrices, _productService);

                    //商品信息
                    m.BigUnitId = product.BigUnitId;
                    m.StrokeUnitId = product.StrokeUnitId;
                    m.SmallUnitId = product.SmallUnitId;
                    m.IsManufactureDete = product.IsManufactureDete;
                    m.ProductTimes = _productService.GetProductDates(curStore.Id, product.Id, o.SaleBill.WareHouseId);

                    m.SaleProductTypeName = CommonHelper.GetEnumDescription<SaleProductTypeEnum>(m.SaleProductTypeId ?? 0);

                    //税价总计
                    m.TaxPriceAmount = m.Amount;

                    if (o.SaleBill.TaxAmount > 0 && m.TaxRate > 0)
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

        /// <summary>
        /// 更新/编辑收款单项目
        /// </summary>
        /// <param name="data"></param>
        /// <param name="saleId"></param>
        /// <returns></returns>
        [HttpPost]
        [AuthCode((int)AccessGranularityEnum.SaleBillSave)]
        public async Task<JsonResult> CreateOrUpdate(SaleBillUpdateModel data, int? billId,bool doAudit = true)
        {
            try
            {
                var bill = new SaleBill();

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

                    bill = _saleBillService.GetSaleBillById(curStore.Id, billId.Value, true);

                    //公共单据验证
                    var commonBillChecking = BillChecking<SaleBill, SaleItem>(bill, BillStates.Draft, ((int)AccessGranularityEnum.SaleBillSave).ToString());
                    if (commonBillChecking.Value != null)
                    {
                        return commonBillChecking;
                    }
                }

                #region 验证赠品

                if (data.Items.Any())
                {
                    //SaleProductTypeId=1为促销活动销售商品
                    List<SaleItem> items = data.Items.Where(it => it.SaleProductTypeId > 0).Select(it => it.ToEntity<SaleItem>()).ToList();
                    if (!_costContractBillService.CheckGift(curStore.Id, data.TerminalId, items, out string msg))
                    {
                        return Warning(msg);
                    }
                }
                #endregion

                #region 预收款 验证
                //预付款 验证
                if (data.Accounting != null)
                {
                    //剩余预收款金额(预收账款科目下的所有子类科目)：
                    //var advanceAmountBalance = _commonBillService.CalcTerminalBalance(curStore.Id, data.TerminalId);
                    if (data.AdvanceAmount > 0 && data.AdvanceAmount > data.AdvanceAmountBalance)
                    {
                        return Warning("预收款余额不足!");
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

                //当前数据
                List<ProductStockItem> productStockItemNews = new List<ProductStockItem>();
                if (data.Items != null && data.Items.Count > 0)
                {
                    allProducts = _productService.GetProductsByIds(curStore.Id, data.Items.Select(pr => pr.ProductId).Distinct().ToArray());
                    var allOptions = _specificationAttributeService.GetSpecificationAttributeOptionByIds(curStore.Id, allProducts.GetProductBigStrokeSmallUnitIds());

                    foreach (SaleItemModel item in data.Items)
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
                if (!_stockService.CheckStockQty(_productService, _specificationAttributeService, curStore.Id, data.WareHouseId, productStockItemNews, out string errMsg, data.OrderId<=0))
                {
                    return Warning(errMsg);
                }

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
                                errMsg += $"商品：{it.ProductName},0元开单，必须选择备注.";
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
                var dataTo = data.ToEntity<SaleBillUpdate>();
                dataTo.Operation = (int)OperationEnum.PC;
                if (data.Accounting == null)
                {
                    return Warning("没有默认的付款账号");
                }
                dataTo.Accounting = data.Accounting.Select(ac =>
                {
                    return ac.ToAccountEntity<SaleBillAccounting>();
                }).ToList();
                dataTo.Items = data.Items.Select(it =>
                {
                    //成本价（此处计算成本价防止web、api成本价未带出,web、api的controller都要单独计算（取消service计算，防止其他service都引用 pruchasebillservice））
                    var item = it.ToEntity<SaleItem>();
                    item.CostPrice = _purchaseBillService.GetReferenceCostPrice(curStore.Id, item.ProductId, item.UnitId);
                    item.CostAmount = item.CostPrice * item.Quantity;
                    return item;
                }).ToList();

                //RedLock
                var result = await _locker.PerformActionWithLockAsync(LockKey(data),
                    TimeSpan.FromSeconds(30),
                    TimeSpan.FromSeconds(10),
                    TimeSpan.FromSeconds(1),
                    () => _saleBillService.BillCreateOrUpdate(curStore.Id, curUser.Id, billId, bill, dataTo.Accounting, accountings, dataTo, dataTo.Items, productStockItemNews, _userService.IsAdmin(curStore.Id, curUser.Id),null, doAudit));

                //如果是转单则转单状态
                if (result.Success && data.OrderId > 0)
                {
                    _saleReservationBillService.ChangedBill(data.OrderId, curUser.Id);
                }

                return Json(result);
            }
            catch (Exception ex)
            {
                //活动日志
                _userActivityService.InsertActivity("CreateOrUpdate", Resources.Bill_CreateOrUpdateFailed, curUser.Id);
                _notificationService.SuccessNotification(Resources.Bill_CreateOrUpdateFailed);
                return Error(ex.Message + ":" + ex.StackTrace);
            }
        }

        #endregion



        
        /// <summary>
        /// 审核
        /// </summary>
        /// <returns></returns>
        [AuthCode((int)AccessGranularityEnum.SaleBillApproved)]
        public async Task<JsonResult> Auditing(int? id)
        {
            try
            {
                var bill = new SaleBill();

                #region 验证

                if (!id.HasValue)
                {
                    return Warning("参数错误.");
                }
                else
                {
                    bill = _saleBillService.GetSaleBillById(curStore.Id, id.Value, true);
                    if (bill.AuditedStatus)
                    {
                        return Warning("单据已审核，请刷新页面.");
                    }
                }

                

                //公共单据验证
                var commonBillChecking = BillChecking<SaleBill, SaleItem>(bill, BillStates.Audited, ((int)AccessGranularityEnum.SaleBillApproved).ToString());
                if (commonBillChecking.Value != null)
                {
                    return commonBillChecking;
                }

                if (_wareHouseService.CheckProductInventory(curStore.Id, bill.WareHouseId, bill.Items?.Select(it => it.ProductId).Distinct().ToArray(), out string thisMsg))
                {
                    return Warning("仓库正在盘点中，拒绝操作");
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
                if (bill.SaleBillAccountings != null)
                {
                    //1.获取当前经销商 预收款科目Id
                    int accountingOptionId = 0;
                    AccountingOption accountingOption = _accountingService.GetAccountingOptionByAccountCodeTypeId(curStore.Id, (int)AccountingCodeEnum.AdvancesReceived);
                    if (accountingOption != null)
                    {
                        accountingOptionId = (accountingOption == null) ? 0 : accountingOption.Id;
                    }
                    //获取用户输入 预收款金额
                    var advancePaymentReceipt = bill.SaleBillAccountings.Where(ac => ac.AccountingOptionId == accountingOptionId).Sum(ac => ac.CollectionAmount);

                    //用户可用 预收款金额
                    var useAdvanceReceiptAmount = _commonBillService.CalcTerminalBalance(curStore.Id, bill.TerminalId);
                    //如果输入预收款大于用户可用预收款
                    if (advancePaymentReceipt > 0 && advancePaymentReceipt > useAdvanceReceiptAmount.AdvanceAmountBalance)
                    {
                        errMsg += "预收款余额不足";
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

                    foreach (SaleItem item in bill.Items)
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
                      () => _saleBillService.Auditing(curStore.Id, curUser.Id, bill));

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
        [AuthCode((int)AccessGranularityEnum.SaleBillReverse)]
        public async Task<JsonResult> Reverse(int? id)
        {
            try
            {
                var bill = new SaleBill() { StoreId = curStore?.Id ?? 0 };

                #region 验证

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
                    bill = _saleBillService.GetSaleBillById(curStore.Id, id.Value, true);
                    if (bill.ReversedStatus)
                    {
                        return Warning("单据已冲红，请刷新页面.");
                    }
                }

                //公共单据验证
                var commonBillChecking = BillChecking<SaleBill, SaleItem>(bill, BillStates.Reversed, ((int)AccessGranularityEnum.SaleBillReverse).ToString());
                if (commonBillChecking.Value != null)
                {
                    return commonBillChecking;
                }

                if (_wareHouseService.CheckProductInventory(curStore.Id, bill.WareHouseId, bill.Items?.Select(it => it.ProductId).Distinct().ToArray(), out string thisMsg))
                {
                    return Warning("仓库正在盘点中，拒绝操作");
                }

                //验证是否收款
                var cashReceipt = _cashReceiptBillService.CheckBillCashReceipt(curStore.Id, (int)BillTypeEnum.SaleBill, bill.BillNumber);
                if (cashReceipt.Item1)
                {
                    return Warning($"单据在收款单:{cashReceipt.Item2}中已经收款.");
                }

                #endregion

                //RedLock
                var result = await _locker.PerformActionWithLockAsync(RedLockKey(bill),
                      TimeSpan.FromSeconds(30),
                      TimeSpan.FromSeconds(10),
                      TimeSpan.FromSeconds(1),
                      () => _saleBillService.Reverse(curUser.Id, bill));
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

        /// <summary>
        /// 作废
        /// </summary>
        /// <returns></returns>
        [AuthCode((int)AccessGranularityEnum.SaleBillsDelete)]
        public JsonResult Delete(int? id)
        {
            try
            {
                var bill = new SaleBill() { StoreId = curStore?.Id ?? 0 };

                #region 验证
                if (!id.HasValue)
                {
                    return Warning("参数错误.");
                }
                else
                {
                    bill = _saleBillService.GetSaleBillById(curStore.Id, id.Value, true);
                    if (bill.AuditedStatus || bill.ReversedStatus)
                    {
                        return Warning("单据已审核或红冲，不能作废.");
                    }
                    if (bill.Deleted)
                    {
                        return Warning("单据已作废，请刷新页面.");
                    }
                    if (bill.SaleReservationBillId > 0)
                    {
                        return Warning("转单单据，不允许作废，请进行审核，并红冲.");
                    }
                    if (bill != null)
                    {
                        var rs = _saleBillService.Delete(curUser.Id, bill);
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
        /// <param name="paymentMethodType"></param>
        /// <param name="billSourceType"></param>
        /// <returns></returns>
        [HttpGet]
        [AuthCode((int)AccessGranularityEnum.SaleBillExport)]
        public FileResult Export(int type, string selectData, int? terminalId, string terminalName, int? businessUserId, string billNumber = "", int? wareHouseId = null, string remark = "", DateTime? startTime = null, DateTime? endTime = null, int? districtId = null, int? deliveryUserId = null, bool? auditedStatus = null, bool? sortByAuditedTime = null, bool? showReverse = null, bool? showReturn = null, int? paymentMethodType = null, int? billSourceType = null)
        {

            #region 查询导出数据

            IList<SaleBill> saleBills = new List<SaleBill>();
            if (type == 1)
            {
                if (!string.IsNullOrEmpty(selectData))
                {
                    List<string> ids = selectData.Split(',').ToList();
                    foreach (var id in ids)
                    {
                        SaleBill saleBill = _saleBillService.GetSaleBillById(curStore.Id, int.Parse(id), true);
                        if (saleBill != null)
                        {
                            saleBills.Add(saleBill);
                        }
                    }
                }
            }
            else if (type == 2)
            {
                saleBills = _saleBillService.GetSaleBillList(curStore?.Id ?? 0,
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
                                paymentMethodType,
                                billSourceType,
                                null,
                                false,
                                null,
                                null, 0);
            }

            #endregion

            #region 导出
            var ms = _exportManager.ExportSaleBillToXlsx(saleBills, curStore.Id);
            if (ms != null)
            {
                return File(ms, "application/vnd.ms-excel", "销售单.xlsx");
            }
            else
            {
                return File(new MemoryStream(), "application/vnd.ms-excel", "销售单.xlsx");
            }
            #endregion

        }
        [AuthCode((int)AccessGranularityEnum.SaleBillPrint)]
        public JsonResult PrintSetting() 
        {
            var printTemplates = _printTemplateService.GetAllPrintTemplates(curStore.Id).ToList();
            var printTemplate = printTemplates.Where(a => a.BillType == (int)BillTypeEnum.SaleBill).FirstOrDefault();
            //获取打印设置
            var pCPrintSetting = _settingService.LoadSetting<PCPrintSetting>(_storeContext.CurrentStore.Id);
            var settings = new object();
            if (pCPrintSetting != null)
            {
                settings = new
                {
                    PaperWidth = (printTemplate?.PaperWidth == 0 || printTemplate?.PaperHeight == 0) ? pCPrintSetting.PaperWidth : printTemplate.PaperWidth,
                    PaperHeight = (printTemplate?.PaperWidth == 0 || printTemplate?.PaperHeight == 0) ? pCPrintSetting.PaperHeight : printTemplate.PaperHeight,
                    pCPrintSetting.BorderType,
                    pCPrintSetting.MarginTop,
                   pCPrintSetting.MarginBottom,
                   pCPrintSetting.MarginLeft,
                   pCPrintSetting.MarginRight,
                   pCPrintSetting.IsPrintPageNumber,
                   pCPrintSetting.PrintHeader,
                    pCPrintSetting.PrintFooter,
                     pCPrintSetting.FixedRowNumber,
                   pCPrintSetting.PrintSubtotal,
                    pCPrintSetting.PrintPort,
                    pCPrintSetting.PrintInAllPages,
                    pCPrintSetting.PageRowsCount,
                    pCPrintSetting.HeaderHeight,
                    pCPrintSetting.FooterHeight
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
        [AuthCode((int)AccessGranularityEnum.SaleBillPrint)]
        public JsonResult Print(int type, string selectData, int? terminalId, string terminalName, int? businessUserId, string billNumber = "", int? wareHouseId = null, string remark = "", DateTime? startTime = null, DateTime? endTime = null, int? districtId = null, int? deliveryUserId = null, bool? auditedStatus = null, bool? sortByAuditedTime = null, bool? showReverse = null, bool? showReturn = null, int? paymentMethodType = null, int? billSourceType = null)
        {
            try
            {
                bool fg = true;
                string errMsg = string.Empty;

                #region 查询打印数据

                IList<SaleBill> saleBills = new List<SaleBill>();
                var datas = new List<string>();
                //默认选择
                type = type == 0 ? 1 : type;
                if (type == 1)
                {
                    if (!string.IsNullOrEmpty(selectData))
                    {
                        List<string> ids = selectData.Split(',').ToList();
                        foreach (var id in ids)
                        {
                            SaleBill saleBill = _saleBillService.GetSaleBillById(curStore.Id, int.Parse(id), true);
                            if (saleBill != null)
                            {
                                saleBills.Add(saleBill);
                            }
                        }
                    }
                }
                else if (type == 2)
                {
                    saleBills = _saleBillService.GetSaleBillList(curStore?.Id ?? 0,
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
                                paymentMethodType,
                                billSourceType,
                                null,
                                false,
                                null,
                                null,
                                0);
                }

                #endregion

                #region 修改数据
                if (saleBills != null && saleBills.Count > 0)
                {
                    #region 修改单据表打印数
                    foreach (var d in saleBills)
                    {
                        //获取默认收款账户
                        d.SaleBillAccountings.Select(s =>
                        {
                            s.AccountingOptionName = _accountingService.GetAccountingOptionName(curStore.Id, s.AccountingOptionId);
                            return s;
                        }).ToList();

                        d.PrintNum += 1;
                        _saleBillService.UpdateSaleBill(d);
                    }
                    #endregion
                }

                //获取打印模板
                var printTemplates = _printTemplateService.GetAllPrintTemplates(curStore.Id).ToList();
                var printTemplate = printTemplates.Where(a => a.BillType == (int)BillTypeEnum.SaleBill).FirstOrDefault();
                var content = printTemplate?.Content;
                //获取打印设置
                var pCPrintSetting = _settingService.LoadSetting<PCPrintSetting>(_storeContext.CurrentStore.Id);

                //填充打印数据
                foreach (var d in saleBills)
                {

                    StringBuilder sb = new StringBuilder();
                    sb.Append(content);

                    #region theadid
                    //sb.Replace("@商铺名称", curStore.Name);
                    if (pCPrintSetting != null)
                    {
                        sb.Replace("@商铺名称", pCPrintSetting?.StoreName ?? "&nbsp;");
                    }

                    Terminal terminal = _terminalService.GetTerminalById(curStore.Id, d?.TerminalId ?? 0);
                    if (terminal != null)
                    {
                        sb.Replace("@客户名称", terminal.Name);
                        sb.Replace("@老板姓名", terminal.BossName);
                        sb.Replace("@客户电话", terminal.BossCall);
                        sb.Replace("@客户地址", terminal.Address);
                    }
                    sb.Replace("@单据编号", d.BillNumber);
                    User makeUser = _userService.GetUserById(curStore.Id, d?.MakeUserId ?? 0);
                    if (makeUser != null)
                    {
                        sb.Replace("@制单", makeUser.UserRealName);
                    }

                    sb.Replace("@交易日期", d.TransactionDate == null ? "" : ((DateTime)d.TransactionDate).ToString("yyyy/MM/dd HH:mm:ss"));
                    sb.Replace("@打印日期", DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss"));
                    User businessUser = _userService.GetUserById(curStore.Id, d?.BusinessUserId ?? 0);
                    if (businessUser != null)
                    {
                        sb.Replace("@业务员", businessUser.UserRealName);
                        sb.Replace("@业务电话", businessUser.MobileNumber);
                    }
                    User deliveryUser = _userService.GetUserById(curStore.Id, d?.DeliveryUserId ?? 0);
                    if (deliveryUser != null)
                    {
                        sb.Replace("@送货员", deliveryUser.UserRealName);
                        sb.Replace("@送货人", deliveryUser.UserRealName);
                        sb.Replace("@送货人电话", deliveryUser.MobileNumber);
                    }
                    WareHouse wareHouse = _wareHouseService.GetWareHouseById(curStore.Id, d?.WareHouseId ?? 0);
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
                        foreach (var item in d.Items.ToList())
                        {
                            int index = sb.ToString().IndexOf("</tbody>", beginTbody);
                            i++;
                            StringBuilder sb2 = new StringBuilder();
                            sb2.Append(tbodytr);

                            sb2.Replace("#序号", i.ToString());
                            var product = allProducts.Where(ap => ap.Id == item.ProductId).FirstOrDefault();
                            if (product != null)
                            {
                                sb2.Replace("#商品名称", string.IsNullOrWhiteSpace(product.MnemonicCode) ?   product.Name : product.MnemonicCode);
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
                                var qty = Pexts.StockQuantityFormat(conversionQuantity * item.Quantity,0, product.BigQuantity ?? 0);
                                //sb2.Replace("#辅助数量", qty.Item1+ "大" + qty.Item2 + "中" + qty.Item3 + "小");
                                sb2.Replace("#辅助数量", qty.Item1 + productUnitOption.bigOption.Name + qty.Item3 + productUnitOption.smallOption.Name);
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
                        sb.Replace("备注:###", d.Remark);
                    }
                    #endregion


                    #region tfootid
                    sb.Replace("@营业执照", "***");
                    sb.Replace("@食品流通许可证", "***");
                    sb.Replace("@公司地址", pCPrintSetting?.Address);
                    sb.Replace("@订货电话", pCPrintSetting?.PlaceOrderTelphone);
                    sb.Replace("@优惠金额", d?.PreferentialAmount.ToString());
                    sb.Replace("@优惠后金额", (d?.SumAmount - d?.PreferentialAmount).ToString());

                    //收/付款方式
                    var accounts = new StringBuilder();
                    foreach (var acc in d?.SaleBillAccountings)
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
        private SaleBillListModel PrepareSaleBillListModel(SaleBillListModel model)
        {
            var isAdmin = _userService.IsAdmin(curStore?.Id ?? 0, curUser.Id);

            //业务员
            model.BusinessUsers = BindUserSelection(_userService.BindUserList, curStore, DCMSDefaults.Salesmans, curUser.Id, true, isAdmin);
            model.BusinessUserId ??= -1;

            //仓库
            model.WareHouses = BindWareHouseSelection(_wareHouseService.BindWareHouseList, curStore, WHAEnum.SaleBill, curUser.Id);

            //送货员
            model.DeliveryUsers = BindUserSelection(_userService.BindUserList, curStore, DCMSDefaults.Delivers, curUser.Id, true, isAdmin);

            //部门
            model = BindDropDownList<SaleBillListModel>(model, new Func<int?, int, List<Branch>>(_branchService.BindBranchsByParentId), curStore?.Id ?? 0, 0);

            //片区
            model.Districts = BindDistrictSelection(_districtService.BindDistrictList, curStore);

            return model;
        }

        [NonAction]
        private SaleBillModel PrepareSaleBillModel(SaleBillModel model)
        {
            model.BillTypeEnumId = (int)BillTypeEnum.SaleBill;

            //业务员
            model.BusinessUsers = BindUserSelection(_userService.BindUserList, curStore, DCMSDefaults.Salesmans,curUser.Id,true, _userService.IsAdmin(curStore.Id,curUser.Id));

            //当前用户为业务员时默认绑定
            if (curUser.IsSalesman() && !(model.Id > 0))
            {
                model.BusinessUserId = curUser.Id;
            }
            else
            {
                model.BusinessUserId = model.BusinessUserId == 0 ? null : model.BusinessUserId;
            }

            //仓库
            model.WareHouses = BindWareHouseSelection(_wareHouseService.BindWareHouseList, curStore, WHAEnum.SaleBill, curUser.Id);

            //送货员
            model.DeliveryUsers = BindUserSelection(_userService.BindUserList, curStore, DCMSDefaults.Delivers, curUser.Id, true, _userService.IsAdmin(curStore.Id,curUser.Id));
            model.DeliveryUserId = model.DeliveryUserId == 0 ? null : model.DeliveryUserId;

            //默认售价类型（价格体系）
            model.SaleDefaultAmounts = BindPricePlanSelection(_productTierPricePlanService.GetAllPricePlan, curStore);
            ////model.DefaultAmountId = (model.DefaultAmountId ?? "");
            ////默认上次售价
            //string lastedPriceDes = CommonHelper.GetEnumDescription(PriceType.LastedPrice);
            //var lastSaleType = model.SaleDefaultAmounts.Where(sr => sr.Text == lastedPriceDes).FirstOrDefault();
            //model.DefaultAmountId = (model.DefaultAmountId ?? (lastSaleType == null ? "" : lastSaleType.Value));
            //默认
            model.DefaultAmountId = "0_5";
            var companySetting = _settingService.LoadSetting<CompanySetting>(curStore.Id);
            if (companySetting != null)
            {
                if (!string.IsNullOrEmpty(companySetting.DefaultPricePlan))
                {
                    //分析配置 格式："{PricesPlanId}_{PriceTypeId}"
                    var settingDefault = model.SaleDefaultAmounts?.Where(s => s.Value.EndsWith(companySetting.DefaultPricePlan.ToString())).FirstOrDefault();
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