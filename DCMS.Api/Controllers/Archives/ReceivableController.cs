using DCMS.Core;
using DCMS.Core.Caching;
using DCMS.Services.Finances;
using DCMS.Services.Logging;
using DCMS.Services.Terminals;
using DCMS.Services.Users;
using DCMS.ViewModel.Models.Terminals;
using DCMS.Web.Framework.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Swashbuckle.AspNetCore.Annotations;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace DCMS.Api.Controllers
{
    /// <summary>
    ///  应收款
    /// </summary>
   [Authorize]
    [Route("api/v{version:apiVersion}/dcms/archives")]
    public class ReceivableController : BaseAPIController
    {
        private readonly IReceivableService _receivableService;
        private readonly ITerminalService _terminalService;
        private readonly IUserService _userService;
        
        private readonly IUserActivityService _userActivityService;

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="receivableService"></param>
        /// <param name="userService"></param>
        /// <param name="terminalService"></param>
        /// <param name="cacheManager"></param>
        public ReceivableController(
            IReceivableService receivableService,
            IUserService userService,
            ITerminalService terminalService,
           

        IUserActivityService userActivityService, ILogger<BaseAPIController> logger) : base(logger)
        {
            _receivableService = receivableService;
            _userService = userService;
            _terminalService = terminalService;
            

            _userActivityService = userActivityService;
        }

        /// <summary>
        /// 获取全部应收款
        /// </summary>
        /// <param name="store"></param>
        /// <param name="pagenumber"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        [HttpGet("receivable/GetAllReceivables/{store}")]
        [SwaggerOperation("GetAllReceivables")]
        //[ValidateActionParameters]
        //[AuthBaseFilter]
        public async Task<APIResult<IList<ReceivableSummeriesModel>>> GetAllReceivables(int? store, int pagenumber = 0, int pageSize = int.MaxValue)
        {
            if (!store.HasValue || store.Value == 0)
                return this.Error2<ReceivableSummeriesModel>(Resources.ParameterError);

            return await Task.Run(() =>
            {
                try
                {
                    if (pagenumber > 0)
                    {
                        pagenumber -= 1;
                    }

                    var receivables = new List<ReceivableSummeriesModel>();

                    //待补充。。。

                    return this.Successful2("", receivables);
                }
                catch (Exception ex)
                {
                    return this.Error<ReceivableSummeriesModel>(ex.Message);
                }

            });
        }

    }
}