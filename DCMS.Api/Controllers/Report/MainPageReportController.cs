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
using Microsoft.AspNetCore.Authorization;

namespace DCMS.Api.Controllers
{
    /// <summary>
    /// 
    /// </summary>
   [Authorize]
    [Route("api/v{version:apiVersion}/dcms/reporting")]
    public class MainPageReportController : BaseAPIController
    {
        private readonly IMainPageReportService _mainPageReportService;


        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="mainPageReportService"></param>
        public MainPageReportController(
            IMainPageReportService mainPageReportService
           , ILogger<BaseAPIController> logger) : base(logger)
        {
            _mainPageReportService = mainPageReportService;
        }



        /// <summary>
        /// 仪表盘
        /// </summary>
        /// <param name="store">经销商</param>
        /// <param name="businessUserIds">业务员</param>
        /// <returns></returns>
        [HttpGet("dashboard/getDashboardReport/{store}")]
        [SwaggerOperation("getDashboardReport")]
        //[ValidateActionParameters]
        //[AuthBaseFilter]
        public async Task<APIResult<DashboardReport>> GetDashboardReport(int? store, [FromQuery] int[] businessUserIds)
        {
            if (!store.HasValue || store.Value == 0)
                return this.Error3<DashboardReport>(Resources.ParameterError);

            return await Task.Run(() =>
            {
                var model = new DashboardReport();
                try
                {
                    model = _mainPageReportService.GetDashboardReport(store, businessUserIds);
                    return this.Successful3("", model);
                }
                catch (Exception ex)
                {
                    return this.Error3<DashboardReport>(ex.Message);
                }

            });
        }


        /// <summary>
        /// 当月销量
        /// </summary>
        /// <param name="store">经销商</param>
        /// <param name="startTime">开始时间</param>
        /// <param name="endTime">结束时间</param>
        /// <param name="brandIds">品牌</param>
        /// <param name="businessUserIds">业务员</param>
        /// <returns></returns>
        [HttpGet("dashboard/getMonthSaleReport/{store}")]
        [SwaggerOperation("getMonthSaleReport")]
        //[ValidateActionParameters]
        //[AuthBaseFilter]
        public async Task<APIResult<IList<MonthSaleReport>>> GetMonthSaleReport(int? store,
            DateTime? startTime,
            DateTime? endTime,
            [FromQuery] int[] brandIds,
            [FromQuery] int[] businessUserIds)
        {
            if (!store.HasValue || store.Value == 0)
                return this.Error2<MonthSaleReport>(Resources.ParameterError);

            return await Task.Run(() =>
            {
                try
                {
                    if (startTime != null)
                        startTime = DateTime.Parse(((DateTime)startTime).ToString("yyyy-MM-dd 00:00:00"));

                    if (endTime != null)
                        endTime = DateTime.Parse(((DateTime)endTime).ToString("yyyy-MM-dd 23:59:59"));

                    var result = _mainPageReportService.GetMonthSaleReport(store, startTime, endTime, brandIds, businessUserIds);

                    return this.Successful2("", result);
                }
                catch (Exception ex)
                {
                    return this.Error2<MonthSaleReport>(ex.Message);
                }

            });
        }


    }
}
