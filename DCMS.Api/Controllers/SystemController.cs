using DCMS.Core;
using DCMS.Core.Domain;
using DCMS.Core.Domain.Configuration;
using DCMS.Core.Domain.Logging;
using DCMS.Core.Domain.Security;
using DCMS.Services.Configuration;
using DCMS.Services.Logging;
using DCMS.Services.Security;
using DCMS.Services.Settings;
using DCMS.Services.Users;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Swashbuckle.AspNetCore.Annotations;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using DCMS.Core.Configuration;
using DCMS.Services.Sales;
using DCMS.Services.Terminals;
using Newtonsoft.Json;

namespace DCMS.Api.Controllers
{
    /// <summary>
    /// 用于系统配置
    /// </summary>
   [Authorize]
    [Route("api/v{version:apiVersion}/dcms/system")]
    public class SystemController : BaseAPIController
    {
        private readonly ISettingService _settingService;
        private readonly IUserActivityService _userActivityService;
        private readonly DCMSConfig _dCMSConfig;
        private readonly IModuleService _moduleService;
        private readonly IPermissionService _permissionService;
        private readonly IDispatchBillService _dispatchBillService;
        private readonly ITerminalService _terminalService;
        private readonly IUserService _userService;

        /// <summary>
        /// 用于系统配置
        /// </summary>
        /// <param name="settingService"></param>
        /// <param name="userActivityService"></param>
        /// <param name="dCMSConfig"></param>
        /// <param name="moduleService"></param>
        /// <param name="permissionService"></param>
        /// <param name="dispatchBillService"></param>
        /// <param name="terminalService"></param>
        /// <param name="accountingService"></param>
        /// <param name="logger"></param>
        public SystemController(ISettingService settingService,
            IUserActivityService userActivityService,
            DCMSConfig dCMSConfig,
            IModuleService moduleService,
            IPermissionService permissionService,
            IDispatchBillService dispatchBillService,
            IUserService userService,
            ITerminalService terminalService,
            IAccountingService accountingService, ILogger<BaseAPIController> logger) : base(logger)
        {
            _settingService = settingService;
            _userActivityService = userActivityService;
            _dCMSConfig = dCMSConfig;
            _userService = userService;
            _moduleService = moduleService;
            _permissionService = permissionService;
            _dispatchBillService = dispatchBillService;
            _terminalService = terminalService;
        }


        /// <summary>
        /// 获取推送终结点
        /// </summary>
        /// <returns></returns>
        [HttpGet("app/android/getpushermqendpoint")]
        [SwaggerOperation("getPusherMQEndpoint")]
        public async Task<string> GetPusherMQEndpoint()
        {
            return await Task.Run(() =>
            {
                return DCMSConfigExt.RabbitMQConnectionString(_dCMSConfig);
            });
        }



        /// <summary>
        /// 检查Android客户端版本更新
        /// </summary>
        /// <returns></returns>
        [HttpGet("app/android/checkupdate")]
        [SwaggerOperation("checkupdate")]
        //[ValidateActionParameters]
        //[AuthBaseFilter]
        public async Task<APIResult<VersionUpdateSetting>> CheckUpdateForAndroid()
        {
            return await Task.Run(() =>
            {
                try
                {
                    var model = _settingService.LoadSetting<VersionUpdateSetting>();
                    return this.Successful3("", model);
                }
                catch (Exception ex)
                {
                    return this.Error3<VersionUpdateSetting>(ex.Message);
                }
            });
        }

        /// <summary>
        /// 获取传统配置
        /// </summary>
        /// <returns></returns>
        [HttpGet("traditionsetting")]
        [SwaggerOperation("traditionsetting")]
        //[ValidateActionParameters]
        //[AuthBaseFilter]
        public async Task<APIResult<TraditionSetting>> GetTraditionSetting()
        {
            return await Task.Run(() =>
            {
                var model = new TraditionSetting();
                try
                {
                    model = _settingService.LoadSetting<TraditionSetting>();
                    return this.Successful3("", model);
                }
                catch (Exception ex)
                {
                    return this.Error3<TraditionSetting>(ex.Message);
                }

            });
        }

        /// <summary>
        /// 获取餐饮配置
        /// </summary>
        /// <returns></returns>
        [HttpGet("restaurantsetting")]
        [SwaggerOperation("restaurantsetting")]
        //[ValidateActionParameters]
        //[AuthBaseFilter]
        public async Task<APIResult<RestaurantSetting>> GetRestaurantSetting()
        {
            return await Task.Run(() =>
            {
                var model = new RestaurantSetting();
                try
                {
                    model = _settingService.LoadSetting<RestaurantSetting>();
                    return this.Successful3("", model);
                }
                catch (Exception ex)
                {
                    return this.Error3<RestaurantSetting>(ex.Message);
                }

            });
        }

        /// <summary>
        /// 获取销售配置
        /// </summary>
        /// <returns></returns>
        [HttpGet("salesproductsetting")]
        [SwaggerOperation("salesproductsetting")]
        //[ValidateActionParameters]
        //[AuthBaseFilter]
        public async Task<APIResult<SalesProductSetting>> GetSalesProductSetting()
        {
            return await Task.Run(() =>
            {
                var model = new SalesProductSetting();
                try
                {
                    model = _settingService.LoadSetting<SalesProductSetting>();
                    return this.Successful3("", model);
                }
                catch (Exception ex)
                {
                    return this.Error3<SalesProductSetting>(ex.Message);
                }

            });
        }

        /// <summary>
        /// 获取新闻配置
        /// </summary>
        /// <returns></returns>
        [HttpGet("newssetting")]
        [SwaggerOperation("newssetting")]
        //[ValidateActionParameters]
        //[AuthBaseFilter]
        public async Task<APIResult<NewsSetting>> GetNewsSetting()
        {
            return await Task.Run(() =>
            {
                var model = new NewsSetting();
                try
                {
                    model = _settingService.LoadSetting<NewsSetting>();
                    return this.Successful3("", model);
                }
                catch (Exception ex)
                {
                    return this.Error3<NewsSetting>(ex.Message);
                }

            });

        }

        /// <summary>
        /// 创建日志
        /// </summary>
        /// <param name="content"></param>
        /// <param name="userId"></param>
        /// <param name="store"></param>
        /// <returns></returns>
        [HttpPut("insertactivity")]
        [SwaggerOperation("insertactivity")]
        //[ValidateActionParameters]
        //[AuthBaseFilter]
        public async Task<APIResult<dynamic>> InsertActivity(string content, int userId, int? store)
        {
            if (!store.HasValue && store.Value == 0)
                return this.Error(Resources.ParameterError);

            return await Task.Run(() =>
            {
                try
                {
                    var result = new ActivityLog();
                    string title = "Android";
                    if (!string.IsNullOrWhiteSpace(content))
                    {
                        result = _userActivityService.InsertActivity(title, content, userId);
                    }

                    return this.Successful("", result);
                }
                catch (Exception ex)
                {
                    return this.Error(ex.Message);
                }

            });
        }

        /// <summary>
        /// 上报客户端异常
        /// </summary>
        /// <param name="content"></param>
        /// <param name="userId"></param>
        /// <param name="store"></param>
        /// <returns></returns>
        [HttpPut("catchlogs")]
        [SwaggerOperation("catchlogs")]
        //[ValidateActionParameters]
        //[AuthBaseFilter]
        public async Task<APIResult<dynamic>> Catchlogs(string content, int userId, int? store)
        {
            if (!store.HasValue && store.Value == 0)
                return this.Error(Resources.ParameterError);

            return await Task.Run(() =>
            {
                try
                {
                    return this.Successful("");
                }
                catch (Exception ex)
                {
                    return this.Error(ex.Message);
                }

            });
        }

        /// <summary>
        /// 获取系统模块权限初始
        /// </summary>
        /// <param name="store"></param>
        /// <param name="paltform"></param>
        /// <returns></returns>
        [HttpGet("security/getModulePermissionRecords")]
        [SwaggerOperation("getModulePermissionRecords")]
        //[ValidateActionParameters]
        //[AuthBaseFilter]
        public async Task<APIResult<IList<QueryModule>>> GetModulePermissionRecords(int? store, bool paltform = false)
        {
            if (!store.HasValue && store.Value == 0)
                return this.Error2<QueryModule>(Resources.ParameterError);

            return await Task.Run(() =>
            {
                try
                {
                    var result = _moduleService.GetModulePermissionRecords(paltform);

                    return this.Successful2(Resources.Successful, result);
                }
                catch (Exception ex)
                {
                    return this.Error2<QueryModule>(ex.Message);
                }

            });
        }


        /// <summary>
        /// 获取经销商APP功能项
        /// </summary>
        /// <param name="storeId"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        [HttpGet("getappfeatures")]
        [SwaggerOperation("GetAPPFeatures")]
        //[AuthBaseFilter]
        public async Task<APIResult<dynamic>> GetAPPFeatures(int? storeId, int userId)
        {
            if (!storeId.HasValue && storeId.Value == 0)
                return this.Error(Resources.ParameterError);

            return await Task.Run(() =>
            {
                try
                {
                    var reportsDatas = AppGlobalSettings.ParpaerReports();
                    var parpaerAPPs = AppGlobalSettings.ParpaerAPPs();
                    var subscribeDatas = AppGlobalSettings.ParpaerSubscribes();
                    var user = _userService.GetUserById(storeId, userId);
                    var acls = JsonConvert.DeserializeObject<List<int>>(user?.AppModuleAcl ?? "[]");
                    var subs = JsonConvert.DeserializeObject<List<int>>(user?.Subordinates ?? "[]");

                    var appDatas = parpaerAPPs;
                    if (acls != null && acls.Any())
                        appDatas = parpaerAPPs?.Where(s => acls.Contains(s.Id) || s.Id == 0).ToList();
                   
                    return this.Successful("获取成功", new
                    {
                        storeId,
                        userId,
                        reportsDatas,
                        appDatas,
                        subscribeDatas,
                        acls
                    });
                }
                catch (Exception ex)
                {
                    return this.Error(ex.Message);
                }

            });
        }

        /// <summary>
        /// 发送短信
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        [HttpPut("resetVerifiCode")]
        [SwaggerOperation("resetVerifiCode")]
        public async Task<APIResult<dynamic>> ResetVerifiCode(SMSParams param)
        {
            if (param.StoreId == 0)
                return this.Error(Resources.ParameterError);

            return await Task.Run(() =>
            {
                try
                {
                    var dispatchItem = _dispatchBillService.GetDispatchItemsById(param.StoreId, param.Id);

                    if (dispatchItem != null)
                    {
                        dispatchItem.VerificationCode = CommonHelper.GenerateNumber(6); //6位随机验证码
                        _dispatchBillService.UpdateDispatchItem(dispatchItem); //重置

                        var mobile = _terminalService.GetTerminalById(param.StoreId, dispatchItem.TerminalId)?.BossCall;
                        var messageContent = "【经销商管家】验证码：" + dispatchItem.VerificationCode + ",该验证码仅用于送货签收，请勿泄露给任何人.";
                        _dispatchBillService.SendMessage(mobile, messageContent); //发送
                    }

                    return this.Successful(Resources.Successful);
                }
                catch (Exception ex)
                {
                    return this.Error(ex.Message);
                }
            });
        }

    }
}

