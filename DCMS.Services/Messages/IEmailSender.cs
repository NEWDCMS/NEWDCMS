using DCMS.Core.Domain.Messages;
using System.Collections.Generic;
using System.Net.Mail;

namespace DCMS.Services.Tasks
{
    public partial interface IEmailSender
    {

        void SendEmail(EmailAccount emailAccount, string subject, string body,
            string fromAddress, string fromName, string toAddress, string toName,
            IEnumerable<string> bcc = null, IEnumerable<string> cc = null);

        void SendEmail(EmailAccount emailAccount, string subject, string body,
       MailAddress from, MailAddress to,
       IEnumerable<string> bcc = null, IEnumerable<string> cc = null);
    }
}
