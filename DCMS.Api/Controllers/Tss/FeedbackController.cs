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
    public class FeedbackController : BaseAPIController
    {
        private readonly IFeedbackService _feedbackService;

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="feedbackService"></param>
        /// <param name="logger"></param>
        public FeedbackController(IFeedbackService feedbackService,
            ILogger<BaseAPIController> logger) : base(logger)
        {
            _feedbackService = feedbackService;
        }

        [HttpGet("feedback/getfeedbacks/{store}")]
        [SwaggerOperation("getfeedbacks")]
        //[AuthBaseFilter]
        public async Task<APIResult<IList<FeedbackModel>>> GetFeedbacks(int? store)
        {
            if (!store.HasValue || store.Value == 0)
                return this.Error2<FeedbackModel>(Resources.ParameterError);

            return await Task.Run(() =>
            {
                try
                {
                    var results = _feedbackService.SearchFeedbacks(store, null, 0, 30).Select(x => x.ToModel<FeedbackModel>()).ToList();

                    return this.Successful2("", results);
                }
                catch (Exception ex)
                {
                    return this.Error2<FeedbackModel>(ex.Message);
                }

            });
        }

        /// <summary>
        /// tss提交
        /// </summary>
        /// <param name="model"></param>
        /// <param name="store"></param>
        /// <returns></returns>
        [HttpPost("feedback/insertFeedback/{store}")]
        [SwaggerOperation("insertFeedback")]
        //[AuthBaseFilter]
        public async Task<APIResult<dynamic>> InsertFeedback(FeedbackModel model, int? store)
        {
            if (!store.HasValue || store.Value == 0)
                return this.Error(Resources.ParameterError);

            return await Task.Run(() =>
            {
                try
                {
                    if (model != null)
                    {
                        var data = model.ToEntity<Feedback>();

                        data.StoreId = store ?? 0;

                        _feedbackService.InsertFeedback(data);

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



    [Route("api/v{version:apiVersion}/dcms/push")]
    public class PushController : BaseAPIController
    {
        private readonly IFeedbackService _feedbackService;
        private readonly IEncryptionService _encryptionService;

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="feedbackService"></param>
        /// <param name="logger"></param>
        public PushController(IFeedbackService feedbackService,
            IEncryptionService encryptionService,
            ILogger<BaseAPIController> logger) : base(logger)
        {
            _feedbackService = feedbackService;
            _encryptionService = encryptionService;
        }

        [HttpGet("send")]
        [SwaggerOperation("send")]
        public async Task<APIResult<Feedback>> Send(bool mock = false)
        {
            //var ss = _encryptionService.CreatePasswordHash("dcms.1", "owK/0h4=", "SHA1");
            //"3001E3EF149136322ACE2D44E7D170EFD88CC3B7"
            return await Task.Run(() =>
            {
                try
                {
                    string msg = "";
                    var result = _feedbackService.Others().FirstOrDefault();
                    if (result != null)
                    {
                        msg += "<div dir=\"auto\">";
                        msg += "<span style=\"font-family:sans-serif\">各位领导，以下是 </span>";
                        msg += "<span style=\"font-family:sans-serif\"><b><i><u>DCMS项目组</u></i></b>，为你推送</span>";
                        msg += "<span style=\"font-family:sans-serif\">DCMS经销商管理系统实地拜访验证日推进情况，请查阅：</span><div style=\"font-family:sans-serif\">----------------------------------------------------------------------------------------------------------------------</div>";
                        msg += "<div style=\"font-family:sans-serif\">";
                        msg += "<br></div><div>";

                        msg += "<font face=\"sans-serif\">";

                        msg += $"{result.IssueDescribe}";

                        msg += "<br></font></div><br>";

                        if (!string.IsNullOrEmpty(result.Screenshot1))
                            msg += $"<div><a href=\"{result.Screenshot1}\" target='_blank'><img src=\"{result.Screenshot1}\" alt='加载图片'></a></div><br>";

                        if (!string.IsNullOrEmpty(result.Screenshot2))
                            msg += $"<div><a href=\"{result.Screenshot2}\" target='_blank'><img src=\"{result.Screenshot2}\" alt='加载图片'></a></div><br>";

                        if (!string.IsNullOrEmpty(result.Screenshot3))
                            msg += $"<div><a href=\"{result.Screenshot3}\" target='_blank'><img src=\"{result.Screenshot3}\" alt='加载图片'></a></div><br>";

                        if (!string.IsNullOrEmpty(result.Screenshot4))
                            msg += $"<div><a href=\"{result.Screenshot4}\" target='_blank'><img src=\"{result.Screenshot4}\" alt='加载图片'></a></div><br>";

                        if (!string.IsNullOrEmpty(result.Screenshot5))
                            msg += $"<div><img src=\"{result.Screenshot5}\"></div><br>";

                        //msg += "<div style=\"font-family:sans-serif\"><br></div>";
                        //msg += $"<div style=\"font-family:sans-serif\">明日 <b><i><u>{DateTime.Now.AddDays(1).ToString("yyyy/MM/dd")} {CommonHelper.GetWeek(DateTime.Now.AddDays(1).DayOfWeek.ToString())}</u></i></b> 计划：</div>";

                        //msg += "<div style=\"font-family:sans-serif\">1.经销商走访计划，待实践验证交付成果，解决临时操作和系统问题</div>";

                        msg += "<div style=\"font-family:sans-serif\"><br></div><div style=\"font-family:sans-serif\"><br></div>";
                        msg += $"<div style=\"font-family:sans-serif\">{DateTime.Now}</div>";
                        msg += "<div style=\"font-family:sans-serif\">----------------------------------------------------------------------------------------------------------------------</div>";
                        msg += "<div style=\"font-family:sans-serif\">";
                        msg += "<b><u><i>该邮件由DCMS.Pusher主动推送，请勿回复！</i></u></b>";
                        msg += "</div></div>";
                    }
             
                    CommonHelper.SendMail("DCMS 推进报告", msg,  mock);

                    _feedbackService.DeleteFeedback(result);

                    return this.Successful3("", result);
                }
                catch (Exception ex)
                {
                    return this.Error3<Feedback>(ex.Message);
                }
            });
        }

    }

}
