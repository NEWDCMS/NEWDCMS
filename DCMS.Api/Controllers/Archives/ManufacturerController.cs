using DCMS.Api.Infrastructure.Mapper.Extensions;
using DCMS.Core;
using DCMS.Core.Domain.Products;
using DCMS.Services.Common;
using DCMS.Services.Products;
using DCMS.ViewModel.Models.Products;
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
    /// 用于供应商信息管理
    /// </summary>
   [Authorize]
    [Route("api/v{version:apiVersion}/dcms/archives")]
    public class ManufacturerController : BaseAPIController
    {
        private readonly IManufacturerService _manufacturerService;
        private readonly ICommonBillService _commonBillService;

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="manufacturerService"></param>
        /// <param name="commonBillService"></param>
        /// <param name="logger"></param>
        public ManufacturerController(
            IManufacturerService manufacturerService,
            ICommonBillService commonBillService
          , ILogger<BaseAPIController> logger) : base(logger)
        {
            _manufacturerService = manufacturerService;
            _commonBillService = commonBillService;
        }

        /// <summary>
        /// 获取经销商供应商信息
        /// </summary>
        /// <param name="store"></param>
        /// <returns></returns>

        [HttpGet("manufacturer/getManufacturers/{store}")]
        [SwaggerOperation("getManufacturers")]
        //[ValidateActionParameters]
        //[AuthBaseFilter]
        public async Task<APIResult<IList<ManufacturerModel>>> GetManufacturers(int? store, string searchStr, int pageIndex = 0, int pageSize = int.MaxValue)
        {
            if (!store.HasValue || store.Value == 0)
                return this.Error2<ManufacturerModel>(Resources.ParameterError);

            return await Task.Run(() =>
            {
                try
                {
                    var result = new List<ManufacturerModel>();
                    result = _manufacturerService.GetAllManufactureies(searchStr, store != null ? store : 0, pageIndex, pageSize).Select(u => u.ToModel<ManufacturerModel>()).ToList();
                    return this.Successful("", result);
                }
                catch (Exception ex)
                {
                    return this.Error2<ManufacturerModel>(ex.Message);

                }
            });
        }


        /// <summary>
        /// 获取供应商账户余额
        /// </summary>
        /// <param name="store"></param>
        /// <param name="manufacturerId"></param>
        /// <returns></returns>
        [HttpGet("manufacturer/getmanufacturerbalance/{store}/{manufacturerId}")]
        [SwaggerOperation("getManufacturerBalance")]
        //[ValidateActionParameters]
        //[AuthBaseFilter]
        public async Task<APIResult<ManufacturerBalance>> GetManufacturerBalance(int? store, int? manufacturerId)
        {
            if (!store.HasValue || store.Value == 0)
                return this.Error3<ManufacturerBalance>(Resources.ParameterError);

            return await Task.Run(() =>
            {
                try
                {

                    var terminalBalance = _commonBillService.CalcManufacturerBalance(store ?? 0, manufacturerId ?? 0);
                    return this.Successful3("", terminalBalance);
                }
                catch (Exception ex)
                {
                    return this.Error3<ManufacturerBalance>(ex.Message);

                }

            });
        }

    }
}