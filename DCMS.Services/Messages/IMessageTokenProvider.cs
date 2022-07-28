using System.Collections.Generic;
using DCMS.Core.Domain.Products;
using DCMS.Core.Domain.Users;
using DCMS.Core.Domain.Messages;
using DCMS.Core.Domain.News;
using DCMS.Core.Domain.Stores;
using DCMS.Core.Domain.Vendors;

namespace DCMS.Services.Messages
{
    /// <summary>
    /// Message token provider
    /// </summary>
    public partial interface IMessageTokenProvider
    {

        void AddStoreTokens(IList<Token> tokens, Store store, EmailAccount emailAccount);

        void AddUserTokens(IList<Token> tokens, User user);

        void AddNewsLetterSubscriptionTokens(IList<Token> tokens, NewsLetterSubscription subscription);

        IEnumerable<string> GetListOfCampaignAllowedTokens();

        IEnumerable<string> GetListOfAllowedTokens(IEnumerable<string> tokenGroups = null);

        IEnumerable<string> GetTokenGroups(MessageTemplate messageTemplate);
    }
}