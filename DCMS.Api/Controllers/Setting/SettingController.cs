using DCMS.Api.Infrastructure.Mapper.Extensions;
using DCMS.Api.Models;
using DCMS.Core;
using DCMS.Core.Domain.Configuration;
using DCMS.Core.Domain.Products;
using DCMS.Services.Configuration;
using DCMS.Services.Products;
using DCMS.Services.Settings;
using DCMS.Services.Terminals;
using DCMS.ViewModel.Models.Configuration;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Swashbuckle.AspNetCore.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Task = System.Threading.Tasks.Task;
using Microsoft.AspNetCore.Authorization;
using DCMS.Services.Users;
using System.Linq.Expressions;
using DCMS.Core.Domain.Users;


namespace DCMS.Api.Controllers.Setting
{
    /// <summary>
    /// Setting
    /// </summary>
   [Authorize]
    [Route("api/v{version:apiVersion}/dcms/config")]
    public class SettingController : BaseAPIController
    {
        private readonly ISettingService _settingService;
        private readonly IStoreContext _storeContext;
        private readonly IProductService _productService;
        private readonly ISpecificationAttributeService _specificationAttributeService;
        private readonly IProductTierPricePlanService _productTierPricePlanService;
        private readonly IStockEarlyWarningService _stockEarlyWarningService;
        private readonly IAccountingService _accountingService;
        private readonly IChannelService _channelService;
        private readonly IPricingStructureService _pricingStructureService;
        private readonly IRemarkConfigService _remarkConfigService;
        private readonly IPrintTemplateService _printTemplateService;
        private readonly IUserService _userService;



        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="storeContext"></param>
        /// <param name="settingService"></param>
        /// <param name="productService"></param>
        /// <param name="specificationAttributeService"></param>
        /// <param name="productTierPricePlanService"></param>
        /// <param name="stockEarlyWarningService"></param>
        /// <param name="accountingService"></param>
        /// <param name="channelService"></param>
        /// <param name="pricingStructureService"></param>
        /// <param name="remarkConfigService"></param>
        /// <param name="printTemplateService"></param>
        public SettingController(
            IStoreContext storeContext,
            ISettingService settingService,
            IProductService productService,
            ISpecificationAttributeService specificationAttributeService,
            IProductTierPricePlanService productTierPricePlanService,
            IStockEarlyWarningService stockEarlyWarningService,
            IAccountingService accountingService,
            IChannelService channelService,
            IPricingStructureService pricingStructureService,
            IRemarkConfigService remarkConfigService,
            IPrintTemplateService printTemplateService,
            IUserService userService,
            ILogger<BaseAPIController> logger) : base(logger)
        {
            _storeContext = storeContext;
            _settingService = settingService;
            _productService = productService;
            _specificationAttributeService = specificationAttributeService;
            _productTierPricePlanService = productTierPricePlanService;
            _stockEarlyWarningService = stockEarlyWarningService;
            _accountingService = accountingService;
            _channelService = channelService;
            _pricingStructureService = pricingStructureService;
            _remarkConfigService = remarkConfigService;
            _printTemplateService = printTemplateService;
            _userService = userService;
        }

        #region 公司设置

        /// <summary>
        ///  获取公司设置
        /// </summary>
        /// <param name="store"></param>
        /// <returns></returns>
        [HttpGet("setting/getCompanySetting/{store}")]
        [SwaggerOperation("getCompanySetting")]
        //[ValidateActionParameters]
        //[AuthBaseFilter]
        public async Task<APIResult<CompanySettingModel>> GetCompanySetting(int? store, int? userId)
        {
            if (!store.HasValue || store.Value == 0)
                return this.Error3<CompanySettingModel>(Resources.ParameterError);

            return await Task.Run(() =>
            {
                var result = new APIResult<CompanySettingModel>();

                try
                {
                    var companySetting = _settingService.LoadSetting<CompanySetting>(store ?? 0);

                    var model = companySetting.ToSettingsModel<CompanySettingModel>();

                    model.OpenBillMakeDates = new SelectList(from a in Enum.GetValues(typeof(OpenBillMakeDate)).Cast<OpenBillMakeDate>()
                                                             select new SelectListItem
                                                             {
                                                                 Text = CommonHelper.GetEnumDescription(a),
                                                                 Value = ((int)a).ToString()
                                                             }, "Value", "Text");
                    model.MulProductPriceUnits = new SelectList(from a in Enum.GetValues(typeof(MuitProductPriceUnit)).Cast<MuitProductPriceUnit>()
                                                                select new SelectListItem
                                                                {
                                                                    Text = CommonHelper.GetEnumDescription(a),
                                                                    Value = ((int)a).ToString()
                                                                }, "Value", "Text");
                    model.DefaultPurchasePrices = new SelectList(from a in Enum.GetValues(typeof(DefaultPurchasePrice)).Cast<DefaultPurchasePrice>()
                                                                 select new SelectListItem
                                                                 {
                                                                     Text = CommonHelper.GetEnumDescription(a),
                                                                     Value = ((int)a).ToString()
                                                                 }, "Value", "Text");
                    model.VariablePriceCommodities = new SelectList(from a in Enum.GetValues(typeof(VariablePriceCommodity)).Cast<VariablePriceCommodity>()
                                                                    select new SelectListItem
                                                                    {
                                                                        Text = CommonHelper.GetEnumDescription(a),
                                                                        Value = ((int)a).ToString()
                                                                    }, "Value", "Text");
                    model.AccuracyRoundings = new SelectList(from a in Enum.GetValues(typeof(AccuracyRounding)).Cast<AccuracyRounding>()
                                                             select new SelectListItem
                                                             {
                                                                 Text = CommonHelper.GetEnumDescription(a),
                                                                 Value = ((int)a).ToString()
                                                             }, "Value", "Text");

                    model.MakeBillDisplayBarCodes = new SelectList(from a in Enum.GetValues(typeof(MakeBillDisplayBarCode)).Cast<MakeBillDisplayBarCode>()
                                                                   select new SelectListItem
                                                                   {
                                                                       Text = CommonHelper.GetEnumDescription(a),
                                                                       Value = ((int)a).ToString()
                                                                   }, "Value", "Text");
                    model.ReferenceCostPrices = new SelectList(from a in Enum.GetValues(typeof(ReferenceCostPrice)).Cast<ReferenceCostPrice>()
                                                               select new SelectListItem
                                                               {
                                                                   Text = CommonHelper.GetEnumDescription(a),
                                                                   Value = ((int)a).ToString()
                                                               }, "Value", "Text");


                    //V3
                    if (userId.HasValue && userId > 0)
                    {
                        var sms = companySetting.SalesmanManagements;
                        var curt = sms?.Where(sm => sm.UserId == userId).FirstOrDefault();
                        model.OnStoreStopSeconds = curt?.OnStoreStopSeconds ?? 10;
                        model.EnableSalesmanTrack = curt?.EnableSalesmanTrack ?? false;
                        model.Start = curt?.Start ?? "07:00";
                        model.End = curt?.End ?? "19:00";
                        model.FrequencyTimer = curt?.FrequencyTimer ?? 0;
                        model.SalesmanOnlySeeHisCustomer = curt?.SalesmanOnlySeeHisCustomer ?? false;
                        model.SalesmanVisitStoreBefore = curt?.SalesmanVisitStoreBefore ?? false;
                        model.SalesmanVisitMustPhotographed = curt?.SalesmanVisitMustPhotographed ?? false;
                        model.SalesmanDeliveryDistance = curt?.SalesmanDeliveryDistance ?? 50;
                        model.DoorheadPhotoNum = curt?.DoorheadPhotoNum ?? 1;
                        model.DisplayPhotoNum = curt?.DisplayPhotoNum ?? 4;
                        model.EnableBusinessTime = curt?.EnableBusinessTime ?? false;
                        model.BusinessStart = curt?.BusinessStart ?? "07:00";
                        model.BusinessEnd = curt?.BusinessEnd ?? "19:00";
                        model.EnableBusinessVisitLine = curt?.EnableBusinessVisitLine ?? false;
                    }

                    return this.Successful3("", model);
                }
                catch (Exception ex)
                {
                    return this.Error3<CompanySettingModel>(ex.Message);
                }
            });
        }
        #endregion

        #region 获取库存预警设置

        /// <summary>
        /// 获取库存预警设置
        /// </summary>
        /// <param name="store"></param>
        /// <param name="name"></param>
        /// <param name="wareHouseId"></param>
        /// <param name="pageNumber"></param>
        /// <returns></returns>
        [HttpGet("setting/getStockEarlyWarningSetting/{store}")]
        [SwaggerOperation("getStockEarlyWarningSetting")]
        //[ValidateActionParameters]
        //[AuthBaseFilter]
        public async Task<APIResult<IList<StockEarlyWarningModel>>> GetStockEarlyWarningSetting(int? store, string name = "", int? wareHouseId = 0, int pageNumber = 0)
        {
            if (!store.HasValue || store.Value == 0)
                return this.Error2<StockEarlyWarningModel>(Resources.ParameterError);

            return await Task.Run(() =>
            {
                try
                {
                    var lists = _stockEarlyWarningService.GetAllStockEarlyWarnings(store != null ? store : 0, name, pageNumber, 30);

                    var allProducts = _productService.GetProductsByIds(store ?? 0, lists.Select(pr => pr.ProductId).Distinct().ToArray());
                    var allOptions = _specificationAttributeService.GetSpecificationAttributeOptionByIds(store, allProducts.GetProductBigStrokeSmallUnitIds());
                    var allProductTierPrices = _productService.GetProductTierPriceByProductIds(store, allProducts.Select(p => p.Id).Distinct().ToArray());
                    var allProductPrices = _productService.GetProductPricesByProductIds(store, allProducts.Select(ap => ap.Id).Distinct().ToArray());

                    var results = lists.Select(s =>
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

                    return this.Successful2("", results);
                }
                catch (Exception ex)
                {
                    return this.Error2<StockEarlyWarningModel>(ex.Message);
                }

            });
        }

        #endregion

        #region APP打印设置

        /// <summary>
        ///  APP打印设置
        /// </summary>
        /// <param name="store"></param>
        /// <returns></returns>
        [HttpGet("setting/getAPPPrintSetting/{store}")]
        [SwaggerOperation("getAPPPrintSetting")]
        //[ValidateActionParameters]
        //[AuthBaseFilter]
        public async Task<APIResult<APPPrintSettingModel>> GetAPPPrintSetting(int? store)
        {
            if (!store.HasValue || store.Value == 0)
                return this.Error3<APPPrintSettingModel>(Resources.ParameterError);

            return await Task.Run(() =>
            {
                try
                {
                    var appPrintSetting = _settingService.LoadSetting<APPPrintSetting>(store ?? 0);
                    var model = appPrintSetting.ToSettingsModel<APPPrintSettingModel>();

                    //打印模式
                    IEnumerable<PrintMode> attributes = Enum.GetValues(typeof(PrintMode)).Cast<PrintMode>();

                    var printModes = from a in attributes
                                     select new SelectListItem
                                     {
                                         Text = CommonHelper.GetEnumDescription(a),
                                         Value = ((int)a).ToString()
                                     };
                    model.PrintModes = new SelectList(printModes, "Value", "Text");

                    return this.Successful3("", model);
                }
                catch (Exception ex)
                {
                    return this.Error3<APPPrintSettingModel>(ex.Message);
                }
            });
        }
        #endregion

        #region 会计科目

        /// <summary>
        /// 获取科目类别
        /// </summary>
        /// <param name="store"></param>
        /// <returns></returns>
        [HttpGet("setting/getAccountingTypesSetting/{store}")]
        [SwaggerOperation("getAccountingTypesSetting")]
        //[ValidateActionParameters]
        //[AuthBaseFilter]
        public async Task<APIResult<IList<AccountingTypeModel>>> GetAccountingTypesSetting(int? store)
        {
            if (!store.HasValue || store.Value == 0)
                return this.Error2<AccountingTypeModel>(Resources.ParameterError);

            return await Task.Run(() =>
            {
                try
                {
                    var accountings = _accountingService.GetAccountingTypes();
                    var result = accountings.Select(a =>
                     {
                         return a.ToModel<AccountingTypeModel>();
                     }).ToList();

                    return this.Successful2("", result);
                }
                catch (Exception ex)
                {
                    return this.Error2<AccountingTypeModel>(ex.Message);
                }


            });
        }
        #endregion

        #region 价格体系设置

        /// <summary>
        /// 价格体系设置
        /// </summary>
        /// <param name="store"></param>
        /// <returns></returns>
        [HttpGet("setting/getPricingStructureSetting/{store}")]
        [SwaggerOperation("getPricingStructureSetting")]
        //[ValidateActionParameters]
        //[AuthBaseFilter]
        public async Task<APIResult<PricingStructureListModel>> GetPricingStructureSetting(int? store)
        {
            if (!store.HasValue || store.Value == 0)
                return this.Error3<PricingStructureListModel>(Resources.ParameterError);

            return await Task.Run(() =>
            {
                try
                {
                    var model = new PricingStructureListModel();

                    var chennels = _channelService.GetChannels("", store != null ? store : 0, 0, 30);
                    var plans = _productTierPricePlanService.GetAllPricePlan(store != null ? store : 0).Select(p => p);

                    model.ChannelDatas = JsonConvert.SerializeObject(chennels);
                    model.LevelDatas = JsonConvert.SerializeObject(new List<object>() { new { Id = 0, Name = "批发" }, new { Id = 1, Name = "商超" }, new { Id = 2, Name = "餐饮" } });
                    model.TierPricePlanDatas = JsonConvert.SerializeObject(plans);

                    model.Items1 = _pricingStructureService.GetAllPricingStructures(store != null ? store : 0, 0, "", 0, 30).Select(p => p.ToModel<PricingStructureModel>()).ToList();
                    model.Items2 = _pricingStructureService.GetAllPricingStructures(store != null ? store : 0, 1, "", 0, 30).Select(p => p.ToModel<PricingStructureModel>()).ToList();

                    return this.Successful3("", model);
                }
                catch (Exception ex)
                {
                    return this.Error3<PricingStructureListModel>(ex.Message);
                }
            });
        }
        #endregion

        #region 打印模板

        /// <summary>
        /// 打印模板设置
        /// </summary>
        /// <param name="store"></param>
        /// <returns></returns>
        [HttpGet("setting/getPrintTemplateSetting/{store}")]
        [SwaggerOperation("getPrintTemplateSetting")]
        //[ValidateActionParameters]
        //[AuthBaseFilter]
        public async Task<APIResult<IList<PrintTemplateModel>>> GetPrintTemplateSetting(int? store, int pageNumber = 0)
        {
            if (!store.HasValue || store.Value == 0)
                return this.Error2<PrintTemplateModel>(Resources.ParameterError);

            return await Task.Run(() =>
            {
                try
                {
                    var lists = _printTemplateService.GetAllPrintTemplates(store != null ? store : 0, null, pageNumber, 30);
                    var results = lists.Select(s =>
                    {
                        var m = s.ToModel<PrintTemplateModel>();
                        m.TemplateTypeName = CommonHelper.GetEnumDescription((TemplateType)Enum.Parse(typeof(TemplateType), s.TemplateType.ToString()));
                        m.BillTypeName = CommonHelper.GetEnumDescription((BillTypeEnum)Enum.Parse(typeof(BillTypeEnum), s.BillType.ToString()));
                        return m;

                    }).ToList();

                    return this.Successful2("", results);
                }
                catch (Exception ex)
                {
                    return this.Error2<PrintTemplateModel>(ex.Message);
                }
            });
        }
        #endregion

        #region 备注设置

        /// <summary>
        /// 备注配置
        /// </summary>
        /// <param name="store"></param>
        /// <returns></returns>
        [HttpGet("setting/getRemarkConfigListSetting/{store}")]
        [SwaggerOperation("getRemarkConfigListSetting")]
        //[ValidateActionParameters]
        //[AuthBaseFilter]
        public async Task<APIResult<dynamic>> GetRemarkConfigListSetting(int? store)
        {
            if (!store.HasValue || store.Value == 0)
                return this.Error(Resources.ParameterError);

            return await Task.Run(() =>
            {
                try
                {
                    Dictionary<int, string> dic = new Dictionary<int, string>();
                    var remarkConfigs = _remarkConfigService.BindRemarkConfigList(store);
                    if (remarkConfigs != null && remarkConfigs.Count > 0)
                    {
                        remarkConfigs.ToList().ForEach(rm =>
                        {
                            dic.Add(rm.Id, rm.Name);
                        });
                    }

                    return this.Successful("", dic);
                }
                catch (Exception ex)
                {
                    return this.Error(ex.Message);
                }
            });
        }
        #endregion

        #region 商品设置

        /// <summary>
        /// 商品设置
        /// </summary>
        /// <param name="store"></param>
        /// <returns></returns>
        [HttpGet("setting/getProductSetting/{store}")]
        [SwaggerOperation("getProductSetting")]
        //[ValidateActionParameters]
        //[AuthBaseFilter]
        public async Task<APIResult<ProductSettingModel>> GetProductSetting(int? store)
        {
            if (!store.HasValue || store.Value == 0)
                return this.Error3<ProductSettingModel>(Resources.ParameterError);

            return await Task.Run(() =>
            {
                var result = new APIResult<ProductSettingModel>();

                try
                {
                    var productSetting = _settingService.LoadSetting<ProductSetting>(store ?? 0);
                    var model = productSetting.ToSettingsModel<ProductSettingModel>();

                    var smalllists = new List<SelectListItem>();
                    var stroklists = new List<SelectListItem>();
                    var biglists = new List<SelectListItem>();

                    //规格属性
                    var specificationAttributes = _specificationAttributeService.GetSpecificationAttributesBtStore(store ?? 0).ToList();
                    specificationAttributes.ForEach(sp =>
                    {
                        smalllists.Add(new SelectListItem() { Text = sp.Name, Value = sp.Id.ToString() });
                        stroklists.Add(new SelectListItem() { Text = sp.Name, Value = sp.Id.ToString() });
                        biglists.Add(new SelectListItem() { Text = sp.Name, Value = sp.Id.ToString() });
                    });

                    model.SmallUnits = new SelectList(smalllists, "Value", "Text");
                    model.StrokeUnits = new SelectList(stroklists, "Value", "Text");
                    model.BigUnits = new SelectList(biglists, "Value", "Text");

                    return this.Successful3("", model);
                }
                catch (Exception ex)
                {
                    return this.Error3<ProductSettingModel>(ex.Message);
                }
            });
        }
        #endregion

        #region 财务设置

        /// <summary>
        /// 财务设置
        /// </summary>
        /// <param name="store"></param>
        /// <returns></returns>
        [HttpGet("setting/getFinancesSetting/{store}")]
        [SwaggerOperation("getFinancesSetting")]
        //[ValidateActionParameters]
        //[AuthBaseFilter]
        public async Task<APIResult<FinanceSettingModel>> GetFinancesSetting(int? store)
        {
            if (!store.HasValue || store.Value == 0)
                return this.Error3<FinanceSettingModel>(Resources.ParameterError);

            return await Task.Run(() =>
            {
                try
                {
                    var model = new FinanceSettingModel();

                    var financeSetting = _settingService.LoadSetting<FinanceSetting>(store ?? 0);

                    //默认账户集
                    var options = (from option in _accountingService.GetDefaultAccounts(store != null ? store : 0) select option).ToList();

                    model.Options = options.Select(o => o.ToModel<AccountingOptionModel>()).ToList();

                    //解析配置
                    var saleFinanceConfiguer = string.IsNullOrEmpty(financeSetting.SaleBillAccountingOptionConfiguration) ? null : JsonConvert.DeserializeObject<FinanceAccountingMap>(financeSetting.SaleBillAccountingOptionConfiguration);
                    var saleReservationFinanceConfiguer = string.IsNullOrEmpty(financeSetting.SaleReservationBillAccountingOptionConfiguration) ? null : JsonConvert.DeserializeObject<FinanceAccountingMap>(financeSetting.SaleReservationBillAccountingOptionConfiguration);

                    var returnFinanceConfiguer = string.IsNullOrEmpty(financeSetting.ReturnBillAccountingOptionConfiguration) ? null : JsonConvert.DeserializeObject<FinanceAccountingMap>(financeSetting.ReturnBillAccountingOptionConfiguration);
                    var returnReservationFinanceConfiguer = string.IsNullOrEmpty(financeSetting.ReturnReservationBillAccountingOptionConfiguration) ? null : JsonConvert.DeserializeObject<FinanceAccountingMap>(financeSetting.ReturnReservationBillAccountingOptionConfiguration);

                    var receiptFinanceConfiguer = string.IsNullOrEmpty(financeSetting.ReceiptAccountingOptionConfiguration) ? null : JsonConvert.DeserializeObject<FinanceAccountingMap>(financeSetting.ReceiptAccountingOptionConfiguration);
                    var paymentFinanceConfiguer = string.IsNullOrEmpty(financeSetting.PaymentAccountingOptionConfiguration) ? null : JsonConvert.DeserializeObject<FinanceAccountingMap>(financeSetting.PaymentAccountingOptionConfiguration);

                    var advanceReceiptFinanceConfiguer = string.IsNullOrEmpty(financeSetting.AdvanceReceiptAccountingOptionConfiguration) ? null : JsonConvert.DeserializeObject<FinanceAccountingMap>(financeSetting.AdvanceReceiptAccountingOptionConfiguration);
                    var advancePaymentFinanceConfiguer = string.IsNullOrEmpty(financeSetting.AdvancePaymentAccountingOptionConfiguration) ? null : JsonConvert.DeserializeObject<FinanceAccountingMap>(financeSetting.AdvancePaymentAccountingOptionConfiguration);

                    var purchaseFinanceConfiguer = string.IsNullOrEmpty(financeSetting.PurchaseBillAccountingOptionConfiguration) ? null : JsonConvert.DeserializeObject<FinanceAccountingMap>(financeSetting.PurchaseBillAccountingOptionConfiguration);
                    var purchaseReturnFinanceConfiguer = string.IsNullOrEmpty(financeSetting.PurchaseReturnBillAccountingOptionConfiguration) ? null : JsonConvert.DeserializeObject<FinanceAccountingMap>(financeSetting.PurchaseReturnBillAccountingOptionConfiguration);

                    var costExpenditureFinanceConfiguer = string.IsNullOrEmpty(financeSetting.CostExpenditureAccountingOptionConfiguration) ? null : JsonConvert.DeserializeObject<FinanceAccountingMap>(financeSetting.CostExpenditureAccountingOptionConfiguration);

                    var financialIncomeFinanceConfiguer = string.IsNullOrEmpty(financeSetting.FinancialIncomeAccountingOptionConfiguration) ? null : JsonConvert.DeserializeObject<FinanceAccountingMap>(financeSetting.FinancialIncomeAccountingOptionConfiguration);

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


                    model.FinanceReceiveAccountingAccountingMap = financeSetting.FinanceReceiveAccountingOptionConfiguration();

                    return this.Successful3("", model);
                }
                catch (Exception ex)
                {
                    return this.Error3<FinanceSettingModel>(ex.Message);
                }
            });
        }
        #endregion

    }
}