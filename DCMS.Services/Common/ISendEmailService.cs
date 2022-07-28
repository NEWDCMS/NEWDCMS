using DCMS.Core.Domain.Users;
using DCMS.Services.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DCMS.Services.Global.Common
{
    public interface ISendEmailService
    {
        SendResult SendInactiveMail(EmailSettings _emailSettings, User user, /*System.Web.HttpRequestBase Request,*/ string token, string timeStamp);

        void SendEmail(int store, string title, string content, string toName);
    }
}
