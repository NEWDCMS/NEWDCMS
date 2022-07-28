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
    /// 退货订单
    /// </summary>
    public class ReturnReservationBillController : BasePublicController
    {
        private readonly IPrintTemplateService _printTemplateService;
        private readonly IUserActivityService _userActivityService;
        private readonly IReturnReservationBillService _returnReservationBillService;
        private readonly IReturnBillService _returnBillService;
        private readonly IDistrictService _districtService;
        private readonly IWareHouseService _wareHouseService;
        private readonly IProductService _productService;
        private readonly ISettingService _settingService;
        private readonly ISpecificationAttributeService _specificationAttributeService;
        private readonly IProductTierPricePlanService _productTierPricePlanService;
        private readonly IBranchService _branchService;
        private readonly IUserService _userService;
        private readonly ITerminalService _terminalService;
        private readonly IAccountingService _accountingService;
        private readonly IMediaService _mediaService;
        private readonly IRedLocker _locker;
        private readonly IExportManager _exportManager;
        private readonly IPurchaseBillService _purchaseBillService;

        public ReturnReservationBillController(
            IWorkContext workContext,
            IStoreContext storeContext,
            ILogger loggerService,
            IPrintTemplateService printTemplateService,
            IUserActivityService userActivityService,
            IReturnReservationBillService returnReservationBillService,
            IReturnBillService returnBillService,
            IDistrictService districtService,
            IWareHouseService wareHouseService,
            IProductService productService,
            ISettingService settingService,
            ISpecificationAttributeService specificationAttributeService,
            IProductTierPricePlanService productTierPricePlanService,
            IBranchService branchService,
            IUserService userService,
            ITerminalService terminalService,
            IAccountingService accountingService,
            IMediaService mediaService,
            INotificationService notificationService,
            IRedLocker locker,
            IExportManager exportManager,
            IPurchaseBillService purchaseBillService
            ) : base(workContext, loggerService, storeContext, notificationService)
        {
            _printTemplateService = printTemplateService;
            _userActivityService = userActivityService;
            _returnReservationBillService = returnReservationBillService;
            _returnBillService = returnBillService;
            _districtService = districtService;
            _wareHouseService = wareHouseService;
            _productService = productService;
            _specificationAttributeService = specificationAttributeService;
            _productTierPricePlanService = productTierPricePlanService;
            _settingService = settingService;
            _branchService = branchService;
            _userService = userService;
            _terminalService = terminalService;
            _accountingService = accountingService;
            _mediaService = mediaService;
            _locker = locker;
            _exportManager = exportManager;
            _purchaseBillService = purchaseBillService;
        }

        public IActionResult Index()
        {
            return RedirectToAction("List");
        }

        [AuthCode((int)AccessGranularityEnum.ReturnOrderView)]
        public IActionResult List(int? terminalId, string terminalName, int? businessUserId, int? deliveryUserId, int? wareHouseId = null, int? districtId = null, string billNumber = "", string remark = "", DateTime? startTime = null, DateTime? endTime = null, bool? auditedStatus = null, bool? sortByAuditedTime = null, bool? showReverse = null, bool? showReturn = null, bool? alreadyChange = null, int pagenumber = 0)
        {
            var companySetting = _settingService.LoadSetting<CompanySetting>(curStore.Id);
            var model = new ReturnReservationBillListModel();

            if (pagenumber > 0)
            {
                pagenumber -= 1;
            }

            #region 绑定数据源

            model = PrepareReturnReservationBillListModel(model);

            model.TerminalId = terminalId ?? null;
            model.TerminalName = terminalName;
            model.BusinessUserId = businessUserId ?? null;
            model.DeliveryUserId = deliveryUserId ?? null;
            model.BillNumber = billNumber;
            model.WareHouseId = wareHouseId ?? null;
            model.DistrictId = districtId ?? null;
            model.Remark = remark;
            model.StartTime = startTime ?? DateTime.Parse(DateTime.Now.ToString("yyyy-MM-01"));
            model.EndTime = endTime ?? DateTime.Now.AddDays(1);

            model.AuditedStatus = auditedStatus;
            model.SortByAuditedTime = sortByAuditedTime;
            model.ShowReverse = showReverse;
            model.ShowReturn = showReturn;
            model.AlreadyChange = alreadyChange;
            #endregion

            var returns = _returnReservationBillService.GetReturnReservationBillList(curStore?.Id ?? 0,
                 curUser.Id,
                terminalId,
                terminalName,
                businessUserId,
                deliveryUserId,
                billNumber,
                wareHouseId,
                remark,
                //startTime, //?? DateTime.Parse(DateTime.Now.ToString("yyyy-MM-dd")),
                //endTime, //?? DateTime.Now.AddDays(1),
                model.StartTime,
                model.EndTime,
                districtId,
                auditedStatus,
                sortByAuditedTime,
                showReverse,
                alreadyChange,
                false,//未删除单据
                pagenumber,
                pageSize: 30);

            //默认账户
            var defaultAcc = _accountingService.GetDefaultAccounting(curStore?.Id ?? 0, BillTypeEnum.ReturnReservationBill);
            model.DynamicColumns = defaultAcc?.Item4?.OrderBy(s => s.Key).Select(s => s.Value).ToList();

            model.PagingFilteringContext.LoadPagedList(returns);

            #region 查询需要关联其他表的数据

            var allUsers = _userService.GetUsersDictsByIds(curStore.Id, returns.Select(b => b.BusinessUserId).Distinct().ToArray());
            var allTerminal = _terminalService.GetTerminalsByIds(curStore.Id, returns.Select(b => b.TerminalId).Distinct().ToArray());
            //var allWareHouses = _wareHouseService.GetWareHouseByIds(curStore.Id, returns.Select(b => b.WareHouseId).Distinct().ToArray());
            #endregion

            model.Lists = returns.Select(s =>
            {
                var m = s.ToModel<ReturnReservationBillModel>();


                //业务员名称
                m.BusinessUserName = allUsers.Where(aw => aw.Key == m.BusinessUserId).Select(aw => aw.Value).FirstOrDefault();
                //送货员名称
                m.DeliveryUserName = allUsers.Where(aw => aw.Key == m.DeliveryUserId).Select(aw => aw.Value).FirstOrDefault();

                //客户名称
                var terminal = allTerminal.Where(at => at.Id == m.TerminalId).FirstOrDefault();
                m.TerminalName = terminal?.Name;
                m.TerminalPointCode = terminal?.Code;

                //仓库名称
                var warehouse = model.WareHouses.Where(s => s.Value == m.WareHouseId.ToString()).FirstOrDefault();
                m.WareHouseName = warehouse?.Text;

                //应收金额	
                m.ReceivableAmount = (s.ReceivableAmount == 0) ? ((s.OweCash != 0) ? s.OweCash : s.ReturnReservationBillAccountings.Sum(sa => sa.CollectionAmount)) : s.ReceivableAmount;

                //收款账户
                m.ReturnReservationBillAccountings = defaultAcc?.Item4?.OrderBy(sb => sb.Key).Select(sb =>
                {
                    var acc = s.ReturnReservationBillAccountings.Where(a => a?.AccountingOption?.ParentId == sb.Key).FirstOrDefault();
                    return new ReturnReservationBillAccountingModel()
                    {
                        AccountingOptionId = acc?.AccountingOptionId ?? 0,
                        CollectionAmount = acc?.CollectionAmount ?? 0
                    };
                }).ToList();


                //查询退货订单 关联的退货单
                ReturnBill returnBill = _returnBillService.GetReturnBillByReturnReservationBillId(curStore.Id, m.Id);
                if (returnBill != null)
                {
                    m.ReturnBillId = returnBill.Id;
                    m.ReturnBillNumber = returnBill.BillNumber;
                }

                return m;
            }).ToList();

            return View(model);
        }

        /// <summary>
        /// 获取退货订单列表
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
        [AuthCode((int)AccessGranularityEnum.ReturnOrderView)]
        public async Task<JsonResult> AsyncList(int? terminalId, int? businessUserId, int? deliveryUserId, string billNumber = "", int? wareHouseId = null, string remark = "", DateTime? start = null, DateTime? end = null, int? districtId = null, bool? auditedStatus = null, bool? sortByAuditedTime = null, bool? showReverse = null, bool? showReturn = null, bool? alreadyChange = null, int pageIndex = 0, int pageSize = 20)
        {
            return await Task.Run(() =>
            {
                var companySetting = _settingService.LoadSetting<CompanySetting>(curStore.Id);

                var gridModel = _returnReservationBillService.GetReturnReservationBillList(curStore?.Id ?? 0, curUser.Id, terminalId, "", businessUserId, deliveryUserId, billNumber, wareHouseId, remark, start, end, districtId, auditedStatus, sortByAuditedTime, showReverse, alreadyChange, false, pageIndex, pageSize);

                //默认账户
                var defaultAcc = _accountingService.GetDefaultAccounting(curStore?.Id ?? 0, BillTypeEnum.ReturnReservationBill);

                return Json(new
                {
                    Success = true,
                    total = gridModel.TotalCount,
                    rows = gridModel.Select(s =>
                    {

                        var m = s.ToModel<ReturnReservationBillModel>();

                        //业务员名称
                        m.BusinessUserName = _userService.GetUserName(curStore.Id, m.BusinessUserId ?? 0);

                        //客户名称
                        var terminal = _terminalService.GetTerminalById(curStore.Id, m.TerminalId);
                        m.TerminalName = terminal == null ? "" : terminal.Name;
                        m.TerminalPointCode = terminal == null ? "" : terminal.Code;
                        //仓库名称
                        m.WareHouseName = _wareHouseService.GetWareHouseName(curStore.Id, m.WareHouseId);

                        //应收金额	
                        m.ReceivableAmount = (s.ReceivableAmount == 0) ? ((s.OweCash != 0) ? s.OweCash : s.ReturnReservationBillAccountings.Sum(sa => sa.CollectionAmount)) : s.ReceivableAmount;

                        //优惠金额
                        m.PreferentialAmount = s.PreferentialAmount;

                        //订货金额
                        m.SubscribeAmount = 0;

                        //收款账户
                        m.ReturnReservationBillAccountings = defaultAcc?.Item4?.OrderBy(sb => sb.Key).Select(sb =>
                        {
                            var acc = s.ReturnReservationBillAccountings.Where(a => a?.AccountingOption?.ParentId == sb.Key).FirstOrDefault();
                            return new ReturnReservationBillAccountingModel()
                            {
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
        /// 添加退货订单
        /// </summary>
        /// <returns></returns>
        [AuthCode((int)AccessGranularityEnum.ReturnOrderSave)]
        public IActionResult Create()
        {

            ReturnReservationBillModel model = new ReturnReservationBillModel();

            #region 绑定数据源
            var companySetting = _settingService.LoadSetting<CompanySetting>(curStore.Id);
            model = PrepareReturnReservationBillModel(model);

            #endregion

            //付款方式默认挂账
            model.PayTypeId = (int)SaleReservationBillPayType.OnAccount;

            model.PreferentialAmount = 0;
            model.PreferentialEndAmount = 0;
            model.OweCash = 0;
            model.TransactionDate = DateTime.Now;


            //单号
            model.BillNumber = CommonHelper.GetBillNumber(CommonHelper.GetEnumDescription(BillTypeEnum.ReturnReservationBill).Split(',')[1], curStore.Id);
            model.BillBarCode = _mediaService.GenerateBarCodeForBase64(model.BillNumber, 150, 50);
            //制单人
            var mu = _userService.GetUserById(curStore.Id, curUser.Id);
            model.MakeUserName = mu != null ? (mu.UserRealName + " " + DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss")) : "";


            //退货订单默认收款账户：(预收款)24
            // var defaultAccount = _accountingService.GetAccountingOptionByAccountCodeTypeId(curStore?.Id ?? 0, (int)AccountingCodeEnum.AdvancesReceived);
            //默认账户
            var defaultAcc = _accountingService.GetDefaultAccounting(curStore?.Id ?? 0, BillTypeEnum.ReturnReservationBill);
            model.ReturnReservationBillAccountings.Add(new ReturnReservationBillAccountingModel()
            {
                AccountingOptionId = defaultAcc?.Item1?.Id ?? 0,
                CollectionAmount = 0,
                Name = defaultAcc?.Item1?.Name
            });


            //是否开启生产日期配置
            model.IsShowCreateDate = companySetting.OpenBillMakeDate == 0 ? false : true;
            //商品变价参考
            model.VariablePriceCommodity = companySetting.VariablePriceCommodity;
            //单据合计精度
            model.AccuracyRounding = companySetting.AccuracyRounding;
            //交易日期可选范围
            model.AllowSelectionDateRange = companySetting.AllowSelectionDateRange;
            //允许预收款支付成负数
            model.AllowAdvancePaymentsNegative = companySetting.AllowAdvancePaymentsNegative;
            //启用税务功能
            model.EnableTaxRate = companySetting.EnableTaxRate;
            model.TaxRate = companySetting.TaxRate;
            return View(model);
        }

        /// <summary>
        /// 编辑退货订单
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [AuthCode((int)AccessGranularityEnum.ReturnOrderView)]
        public IActionResult Edit(int? id)
        {
            //没有值跳转到列表
            if (id == null)
            {
                return RedirectToAction("List");
            }
            var companySetting = _settingService.LoadSetting<CompanySetting>(curStore.Id);
            var model = new ReturnReservationBillModel();

            var bill = _returnReservationBillService.GetReturnReservationBillById(curStore.Id, id.Value, true);

            //没有值跳转到列表
            if (bill == null)
            {
                return RedirectToAction("List");
            }
            if (bill.StoreId != curStore.Id)
            {
                return RedirectToAction("List");
            }

            model = bill.ToModel<ReturnReservationBillModel>();
            model.BillBarCode = _mediaService.GenerateBarCodeForBase64(bill.BillNumber, 150, 50);

            //默认收款账户金额
            var defaultAcc = _accountingService.GetDefaultAccounting(curStore?.Id ?? 0, BillTypeEnum.ReturnReservationBill);
            model.CollectionAccount = defaultAcc?.Item1?.Id ?? 0;
            model.CollectionAmount = bill.ReturnReservationBillAccountings.Where(sa => sa.AccountingOptionId == defaultAcc?.Item1?.Id).Sum(sa => sa.CollectionAmount);
            model.ReturnReservationBillAccountings = bill.ReturnReservationBillAccountings.Select(s =>
            {
                var m = s.ToAccountModel<ReturnReservationBillAccountingModel>();
                m.Name = _accountingService.GetAccountingOptionName(curStore.Id, s.AccountingOptionId);
                return m;
            }).ToList();

            //取单据项目
            model.Items = bill.Items.Select(s => s.ToModel<ReturnReservationItemModel>()).ToList();

            //获取客户名称
            var terminal = _terminalService.GetTerminalById(curStore.Id, model.TerminalId);
            model.TerminalName = terminal == null ? "" : terminal.Name;
            model.TerminalPointCode = terminal == null ? "" : terminal.Code;

            #region 绑定数据源

            model = PrepareReturnReservationBillModel(model);

            #endregion

            //制单人
            //var mu = _userService.GetUserById(curStore.Id, returnReservation.MakeUserId);
            //model.MakeUserName = mu != null ? (mu.UserRealName + " " + returnReservation.CreatedOnUtc.ToString("yyyy/MM/dd HH:mm:ss")) : "";
            var mu = string.Empty;
            if (bill.MakeUserId > 0)
            {
                mu = _userService.GetUserName(curStore.Id, bill.MakeUserId);
            }
            model.MakeUserName = mu + " " + bill.CreatedOnUtc.ToString("yyyy/MM/dd HH:mm:ss");

            //审核人
            //var au = _userService.GetUserById(curStore.Id, returnReservation.AuditedUserId ?? 0);
            //model.AuditedUserName = au != null ? (au.UserRealName + " " + (returnReservation.AuditedDate.HasValue ? returnReservation.AuditedDate.Value.ToString("yyyy/MM/dd HH:mm:ss") : "")) : "";
            var au = string.Empty;
            if (bill.AuditedUserId != null && bill.AuditedUserId > 0)
            {
                au = _userService.GetUserName(curStore.Id, bill.AuditedUserId ?? 0);
            }
            model.AuditedUserName = au + " " + (bill.AuditedDate.HasValue ? bill.AuditedDate.Value.ToString("yyyy/MM/dd HH:mm:ss") : "");

            model.IsShowCreateDate = companySetting.OpenBillMakeDate == 0 ? false : true;

            //优惠后金额
            model.PreferentialEndAmount = model.SumAmount - model.PreferentialAmount;

            model.PayTypeId = model.PayTypeId;
            //商品变价参考
            model.VariablePriceCommodity = companySetting.VariablePriceCommodity;
            //单据合计精度
            model.AccuracyRounding = companySetting.AccuracyRounding;
            //交易日期可选范围
            model.AllowSelectionDateRange = companySetting.AllowSelectionDateRange;
            //允许预收款支付成负数
            model.AllowAdvancePaymentsNegative = companySetting.AllowAdvancePaymentsNegative;
            //启用税务功能
            model.EnableTaxRate = companySetting.EnableTaxRate;
            model.TaxRate = companySetting.TaxRate;
            return View(model);
        }

        #region 单据项目

        /// <summary>
        /// 异步获取退货订单项目
        /// </summary>
        /// <param name="returnReservationId"></param>
        /// <returns></returns>
        public JsonResult AsyncReturnReservationItems(int returnReservationBillId)
        {

            var gridModel = _returnReservationBillService.GetReturnReservationItemList(returnReservationBillId);

            var allProducts = _productService.GetProductsByIds(curStore.Id, gridModel.Select(pr => pr.ProductId).Distinct().ToArray());
            var allOptions = _specificationAttributeService.GetSpecificationAttributeOptionByIds(curStore.Id, allProducts.GetProductBigStrokeSmallUnitIds());
            var allProductPrices = _productService.GetProductPricesByProductIds(curStore.Id, gridModel.Select(gm => gm.ProductId).Distinct().ToArray());
            var allProductTierPrices = _productService.GetProductTierPriceByProductIds(curStore.Id, gridModel.Select(pr => pr.ProductId).Distinct().ToArray());


            var details = gridModel.Select(o =>
            {
                var m = o.ToModel<ReturnReservationItemModel>();
                var product = allProducts.Where(ap => ap.Id == m.ProductId).FirstOrDefault();
                if (product != null)
                {
                    //这里替换成高级用法
                    m = product.InitBaseModel<ReturnReservationItemModel>(m, o.ReturnReservationBill.WareHouseId, allOptions, allProductPrices, allProductTierPrices, _productService);

                    //商品信息
                    m.BigUnitId = product.BigUnitId;
                    m.StrokeUnitId = product.StrokeUnitId;
                    m.SmallUnitId = product.SmallUnitId;
                    m.IsManufactureDete = product.IsManufactureDete;
                    m.ProductTimes = _productService.GetProductDates(curStore.Id, product.Id, o.ReturnReservationBill.WareHouseId);

                    //税价总计
                    m.TaxPriceAmount = m.Amount;

                    if (o.ReturnReservationBill.TaxAmount > 0 && m.TaxRate > 0)
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
        /// <param name="returnReservationBillId"></param>
        /// <returns></returns>
        [HttpPost]
        [AuthCode((int)AccessGranularityEnum.ReturnOrderSave)]
        public async Task<JsonResult> CreateOrUpdate(ReturnReservationBillUpdateModel data, int? billId)
        {

            try
            {
                if (data != null)
                {

                    var bill = new ReturnReservationBill();

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
                        bill = _returnReservationBillService.GetReturnReservationBillById(curStore.Id, billId.Value);

                        //公共单据验证
                        var commonBillChecking = BillChecking<ReturnReservationBill, ReturnReservationItem>(bill, BillStates.Draft, ((int)AccessGranularityEnum.ReturnOrderSave).ToString());
                        if (commonBillChecking.Value != null)
                        {
                            return commonBillChecking;
                        }
                    }
                    #endregion

                    //业务逻辑
                    var accountings = _accountingService.GetAllAccountingOptions(curStore.Id, 0, true);
                    var dataTo = data.ToEntity<ReturnReservationBillUpdate>();
                    dataTo.Operation = (int)OperationEnum.PC;
                    if (data.Accounting == null)
                    {
                        return Warning("没有默认的付款账号");
                    }
                    dataTo.Accounting = data.Accounting.Select(ac =>
                    {
                        return ac.ToAccountEntity<ReturnReservationBillAccounting>();
                    }).ToList();
                    dataTo.Items = data.Items.Select(it =>
                    {
                        //成本价（此处计算成本价防止web、api成本价未带出,web、api的controller都要单独计算（取消service计算，防止其他service都引用 pruchasebillservice））
                        var item = it.ToEntity<ReturnReservationItem>();
                        item.CostPrice = _purchaseBillService.GetReferenceCostPrice(curStore.Id, item.ProductId, item.UnitId);
                        item.CostAmount = item.CostPrice * item.Quantity;
                        return item;
                    }).ToList();

                    //RedLock
                    var result = await _locker.PerformActionWithLockAsync(LockKey(data),
                        TimeSpan.FromSeconds(30),
                        TimeSpan.FromSeconds(10),
                        TimeSpan.FromSeconds(1),
                        () => _returnReservationBillService.BillCreateOrUpdate(curStore.Id, curUser.Id, billId, bill, dataTo.Accounting, accountings, dataTo, dataTo.Items, new List<ProductStockItem>(), _userService.IsAdmin(curStore.Id, curUser.Id)));
                }
            }
            catch (Exception ex)
            {
                //活动日志
                _userActivityService.InsertActivity("CreateOrUpdate", Resources.Bill_CreateOrUpdateFailed, curUser.Id);
                _notificationService.SuccessNotification(Resources.Bill_CreateOrUpdateFailed);
                return Error(ex.Message);
            }

            _userActivityService.InsertActivity("CreateOrUpdate", Resources.Bill_CreateOrUpdateSuccessful, curUser.Id);
            _notificationService.SuccessNotification(Resources.Bill_CreateOrUpdateSuccessful);
            return Json(new { Success = true, Message = Resources.Bill_CreateOrUpdateSuccessful });
        }

        #endregion

        /// <summary>
        /// 审核
        /// </summary>
        /// <returns></returns>
        [AuthCode((int)AccessGranularityEnum.ReturnOrderApproved)]
        public async Task<JsonResult> Auditing(int? id)
        {
            try
            {

                var bill = new ReturnReservationBill();

                #region 验证

                if (!id.HasValue)
                {
                    return Warning("参数错误.");
                }
                else
                {
                    bill = _returnReservationBillService.GetReturnReservationBillById(curStore.Id, id.Value, true);
                    if (bill.AuditedStatus)
                    {
                        return Warning("单据已审核，请刷新页面.");
                    }
                }

                //公共单据验证
                var commonBillChecking = BillChecking<ReturnReservationBill, ReturnReservationItem>(bill, BillStates.Audited, ((int)AccessGranularityEnum.ReturnOrderApproved).ToString());
                if (commonBillChecking.Value != null)
                {
                    return commonBillChecking;
                }

                if (_wareHouseService.CheckProductInventory(curStore.Id, bill.WareHouseId, bill.Items?.Select(it => it.ProductId).Distinct().ToArray(), out string thisMsg))
                {
                    return Warning("仓库正在盘点中，拒绝操作.");
                }

                #endregion

                //RedLock
                var result = await _locker.PerformActionWithLockAsync(RedLockKey(bill),
                      TimeSpan.FromSeconds(30),
                      TimeSpan.FromSeconds(10),
                      TimeSpan.FromSeconds(1),
                      () => _returnReservationBillService.Auditing(curStore.Id, curUser.Id, bill));
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
        [AuthCode((int)AccessGranularityEnum.ReturnOrdernReverse)]
        public async Task<JsonResult> Reverse(int? id)
        {
            try
            {

                var bill = new ReturnReservationBill() { StoreId = curStore?.Id ?? 0 };

                if (PeriodClosed(DateTime.Now))
                {
                    return Warning("系统当月已经结转，不允许红冲");
                }

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
                    bill = _returnReservationBillService.GetReturnReservationBillById(curStore.Id, id.Value, true);
                    if (bill.ReversedStatus)
                    {
                        return Warning("单据已红冲，请刷新页面.");
                    }
                }

                //公共单据验证
                var commonBillChecking = BillChecking<ReturnReservationBill, ReturnReservationItem>(bill, BillStates.Reversed, ((int)AccessGranularityEnum.ReturnOrdernReverse).ToString());
                if (commonBillChecking.Value != null)
                {
                    return commonBillChecking;
                }

                if (_wareHouseService.CheckProductInventory(curStore.Id, bill.WareHouseId, bill.Items?.Select(it => it.ProductId).Distinct().ToArray(), out string thisMsg))
                {
                    return Warning("仓库正在盘点中，拒绝操作.");
                }

                #endregion

                //RedLock
                var result = await _locker.PerformActionWithLockAsync(RedLockKey(bill),
                      TimeSpan.FromSeconds(30),
                      TimeSpan.FromSeconds(10),
                      TimeSpan.FromSeconds(1),
                      () => _returnReservationBillService.Reverse(curUser.Id, bill));
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
        [AuthCode((int)AccessGranularityEnum.ReturnOrdernReverse)]
        public JsonResult Delete(int? id)
        {
            try
            {
                var bill = new ReturnReservationBill() { StoreId = curStore?.Id ?? 0 };

                #region 验证
                if (!id.HasValue)
                {
                    return Warning("参数错误.");
                }
                else
                {
                    bill = _returnReservationBillService.GetReturnReservationBillById(curStore.Id, id.Value, true);
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
                        var rs = _returnReservationBillService.Delete(curUser.Id, bill);
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
        [AuthCode((int)AccessGranularityEnum.ReturnOrderExport)]
        public FileResult Export(int type, string selectData, int? terminalId, string terminalName, int? businessUserId, int? deliveryUserId, string billNumber = "", int? wareHouseId = null, string remark = "", DateTime? startTime = null, DateTime? endTime = null, int? districtId = null, bool? auditedStatus = null, bool? sortByAuditedTime = null, bool? showReverse = null, bool? showReturn = null, bool? alreadyChange = false)
        {

            #region 查询导出数据

            IList<ReturnReservationBill> returnReservationBills = new List<ReturnReservationBill>();

            if (type == 1)
            {
                if (!string.IsNullOrEmpty(selectData))
                {
                    List<string> ids = selectData.Split(',').ToList();
                    foreach (var id in ids)
                    {
                        ReturnReservationBill returnReservationBill = _returnReservationBillService.GetReturnReservationBillById(curStore.Id, int.Parse(id));
                        if (returnReservationBill != null)
                        {
                            returnReservationBills.Add(returnReservationBill);
                        }
                    }
                }
            }
            else if (type == 2)
            {
                returnReservationBills = _returnReservationBillService.GetReturnReservationBillList(curStore?.Id ?? 0,
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
                           alreadyChange,
                           false,
                           0
                           );
            }

            #endregion

            #region 导出
            var ms = _exportManager.ExportReturnReservationBillToXlsx(returnReservationBills, curStore.Id);
            if (ms != null)
            {
                return File(ms, "application/vnd.ms-excel", "退货订单.xlsx");
            }
            else
            {
                return File(new MemoryStream(), "application/vnd.ms-excel", "退货订单.xlsx");
            }
            #endregion

        }
        [AuthCode((int)AccessGranularityEnum.ReturnOrderPrint)]
        public JsonResult PrintSetting()
        {
            var printTemplates = _printTemplateService.GetAllPrintTemplates(curStore.Id).ToList();
            var printTemplate = printTemplates.Where(a => a.BillType == (int)BillTypeEnum.ReturnReservationBill).FirstOrDefault();
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
        /// <param name="selectData"></param>
        /// <returns></returns>
        [AuthCode((int)AccessGranularityEnum.ReturnOrderPrint)]
        public JsonResult Print(int type, string selectData, int? terminalId, string terminalName, int? businessUserId, int? deliveryUserId, string billNumber = "", int? wareHouseId = null, string remark = "", DateTime? startTime = null, DateTime? endTime = null, int? districtId = null, bool? auditedStatus = null, bool? sortByAuditedTime = null, bool? showReverse = null, bool? showReturn = null, bool? alreadyChange = false)
        {
            try
            {

                bool fg = true;
                string errMsg = string.Empty;

                #region 查询打印数据

                IList<ReturnReservationBill> returnReservationBills = new List<ReturnReservationBill>();
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
                            ReturnReservationBill returnReservationBill = _returnReservationBillService.GetReturnReservationBillById(curStore.Id, int.Parse(id));
                            if (returnReservationBill != null)
                            {
                                returnReservationBills.Add(returnReservationBill);
                            }
                        }
                    }
                }
                else if (type == 2)
                {
                    returnReservationBills = _returnReservationBillService.GetReturnReservationBillList(curStore?.Id ?? 0,
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
                           alreadyChange,
                           false,
                           0
                           );
                }



                #endregion

                #region 修改数据

                if (returnReservationBills != null && returnReservationBills.Count > 0)
                {
                    returnReservationBills?.ToList().ForEach(s => s.PrintNum += 1); //修改单据表打印数
                    _returnReservationBillService.UpdateReturnReservationBill(returnReservationBills);
                }


                //获取打印模板
                var printTemplates = _printTemplateService.GetAllPrintTemplates(curStore.Id).ToList();
                var content = printTemplates.Where(a => a.BillType == (int)BillTypeEnum.ReturnReservationBill).Select(a => a.Content).FirstOrDefault();

                //获取打印设置
                var pCPrintSetting = _settingService.LoadSetting<PCPrintSetting>(_storeContext.CurrentStore.Id);

                //填充打印数据
                foreach (var d in returnReservationBills)
                {

                    StringBuilder sb = new StringBuilder();
                    sb.Append(content);

                    #region theadid
                    //sb.Replace("@商铺名称", curStore.Name);
                    if (pCPrintSetting != null)
                    {
                        sb.Replace("@商铺名称", string.IsNullOrWhiteSpace(pCPrintSetting.StoreName) ? "&nbsp;" : pCPrintSetting.StoreName);
                    }

                    Terminal terminal = _terminalService.GetTerminalById(curStore.Id, d.TerminalId);
                    if (terminal != null)
                    {
                        sb.Replace("@客户名称", terminal.Name);
                        sb.Replace("@客户电话", terminal.BossCall);
                        sb.Replace("@客户地址", terminal.Address);
                        sb.Replace("@老板姓名", terminal.BossName);
                    }
                    User businessUser = _userService.GetUserById(curStore.Id, d.BusinessUserId);
                    if (businessUser != null)
                    {
                        sb.Replace("@业务员", businessUser.UserRealName);
                        sb.Replace("@业务电话", businessUser.MobileNumber);
                    }
                    sb.Replace("@单据编号", d.BillNumber);
                    sb.Replace("@交易日期", d.TransactionDate == null ? "" : ((DateTime)d.TransactionDate).ToString("yyyy/MM/dd HH:mm:ss"));
                    sb.Replace("@打印日期", DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss"));
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

                    User makeUser = _userService.GetUserById(curStore.Id, d.MakeUserId);
                    if (makeUser != null)
                    {
                        sb.Replace("@制单", makeUser.UserRealName);
                    }
                    sb.Replace("@日期", d.CreatedOnUtc.ToString("yyyy/MM/dd HH:mm:ss"));
                    //sb.Replace("@公司地址", "");
                    if (pCPrintSetting != null)
                    {
                        sb.Replace("@公司地址", pCPrintSetting.Address);
                    }

                    //sb.Replace("@订货电话", "");
                    if (pCPrintSetting != null)
                    {
                        sb.Replace("@订货电话", pCPrintSetting.PlaceOrderTelphone);
                    }

                    sb.Replace("@备注", d.Remark);

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
        private ReturnReservationBillListModel PrepareReturnReservationBillListModel(ReturnReservationBillListModel model)
        {
            var isAdmin = _userService.IsAdmin(curStore.Id, curUser.Id);

            //默认收款账户动态列
            var alls = _accountingService.GetAllAccounts(curStore?.Id ?? 0);
            var defaultAccounting = _accountingService
                .GetDefaultAccounting(curStore?.Id ?? 0, BillTypeEnum.ReturnReservationBill, 0, alls?.ToList()).FirstOrDefault();
            model.DynamicColumns.Add(defaultAccounting.Name);

            //业务员
            model.BusinessUsers = BindUserSelection(_userService.BindUserList, curStore, DCMSDefaults.Salesmans, curUser.Id, true, isAdmin);

            //仓库
            model.WareHouses = BindWareHouseSelection(_wareHouseService.BindWareHouseList, curStore, WHAEnum.ReturnReservationBill, curUser.Id);

            //送货员
            model.DeliveryUsers = BindUserSelection(_userService.BindUserList, curStore, DCMSDefaults.Delivers, curUser.Id, true, isAdmin);

            //部门
            model = BindDropDownList<ReturnReservationBillListModel>(model, _branchService.BindBranchsByParentId, curStore?.Id, 0);

            //片区
            model.Districts = BindDistrictSelection(_districtService.BindDistrictList, curStore);

            return model;
        }

        [NonAction]
        private ReturnReservationBillModel PrepareReturnReservationBillModel(ReturnReservationBillModel model)
        {
            model.BillTypeEnumId = (int)BillTypeEnum.ReturnReservationBill;

            //业务员
            model.BusinessUsers = BindUserSelection(_userService.BindUserList, curStore, DCMSDefaults.Salesmans, curUser.Id, true, _userService.IsAdmin(curStore.Id, curUser.Id));

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
            model.WareHouses = BindWareHouseSelection(_wareHouseService.BindWareHouseList, curStore, WHAEnum.ReturnReservationBill, curUser.Id);

            //送货员
            model.DeliveryUsers = BindUserSelection(_userService.BindUserList, curStore, DCMSDefaults.Delivers, curUser.Id, true, _userService.IsAdmin(curStore.Id, curUser.Id));
            model.DeliveryUserId = model.DeliveryUserId == 0 ? null : model.DeliveryUserId;

            //默认售价类型
            model.ReturnReservationDefaultAmounts = BindPricePlanSelection(_productTierPricePlanService.GetAllPricePlan, curStore);
            ////model.DefaultAmountId = (model.DefaultAmountId ?? "");
            ////默认上次售价
            //string lastedPriceDes = CommonHelper.GetEnumDescription(PriceType.LastedPrice);
            //var lastSaleType = model.ReturnReservationDefaultAmounts.Where(sr => sr.Text == lastedPriceDes).FirstOrDefault();
            //model.DefaultAmountId = (model.DefaultAmountId ?? (lastSaleType == null ? "" : lastSaleType.Value));
            //默认
            model.DefaultAmountId = "0_5";
            var companySetting = _settingService.LoadSetting<CompanySetting>(curStore.Id);
            if (companySetting != null)
            {
                if (!string.IsNullOrEmpty(companySetting.DefaultPricePlan))
                {
                    //分析配置 格式："{PricesPlanId}_{PriceTypeId}"
                    var settingDefault = model.ReturnReservationDefaultAmounts?.Where(s => s.Value.EndsWith(companySetting.DefaultPricePlan.ToString())).FirstOrDefault();
                    //这里取默认（不选择时），否启手动下拉选择
                    model.DefaultAmountId = settingDefault?.Value; //如：0_0
                }
            }

            return model;
        }

    }
}