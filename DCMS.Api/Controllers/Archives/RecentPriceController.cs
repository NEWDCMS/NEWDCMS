using DCMS.Api.Infrastructure.Mapper.Extensions;
using DCMS.Core;
using DCMS.Core.Caching;
using DCMS.Services.Products;
using DCMS.Services.Terminals;
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
    /// 用于促销售价格管理
    /// </summary>
   [Authorize]
    [Route("api/v{version:apiVersion}/dcms/archives")]
    public class RecentPriceController : BaseAPIController
    {
        private readonly IProductService _productService;
        private readonly ITerminalService _terminalService;
        

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="productService"></param>
        /// <param name="cacheManager"></param>
        /// <param name="terminalService"></param>
        public RecentPriceController(
            IProductService productService,
            ITerminalService terminalService,
            ILogger<BaseAPIController> logger) : base(logger)
        {
            _productService = productService;
            _terminalService = terminalService;
            
        }

        /// <summary>
        /// 获取上次售价
        /// </summary>
        /// <param name="store"></param>
        /// <param name="productName"></param>
        /// <param name="cumtomerName"></param>
        /// <param name="pagenumber"></param>
        /// <returns></returns>
        [HttpGet("product/getAllRecentPrices/{store}")]
        [SwaggerOperation("getAllRecentPrices")]
        //[ValidateActionParameters]
        //[AuthBaseFilter]
        public async Task<APIResult<IList<RecentPriceModel>>> GetAllRecentPrices(int? store, string cumtomerName = "", string productName = "", int pagenumber = 0)
        {
            if (!store.HasValue || store.Value == 0)
                return this.Error<RecentPriceModel>(Resources.ParameterError);


            return await Task.Run(() =>
            {
                try
                {
                    if (pagenumber > 0)
                    {
                        pagenumber -= 1;
                    }

                    #region 查询需要关联其他表的数据
                    var recentPrices = _productService.GetAllRecentPrices(store ?? 0, productName, cumtomerName, pagenumber, 30);
                    var allProducts = _productService.GetProductsByIds(store ?? 0, recentPrices.Select(pr => pr.ProductId).Distinct().ToArray());
                    var allTerminal = _terminalService.GetTerminalsByIds(store, recentPrices.Select(pr => pr.CustomerId).Distinct().ToArray());
                    #endregion

                    var result = recentPrices.Select(r =>
                    {
                        var product = allProducts.Where(ap => ap.Id == r.ProductId).FirstOrDefault();
                        var customer = allTerminal.Where(at => at.Id == r.CustomerId).FirstOrDefault();
                        var m = r.ToModel<RecentPriceModel>();
                        m.ProductName = product != null ? product.Name : "";
                        m.CustomerName = customer != null ? customer.Name : "";
                        return m;
                    }).ToList();


                    return this.Successful("", result);
                }
                catch (Exception ex)
                {
                    return this.Error<RecentPriceModel>(ex.Message);
                }

            });
        }


    }
}
