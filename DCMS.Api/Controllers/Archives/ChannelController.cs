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
    /// 用于渠道信息管理
    /// </summary>
   [Authorize]
    [Route("api/v{version:apiVersion}/dcms/archives")]
    public class ChannelController : BaseAPIController
    {
        private readonly IChannelService _channelService;

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="channelService"></param>
        public ChannelController(IChannelService channelService, ILogger<BaseAPIController> logger) : base(logger)
        {
            _channelService = channelService;
        }

        /// <summary>
        ///  获取经销商渠道档案
        /// </summary>
        /// <param name="store"></param>
        /// <returns></returns>
        [HttpGet("channel/getChannels/{store}")]
        [SwaggerOperation("getChannels")]
        //[ValidateActionParameters]
        //[AuthBaseFilter]
        public async Task<APIResult<IList<ChannelModel>>> GetChannels(int? store, string searchStr, int pageIndex = 0, int pageSize = int.MaxValue)
        {
            if (!store.HasValue || store.Value == 0)
                return this.Error2<ChannelModel>(Resources.ParameterError);

            return await Task.Run(() =>
            {
                try
                {
                    var channels = _channelService.GetChannels(searchStr, store != null ? store : 0, pageIndex, pageSize).Select(c => { return c.ToModel<ChannelModel>(); }).ToList();

                    return this.Successful("", channels);
                }
                catch (Exception ex)
                {
                    return this.Error<ChannelModel>(ex.Message);
                }

            });
        }



    }
}