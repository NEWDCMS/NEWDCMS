using DCMS.Api.Infrastructure.Mapper.Extensions;
using DCMS.Core;
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
    /// 用于商品品牌管理
    /// </summary>
   [Authorize]
    [Route("api/v{version:apiVersion}/dcms/archives")]
    public class BrandController : BaseAPIController
    {
        private readonly IBrandService _brandService;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="brandService"></param>
        public BrandController(IBrandService brandService, ILogger<BaseAPIController> logger) : base(logger)
        {
            _brandService = brandService;

        }

        /// <summary>
        /// 获取商品品牌信息
        /// </summary>
        /// <param name="store"></param>
        /// <param name="name"></param>
        /// <param name="pagenumber"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        [HttpGet("getAllBrands/{store}")]
        [SwaggerOperation("getAllBrands")]
        //[ValidateActionParameters]
        //[AuthBaseFilter]
        public async Task<APIResult<IList<BrandModel>>> GetAllBrands(int? store, string name = "", int pagenumber = 0, int pageSize = int.MaxValue)
        {
            if (!store.HasValue || store.Value == 0)
                return this.Error<BrandModel>(Resources.ParameterError);

            return await Task.Run(() =>
            {
                try
                {
                    if (pagenumber > 1)
                    {
                        pagenumber -= 1;
                    }

                    var brands = _brandService.GetAllBrands(store != null ? store : 0, name, pageIndex: pagenumber, pageSize: pageSize);
                    var result = brands.Select(s => s.ToModel<BrandModel>()).ToList();

                    return this.Successful(Resources.Successful, result);
                }
                catch (Exception ex)
                {
                    return this.Error<BrandModel>(ex.Message);
                }
            });
        }

    }
}
