using DCMS.Api.Infrastructure.Mapper.Extensions;
using DCMS.Core;
using DCMS.Services.Terminals;
using DCMS.ViewModel.Models.Terminals;
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
    /// 用于等级信息管理
    /// </summary>
   [Authorize]
    [Route("api/v{version:apiVersion}/dcms/archives")]
    public class RankController : BaseAPIController
    {
        private readonly IRankService _rankService;

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="rankService"></param>
        public RankController(IRankService rankService, ILogger<BaseAPIController> logger) : base(logger)
        {
            _rankService = rankService;
        }


        /// <summary>
        /// 获取经销商等级
        /// </summary>
        /// <param name="store"></param>
        /// <returns></returns>
        [HttpGet("rank/getRanks/{store}")]
        [SwaggerOperation("getRanks")]
        //[ValidateActionParameters]
        //[AuthBaseFilter]
        public async Task<APIResult<IList<RankModel>>> GetRanks(int? store, string searchStr, int pageIndex = 0, int pageSize = int.MaxValue)
        {
            if (!store.HasValue || store.Value == 0)
                return this.Error<RankModel>(Resources.ParameterError);

            return await Task.Run(() =>
            {

                try
                {
                    var channels = _rankService.GetRanks(searchStr, store ?? 0, pageIndex, pageSize).Select(c => { return c.ToModel<RankModel>(); }).ToList();
                    return this.Successful("", channels);
                }
                catch (Exception ex)
                {
                    return this.Error<RankModel>(ex.Message);
                }

            });
        }



    }
}