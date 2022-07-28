using DCMS.Api.Infrastructure.Mapper.Extensions;
using DCMS.Core;
using DCMS.Core.Domain.Products;
using DCMS.Services.Campaigns;
using DCMS.Services.Messages;
using DCMS.Services.Products;
using DCMS.Services.Purchases;
using DCMS.ViewModel.Models.Campaigns;
using DCMS.ViewModel.Models.Products;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Swashbuckle.AspNetCore.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Task = System.Threading.Tasks.Task;
using DCMS.Api.Models;
using Microsoft.AspNetCore.Authorization;

namespace DCMS.Api.Controllers
{

    /// <summary>
    /// 用于促销活动管理
    /// </summary>
   [Authorize]
    [Route("api/v{version:apiVersion}/dcms/archives")]
    public class CampaignController : BaseAPIController
    {
        private readonly IProductService _productService;
        private readonly ICampaignService _campaignService;
        private readonly ISpecificationAttributeService _specificationAttributeService;
        private readonly IPurchaseBillService _purchaseBillService;

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="productService"></param>
        /// <param name="workContext"></param>
        /// <param name="storeContext"></param>
        /// <param name="campaignService"></param>
        /// <param name="notificationService"></param>
        /// <param name="specificationAttributeService"></param>
        /// <param name="purchaseBillService"></param>
        /// <param name="logger"></param>
        public CampaignController(
            IProductService productService,
            IWorkContext workContext,
            IStoreContext storeContext,
            ICampaignService campaignService,
            INotificationService notificationService,
            ISpecificationAttributeService specificationAttributeService,
            IPurchaseBillService purchaseBillService,
            ILogger<BaseAPIController> logger) : base(logger)
        {
            _productService = productService;
            _campaignService = campaignService;
            _specificationAttributeService = specificationAttributeService;
            _purchaseBillService = purchaseBillService;
        }

        /// <summary>
        /// 获取所有可用活动的赠送商品
        /// </summary>
        /// <param name="store"></param>
        /// <param name="name"></param>
        /// <param name="terminalId"></param>
        /// <param name="channelId"></param>
        /// <param name="wareHouseId"></param>
        /// <param name="pagenumber"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        [HttpGet("campaign/getAllCampaigns/{store}")]
        [SwaggerOperation("getAllCampaigns")]
        //[AuthBaseFilter]
        public async Task<APIResult<IList<CampaignBuyGiveProductModel>>> GetAllCampaigns(int? store, string name, int terminalId = 0, int channelId = 0, int wareHouseId = 0, int pagenumber = 0, int pageSize = 50)
        {
            if (!store.HasValue || store.Value == 0)
                return this.Error2<CampaignBuyGiveProductModel>(Resources.ParameterError);

            return await Task.Run(() =>
            {
                try
                {
                    if (pagenumber > 0)
                    {
                        pagenumber -= 1;
                    }

                    var allCampaigns = _campaignService.GetAvailableCampaigns(name, store??0, channelId, pagenumber, pageSize);
                    var results = new List<CampaignBuyGiveProductModel>();
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
                            var lst = new List<int>();
                            var buyProducts = _campaignService.GetCampaignBuyByCampaignId(ac.Id);
                            if (buyProducts != null && buyProducts.Count > 0)
                            {
                                buyProducts.ToList().ForEach(bp =>
                                {
                                    model.CampaignBuyProducts.Add(new CampaignBuyProductModel() { Id = bp.Id, CampaignId = ac.Id, Quantity = bp.Quantity, UnitId = bp.UnitId??0, ProductId = bp.ProductId });
                                    productIds.Add(bp.ProductId);
                                    lst.Add(bp.ProductId);
                                });
                            }
                            var giveProducts = _campaignService.GetCampaignGiveByCampaignId(ac.Id);
                            if (giveProducts != null && giveProducts.Count > 0)
                            {
                                giveProducts.ToList().ForEach(bp =>
                                {
                                    model.CampaignGiveProducts.Add(new CampaignGiveProductModel() { Id = bp.Id, CampaignId = ac.Id, Quantity = bp.Quantity, UnitId = bp.UnitId??0, ProductId = bp.ProductId });
                                    productIds.Add(bp.ProductId);
                                    lst.Add(bp.ProductId);
                                });
                            }
                            //判断是否有禁用或删除的商品
                            var count = _productService.GetProductsByIds(store ?? 0, lst.Distinct().ToArray()).Where(w => w.Deleted == true || w.Status == false).Count();
                            if (count == 0) results.Add(model);
                        });

                        var allProducts = _productService.GetProductsByIds(store ?? 0, productIds.Distinct().ToArray());
                        var allOptions = _specificationAttributeService.GetSpecificationAttributeOptionByIds(store ?? 0, allProducts.GetProductBigStrokeSmallUnitIds());
                        var allProductPrices = _productService.GetProductPricesByProductIds(store ?? 0, productIds.Distinct().ToArray());
                        var allProductTierPrices = _productService.GetProductTierPriceByProductIds(store ?? 0, productIds.Distinct().ToArray());

                        //关联商品详情
                        results.ToList().ForEach(r =>
                        {
                            if (r.CampaignBuyProducts != null && r.CampaignBuyProducts.Count > 0)
                            {
                                r.CampaignBuyProducts.ToList().ForEach(a =>
                                {
                                    Product product = allProducts.Where(ap => ap.Id == a.ProductId).FirstOrDefault();
                                    if (product != null)
                                    {
                                        a.ProductName = product.Name;
                                        a.ProductSKU = product.Sku;
                                        a.BarCode = product.SmallBarCode;
                                        a.UnitConversion = product.GetProductUnitConversion(allOptions);
                                        var option = allOptions.Where(al => al.Id == (a.UnitId )).FirstOrDefault();
                                        a.UnitName = option == null ? "" : option.Name;
                                        a.Units = product.GetProductUnits(allOptions);
                                        a.BigQuantity = product.BigQuantity;
                                        a.StrokeQuantity = product.StrokeQuantity;
                                        //这里替换成高级用法
                                        var p = product.ToModel<ProductModel>();
                                        p = product.InitBaseModel<ProductModel>(p, wareHouseId, allOptions, allProductPrices, allProductTierPrices, _productService, terminalId);

                                        a.BigUnitId = p.BigUnitId;
                                        a.StrokeUnitId = p.StrokeUnitId;
                                        a.SmallUnitId = p.SmallUnitId;

                                        a.Prices = p.Prices;
                                        a.Price = p.Prices.Where(s => s.UnitId == a.UnitId).Select(s => s.ProductPrice.RetailPrice).FirstOrDefault() ?? 0;
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
                                    if (product != null)
                                    {
                                        a.ProductName = product.Name;
                                        a.ProductSKU = product.Sku;
                                        a.BarCode = product.SmallBarCode;
                                        a.UnitConversion = product.GetProductUnitConversion(allOptions);
                                        var option = allOptions.Where(al => al.Id == (a.UnitId)).FirstOrDefault();
                                        a.UnitName = option == null ? "" : option.Name;
                                        a.Units = product.GetProductUnits(allOptions);
                                        a.BigQuantity = product.BigQuantity;
                                        a.StrokeQuantity = product.StrokeQuantity;
                                        //这里替换成高级用法
                                        var p = product.ToModel<ProductModel>();
                                        p = product.InitBaseModel<ProductModel>(p, wareHouseId, allOptions, allProductPrices, allProductTierPrices, _productService, terminalId);

                                        a.BigUnitId = p.BigUnitId;
                                        a.StrokeUnitId = p.StrokeUnitId;
                                        a.SmallUnitId = p.SmallUnitId;

                                        a.Prices = p.Prices;
                                        a.Price = p.Prices.Where(s => s.UnitId == a.UnitId).Select(s => s.ProductPrice.RetailPrice).FirstOrDefault() ?? 0;
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

                    return this.Successful2(Resources.Successful, results);
                }
                catch (Exception ex)
                {
                    return this.Error2<CampaignBuyGiveProductModel>(ex.Message);
                }
            });
        }

    }
}
