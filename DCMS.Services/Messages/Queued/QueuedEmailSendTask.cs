using DCMS.Services.Logging;
using System;

namespace DCMS.Services.Tasks
{
    /// <summary>
    /// 邮件队列发送任务
    /// </summary>
    public partial class QueuedEmailSendTask : IScheduleTask
    {
        private readonly IQueuedEmailService _queuedEmailService;
        private readonly IEmailSender _emailSender;
        private readonly ILogger _logger;

        public QueuedEmailSendTask(IQueuedEmailService queuedEmailService,
            IEmailSender emailSender,
            ILogger logger)
        {
            _queuedEmailService = queuedEmailService;
            _emailSender = emailSender;
            _logger = logger;
        }

        /// <summary>
        /// 执行任务
        /// </summary>
        public void Execute()
        {
            var maxTries = 30;
            var queuedEmails = _queuedEmailService.SearchEmails(null, null, null, null,
                true, maxTries, false, 0, 500);
            foreach (var queuedEmail in queuedEmails)
            {
                var bcc = string.IsNullOrWhiteSpace(queuedEmail.Bcc)
                            ? null
                            : queuedEmail.Bcc.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                var cc = string.IsNullOrWhiteSpace(queuedEmail.CC)
                            ? null
                            : queuedEmail.CC.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);

                try
                {
                    _emailSender.SendEmail(queuedEmail.EmailAccount, queuedEmail.Subject, queuedEmail.Body,
                       queuedEmail.From, queuedEmail.FromName, queuedEmail.To, queuedEmail.ToName, bcc, cc);

                    queuedEmail.SentOnUtc = DateTime.Now;
                }
                catch (Exception exc)
                {
                    _logger.Error(string.Format("Error sending e-mail. {0}", exc.Message), exc);
                }
                finally
                {
                    queuedEmail.SentTries += 1;
                    _queuedEmailService.UpdateQueuedEmail(queuedEmail);
                }
            }
        }
    }
}
