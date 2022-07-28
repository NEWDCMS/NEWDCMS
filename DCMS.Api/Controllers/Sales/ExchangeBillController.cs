using DCMS.Api.Infrastructure.Mapper.Extensions;
using DCMS.Api.Models;
using DCMS.Core;
using DCMS.Core.Caching;
using DCMS.Core.Domain.Configuration;
using DCMS.Core.Domain.Products;
using DCMS.Core.Domain.Sales;
using DCMS.Core.Domain.WareHouses;
using DCMS.Services.Common;
using DCMS.Services.Configuration;
using DCMS.Services.Finances;
using DCMS.Services.Logging;
using DCMS.Services.Products;
using DCMS.Services.Purchases;
using DCMS.Services.Sales;
using DCMS.Services.Settings;
using DCMS.Services.Terminals;
using DCMS.Services.Users;
using DCMS.Services.WareHouses;
using DCMS.ViewModel.Models.Sales;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Swashbuckle.AspNetCore.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Task = System.Threading.Tasks.Task;
using Microsoft.AspNetCore.Authorization;

namespace DCMS.Api.Controllers
{
    /// <summary>
    /// 换货单
    /// </summary>
   [Authorize]
    [Route("api/v{version:apiVersion}/dcms/sales")]
    public class ExchangeBillController : BaseAPIController
    {
        private readonly ISaleReservationBillService  _saleReservationBillService;
        private readonly IWareHouseService _wareHouseService;
        private readonly IProductService _productService;
        private readonly IStockService _stockService;
        private readonly ISettingService _settingService;
        private readonly ISpecificationAttributeService _specificationAttributeService;
        private readonly IProductTierPricePlanService _productTierPricePlanService;
        private readonly IUserService _userService;
        private readonly ITerminalService _terminalService;
        private readonly IAccountingService _accountingService;
        private readonly ICostContractBillService _costContractBillService;
        private readonly IRedLocker _locker;
        private readonly IUserActivityService _userActivityService;
        private readonly ICommonBillService _commonBillService;
        private readonly IDispatchBillService _dispatchBillService;
        private readonly IPurchaseBillService _purchaseBillService;
        private readonly ISaleBillService _saleBillService;
        private readonly IExchangeBillService _exchangeBillService;

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="exchangeBillService"></param>
        /// <param name="wareHouseService"></param>
        /// <param name="productService"></param>
        /// <param name="stockService"></param>
        /// <param name="settingService"></param>
        /// <param name="specificationAttributeService"></param>
        /// <param name="productTierPricePlanService"></param>
        /// <param name="userService"></param>
        /// <param name="terminalService"></param>
        /// <param name="accountingService"></param>
        /// <param name="costContractBillService"></param>
        /// <param name="locker"></param>
        /// <param name="userActivityService"></param>
        /// <param name="commonBillService"></param>
        /// <param name="dispatchBillService"></param>
        /// <param name="purchaseBillService"></param>
        /// <param name="saleBillService"></param>
        /// <param name="logger"></param>
        public ExchangeBillController(
            ISaleReservationBillService saleReservationBillService,
            IWareHouseService wareHouseService,
            IProductService productService,
            IStockService stockService,
            ISettingService settingService,
            ISpecificationAttributeService specificationAttributeService,
            IProductTierPricePlanService productTierPricePlanService,
            IUserService userService,
            ITerminalService terminalService,
            IAccountingService accountingService,
            IExchangeBillService exchangeBillService,
            ICostContractBillService costContractBillService,
            IRedLocker locker,
            IUserActivityService userActivityService,
            ICommonBillService commonBillService,
            IDispatchBillService dispatchBillService,
            IPurchaseBillService purchaseBillService,
            ISaleBillService saleBillService, 
            ILogger<BaseAPIController> logger) : base(logger)
        {
            _saleReservationBillService = saleReservationBillService;
            _wareHouseService = wareHouseService;
            _productService = productService;
            _stockService = stockService;
            _specificationAttributeService = specificationAttributeService;
            _productTierPricePlanService = productTierPricePlanService;
            _settingService = settingService;
            _userService = userService;
            _terminalService = terminalService;
            _accountingService = accountingService;
            _exchangeBillService = exchangeBillService;
            _costContractBillService = costContractBillService;
            _locker = locker;
            _userActivityService = userActivityService;
            _commonBillService = commonBillService;
            _dispatchBillService = dispatchBillService;
            _purchaseBillService = purchaseBillService;
            _saleBillService = saleBillService;
        }

        /// <summary>
        /// 获取换货单列表
        /// </summary>
        /// <param name="store"></param>
        /// <param name="terminalId"></param>
        /// <param name="terminalName"></param>
        /// <param name="businessUserId"></param>
        /// <param name="deliveryUserId"></param>
        /// <param name="billNumber"></param>
        /// <param name="wareHouseId"></param>
        /// <param name="remark"></param>
        /// <param name="districtId"></param>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <param name="auditedStatus"></param>
        /// <param name="sortByAuditedTime"></param>
        /// <param name="showReverse"></param>
        /// <param name="showReturn"></param>
        /// <param name="alreadyChange"></param>
        /// <param name="pagenumber"></param>
        /// <returns></returns>
        [HttpGet("exchangeBill/getbills/{store}/{terminalId}/{businessUserId}/{wareHouseId}")]
        [SwaggerOperation("getbills")]
        public async Task<APIResult<IList<ExchangeBillModel>>> AsyncList(int? store, int? makeuserId, int? terminalId, string terminalName, int? businessUserId, int? deliveryUserId, string billNumber, int? wareHouseId, string remark, int? districtId, DateTime? startTime = null, DateTime? endTime = null, bool? auditedStatus = null, bool? sortByAuditedTime = null, bool? showReverse = null, bool? showReturn = null, bool? alreadyChange = null, int pagenumber = 0, int pageSize = 50)
        {
            if (!store.HasValue || store.Value == 0)
                return this.Error<ExchangeBillModel>(Resources.ParameterError);

            return await Task.Run(() =>
            {
                try
                {
                    var exchanges = _exchangeBillService.GetExchangeBillList(store ?? 0,
                        makeuserId ?? 0,
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
                        pagenumber,
                        pageSize: 30);

                    // 查询需要关联其他表的数据
                    List<int> userIds = new List<int>();

                    userIds.AddRange(exchanges.Select(b => b.BusinessUserId).Distinct().ToArray());
                    userIds.AddRange(exchanges.Select(b => b.DeliveryUserId).Distinct().ToArray());

                    var allUsers = _userService.GetUsersDictsByIds(store ?? 0, userIds.Distinct().ToArray());

                    var allTerminal = _terminalService.GetTerminalsByIds(store ?? 0, exchanges.Select(b => b.TerminalId).Distinct().ToArray(), true);
                    var allWareHouses = _wareHouseService.GetWareHouseByIds(store ?? 0, exchanges.Select(b => b.WareHouseId).Distinct().ToArray(), true);

                    //商品价格
                    List<int> productIds = new List<int>();

                    //换货单中的商品
                    if (exchanges != null && exchanges.Count > 0)
                    {
                        exchanges.ToList().ForEach(sale =>
                        {
                            if (sale != null && sale.Items != null && sale.Items.Count > 0)
                            {
                                productIds.AddRange(sale.Items.Select(it => it.ProductId).Distinct().ToArray());
                            }
                        });
                    }
                    productIds = productIds.Distinct().ToList();

                    //商品价格
                    var allProductPrices = _productService.GetProductPricesByProductIds(store ?? 0, productIds.ToArray(), true);

                    var allProducts = _productService.GetProductsByIds(store ?? 0, productIds.ToArray());

                    var results = exchanges.Select(s =>
                    {
                        var m = s.ToModel<ExchangeBillModel>();

                        //业务员名称
                        m.BusinessUserName = allUsers.Where(aw => aw.Key == m.BusinessUserId).Select(aw => aw.Value).FirstOrDefault();
                        //送货员名称
                        m.DeliveryUserName = allUsers.Where(aw => aw.Key == m.DeliveryUserId).Select(aw => aw.Value).FirstOrDefault();

                        //客户名称
                        var terminal = allTerminal.FirstOrDefault(at => at.Id == m.TerminalId);
                        m.TerminalName = terminal == null ? "" : terminal.Name;
                        m.TerminalPointCode = terminal == null ? "" : terminal.Code;
                        //仓库名称
                        var warehouse = allWareHouses.FirstOrDefault(aw => aw.Id == m.WareHouseId);
                        m.WareHouseName = warehouse == null ? "" : warehouse.Name;

                        //是否变价
                        bool exchangeChangePrice = false;
                        if (s.Items != null && s.Items.Count > 0)
                        {
                            s.Items.ToList().ForEach(it =>
                            {
                                ProductPrice productPrice = allProductPrices.FirstOrDefault(ap => ap.ProductId == it.ProductId && ap.UnitId == it.UnitId);
                                if (productPrice != null && it.Price != productPrice.TradePrice)
                                {
                                    exchangeChangePrice = true;
                                }
                            });
                        }
                        m.SaleReservationChangePrice = exchangeChangePrice;


                        m.Items?.ToList()?.ForEach(item =>
                        {
                            var p = allProducts.Where(s => s.Id == item.ProductId).FirstOrDefault();
                            item.ProductName = p?.Name;
                            item.UnitName = _specificationAttributeService.GetSpecificationAttributeOptionName(store, item.UnitId);
                        });

                        return m;
                    }).ToList();

                    return this.Successful(Resources.Successful, results);

                }
                catch (Exception ex)
                {
                    return this.Error<ExchangeBillModel>(ex.Message);
                }
            });
        }

        /// <summary>
        /// 获取换货单
        /// </summary>
        /// <param name="store"></param>
        /// <param name="billId"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        [HttpGet("exchangeBill/getExchangeBill/{store}/{billId}/{userId}")]
        [SwaggerOperation("getExchangeBill")]
        //[ValidateActionParameters]
        //[AuthBaseFilter]
        public async Task<APIResult<ExchangeBillModel>> GetExchangeBill(int? store, int? billId, int? userId = 0)
        {
            if (!store.HasValue || store.Value == 0)
                return this.Error3<ExchangeBillModel>(Resources.ParameterError);

            return await Task.Run(() =>
            {

                var model = new ExchangeBillModel();

                try
                {
                    var companySetting = _settingService.LoadSetting<CompanySetting>(store ?? 0);

                    var exchange = _exchangeBillService.GetExchangeBillById(store ?? 0, billId.Value, true);

                    model = exchange.ToModel<ExchangeBillModel>();

                    //取单据项目
                    var allProductPrices = _productService.GetProductPricesByProductIds(store, exchange.Items.Select(p => p.ProductId).ToArray());
                    var allProducts = _productService.GetProductsByIds(store??0, exchange.Items.Select(pr => pr.ProductId).Distinct().ToArray());
                    var allOptions = _specificationAttributeService.GetSpecificationAttributeOptionByIds(store, allProducts.GetProductBigStrokeSmallUnitIds());
                    var allProductTierPrices = _productService.GetProductTierPriceByProductIds(store, exchange.Items.Select(pr => pr.ProductId).Distinct().ToArray());
                    model.Items = exchange.Items.Select(o =>
                    {
                        var m = o.ToModel<ExchangeItemModel>();
                        var product = allProducts.Where(ap => ap.Id == m.ProductId).FirstOrDefault();
                        if (product != null)
                        {
                            //这里替换成高级用法
                            m = product.InitBaseModel<ExchangeItemModel>(m, o.ExchangeBill.WareHouseId, allOptions, allProductPrices, allProductTierPrices, _productService);

                            m.UnitId = o.UnitId;
                            m.UnitName = o.UnitId == 0 ? m.Units.Keys.Select(k => k).ToArray()[2] : m.Units.Where(q => q.Value == o.UnitId).Select(q => q.Key)?.FirstOrDefault();

                            m.SaleProductTypeName = CommonHelper.GetEnumDescription<SaleProductTypeEnum>(m.SaleProductTypeId ?? 0);
                        }

                        return m;

                    }).ToList();

                    //获取客户名称
                    var terminal = _terminalService.GetTerminalById(store ?? 0, model.TerminalId);
                    model.TerminalName = terminal == null ? "" : terminal.Name;
                    model.TerminalPointCode = terminal == null ? "" : terminal.Code;

                    model.BillTypeEnumId = (int)BillTypeEnum.ExchangeBill;
                    model.BusinessUserName =_userService.GetUserName(store, model.BusinessUserId??0);

                    //仓库
                    model.WareHouseName = _wareHouseService.GetWareHouseName(store, model.WareHouseId);
                    model.PayTypeId = model.PayTypeId;

                    //默认
                    model.DefaultAmountId = "0_5";

                    //用户可用 预收款金额
                    decimal useAdvanceReceiptAmount = 0;
                    //预收款
                    model.AdvanceReceiptAmount = useAdvanceReceiptAmount;
                    //制单人
                    var mu = _userService.GetUserName(store ?? 0, exchange.MakeUserId);
                    model.MakeUserName = mu + " " + exchange.CreatedOnUtc.ToString("yyyy/MM/dd HH:mm:ss");
                    //审核人
                    var au = _userService.GetUserName(store ?? 0, exchange.AuditedUserId ?? 0);
                    model.AuditedUserName = au + " " + (exchange.AuditedDate.HasValue ? exchange.AuditedDate.Value.ToString("yyyy/MM/dd HH:mm:ss") : "");
                    //是否显示生产日期
                    model.IsShowCreateDate = companySetting.OpenBillMakeDate == 0 ? false : true;
                    //优惠后金额
                    model.PreferentialEndAmount = 0;
                    //商品变价参考
                    model.VariablePriceCommodity = companySetting.VariablePriceCommodity;
                    //单据合计精度
                    model.AccuracyRounding = companySetting.AccuracyRounding;
                    //允许预收款支付成负数
                    model.AllowAdvancePaymentsNegative = companySetting.AllowAdvancePaymentsNegative;
                    //显示订单占用库存
                    model.APPShowOrderStock = companySetting.APPShowOrderStock;

                    //启用税务功能
                    model.EnableTaxRate = companySetting.EnableTaxRate;
                    model.TaxRate = companySetting.TaxRate;

                    return this.Successful3(Resources.Successful, model);
                }
                catch (Exception ex)
                {
                    return this.Error3<ExchangeBillModel>(ex.Message);
                }
            });
        }


        /// <summary>
        /// 换货单创建/更新
        /// </summary>
        /// <param name="data"></param>
        /// <param name="billId"></param>
        /// <param name="store"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        [HttpPost("exchangeBill/createorupdate/{store}/{userId}/{billId}")]
        [SwaggerOperation("createorupdate")]
        public async Task<APIResult<dynamic>> CreateOrUpdate(ExchangeBillUpdateModel data, int? billId, int? store, int? userId)
        {
            if (!store.HasValue || store.Value == 0)
                return this.Error(Resources.ParameterError);

            return await Task.Run(async () =>
            {

                try
                {
                    var bill = new ExchangeBill();

                    if (data == null || data.Items == null)
                        return this.Warning("请录入数据.");

                    if (PeriodLocked(DateTime.Now, store ?? 0))
                        return this.Warning("锁账期间,禁止业务操作.");

                    if (PeriodClosed(DateTime.Now, store ?? 0))
                        return this.Warning("会计期间已结账,禁止业务操作.");


                    if (billId.HasValue && billId.Value != 0)
                    {
                        bill = _exchangeBillService.GetExchangeBillById(store ?? 0, billId.Value, true);

                        //公共单据验证
                        var commonBillChecking = BillChecking<ExchangeBill, ExchangeItem>(bill, store ?? 0, BillStates.Draft);
                        if (commonBillChecking.Data != null)
                            return commonBillChecking;
                    }

                    #region 验证盘点
                    if (data.Items.Any())
                    {
                        if (_wareHouseService.CheckProductInventory(store ?? 0, data.WareHouseId, bill.Items?.Select(it => it.ProductId).Distinct().ToArray(), out string thisMsg))
                        {
                            return this.Warning(thisMsg);
                        }
                    }
                    #endregion

                    #region 验证库存
                    IList<Product> allProducts = new List<Product>();
                    //当前数据
                    var productStockItemNews = new List<ProductStockItem>();
                    if (data.Items != null && data.Items.Count > 0)
                    {
                        allProducts = _productService
                        .GetProductsByIds(store ?? 0, data.Items.Select(pr => pr.ProductId)
                        .Distinct()
                        .ToArray());

                        foreach (ExchangeItemModel item in data.Items)
                        {
                            if (item.ProductId != 0)
                            {
                                var product = allProducts.Where(ap => ap.Id == item.ProductId).FirstOrDefault();
                                if (product != null)
                                {
                                    var productStockItem = productStockItemNews.Where(a => a.ProductId == item.ProductId).FirstOrDefault();
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

                    //验证库存
                    if (!_stockService.CheckStockQty(_productService, _specificationAttributeService, store ?? 0, data.WareHouseId, productStockItemNews, out string errMsg))
                    {
                        return this.Warning(errMsg);
                    }
                    #endregion

                    //业务逻辑
                    var dataTo = data.ToEntity<ExchangeBillUpdate>();
                    dataTo.Operation = (int)OperationEnum.APP;
                    dataTo.Items = data.Items.Select(it =>
                    {
                        //成本价（此处计算成本价防止web、api成本价未带出,web、api的controller都要单独计算（取消service计算，防止其他service都引用 pruchasebillservice））
                        var item = it.ToEntity<ExchangeItem>();
                        item.CostPrice = _purchaseBillService.GetReferenceCostPrice(store ?? 0, item.ProductId, item.UnitId);
                        item.CostAmount = item.CostPrice * item.Quantity;
                        return item;
                    }).ToList();

                    //RedLock
                    var results = await _locker.PerformActionWithLockAsync(this.LockKey(data, store ?? 0, userId ?? 0),
                        TimeSpan.FromSeconds(30),
                        TimeSpan.FromSeconds(10),
                        TimeSpan.FromSeconds(1),
                        () => _exchangeBillService.BillCreateOrUpdate(store ?? 0,
                        userId ?? 0,
                        billId,
                        bill,
                        null,
                        dataTo,
                        dataTo.Items,
                        productStockItemNews,
                       _userService.IsAdmin(store ?? 0, userId ?? 0)));

                    return this.Successful(Resources.Successful, results);

                }
                catch (Exception ex)
                {
                    _userActivityService.InsertActivity("CreateOrUpdate", Resources.Bill_CreateOrUpdateFailed, userId ?? 0);
                    return this.Error(ex.Message);
                }

            });
        }

        /// <summary>
        /// 审核
        /// </summary>
        /// <param name="store"></param>
        /// <param name="userId"></param>
        /// <param name="billId"></param>
        /// <returns></returns>
        [HttpGet("exchangeBill/auditing/{store}/{userId}/{billId}")]
        [SwaggerOperation("auditing")]
        public async Task<APIResult<dynamic>> Auditing(int? store, int? userId, int? billId)
        {
            if (!store.HasValue || store.Value == 0)
                return this.Error(Resources.ParameterError);

            return await Task.Run(async () =>
            {
                try
                {
                    var bill = new ExchangeBill();

                    // 验证
                    if (!billId.HasValue)
                        return this.Warning("参数错误.");
                    else
                    { 
                        bill = _exchangeBillService.GetExchangeBillById(store ?? 0, billId.Value, true);
                        if (bill.AuditedStatus)
                        {
                            return Warning("单据已审核，请刷新页面.");
                        }
                    }

                    //公共单据验证
                    var commonBillChecking = BillChecking<ExchangeBill, ExchangeItem>(bill, store ?? 0, BillStates.Audited);
                    if (commonBillChecking.Data != null)
                        return commonBillChecking;

                    if (_wareHouseService.CheckProductInventory(store ?? 0, bill.WareHouseId, bill.Items?.Select(it => it.ProductId).Distinct().ToArray(), out string thisMsg))
                        return this.Warning("仓库正在盘点中，拒绝操作.");

                    #region 验证库存
                    IList<Product> allProducts = new List<Product>();
                    //当前数据
                    List<ProductStockItem> productStockItemNews = new List<ProductStockItem>();
                    if (bill.Items != null && bill.Items.Count > 0)
                    {
                        allProducts = _productService.GetProductsByIds(store ?? 0, bill.Items.Select(pr => pr.ProductId).Distinct().ToArray());
                        var allOptions = _specificationAttributeService.GetSpecificationAttributeOptionByIds(store ?? 0, allProducts.GetProductBigStrokeSmallUnitIds());

                        foreach (ExchangeItem item in bill.Items)
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

                    if (!_stockService.CheckStockQty(_productService, _specificationAttributeService, store ?? 0, bill.WareHouseId, productStockItemNews, out string errMsg2))
                    {
                        return this.Warning(errMsg2);
                    }
                    #endregion

                    //RedLock
                    var results = await _locker.PerformActionWithLockAsync(RedLockKey(bill, store ?? 0, userId ?? 0),
                          TimeSpan.FromSeconds(30),
                          TimeSpan.FromSeconds(10),
                          TimeSpan.FromSeconds(1),
                          () => _exchangeBillService.Auditing(store ?? 0, userId ?? 0, bill));

                    return this.Successful("审核成功", results);
                }
                catch (Exception ex)
                {
                    _userActivityService.InsertActivity("Auditing", "单据审核失败", userId);
                    return this.Error(ex.Message);
                }

            });
        }

        /// <summary>
        /// 红冲
        /// </summary>
        /// <param name="store"></param>
        /// <param name="userId"></param>
        /// <param name="billId"></param>
        /// <returns></returns>
        [HttpGet("exchangeBill/reverse/{store}/{userId}/{billId}")]
        [SwaggerOperation("reverse")]
        //[AuthBaseFilter]
        public async Task<APIResult<dynamic>> Reverse(int? store, int? userId, int? billId, string remark = "")
        {
            if (!store.HasValue || store.Value == 0)
                return this.Error(Resources.ParameterError);

            return await Task.Run(async () =>
            {
                try
                {

                    var bill = new ExchangeBill() { StoreId = store ?? 0 };

                    if (this.PeriodClosed(DateTime.Now, store ?? 0))
                        return this.Warning("系统当月已经结转，不允许红冲");

                    if (!billId.HasValue)
                        return this.Warning("参数错误.");
                    else
                    { 
                        bill = _exchangeBillService.GetExchangeBillById(store ?? 0, billId.Value, true);
                        bill.Remark = remark;
                        if (bill.ReversedStatus)
                        {
                            return Warning("单据已红冲，请刷新页面.");
                        }
                    }


                    //公共单据验证
                    var commonBillChecking = BillChecking<ExchangeBill, ExchangeItem>(bill, store ?? 0, BillStates.Reversed);
                    if (commonBillChecking.Data != null)
                        return commonBillChecking;

                    if (_wareHouseService.CheckProductInventory(store ?? 0, bill.WareHouseId, bill.Items?.Select(it => it.ProductId).Distinct().ToArray(), out string thisMsg))
                        return this.Warning("仓库正在盘点中，拒绝操作.");


                    // 验证送货签收
                    if (_dispatchBillService.CheckSign(bill))
                        return this.Warning("当前换货单已经签收，不能红冲.");

                    //RedLock
                    var result = await _locker.PerformActionWithLockAsync(RedLockKey(bill, store ?? 0, userId ?? 0),
                          TimeSpan.FromSeconds(30),
                          TimeSpan.FromSeconds(10),
                          TimeSpan.FromSeconds(1),
                          () => _exchangeBillService.Reverse(userId ?? 0, bill));

                    return this.Successful("红冲成功", result);
                }
                catch (Exception ex)
                {
                    _userActivityService.InsertActivity("Reverse", "单据红冲失败", userId);
                    return this.Error(ex.Message);
                }
            });
        }


    }
}