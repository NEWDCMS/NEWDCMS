using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DCMS.Api.Infrastructure.Mapper.Extensions;
using DCMS.Core;
using DCMS.Core.Domain.TSS;
using DCMS.Services.TSS;
using DCMS.ViewModel.Models.Tss;
using DCMS.Web.Framework.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Swashbuckle.AspNetCore.Annotations;
using Microsoft.AspNetCore.Authorization;
using DCMS.Services.Security;

namespace DCMS.Api.Controllers.Tss
{
    [Route("api/v{version:apiVersion}/dcms/tss")]
    [Authorize]
    public class MarketFeedbackController : BaseAPIController
    {
        private readonly IMarketFeedbackService _feedbackService;

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="feedbackService"></param>
        /// <param name="logger"></param>
        public MarketFeedbackController(IMarketFeedbackService feedbackService,
            ILogger<BaseAPIController> logger) : base(logger)
        {
            _feedbackService = feedbackService;
        }

        [HttpGet("feedback/getmarketfeedbacks/{store}")]
        [SwaggerOperation("getmarketfeedbacks")]
        //[AuthBaseFilter]
        public async Task<APIResult<IList<MarketFeedbackModel>>> GetMarketFeedbacks(int? store)
        {
            if (!store.HasValue || store.Value == 0)
                return this.Error2<MarketFeedbackModel>(Resources.ParameterError);

            return await Task.Run(() =>
            {
                try
                {
                    var results = _feedbackService.SearchMarketFeedbacks(store, null, 0, 30).Select(x => x.ToModel<MarketFeedbackModel>()).ToList();

                    return this.Successful2("", results);
                }
                catch (Exception ex)
                {
                    return this.Error2<MarketFeedbackModel>(ex.Message);
                }

            });
        }

        /// <summary>
        /// tss提交
        /// </summary>
        /// <param name="model"></param>
        /// <param name="store"></param>
        /// <returns></returns>
        [HttpPost("feedback/insertmarketfeedback/{store}")]
        [SwaggerOperation("insertMarketFeedback")]
        public async Task<APIResult<dynamic>> InsertMarketFeedback(MarketFeedbackModel model, int? store)
        {
            if (!store.HasValue || store.Value == 0)
                return this.Error(Resources.ParameterError);

            return await Task.Run(() =>
            {
                try
                {
                    if (model != null)
                    {
                        var data = model.ToEntity<MarketFeedback>();

                        data.StoreId = store ?? 0;

                        _feedbackService.InsertMarketFeedback(data);

                        return this.Successful("提交成功");
                    }
                    else
                    {
                        return this.Error("提交失败");
                    }
                }
                catch (Exception ex)
                {
                    return this.Error(ex.Message);
                }
            });
        }
    }

}
