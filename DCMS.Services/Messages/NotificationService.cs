using DCMS.Core;
using DCMS.Services.Logging;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace DCMS.Services.Messages
{
    /// <summary>
    /// 通知服务
    /// </summary>
    public partial class NotificationService : INotificationService
    {

        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger _logger;
        private readonly ITempDataDictionaryFactory _tempDataDictionaryFactory;
        private readonly IWorkContext _workContext;


        public NotificationService(IHttpContextAccessor httpContextAccessor,
            ILogger logger,
            ITempDataDictionaryFactory tempDataDictionaryFactory,
            IWorkContext workContext)
        {
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
            _tempDataDictionaryFactory = tempDataDictionaryFactory;
            _workContext = workContext;
        }



        /// <summary>
        /// 保存消息到TempData
        /// </summary>
        /// <param name="type">Notification type</param>
        /// <param name="message">Message</param>
        /// <param name="encode">A value indicating whether the message should not be encoded</param>
        protected virtual void PrepareTempData(NotifyType type, string message, bool encode = true)
        {
            var context = _httpContextAccessor.HttpContext;
            var tempData = _tempDataDictionaryFactory.GetTempData(context);

            //Messages have stored in a serialized list
            var messages = tempData.ContainsKey(DCMSMessageDefaults.NotificationListKey)
                ? JsonConvert.DeserializeObject<IList<NotifyData>>(tempData[DCMSMessageDefaults.NotificationListKey].ToString())
                : new List<NotifyData>();

            messages.Add(new NotifyData
            {
                Message = message,
                Type = type,
                Encode = encode
            });

            tempData[DCMSMessageDefaults.NotificationListKey] = JsonConvert.SerializeObject(messages);
        }

        /// <summary>
        /// 记录异常日志
        /// </summary>
        /// <param name="exception">Exception</param>
        protected virtual void LogException(Exception exception)
        {
            if (exception == null)
            {
                return;
            }

            var user = _workContext.CurrentUser;
            _logger.Error(exception.Message, exception, user);
        }



        /// <summary>
        /// 显示通知
        /// </summary>
        /// <param name="type">Notification type</param>
        /// <param name="message">Message</param>
        /// <param name="encode">A value indicating whether the message should not be encoded</param>
        public virtual void Notification(NotifyType type, string message, bool encode = true)
        {
            PrepareTempData(type, message, encode);
        }

        /// <summary>
        /// 显示成功通知
        /// </summary>
        /// <param name="message">Message</param>
        /// <param name="encode">A value indicating whether the message should not be encoded</param>
        public virtual void SuccessNotification(string message, bool encode = true)
        {
            PrepareTempData(NotifyType.Success, message, encode);
        }

        /// <summary>
        /// 显示警告通知
        /// </summary>
        /// <param name="message">Message</param>
        /// <param name="encode">A value indicating whether the message should not be encoded</param>
        public virtual void WarningNotification(string message, bool encode = true)
        {
            PrepareTempData(NotifyType.Warning, message, encode);
        }

        /// <summary>
        /// 显示错误通知
        /// </summary>
        /// <param name="message">Message</param>
        /// <param name="encode">A value indicating whether the message should not be encoded</param>
        public virtual void ErrorNotification(string message, bool encode = true)
        {
            PrepareTempData(NotifyType.Error, message, encode);
        }

        /// <summary>
        /// 显示错误通知
        /// </summary>
        /// <param name="exception">Exception</param>
        /// <param name="logException">A value indicating whether exception should be logged</param>
        public virtual void ErrorNotification(Exception exception, bool logException = true)
        {
            if (exception == null)
            {
                return;
            }

            if (logException)
            {
                LogException(exception);
            }

            ErrorNotification(exception.Message);
        }


    }
}
