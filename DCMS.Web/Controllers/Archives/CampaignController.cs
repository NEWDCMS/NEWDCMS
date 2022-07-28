using DCMS.Core;
using DCMS.Core.Caching;
using DCMS.Core.Domain.Campaigns;
using DCMS.Core.Domain.Products;
using DCMS.Services.Campaigns;
using DCMS.Services.Logging;
using DCMS.Services.Messages;
using DCMS.Services.Products;
using DCMS.Services.Purchases;
using DCMS.Services.Terminals;
using DCMS.Services.Users;
using DCMS.ViewModel.Models.Campaigns;
using DCMS.ViewModel.Models.Products;
using DCMS.ViewModel.Models.Terminals;
using DCMS.Web.Framework.Mvc.Filters;
using DCMS.Web.Infrastructure.Mapper.Extensions;
using DCMS.Web.Models;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Task = System.Threading.Tasks.Task;

namespace DCMS.Web.Controllers
{

    /// <summary>
    /// 用于促销活动管理
    /// </summary>
    public class CampaignController : BasePublicController
    {
        private readonly ICategoryService _productCategoryService;
        private readonly IProductService _productService;
        private readonly IUserService _userService;
        private readonly IUserActivityService _userActivityService;
        private readonly ICampaignService _campaignService;
        private readonly IChannelService _channelService;
        private readonly ISpecificationAttributeService _specificationAttributeService;
        private readonly IPurchaseBillService _purchaseBillService;
        private readonly IRedLocker _locker;

        public CampaignController(ICategoryService productCategoryService,
            IProductService productService,
            IUserService userService,
            IUserActivityService userActivityService,
            IWorkContext workContext,
            IStoreContext storeContext,
            ICampaignService campaignService,
            IChannelService channelService,
            ILogger loggerService,
            INotificationService notificationService,
            ISpecificationAttributeService specificationAttributeService,
            IPurchaseBillService purchaseBillService,
            IRedLocker locker
            ) : base(workContext, loggerService, storeContext, notificationService)
        {
            _productCategoryService = productCategoryService;
            _userService = userService;
            _userActivityService = userActivityService;
            _productService = productService;
            _campaignService = campaignService;
            _channelService = channelService;
            _specificationAttributeService = specificationAttributeService;
            _purchaseBillService = purchaseBillService;
            _locker = locker;
        }


        public IActionResult Index()
        {
            return RedirectToAction("List");
        }

        /// <summary>
        /// 活动列表
        /// </summary>
        /// <param name="name"></param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="pagenumber"></param>
        /// <returns></returns>
        [HttpGet]
        [AuthCode((int)AccessGranularityEnum.SalesPromotionView)]
        public IActionResult List(string name, string billNumber, string remark, int? channelId, DateTime? startTime = null, DateTime? endTime = null, bool showExpire = false, bool? enabled = null, int pagenumber = 0)
        {

            var model = new CampaignListModel
            {
                //StartTime = (startTime == null) ? DateTime.Parse((DateTime.Now).ToString("yyyy-MM-01 00:00:00")) : (DateTime)startTime,
                EndTime = (endTime == null) ? DateTime.Now.AddDays(1) : (DateTime)endTime
            };
            if (pagenumber > 0)
            {
                pagenumber -= 1;
            }

            var chennels = _channelService.GetAll(curStore?.Id ?? 0);
            var campaigns = _campaignService.GetAllCampaigns(curStore?.Id ?? 0, name, billNumber, remark, channelId ?? 0, model.StartTime, model.EndTime, showExpire, enabled, pagenumber, 30);

            model.Channels = new SelectList(chennels.Select(c =>
            {
                return new SelectListItem()
                {
                    Text = c.Name,
                    Value = c.Id.ToString()
                };
            }), "Value", "Text");
            model.ChannelId = channelId ?? null;

            model.Items = campaigns.Select(c =>
            {
                var campaignChannels = c.CampaignChannels.Select(p => p.ChannelId).ToArray();
                var m = c.ToModel<CampaignModel>();
                m.TotalDay = c.EndTime.Subtract(c.StartTime).Days;
                m.ValidlDay = c.EndTime.Subtract(DateTime.Now).Days;
                m.Channels = chennels.Select(cc => cc.ToModel<ChannelModel>()).Where(cc => campaignChannels.Contains(cc.Id)).ToList();
                return m;
            }).ToList();

            return View(model);
        }

        /// <summary>
        /// 创建活动
        /// </summary>
        /// <param name="store"></param>
        /// <returns></returns>
        [HttpGet]
        [AuthCode((int)AccessGranularityEnum.SalesPromotionSave)]
        public IActionResult Create(int? store)
        {
            var model = new CampaignModel();

            var chennels = _channelService.GetAll(curStore?.Id ?? 0);
            model.Channels = chennels.Select(c => c.ToModel<ChannelModel>()).ToList();

            model.BillNumber = "";
            model.MakeUserId = curUser.Id;
            model.MakeUserName = curUser.UserRealName;
            model.CreatedOnUtc = DateTime.Now.ToString();
            model.StartTime = DateTime.Now;
            model.EndTime = DateTime.Now;
            model.Enabled = true;
            return View(model);
        }

        [HttpPost, ParameterBasedOnFormNameAttribute("save-continue", "continueEditing")]
        [AuthCode((int)AccessGranularityEnum.SalesPromotionSave)]
        public async Task<JsonResult> Create(CampaignUpdateModel data, int? billId)
        {
            try
            {
                if (PeriodClosed(DateTime.Now))
                {
                    return Warning("当期已经结帐,禁止手工日记帐.");
                }

                if (data != null)
                {
                    #region 验证
                    string errMsg = string.Empty;
                    if (string.IsNullOrEmpty(data.Name))
                    {
                        return Warning("活动名称不能为空");
                    }
                    if (data.StartTime == null)
                    {
                        return Warning("活动开始时间不能为空");
                    }
                    if (data.EndTime == null)
                    {
                        return Warning("活动结束时间不能为空");
                    }
                    if (data.StartTime > data.EndTime)
                    {
                        return Warning("活动开始时间不能大于结束时间");
                    }
                    if (data.SelectedChannelIds == null || data.SelectedChannelIds.Length == 0)
                    {
                        return Warning("请选择渠道");
                    }
                    if (!string.IsNullOrWhiteSpace(data.ProtocolNum))
                    {
                        data.ProtocolNum = data.ProtocolNum.ToUpper();
                        if (!Regex.IsMatch(data.ProtocolNum, @"^(PZC){1}[0-9]{11}$"))
                        {
                            return Warning("请填入正确TPM协议编码");
                        }
                    }
                    #endregion

                    var campaign = new Campaign();
                    if (billId.HasValue && billId.Value != 0)
                    {
                        campaign = _campaignService.GetCampaignById(curStore.Id, billId ?? 0);
                        //单据不存在
                        if (campaign == null)
                        {
                            return Warning("促销活动不存在");
                        }
                    }

                    var dataTo = data.ToEntity<CampaignUpdate>();

                    dataTo.CampaignBuyProducts = data.CampaignBuyProducts.Select(it =>
                    {
                        return it.ToEntity<CampaignBuyProduct>();
                    }).ToList();
                    dataTo.CampaignGiveProducts = data.CampaignGiveProducts.Select(it =>
                    {
                        return it.ToEntity<CampaignGiveProduct>();
                    }).ToList();


                    //RedLock
                    var result = await _locker.PerformActionWithLockAsync(LockKey(data),
                        TimeSpan.FromSeconds(30),
                        TimeSpan.FromSeconds(10),
                        TimeSpan.FromSeconds(1),
                        () => _campaignService.BillCreateOrUpdate(curStore.Id, curUser.Id, billId, campaign, dataTo, dataTo.CampaignBuyProducts, dataTo.CampaignGiveProducts, _userService.IsAdmin(curStore.Id, curUser.Id)));

                    return Successful("操作成功.", new { result.Code });
                }

                return Warning("操作失败.", new { Code = 0 });
            }
            catch (Exception)
            {
                //活动日志
                _userActivityService.InsertActivity("CreateOrUpdate", Resources.Bill_CreateOrUpdateFailed, curUser.Id);
                _notificationService.SuccessNotification(Resources.Bill_CreateOrUpdateFailed);

                return Warning("操作失败.", new { Code = 0 });
            }

        }

        /// <summary>
        /// 编辑活动
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [AuthCode((int)AccessGranularityEnum.SalesPromotionUpdate)]
        public IActionResult Edit(int id = 0)
        {

            var model = new CampaignModel();

            var chennels = _channelService.GetAll(curStore?.Id ?? 0);
            var campaign = _campaignService.GetCampaignById(curStore.Id, id);

            if (campaign == null)
            {
                return RedirectToAction("List");
            }
            //只能操作当前经销商数据
            else if (campaign.StoreId != curStore.Id)
            {
                return RedirectToAction("List");
            }

            if (campaign != null)
            {
                model = campaign.ToModel<CampaignModel>();
                model.Id = id;
                model.MakeUserId = curUser.Id;
                model.MakeUserName = curUser.UserRealName;
                model.Channels = chennels.Select(c => c.ToModel<ChannelModel>()).ToList();
                model.SelectedChannelIds = campaign.CampaignChannels.Select(c => c.ChannelId).ToArray();
                model.CampaignBuyProducts = campaign.BuyProducts.Select(p => p.ToModel<CampaignBuyProductModel>()).ToList();
                model.CampaignGiveProducts = campaign.GiveProducts.Select(p => p.ToModel<CampaignGiveProductModel>()).ToList();
            }
            return View(model);
        }


        [HttpPost, ParameterBasedOnFormNameAttribute("save-continue", "continueEditing")]
        [AuthCode((int)AccessGranularityEnum.SalesPromotionSave)]
        public async Task<JsonResult> Edit(CampaignUpdateModel data, int? billId)
        {
            try
            {
                if (data != null)
                {
                    #region 验证
                    string errMsg = string.Empty;
                    if (string.IsNullOrEmpty(data.Name))
                    {
                        return Warning("活动名称不能为空");
                    }
                    if (data.StartTime == null)
                    {
                        return Warning("活动开始时间不能为空");
                    }
                    if (data.EndTime == null)
                    {
                        return Warning("活动结束时间不能为空");
                    }
                    if (data.StartTime > data.EndTime)
                    {
                        return Warning("活动开始时间不能大于结束时间");
                    }
                    if (data.SelectedChannelIds == null || data.SelectedChannelIds.Length == 0)
                    {
                        return Warning("请选择渠道");
                    }
                    if (!string.IsNullOrWhiteSpace(data.ProtocolNum))
                    {
                        data.ProtocolNum = data.ProtocolNum.ToUpper();
                        if (!Regex.IsMatch(data.ProtocolNum, @"^(PZC){1}[0-9]{11}$"))
                        {
                            return Warning("请填入正确TPM协议编码");
                        }
                    }
                    #endregion

                    var campaign = new Campaign();
                    if (billId.HasValue && billId.Value != 0)
                    {
                        campaign = _campaignService.GetCampaignById(curStore.Id, billId ?? 0);
                        billId = campaign.Id;

                        //单据不存在
                        if (campaign == null)
                        {
                            return Warning("促销活动不存在");
                        }
                    }


                    var dataTo = data.ToEntity<CampaignUpdate>();

                    dataTo.CampaignBuyProducts = data.CampaignBuyProducts.Select(it => it.ToEntity<CampaignBuyProduct>()).ToList();
                    dataTo.CampaignGiveProducts = data.CampaignGiveProducts.Select(it => it.ToEntity<CampaignGiveProduct>()).ToList();


                    //RedLock
                    var result = await _locker.PerformActionWithLockAsync(LockKey(data),
                        TimeSpan.FromSeconds(30),
                        TimeSpan.FromSeconds(10),
                        TimeSpan.FromSeconds(1),
                        () => _campaignService.BillCreateOrUpdate(curStore.Id, curUser.Id, billId, campaign, dataTo, dataTo.CampaignBuyProducts, dataTo.CampaignGiveProducts, _userService.IsAdmin(curStore.Id, curUser.Id)));

                    return Json(new { result.Success, result.Message });
                }

                return Json(new { Success = false, Message = Resources.Bill_CreateOrUpdateFailed });
            }
            catch (Exception ex)
            {
                //活动日志
                _userActivityService.InsertActivity("CreateOrUpdate", Resources.Bill_CreateOrUpdateFailed, curUser.Id);
                _notificationService.SuccessNotification(Resources.Bill_CreateOrUpdateFailed);
                return Error(ex.Message);
            }

        }

        /// <summary>
        /// 删除活动
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [AuthCode((int)AccessGranularityEnum.SalesPromotionDelete)]
        public IActionResult Delete(int? id)
        {
            if (id.HasValue && id.Value != 0)
            {
                var campaign = _campaignService.GetCampaignById(curStore.Id, id.Value);
                if (campaign != null)
                {
                    campaign.CampaignChannels.ToList().ForEach(c =>
                    {
                        _campaignService.DeleteCampaignChannel(c);
                    });

                    campaign.BuyProducts.ToList().ForEach(p =>
                    {
                        _campaignService.DeleteCampaignBuyProduct(p);
                    });

                    campaign.GiveProducts.ToList().ForEach(p =>
                    {
                        _campaignService.DeleteCampaignGiveProduct(p);
                    });

                    _campaignService.DeleteCampaign(campaign);
                    //活动日志
                    _userActivityService.InsertActivity("DeleteCampaign", "删除活动", curUser, id);
                    _notificationService.SuccessNotification("删除活动成功");
                }
            }

            return RedirectToAction("List");
        }

        /// <summary>
        /// 异步获取购买商品
        /// </summary>
        /// <param name="campaignId"></param>
        /// <returns></returns>
        public async Task<JsonResult> AsyncBuyProducts(int campaignId)
        {
            return await Task.Run(() =>
            {
                var cproducts = _campaignService.GetCampaignBuyProductsByCampaignId(campaignId, _workContext.CurrentUser.Id, _storeContext.CurrentStore.Id, 0, 30);
                var allProducts = _productService.GetProductsByIds(curStore.Id, cproducts.Select(pr => pr.ProductId).Distinct().ToArray());
                var allOptions = _specificationAttributeService.GetSpecificationAttributeOptionByIds(curStore.Id, allProducts.GetProductBigStrokeSmallUnitIds());

                var products = cproducts.Select(o =>
                   {
                       var m = o.ToModel<CampaignBuyProductModel>();
                       var product = allProducts.Where(ap => ap.Id == m.ProductId).FirstOrDefault();
                       if (product != null)
                       {
                           m.ProductName = product != null ? product.Name : "";
                           m.ProductSKU = product != null ? product.Sku : "";
                           m.BarCode = product != null ? product.SmallBarCode : "";
                           m.UnitConversion = product.GetProductUnitConversion(allOptions);
                           //m.Units = product.GetProductUnits(_productService, _specificationAttributeService);
                           m.Units = product.GetProductUnits(allOptions);
                           //m.UnitName = m.Units.Keys.Select(k => k).ToArray()[2];
                           //m.UnitName = _specificationAttributeService.GetSpecificationAttributeOptionName(m.UnitId ?? 0);
                           var option = allOptions.Where(al => al.Id == m.UnitId).FirstOrDefault();
                           m.UnitName = option == null ? "" : option.Name;
                       }
                       return m;

                   }).ToList();

                return Json(new
                {
                    total = products.Count,
                    rows = products
                });
            });
        }

        /// <summary>
        /// 异步获取赠送商品
        /// </summary>
        /// <param name="campaignId"></param>
        /// <returns></returns>
        public async Task<JsonResult> AsyncGiveProducts(int campaignId)
        {

            return await Task.Run(() =>
            {
                var cproducts = _campaignService.GetCampaignGiveProductByCampaignId(campaignId, _workContext.CurrentUser.Id, curStore?.Id ?? 0, 0, 30);
                var allProducts = _productService.GetProductsByIds(curStore.Id, cproducts.Select(pr => pr.ProductId).Distinct().ToArray());
                var allOptions = _specificationAttributeService.GetSpecificationAttributeOptionByIds(curStore.Id, allProducts.GetProductBigStrokeSmallUnitIds());
                var products = cproducts.Select(o =>
                     {
                         var m = o.ToModel<CampaignGiveProductModel>();
                         var product = allProducts.Where(ap => ap.Id == m.ProductId).FirstOrDefault();
                         if (product != null)
                         {
                             m.CategoryName = _productCategoryService.GetCategoryName(curStore.Id, product.CategoryId);
                             m.ProductName = product != null ? product.Name : "";
                             m.ProductSKU = product != null ? product.Sku : "";
                             m.BarCode = product != null ? product.SmallBarCode : "";
                             m.UnitConversion = product.GetProductUnitConversion(allOptions);
                             //m.Units = product.GetProductUnits(_productService, _specificationAttributeService);
                             m.Units = product.GetProductUnits(allOptions);
                             ////m.UnitName = m.Units.Keys.Select(k => k).ToArray()[2];
                             //m.UnitName = _specificationAttributeService.GetSpecificationAttributeOptionName(m.UnitId ?? 0);
                             var option = allOptions.Where(al => al.Id == (m.UnitId )).FirstOrDefault();
                             m.UnitName = option == null ? "" : option.Name;
                         }
                         return m;

                     }).ToList();

                return Json(new
                {
                    total = products.Count,
                    rows = products
                });
            });
        }

        /// <summary>
        /// 异步获取所有可用活动的赠送商品 Campaign/AsyncAllGiveProducts?channelId=1
        /// </summary>
        /// <param name="campaignId"></param>
        /// <returns></returns>
        public async Task<JsonResult> AsyncAllGiveProducts(string key, int terminalId = 0, int channelId = 0, int wareHouseId = 0, int pageSize = 10, int pageIndex = 0)
        {

            return await Task.Run(() =>
            {
                var allCampaigns = _campaignService.GetAvailableCampaigns(key, curStore.Id, channelId, pageIndex, pageSize);
                var rows = new List<CampaignBuyGiveProductModel>();
                if (allCampaigns != null && allCampaigns.Count > 0)
                {

                    List<int> productIds = new List<int>();

                    //查询购买商品、赠送商品
                    allCampaigns.ToList().ForEach(ac =>
                    {
                        CampaignBuyGiveProductModel model = new CampaignBuyGiveProductModel
                        {
                            CampaignId = ac.Id,
                            CampaignName = ac.Name,
                            GiveTypeId = (int)GiveTypeEnum.Promotional,
                            //销售与赠送关联号
                            CampaignLinkNumber = CommonHelper.GetTimeStamp(DateTime.Now, 12),
                            //默认1倍
                            SaleBuyQuantity = 1
                        };

                        var buyProducts = _campaignService.GetCampaignBuyByCampaignId(ac.Id);
                        if (buyProducts != null && buyProducts.Count > 0)
                        {
                            buyProducts.ToList().ForEach(bp =>
                            {
                                model.CampaignBuyProducts.Add(new CampaignBuyProductModel() { Id = bp.Id, CampaignId = ac.Id, Quantity = bp.Quantity, UnitId = bp.UnitId??0, ProductId = bp.ProductId });
                                productIds.Add(bp.ProductId);
                            });
                        }
                        var giveProducts = _campaignService.GetCampaignGiveByCampaignId(ac.Id);
                        if (giveProducts != null && giveProducts.Count > 0)
                        {
                            giveProducts.ToList().ForEach(bp =>
                            {
                                model.CampaignGiveProducts.Add(new CampaignGiveProductModel() { Id = bp.Id, CampaignId = ac.Id, Quantity = bp.Quantity, UnitId = bp.UnitId??0, ProductId = bp.ProductId });
                                productIds.Add(bp.ProductId);
                            });
                        }
                        rows.Add(model);

                    });

                    IList<Product> allProducts = _productService.GetProductsByIds(curStore.Id, productIds.Distinct().ToArray());
                    IList<SpecificationAttributeOption> allOptions = _specificationAttributeService.GetSpecificationAttributeOptionByIds(curStore.Id, allProducts.GetProductBigStrokeSmallUnitIds());
                    IList<ProductPrice> allProductPrices = _productService.GetProductPricesByProductIds(curStore.Id, productIds.Distinct().ToArray());
                    var allProductTierPrices = _productService.GetProductTierPriceByProductIds(curStore.Id, productIds.Distinct().ToArray());

                    //关联商品详情
                    rows.ToList().ForEach(r =>
                    {
                        if (r.CampaignBuyProducts != null && r.CampaignBuyProducts.Count > 0)
                        {
                            r.CampaignBuyProducts.ToList().ForEach(a =>
                            {
                                Product product = allProducts.Where(ap => ap.Id == a.ProductId).FirstOrDefault();
                                product.Stocks = _productService.GetStocks(curStore.Id, new int[] { product.Id });
                                if (product != null)
                                {
                                    a.ProductName = product.Name;
                                    a.ProductSKU = product.Sku;
                                    a.BarCode = product.SmallBarCode;
                                    a.UnitConversion = product.GetProductUnitConversion(allOptions);
                                    //a.UnitName = allOptions.Where(ao => ao.Id == a.UnitId).Select(s => s.Name).FirstOrDefault();
                                    var option = allOptions.Where(al => al.Id == (a.UnitId )).FirstOrDefault();
                                    a.UnitName = option == null ? "" : option.Name;
                                    //a.Units = product.GetProductUnits(_productService, _specificationAttributeService);
                                    a.Units = product.GetProductUnits(allOptions);
                                    a.BigQuantity = product.BigQuantity;
                                    a.StrokeQuantity = product.StrokeQuantity;

                                    //这里替换成高级用法
                                    var p = product.ToModel<ProductModel>();
                                    p = product.InitBaseModel<ProductModel>(p, wareHouseId, allOptions, allProductPrices, allProductTierPrices, _productService, terminalId);
                                    a.Prices = p.Prices;
                                    a.ProductTierPrices = p.ProductTierPrices;
                                    a.StockQuantities = p.StockQuantities;
                                    a.BuyProductTypeId = (int)SaleProductTypeEnum.CampaignBuyProduct;
                                    a.BuyProductTypeName = CommonHelper.GetEnumDescription(SaleProductTypeEnum.CampaignBuyProduct);

                                    r.BuyProductMessage += (string.IsNullOrEmpty(r.BuyProductMessage) ? "" : "</br>") + $"{a.ProductName}({a.UnitConversion})({a.Quantity}{a.UnitName}).";

                                    //成本价
                                    a.CostPrices = _purchaseBillService.GetReferenceCostPrice(product);

                                }
                            });
                        }

                        if (r.CampaignGiveProducts != null && r.CampaignGiveProducts.Count > 0)
                        {
                            r.CampaignGiveProducts.ToList().ForEach(a =>
                            {
                                Product product = allProducts.Where(ap => ap.Id == a.ProductId).FirstOrDefault();
                                product.Stocks = _productService.GetStocks(curStore.Id, new int[] { product.Id });
                                if (product != null)
                                {
                                    a.ProductName = product.Name;
                                    a.ProductSKU = product.Sku;
                                    a.BarCode = product.SmallBarCode;
                                    a.UnitConversion = product.GetProductUnitConversion(allOptions);
                                    //a.UnitName = allOptions.Where(ao => ao.Id == a.UnitId).Select(s => s.Name).FirstOrDefault();
                                    var option = allOptions.Where(al => al.Id == (a.UnitId)).FirstOrDefault();
                                    a.UnitName = option == null ? "" : option.Name;
                                    //a.Units = product.GetProductUnits(_productService, _specificationAttributeService);
                                    a.Units = product.GetProductUnits(allOptions);
                                    a.BigQuantity = product.BigQuantity;
                                    a.StrokeQuantity = product.StrokeQuantity;

                                    //这里替换成高级用法
                                    var p = product.ToModel<ProductModel>();
                                    p = product.InitBaseModel<ProductModel>(p, wareHouseId, allOptions, allProductPrices, allProductTierPrices, _productService, terminalId);
                                    a.Prices = p.Prices;
                                    a.ProductTierPrices = p.ProductTierPrices;
                                    a.StockQuantities = p.StockQuantities;
                                    a.GiveProductTypeId = (int)SaleProductTypeEnum.CampaignGiveProduct;
                                    a.GiveProductTypeName = CommonHelper.GetEnumDescription(SaleProductTypeEnum.CampaignGiveProduct);

                                    r.GiveProductMessage += (string.IsNullOrEmpty(r.GiveProductMessage) ? "" : "</br>") + $"{a.ProductName}({a.UnitConversion})({a.Quantity}{a.UnitName}).";

                                    //成本价
                                    a.CostPrices = _purchaseBillService.GetReferenceCostPrice(product);
                                }
                            });
                        }

                    });

                }

                return Json(new
                {
                    total = allCampaigns.TotalCount,
                    rows
                });

            });
        }

    }
}
