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
    /// 销售订单
    /// </summary>
   [Authorize]
    [Route("api/v{version:apiVersion}/dcms/sales")]
    public class SaleReservationBillController : BaseAPIController
    {
        private readonly ISaleReservationBillService _saleReservationBillService;
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


        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="saleReservationBillService"></param>
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
        public SaleReservationBillController(
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
           
            ICostContractBillService costContractBillService,
            IRedLocker locker,
            IUserActivityService userActivityService,
            ICommonBillService commonBillService,
            IDispatchBillService dispatchBillService,
            IPurchaseBillService purchaseBillService,
            ISaleBillService saleBillService
            , ILogger<BaseAPIController> logger) : base(logger)
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
            
            _costContractBillService = costContractBillService;
            _locker = locker;
            _userActivityService = userActivityService;
            _commonBillService = commonBillService;
            _dispatchBillService = dispatchBillService;
            _purchaseBillService = purchaseBillService;
            _saleBillService = saleBillService;
        }

        /// <summary>
        /// 获取销售订单列表
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
        [HttpGet("salereservationbill/getbills/{store}/{terminalId}/{businessUserId}/{wareHouseId}")]
        [SwaggerOperation("getbills")]
        //[ValidateActionParameters]
        //[AuthBaseFilter]
        public async Task<APIResult<IList<SaleReservationBillModel>>> AsyncList(int? store, int? makeuserId, int? terminalId, string terminalName, int? businessUserId, int? deliveryUserId, string billNumber, int? wareHouseId, string remark, int? districtId, DateTime? startTime = null, DateTime? endTime = null, bool? auditedStatus = null, bool? sortByAuditedTime = null, bool? showReverse = null, bool? showReturn = null, bool? alreadyChange = null, int pagenumber = 0, int pageSize = 50)
        {
            if (!store.HasValue || store.Value == 0)
                return this.Error<SaleReservationBillModel>(Resources.ParameterError);

            return await Task.Run(() =>
            {
                try
                {
                    var saleReservations = _saleReservationBillService.GetSaleReservationBillList(store ?? 0,
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

                    //默认收款账户动态列
                    var defaultAcc = _accountingService.GetDefaultAccounting(store ?? 0, BillTypeEnum.SaleReservationBill);

                    // 查询需要关联其他表的数据
                    List<int> userIds = new List<int>();

                    userIds.AddRange(saleReservations.Select(b => b.BusinessUserId).Distinct().ToArray());
                    userIds.AddRange(saleReservations.Select(b => b.DeliveryUserId).Distinct().ToArray());

                    var allUsers = _userService.GetUsersDictsByIds(store ?? 0, userIds.Distinct().ToArray());

                    var allTerminal = _terminalService.GetTerminalsByIds(store ?? 0, saleReservations.Select(b => b.TerminalId).Distinct().ToArray(), true);
                    var allWareHouses = _wareHouseService.GetWareHouseByIds(store ?? 0, saleReservations.Select(b => b.WareHouseId).Distinct().ToArray(), true);

                    //商品价格
                    List<int> productIds = new List<int>();

                    //所有关联销售单
                    var allSaleBills = _saleBillService.GetSaleBillsBySaleReservationIds(store ?? 0, saleReservations.Select(b => b.Id).Distinct().ToArray());
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
                    var allProductPrices = _productService.GetProductPricesByProductIds(store ?? 0, productIds.ToArray(), true);

                    var allProducts = _productService.GetProductsByIds(store ?? 0, productIds.ToArray());

                    var results = saleReservations.Select(s =>
                    {
                        var m = s.ToModel<SaleReservationBillModel>();

                        //m.Items = s.Items.Select(s => s.ToModel<SaleReservationItemModel>()).ToList();

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

                        //应收金额	
                        m.ReceivableAmount = (s.ReceivableAmount == 0) ? ((s.OweCash != 0) ? s.OweCash : s.SaleReservationBillAccountings.Sum(sa => sa.CollectionAmount)) : s.ReceivableAmount;

                        //收款账户(只有允许预收款)
                        m.SaleReservationBillAccountings = defaultAcc?.Item4?.OrderBy(sb => sb.Key).Select(sb =>
                        {
                            var acc = s.SaleReservationBillAccountings.Where(a => a?.AccountingOption?.ParentId == sb.Key).FirstOrDefault();
                            return new SaleReservationBillAccountingModel()
                            {
                                Name = acc?.AccountingOption?.Name,
                                AccountingOptionName = acc?.AccountingOption?.Name,
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
                                    var productPrice = allProductPrices.FirstOrDefault(ap => ap.ProductId == it.ProductId && ap.UnitId == it.UnitId);
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


                        m.Items?.ToList()?.ForEach(item =>
                        {
                            var p = allProducts.Where(s => s.Id == item.ProductId).FirstOrDefault();
                            item.ProductName = p?.Name;
                            item.UnitName = _specificationAttributeService.GetSpecificationAttributeOptionName(store, item.UnitId);
                            item.BigUnitId = p?.BigUnitId;
                            item.SmallUnitId = p?.SmallUnitId ?? 0;
                        });

                        return m;
                    }).ToList();

                    return this.Successful(Resources.Successful, results);

                }
                catch (Exception ex)
                {
                    return this.Error<SaleReservationBillModel>(ex.Message);
                }
            });
        }

        /// <summary>
        /// 获取销售订单
        /// </summary>
        /// <param name="store"></param>
        /// <param name="billId"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        [HttpGet("salereservationbill/getSaleReservationBill/{store}/{billId}/{userId}")]
        [SwaggerOperation("getSaleReservationBill")]
        //[ValidateActionParameters]
        //[AuthBaseFilter]
        public async Task<APIResult<SaleReservationBillModel>> GetSaleReservationBill(int? store, int? billId, int? userId = 0)
        {
            if (!store.HasValue || store.Value == 0)
                return this.Error3<SaleReservationBillModel>(Resources.ParameterError);

            return await Task.Run(() =>
            {

                var model = new SaleReservationBillModel();

                try
                {
                    var companySetting = _settingService.LoadSetting<CompanySetting>(store ?? 0);

                    var saleReservation = _saleReservationBillService.GetSaleReservationBillById(store ?? 0, billId.Value, true);

                    model = saleReservation.ToModel<SaleReservationBillModel>();

                    //获取默认收款账户
                    var defaultAcc = _accountingService.GetDefaultAccounting(store ?? 0, BillTypeEnum.SaleReservationBill);
                    model.CollectionAccount = defaultAcc?.Item1?.Id ?? 0;
                    model.CollectionAmount = saleReservation.SaleReservationBillAccountings.Where(sa => sa.AccountingOptionId == defaultAcc?.Item1?.Id).Sum(sa => sa.CollectionAmount);
                    model.SaleReservationBillAccountings = saleReservation.SaleReservationBillAccountings.Select(s =>
                    {
                        var m = s.ToAccountModel<SaleReservationBillAccountingModel>();

                        m.AccountingOptionName = _accountingService.GetAccountingOptionName(store, s.AccountingOptionId);
                        m.Name = _accountingService.GetAccountingOptionName(store ?? 0, s.AccountingOptionId);
                        return m;
                    }).ToList();

                    //取单据项目
                    //var gridModel = _saleReservationBillService.GetSaleReservationItemList(billId.Value);
                    var allProductPrices = _productService.GetProductPricesByProductIds(store, saleReservation.Items.Select(p => p.ProductId).ToArray());
                    var allProducts = _productService.GetProductsByIds(store??0, saleReservation.Items.Select(pr => pr.ProductId).Distinct().ToArray());
                    var allOptions = _specificationAttributeService.GetSpecificationAttributeOptionByIds(store, allProducts.GetProductBigStrokeSmallUnitIds());
                    var allProductTierPrices = _productService.GetProductTierPriceByProductIds(store, saleReservation.Items.Select(pr => pr.ProductId).Distinct().ToArray());
                    model.Items = saleReservation.Items.Select(o =>
                    {
                        var m = o.ToModel<SaleReservationItemModel>();
                        var product = allProducts.Where(ap => ap.Id == m.ProductId).FirstOrDefault();
                        if (product != null)
                        {
                            //这里替换成高级用法
                            m = product.InitBaseModel<SaleReservationItemModel>(m, o.SaleReservationBill.WareHouseId, allOptions, allProductPrices, allProductTierPrices, _productService);

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

                    model.BillTypeEnumId = (int)BillTypeEnum.SaleReservationBill;
                    model.BusinessUserName =_userService.GetUserName(store, model.BusinessUserId??0);

                    //仓库
                    model.WareHouseName = _wareHouseService.GetWareHouseName(store, model.WareHouseId);
                    model.PayTypeId = model.PayTypeId;


                    //默认售价类型 
                    model.SaleReservationBillDefaultAmounts = BindPricePlanSelection(_productTierPricePlanService.GetAllPricePlan, store ?? 0);
                    //默认
                    model.DefaultAmountId = "0_5";

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
                    if (store.Value > 0 && model.TerminalId > 0)
                    {
                        useAdvanceReceiptAmount = _commonBillService.CalcTerminalBalance(store ?? 0, model.TerminalId).AdvanceAmountBalance;
                    }

                    //预收款
                    model.AdvanceReceiptAmount = useAdvanceReceiptAmount;

                    //制单人
                    var mu = _userService.GetUserName(store ?? 0, saleReservation.MakeUserId);
                    model.MakeUserName = mu + " " + saleReservation.CreatedOnUtc.ToString("yyyy/MM/dd HH:mm:ss");

                    //审核人
                    var au = _userService.GetUserName(store ?? 0, saleReservation.AuditedUserId ?? 0);
                    model.AuditedUserName = au + " " + (saleReservation.AuditedDate.HasValue ? saleReservation.AuditedDate.Value.ToString("yyyy/MM/dd HH:mm:ss") : "");

                    //是否显示生产日期
                    model.IsShowCreateDate = companySetting.OpenBillMakeDate == 0 ? false : true;
                    //优惠后金额
                    model.PreferentialEndAmount = model.ReceivableAmount - model.PreferentialAmount;
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
                    return this.Error3<SaleReservationBillModel>(ex.Message);
                }
            });
        }


        /// <summary>
        /// 销售订单创建/更新
        /// </summary>
        /// <param name="data"></param>
        /// <param name="billId"></param>
        /// <param name="store"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        [HttpPost("salereservationbill/createorupdate/{store}/{userId}/{billId}")]
        [SwaggerOperation("createorupdate")]
        //[ValidateActionParameters]
        //[AuthBaseFilter]
        public async Task<APIResult<dynamic>> CreateOrUpdate(SaleReservationBillUpdateModel data, int? billId, int? store, int? userId)
        {
            if (!store.HasValue || store.Value == 0)
                return this.Error(Resources.ParameterError);

            return await Task.Run(async () =>
            {

                try
                {
                    var bill = new SaleReservationBill();

                    if (data == null || data.Items == null)
                        return this.Warning("请录入数据.");

                    if (PeriodLocked(DateTime.Now, store ?? 0))
                        return this.Warning("锁账期间,禁止业务操作.");

                    if (PeriodClosed(DateTime.Now, store ?? 0))
                        return this.Warning("会计期间已结账,禁止业务操作.");


                    if (billId.HasValue && billId.Value != 0)
                    {
                        bill = _saleReservationBillService.GetSaleReservationBillById(store ?? 0, billId.Value, true);

                        //公共单据验证
                        var commonBillChecking = BillChecking<SaleReservationBill, SaleReservationItem>(bill, store ?? 0, BillStates.Draft);
                        if (commonBillChecking.Data != null)
                            return commonBillChecking;
                    }

                    #region 验证赠品
                    if (data != null && data.Items != null && data.Items.Count > 0)
                    {
                        List<SaleReservationItem> items = data.Items.Where(it => it.SaleProductTypeId > 0).Select(it =>
                        {
                            SaleReservationItem saleItem = it.ToEntity<SaleReservationItem>();
                            return saleItem;
                        }).ToList();

                        if (!_costContractBillService.CheckGift(store ?? 0, data.TerminalId, items, out string errMsg2))
                        {
                            return this.Warning(errMsg2);
                        }
                    }
                    #endregion

                    #region 预收款 验证
                    if (data.Accounting != null)
                    {
                        if (data.AdvanceAmount > 0 && data.AdvanceAmount > data.AdvanceAmountBalance)
                        {
                            return this.Warning("预收款余额不足!");
                        }
                    }
                    #endregion

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

                    string errMsg = "";
                    IList<Product> allProducts = new List<Product>();
                    //当前数据
                    List<ProductStockItem> productStockItemNews = new List<ProductStockItem>();
                    if (data.Items != null && data.Items.Count > 0)
                    {
                        allProducts = _productService.GetProductsByIds(store ?? 0, data.Items.Select(pr => pr.ProductId).Distinct().ToArray());
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
                if (!_stockService.CheckStockQty(_productService, _specificationAttributeService, store ?? 0, data.WareHouseId, productStockItemNews, out string errMsg))
                {
                    return this.Warning(errMsg);
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
                                    errMsg += $"商品：{it.ProductName},0元开单，必须选择备注.";
                                }
                            });
                        }
                    }
                    if (!string.IsNullOrEmpty(errMsg))
                    {
                        return this.Warning(errMsg);
                    }
                    #endregion

                    //业务逻辑
                    var accountings = _accountingService.GetAllAccountingOptions(store ?? 0, 0, true);
                    var dataTo = data.ToEntity<SaleReservationBillUpdate>();
                    dataTo.Operation = (int)OperationEnum.APP;
                    if (data.Accounting == null)
                    {
                        return this.Warning("没有默认的付款账号");
                    }
                    dataTo.Accounting = data.Accounting.Select(ac =>
                    {
                        return ac.ToAccountEntity<SaleReservationBillAccounting>();
                    }).ToList();
                    dataTo.Items = data.Items.OrderBy(it => it.SortIndex).Select(it =>
                    {
                        //成本价（此处计算成本价防止web、api成本价未带出,web、api的controller都要单独计算（取消service计算，防止其他service都引用 pruchasebillservice））
                        var item = it.ToEntity<SaleReservationItem>();
                        item.CostPrice = _purchaseBillService.GetReferenceCostPrice(store ?? 0, item.ProductId, item.UnitId);
                        item.CostAmount = item.CostPrice * item.Quantity;
                        return item;
                    }).ToList();

                    //RedLock
                    var results = await _locker.PerformActionWithLockAsync(this.LockKey(data, store ?? 0, userId ?? 0),
                        TimeSpan.FromSeconds(30),
                        TimeSpan.FromSeconds(10),
                        TimeSpan.FromSeconds(1),
                        () => _saleReservationBillService.BillCreateOrUpdate(store ?? 0,
                        userId ?? 0,
                        billId,
                        bill,
                        dataTo.Accounting,
                        accountings,
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
        [HttpGet("salereservationbill/auditing/{store}/{userId}/{billId}")]
        [SwaggerOperation("auditing")]
        //[ValidateActionParameters]
        //[AuthBaseFilter]
        public async Task<APIResult<dynamic>> Auditing(int? store, int? userId, int? billId)
        {
            if (!store.HasValue || store.Value == 0)
                return this.Error(Resources.ParameterError);

            return await Task.Run(async () =>
            {
                try
                {
                    var bill = new SaleReservationBill();

                    // 验证
                    if (!billId.HasValue)
                        return this.Warning("参数错误.");
                    else
                    { 
                        bill = _saleReservationBillService.GetSaleReservationBillById(store ?? 0, billId.Value, true);
                        if (bill.AuditedStatus)
                        {
                            return Warning("单据已审核，请刷新页面.");
                        }
                    }

                    //公共单据验证
                    var commonBillChecking = BillChecking<SaleReservationBill, SaleReservationItem>(bill, store ?? 0, BillStates.Audited);
                    if (commonBillChecking.Data != null)
                        return commonBillChecking;

                    if (_wareHouseService.CheckProductInventory(store ?? 0, bill.WareHouseId, bill.Items?.Select(it => it.ProductId).Distinct().ToArray(), out string thisMsg))
                        return this.Warning("仓库正在盘点中，拒绝操作.");

                    #region 验证赠品
                    var items = bill.Items.Where(it => it.SaleProductTypeId > 0).ToList();
                    if (!_costContractBillService.CheckGift(store ?? 0, bill.TerminalId, items, out string errMsg))
                    {
                        return this.Warning(errMsg);
                    }
                    #endregion

                    #region 预收款 验证
                    if (bill.SaleReservationBillAccountings != null)
                    {
                        //1.获取当前经销商 预收款科目Id
                        int accountingOptionId = 0;
                        AccountingOption accountingOption = _accountingService.GetAccountingOptionByAccountCodeTypeId(store ?? 0, (int)AccountingCodeEnum.AdvancesReceived);
                        if (accountingOption != null)
                        {
                            accountingOptionId = (accountingOption == null) ? 0 : accountingOption.Id;
                        }

                        //获取用户输入 预收款金额
                        var advancePaymentReceipt = bill.SaleReservationBillAccountings.Where(ac => ac.AccountingOptionId == accountingOptionId).Sum(ac => ac.CollectionAmount);

                        //用户可用 预收款金额
                        decimal useAdvanceReceiptAmount = _commonBillService.CalcTerminalBalance(store ?? 0, bill.TerminalId).AdvanceAmountBalance;
                        //如果输入预收款大于用户可用预收款
                        if (advancePaymentReceipt > 0 && advancePaymentReceipt > useAdvanceReceiptAmount)
                        {
                            return this.Warning("预收款余额不足！");
                        }
                    }
                    #endregion

                    #region 验证库存
                    IList<Product> allProducts = new List<Product>();
                    //当前数据
                    List<ProductStockItem> productStockItemNews = new List<ProductStockItem>();
                    if (bill.Items != null && bill.Items.Count > 0)
                    {
                        allProducts = _productService.GetProductsByIds(store ?? 0, bill.Items.Select(pr => pr.ProductId).Distinct().ToArray());
                        var allOptions = _specificationAttributeService.GetSpecificationAttributeOptionByIds(store ?? 0, allProducts.GetProductBigStrokeSmallUnitIds());

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
                          () => _saleReservationBillService.Auditing(store ?? 0, userId ?? 0, bill));

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
        [HttpGet("salereservationbill/reverse/{store}/{userId}/{billId}")]
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

                    var bill = new SaleReservationBill() { StoreId = store ?? 0 };

                    if (this.PeriodClosed(DateTime.Now, store ?? 0))
                        return this.Warning("系统当月已经结转，不允许红冲");

                    if (!billId.HasValue)
                        return this.Warning("参数错误.");
                    else
                    { 
                        bill = _saleReservationBillService.GetSaleReservationBillById(store ?? 0, billId.Value, true);
                        bill.Remark = remark;
                        if (bill.ReversedStatus)
                        {
                            return Warning("单据已红冲，请刷新页面.");
                        }
                    }


                    //公共单据验证
                    var commonBillChecking = BillChecking<SaleReservationBill, SaleReservationItem>(bill, store ?? 0, BillStates.Reversed);
                    if (commonBillChecking.Data != null)
                        return commonBillChecking;

                    if (_wareHouseService.CheckProductInventory(store ?? 0, bill.WareHouseId, bill.Items?.Select(it => it.ProductId).Distinct().ToArray(), out string thisMsg))
                        return this.Warning("仓库正在盘点中，拒绝操作.");


                    // 验证送货签收
                    if (_dispatchBillService.CheckSign(bill))
                        return this.Warning("当前销售订单已经签收，不能红冲.");

                    //RedLock
                    var result = await _locker.PerformActionWithLockAsync(RedLockKey(bill, store ?? 0, userId ?? 0),
                          TimeSpan.FromSeconds(30),
                          TimeSpan.FromSeconds(10),
                          TimeSpan.FromSeconds(1),
                          () => _saleReservationBillService.Reverse(userId ?? 0, bill));

                    return this.Successful("红冲成功", result);
                }
                catch (Exception ex)
                {
                    _userActivityService.InsertActivity("Reverse", "单据红冲失败", userId);
                    return this.Error(ex.Message);
                }
            });
        }


        /// <summary>
        /// 获取初始绑定
        /// </summary>
        /// <param name="store"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        [HttpGet("salereservationbill/getinitdataasync/{store}/{userId}")]
        [SwaggerOperation("GetInitDataAsync")]
        //[AuthBaseFilter]
        public async Task<APIResult<SaleReservationBillModel>> GetInitDataAsync(int? store, int? userId)
        {
            if (!store.HasValue || store.Value == 0)
                return this.Error3<SaleReservationBillModel>(Resources.ParameterError);

            return await Task.Run(() =>
            {
                try
                {
                    var model = new SaleReservationBillModel
                    {
                        //默认售价（方案）
                        SaleReservationBillDefaultAmounts = BindPricePlanSelection(_productTierPricePlanService.GetAllPricePlan, store ?? 0),
                        DefaultAmountId = "0_5"
                    };

                    var companySetting = _settingService.LoadSetting<CompanySetting>(store ?? 0);
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

                    //默认账户设置
                    var defaultAcc = _accountingService.GetDefaultAccounting(store ?? 0, BillTypeEnum.SaleReservationBill);
                    model.SaleReservationBillAccountings.Add(new SaleReservationBillAccountingModel()
                    {
                        AccountingOptionId = defaultAcc?.Item1?.Id ?? 0,
                        CollectionAmount = 0,
                        Name = defaultAcc.Item1?.Name,
                        AccountCodeTypeId = defaultAcc?.Item1?.AccountCodeTypeId ?? 0
                    });

                    model.WareHouseId = _wareHouseService.GetWareHouseList(store).FirstOrDefault(c => c.Type == 1)?.Id??0;
                    model.WareHouseName = _wareHouseService.GetWareHouseList(store).FirstOrDefault(c => c.Type == 1)?.Name;

                    return this.Successful("", model);
                }
                catch (Exception ex)
                {
                    return this.Error3<SaleReservationBillModel>(ex.Message);
                }
            });

        }




    }
}