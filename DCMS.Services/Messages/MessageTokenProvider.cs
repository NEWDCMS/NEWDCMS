using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using DCMS.Core;
using DCMS.Core.Domain;
using DCMS.Core.Domain.Products;
using DCMS.Core.Domain.Common;
using DCMS.Core.Domain.Users;
using DCMS.Core.Domain.Localization;
using DCMS.Core.Domain.Messages;
using DCMS.Core.Domain.News;
using DCMS.Core.Domain.Stores;
using DCMS.Core.Domain.Tax;
using DCMS.Core.Domain.Vendors;
using DCMS.Core.Html;
using DCMS.Core.Infrastructure;
using DCMS.Services.Common;
using DCMS.Services.Users;
using DCMS.Services.Events;
using DCMS.Services.Helpers;
using DCMS.Services.Localization;
using DCMS.Services.Seo;
using DCMS.Services.Stores;


namespace DCMS.Services.Messages
{
    /// <summary>
    /// Message token provider
    /// </summary>
    public partial class MessageTokenProvider : BaseService, IMessageTokenProvider
    {
        #region Fields


        private readonly IActionContextAccessor _actionContextAccessor;
        private readonly IUserService _userService;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly IEventPublisher _eventPublisher;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly IStoreService _storeService;

        private readonly IStoreContext _storeContext;
      
        private readonly IUrlHelperFactory _urlHelperFactory;
 
    
        private readonly IWorkContext _workContext;
        private readonly MessageTemplatesSettings _templatesSettings;
      
        private readonly StoreInformationSettings _storeInformationSettings;
        private readonly TaxSettings _taxSettings;

        private Dictionary<string, IEnumerable<string>> _allowedTokens;

        #endregion

        #region Ctor

        public MessageTokenProvider(
            IActionContextAccessor actionContextAccessor,
         IStoreService storeService,
            IUserService userService,
            IDateTimeHelper dateTimeHelper,
         
            IEventPublisher eventPublisher,
            IGenericAttributeService genericAttributeService,
        
            IStoreContext storeContext,
          
            IUrlHelperFactory urlHelperFactory,
            IUrlRecordService urlRecordService,
        
            IWorkContext workContext,
            MessageTemplatesSettings templatesSettings,
       
            StoreInformationSettings storeInformationSettings,
            TaxSettings taxSettings)
        {
      
            _actionContextAccessor = actionContextAccessor;
            _storeService = storeService;
                     _userService = userService;
            _dateTimeHelper = dateTimeHelper;
    
            _eventPublisher = eventPublisher;
            _genericAttributeService = genericAttributeService;
         
         
            _storeContext = storeContext;
        
            _urlHelperFactory = urlHelperFactory;
            _urlRecordService = urlRecordService;
         
            _workContext = workContext;
            _templatesSettings = templatesSettings;
          
            _storeInformationSettings = storeInformationSettings;
            _taxSettings = taxSettings;
        }

        #endregion

        #region Allowed tokens

        /// <summary>
        /// Get all available tokens by token groups
        /// </summary>
        protected Dictionary<string, IEnumerable<string>> AllowedTokens
        {
            get
            {
                if (_allowedTokens != null)
                    return _allowedTokens;

                _allowedTokens = new Dictionary<string, IEnumerable<string>>();

                //store tokens
                _allowedTokens.Add(TokenGroupNames.StoreTokens, new[]
                {
                    "%Store.Name%",
                    "%Store.URL%",
                    "%Store.Email%",
                    "%Store.CompanyName%",
                    "%Store.CompanyAddress%",
                    "%Store.CompanyPhoneNumber%",
                    "%Store.CompanyVat%",
                    "%Facebook.URL%",
                    "%Twitter.URL%",
                    "%YouTube.URL%"
                });

                //user tokens
                _allowedTokens.Add(TokenGroupNames.UserTokens, new[]
                {
                    "%User.Email%",
                    "%User.Username%",
                    "%User.FullName%",
                    "%User.FirstName%",
                    "%User.LastName%",
                    "%User.VatNumber%",
                    "%User.VatNumberStatus%",
                    "%User.CustomAttributes%",
                    "%User.PasswordRecoveryURL%",
                    "%User.AccountActivationURL%",
                    "%User.EmailRevalidationURL%",
                    "%Wishlist.URLForUser%"
                });

                //order tokens
                _allowedTokens.Add(TokenGroupNames.OrderTokens, new[]
                {
                    "%Order.OrderNumber%",
                    "%Order.UserFullName%",
                    "%Order.UserEmail%",
                    "%Order.BillingFirstName%",
                    "%Order.BillingLastName%",
                    "%Order.BillingPhoneNumber%",
                    "%Order.BillingEmail%",
                    "%Order.BillingFaxNumber%",
                    "%Order.BillingCompany%",
                    "%Order.BillingAddress1%",
                    "%Order.BillingAddress2%",
                    "%Order.BillingCity%",
                    "%Order.BillingCounty%",
                    "%Order.BillingStateProvince%",
                    "%Order.BillingZipPostalCode%",
                    "%Order.BillingCountry%",
                    "%Order.BillingCustomAttributes%",
                    "%Order.Shippable%",
                    "%Order.ShippingMethod%",
                    "%Order.ShippingFirstName%",
                    "%Order.ShippingLastName%",
                    "%Order.ShippingPhoneNumber%",
                    "%Order.ShippingEmail%",
                    "%Order.ShippingFaxNumber%",
                    "%Order.ShippingCompany%",
                    "%Order.ShippingAddress1%",
                    "%Order.ShippingAddress2%",
                    "%Order.ShippingCity%",
                    "%Order.ShippingCounty%",
                    "%Order.ShippingStateProvince%",
                    "%Order.ShippingZipPostalCode%",
                    "%Order.ShippingCountry%",
                    "%Order.ShippingCustomAttributes%",
                    "%Order.PaymentMethod%",
                    "%Order.VatNumber%",
                    "%Order.CustomValues%",
                    "%Order.Product(s)%",
                    "%Order.CreatedOn%",
                    "%Order.OrderURLForUser%",
                    "%Order.PickupInStore%",
                    "%Order.OrderId%"
                });

                //shipment tokens
                _allowedTokens.Add(TokenGroupNames.ShipmentTokens, new[]
                {
                    "%Shipment.ShipmentNumber%",
                    "%Shipment.TrackingNumber%",
                    "%Shipment.TrackingNumberURL%",
                    "%Shipment.Product(s)%",
                    "%Shipment.URLForUser%"
                });

                //refunded order tokens
                _allowedTokens.Add(TokenGroupNames.RefundedOrderTokens, new[]
                {
                    "%Order.AmountRefunded%"
                });

                //order note tokens
                _allowedTokens.Add(TokenGroupNames.OrderNoteTokens, new[]
                {
                    "%Order.NewNoteText%",
                    "%Order.OrderNoteAttachmentUrl%"
                });

                //recurring payment tokens
                _allowedTokens.Add(TokenGroupNames.RecurringPaymentTokens, new[]
                {
                    "%RecurringPayment.ID%",
                    "%RecurringPayment.CancelAfterFailedPayment%",
                    "%RecurringPayment.RecurringPaymentType%"
                });

                //newsletter subscription tokens
                _allowedTokens.Add(TokenGroupNames.SubscriptionTokens, new[]
                {
                    "%NewsLetterSubscription.Email%",
                    "%NewsLetterSubscription.ActivationUrl%",
                    "%NewsLetterSubscription.DeactivationUrl%"
                });

                //product tokens
                _allowedTokens.Add(TokenGroupNames.ProductTokens, new[]
                {
                    "%Product.ID%",
                    "%Product.Name%",
                    "%Product.ShortDescription%",
                    "%Product.ProductURLForUser%",
                    "%Product.SKU%",
                    "%Product.StockQuantity%"
                });

                //return request tokens
                _allowedTokens.Add(TokenGroupNames.ReturnRequestTokens, new[]
                {
                    "%ReturnRequest.CustomNumber%",
                    "%ReturnRequest.OrderId%",
                    "%ReturnRequest.Product.Quantity%",
                    "%ReturnRequest.Product.Name%",
                    "%ReturnRequest.Reason%",
                    "%ReturnRequest.RequestedAction%",
                    "%ReturnRequest.UserComment%",
                    "%ReturnRequest.StaffNotes%",
                    "%ReturnRequest.Status%"
                });

                //forum tokens
                _allowedTokens.Add(TokenGroupNames.ForumTokens, new[]
                {
                    "%Forums.ForumURL%",
                    "%Forums.ForumName%"
                });

                //forum topic tokens
                _allowedTokens.Add(TokenGroupNames.ForumTopicTokens, new[]
                {
                    "%Forums.TopicURL%",
                    "%Forums.TopicName%"
                });

                //forum post tokens
                _allowedTokens.Add(TokenGroupNames.ForumPostTokens, new[]
                {
                    "%Forums.PostAuthor%",
                    "%Forums.PostBody%"
                });

                //private message tokens
                _allowedTokens.Add(TokenGroupNames.PrivateMessageTokens, new[]
                {
                    "%PrivateMessage.Subject%",
                    "%PrivateMessage.Text%"
                });

                //vendor tokens
                _allowedTokens.Add(TokenGroupNames.VendorTokens, new[]
                {
                    "%Vendor.Name%",
                    "%Vendor.Email%",
                    "%Vendor.VendorAttributes%"
                });

                //gift card tokens
                _allowedTokens.Add(TokenGroupNames.GiftCardTokens, new[]
                {
                    "%GiftCard.SenderName%",
                    "%GiftCard.SenderEmail%",
                    "%GiftCard.RecipientName%",
                    "%GiftCard.RecipientEmail%",
                    "%GiftCard.Amount%",
                    "%GiftCard.CouponCode%",
                    "%GiftCard.Message%"
                });

                //product review tokens
                _allowedTokens.Add(TokenGroupNames.ProductReviewTokens, new[]
                {
                    "%ProductReview.ProductName%",
                    "%ProductReview.Title%",
                    "%ProductReview.IsApproved%",
                    "%ProductReview.ReviewText%",
                    "%ProductReview.ReplyText%"
                });

                //attribute combination tokens
                _allowedTokens.Add(TokenGroupNames.AttributeCombinationTokens, new[]
                {
                    "%AttributeCombination.Formatted%",
                    "%AttributeCombination.SKU%",
                    "%AttributeCombination.StockQuantity%"
                });

                //blog comment tokens
                _allowedTokens.Add(TokenGroupNames.BlogCommentTokens, new[]
                {
                    "%BlogComment.BlogPostTitle%"
                });

                //news comment tokens
                _allowedTokens.Add(TokenGroupNames.NewsCommentTokens, new[]
                {
                    "%NewsComment.NewsTitle%"
                });

                //product back in stock tokens
                _allowedTokens.Add(TokenGroupNames.ProductBackInStockTokens, new[]
                {
                    "%BackInStockSubscription.ProductName%",
                    "%BackInStockSubscription.ProductUrl%"
                });

                //email a friend tokens
                _allowedTokens.Add(TokenGroupNames.EmailAFriendTokens, new[]
                {
                    "%EmailAFriend.PersonalMessage%",
                    "%EmailAFriend.Email%"
                });

                //wishlist to friend tokens
                _allowedTokens.Add(TokenGroupNames.WishlistToFriendTokens, new[]
                {
                    "%Wishlist.PersonalMessage%",
                    "%Wishlist.Email%"
                });

                //VAT validation tokens
                _allowedTokens.Add(TokenGroupNames.VatValidation, new[]
                {
                    "%VatValidationResult.Name%",
                    "%VatValidationResult.Address%"
                });

                //contact us tokens
                _allowedTokens.Add(TokenGroupNames.ContactUs, new[]
                {
                    "%ContactUs.SenderEmail%",
                    "%ContactUs.SenderName%",
                    "%ContactUs.Body%"
                });

                //contact vendor tokens
                _allowedTokens.Add(TokenGroupNames.ContactVendor, new[]
                {
                    "%ContactUs.SenderEmail%",
                    "%ContactUs.SenderName%",
                    "%ContactUs.Body%"
                });

                return _allowedTokens;
            }
        }

        #endregion

        #region Utilities



        /// <summary>
        /// Generates an absolute URL for the specified store, routeName and route values
        /// </summary>
        /// <param name="storeId">Store identifier; Pass 0 to load URL of the current store</param>
        /// <param name="routeName">The name of the route that is used to generate URL</param>
        /// <param name="routeValues">An object that contains route values</param>
        /// <returns>Generated URL</returns>
        protected virtual string RouteUrl(int storeId = 0, string routeName = null, object routeValues = null)
        {
            //try to get a store by the passed identifier
            var store = _storeService.GetStoreById(storeId) ?? _storeContext.CurrentStore
                ?? throw new Exception("No store could be loaded");

            //ensure that the store URL is specified
            if (string.IsNullOrEmpty(store.Url))
                throw new Exception("URL cannot be null");

            //generate a URL with an absolute path
            var urlHelper = _urlHelperFactory.GetUrlHelper(_actionContextAccessor.ActionContext);
            var url = new PathString(urlHelper.RouteUrl(routeName, routeValues));

            //remove the application path from the generated URL if exists
            var pathBase = _actionContextAccessor.ActionContext?.HttpContext?.Request?.PathBase ?? PathString.Empty;
            url.StartsWithSegments(pathBase, out url);

            //compose the result
            return Uri.EscapeUriString(WebUtility.UrlDecode($"{store.Url.TrimEnd('/')}{url}"));
        }

        #endregion

        #region Methods

        /// <summary>
        /// Add store tokens
        /// </summary>
        /// <param name="tokens">List of already added tokens</param>
        /// <param name="store">Store</param>
        /// <param name="emailAccount">Email account</param>
        public virtual void AddStoreTokens(IList<Token> tokens, Store store, EmailAccount emailAccount)
        {
            if (emailAccount == null)
                throw new ArgumentNullException(nameof(emailAccount));

            tokens.Add(new Token("Store.Name", store.Name));
            tokens.Add(new Token("Store.URL", store.Url, true));
            tokens.Add(new Token("Store.Email", emailAccount.Email));
            tokens.Add(new Token("Store.CompanyName", store.Name));
            //tokens.Add(new Token("Store.CompanyAddress", store.CompanyAddress));
            //tokens.Add(new Token("Store.CompanyPhoneNumber", store.CompanyPhoneNumber));
            //tokens.Add(new Token("Store.CompanyVat", store.CompanyVat));


            //event notification
            _eventPublisher.EntityTokensAdded(store, tokens);
        }

        /// <summary>
   
        /// <summary>
        /// Add user tokens
        /// </summary>
        /// <param name="tokens">List of already added tokens</param>
        /// <param name="user">User</param>
        public virtual void AddUserTokens(IList<Token> tokens, User user)
        {
            tokens.Add(new Token("User.Email", user.Email));
            tokens.Add(new Token("User.Username", user.Username));
            tokens.Add(new Token("User.FullName", _userService.GetUserName(user.Id)));
            tokens.Add(new Token("User.VatNumber", _genericAttributeService.GetAttribute<string>(user, DCMSUserDefaults.VatNumberAttribute)));
            tokens.Add(new Token("User.VatNumberStatus", ((VatNumberStatus)_genericAttributeService.GetAttribute<int>(user, DCMSUserDefaults.VatNumberStatusIdAttribute)).ToString()));

            var customAttributesXml = _genericAttributeService.GetAttribute<string>(user, DCMSUserDefaults.CustomUserAttributes);
            //tokens.Add(new Token("User.CustomAttributes", _userAttributeFormatter.FormatAttributes(customAttributesXml), true));

            //note: we do not use SEO friendly URLS for these links because we can get errors caused by having .(dot) in the URL (from the email address)
            var passwordRecoveryUrl = RouteUrl(routeName: "PasswordRecoveryConfirm", routeValues: new { token = _genericAttributeService.GetAttribute<string>(user, DCMSUserDefaults.PasswordRecoveryTokenAttribute), email = user.Email });
            var accountActivationUrl = RouteUrl(routeName: "AccountActivation", routeValues: new { token = _genericAttributeService.GetAttribute<string>(user, DCMSUserDefaults.AccountActivationTokenAttribute), email = user.Email });
            var emailRevalidationUrl = RouteUrl(routeName: "EmailRevalidation", routeValues: new { token = _genericAttributeService.GetAttribute<string>(user, DCMSUserDefaults.EmailRevalidationTokenAttribute), email = user.Email });
            var wishlistUrl = RouteUrl(routeName: "Wishlist", routeValues: new { userGuid = user.UserGuid });
            tokens.Add(new Token("User.PasswordRecoveryURL", passwordRecoveryUrl, true));
            tokens.Add(new Token("User.AccountActivationURL", accountActivationUrl, true));
            tokens.Add(new Token("User.EmailRevalidationURL", emailRevalidationUrl, true));
            tokens.Add(new Token("Wishlist.URLForUser", wishlistUrl, true));

            //event notification
            _eventPublisher.EntityTokensAdded(user, tokens);
        }


        /// <summary>
        /// Add newsletter subscription tokens
        /// </summary>
        /// <param name="tokens">List of already added tokens</param>
        /// <param name="subscription">Newsletter subscription</param>
        public virtual void AddNewsLetterSubscriptionTokens(IList<Token> tokens, NewsLetterSubscription subscription)
        {
            tokens.Add(new Token("NewsLetterSubscription.Email", subscription.Email));

            var activationUrl = RouteUrl(routeName: "NewsletterActivation", routeValues: new { token = subscription.NewsLetterSubscriptionGuid, active = "true" });
            tokens.Add(new Token("NewsLetterSubscription.ActivationUrl", activationUrl, true));

            var deactivationUrl = RouteUrl(routeName: "NewsletterActivation", routeValues: new { token = subscription.NewsLetterSubscriptionGuid, active = "false" });
            tokens.Add(new Token("NewsLetterSubscription.DeactivationUrl", deactivationUrl, true));

            //event notification
            _eventPublisher.EntityTokensAdded(subscription, tokens);
        }



        /// <summary>
        /// Get collection of allowed (supported) message tokens for campaigns
        /// </summary>
        /// <returns>Collection of allowed (supported) message tokens for campaigns</returns>
        public virtual IEnumerable<string> GetListOfCampaignAllowedTokens()
        {
            var additionalTokens = new CampaignAdditionalTokensAddedEvent();
            _eventPublisher.Publish(additionalTokens);

            var allowedTokens = GetListOfAllowedTokens(new[] { TokenGroupNames.StoreTokens, TokenGroupNames.SubscriptionTokens }).ToList();
            allowedTokens.AddRange(additionalTokens.AdditionalTokens);

            return allowedTokens.Distinct();
        }

        /// <summary>
        /// Get collection of allowed (supported) message tokens
        /// </summary>
        /// <param name="tokenGroups">Collection of token groups; pass null to get all available tokens</param>
        /// <returns>Collection of allowed message tokens</returns>
        public virtual IEnumerable<string> GetListOfAllowedTokens(IEnumerable<string> tokenGroups = null)
        {
            var additionalTokens = new AdditionalTokensAddedEvent();
            _eventPublisher.Publish(additionalTokens);

            var allowedTokens = AllowedTokens.Where(x => tokenGroups == null || tokenGroups.Contains(x.Key))
                .SelectMany(x => x.Value).ToList();

            allowedTokens.AddRange(additionalTokens.AdditionalTokens);

            return allowedTokens.Distinct();
        }

        /// <summary>
        /// Get token groups of message template
        /// </summary>
        /// <param name="messageTemplate">Message template</param>
        /// <returns>Collection of token group names</returns>
        public virtual IEnumerable<string> GetTokenGroups(MessageTemplate messageTemplate)
        {
            //groups depend on which tokens are added at the appropriate methods in IWorkflowMessageService
            switch (messageTemplate.Name)
            {
                case MessageTemplateSystemNames.UserRegisteredNotification:
                case MessageTemplateSystemNames.UserWelcomeMessage:
                case MessageTemplateSystemNames.UserEmailValidationMessage:
                case MessageTemplateSystemNames.UserEmailRevalidationMessage:
                case MessageTemplateSystemNames.UserPasswordRecoveryMessage:
                    return new[] { TokenGroupNames.StoreTokens, TokenGroupNames.UserTokens };

                case MessageTemplateSystemNames.OrderPlacedVendorNotification:
                case MessageTemplateSystemNames.OrderPlacedStoreOwnerNotification:
                case MessageTemplateSystemNames.OrderPlacedAffiliateNotification:
                case MessageTemplateSystemNames.OrderPaidStoreOwnerNotification:
                case MessageTemplateSystemNames.OrderPaidUserNotification:
                case MessageTemplateSystemNames.OrderPaidVendorNotification:
                case MessageTemplateSystemNames.OrderPaidAffiliateNotification:
                case MessageTemplateSystemNames.OrderPlacedUserNotification:
                case MessageTemplateSystemNames.OrderCompletedUserNotification:
                case MessageTemplateSystemNames.OrderCancelledUserNotification:
                    return new[] { TokenGroupNames.StoreTokens, TokenGroupNames.OrderTokens, TokenGroupNames.UserTokens };

                case MessageTemplateSystemNames.ShipmentSentUserNotification:
                case MessageTemplateSystemNames.ShipmentDeliveredUserNotification:
                    return new[] { TokenGroupNames.StoreTokens, TokenGroupNames.ShipmentTokens, TokenGroupNames.OrderTokens, TokenGroupNames.UserTokens };

                case MessageTemplateSystemNames.OrderRefundedStoreOwnerNotification:
                case MessageTemplateSystemNames.OrderRefundedUserNotification:
                    return new[] { TokenGroupNames.StoreTokens, TokenGroupNames.OrderTokens, TokenGroupNames.RefundedOrderTokens, TokenGroupNames.UserTokens };

                case MessageTemplateSystemNames.NewOrderNoteAddedUserNotification:
                    return new[] { TokenGroupNames.StoreTokens, TokenGroupNames.OrderNoteTokens, TokenGroupNames.OrderTokens, TokenGroupNames.UserTokens };

                case MessageTemplateSystemNames.RecurringPaymentCancelledStoreOwnerNotification:
                case MessageTemplateSystemNames.RecurringPaymentCancelledUserNotification:
                case MessageTemplateSystemNames.RecurringPaymentFailedUserNotification:
                    return new[] { TokenGroupNames.StoreTokens, TokenGroupNames.OrderTokens, TokenGroupNames.UserTokens, TokenGroupNames.RecurringPaymentTokens };

                case MessageTemplateSystemNames.NewsletterSubscriptionActivationMessage:
                case MessageTemplateSystemNames.NewsletterSubscriptionDeactivationMessage:
                    return new[] { TokenGroupNames.StoreTokens, TokenGroupNames.SubscriptionTokens };

                case MessageTemplateSystemNames.EmailAFriendMessage:
                    return new[] { TokenGroupNames.StoreTokens, TokenGroupNames.UserTokens, TokenGroupNames.ProductTokens, TokenGroupNames.EmailAFriendTokens };

                case MessageTemplateSystemNames.WishlistToFriendMessage:
                    return new[] { TokenGroupNames.StoreTokens, TokenGroupNames.UserTokens, TokenGroupNames.WishlistToFriendTokens };

                case MessageTemplateSystemNames.NewReturnRequestStoreOwnerNotification:
                case MessageTemplateSystemNames.NewReturnRequestUserNotification:
                case MessageTemplateSystemNames.ReturnRequestStatusChangedUserNotification:
                    return new[] { TokenGroupNames.StoreTokens, TokenGroupNames.OrderTokens, TokenGroupNames.UserTokens, TokenGroupNames.ReturnRequestTokens };

                case MessageTemplateSystemNames.NewForumTopicMessage:
                    return new[] { TokenGroupNames.StoreTokens, TokenGroupNames.ForumTopicTokens, TokenGroupNames.ForumTokens, TokenGroupNames.UserTokens };

                case MessageTemplateSystemNames.NewForumPostMessage:
                    return new[] { TokenGroupNames.StoreTokens, TokenGroupNames.ForumPostTokens, TokenGroupNames.ForumTopicTokens, TokenGroupNames.ForumTokens, TokenGroupNames.UserTokens };

                case MessageTemplateSystemNames.PrivateMessageNotification:
                    return new[] { TokenGroupNames.StoreTokens, TokenGroupNames.PrivateMessageTokens, TokenGroupNames.UserTokens };

                case MessageTemplateSystemNames.NewVendorAccountApplyStoreOwnerNotification:
                    return new[] { TokenGroupNames.StoreTokens, TokenGroupNames.UserTokens, TokenGroupNames.VendorTokens };

                case MessageTemplateSystemNames.VendorInformationChangeNotification:
                    return new[] { TokenGroupNames.StoreTokens, TokenGroupNames.VendorTokens };

                case MessageTemplateSystemNames.GiftCardNotification:
                    return new[] { TokenGroupNames.StoreTokens, TokenGroupNames.GiftCardTokens };

                case MessageTemplateSystemNames.ProductReviewStoreOwnerNotification:
                case MessageTemplateSystemNames.ProductReviewReplyUserNotification:
                    return new[] { TokenGroupNames.StoreTokens, TokenGroupNames.ProductReviewTokens, TokenGroupNames.UserTokens };

                case MessageTemplateSystemNames.QuantityBelowStoreOwnerNotification:
                    return new[] { TokenGroupNames.StoreTokens, TokenGroupNames.ProductTokens };

                case MessageTemplateSystemNames.QuantityBelowAttributeCombinationStoreOwnerNotification:
                    return new[] { TokenGroupNames.StoreTokens, TokenGroupNames.ProductTokens, TokenGroupNames.AttributeCombinationTokens };

                case MessageTemplateSystemNames.NewVatSubmittedStoreOwnerNotification:
                    return new[] { TokenGroupNames.StoreTokens, TokenGroupNames.UserTokens, TokenGroupNames.VatValidation };

                case MessageTemplateSystemNames.BlogCommentNotification:
                    return new[] { TokenGroupNames.StoreTokens, TokenGroupNames.BlogCommentTokens, TokenGroupNames.UserTokens };

                case MessageTemplateSystemNames.NewsCommentNotification:
                    return new[] { TokenGroupNames.StoreTokens, TokenGroupNames.NewsCommentTokens, TokenGroupNames.UserTokens };

                case MessageTemplateSystemNames.BackInStockNotification:
                    return new[] { TokenGroupNames.StoreTokens, TokenGroupNames.UserTokens, TokenGroupNames.ProductBackInStockTokens };

                case MessageTemplateSystemNames.ContactUsMessage:
                    return new[] { TokenGroupNames.StoreTokens, TokenGroupNames.ContactUs };

                case MessageTemplateSystemNames.ContactVendorMessage:
                    return new[] { TokenGroupNames.StoreTokens, TokenGroupNames.ContactVendor };

                default:
                    return new string[] { };
            }
        }

        #endregion
    }
}