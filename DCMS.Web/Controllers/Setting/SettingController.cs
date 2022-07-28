using DCMS.Core;
using DCMS.Core.Caching;
using DCMS.Core.Domain.Configuration;
using DCMS.Core.Domain.Products;
using DCMS.Core.Domain.WareHouses;
using DCMS.Services.Configuration;
using DCMS.Services.Installation;
using DCMS.Services.Logging;
using DCMS.Services.Messages;
using DCMS.Services.Products;
using DCMS.Services.Settings;
using DCMS.Services.Terminals;
using DCMS.Services.WareHouses;
using DCMS.ViewModel.Models.Configuration;
using DCMS.Web.Framework.Extensions;
using DCMS.Web.Framework.Mvc.Filters;
using DCMS.Web.Infrastructure.Mapper.Extensions;
using DCMS.Web.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Task = System.Threading.Tasks.Task;
using DCMS.Services.Users;
using System.Linq.Expressions;
using DCMS.Core.Domain.Users;
using DCMS.Core.Infrastructure;
using DCMS.Core.Configuration;



namespace DCMS.Web.Controllers
{
    public class SettingController : BasePublicController
    {
        private readonly IProductService _productService;
        private readonly IUserActivityService _userActivityService;
        private readonly ISettingService _settingService;
        private readonly ISpecificationAttributeService _specificationAttributeService;
        private readonly IAccountingService _accountingService;
        private readonly IStockEarlyWarningService _stockEarlyWarningService;
        private readonly IWareHouseService _wareHouseService;
        private readonly IPricingStructureService _pricingStructureService;
        private readonly IChannelService _channelService;
        private readonly IProductTierPricePlanService _productTierPricePlanService;
        private readonly IPrintTemplateService _printTemplateService;
        private readonly IRemarkConfigService _remarkConfigService;
        private readonly IInstallationService _installationService;
        private readonly IWebHostEnvironment _hostingEnvironment;
        private readonly IRedLocker _locker;
        protected readonly IStaticCacheManager _cacheManager;
        private readonly IUserService _userService;


        public SettingController(IProductService productService,
            IUserActivityService userActivityService,
            IWorkContext workContext,
            IStoreContext storeContext,
            ISettingService settingService,
            IAccountingService accountingService,
            ISpecificationAttributeService specificationAttributeService,
            IStockEarlyWarningService stockEarlyWarningService,
            IWareHouseService wareHouseService,
            IPricingStructureService pricingStructureService,
            IChannelService channelService,
            IProductTierPricePlanService productTierPricePlanService,
            IPrintTemplateService printTemplateService,
            IRemarkConfigService remarkConfigService,
            ILogger loggerService,
            IUserService userService,
            IStaticCacheManager cacheManager,
            INotificationService notificationService,
            IInstallationService installationService,
            IWebHostEnvironment hostingEnvironment,
            IRedLocker locker
            ) : base(workContext, loggerService, storeContext, notificationService)
        {
            _userActivityService = userActivityService;
            _productService = productService;
            _settingService = settingService;
            _specificationAttributeService = specificationAttributeService;
            _accountingService = accountingService;
            _wareHouseService = wareHouseService;
            _pricingStructureService = pricingStructureService;
            _channelService = channelService;
            _productTierPricePlanService = productTierPricePlanService;
            _printTemplateService = printTemplateService;
            _remarkConfigService = remarkConfigService;
            _stockEarlyWarningService = stockEarlyWarningService;
            _installationService = installationService;
            _hostingEnvironment = hostingEnvironment;
            _cacheManager = cacheManager;
            _locker = locker;
            _userService = userService;
        }


        [HttpGet]
        public async Task<JsonResult> Test()
        {
            return await Task.Run(() =>
            {
                try
                {
                    var _config = EngineContext.Current.Resolve<DCMSConfig>();

                    var key = string.Format(DCMSDefaults.SETTINGS_PK, _storeContext.CurrentStore.Id);
                    var cache = _cacheManager.Get<List<DCMS.Core.Domain.Configuration.Setting>>(new CacheKey(key), null);

                    //var cacheKey = new CacheKey($"SETTINGS_PK.{_storeContext.CurrentStore.Id}") { CacheTime = 5 };
                    //var cache2 = _cacheManager.Get<List<DCMS.Core.Domain.Configuration.Setting>>(cacheKey, null);
                    var cm = _cacheManager.ToString();
                    return Json(new
                    {
                        RedisConnectionString = _config.RedisConnectionString,
                        DCMSConfig = _config,
                        cacheManager = cm,
                        smallunit = cache.Where(s => s.Name.Equals("productsetting.smallunitspecificationattributeoptionsmapping"))
                    });

                }
                catch (Exception ex)
                {
                    return Json(new { ex });
                }
            });
        }



        #region 商品高级配置

        [AuthCode((int)AccessGranularityEnum.ProductSettingView)]
        public IActionResult Product()
        {

            //获取当前经销商配置
            _settingService.ClearCache(_storeContext.CurrentStore.Id);
            var productSetting = _settingService.LoadSetting<ProductSetting>(_storeContext.CurrentStore.Id);
            //var model = productSetting.ToModel<ProductSettingModel>();
            var model = new ProductSettingModel
            {
                SmallUnitSpecificationAttributeOptionsMapping = productSetting.SmallUnitSpecificationAttributeOptionsMapping,
                StrokeUnitSpecificationAttributeOptionsMapping = productSetting.StrokeUnitSpecificationAttributeOptionsMapping,
                BigUnitSpecificationAttributeOptionsMapping = productSetting.BigUnitSpecificationAttributeOptionsMapping
            };

            var smalllists = new List<SelectListItem>();
            var stroklists = new List<SelectListItem>();
            var biglists = new List<SelectListItem>();

            //规格属性
            var specificationAttributes = _specificationAttributeService.GetSpecificationAttributesBtStore(curStore?.Id ?? 0).ToList();
            specificationAttributes.ForEach(sp =>
            {
                smalllists.Add(new SelectListItem() { Text = sp.Name, Value = sp.Id.ToString() });
                stroklists.Add(new SelectListItem() { Text = sp.Name, Value = sp.Id.ToString() });
                biglists.Add(new SelectListItem() { Text = sp.Name, Value = sp.Id.ToString() });
            });

            model.SmallUnits = new SelectList(smalllists, "Value", "Text");
            model.StrokeUnits = new SelectList(stroklists, "Value", "Text");
            model.BigUnits = new SelectList(biglists, "Value", "Text");

            return View(model);
        }

        [HttpPost]
        [AuthCode((int)AccessGranularityEnum.ProductSettingUpdate)]
        public IActionResult Product(ProductSettingModel model)
        {

            //获取当前经销商配置
            var storeId = _storeContext.CurrentStore.Id;
            var productSetting = _settingService.LoadSetting<ProductSetting>(storeId);
            //productSetting = model.ToEntity<ProductSetting>(productSetting);
            productSetting.SmallUnitSpecificationAttributeOptionsMapping = model.SmallUnitSpecificationAttributeOptionsMapping;
            productSetting.StrokeUnitSpecificationAttributeOptionsMapping = model.StrokeUnitSpecificationAttributeOptionsMapping;
            productSetting.BigUnitSpecificationAttributeOptionsMapping = model.BigUnitSpecificationAttributeOptionsMapping;

            _settingService.SaveSetting(productSetting, x => x.SmallUnitSpecificationAttributeOptionsMapping, storeId, false);
            _settingService.SaveSetting(productSetting, x => x.StrokeUnitSpecificationAttributeOptionsMapping, storeId, false);
            _settingService.SaveSetting(productSetting, x => x.BigUnitSpecificationAttributeOptionsMapping, storeId, false);


            //清除缓存
            _settingService.ClearCache(storeId);

            //日志
            _userActivityService.InsertActivity("EditSettings", "编辑配置", curUser);
            _notificationService.SuccessNotification("配置已经更新");
            return RedirectToAction("Product");
        }

        #endregion

        #region 财务设置

        public IActionResult Finances()
        {
            var model = new FinanceSettingModel();

            var fs = _settingService.LoadSetting<FinanceSetting>(_storeContext.CurrentStore.Id);

            //默认账户集
            var options = (from option in _accountingService.GetDefaultAccounts(curStore?.Id ?? 0) select option).ToList();

            model.Options = options.Select(o => o.ToModel<AccountingOptionModel>()).ToList();

            //解析配置

            #region 销售
            var saleFinanceConfiguer = string.IsNullOrEmpty(fs.SaleBillAccountingOptionConfiguration) ? null : JsonConvert.DeserializeObject<FinanceAccountingMap>(fs.SaleBillAccountingOptionConfiguration);
            var saleReservationFinanceConfiguer = string.IsNullOrEmpty(fs.SaleReservationBillAccountingOptionConfiguration) ? null : JsonConvert.DeserializeObject<FinanceAccountingMap>(fs.SaleReservationBillAccountingOptionConfiguration);

            var returnFinanceConfiguer = string.IsNullOrEmpty(fs.ReturnBillAccountingOptionConfiguration) ? null : JsonConvert.DeserializeObject<FinanceAccountingMap>(fs.ReturnBillAccountingOptionConfiguration);
            var returnReservationFinanceConfiguer = string.IsNullOrEmpty(fs.ReturnReservationBillAccountingOptionConfiguration) ? null : JsonConvert.DeserializeObject<FinanceAccountingMap>(fs.ReturnReservationBillAccountingOptionConfiguration);
            #endregion

            #region 采购
            var purchaseFinanceConfiguer = string.IsNullOrEmpty(fs.PurchaseBillAccountingOptionConfiguration) ? null : JsonConvert.DeserializeObject<FinanceAccountingMap>(fs.PurchaseBillAccountingOptionConfiguration);
            var purchaseReturnFinanceConfiguer = string.IsNullOrEmpty(fs.PurchaseReturnBillAccountingOptionConfiguration) ? null : JsonConvert.DeserializeObject<FinanceAccountingMap>(fs.PurchaseReturnBillAccountingOptionConfiguration);
            #endregion

            #region 仓储
            var inventoryProfitLossFinanceConfiguer = string.IsNullOrEmpty(fs.InventoryProfitLossAccountingOptionConfiguration) ? null : JsonConvert.DeserializeObject<FinanceAccountingMap>(fs.InventoryProfitLossAccountingOptionConfiguration);
            var scrapProductFinanceConfiguer = string.IsNullOrEmpty(fs.ScrapProductAccountingOptionConfiguration) ? null : JsonConvert.DeserializeObject<FinanceAccountingMap>(fs.ScrapProductAccountingOptionConfiguration);
            #endregion

            #region 财务
            var receiptFinanceConfiguer = string.IsNullOrEmpty(fs.ReceiptAccountingOptionConfiguration) ? null : JsonConvert.DeserializeObject<FinanceAccountingMap>(fs.ReceiptAccountingOptionConfiguration);
            var paymentFinanceConfiguer = string.IsNullOrEmpty(fs.PaymentAccountingOptionConfiguration) ? null : JsonConvert.DeserializeObject<FinanceAccountingMap>(fs.PaymentAccountingOptionConfiguration);

            var advanceReceiptFinanceConfiguer = string.IsNullOrEmpty(fs.AdvanceReceiptAccountingOptionConfiguration) ? null : JsonConvert.DeserializeObject<FinanceAccountingMap>(fs.AdvanceReceiptAccountingOptionConfiguration);
            var advancePaymentFinanceConfiguer = string.IsNullOrEmpty(fs.AdvancePaymentAccountingOptionConfiguration) ? null : JsonConvert.DeserializeObject<FinanceAccountingMap>(fs.AdvancePaymentAccountingOptionConfiguration);

            var costExpenditureFinanceConfiguer = string.IsNullOrEmpty(fs.CostExpenditureAccountingOptionConfiguration) ? null : JsonConvert.DeserializeObject<FinanceAccountingMap>(fs.CostExpenditureAccountingOptionConfiguration);

            var financialIncomeFinanceConfiguer = string.IsNullOrEmpty(fs.FinancialIncomeAccountingOptionConfiguration) ? null : JsonConvert.DeserializeObject<FinanceAccountingMap>(fs.FinancialIncomeAccountingOptionConfiguration);

            var costcontractFinanceConfiguer = string.IsNullOrEmpty(fs.CostContractAccountingOptionConfiguration) ? null : JsonConvert.DeserializeObject<FinanceAccountingMap>(fs.CostContractAccountingOptionConfiguration);
            #endregion

            #region 销售
            //销售单(收款账户)
            model.SaleFinanceAccountingMap = new FinanceAccountingMap()
            {
                Options = saleFinanceConfiguer != null && saleFinanceConfiguer.Options.Count > 0 ? saleFinanceConfiguer.Options : new List<AccountingOption>(),
                DefaultOption = saleFinanceConfiguer != null ? saleFinanceConfiguer.DefaultOption : 0,
                DebitOption = saleFinanceConfiguer != null ? saleFinanceConfiguer.DebitOption : 0,
                CreditOption = saleFinanceConfiguer != null ? saleFinanceConfiguer.CreditOption : 0
            };
            //销售订单(收款账户)
            model.SaleReservationBillFinanceAccountingMap = new FinanceAccountingMap()
            {
                Options = saleReservationFinanceConfiguer != null ? saleReservationFinanceConfiguer.Options : new List<AccountingOption>(),
                DefaultOption = saleReservationFinanceConfiguer != null ? saleReservationFinanceConfiguer.DefaultOption : 0,
                DebitOption = saleReservationFinanceConfiguer != null ? saleReservationFinanceConfiguer.DebitOption : 0,
                CreditOption = saleReservationFinanceConfiguer != null ? saleReservationFinanceConfiguer.CreditOption : 0
            };

            //退货单(收款账户)
            model.ReturnFinanceAccountingMap = new FinanceAccountingMap()
            {
                Options = returnFinanceConfiguer != null && returnFinanceConfiguer.Options.Count > 0 ? returnFinanceConfiguer.Options : new List<AccountingOption>(),
                DefaultOption = returnFinanceConfiguer != null ? returnFinanceConfiguer.DefaultOption : 0,
                DebitOption = returnFinanceConfiguer != null ? returnFinanceConfiguer.DebitOption : 0,
                CreditOption = returnFinanceConfiguer != null ? returnFinanceConfiguer.CreditOption : 0
            };
            //退货订单(收款账户)
            model.ReturnReservationFinanceAccountingMap = new FinanceAccountingMap()
            {
                Options = returnReservationFinanceConfiguer != null && returnReservationFinanceConfiguer.Options.Count > 0 ? returnReservationFinanceConfiguer.Options : new List<AccountingOption>(),
                DefaultOption = returnReservationFinanceConfiguer != null ? returnReservationFinanceConfiguer.DefaultOption : 0,
                DebitOption = returnReservationFinanceConfiguer != null ? returnReservationFinanceConfiguer.DebitOption : 0,
                CreditOption = returnReservationFinanceConfiguer != null ? returnReservationFinanceConfiguer.CreditOption : 0
            };
            #endregion

            #region 采购
            //采购单(收款账户)
            model.PurchaseFinanceAccountingMap = new FinanceAccountingMap()
            {
                Options = purchaseFinanceConfiguer != null && purchaseFinanceConfiguer.Options.Count > 0 ? purchaseFinanceConfiguer.Options : new List<AccountingOption>(),
                DefaultOption = purchaseFinanceConfiguer != null ? purchaseFinanceConfiguer.DefaultOption : 0,
                DebitOption = purchaseFinanceConfiguer != null ? purchaseFinanceConfiguer.DebitOption : 0,
                CreditOption = purchaseFinanceConfiguer != null ? purchaseFinanceConfiguer.CreditOption : 0
            };
            //采购退货单(收款账户)
            model.PurchaseReturnFinanceAccountingMap = new FinanceAccountingMap()
            {
                Options = purchaseReturnFinanceConfiguer != null && purchaseReturnFinanceConfiguer.Options.Count > 0 ? purchaseReturnFinanceConfiguer.Options : new List<AccountingOption>(),
                DefaultOption = purchaseReturnFinanceConfiguer != null ? purchaseReturnFinanceConfiguer.DefaultOption : 0,
                DebitOption = purchaseReturnFinanceConfiguer != null ? purchaseReturnFinanceConfiguer.DebitOption : 0,
                CreditOption = purchaseReturnFinanceConfiguer != null ? purchaseReturnFinanceConfiguer.CreditOption : 0
            };
            #endregion

            #region 采购
            //盘点盈亏单(盘点盈亏账户)
            model.InventoryProfitLossFinanceAccountingMap = new FinanceAccountingMap()
            {
                Options = inventoryProfitLossFinanceConfiguer != null && inventoryProfitLossFinanceConfiguer.Options.Count > 0 ? inventoryProfitLossFinanceConfiguer.Options : new List<AccountingOption>(),
                DefaultOption = inventoryProfitLossFinanceConfiguer != null ? inventoryProfitLossFinanceConfiguer.DefaultOption : 0,
                DebitOption = inventoryProfitLossFinanceConfiguer != null ? inventoryProfitLossFinanceConfiguer.DebitOption : 0,
                CreditOption = inventoryProfitLossFinanceConfiguer != null ? inventoryProfitLossFinanceConfiguer.CreditOption : 0
            };
            //报损单(报损账户)
            model.ScrapProductFinanceAccountingMap = new FinanceAccountingMap()
            {
                Options = scrapProductFinanceConfiguer != null && scrapProductFinanceConfiguer.Options.Count > 0 ? scrapProductFinanceConfiguer.Options : new List<AccountingOption>(),
                DefaultOption = scrapProductFinanceConfiguer != null ? scrapProductFinanceConfiguer.DefaultOption : 0,
                DebitOption = scrapProductFinanceConfiguer != null ? scrapProductFinanceConfiguer.DebitOption : 0,
                CreditOption = scrapProductFinanceConfiguer != null ? scrapProductFinanceConfiguer.CreditOption : 0
            };
            #endregion

            #region 财务
            //收款单(收款账户)
            model.ReceiptFinanceAccountingMap = new FinanceAccountingMap()
            {
                Options = receiptFinanceConfiguer != null && receiptFinanceConfiguer.Options.Count > 0 ? receiptFinanceConfiguer.Options : new List<AccountingOption>(),
                DefaultOption = receiptFinanceConfiguer != null ? receiptFinanceConfiguer.DefaultOption : 0,
                DebitOption = receiptFinanceConfiguer != null ? receiptFinanceConfiguer.DebitOption : 0,
                CreditOption = receiptFinanceConfiguer != null ? receiptFinanceConfiguer.CreditOption : 0
            };

            //付款单(收款账户)
            model.PaymentFinanceAccountingMap = new FinanceAccountingMap()
            {
                Options = paymentFinanceConfiguer != null && paymentFinanceConfiguer.Options.Count > 0 ? paymentFinanceConfiguer.Options : new List<AccountingOption>(),
                DefaultOption = paymentFinanceConfiguer != null ? paymentFinanceConfiguer.DefaultOption : 0,
                DebitOption = paymentFinanceConfiguer != null ? paymentFinanceConfiguer.DebitOption : 0,
                CreditOption = paymentFinanceConfiguer != null ? paymentFinanceConfiguer.CreditOption : 0
            };


            //预收款单(收款账户)
            model.AdvanceReceiptFinanceAccountingMap = new FinanceAccountingMap()
            {
                Options = advanceReceiptFinanceConfiguer != null && advanceReceiptFinanceConfiguer.Options.Count > 0 ? advanceReceiptFinanceConfiguer.Options : new List<AccountingOption>(),
                DefaultOption = advanceReceiptFinanceConfiguer != null ? advanceReceiptFinanceConfiguer.DefaultOption : 0,
                DebitOption = advanceReceiptFinanceConfiguer != null ? advanceReceiptFinanceConfiguer.DebitOption : 0,
                CreditOption = advanceReceiptFinanceConfiguer != null ? advanceReceiptFinanceConfiguer.CreditOption : 0
            };

            //预付款单(付款账户)
            model.AdvancePaymentFinanceAccountingMap = new FinanceAccountingMap()
            {
                Options = advancePaymentFinanceConfiguer != null && advancePaymentFinanceConfiguer.Options.Count > 0 ? advancePaymentFinanceConfiguer.Options : new List<AccountingOption>(),
                DefaultOption = advancePaymentFinanceConfiguer != null ? advancePaymentFinanceConfiguer.DefaultOption : 0,
                DebitOption = advancePaymentFinanceConfiguer != null ? advancePaymentFinanceConfiguer.DebitOption : 0,
                CreditOption = advancePaymentFinanceConfiguer != null ? advancePaymentFinanceConfiguer.CreditOption : 0
            };

            //费用支出（支出账户） 
            model.CostExpenditureFinanceAccountingMap = new FinanceAccountingMap()
            {
                Options = costExpenditureFinanceConfiguer != null && costExpenditureFinanceConfiguer.Options.Count > 0 ? costExpenditureFinanceConfiguer.Options : new List<AccountingOption>(),
                DefaultOption = costExpenditureFinanceConfiguer != null ? costExpenditureFinanceConfiguer.DefaultOption : 0,
                DebitOption = costExpenditureFinanceConfiguer != null ? costExpenditureFinanceConfiguer.DebitOption : 0,
                CreditOption = costExpenditureFinanceConfiguer != null ? costExpenditureFinanceConfiguer.CreditOption : 0
            };

            //  财务收入（收款账户） 
            model.FinancialIncomeFinanceAccountingMap = new FinanceAccountingMap()
            {
                Options = financialIncomeFinanceConfiguer != null && financialIncomeFinanceConfiguer.Options.Count > 0 ? financialIncomeFinanceConfiguer.Options : new List<AccountingOption>(),
                DefaultOption = financialIncomeFinanceConfiguer != null ? financialIncomeFinanceConfiguer.DefaultOption : 0,
                DebitOption = financialIncomeFinanceConfiguer != null ? financialIncomeFinanceConfiguer.DebitOption : 0,
                CreditOption = financialIncomeFinanceConfiguer != null ? financialIncomeFinanceConfiguer.CreditOption : 0
            };

            //  费用合同（会计科目） 
            model.CostContractAccountingMap = new FinanceAccountingMap()
            {
                Options = costcontractFinanceConfiguer != null && costcontractFinanceConfiguer.Options.Count > 0 ? costcontractFinanceConfiguer.Options : new List<AccountingOption>(),
                DefaultOption = costcontractFinanceConfiguer != null ? costcontractFinanceConfiguer.DefaultOption : 0,
                DebitOption = costcontractFinanceConfiguer != null ? costcontractFinanceConfiguer.DebitOption : 0,
                CreditOption = costcontractFinanceConfiguer != null ? costcontractFinanceConfiguer.CreditOption : 0
            };
            #endregion

            return View(model);
        }
        [HttpPost]
        public IActionResult Finances(IFormCollection form)
        {
            var model = new FinanceSettingModel();

            //获取当前经销商配置
            var storeId = _storeContext.CurrentStore.Id;
            var fs = _settingService.LoadSetting<FinanceSetting>(storeId);

            //默认账户集
            var options = (from option in _accountingService.GetDefaultAccounts(curStore?.Id ?? 0) select option).ToList();
            var options1 = (from option in _accountingService.GetDefaultAccounts(curStore?.Id ?? 0) select option).ToList();
            var options2 = (from option in _accountingService.GetDefaultAccounts(curStore?.Id ?? 0) select option).ToList();
            var options3 = (from option in _accountingService.GetDefaultAccounts(curStore?.Id ?? 0) select option).ToList();
            var options4 = (from option in _accountingService.GetDefaultAccounts(curStore?.Id ?? 0) select option).ToList();
            var options5 = (from option in _accountingService.GetDefaultAccounts(curStore?.Id ?? 0) select option).ToList();
            var options6 = (from option in _accountingService.GetDefaultAccounts(curStore?.Id ?? 0) select option).ToList();
            var options7 = (from option in _accountingService.GetDefaultAccounts(curStore?.Id ?? 0) select option).ToList();
            var options8 = (from option in _accountingService.GetDefaultAccounts(curStore?.Id ?? 0) select option).ToList();
            var options9 = (from option in _accountingService.GetDefaultAccounts(curStore?.Id ?? 0) select option).ToList();
            var options10 = (from option in _accountingService.GetDefaultAccounts(curStore?.Id ?? 0) select option).ToList();
            var options11 = (from option in _accountingService.GetDefaultAccounts(curStore?.Id ?? 0) select option).ToList();
            var options12 = (from option in _accountingService.GetDefaultAccounts(curStore?.Id ?? 0) select option).ToList();
            var options13 = (from option in _accountingService.GetDefaultAccounts(curStore?.Id ?? 0) select option).ToList();
            var options14 = (from option in _accountingService.GetDefaultAccounts(curStore?.Id ?? 0) select option).ToList();

            //获取默认账户
            //销售
            var defaultSaleBillAccounting = !string.IsNullOrEmpty(form["Sale_AccountingOption_Default"]) ? form["Sale_AccountingOption_Default"].ToString() : "0";
            var debitSaleBillAccounting = !string.IsNullOrEmpty(form["Sale_AccountingOption_Debit"]) ? form["Sale_AccountingOption_Debit"].ToString() : "0";
            var creditSaleBillAccounting = !string.IsNullOrEmpty(form["Sale_AccountingOption_Credit"]) ? form["Sale_AccountingOption_Credit"].ToString() : "0";

            var defaultSaleReservationBillAccounting = !string.IsNullOrEmpty(form["SaleReservationBill_AccountingOption_Default"]) ? form["SaleReservationBill_AccountingOption_Default"].ToString() : "0";
            var debitSaleReservationBillAccounting = !string.IsNullOrEmpty(form["SaleReservationBill_AccountingOption_Debit"]) ? form["SaleReservationBill_AccountingOption_Debit"].ToString() : "0";
            var creditSaleReservationBillAccounting = !string.IsNullOrEmpty(form["SaleReservationBill_AccountingOption_Credit"]) ? form["SaleReservationBill_AccountingOption_Credit"].ToString() : "0";

            var defaultReturnBillAccounting = !string.IsNullOrEmpty(form["Return_AccountingOption_Default"]) ? form["Return_AccountingOption_Default"].ToString() : "0";
            var debitReturnBillAccounting = !string.IsNullOrEmpty(form["Return_AccountingOption_Debit"]) ? form["Return_AccountingOption_Debit"].ToString() : "0";
            var creditReturnBillAccounting = !string.IsNullOrEmpty(form["Return_AccountingOption_Credit"]) ? form["Return_AccountingOption_Credit"].ToString() : "0";

            var defaultReturnReservationBillAccounting = !string.IsNullOrEmpty(form["ReturnReservation_AccountingOption_Default"]) ? form["ReturnReservation_AccountingOption_Default"].ToString() : "0";
            var debitReturnReservationBillAccounting = !string.IsNullOrEmpty(form["ReturnReservation_AccountingOption_Debit"]) ? form["ReturnReservation_AccountingOption_Debit"].ToString() : "0";
            var creditReturnReservationBillAccounting = !string.IsNullOrEmpty(form["ReturnReservation_AccountingOption_Credit"]) ? form["ReturnReservation_AccountingOption_Credit"].ToString() : "0";

            //采购
            var defaultPurchaseBillAccounting = !string.IsNullOrEmpty(form["Purchase_AccountingOption_Default"]) ? form["Purchase_AccountingOption_Default"].ToString() : "0";
            var debitPurchaseBillAccounting = !string.IsNullOrEmpty(form["Purchase_AccountingOption_Debit"]) ? form["Purchase_AccountingOption_Debit"].ToString() : "0";
            var creditPurchaseBillAccounting = !string.IsNullOrEmpty(form["Purchase_AccountingOption_Credit"]) ? form["Purchase_AccountingOption_Credit"].ToString() : "0";

            var defaultPurchaseReturnBillAccounting = !string.IsNullOrEmpty(form["PurchaseReturn_AccountingOption_Default"]) ? form["PurchaseReturn_AccountingOption_Default"].ToString() : "0";
            var debitPurchaseReturnBillAccounting = !string.IsNullOrEmpty(form["PurchaseReturn_AccountingOption_Debit"]) ? form["PurchaseReturn_AccountingOption_Debit"].ToString() : "0";
            var creditPurchaseReturnBillAccounting = !string.IsNullOrEmpty(form["PurchaseReturn_AccountingOption_Credit"]) ? form["PurchaseReturn_AccountingOption_Credit"].ToString() : "0";

            //仓储
            var defaultInventoryProfitLossAccounting = !string.IsNullOrEmpty(form["InventoryProfitLoss_AccountingOption_Default"]) ? form["InventoryProfitLoss_AccountingOption_Default"].ToString() : "0";
            var debitInventoryProfitLossAccounting = !string.IsNullOrEmpty(form["InventoryProfitLoss_AccountingOption_Debit"]) ? form["InventoryProfitLoss_AccountingOption_Debit"].ToString() : "0";
            var creditInventoryProfitLossAccounting = !string.IsNullOrEmpty(form["InventoryProfitLoss_AccountingOption_Credit"]) ? form["InventoryProfitLoss_AccountingOption_Credit"].ToString() : "0";

            var defaultScrapProductAccounting = !string.IsNullOrEmpty(form["ScrapProduct_AccountingOption_Default"]) ? form["ScrapProduct_AccountingOption_Default"].ToString() : "0";
            var debitScrapProductAccounting = !string.IsNullOrEmpty(form["ScrapProduct_AccountingOption_Debit"]) ? form["ScrapProduct_AccountingOption_Debit"].ToString() : "0";
            var creditScrapProductAccounting = !string.IsNullOrEmpty(form["ScrapProduct_AccountingOption_Credit"]) ? form["ScrapProduct_AccountingOption_Credit"].ToString() : "0";


            //财务
            var defaultReceiptAccounting = !string.IsNullOrEmpty(form["Receipt_AccountingOption_Default"]) ? form["Receipt_AccountingOption_Default"].ToString() : "0";
            var debitReceiptAccounting = !string.IsNullOrEmpty(form["Receipt_AccountingOption_Debit"]) ? form["Receipt_AccountingOption_Debit"].ToString() : "0";
            var creditReceiptAccounting = !string.IsNullOrEmpty(form["Receipt_AccountingOption_Credit"]) ? form["Receipt_AccountingOption_Credit"].ToString() : "0";

            var defaultPaymentAccounting = !string.IsNullOrEmpty(form["Payment_AccountingOption_Default"]) ? form["Payment_AccountingOption_Default"].ToString() : "0";
            var debitPaymentAccounting = !string.IsNullOrEmpty(form["Payment_AccountingOption_Debit"]) ? form["Payment_AccountingOption_Debit"].ToString() : "0";
            var creditPaymentAccounting = !string.IsNullOrEmpty(form["Payment_AccountingOption_Credit"]) ? form["Payment_AccountingOption_Credit"].ToString() : "0";


            var defaultAdvanceReceiptAccounting = !string.IsNullOrEmpty(form["AdvanceReceipt_AccountingOption_Default"]) ? form["AdvanceReceipt_AccountingOption_Default"].ToString() : "0";
            var debitAdvanceReceiptAccounting = !string.IsNullOrEmpty(form["AdvanceReceipt_AccountingOption_Debit"]) ? form["AdvanceReceipt_AccountingOption_Debit"].ToString() : "0";
            var creditAdvanceReceiptAccounting = !string.IsNullOrEmpty(form["AdvanceReceipt_AccountingOption_Credit"]) ? form["AdvanceReceipt_AccountingOption_Credit"].ToString() : "0";

            var defaultAdvancePaymentAccounting = !string.IsNullOrEmpty(form["AdvancePayment_AccountingOption_Default"]) ? form["AdvancePayment_AccountingOption_Default"].ToString() : "0";
            var debitAdvancePaymentAccounting = !string.IsNullOrEmpty(form["AdvancePayment_AccountingOption_Debit"]) ? form["AdvancePayment_AccountingOption_Debit"].ToString() : "0";
            var creditAdvancePaymentAccounting = !string.IsNullOrEmpty(form["AdvancePayment_AccountingOption_Credit"]) ? form["AdvancePayment_AccountingOption_Credit"].ToString() : "0";

            var defaultCostExpenditureAccounting = !string.IsNullOrEmpty(form["CostExpenditure_AccountingOption_Default"]) ? form["CostExpenditure_AccountingOption_Default"].ToString() : "0";
            var debitCostExpenditureAccounting = !string.IsNullOrEmpty(form["CostExpenditure_AccountingOption_Debit"]) ? form["CostExpenditure_AccountingOption_Debit"].ToString() : "0";
            var creditCostExpenditureAccounting = !string.IsNullOrEmpty(form["CostExpenditure_AccountingOption_Credit"]) ? form["CostExpenditure_AccountingOption_Credit"].ToString() : "0";

            var defaultFinancialIncomeAccounting = !string.IsNullOrEmpty(form["FinancialIncome_AccountingOption_Default"]) ? form["FinancialIncome_AccountingOption_Default"].ToString() : "0";
            var debitFinancialIncomeAccounting = !string.IsNullOrEmpty(form["FinancialIncome_AccountingOption_Debit"]) ? form["FinancialIncome_AccountingOption_Debit"].ToString() : "0";
            var creditFinancialIncomeAccounting = !string.IsNullOrEmpty(form["FinancialIncome_AccountingOption_Credit"]) ? form["FinancialIncome_AccountingOption_Credit"].ToString() : "0";

            var defaultCostContractAccounting = !string.IsNullOrEmpty(form["CostContract_AccountingOption_Default"]) ? form["CostContract_AccountingOption_Default"].ToString() : "0";
            var debitCostContractAccounting = !string.IsNullOrEmpty(form["CostContract_AccountingOption_Debit"]) ? form["CostContract_AccountingOption_Debit"].ToString() : "0";
            var creditCostContractAccounting = !string.IsNullOrEmpty(form["CostContract_AccountingOption_Credit"]) ? form["CostContract_AccountingOption_Credit"].ToString() : "0";

            //销售单(收款账户)
            model.SaleFinanceAccountingMap.DefaultOption = int.Parse(defaultSaleBillAccounting);
            model.SaleFinanceAccountingMap.DebitOption = int.Parse(debitSaleBillAccounting);
            model.SaleFinanceAccountingMap.CreditOption = int.Parse(creditSaleBillAccounting);
            foreach (var option in options)
            {
                option.IsDefault = int.Parse(defaultSaleBillAccounting) == option.Id;
                var optionId = !string.IsNullOrEmpty(form["Sale_AccountingOption_" + option.Id]) ? form["Sale_AccountingOption_" + option.Id].ToString() : "0";
                if (options1.Select(o => o.Id).Contains(int.Parse(optionId)))
                {
                    model.SaleFinanceAccountingMap.Options.Add(option);
                }
            }
            //销售订单(收款账户)
            model.SaleReservationBillFinanceAccountingMap.DefaultOption = int.Parse(defaultSaleReservationBillAccounting);
            model.SaleReservationBillFinanceAccountingMap.DebitOption = int.Parse(debitSaleReservationBillAccounting);
            model.SaleReservationBillFinanceAccountingMap.CreditOption = int.Parse(creditSaleReservationBillAccounting);
            foreach (var option in options1)
            {
                option.IsDefault = int.Parse(defaultSaleReservationBillAccounting) == option.Id;
                var optionId = !string.IsNullOrEmpty(form["SaleReservationBill_AccountingOption_" + option.Id]) ? form["SaleReservationBill_AccountingOption_" + option.Id].ToString() : "0";
                if (options2.Select(o => o.Id).Contains(int.Parse(optionId)))
                {
                    model.SaleReservationBillFinanceAccountingMap.Options.Add(option);
                }
            }

            //退货单(收款账户)
            model.ReturnFinanceAccountingMap.DefaultOption = int.Parse(defaultReturnBillAccounting);
            model.ReturnFinanceAccountingMap.DebitOption = int.Parse(debitReturnBillAccounting);
            model.ReturnFinanceAccountingMap.CreditOption = int.Parse(creditReturnBillAccounting);
            foreach (var option in options2)
            {
                option.IsDefault = int.Parse(defaultReturnBillAccounting) == option.Id;
                var optionId = !string.IsNullOrEmpty(form["Return_AccountingOption_" + option.Id]) ? form["Return_AccountingOption_" + option.Id].ToString() : "0";
                if (options.Select(o => o.Id).Contains(int.Parse(optionId)))
                {
                    model.ReturnFinanceAccountingMap.Options.Add(option);
                }
            }
            //退货订单(收款账户)
            model.ReturnReservationFinanceAccountingMap.DefaultOption = int.Parse(defaultReturnReservationBillAccounting);
            model.ReturnReservationFinanceAccountingMap.DebitOption = int.Parse(debitReturnReservationBillAccounting);
            model.ReturnReservationFinanceAccountingMap.CreditOption = int.Parse(creditReturnReservationBillAccounting);
            foreach (var option in options3)
            {
                option.IsDefault = int.Parse(defaultReturnReservationBillAccounting) == option.Id;
                var optionId = !string.IsNullOrEmpty(form["ReturnReservation_AccountingOption_" + option.Id]) ? form["ReturnReservation_AccountingOption_" + option.Id].ToString() : "0";
                if (options.Select(o => o.Id).Contains(int.Parse(optionId)))
                {
                    model.ReturnReservationFinanceAccountingMap.Options.Add(option);
                }
            }

            //采购单(收款账户)
            model.PurchaseFinanceAccountingMap.DefaultOption = int.Parse(defaultPurchaseBillAccounting);
            model.PurchaseFinanceAccountingMap.DebitOption = int.Parse(debitPurchaseBillAccounting);
            model.PurchaseFinanceAccountingMap.CreditOption = int.Parse(creditPurchaseBillAccounting);
            foreach (var option in options4)
            {
                option.IsDefault = int.Parse(defaultPurchaseBillAccounting) == option.Id;
                var optionId = !string.IsNullOrEmpty(form["Purchase_AccountingOption_" + option.Id]) ? form["Purchase_AccountingOption_" + option.Id].ToString() : "0";
                if (options.Select(o => o.Id).Contains(int.Parse(optionId)))
                {
                    model.PurchaseFinanceAccountingMap.Options.Add(option);
                }
            }
            //采购退货单(收款账户)
            model.PurchaseReturnFinanceAccountingMap.DefaultOption = int.Parse(defaultPurchaseReturnBillAccounting);
            model.PurchaseReturnFinanceAccountingMap.DebitOption = int.Parse(debitPurchaseReturnBillAccounting);
            model.PurchaseReturnFinanceAccountingMap.CreditOption = int.Parse(creditPurchaseReturnBillAccounting);
            foreach (var option in options5)
            {
                option.IsDefault = int.Parse(defaultPurchaseReturnBillAccounting) == option.Id;
                var optionId = !string.IsNullOrEmpty(form["PurchaseReturn_AccountingOption_" + option.Id]) ? form["PurchaseReturn_AccountingOption_" + option.Id].ToString() : "0";
                if (options.Select(o => o.Id).Contains(int.Parse(optionId)))
                {
                    model.PurchaseReturnFinanceAccountingMap.Options.Add(option);
                }
            }

            //盘点盈亏单(盘点盈亏账户)
            model.InventoryProfitLossFinanceAccountingMap.DefaultOption = int.Parse(defaultInventoryProfitLossAccounting);
            model.InventoryProfitLossFinanceAccountingMap.DebitOption = int.Parse(debitInventoryProfitLossAccounting);
            model.InventoryProfitLossFinanceAccountingMap.CreditOption = int.Parse(creditInventoryProfitLossAccounting);
            foreach (var option in options6)
            {
                option.IsDefault = int.Parse(defaultInventoryProfitLossAccounting) == option.Id;
                var optionId = !string.IsNullOrEmpty(form["InventoryProfitLoss_AccountingOption_" + option.Id]) ? form["InventoryProfitLoss_AccountingOption_" + option.Id].ToString() : "0";
                if (options.Select(o => o.Id).Contains(int.Parse(optionId)))
                {
                    model.InventoryProfitLossFinanceAccountingMap.Options.Add(option);
                }
            }

            //报损单(报损账户)
            model.ScrapProductFinanceAccountingMap.DefaultOption = int.Parse(defaultScrapProductAccounting);
            model.ScrapProductFinanceAccountingMap.DebitOption = int.Parse(debitScrapProductAccounting);
            model.ScrapProductFinanceAccountingMap.CreditOption = int.Parse(creditScrapProductAccounting);
            foreach (var option in options7)
            {
                option.IsDefault = int.Parse(defaultScrapProductAccounting) == option.Id;
                var optionId = !string.IsNullOrEmpty(form["ScrapProduct_AccountingOption_" + option.Id]) ? form["ScrapProduct_AccountingOption_" + option.Id].ToString() : "0";
                if (options.Select(o => o.Id).Contains(int.Parse(optionId)))
                {
                    model.ScrapProductFinanceAccountingMap.Options.Add(option);
                }
            }

            //收款单(收款账户)
            model.ReceiptFinanceAccountingMap.DefaultOption = int.Parse(defaultReceiptAccounting);
            model.ReceiptFinanceAccountingMap.DebitOption = int.Parse(debitReceiptAccounting);
            model.ReceiptFinanceAccountingMap.CreditOption = int.Parse(creditReceiptAccounting);
            foreach (var option in options8)
            {
                option.IsDefault = int.Parse(defaultReceiptAccounting) == option.Id;
                var optionId = !string.IsNullOrEmpty(form["Receipt_AccountingOption_" + option.Id]) ? form["Receipt_AccountingOption_" + option.Id].ToString() : "0";
                if (options.Select(o => o.Id).Contains(int.Parse(optionId)))
                {
                    model.ReceiptFinanceAccountingMap.Options.Add(option);
                }
            }

            //付款单(收款账户)
            model.PaymentFinanceAccountingMap.DefaultOption = int.Parse(defaultPaymentAccounting);
            model.PaymentFinanceAccountingMap.DebitOption = int.Parse(debitPaymentAccounting);
            model.PaymentFinanceAccountingMap.CreditOption = int.Parse(creditPaymentAccounting);
            foreach (var option in options9)
            {
                option.IsDefault = int.Parse(defaultPaymentAccounting) == option.Id;
                var optionId = !string.IsNullOrEmpty(form["Payment_AccountingOption_" + option.Id]) ? form["Payment_AccountingOption_" + option.Id].ToString() : "0";
                if (options.Select(o => o.Id).Contains(int.Parse(optionId)))
                {
                    model.PaymentFinanceAccountingMap.Options.Add(option);
                }
            }


            //预收款单(收款账户)
            model.AdvanceReceiptFinanceAccountingMap.DefaultOption = int.Parse(defaultAdvanceReceiptAccounting);
            model.AdvanceReceiptFinanceAccountingMap.DebitOption = int.Parse(debitAdvanceReceiptAccounting);
            model.AdvanceReceiptFinanceAccountingMap.CreditOption = int.Parse(creditAdvanceReceiptAccounting);
            foreach (var option in options10)
            {
                option.IsDefault = int.Parse(defaultAdvanceReceiptAccounting) == option.Id;
                var optionId = !string.IsNullOrEmpty(form["AdvanceReceipt_AccountingOption_" + option.Id]) ? form["AdvanceReceipt_AccountingOption_" + option.Id].ToString() : "0";
                if (options.Select(o => o.Id).Contains(int.Parse(optionId)))
                {
                    model.AdvanceReceiptFinanceAccountingMap.Options.Add(option);
                }
            }

            //预付款单(付款款账户)
            model.AdvancePaymentFinanceAccountingMap.DefaultOption = int.Parse(defaultAdvancePaymentAccounting);
            model.AdvancePaymentFinanceAccountingMap.DebitOption = int.Parse(debitAdvancePaymentAccounting);
            model.AdvancePaymentFinanceAccountingMap.CreditOption = int.Parse(creditAdvancePaymentAccounting);
            foreach (var option in options11)
            {
                option.IsDefault = int.Parse(defaultAdvancePaymentAccounting) == option.Id;
                var optionId = !string.IsNullOrEmpty(form["AdvancePayment_AccountingOption_" + option.Id]) ? form["AdvancePayment_AccountingOption_" + option.Id].ToString() : "0";
                if (options.Select(o => o.Id).Contains(int.Parse(optionId)))
                {
                    model.AdvancePaymentFinanceAccountingMap.Options.Add(option);
                }
            }

            //费用支出（支出账户）
            model.CostExpenditureFinanceAccountingMap.DefaultOption = int.Parse(defaultCostExpenditureAccounting);
            model.CostExpenditureFinanceAccountingMap.DebitOption = int.Parse(debitCostExpenditureAccounting);
            model.CostExpenditureFinanceAccountingMap.CreditOption = int.Parse(creditCostExpenditureAccounting);
            foreach (var option in options12)
            {
                option.IsDefault = int.Parse(defaultCostExpenditureAccounting) == option.Id;
                var optionId = !string.IsNullOrEmpty(form["CostExpenditure_AccountingOption_" + option.Id]) ? form["CostExpenditure_AccountingOption_" + option.Id].ToString() : "0";
                if (options.Select(o => o.Id).Contains(int.Parse(optionId)))
                {
                    model.CostExpenditureFinanceAccountingMap.Options.Add(option);
                }
            }

            // 财务收入（收款账户） 
            model.FinancialIncomeFinanceAccountingMap.DefaultOption = int.Parse(defaultFinancialIncomeAccounting);
            model.FinancialIncomeFinanceAccountingMap.DebitOption = int.Parse(debitFinancialIncomeAccounting);
            model.FinancialIncomeFinanceAccountingMap.CreditOption = int.Parse(creditFinancialIncomeAccounting);
            foreach (var option in options13)
            {
                option.IsDefault = int.Parse(defaultFinancialIncomeAccounting) == option.Id;
                var optionId = !string.IsNullOrEmpty(form["FinancialIncome_AccountingOption_" + option.Id]) ? form["FinancialIncome_AccountingOption_" + option.Id].ToString() : "0";
                if (options.Select(o => o.Id).Contains(int.Parse(optionId)))
                {
                    model.FinancialIncomeFinanceAccountingMap.Options.Add(option);
                }
            }

            //费用合同
            model.CostContractAccountingMap.DefaultOption = int.Parse(defaultCostContractAccounting);
            model.CostContractAccountingMap.DebitOption = int.Parse(debitCostContractAccounting);
            model.CostContractAccountingMap.CreditOption = int.Parse(creditCostContractAccounting);
            foreach (var option in options14)
            {
                option.IsDefault = int.Parse(defaultCostContractAccounting) == option.Id;
                var optionId = !string.IsNullOrEmpty(form["CostContract_AccountingOption_" + option.Id]) ? form["CostContract_AccountingOption_" + option.Id].ToString() : "0";
                if (options.Select(o => o.Id).Contains(int.Parse(optionId)))
                {
                    model.CostContractAccountingMap.Options.Add(option);
                }
            }

            //转化
            fs.SaleBillAccountingOptionConfiguration = JsonConvert.SerializeObject(model.SaleFinanceAccountingMap);
            fs.SaleReservationBillAccountingOptionConfiguration = JsonConvert.SerializeObject(model.SaleReservationBillFinanceAccountingMap);

            fs.ReturnBillAccountingOptionConfiguration = JsonConvert.SerializeObject(model.ReturnFinanceAccountingMap);
            fs.ReturnReservationBillAccountingOptionConfiguration = JsonConvert.SerializeObject(model.ReturnReservationFinanceAccountingMap);

            fs.PurchaseBillAccountingOptionConfiguration = JsonConvert.SerializeObject(model.PurchaseFinanceAccountingMap);
            fs.PurchaseReturnBillAccountingOptionConfiguration = JsonConvert.SerializeObject(model.PurchaseReturnFinanceAccountingMap);

            fs.InventoryProfitLossAccountingOptionConfiguration = JsonConvert.SerializeObject(model.InventoryProfitLossFinanceAccountingMap);
            fs.ScrapProductAccountingOptionConfiguration = JsonConvert.SerializeObject(model.ScrapProductFinanceAccountingMap);

            fs.ReceiptAccountingOptionConfiguration = JsonConvert.SerializeObject(model.ReceiptFinanceAccountingMap);
            fs.PaymentAccountingOptionConfiguration = JsonConvert.SerializeObject(model.PaymentFinanceAccountingMap);

            fs.AdvanceReceiptAccountingOptionConfiguration = JsonConvert.SerializeObject(model.AdvanceReceiptFinanceAccountingMap);
            fs.AdvancePaymentAccountingOptionConfiguration = JsonConvert.SerializeObject(model.AdvancePaymentFinanceAccountingMap);

            fs.CostExpenditureAccountingOptionConfiguration = JsonConvert.SerializeObject(model.CostExpenditureFinanceAccountingMap);

            fs.FinancialIncomeAccountingOptionConfiguration = JsonConvert.SerializeObject(model.FinancialIncomeFinanceAccountingMap);

            fs.CostContractAccountingOptionConfiguration = JsonConvert.SerializeObject(model.CostContractAccountingMap);

            //保存
            _settingService.SaveSetting(fs, x => x.SaleBillAccountingOptionConfiguration, storeId, false);
            _settingService.SaveSetting(fs, x => x.SaleReservationBillAccountingOptionConfiguration, storeId, false);

            _settingService.SaveSetting(fs, x => x.ReturnBillAccountingOptionConfiguration, storeId, false);
            _settingService.SaveSetting(fs, x => x.ReturnReservationBillAccountingOptionConfiguration, storeId, false);

            _settingService.SaveSetting(fs, x => x.PurchaseBillAccountingOptionConfiguration, storeId, false);
            _settingService.SaveSetting(fs, x => x.PurchaseReturnBillAccountingOptionConfiguration, storeId, false);

            _settingService.SaveSetting(fs, x => x.InventoryProfitLossAccountingOptionConfiguration, storeId, false);
            _settingService.SaveSetting(fs, x => x.ScrapProductAccountingOptionConfiguration, storeId, false);

            _settingService.SaveSetting(fs, x => x.ReceiptAccountingOptionConfiguration, storeId, false);
            _settingService.SaveSetting(fs, x => x.PaymentAccountingOptionConfiguration, storeId, false);

            _settingService.SaveSetting(fs, x => x.AdvanceReceiptAccountingOptionConfiguration, storeId, false);
            _settingService.SaveSetting(fs, x => x.AdvancePaymentAccountingOptionConfiguration, storeId, false);

            _settingService.SaveSetting(fs, x => x.CostExpenditureAccountingOptionConfiguration, storeId, false);

            _settingService.SaveSetting(fs, x => x.FinancialIncomeAccountingOptionConfiguration, storeId, false);

            _settingService.SaveSetting(fs, x => x.CostContractAccountingOptionConfiguration, storeId, false);


            //清除缓存
            _settingService.ClearCache(storeId);

            //日志
            _userActivityService.InsertActivity("EditSettings", "编辑配置", curUser);
            _notificationService.SuccessNotification("配置已经更新");


            return RedirectToAction("Finances");
        }

        #endregion

        #region APP打印设置

        [AuthCode((int)AccessGranularityEnum.AppPrintSettingView)]
        public IActionResult APPPrint()
        {

            //获取当前经销商配置
            var aps = _settingService.LoadSetting<APPPrintSetting>(_storeContext.CurrentStore.Id);
            //var model = aps.ToModel<APPPrintSettingModel>();
            var model = new APPPrintSettingModel
            {
                AllowPrintPackPrice = aps.AllowPrintPackPrice,
                PrintMode = aps.PrintMode,
                PrintingNumber = aps.PrintingNumber,
                AllowAutoPrintSalesAndReturn = aps.AllowAutoPrintSalesAndReturn,
                AllowAutoPrintOrderAndReturn = aps.AllowAutoPrintOrderAndReturn,
                AllowAutoPrintAdvanceReceipt = aps.AllowAutoPrintAdvanceReceipt,
                AllowAutoPrintArrears = aps.AllowAutoPrintArrears,
                AllowPrintOnePass = aps.AllowPrintOnePass,
                AllowPringMobile = aps.AllowPringMobile,
                AllowPrintingTimeAndNumber = aps.AllowPrintingTimeAndNumber,
                AllowPrintCustomerBalance = aps.AllowPrintCustomerBalance,
                PageHeaderText = aps.PageHeaderText,
                PageFooterText1 = aps.PageFooterText1,
                PageFooterText2 = aps.PageFooterText2,
                PageHeaderImage = aps.PageHeaderImage,
                PageFooterImage = aps.PageFooterImage,

                //打印模式
                //IEnumerable<PrintMode> attributes = Enum.GetValues(typeof(PrintMode)).Cast<PrintMode>();
                //var printModes = from a in attributes
                //                 select new SelectListItem
                //                 {
                //                     Text = CommonHelper.GetEnumDescription(a),
                //                     Value = ((int)a).ToString()
                //                 };
                //model.PrintModes = new SelectList(printModes, "Value", "Text");

                PrintModes = new SelectList(from a in Enum.GetValues(typeof(PrintMode)).Cast<PrintMode>()
                                            select new SelectListItem
                                            {
                                                Text = CommonHelper.GetEnumDescription(a),
                                                Value = ((int)a).ToString()
                                            }, "Value", "Text")
            };

            return View(model);
        }
        [HttpPost]
        [AuthCode((int)AccessGranularityEnum.AppPrintSettingUpdate)]
        public IActionResult APPPrint(APPPrintSettingModel model)
        {

            //获取当前经销商配置
            var storeId = _storeContext.CurrentStore.Id;
            var aps = _settingService.LoadSetting<APPPrintSetting>(storeId);
            //aps = model.ToEntity(aps);
            aps.AllowPrintPackPrice = model.AllowPrintPackPrice;
            aps.PrintMode = model.PrintMode;
            aps.PrintingNumber = model.PrintingNumber;
            aps.AllowAutoPrintSalesAndReturn = model.AllowAutoPrintSalesAndReturn;
            aps.AllowAutoPrintOrderAndReturn = model.AllowAutoPrintOrderAndReturn;
            aps.AllowAutoPrintAdvanceReceipt = model.AllowAutoPrintAdvanceReceipt;
            aps.AllowAutoPrintArrears = model.AllowAutoPrintArrears;
            aps.AllowPrintOnePass = model.AllowPrintOnePass;
            aps.AllowPringMobile = model.AllowPringMobile;
            aps.AllowPrintingTimeAndNumber = model.AllowPrintingTimeAndNumber;
            aps.AllowPrintCustomerBalance = model.AllowPrintCustomerBalance;
            aps.PageHeaderText = model.PageHeaderText;
            aps.PageFooterText1 = model.PageFooterText1;
            aps.PageFooterText2 = model.PageFooterText2;
            aps.PageHeaderImage = model.PageHeaderImage;
            aps.PageFooterImage = model.PageFooterImage;

            _settingService.SaveSetting(aps, x => x.AllowPrintPackPrice, storeId, false);
            _settingService.SaveSetting(aps, x => x.PrintMode, storeId, false);
            _settingService.SaveSetting(aps, x => x.PrintingNumber, storeId, false);
            _settingService.SaveSetting(aps, x => x.AllowAutoPrintSalesAndReturn, storeId, false);
            _settingService.SaveSetting(aps, x => x.AllowAutoPrintOrderAndReturn, storeId, false);
            _settingService.SaveSetting(aps, x => x.AllowAutoPrintAdvanceReceipt, storeId, false);
            _settingService.SaveSetting(aps, x => x.AllowAutoPrintArrears, storeId, false);
            _settingService.SaveSetting(aps, x => x.AllowPrintOnePass, storeId, false);
            _settingService.SaveSetting(aps, x => x.AllowPringMobile, storeId, false);
            _settingService.SaveSetting(aps, x => x.AllowPrintingTimeAndNumber, storeId, false);
            _settingService.SaveSetting(aps, x => x.AllowPrintCustomerBalance, storeId, false);
            _settingService.SaveSetting(aps, x => x.PageHeaderText, storeId, false);
            _settingService.SaveSetting(aps, x => x.PageFooterText1, storeId, false);
            _settingService.SaveSetting(aps, x => x.PageFooterText2, storeId, false);
            _settingService.SaveSetting(aps, x => x.PageHeaderImage, storeId, false);
            _settingService.SaveSetting(aps, x => x.PageFooterImage, storeId, false);

            //清除缓存
            _settingService.ClearCache(storeId);

            //日志
            _userActivityService.InsertActivity("EditSettings", "编辑配置", curUser);
            _notificationService.SuccessNotification("配置已经更新");
            return RedirectToAction("APPPrint");
        }

        #endregion

        #region 库存预警配置

        [HttpGet]
        [AuthCode((int)AccessGranularityEnum.StockWarningSettingView)]
        public IActionResult StockEarlyWarningList(string key = "", int? wareHouseId = 0, int pageNumber = 0)
        {

            if (pageNumber > 0)
            {
                pageNumber -= 1;
            }

            var model = new StockEarlyWarningListModel
            {
                WareHouses = BindWareHouseSelection(_wareHouseService.BindWareHouseList, curStore,null,0),
                WareHouseId = wareHouseId == 0 ? null : wareHouseId
            };

            var lists = _stockEarlyWarningService.GetAllStockEarlyWarnings(curStore?.Id ?? 0, key, pageNumber, 30);
            model.PagingFilteringContext.LoadPagedList(lists);
            var allProducts = _productService.GetProductsByIds(curStore.Id, lists.Select(pr => pr.ProductId).Distinct().ToArray());
            var allOptions = _specificationAttributeService.GetSpecificationAttributeOptionByIds(curStore.Id, allProducts.GetProductBigStrokeSmallUnitIds());
            var allProductPrices = _productService.GetProductPricesByProductIds(curStore.Id, lists.Select(pr => pr.ProductId).Distinct().ToArray());
            var allProductTierPrices = _productService.GetProductTierPriceByProductIds(curStore.Id, lists.Select(pr => pr.ProductId).Distinct().ToArray());

            model.Items = lists.Select(s =>
            {
                var m = s.ToModel<StockEarlyWarningModel>();
                var product = allProducts.Where(ap => ap.Id == m.ProductId).FirstOrDefault();

                if (product != null)
                {
                    //这里替换成高级用法
                    m = product.InitBaseModel<StockEarlyWarningModel>(m, wareHouseId, allOptions, allProductPrices, allProductTierPrices, _productService);
                }
                return m;

            }).ToList();

            return View(model);
        }

        [HttpGet]
        [AuthCode((int)AccessGranularityEnum.StockWarningSettingUpdate)]
        public IActionResult StockEarlyWarningCreate()
        {

            var model = new StockEarlyWarningModel
            {
                WareHouses = BindWareHouseSelection(_wareHouseService.BindWareHouseList, curStore,null,0),
                WareHouseId = -1,
                UnitLists = new SelectList(new List<SelectListItem>() { new SelectListItem() { Text = "", Value = "" } }, "Value", "Text")
            };
            return View(model);
        }
        [HttpPost, ParameterBasedOnFormNameAttribute("save-continue", "continueEditing")]

        [AuthCode((int)AccessGranularityEnum.StockWarningSettingUpdate)]
        public IActionResult StockEarlyWarningCreate(StockEarlyWarningModel model, bool continueEditing)
        {

            if (model.WareHouseId <= 0)
            {
                ModelState.AddModelError("", "请选择仓库");
            }

            if (model.ProductId <= 0)
            {
                ModelState.AddModelError("", "请选择商品");
            }

            if (model.UnitId <= 0)
            {
                ModelState.AddModelError("", "请选择商品单位");
            }

            if (model.ShortageWarningQuantity == 0)
            {
                ModelState.AddModelError("", "请确认积压预警数");
            }

            if (model.BacklogWarningQuantity == 0)
            {
                ModelState.AddModelError("", "请确认缺货预警数");
            }

            if (_stockEarlyWarningService.CheckExists(model.ProductId, model.WareHouseId))
            {
                ModelState.AddModelError("", "该商品预警配置已存在");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var waring = model.ToEntity<StockEarlyWarning>();

                    waring.StoreId = curStore?.Id ?? 0;
                    waring.CreatedOnUtc = DateTime.Now;

                    _stockEarlyWarningService.InsertStockEarlyWarning(waring);

                    //activity log
                    _userActivityService.InsertActivity("InsertStockEarlyWarning", "添加预警成功", curUser, waring.ProductName);
                    _notificationService.SuccessNotification("添加预警成功");
                    return continueEditing ? RedirectToAction("StockEarlyWarningEdit", new { id = waring.Id }) : RedirectToAction("StockEarlyWarningList");
                }
                catch (Exception ex)
                {
                    _userActivityService.InsertActivity("InsertStockEarlyWarning", "添加预警失败", curUser, model.ProductName);
                    _notificationService.ErrorNotification("添加预警失败:" + ex.Message);
                    return RedirectToAction("StockEarlyWarningList");
                }
            }

            model.UnitLists = new SelectList(new List<SelectListItem>() { new SelectListItem() { Text = "", Value = "" } }, "Value", "Text");
            model.WareHouses = BindWareHouseSelection(_wareHouseService.BindWareHouseList, curStore,null,0);
            model.WareHouseId = -1;

            return View(model);
        }

        [HttpGet]
        [AuthCode((int)AccessGranularityEnum.StockWarningSettingUpdate)]
        public IActionResult StockEarlyWarningEdit(int? id)
        {

            var model = new StockEarlyWarningModel();

            var waring = _stockEarlyWarningService.GetStockEarlyWarningById(curStore.Id, id ?? 0);
            if (waring != null)
            {
                model = waring.ToModel<StockEarlyWarningModel>();
            }

            var product = _productService.GetProductById(curStore.Id, model.ProductId);
            if (product != null)
            {
                model.UnitLists = BindOptionSelection(product.GetProductUnits(_productService, _specificationAttributeService));
            }

            model.WareHouses = BindWareHouseSelection(_wareHouseService.BindWareHouseList, curStore,null,0);

            return View(model);
        }
        [HttpPost, ParameterBasedOnFormNameAttribute("save-continue", "continueEditing")]
        [AuthCode((int)AccessGranularityEnum.StockWarningSettingUpdate)]
        public IActionResult StockEarlyWarningEdit(StockEarlyWarningModel model, bool continueEditing)
        {

            if (model.WareHouseId == 0)
            {
                ModelState.AddModelError("", "请选择仓库");
            }

            if (model.ProductId == 0)
            {
                ModelState.AddModelError("", "请选择商品");
            }

            if (model.UnitId == 0)
            {
                ModelState.AddModelError("", "请选择商品单位");
            }

            if (model.ShortageWarningQuantity == 0)
            {
                ModelState.AddModelError("", "请确认积压预警数");
            }

            if (model.BacklogWarningQuantity == 0)
            {
                ModelState.AddModelError("", "请确认缺货预警数");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var waring = _stockEarlyWarningService.GetStockEarlyWarningById(curStore.Id, model.Id);

                    if (waring != null)
                    {

                        if (!_stockEarlyWarningService.CheckExists(model.Id, waring.ProductId, waring.WareHouseId))
                        {
                            waring = model.ToEntity(waring);
                            waring.StoreId = curStore?.Id ?? 0;

                            _stockEarlyWarningService.UpdateStockEarlyWarning(waring);
                            //activity log
                            _userActivityService.InsertActivity("UpdateStockEarlyWarning", "修改预警成功", curUser, waring.ProductName);
                            _notificationService.SuccessNotification("修改预警成功");
                            return continueEditing ? RedirectToAction("StockEarlyWarningEdit", new { id = waring.Id }) : RedirectToAction("StockEarlyWarningList");
                        }
                        else
                        {
                            _userActivityService.InsertActivity("UpdateStockEarlyWarning", "添加预警失败", curUser, model.ProductName);
                            _notificationService.ErrorNotification("添加预警失败:商品已经存在");
                            return RedirectToAction("StockEarlyWarningList");
                        }
                    }
                    else
                    {
                        _userActivityService.InsertActivity("UpdateStockEarlyWarning", "修改预警失败", curUser, model.ProductName);
                        _notificationService.ErrorNotification("修改预警失败:" + "信息不存在");
                        return RedirectToAction("StockEarlyWarningList");
                    }

                }
                catch (Exception ex)
                {
                    _userActivityService.InsertActivity("UpdateStockEarlyWarning", "修改预警失败", curUser, model.ProductName);
                    _notificationService.ErrorNotification("修改预警失败:" + ex.Message);
                    return RedirectToAction("StockEarlyWarningList");
                }
            }

            var product = _productService.GetProductById(curStore.Id, model.ProductId);
            if (product != null)
            {
                model.UnitLists = BindOptionSelection(product.GetProductUnits(_productService, _specificationAttributeService));
            }

            model.WareHouses = BindWareHouseSelection(_wareHouseService.BindWareHouseList, curStore,null,0);
            model.WareHouseId = -1;
            return View(model);
        }
        [HttpGet]
        [AuthCode((int)AccessGranularityEnum.StockWarningSettingUpdate)]
        public IActionResult StockEarlyWarningDelete(string ids)
        {

            if (!string.IsNullOrEmpty(ids))
            {
                int[] sids = ids.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(u => int.Parse(u)).ToArray();
                var warings = _stockEarlyWarningService.GetStockEarlyWarningsByIds(sids);
                foreach (var waring in warings)
                {
                    if (waring != null)
                    {
                        _stockEarlyWarningService.DeleteStockEarlyWarning(waring);
                    }
                }
                //活动日志
                _userActivityService.InsertActivity("DeleteStockEarlyWarning", "删除方案", curUser, ids);
                _notificationService.SuccessNotification("删除成功");
            }

            return RedirectToAction("StockEarlyWarningList");
        }

        #endregion

        #region 电脑打印信息设置

        [AuthCode((int)AccessGranularityEnum.PcPrintSettingView)]
        public IActionResult PCPrint()
        {

            //获取当前经销商配置
            var pcps = _settingService.LoadSetting<PCPrintSetting>(_storeContext.CurrentStore.Id);

            //var model = pcps.ToModel();
            var model = new PCPrintSettingModel
            {
                PrintMethods = new SelectList(from a in Enum.GetValues(typeof(PrintMethod)).Cast<PrintMethod>()
                                              select new SelectListItem
                                              {
                                                  Text = CommonHelper.GetEnumDescription(a),
                                                  Value = ((int)a).ToString()
                                              }, "Value", "Text"),


                PaperTypes = new SelectList(from a in Enum.GetValues(typeof(PaperType)).Cast<PaperType>()
                                            select new SelectListItem
                                            {
                                                Text = CommonHelper.GetEnumDescription(a),
                                                Value = ((int)a).ToString()
                                            }, "Value", "Text"),



                BorderTypes = new SelectList(from a in Enum.GetValues(typeof(PrintType)).Cast<PrintType>()
                                             select new SelectListItem
                                             {
                                                 Text = CommonHelper.GetEnumDescription(a),
                                                 Value = ((int)a).ToString()
                                             }, "Value", "Text")
            };

            model.StoreName = pcps.StoreName;
            model.Address = pcps.Address;
            model.PlaceOrderTelphone = pcps.PlaceOrderTelphone;
            model.PrintMethod = pcps.PrintMethod;
            model.PaperType = pcps.PaperType;
            model.PaperWidth = pcps.PaperWidth;
            model.PaperHeight = pcps.PaperHeight;
            model.BorderType = pcps.BorderType;
            model.MarginTop = pcps.MarginTop;
            model.MarginBottom = pcps.MarginBottom;
            model.MarginLeft = pcps.MarginLeft;
            model.MarginRight = pcps.MarginRight;
            model.IsPrintPageNumber = pcps.IsPrintPageNumber;
            model.PrintHeader = pcps.PrintHeader;
            model.PrintFooter = pcps.PrintFooter;
            model.IsFixedRowNumber = pcps.IsFixedRowNumber;
            model.FixedRowNumber = pcps.FixedRowNumber;
            model.PrintSubtotal = pcps.PrintSubtotal;
            model.PrintPort = pcps.PrintPort;
            //每页打印页眉页脚配置 2021-09-10 mu 添加
            model.PrintInAllPages = pcps.PrintInAllPages;
            model.PageRowsCount = pcps.PageRowsCount;
            model.HeaderHeight = pcps.HeaderHeight;
            model.FooterHeight = pcps.FooterHeight;

            return View(model);
        }

        [HttpPost]
        [AuthCode((int)AccessGranularityEnum.PcPrintSettingUpdate)]
        public IActionResult PCPrint(PCPrintSettingModel model)
        {
            //获取当前经销商配置
            var storeId = _storeContext.CurrentStore.Id;
            var aps = _settingService.LoadSetting<PCPrintSetting>(storeId);
            //aps = model.ToEntity(aps);
            aps.StoreName = model.StoreName;
            aps.Address = model.Address;
            aps.PlaceOrderTelphone = model.PlaceOrderTelphone;
            aps.PrintMethod = model.PrintMethod;
            aps.PaperType = model.PaperType;
            aps.PaperWidth = model.PaperWidth;
            aps.PaperHeight = model.PaperHeight;
            aps.BorderType = model.BorderType;
            aps.MarginTop = model.MarginTop;
            aps.MarginBottom = model.MarginBottom;
            aps.MarginLeft = model.MarginLeft;
            aps.MarginRight = model.MarginRight;
            aps.IsPrintPageNumber = model.IsPrintPageNumber;
            aps.PrintHeader = model.PrintHeader;
            aps.PrintFooter = model.PrintFooter;
            aps.IsFixedRowNumber = model.IsFixedRowNumber;
            aps.FixedRowNumber = model.FixedRowNumber;
            aps.PrintSubtotal = model.PrintSubtotal;
            aps.PrintPort = model.PrintPort;
            //每页打印页眉页脚配置 2021-09-10 mu 添加
            aps.PrintInAllPages = model.PrintInAllPages;
            aps.PageRowsCount = model.PageRowsCount;
            aps.HeaderHeight = model.HeaderHeight;
            aps.FooterHeight = model.FooterHeight;

            _settingService.SaveSetting(aps, x => x.StoreName, storeId, false);
            _settingService.SaveSetting(aps, x => x.Address, storeId, false);
            _settingService.SaveSetting(aps, x => x.PlaceOrderTelphone, storeId, false);
            _settingService.SaveSetting(aps, x => x.PrintMethod, storeId, false);
            _settingService.SaveSetting(aps, x => x.PaperType, storeId, false);
            _settingService.SaveSetting(aps, x => x.PaperWidth, storeId, false);
            _settingService.SaveSetting(aps, x => x.PaperHeight, storeId, false);
            _settingService.SaveSetting(aps, x => x.BorderType, storeId, false);
            _settingService.SaveSetting(aps, x => x.MarginTop, storeId, false);
            _settingService.SaveSetting(aps, x => x.MarginBottom, storeId, false);
            _settingService.SaveSetting(aps, x => x.MarginLeft, storeId, false);
            _settingService.SaveSetting(aps, x => x.MarginRight, storeId, false);
            _settingService.SaveSetting(aps, x => x.IsPrintPageNumber, storeId, false);
            _settingService.SaveSetting(aps, x => x.PrintHeader, storeId, false);
            _settingService.SaveSetting(aps, x => x.PrintFooter, storeId, false);
            _settingService.SaveSetting(aps, x => x.IsFixedRowNumber, storeId, false);
            _settingService.SaveSetting(aps, x => x.FixedRowNumber, storeId, false);
            _settingService.SaveSetting(aps, x => x.PrintSubtotal, storeId, false);
            _settingService.SaveSetting(aps, x => x.PrintPort, storeId, false);
            //每页打印页眉页脚配置 2021-09-10 mu 添加
            _settingService.SaveSetting(aps, x => x.PrintInAllPages,storeId,false);
            _settingService.SaveSetting(aps, x => x.PageRowsCount,storeId,false);
            _settingService.SaveSetting(aps, x => x.HeaderHeight, storeId, false);
            _settingService.SaveSetting(aps, x => x.FooterHeight, storeId, false);

            //清除缓存
            _settingService.ClearCache(storeId);

            //日志
            _userActivityService.InsertActivity("PCPrint", "编辑配置", curUser);
            _notificationService.SuccessNotification("配置已经更新");
            return RedirectToAction("PCPrint");
        }

        #endregion

        #region 公司设置

        [AuthCode((int)AccessGranularityEnum.CompanySettingView)]
        public IActionResult cst()
        {

            //获取当前经销商配置
            var cst = _settingService.LoadSetting<CompanySetting>(_storeContext.CurrentStore.Id);

            var model = new CompanySettingModel
            {
                OpenBillMakeDates = new SelectList(from a in Enum.GetValues(typeof(OpenBillMakeDate)).Cast<OpenBillMakeDate>()
                                                   select new SelectListItem
                                                   {
                                                       Text = CommonHelper.GetEnumDescription(a),
                                                       Value = ((int)a).ToString()
                                                   }, "Value", "Text"),

                MulProductPriceUnits = new SelectList(from a in Enum.GetValues(typeof(MuitProductPriceUnit)).Cast<MuitProductPriceUnit>()
                                                      select new SelectListItem
                                                      {
                                                          Text = CommonHelper.GetEnumDescription(a),
                                                          Value = ((int)a).ToString()
                                                      }, "Value", "Text"),


                DefaultPurchasePrices = new SelectList(from a in Enum.GetValues(typeof(DefaultPurchasePrice)).Cast<DefaultPurchasePrice>()
                                                       select new SelectListItem
                                                       {
                                                           Text = CommonHelper.GetEnumDescription(a),
                                                           Value = ((int)a).ToString()
                                                       }, "Value", "Text"),

                VariablePriceCommodities = new SelectList(from a in Enum.GetValues(typeof(VariablePriceCommodity)).Cast<VariablePriceCommodity>()
                                                          select new SelectListItem
                                                          {
                                                              Text = CommonHelper.GetEnumDescription(a),
                                                              Value = ((int)a).ToString()
                                                          }, "Value", "Text"),

                AccuracyRoundings = new SelectList(from a in Enum.GetValues(typeof(AccuracyRounding)).Cast<AccuracyRounding>()
                                                   select new SelectListItem
                                                   {
                                                       Text = CommonHelper.GetEnumDescription(a),
                                                       Value = ((int)a).ToString()
                                                   }, "Value", "Text"),

                MakeBillDisplayBarCodes = new SelectList(from a in Enum.GetValues(typeof(MakeBillDisplayBarCode)).Cast<MakeBillDisplayBarCode>()
                                                         select new SelectListItem
                                                         {
                                                             Text = CommonHelper.GetEnumDescription(a),
                                                             Value = ((int)a).ToString()
                                                         }, "Value", "Text"),

                ReferenceCostPrices = new SelectList(from a in Enum.GetValues(typeof(ReferenceCostPrice)).Cast<ReferenceCostPrice>()
                                                     select new SelectListItem
                                                     {
                                                         Text = CommonHelper.GetEnumDescription(a),
                                                         Value = ((int)a).ToString()
                                                     }, "Value", "Text")
            };

            //V3
            var sms = cst.SalesmanManagements;
            //Expression<Func<User, User>> exp = c => new User() { Id = c.Id, UserRealName = c.UserRealName };
            model.SalesmanManagements = _userService.GetAllUsers(_storeContext.CurrentStore.Id, "Salesmans")?.Select(s =>
            {
                var curt = sms?.Where(sm => sm.UserId == s.Key).FirstOrDefault();
                return new ViewModel.Models.Configuration.SalesmanManagement
                {
                    UserId = s.Key,
                    UserName = s.Value,
                    OnStoreStopSeconds = curt?.OnStoreStopSeconds ?? 10,
                    EnableSalesmanTrack = curt?.EnableSalesmanTrack ?? true,
                    Start = curt?.Start ?? "07:00",
                    End = curt?.End ?? "19:00",
                    FrequencyTimer = curt?.FrequencyTimer ?? 0,
                    SalesmanOnlySeeHisCustomer = curt?.SalesmanOnlySeeHisCustomer ?? true,
                    SalesmanVisitStoreBefore = curt?.SalesmanVisitStoreBefore ?? true,
                    SalesmanVisitMustPhotographed = curt?.SalesmanVisitMustPhotographed ?? true,
                    SalesmanDeliveryDistance = curt?.SalesmanDeliveryDistance ?? 50,
                    DoorheadPhotoNum = curt?.DoorheadPhotoNum ?? 1,
                    DisplayPhotoNum = curt?.DisplayPhotoNum ?? 4,
                    EnableBusinessTime = curt?.EnableBusinessTime ?? true,
                    BusinessStart = curt?.BusinessStart ?? "07:00",
                    BusinessEnd = curt?.BusinessEnd ?? "19:00",
                    EnableBusinessVisitLine = curt?.EnableBusinessVisitLine ?? true
                };
            })?.ToList();

            //默认售价下拉
            model.DefaultPricePlans = BindPricePlanSelection(_productTierPricePlanService.GetAllPricePlan, curStore);
            model.DefaultPolicyPrices = BindPricePlanSelection(_productTierPricePlanService.GetAllPricePlan, curStore);
            model.SubmitExchangeBillAutoAudits = cst.SubmitExchangeBillAutoAudits;
            model.OpenBillMakeDate = cst.OpenBillMakeDate;
            model.MulProductPriceUnit = cst.MulProductPriceUnit;
            model.AllowCreateMulSameBarcode = cst.AllowCreateMulSameBarcode;

            //默认售价选择
            model.DefaultPricePlan = cst.DefaultPricePlan;
            model.DefaultPolicyPrice = cst.DefaultPolicyPrice; //默认政策商品价格
            model.DefaultPurchasePrice = cst.DefaultPurchasePrice;
            model.VariablePriceCommodity = cst.VariablePriceCommodity;
            model.AccuracyRounding = cst.AccuracyRounding;
            model.MakeBillDisplayBarCode = cst.MakeBillDisplayBarCode;
            model.AllowSelectionDateRange = cst.AllowSelectionDateRange;
            model.DockingTicketPassSystem = cst.DockingTicketPassSystem;
            model.AllowReturnInSalesAndOrders = cst.AllowReturnInSalesAndOrders;
            model.AppMaybeDeliveryPersonnel = cst.AppMaybeDeliveryPersonnel;
            model.AppSubmitOrderAutoAudits = cst.AppSubmitOrderAutoAudits;
            model.AppSubmitTransferAutoAudits = cst.AppSubmitTransferAutoAudits;
            model.AppSubmitExpenseAutoAudits = cst.AppSubmitExpenseAutoAudits;
            model.AppSubmitBillReturnAutoAudits = cst.AppSubmitBillReturnAutoAudits;
            model.AppAllowWriteBack = cst.AppAllowWriteBack;
            model.AllowAdvancePaymentsNegative = cst.AllowAdvancePaymentsNegative;
            model.ShowOnlyPrepaidAccountsWithPrepaidReceipts = cst.ShowOnlyPrepaidAccountsWithPrepaidReceipts;
            model.TasteByTasteAccountingOnlyPrintMainProduct = cst.TasteByTasteAccountingOnlyPrintMainProduct;
            model.AutoApproveConsumerPaidBill = cst.AutoApproveConsumerPaidBill;
            model.APPOnlyShowHasStockProduct = cst.APPOnlyShowHasStockProduct;
            model.APPShowOrderStock = cst.APPShowOrderStock;


            model.SalesmanDeliveryDistance = cst.SalesmanDeliveryDistance;
            model.DoorheadPhotoNum = cst.DoorheadPhotoNum;
            model.DisplayPhotoNum = cst.DisplayPhotoNum;
            model.OnStoreStopSeconds = cst.OnStoreStopSeconds;
            model.EnableSalesmanTrack = cst.EnableSalesmanTrack;
            model.Start = cst.Start;
            model.End = cst.End;
            model.FrequencyTimer = cst.FrequencyTimer;
            model.SalesmanOnlySeeHisCustomer = cst.SalesmanOnlySeeHisCustomer;
            model.SalesmanVisitStoreBefore = cst.SalesmanVisitStoreBefore;
            model.SalesmanVisitMustPhotographed = cst.SalesmanVisitMustPhotographed;
            model.EnableBusinessTime = cst.EnableBusinessTime;
            model.BusinessStart = cst.BusinessStart;
            model.BusinessEnd = cst.BusinessEnd;
            model.EnableBusinessVisitLine = cst.EnableBusinessVisitLine;


            model.ReferenceCostPrice = cst.ReferenceCostPrice == 0 ? 1 : cst.ReferenceCostPrice;
            model.AveragePurchasePriceCalcNumber = cst.AveragePurchasePriceCalcNumber;
            model.AllowNegativeInventoryMonthlyClosure = cst.AllowNegativeInventoryMonthlyClosure;
            model.EnableTaxRate = cst.EnableTaxRate;
            model.TaxRate = cst.TaxRate;
            model.PhotographedWater = cst.PhotographedWater;
            model.ClearArchiveDatas = cst.ClearArchiveDatas;
            model.ClearBillDatas = cst.ClearBillDatas;


            return View(model);
        }

        [HttpPost]
        [AuthCode((int)AccessGranularityEnum.CompanySettingUpdate)]
        public IActionResult cst(CompanySettingModel model, IFormCollection form)
        {

            try
            {
                //获取当前经销商配置
                var storeId = _storeContext.CurrentStore.Id;
                var cst = _settingService.LoadSetting<CompanySetting>(storeId);

                //确保时间间隔最少为10秒
                //cst = model.ToEntity(cst);
   
                cst.OpenBillMakeDate = model.OpenBillMakeDate;
                cst.MulProductPriceUnit = model.MulProductPriceUnit;
                cst.AllowCreateMulSameBarcode = model.AllowCreateMulSameBarcode;
                cst.DefaultPricePlan = model.DefaultPricePlan;
                cst.DefaultPolicyPrice = model.DefaultPolicyPrice; //默认政策价
                cst.DefaultPurchasePrice = model.DefaultPurchasePrice;
                cst.VariablePriceCommodity = model.VariablePriceCommodity;
                cst.AccuracyRounding = model.AccuracyRounding;
                cst.MakeBillDisplayBarCode = model.MakeBillDisplayBarCode;
                cst.AllowSelectionDateRange = model.AllowSelectionDateRange;
                cst.DockingTicketPassSystem = model.DockingTicketPassSystem;
                cst.AllowReturnInSalesAndOrders = model.AllowReturnInSalesAndOrders;
                cst.SubmitExchangeBillAutoAudits = model.SubmitExchangeBillAutoAudits;
                cst.AppMaybeDeliveryPersonnel = model.AppMaybeDeliveryPersonnel;
                cst.AppSubmitOrderAutoAudits = model.AppSubmitOrderAutoAudits;
                cst.AppSubmitTransferAutoAudits = model.AppSubmitTransferAutoAudits;
                cst.AppSubmitExpenseAutoAudits = model.AppSubmitExpenseAutoAudits;
                cst.AppSubmitBillReturnAutoAudits = model.AppSubmitBillReturnAutoAudits;
                cst.AppAllowWriteBack = model.AppAllowWriteBack;
                cst.AllowAdvancePaymentsNegative = model.AllowAdvancePaymentsNegative;
                cst.ShowOnlyPrepaidAccountsWithPrepaidReceipts = model.ShowOnlyPrepaidAccountsWithPrepaidReceipts;
                cst.TasteByTasteAccountingOnlyPrintMainProduct = model.TasteByTasteAccountingOnlyPrintMainProduct;
                cst.AutoApproveConsumerPaidBill = model.AutoApproveConsumerPaidBill;
                cst.APPOnlyShowHasStockProduct = model.APPOnlyShowHasStockProduct;
                cst.APPShowOrderStock = model.APPShowOrderStock;
                cst.SalesmanDeliveryDistance = model.SalesmanDeliveryDistance;
                cst.OnStoreStopSeconds = model.OnStoreStopSeconds;
                cst.EnableSalesmanTrack = model.EnableSalesmanTrack;

                cst.DisplayPhotoNum = model.DisplayPhotoNum;
                cst.DoorheadPhotoNum = model.DoorheadPhotoNum;

                cst.Start = model.Start;
                cst.End = model.End;
                cst.FrequencyTimer = model.FrequencyTimer;
                cst.SalesmanOnlySeeHisCustomer = model.SalesmanOnlySeeHisCustomer;
                cst.SalesmanVisitStoreBefore = model.SalesmanVisitStoreBefore;
                cst.SalesmanVisitMustPhotographed = model.SalesmanVisitMustPhotographed;
                cst.ReferenceCostPrice = model.ReferenceCostPrice;
                cst.AveragePurchasePriceCalcNumber = model.AveragePurchasePriceCalcNumber;
                cst.AllowNegativeInventoryMonthlyClosure = model.AllowNegativeInventoryMonthlyClosure;
                cst.EnableTaxRate = model.EnableTaxRate;
                cst.TaxRate = model.TaxRate;
                cst.PhotographedWater = model.PhotographedWater;
                cst.ClearArchiveDatas = model.ClearArchiveDatas;
                cst.ClearBillDatas = model.ClearBillDatas;

                cst.EnableBusinessTime = model.EnableBusinessTime;
                cst.BusinessStart = model.BusinessStart;
                cst.BusinessEnd = model.BusinessEnd;
                cst.EnableBusinessVisitLine = model.EnableBusinessVisitLine;


                Expression<Func<User, User>> exp = c => new User() { Id = c.Id, UserRealName = c.UserRealName };

                cst.SalesmanManagements = _userService.GetAllUsers(_storeContext.CurrentStore.Id, "Salesmans")?.Select(s =>
                {
                    var _OnStoreStopSeconds = form["OnStoreStopSeconds_" + s.Key];
                    var _Start = form["Start_" + s.Key];
                    var _End = form["End_" + s.Key];
                    var _FrequencyTimer = form["FrequencyTimer_" + s.Key];
                    var _SalesmanDeliveryDistance = form["SalesmanDeliveryDistance_" + s.Key];
                    var _DoorheadPhotoNum = form["DoorheadPhotoNum_" + s.Key];
                    var _DisplayPhotoNum = form["DisplayPhotoNum_" + s.Key];
                    var _BusinessStart = form["BusinessStart_" + s.Key];
                    var _BusinessEnd = form["BusinessEnd_" + s.Key];

                    var _et = form["EnableSalesmanTrack_" + s.Key];
                    var _ebt = form["EnableBusinessTime_" + s.Key];
                    var _ssc = form["SalesmanOnlySeeHisCustomer_" + s.Key];
                    var _svs = form["SalesmanVisitStoreBefore_" + s.Key];
                    var _svp = form["SalesmanVisitMustPhotographed_" + s.Key];
                    var _ev = form["EnableBusinessVisitLine_" + s.Key];

                    return new Core.Domain.Configuration.SalesmanManagement
                    {
                        UserId = s.Key,
                        UserName = s.Value,

                        OnStoreStopSeconds = string.IsNullOrEmpty(_OnStoreStopSeconds) ? 10 : int.Parse(_OnStoreStopSeconds.ToString()),
                        Start = string.IsNullOrEmpty(_Start) ? "7:00" : _Start.ToString(),
                        End = string.IsNullOrEmpty(_End) ? "19:00" : _End.ToString(),
                        FrequencyTimer = string.IsNullOrEmpty(_FrequencyTimer) ? 10 : int.Parse(_FrequencyTimer.ToString()),
                        SalesmanDeliveryDistance = string.IsNullOrEmpty(_SalesmanDeliveryDistance) ? 50 : int.Parse(_SalesmanDeliveryDistance.ToString()),
                        DoorheadPhotoNum = string.IsNullOrEmpty(_DoorheadPhotoNum) ? 1 : int.Parse(_DoorheadPhotoNum.ToString()),
                        DisplayPhotoNum = string.IsNullOrEmpty(_DisplayPhotoNum) ? 4 : int.Parse(_DisplayPhotoNum.ToString()),
                        BusinessStart = string.IsNullOrEmpty(_BusinessStart) ? "7:00" : _BusinessStart.ToString(),
                        BusinessEnd = string.IsNullOrEmpty(_BusinessEnd) ? "19:00" : _BusinessEnd.ToString(),

                        EnableSalesmanTrack = !string.IsNullOrEmpty(_et) && _et.Equals("on"),
                        SalesmanOnlySeeHisCustomer = !string.IsNullOrEmpty(_ssc) && _ssc.Equals("on"),
                        SalesmanVisitStoreBefore = !string.IsNullOrEmpty(_svs) && _svs.Equals("on"),
                        SalesmanVisitMustPhotographed = !string.IsNullOrEmpty(_svp) && _svp.Equals("on"),
                        EnableBusinessTime = !string.IsNullOrEmpty(_ebt) && _ebt.Equals("on"),
                        EnableBusinessVisitLine = !string.IsNullOrEmpty(_ev) && _ev.Equals("on")
                    };

                })?.ToList();


                _settingService.SaveSetting(cst, x => x.SalesmanManagements, storeId, false);


                _settingService.SaveSetting(cst, x => x.OpenBillMakeDate, storeId, false);
                _settingService.SaveSetting(cst, x => x.MulProductPriceUnit, storeId, false);
                _settingService.SaveSetting(cst, x => x.AllowCreateMulSameBarcode, storeId, false);
                _settingService.SaveSetting(cst, x => x.DefaultPricePlan, storeId, false);
                _settingService.SaveSetting(cst, x => x.DefaultPolicyPrice, storeId, false); //默认政策价
                _settingService.SaveSetting(cst, x => x.DefaultPurchasePrice, storeId, false);
                _settingService.SaveSetting(cst, x => x.VariablePriceCommodity, storeId, false);
                _settingService.SaveSetting(cst, x => x.AccuracyRounding, storeId, false);
                _settingService.SaveSetting(cst, x => x.MakeBillDisplayBarCode, storeId, false);
                _settingService.SaveSetting(cst, x => x.AllowSelectionDateRange, storeId, false);
                _settingService.SaveSetting(cst, x => x.DockingTicketPassSystem, storeId, false);
                _settingService.SaveSetting(cst, x => x.AllowReturnInSalesAndOrders, storeId, false);
                _settingService.SaveSetting(cst, x => x.SubmitExchangeBillAutoAudits, storeId, false);
                _settingService.SaveSetting(cst, x => x.AppMaybeDeliveryPersonnel, storeId, false);
                _settingService.SaveSetting(cst, x => x.AppSubmitOrderAutoAudits, storeId, false);
                _settingService.SaveSetting(cst, x => x.AppSubmitTransferAutoAudits, storeId, false);
                _settingService.SaveSetting(cst, x => x.AppSubmitExpenseAutoAudits, storeId, false);
                _settingService.SaveSetting(cst, x => x.AppSubmitBillReturnAutoAudits, storeId, false);
                _settingService.SaveSetting(cst, x => x.AppAllowWriteBack, storeId, false);
                _settingService.SaveSetting(cst, x => x.AllowAdvancePaymentsNegative, storeId, false);
                _settingService.SaveSetting(cst, x => x.ShowOnlyPrepaidAccountsWithPrepaidReceipts, storeId, false);
                _settingService.SaveSetting(cst, x => x.TasteByTasteAccountingOnlyPrintMainProduct, storeId, false);
                _settingService.SaveSetting(cst, x => x.APPOnlyShowHasStockProduct, storeId, false);
                _settingService.SaveSetting(cst, x => x.APPShowOrderStock, storeId, false);
                _settingService.SaveSetting(cst, x => x.SalesmanDeliveryDistance, storeId, false);
                _settingService.SaveSetting(cst, x => x.OnStoreStopSeconds, storeId, false);
                _settingService.SaveSetting(cst, x => x.EnableSalesmanTrack, storeId, false);

                _settingService.SaveSetting(cst, x => x.DisplayPhotoNum, storeId, false);
                _settingService.SaveSetting(cst, x => x.DoorheadPhotoNum, storeId, false);

                _settingService.SaveSetting(cst, x => x.Start, storeId, false);
                _settingService.SaveSetting(cst, x => x.End, storeId, false);
                _settingService.SaveSetting(cst, x => x.FrequencyTimer, storeId, false);
                _settingService.SaveSetting(cst, x => x.SalesmanOnlySeeHisCustomer, storeId, false);
                _settingService.SaveSetting(cst, x => x.SalesmanVisitStoreBefore, storeId, false);
                _settingService.SaveSetting(cst, x => x.SalesmanVisitMustPhotographed, storeId, false);
                _settingService.SaveSetting(cst, x => x.ReferenceCostPrice, storeId, false);
                _settingService.SaveSetting(cst, x => x.AveragePurchasePriceCalcNumber, storeId, false);
                _settingService.SaveSetting(cst, x => x.AllowNegativeInventoryMonthlyClosure, storeId, false);
                _settingService.SaveSetting(cst, x => x.EnableTaxRate, storeId, false);
                _settingService.SaveSetting(cst, x => x.TaxRate, storeId, false);
                _settingService.SaveSetting(cst, x => x.PhotographedWater, storeId, false);
                _settingService.SaveSetting(cst, x => x.ClearArchiveDatas, storeId, false);
                _settingService.SaveSetting(cst, x => x.ClearBillDatas, storeId, false);
                _settingService.SaveSetting(cst, x => x.AutoApproveConsumerPaidBill, storeId, false);

                _settingService.SaveSetting(cst, x => x.EnableBusinessTime, storeId, false);
                _settingService.SaveSetting(cst, x => x.BusinessStart, storeId, false);
                _settingService.SaveSetting(cst, x => x.BusinessEnd, storeId, false);
                _settingService.SaveSetting(cst, x => x.EnableBusinessVisitLine, storeId, false);

                //

                //清除缓存
                _settingService.ClearCache(storeId);

                //日志
                _userActivityService.InsertActivity("cst", "编辑配置", curUser);
                //_notificationService.SuccessNotification("配置已经更新");
                return RedirectToAction("cst");
            }
            catch (Exception ex)
            {
                return RedirectToAction("cst");
            }
        }


        [HttpGet]
        [AuthCode((int)AccessGranularityEnum.CompanySettingUpdate)]
        public async Task<JsonResult> SendVerificationCode()
        {
            return await Task.Run(() =>
            {
                try
                {
                    var code = CommonHelper.GenerateRandomInteger(1000, 5000);
                    string key = $"{_storeContext.CurrentStore.Id}_{code}_ClearVerificationCode";
                    var cacheKey = new CacheKey(key)
                    {
                        CacheTime = 5
                    };
                    _cacheManager.Set(cacheKey, code);

                    string msg = "";
                    msg += "<div dir=\"auto\">";
                    msg += "<span style=\"font-family:sans-serif\"><b><i><u>DCMS</u></i></b>，为你推送</span>";
                    msg += "<span style=\"font-family:sans-serif\">DCMS经销商管理系统数据清理验证，请查阅：</span><div style=\"font-family:sans-serif\">----------------------------------------------------------------------------------------------------------------------</div>";
                    msg += "<div style=\"font-family:sans-serif\">";
                    msg += "<br></div><div>";

                    msg += "<font face=\"sans-serif\">";

                    msg += $"验证码：{code}";

                    msg += "<br></font></div><br>";

                    msg += "<div style=\"font-family:sans-serif\"><br></div><div style=\"font-family:sans-serif\"><br></div>";
                    msg += $"<div style=\"font-family:sans-serif\">{DateTime.Now}</div>";
                    msg += "<div style=\"font-family:sans-serif\">----------------------------------------------------------------------------------------------------------------------</div>";
                    msg += "<div style=\"font-family:sans-serif\">";
                    msg += "<b><u><i>该邮件由DCMS.Pusher主动推送，请勿回复！</i></u></b>";
                    msg += "</div></div>";

                    CommonHelper.SendMail("DCMS数据清理验证", msg, false);

                    return Json(new { Success = true, Message = "成功" });
                }
                catch (Exception ex)
                {
                    return Json(new { Success = false, ex.Message });
                }
            });
        }


        [HttpPost]
        [AuthCode((int)AccessGranularityEnum.CompanySettingUpdate)]
        public async Task<JsonResult> ClearData(string code)
        {
            return await Task.Run(() =>
            {
                try
                {
                    string key = $"{_storeContext.CurrentStore.Id}_{code}_ClearVerificationCode";
                    var cacheKey = new CacheKey(key, "");
                    var ocode = _cacheManager.Get<int>(cacheKey, null);
                    if (string.IsNullOrEmpty(code) || string.IsNullOrEmpty(ocode.ToString()))
                        return Json(new { Success = false, Message = "清除失败" });
                    else
                    {
                        if (ocode.ToString().Equals(code.ToString()))
                        {
                            _settingService.ClearData(_storeContext.CurrentStore.Id);
                            _cacheManager.Remove(cacheKey);
                            return Json(new { Success = true, Message = "清除成功" });
                        }
                        else
                            return Json(new { Success = false, Message = "清除失败，验证码已失效" });
                    }
                }
                catch (Exception ex)
                {
                    return Json(new { Success = false, ex.Message });
                }
            });
        }


        private string DateDiff(DateTime start, DateTime end)
        {
            string dateDiff = null;
            TimeSpan ts1 = new TimeSpan(start.Ticks);
            TimeSpan ts2 = new TimeSpan(end.Ticks);
            TimeSpan ts = ts1.Subtract(ts2).Duration();
            dateDiff = ts.Seconds.ToString();
            return dateDiff;
        }
        #endregion

        #region 价格体系

        [AuthCode((int)AccessGranularityEnum.TierPricesSettingView)]
        public IActionResult PricingStructure()
        {
            var model = new PricingStructureListModel();

            var chennels = _channelService.GetChannels("", curStore?.Id ?? 0, 0, 30);
            var plans = _productTierPricePlanService.GetAllPricePlan(curStore?.Id ?? 0).Select(p => p);

            model.ChannelDatas = JsonConvert.SerializeObject(chennels);
            model.LevelDatas = JsonConvert.SerializeObject(new List<object>() { new { Id = 0, Name = "批发" }, new { Id = 1, Name = "商超" }, new { Id = 2, Name = "餐饮" } });
            model.TierPricePlanDatas = JsonConvert.SerializeObject(plans);

            model.Items1 = _pricingStructureService.GetAllPricingStructures(curStore?.Id ?? 0, 0, "", 0, 30).Select(p => p.ToModel<PricingStructureModel>()).ToList();
            model.Items2 = _pricingStructureService.GetAllPricingStructures(curStore?.Id ?? 0, 1, "", 0, 30).Select(p => p.ToModel<PricingStructureModel>()).ToList();

            return View(model);
        }

        [HttpPost]
        [AuthCode((int)AccessGranularityEnum.TierPricesSettingUpdate)]
        public async Task<JsonResult> PricingStructure(List<PricingStructureModel> Datas)
        {
            try
            {
                if (Datas != null)
                {
                    List<PricingStructure> pricingStructures = Datas.Select(d =>
                    {
                        return d.ToEntity<PricingStructure>();
                    }).ToList();

                    //Redis事务锁(防止重复保存)
                    string lockKey = string.Format(DCMSCachingDefaults.RedisDataReSubmitKey, Request.GetUrl(), curStore.Id, curUser.Id, CommonHelper.MD5(JsonConvert.SerializeObject(Datas)));
                    var result = await _locker.PerformActionWithLockAsync(lockKey,
                          TimeSpan.FromSeconds(30),
                          TimeSpan.FromSeconds(10),
                          TimeSpan.FromSeconds(1),
                          () => _pricingStructureService.CreateOrUpdate(curStore.Id, curUser.Id, pricingStructures));
                    return Json(result);

                }
            }
            catch (Exception ex)
            {
                _userActivityService.InsertActivity("CreateOrUpdate", "创建/更新失败", curUser.Id);
                _notificationService.ErrorNotification("创建/更新失败");
                return Error(ex.Message);
            }

            _userActivityService.InsertActivity("CreateOrUpdate", "创建/更新成功", curUser.Id);
            _notificationService.SuccessNotification("创建/更新成功");
            return Successful("创建/更新成功");
        }

        /// <summary>
        /// 获取价格体系项目
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public async Task<JsonResult> AsyncPricingStructures(int? type, string name = "")
        {

            return await Task.Run(() =>
            {
                var plans = _productTierPricePlanService.GetAllPricePlan(curStore?.Id ?? 0).Select(p => p);

                var items = _pricingStructureService.GetAllPricingStructures(curStore?.Id ?? 0, type, name, 0, 30).Select(o =>
                  {
                      var m = o.ToModel<PricingStructureModel>();

                      m.ChannelName = _channelService.GetChannelName(curStore.Id, m.ChannelId);

                      if (m.EndPointLevel == 0)
                      {
                          m.EndPointLevelName = "批发";
                      }
                      else if (m.EndPointLevel == 1)
                      {
                          m.EndPointLevelName = "商超";
                      }
                      else if (m.EndPointLevel == 2)
                      {
                          m.EndPointLevelName = "餐饮";
                      }

                      m.PreferredPriceName = plans.Where(p => (p.PricesPlanId.ToString() + "_" + p.PriceTypeId.ToString()) == m.PreferredPrice).Select(p => p.Name).FirstOrDefault();
                      m.SecondaryPriceName = plans.Where(p => (p.PricesPlanId.ToString() + "_" + p.PriceTypeId.ToString()) == m.SecondaryPrice).Select(p => p.Name).FirstOrDefault();
                      m.FinalPriceName = plans.Where(p => (p.PricesPlanId.ToString() + "_" + p.PriceTypeId.ToString()) == m.FinalPrice).Select(p => p.Name).FirstOrDefault();

                      return m;

                  }).ToList();

                return Json(new
                {
                    Success = true,
                    total = items.Count,
                    rows = items
                });
            });
        }

        #endregion

        #region 打印模板

        [HttpGet]
        [AuthCode((int)AccessGranularityEnum.PrintTemplateSettingView)]
        public IActionResult PrintTemplateList(int pageNumber = 0)
        {

            if (pageNumber > 0)
            {
                pageNumber -= 1;
            }

            var model = new PrintTemplateListModel();

            var lists = _printTemplateService.GetAllPrintTemplates(curStore?.Id ?? 0, null, pageNumber, 30);
            model.PagingFilteringContext.LoadPagedList(lists);
            model.Lists = lists.Select(s =>
            {
                var m = s.ToModel<PrintTemplateModel>();
                m.TemplateTypeName = CommonHelper.GetEnumDescription((TemplateType)Enum.Parse(typeof(TemplateType), s.TemplateType.ToString()));
                m.BillTypeName = CommonHelper.GetEnumDescription((BillTypeEnum)Enum.Parse(typeof(BillTypeEnum), s.BillType.ToString()));
                return m;

            }).ToList();

            return View(model);
        }

        [HttpGet]
        [AuthCode((int)AccessGranularityEnum.PrintTemplateSettingUpdate)]
        public IActionResult PrintTemplateCreate()
        {

            var model = new PrintTemplateModel
            {
                TemplateTypes = new SelectList(from a in Enum.GetValues(typeof(TemplateType)).Cast<TemplateType>()
                                               select new SelectListItem
                                               {
                                                   Text = CommonHelper.GetEnumDescription(a),
                                                   Value = ((int)a).ToString()
                                               }, "Value", "Text"),

                BillTypes = new SelectList(from a in Enum.GetValues(typeof(BillTypeEnum)).Cast<BillTypeEnum>()
                                           select new SelectListItem
                                           {
                                               Text = CommonHelper.GetEnumDescription(a),
                                               Value = ((int)a).ToString()
                                           }, "Value", "Text"),

                PaperTypes = new SelectList(from a in Enum.GetValues(typeof(PaperTypeEnum)).Cast<PaperTypeEnum>()
                                               select new SelectListItem
                                               {
                                                   Text = CommonHelper.GetEnumDescription(a),
                                                   Value = ((int)a).ToString()
                                               }, "Value", "Text"),
            };

            return View(model);
        }

        //[ValidateInput(false)]
        [HttpPost, ParameterBasedOnFormNameAttribute("save-continue", "continueEditing")]
        [AuthCode((int)AccessGranularityEnum.PrintTemplateSettingUpdate)]
        public IActionResult PrintTemplateCreate(PrintTemplateModel model, bool continueEditing)
        {

            if (string.IsNullOrEmpty(model.Title))
            {
                ModelState.AddModelError("", "标题不能为空");
            }

            if (string.IsNullOrEmpty(model.Content))
            {
                ModelState.AddModelError("", "内容不能为空");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var tpmplate = model.ToEntity<PrintTemplate>();
                    tpmplate.StoreId = curStore?.Id ?? 0;

                    _printTemplateService.InsertPrintTemplate(tpmplate);

                    //activity log
                    _userActivityService.InsertActivity("InsertPrintTemplate", "添加成功", curUser, tpmplate.Title);
                    _notificationService.SuccessNotification("添加成功");
                    return continueEditing ? RedirectToAction("PrintTemplateEdit", new { id = tpmplate.Id }) : RedirectToAction("PrintTemplateList");
                }
                catch (Exception ex)
                {
                    _userActivityService.InsertActivity("InsertProduct", "添加失败", curUser, model.Title);
                    _notificationService.ErrorNotification("添加失败:" + ex.Message);
                    return RedirectToAction("PrintTemplateList");
                }
            }

            model.TemplateTypes = new SelectList(from a in Enum.GetValues(typeof(TemplateType)).Cast<TemplateType>()
                                                 select new SelectListItem
                                                 {
                                                     Text = CommonHelper.GetEnumDescription(a),
                                                     Value = ((int)a).ToString()
                                                 }, "Value", "Text");


            model.BillTypes = new SelectList(from a in Enum.GetValues(typeof(BillTypeEnum)).Cast<BillTypeEnum>()
                                             select new SelectListItem
                                             {
                                                 Text = CommonHelper.GetEnumDescription(a),
                                                 Value = ((int)a).ToString()
                                             }, "Value", "Text");

            return View(model);
        }

        [HttpGet]
        [AuthCode((int)AccessGranularityEnum.PrintTemplateSettingUpdate)]
        public IActionResult PrintTemplateEdit(int? id)
        {
            var model = new PrintTemplateModel();

            var tpmplate = _printTemplateService.GetPrintTemplateById(curStore.Id, id ?? 0);
            if (tpmplate != null)
            {
                model = tpmplate.ToModel<PrintTemplateModel>();
            }

            model.TemplateTypes = new SelectList(from a in Enum.GetValues(typeof(TemplateType)).Cast<TemplateType>()
                                                 select new SelectListItem
                                                 {
                                                     Text = CommonHelper.GetEnumDescription(a),
                                                     Value = ((int)a).ToString()
                                                 }, "Value", "Text");


            model.BillTypes = new SelectList(from a in Enum.GetValues(typeof(BillTypeEnum)).Cast<BillTypeEnum>()
                                             select new SelectListItem
                                             {
                                                 Text = CommonHelper.GetEnumDescription(a),
                                                 Value = ((int)a).ToString()
                                             }, "Value", "Text");


            model.PaperTypes = new SelectList(from a in Enum.GetValues(typeof(PaperTypeEnum)).Cast<PaperTypeEnum>()
                                              select new SelectListItem
                                              {
                                                  Text = CommonHelper.GetEnumDescription(a),
                                                  Value = ((int)a).ToString()
                                              }, "Value", "Text");

            return View(model);
        }

        //[ValidateInput(false)]
        [HttpPost, ParameterBasedOnFormNameAttribute("save-continue", "continueEditing")]
        [AuthCode((int)AccessGranularityEnum.PrintTemplateSettingUpdate)]
        public IActionResult PrintTemplateEdit(PrintTemplateModel model, bool continueEditing)
        {

            if (string.IsNullOrEmpty(model.Title))
            {
                ModelState.AddModelError("", "标题不能为空");
            }

            if (string.IsNullOrEmpty(model.Content))
            {
                ModelState.AddModelError("", "内容不能为空");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var tpmplate = _printTemplateService.GetPrintTemplateById(curStore.Id, model.Id);
                    if (tpmplate != null)
                    {
                        tpmplate = model.ToEntity(tpmplate);
                    }
                    tpmplate.StoreId = curStore.Id;
                    tpmplate.Content = model.Content;
                    _printTemplateService.UpdatePrintTemplate(tpmplate);

                    //activity log
                    _userActivityService.InsertActivity("UpdatePrintTemplate", "修改成功", curUser, tpmplate.Title);
                    _notificationService.SuccessNotification("修改成功");
                    return continueEditing ? RedirectToAction("PrintTemplateEdit", new { id = tpmplate.Id }) : RedirectToAction("PrintTemplateList");
                }
                catch (Exception ex)
                {
                    _userActivityService.InsertActivity("UpdatePrintTemplate", "修改失败", curUser, model.Title);
                    _notificationService.ErrorNotification("修改失败:" + ex.Message);
                    return RedirectToAction("PrintTemplateList");
                }
            }


            model.TemplateTypes = new SelectList(from a in Enum.GetValues(typeof(TemplateType)).Cast<TemplateType>()
                                                 select new SelectListItem
                                                 {
                                                     Text = CommonHelper.GetEnumDescription(a),
                                                     Value = ((int)a).ToString()
                                                 }, "Value", "Text");


            model.BillTypes = new SelectList(from a in Enum.GetValues(typeof(BillTypeEnum)).Cast<BillTypeEnum>()
                                             select new SelectListItem
                                             {
                                                 Text = CommonHelper.GetEnumDescription(a),
                                                 Value = ((int)a).ToString()
                                             }, "Value", "Text");
            return View(model);
        }

        [HttpGet]
        [AuthCode((int)AccessGranularityEnum.PrintTemplateSettingUpdate)]
        public IActionResult PrintTemplateDelete(string ids)
        {

            if (!string.IsNullOrEmpty(ids))
            {
                int[] sids = ids.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(u => int.Parse(u)).ToArray();
                var tpmplates = _printTemplateService.GetPrintTemplatesByIds(sids);
                foreach (var tpmplate in tpmplates)
                {
                    if (tpmplate != null)
                    {
                        _printTemplateService.DeletePrintTemplate(tpmplate);
                    }
                }
                //活动日志
                _userActivityService.InsertActivity("DeletePrintTemplate", "删除模板", curUser, ids);
                _notificationService.SuccessNotification("删除成功");
            }

            return RedirectToAction("PrintTemplateList");
        }

        [HttpGet]
        public IActionResult PrintTemplateInit()
        {
            _installationService.InstallPrintTemplate(curStore?.Id ?? 0);
            return RedirectToAction("PrintTemplateList");
        }

        #endregion

        #region 备注设置

        [HttpGet]
        [AuthCode((int)AccessGranularityEnum.NoteSettingView)]
        public IActionResult RemarkConfigList(int pageNumber = 0)
        {

            if (pageNumber > 0)
            {
                pageNumber -= 1;
            }

            var model = new RemarkConfigListModel();

            var lists = _remarkConfigService.GetAllRemarkConfigs(curStore?.Id ?? 0, pageNumber, 30);
            model.PagingFilteringContext.LoadPagedList(lists);
            model.Lists = lists.Select(s =>
            {
                var m = s.ToModel<RemarkConfigModel>();
                return m;

            }).ToList();

            return View(model);
        }

        [HttpGet]
        [AuthCode((int)AccessGranularityEnum.NoteSettingUpdate)]
        public IActionResult RemarkConfigCreate()
        {
            var model = new RemarkConfigModel();

            return View(model);
        }

        //[ValidateInput(false)]
        [HttpPost, ParameterBasedOnFormNameAttribute("save-continue", "continueEditing")]
        [AuthCode((int)AccessGranularityEnum.NoteSettingUpdate)]
        public IActionResult RemarkConfigCreate(RemarkConfigModel model, bool continueEditing)
        {
            if (string.IsNullOrEmpty(model.Name))
            {
                ModelState.AddModelError("", "名称不能为空");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var tpmplate = model.ToEntity<RemarkConfig>();
                    tpmplate.StoreId = curStore?.Id ?? 0;

                    _remarkConfigService.InsertRemarkConfig(tpmplate);

                    //activity log
                    _userActivityService.InsertActivity("InsertRemarkConfig", "添加成功", curUser, tpmplate.Name);
                    _notificationService.SuccessNotification("添加成功");
                    return continueEditing ? RedirectToAction("RemarkConfigEdit", new { id = tpmplate.Id }) : RedirectToAction("RemarkConfigList");
                }
                catch (Exception ex)
                {
                    _userActivityService.InsertActivity("InsertProduct", "添加失败", curUser, model.Name);
                    _notificationService.ErrorNotification("添加失败:" + ex.Message);
                    return RedirectToAction("RemarkConfigList");
                }
            }

            return View(model);
        }

        [HttpGet]
        [AuthCode((int)AccessGranularityEnum.NoteSettingUpdate)]
        public IActionResult RemarkConfigEdit(int? id)
        {
            var model = new RemarkConfigModel();

            var tpmplate = _remarkConfigService.GetRemarkConfigById(curStore.Id, id ?? 0);
            if (tpmplate != null)
            {
                model = tpmplate.ToModel<RemarkConfigModel>();
            }

            return View(model);
        }

        //[ValidateInput(false)]
        [HttpPost, ParameterBasedOnFormNameAttribute("save-continue", "continueEditing")]
        [AuthCode((int)AccessGranularityEnum.NoteSettingUpdate)]
        public IActionResult RemarkConfigEdit(RemarkConfigModel model, bool continueEditing)
        {
            if (string.IsNullOrEmpty(model.Name))
            {
                ModelState.AddModelError("", "名称不能为空");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var tpmplate = _remarkConfigService.GetRemarkConfigById(curStore.Id, model.Id);
                    if (tpmplate != null)
                    {
                        tpmplate = model.ToEntity(tpmplate);
                    }

                    _remarkConfigService.UpdateRemarkConfig(tpmplate);

                    //activity log
                    _userActivityService.InsertActivity("UpdateRemarkConfig", "修改成功", curUser, tpmplate.Name);
                    _notificationService.SuccessNotification("修改成功");
                    return continueEditing ? RedirectToAction("RemarkConfigEdit", new { id = tpmplate.Id }) : RedirectToAction("RemarkConfigList");
                }
                catch (Exception ex)
                {
                    _userActivityService.InsertActivity("UpdateRemarkConfig", "修改失败", curUser, model.Name);
                    _notificationService.ErrorNotification("修改失败:" + ex.Message);
                    return RedirectToAction("RemarkConfigList");
                }
            }

            return View(model);
        }

        [HttpGet]
        [AuthCode((int)AccessGranularityEnum.NoteSettingUpdate)]
        public IActionResult RemarkConfigDelete(string ids)
        {

            if (!string.IsNullOrEmpty(ids))
            {
                int[] sids = ids.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(u => int.Parse(u)).ToArray();
                var tpmplates = _remarkConfigService.GetRemarkConfigsByIds(sids);
                foreach (var tpmplate in tpmplates)
                {
                    if (tpmplate != null)
                    {
                        _remarkConfigService.DeleteRemarkConfig(tpmplate);
                    }
                }
                //活动日志
                _userActivityService.InsertActivity("DeleteRemarkConfig", "删除模板", curUser, ids);
                _notificationService.SuccessNotification("删除成功");
            }

            return RedirectToAction("RemarkConfigList");
        }

        /// <summary>
        /// 开单备注选择
        /// </summary>
        /// <param name="storeId"></param>
        /// <returns></returns>
        public async Task<JsonResult> GetRemarkConfigDic(int storeId = 0)
        {
            return await Task.Run(() =>
            {

                try
                {
                    Dictionary<int, string> dic = new Dictionary<int, string>();
                    var remarkConfigs = _remarkConfigService.BindRemarkConfigList(curStore?.Id ?? 0);
                    if (remarkConfigs != null && remarkConfigs.Count > 0)
                    {
                        remarkConfigs.ToList().ForEach(rm =>
                        {
                            dic.Add(rm.Id, rm.Name);
                        });
                    }
                    return Json(new { Success = true, Data = dic });
                }
                catch (Exception)
                {
                    return Json(new { Success = false, Data = new Dictionary<int, string>() });
                }

            });
        }

        [HttpGet]
        public IActionResult RemarkConfigInit()
        {

            string contentRootPath = _hostingEnvironment.ContentRootPath;

            //var data = _remarkConfigService.GetAllRemarkConfigs(2);
            //contentRootPath = contentRootPath + @"\App_Data\TempUploads\RemarkConfig.json";
            //FileInfo myFile = new FileInfo(contentRootPath);
            //StreamWriter sW5 = myFile.CreateText();
            //sW5.Write(JsonConvert.SerializeObject(data));
            ////StreamReader sW5 = myFile.OpenText();
            ////var result = sW5.ReadToEnd();
            //sW5.Close();

            _installationService.InstallRemarkConfigs(curStore.Id, contentRootPath);

            return RedirectToAction("RemarkConfigList");
        }
        #endregion

    }
}
