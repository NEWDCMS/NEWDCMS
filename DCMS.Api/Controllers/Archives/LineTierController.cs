using DCMS.Api.Infrastructure.Mapper.Extensions;
using DCMS.Core;
using DCMS.Services.Terminals;
using DCMS.Services.Visit;
using DCMS.ViewModel.Models.Terminals;
using DCMS.ViewModel.Models.Visit;
using DCMS.Web.Framework.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Swashbuckle.AspNetCore.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using DCMS.Services.Configuration;
using DCMS.Core.Domain.Configuration;
using DCMS.Services.Users;

namespace DCMS.Api.Controllers.Archives
{

    /// <summary>
    /// 拜访线路设置
    /// </summary>
   [Authorize]
    [Route("api/v{version:apiVersion}/dcms/archives")]
    public class LineTierController : BaseAPIController
    {
        private readonly ILineTierService _lineTierService;
        private readonly ITerminalService _terminalService;
        private readonly ISettingService _settingService;
        private readonly IUserService _userService;


        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="lineTierService"></param>
        /// <param name="terminalService"></param>
        public LineTierController(
            ILineTierService lineTierService,
            ITerminalService terminalService,
            ISettingService settingService,
            IUserService userService,
            ILogger<BaseAPIController> logger) : base(logger)
        {
            _lineTierService = lineTierService;
            _terminalService = terminalService;
            _settingService = settingService;
            _userService = userService;
        }


        /// <summary>
        ///  获取经销商终端拜访线路
        /// </summary>
        /// <param name="store"></param>
        /// <returns></returns>
        [HttpGet("linetier/getLineTiers/{store}")]
        [SwaggerOperation("getLineTiers")]
        //[ValidateActionParameters]
        //[AuthBaseFilter]
        public async Task<APIResult<IList<LineTierModel>>> GetLineTiers(int? store, int? lineTierId, int? userId)
        {
            if (!store.HasValue || store.Value == 0)
                return this.Error2<LineTierModel>(Resources.ParameterError);

            return await Task.Run(() =>
            {
                try
                {
                    var companySetting = _settingService.LoadSetting<CompanySetting>(store ?? 0);
                    //如果未启用业务员线路或者用户为超级管理员
                    if (!companySetting.EnableBusinessVisitLine || _userService.IsAdmin(store ?? 0, userId ??0))
                        userId = 0; //userId为0则获取全部线路
                    var result = _lineTierService.GetLineTiers(store ?? 0,userId, 0).Select(l => l.ToModel<LineTierModel>()).ToList();
                    return this.Successful<LineTierModel>("", result);
                }
                catch (Exception ex)
                {
                    return this.Error2<LineTierModel>(ex.Message);
                }
            });
        }




        /// <summary>
        ///  获取经销商业务员终端拜访支配线路
        /// </summary>
        /// <param name="store"></param>
        /// <param name="userid"></param>
        /// <returns></returns>
        [HttpGet("linetier/getLineTiersByUser/{store}/{userid}")]
        [SwaggerOperation("getLineTiersByUser")]
        //[ValidateActionParameters]
        //[AuthBaseFilter]
        public async Task<APIResult<IList<LineTierModel>>> GetLineTiersByUser(int? store, int? userid)
        {
            if (!store.HasValue || store.Value == 0)
                return this.Error2<LineTierModel>(Resources.ParameterError);

            return await Task.Run(() =>
            {
                try
                {
                    var lines = new List<LineTierModel>();
                    var userLines = _lineTierService.GetUserLineTierAssigns(userid ?? 0);
                    if (userLines != null)
                    {
                        lines = userLines.Select(l =>
                        {
                            var line = l.LineTier?.ToModel<LineTierModel>();
                            var tids = l.LineTier.LineTierOptions.Select(o => o.TerminalId).ToArray();
                            line.Terminals = _terminalService.GetTerminalsByIds(store, tids, false)
                            .Select(t => t.ToModel<TerminalModel>())
                            .ToList();
                            return line;
                        }).ToList();
                    }
                    return this.Successful("", lines);
                }
                catch (Exception ex)
                {
                    return this.Error<LineTierModel>(ex.Message);
                }
            });
        }
    }
}