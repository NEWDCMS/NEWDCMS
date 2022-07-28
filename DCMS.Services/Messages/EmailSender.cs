using DCMS.Core.Domain.Messages;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;

namespace DCMS.Services.Tasks
{
    public partial class EmailSender : IEmailSender
    {
        /// <summary>
        /// 发送邮件
        /// </summary>
        /// <param name="emailAccount"></param>
        /// <param name="subject"></param>
        /// <param name="body"></param>
        /// <param name="fromAddress"></param>
        /// <param name="fromName"></param>
        /// <param name="toAddress"></param>
        /// <param name="toName"></param>
        /// <param name="bcc"></param>
        /// <param name="cc"></param>
        public void SendEmail(EmailAccount emailAccount, string subject, string body,
            string fromAddress, string fromName, string toAddress, string toName,
            IEnumerable<string> bcc = null, IEnumerable<string> cc = null)
        {
            SendEmail(emailAccount, subject, body,
                new MailAddress(fromAddress, fromName), new MailAddress(toAddress, toName),
                bcc, cc);
        }

        /// <summary>
        /// 发送邮件
        /// </summary>
        /// <param name="emailAccount"></param>
        /// <param name="subject"></param>
        /// <param name="body"></param>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <param name="bcc"></param>
        /// <param name="cc"></param>
        public virtual void SendEmail(EmailAccount emailAccount, string subject, string body,
            MailAddress from, MailAddress to,
            IEnumerable<string> bcc = null, IEnumerable<string> cc = null)
        {
            var message = new MailMessage
            {
                From = from
            };
            message.To.Add(to);
            if (null != bcc)
            {
                foreach (var address in bcc.Where(bccValue => !string.IsNullOrWhiteSpace(bccValue)))
                {
                    message.Bcc.Add(address.Trim());
                }
            }
            if (null != cc)
            {
                foreach (var address in cc.Where(ccValue => !string.IsNullOrWhiteSpace(ccValue)))
                {
                    message.CC.Add(address.Trim());
                }
            }
            message.Subject = subject;
            message.Body = body;
            message.IsBodyHtml = true;

            using (var smtpClient = new SmtpClient())
            {
                smtpClient.UseDefaultCredentials = emailAccount.UseDefaultCredentials;
                smtpClient.Host = emailAccount.Host;
                smtpClient.Port = emailAccount.Port;
                smtpClient.EnableSsl = emailAccount.EnableSsl;
                if (emailAccount.UseDefaultCredentials)
                {
                    smtpClient.Credentials = CredentialCache.DefaultNetworkCredentials;
                }
                else
                {
                    smtpClient.Credentials = new NetworkCredential(emailAccount.Username, emailAccount.Password);
                }

                smtpClient.Send(message);
            }
        }
    }
}
