using DCMS.Core;
using DCMS.Core.Caching;
using DCMS.Core.Domain.Configuration;
using DCMS.Services.Configuration;
using DCMS.ViewModel.Models.Configuration;
using DCMS.Web.Framework.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Swashbuckle.AspNetCore.Annotations;
using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace DCMS.Api.Controllers.Setting
{
    /// <summary>
    /// 消息推送
    /// </summary>
   [Authorize]
    [Route("api/v{version:apiVersion}/dcms/config")]
    public class PushConfigController : BaseAPIController
    {
        private readonly ISettingService _settingService;
        

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="settingService"></param>
        public PushConfigController(
            ISettingService settingService,
            ILogger<BaseAPIController> logger) : base(logger)
        {
            _settingService = settingService;
            
        }

        /// <summary>
        /// 获取经销商消息推送项目配置
        /// </summary>
        /// <returns></returns>
        [HttpGet("push/getPushConfigSetting/{store}")]
        [SwaggerOperation("getPushConfigSetting")]
        //[ValidateActionParameters]
        //[AuthBaseFilter]
        public async Task<APIResult<PushSettingsModel>> GetPushOptionSetting(int? store)
        {
            if (!store.HasValue || store.Value == 0)
                return this.Error3<PushSettingsModel>(Resources.ParameterError);

            return await Task.Run(() =>
            {
                var result = new APIResult<PushSettingsModel>();
                try
                {
                    var model = new PushSettingsModel();
                    var pushSetting = _settingService.LoadSetting<PushSetting>(store ?? 0);
                    model.Host = pushSetting.Host;
                    model.VirtualHost = pushSetting.VirtualHost;
                    model.UserName = pushSetting.UserName;
                    model.Password = pushSetting.Password;
                    return this.Successful3("", model);
                }
                catch (Exception ex)
                {
                    return this.Error3<PushSettingsModel>(ex.Message);
                }
            });
        }
    }
}