using DCMS.Api.Infrastructure.Mapper.Extensions;
using DCMS.Core;
using DCMS.Services.Tasks;
using DCMS.ViewModel.Models.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Swashbuckle.AspNetCore.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Task = System.Threading.Tasks.Task;
using DCMS.Core.Domain.Tasks;


namespace DCMS.Api.Controllers
{
    /// <summary>
    /// 用于SKD服务
    /// </summary>
    [Authorize]
    [Route("api/v{version:apiVersion}/dcms/skd")]
    public class QueuedMessageController : BaseAPIController
    {
        private readonly IQueuedMessageService  _queuedMessageService;
        public QueuedMessageController(
            IQueuedMessageService queuedMessageService, ILogger<BaseAPIController> logger) : base(logger)
        {
            _queuedMessageService = queuedMessageService;
        }

        /// <summary>
        /// 获取消息
        /// </summary>
        /// <param name="storeId"></param>
        /// <param name="mTypeId"></param>
        /// <param name="sentStatus"></param>
        /// <param name="orderByCreatedOnUtc"></param>
        /// <param name="maxSendTries"></param>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        [HttpGet("getQueuedMessages/{storeId}")]
        [SwaggerOperation("GetQueuedMessages")]
        public async Task<APIResult<IPagedList<QueuedMessage>>> GetQueuedMessages(int? storeId, [FromQuery] int[] mTypeId, string toUser, bool? sentStatus, bool? orderByCreatedOnUtc, int? maxSendTries, DateTime? startTime = null, DateTime? endTime = null, int pageIndex = 0, int pageSize = int.MaxValue)
        {
            return await Task.Run(() =>
            {
                try
                {
                    if (pageIndex > 1) pageIndex -= 1;
                    var qms = _queuedMessageService.SearchMessages(storeId, mTypeId, toUser, sentStatus, orderByCreatedOnUtc, maxSendTries, startTime, endTime, pageIndex, pageSize);
                    return this.Successful8("", qms);
                }
                catch (Exception )
                {
                    return new APIResult<IPagedList<QueuedMessage>>();
                }
            });
        }

    }
}
