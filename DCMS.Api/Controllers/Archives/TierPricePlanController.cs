using DCMS.Api.Infrastructure.Mapper.Extensions;
using DCMS.Core;
using DCMS.Core.Caching;
using DCMS.Core.Domain.Products;
using DCMS.Services.Products;
using DCMS.ViewModel.Models.Products;
using DCMS.Web.Framework.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Swashbuckle.AspNetCore.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace DCMS.Api.Controllers
{

    /// <summary>
    /// 获取层次价格
    /// </summary>
   [Authorize]
    [Route("api/v{version:apiVersion}/dcms/archives")]
    public class TierPricePlanController : BaseAPIController
    {
        private readonly IProductService _productService;
        private readonly IProductTierPricePlanService _productTierPricePlanService;
        
        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="productService"></param>
        public TierPricePlanController(IProductService productService
            , IProductTierPricePlanService productTierPricePlanService,
            ILogger<BaseAPIController> logger) : base(logger)
        {
            _productService = productService;
            _productTierPricePlanService = productTierPricePlanService;
            
        }

        /// <summary>
        /// 获取价格方案
        /// </summary>
        /// <param name="store"></param>
        /// <param name="name"></param>
        /// <param name="pagenumber"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        [HttpGet("pricePlan/getAllPricePlan/{store}")]
        [SwaggerOperation("getAllPricePlan")]
        //[ValidateActionParameters]
        //[AuthBaseFilter]
        public async Task<APIResult<IList<ProductPricePlan>>> GetAllPricePlan(int? store, string name, int pagenumber = 0, int pageSize = int.MaxValue)
        {
            if (!store.HasValue || store.Value == 0)
                return this.Error2<ProductPricePlan>(Resources.ParameterError);

            return await Task.Run(() =>
            {
                try
                {
                    if (pagenumber > 1)
                    {
                        pagenumber -= 1;
                    }

                    var result = _productTierPricePlanService.GetAllPricePlan(store);

                    return this.Successful2("", result);
                }
                catch (Exception ex)
                {
                    return this.Error2<ProductPricePlan>(ex.Message);
                }

            });
        }

        /// <summary>
        /// 获取经销商所有价格方案
        /// </summary>
        /// <param name="store"></param>
        /// <returns></returns>
        [HttpGet("pricePlan/getAllProductTierPrice/{store}")]
        [SwaggerOperation("getAllProductTierPrice")]
        //[ValidateActionParameters]
        //[AuthBaseFilter]
        public async Task<APIResult<IList<ProductTierPriceModel>>> GetAllProductTierPrice(int? store, int productld)
        {
            if (!store.HasValue || store.Value == 0)
                return this.Error2<ProductTierPriceModel>(Resources.ParameterError);

            return await Task.Run(() =>
         {
             try
             {
                 var productPricePlans = _productTierPricePlanService.GetAllPricePlan(store ?? 0);
                 var productTierPrices = new List<ProductTierPrice>();

                 foreach (ProductPricePlan productPricePlan in productPricePlans)
                 {
                     var productTierPrice = _productService.GetProductTierPriceById(store ?? 0, productld, productPricePlan.PricesPlanId, productPricePlan.PriceTypeId);
                     if (productTierPrice != null)
                     {
                         productTierPrice.PriceType = (PriceType)productPricePlan.PriceTypeId;
                         productTierPrices.Add(productTierPrice);
                     }
                 }

                 var results = productTierPrices.Select(s =>
                 {
                     var a = s.ToModel<ProductTierPriceModel>();

                     if (a.PriceTypeId == 0)
                     {
                         a.PriceTypeName = "进价";
                     }
                     if (a.PriceTypeId == 1)
                     {
                         a.PriceTypeName = "批发价格";
                     }
                     if (a.PriceTypeId == 2)
                     {
                         a.PriceTypeName = "零售价格";
                     }
                     if (a.PriceTypeId == 3)
                     {
                         a.PriceTypeName = "最低售价";
                     }
                     if (a.PriceTypeId == 4)
                     {
                         a.PriceTypeName = "上次售价";
                     }
                     if (a.PriceTypeId == 5)
                     {
                         a.PriceTypeName = "成本价";
                     }
                     if (a.PriceTypeId == 88)
                     {
                         a.PriceTypeName = "自定义方案";
                     }

                     a.StoreId = s.StoreId;
                     a.PriceTypeId = s.PriceTypeId;
                     a.SmallUnitPrice = s.SmallUnitPrice;
                     a.StrokeUnitPrice = s.StrokeUnitPrice;
                     a.BigUnitPrice = s.BigUnitPrice;

                     return a;

                 }).ToList();

                 return this.Successful("", results);

             }
             catch (Exception ex)
             {
                 return this.Error<ProductTierPriceModel>(ex.Message);
             }

         });
        }
    }
}
