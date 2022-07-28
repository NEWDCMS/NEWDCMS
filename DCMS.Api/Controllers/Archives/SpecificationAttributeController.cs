using DCMS.Api.Infrastructure.Mapper.Extensions;
using DCMS.Core;
using DCMS.Core.Domain.Products;
using DCMS.Services.Configuration;
using DCMS.Services.Products;
using DCMS.ViewModel.Models.Products;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Swashbuckle.AspNetCore.Annotations;
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace DCMS.Api.Controllers
{
    /// <summary>
    /// 用于商品信息管理
    /// </summary>
   [Authorize]
    [Route("api/v{version:apiVersion}/dcms/archives")]
    public partial class SpecificationAttributeController : BaseAPIController
    {
        private readonly ISpecificationAttributeService _specificationAttributeService;
        
        private readonly ISettingService _settingService;

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="specificationAttributeService"></param>
        /// <param name="settingService"></param>
        /// <param name="logger"></param>
        public SpecificationAttributeController(ISpecificationAttributeService specificationAttributeService,
           
            ISettingService settingService, ILogger<BaseAPIController> logger) : base(logger)
        {
            _specificationAttributeService = specificationAttributeService;
            
            _settingService = settingService;
        }



        /// <summary>
        /// 获取经销商商品单位规格属性
        /// </summary>
        /// <param name="store"></param>
        /// <returns></returns>
        [HttpGet("product/getSpecificationAttributeOptions/{store}")]
        [SwaggerOperation("getSpecificationAttributeOptions")]
        //[ValidateActionParameters]
        //[AuthBaseFilter]
        public async Task<APIResult<SpecificationModel>> GetSpecificationAttributeOptions(int? store)
        {
            if (!store.HasValue || store.Value == 0)
                return this.Error3<SpecificationModel>(Resources.ParameterError);

            return await Task.Run(() =>
            {
                try
                {
                    var model = new SpecificationModel();
                    //获取配置
                    var ps = _settingService.LoadSetting<ProductSetting>(store ?? 0);
                    //规格属性
                    model.smallOptions = _specificationAttributeService
                    .GetSpecificationAttributeOptionsBySpecificationAttribute(store ?? 0, ps.SmallUnitSpecificationAttributeOptionsMapping)
                    .Select(s => s.ToModel<SpecificationAttributeOptionModel>()).ToList();
                    model.strokOptions = _specificationAttributeService
                    .GetSpecificationAttributeOptionsBySpecificationAttribute(store ?? 0, ps.StrokeUnitSpecificationAttributeOptionsMapping)
                    .Select(s => s.ToModel<SpecificationAttributeOptionModel>()).ToList();
                    model.bigOptions = _specificationAttributeService
                    .GetSpecificationAttributeOptionsBySpecificationAttribute(store ?? 0, ps.BigUnitSpecificationAttributeOptionsMapping)
                    .Select(s => s.ToModel<SpecificationAttributeOptionModel>()).ToList();

                    return this.Successful3("", model);
                }
                catch (Exception ex)
                {
                    return this.Error3<SpecificationModel>(ex.Message);
                }

            });
        }
    }
}
