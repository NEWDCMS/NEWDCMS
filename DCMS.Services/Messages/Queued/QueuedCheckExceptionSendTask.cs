using DCMS.Core;
using DCMS.Core.Domain.Tasks;
using DCMS.Services.Logging;
using DCMS.Services.Messages;
using System;

namespace DCMS.Services.Tasks
{
    /// <summary>
    /// 签到异常发送队列  DCMS.Services.Tasks.QueuedCheckExceptionSendTask,DCMS.Services
    /// </summary>
    public partial class QueuedCheckExceptionSendTask : IScheduleTask
    {
        private readonly IQueuedMessageService _queuedMessageService;
        private readonly IMessageSender _messageSender;
        private readonly ILogger _logger;

        public QueuedCheckExceptionSendTask(IQueuedMessageService queuedMessageService,
            IMessageSender messageSender,
            ILogger logger)
        {
            _queuedMessageService = queuedMessageService;
            _messageSender = messageSender;
            _logger = logger;
        }

        /// <summary>
        /// 执行任务
        /// </summary>
        public void Execute()
        {
            var queuedMessages = _queuedMessageService.SearchMessages(null, (int)MTypeEnum.CheckException, null, DCMSMessageDefaults.MaxTries, null,
               true, false);
            foreach (var queuedMessage in queuedMessages)
            {
                bool result = false;
                try
                {
                    var message = queuedMessage.ToStructure();
                    result = _messageSender.SendMessageOrNotity(message);
                    queuedMessage.SentOnUtc = DateTime.Now;
                }
                catch (Exception exc)
                {
                    result = false;
                    _logger.Error(string.Format("Error sending message. {0}", exc.Message), exc);
                }
                finally
                {
                    if (!result)
                    {
                        queuedMessage.SentTries += 1;
                    }
                    else
                    {
                        queuedMessage.IsRead = true;
                    }

                    _queuedMessageService.UpdateQueuedMessage(queuedMessage);
                }
            }

        }


    }
}
