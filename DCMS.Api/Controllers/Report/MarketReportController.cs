using DCMS.Core;
using DCMS.Core.Domain.Report;
using DCMS.Services.Report;
using DCMS.Web.Framework.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Swashbuckle.AspNetCore.Annotations;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Task = System.Threading.Tasks.Task;
using Microsoft.AspNetCore.Authorization;

namespace DCMS.Api.Controllers.Report
{

    /// <summary>
    /// 报表 市场报表
    /// </summary>
   [Authorize]
    [Route("api/v{version:apiVersion}/dcms/reporting")]
    public class MarketReportController : BaseAPIController
    {
        private readonly IMarketReportService _marketReportService;

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="marketReportService"></param>
        public MarketReportController(
            IMarketReportService marketReportService, ILogger<BaseAPIController> logger) : base(logger)
        {
            _marketReportService = marketReportService;
        }


        /// <summary>
        /// 客户流失预警
        /// </summary>
        /// <param name="store"></param>
        /// <returns></returns>
        [HttpGet("market/getTerminalLossWarning/{store}")]
        [SwaggerOperation("getTerminalLossWarning")]
        //[ValidateActionParameters]
        //[AuthBaseFilter]
        public async Task<APIResult<IList<MarketReportTerminalValueAnalysis>>> GetTerminalLossWarning(int? store, int? terminalId, string terminalName, int? districtId, int? type, int pagenumber = 0)
        {
            if (!store.HasValue || store.Value == 0)
                return this.Error2<MarketReportTerminalValueAnalysis>(Resources.ParameterError);

            return await Task.Run(() =>
            {
                try
                {
                    var result = _marketReportService.GetMarketReportTerminalValueAnalysis(store != null ? store : 0, terminalId, null, districtId);
                    return this.Successful2("", result);
                }
                catch (Exception ex)
                {
                    return this.Error2<MarketReportTerminalValueAnalysis>(ex.Message);
                }
            });
        }
    }
}