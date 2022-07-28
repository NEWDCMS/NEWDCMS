using DCMS.Core;
using DCMS.Core.Domain.Messages;
using System.Collections.Generic;


namespace DCMS.Services.Messages
{
    public partial interface IEmailAccountService
    {

        void InsertEmailAccount(EmailAccount emailAccount);
        void UpdateEmailAccount(EmailAccount emailAccount);
        void DeleteEmailAccount(EmailAccount emailAccount);
        EmailAccount GetEmailAccountById(int emailAccountId);

        IList<EmailAccount> GetAllEmailAccounts();
        IList<EmailAccount> GetAllEmailAccounts(int? storeId);

        IPagedList<EmailAccount> SearchEmailAccounts(string username, int? storeId, int pageIndex = 0, int pageSize = int.MaxValue);
    }
}
