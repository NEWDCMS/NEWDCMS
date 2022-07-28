using DCMS.Core.Configuration;
using DCMS.Core.Domain.Messages;
using DCMS.Core.Domain.Users;
using DCMS.Services.Configuration;
using DCMS.Services.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace DCMS.Services.Global.Common
{
    public class SendEmailService:ISendEmailService
    {
        private readonly IQueuedEmailService _queuedEmailService;
        private readonly IEmailAccountService _emailAccountService;

        public SendEmailService(IQueuedEmailService queuedEmailService,
            IEmailAccountService emailAccountService)
        {
            _queuedEmailService = queuedEmailService;
            _emailAccountService = emailAccountService;
        }

        public SendResult SendMail(EmailSettings setting, string subject, string content)
        {
            var result = new SendResult();
            var toEmail = setting.To;
            var formEmail = setting.Form;
            var attachmentPath = "";
            try
            {
                if (!string.IsNullOrEmpty(subject) && !string.IsNullOrEmpty(content))
                {
                    var email = new MailMessage();
                    email.From = new MailAddress(formEmail, "Sportsarbs");
                    email.To.Add(new MailAddress(toEmail));
                    if (!string.IsNullOrEmpty(setting.ReplyTo))
                        email.ReplyToList.Add(new MailAddress(setting.ReplyTo));
                    email.Subject = subject;
                    email.Body = content;
                    if (!string.IsNullOrEmpty(attachmentPath))
                    {
                        var attachment = new Attachment(attachmentPath);
                        email.Attachments.Add(attachment);
                    }
                    email.IsBodyHtml = true;
                    email.Priority = MailPriority.Normal;
                    var stmp = new SmtpClient(setting.Smtp.Trim(), setting.Port);
                    stmp.UseDefaultCredentials = true;
                    stmp.Credentials = new NetworkCredential(setting.Account, setting.Password);
                    stmp.EnableSsl = setting.SSL;
                    stmp.DeliveryMethod = SmtpDeliveryMethod.Network;
                    stmp.Send(email);
                }
                result.Success = true;
            }
            catch (SmtpException ex)
            {
                result.Success = false;
                result.Message = ex.Message + "," + ex.InnerException.Message;
            }

            return result;
        }


        public SendResult SendInactiveMail(EmailSettings _emailSettings, User user, /*Url.Request Request,*/ string token, string timeStamp)
        {
            //var path = "http://manage.jsdcms.com/user/activation?token=";
            var path = "http://172.15.224.50:9991/user/activation?token=";
            var result = new SendResult();
            //string links = "http://" + (Request.Url.Host + (Request.Url.Port == 80 ? "" : ":" + Request.Url.Port.ToString()) + "").Trim() + "api/dcms/account/account-activation?token=" + token + "&t=" + timeStamp + "&email=" + user.Email;
            string links = path + token + "&t=" + timeStamp + "&email=" + user.Email;
            string content = "欢迎来到DSMS系统! <br/><br/>请在60秒内点击下面的链接. <br/><br/><a href=\"" + links + "\" target=\"_blank\">" + links + "</a><br/><br/>然后您可继续修改密码<br/><br/>祝您好运!,<br/>华润雪花啤酒";

            _emailSettings.Form = _emailSettings.Form;
            _emailSettings.To = user.Email;
            result = SendMail(_emailSettings, "修改密码", content);
            return result;
        }

        public void SendEmail(int store, string title, string content, string toName)
        {
            var fromUsers = _emailAccountService.GetAllEmailAccounts().Where(x => x.Username.Contains("1481935320")).FirstOrDefault();

            if (fromUsers != null)
            {
                QueuedEmail queuedMessage = new QueuedEmail()
                {
                    StoreId = store,
                    Priority = 0, //优先级
                    From = fromUsers.Email,
                    FromName = fromUsers.Username,
                    To = toName,
                    ToName = toName,
                    Subject = title,
                    Body = content,
                    EmailAccountId = fromUsers.Id,
                    CreatedOnUtc = DateTime.Now
                };
                _queuedEmailService.InsertQueuedEmail(queuedMessage);
            }
        }
    }
}
