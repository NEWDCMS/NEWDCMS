using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using DCMS.Core;
using DCMS.Core.Domain.Products;
using DCMS.Core.Domain.Common;
using DCMS.Core.Domain.Users;
using DCMS.Core.Domain.Messages;
using DCMS.Core.Domain.News;
using DCMS.Core.Domain.Vendors;
using DCMS.Services.Users;
using DCMS.Services.Events;
using DCMS.Services.Localization;
using DCMS.Services.Stores;

namespace DCMS.Services.Messages
{
    /// <summary>
    /// 工作流消息服务
    /// </summary>
    public partial class WorkflowMessageService : IWorkflowMessageService
    {
        #region Fields

        private readonly CommonSettings _commonSettings;
        private readonly EmailAccountSettings _emailAccountSettings;
        private readonly IUserService _userService;
        private readonly IEmailAccountService _emailAccountService;
        private readonly IEventPublisher _eventPublisher;
        private readonly IMessageTemplateService _messageTemplateService;
        private readonly IMessageTokenProvider _messageTokenProvider;
        private readonly IQueuedEmailService _queuedEmailService;
        private readonly IStoreContext _storeContext;
        private readonly IStoreService _storeService;
        private readonly ITokenizer _tokenizer;

        #endregion

        #region Ctor

        public WorkflowMessageService(CommonSettings commonSettings,
            EmailAccountSettings emailAccountSettings,
            IUserService userService,
            IEmailAccountService emailAccountService,
            IEventPublisher eventPublisher,
            IMessageTemplateService messageTemplateService,
            IMessageTokenProvider messageTokenProvider,
            IQueuedEmailService queuedEmailService,
            IStoreContext storeContext,
            IStoreService storeService,
            ITokenizer tokenizer)
        {
            _commonSettings = commonSettings;
            _emailAccountSettings = emailAccountSettings;
            _userService = userService;
            _emailAccountService = emailAccountService;
            _eventPublisher = eventPublisher;
            _messageTemplateService = messageTemplateService;
            _messageTokenProvider = messageTokenProvider;
            _queuedEmailService = queuedEmailService;
            _storeContext = storeContext;
            _storeService = storeService;
            _tokenizer = tokenizer;
        }

        #endregion

        #region Utilities

        /// <summary>
        /// Get active message templates by the name
        /// </summary>
        /// <param name="messageTemplateName">Message template name</param>
        /// <param name="storeId">Store identifier</param>
        /// <returns>List of message templates</returns>
        protected virtual IList<MessageTemplate> GetActiveMessageTemplates(string messageTemplateName, int storeId)
        {
            //get message templates by the name
            var messageTemplates = _messageTemplateService.GetMessageTemplatesByName(messageTemplateName, storeId);

            //no template found
            if (!messageTemplates?.Any() ?? true)
                return new List<MessageTemplate>();

            //filter active templates
            messageTemplates = messageTemplates.Where(messageTemplate => messageTemplate.IsActive).ToList();

            return messageTemplates;
        }

        /// <summary>
        /// Get EmailAccount to use with a message templates
        /// </summary>
        /// <param name="messageTemplate">Message template</param>
        /// <returns>EmailAccount</returns>
        protected virtual EmailAccount GetEmailAccountOfMessageTemplate(MessageTemplate messageTemplate)
        {
            var emailAccountId = messageTemplate.EmailAccountId;
            //some 0 validation (for localizable "Email account" dropdownlist which saves 0 if "Standard" value is chosen)
            if (emailAccountId == 0)
                emailAccountId = messageTemplate.EmailAccountId;

            var emailAccount = (_emailAccountService.GetEmailAccountById(emailAccountId) ?? _emailAccountService.GetEmailAccountById(_emailAccountSettings.DefaultEmailAccountId)) ??
                               _emailAccountService.GetAllEmailAccounts().FirstOrDefault();
            return emailAccount;
        }


        #endregion

        #region Methods

        #region User workflow

        /// <summary>
        /// Sends 'New user' notification message to a store owner
        /// </summary>
        /// <param name="user">User instance</param>
        /// <returns>Queued email identifier</returns>
        public virtual IList<int> SendUserRegisteredNotificationMessage(User user)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            var store = _storeContext.CurrentStore;


            var messageTemplates = GetActiveMessageTemplates(MessageTemplateSystemNames.UserRegisteredNotification, store.Id);
            if (!messageTemplates.Any())
                return new List<int>();

            //tokens
            var commonTokens = new List<Token>();
            _messageTokenProvider.AddUserTokens(commonTokens, user);

            return messageTemplates.Select(messageTemplate =>
            {
                //email account
                var emailAccount = GetEmailAccountOfMessageTemplate(messageTemplate);

                var tokens = new List<Token>(commonTokens);
                _messageTokenProvider.AddStoreTokens(tokens, store, emailAccount);

                //event notification
                _eventPublisher.MessageTokensAdded(messageTemplate, tokens);

                var toEmail = emailAccount.Email;
                var toName = emailAccount.DisplayName;

                return SendNotification(messageTemplate, emailAccount, tokens, toEmail, toName);
            }).ToList();
        }

        /// <summary>
        /// Sends a welcome message to a user
        /// </summary>
        /// <param name="user">User instance</param>
        /// <returns>Queued email identifier</returns>
        public virtual IList<int> SendUserWelcomeMessage(User user)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            var store = _storeContext.CurrentStore;


            var messageTemplates = GetActiveMessageTemplates(MessageTemplateSystemNames.UserWelcomeMessage, store.Id);
            if (!messageTemplates.Any())
                return new List<int>();

            //tokens
            var commonTokens = new List<Token>();
            _messageTokenProvider.AddUserTokens(commonTokens, user);

            return messageTemplates.Select(messageTemplate =>
            {
                //email account
                var emailAccount = GetEmailAccountOfMessageTemplate(messageTemplate);

                var tokens = new List<Token>(commonTokens);
                _messageTokenProvider.AddStoreTokens(tokens, store, emailAccount);

                //event notification
                _eventPublisher.MessageTokensAdded(messageTemplate, tokens);

                var toEmail = user.Email;
                var toName = _userService.GetUserName(user.Id);

                return SendNotification(messageTemplate, emailAccount, tokens, toEmail, toName);
            }).ToList();
        }

        /// <summary>
        /// Sends an email validation message to a user
        /// </summary>
        /// <param name="user">User instance</param>
        /// <returns>Queued email identifier</returns>
        public virtual IList<int> SendUserEmailValidationMessage(User user)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            var store = _storeContext.CurrentStore;


            var messageTemplates = GetActiveMessageTemplates(MessageTemplateSystemNames.UserEmailValidationMessage, store.Id);
            if (!messageTemplates.Any())
                return new List<int>();

            //tokens
            var commonTokens = new List<Token>();
            _messageTokenProvider.AddUserTokens(commonTokens, user);

            return messageTemplates.Select(messageTemplate =>
            {
                //email account
                var emailAccount = GetEmailAccountOfMessageTemplate(messageTemplate);

                var tokens = new List<Token>(commonTokens);
                _messageTokenProvider.AddStoreTokens(tokens, store, emailAccount);

                //event notification
                _eventPublisher.MessageTokensAdded(messageTemplate, tokens);

                var toEmail = user.Email;
                var toName = _userService.GetUserName(user.Id);

                return SendNotification(messageTemplate, emailAccount, tokens, toEmail, toName);
            }).ToList();
        }

        /// <summary>
        /// Sends an email re-validation message to a user
        /// </summary>
        /// <param name="user">User instance</param>
        /// <returns>Queued email identifier</returns>
        public virtual IList<int> SendUserEmailRevalidationMessage(User user)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            var store = _storeContext.CurrentStore;


            var messageTemplates = GetActiveMessageTemplates(MessageTemplateSystemNames.UserEmailRevalidationMessage, store.Id);
            if (!messageTemplates.Any())
                return new List<int>();

            //tokens
            var commonTokens = new List<Token>();
            _messageTokenProvider.AddUserTokens(commonTokens, user);

            return messageTemplates.Select(messageTemplate =>
            {
                //email account
                var emailAccount = GetEmailAccountOfMessageTemplate(messageTemplate);

                var tokens = new List<Token>(commonTokens);
                _messageTokenProvider.AddStoreTokens(tokens, store, emailAccount);

                //event notification
                _eventPublisher.MessageTokensAdded(messageTemplate, tokens);

                //email to re-validate
                var toEmail = user.Email;
                var toName = _userService.GetUserName(user.Id);

                return SendNotification(messageTemplate, emailAccount, tokens, toEmail, toName);
            }).ToList();
        }

        /// <summary>
        /// Sends password recovery message to a user
        /// </summary>
        /// <param name="user">User instance</param>
        /// <returns>Queued email identifier</returns>
        public virtual IList<int> SendUserPasswordRecoveryMessage(User user)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            var store = _storeContext.CurrentStore;


            var messageTemplates = GetActiveMessageTemplates(MessageTemplateSystemNames.UserPasswordRecoveryMessage, store.Id);
            if (!messageTemplates.Any())
                return new List<int>();

            //tokens
            var commonTokens = new List<Token>();
            _messageTokenProvider.AddUserTokens(commonTokens, user);

            return messageTemplates.Select(messageTemplate =>
            {
                //email account
                var emailAccount = GetEmailAccountOfMessageTemplate(messageTemplate);

                var tokens = new List<Token>(commonTokens);
                _messageTokenProvider.AddStoreTokens(tokens, store, emailAccount);

                //event notification
                _eventPublisher.MessageTokensAdded(messageTemplate, tokens);

                var toEmail = user.Email;
                var toName = _userService.GetUserName(user.Id);

                return SendNotification(messageTemplate, emailAccount, tokens, toEmail, toName);
            }).ToList();
        }

        #endregion


        #region Misc

        /// <summary>
        /// Sends 'New vendor account submitted' message to a store owner
        /// </summary>
        /// <param name="user">User</param>
        /// <param name="vendor">Vendor</param>
        /// <returns>Queued email identifier</returns>
        public virtual IList<int> SendNewVendorAccountApplyStoreOwnerNotification(User user, Vendor vendor)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            if (vendor == null)
                throw new ArgumentNullException(nameof(vendor));

            var store = _storeContext.CurrentStore;


            var messageTemplates = GetActiveMessageTemplates(MessageTemplateSystemNames.NewVendorAccountApplyStoreOwnerNotification, store.Id);
            if (!messageTemplates.Any())
                return new List<int>();

            //tokens
            var commonTokens = new List<Token>();
            _messageTokenProvider.AddUserTokens(commonTokens, user);
    

            return messageTemplates.Select(messageTemplate =>
            {
                //email account
                var emailAccount = GetEmailAccountOfMessageTemplate(messageTemplate);

                var tokens = new List<Token>(commonTokens);
                _messageTokenProvider.AddStoreTokens(tokens, store, emailAccount);

                //event notification
                _eventPublisher.MessageTokensAdded(messageTemplate, tokens);

                var toEmail = emailAccount.Email;
                var toName = emailAccount.DisplayName;

                return SendNotification(messageTemplate, emailAccount, tokens, toEmail, toName);
            }).ToList();
        }

        /// <summary>
        /// Sends 'Vendor information changed' message to a store owner
        /// </summary>
        /// <param name="vendor">Vendor</param>
        /// <returns>Queued email identifier</returns>
        public virtual IList<int> SendVendorInformationChangeNotification(Vendor vendor)
        {
            if (vendor == null)
                throw new ArgumentNullException(nameof(vendor));

            var store = _storeContext.CurrentStore;


            var messageTemplates = GetActiveMessageTemplates(MessageTemplateSystemNames.VendorInformationChangeNotification, store.Id);
            if (!messageTemplates.Any())
                return new List<int>();

            //tokens
            var commonTokens = new List<Token>();
        
            return messageTemplates.Select(messageTemplate =>
            {
                //email account
                var emailAccount = GetEmailAccountOfMessageTemplate(messageTemplate);

                var tokens = new List<Token>(commonTokens);
                _messageTokenProvider.AddStoreTokens(tokens, store, emailAccount);

                //event notification
                _eventPublisher.MessageTokensAdded(messageTemplate, tokens);

                var toEmail = emailAccount.Email;
                var toName = emailAccount.DisplayName;

                return SendNotification(messageTemplate, emailAccount, tokens, toEmail, toName);
            }).ToList();
        }


        /// <summary>
        /// Sends a "quantity below" notification to a store owner
        /// </summary>
        /// <param name="product">Product</param>
        /// <returns>Queued email identifier</returns>
        public virtual IList<int> SendQuantityBelowStoreOwnerNotification(Product product)
        {
            if (product == null)
                throw new ArgumentNullException(nameof(product));

            var store = _storeContext.CurrentStore;

            var messageTemplates = GetActiveMessageTemplates(MessageTemplateSystemNames.QuantityBelowStoreOwnerNotification, store.Id);
            if (!messageTemplates.Any())
                return new List<int>();

            var commonTokens = new List<Token>();
            //_messageTokenProvider.AddProductTokens(commonTokens, product);

            return messageTemplates.Select(messageTemplate =>
            {
                //email account
                var emailAccount = GetEmailAccountOfMessageTemplate(messageTemplate);

                var tokens = new List<Token>(commonTokens);
                _messageTokenProvider.AddStoreTokens(tokens, store, emailAccount);

                //event notification
                _eventPublisher.MessageTokensAdded(messageTemplate, tokens);

                var toEmail = emailAccount.Email;
                var toName = emailAccount.DisplayName;

                return SendNotification(messageTemplate, emailAccount, tokens, toEmail, toName);
            }).ToList();
        }


        /// <summary>
        /// Sends "contact us" message
        /// </summary>
        /// <param name="senderEmail">Sender email</param>
        /// <param name="senderName">Sender name</param>
        /// <param name="subject">Email subject. Pass null if you want a message template subject to be used.</param>
        /// <param name="body">Email body</param>
        /// <returns>Queued email identifier</returns>
        public virtual IList<int> SendContactUsMessage(string senderEmail,
            string senderName, string subject, string body)
        {
            var store = _storeContext.CurrentStore;


            var messageTemplates = GetActiveMessageTemplates(MessageTemplateSystemNames.ContactUsMessage, store.Id);
            if (!messageTemplates.Any())
                return new List<int>();

            //tokens
            var commonTokens = new List<Token>
            {
                new Token("ContactUs.SenderEmail", senderEmail),
                new Token("ContactUs.SenderName", senderName),
                new Token("ContactUs.Body", body, true)
            };

            return messageTemplates.Select(messageTemplate =>
            {
                //email account
                var emailAccount = GetEmailAccountOfMessageTemplate(messageTemplate);

                var tokens = new List<Token>(commonTokens);
                _messageTokenProvider.AddStoreTokens(tokens, store, emailAccount);

                string fromEmail;
                string fromName;
                //required for some SMTP servers
                if (_commonSettings.UseSystemEmailForContactUsForm)
                {
                    fromEmail = emailAccount.Email;
                    fromName = emailAccount.DisplayName;
                    body = $"<strong>From</strong>: {WebUtility.HtmlEncode(senderName)} - {WebUtility.HtmlEncode(senderEmail)}<br /><br />{body}";
                }
                else
                {
                    fromEmail = senderEmail;
                    fromName = senderName;
                }

                //event notification
                _eventPublisher.MessageTokensAdded(messageTemplate, tokens);

                var toEmail = emailAccount.Email;
                var toName = emailAccount.DisplayName;

                return SendNotification(messageTemplate, emailAccount, tokens, toEmail, toName,
                    fromEmail: fromEmail,
                    fromName: fromName,
                    subject: subject,
                    replyToEmailAddress: senderEmail,
                    replyToName: senderName);
            }).ToList();
        }

        /// <summary>
        /// Sends "contact vendor" message
        /// </summary>
        /// <param name="vendor">Vendor</param>
        /// <param name="senderEmail">Sender email</param>
        /// <param name="senderName">Sender name</param>
        /// <param name="subject">Email subject. Pass null if you want a message template subject to be used.</param>
        /// <param name="body">Email body</param>
        /// <returns>Queued email identifier</returns>
        public virtual IList<int> SendContactVendorMessage(Vendor vendor, string senderEmail,
            string senderName, string subject, string body)
        {
            if (vendor == null)
                throw new ArgumentNullException(nameof(vendor));

            var store = _storeContext.CurrentStore;


            var messageTemplates = GetActiveMessageTemplates(MessageTemplateSystemNames.ContactVendorMessage, store.Id);
            if (!messageTemplates.Any())
                return new List<int>();

            //tokens
            var commonTokens = new List<Token>
            {
                new Token("ContactUs.SenderEmail", senderEmail),
                new Token("ContactUs.SenderName", senderName),
                new Token("ContactUs.Body", body, true)
            };

            return messageTemplates.Select(messageTemplate =>
            {
                //email account
                var emailAccount = GetEmailAccountOfMessageTemplate(messageTemplate);

                string fromEmail;
                string fromName;
                //required for some SMTP servers
                if (_commonSettings.UseSystemEmailForContactUsForm)
                {
                    fromEmail = emailAccount.Email;
                    fromName = emailAccount.DisplayName;
                    body = $"<strong>From</strong>: {WebUtility.HtmlEncode(senderName)} - {WebUtility.HtmlEncode(senderEmail)}<br /><br />{body}";
                }
                else
                {
                    fromEmail = senderEmail;
                    fromName = senderName;
                }

                var tokens = new List<Token>(commonTokens);
                _messageTokenProvider.AddStoreTokens(tokens, store, emailAccount);

                //event notification
                _eventPublisher.MessageTokensAdded(messageTemplate, tokens);

                var toEmail = vendor.Email;
                var toName = vendor.Name;

                return SendNotification(messageTemplate, emailAccount, tokens, toEmail, toName,
                    fromEmail: fromEmail,
                    fromName: fromName,
                    subject: subject,
                    replyToEmailAddress: senderEmail,
                    replyToName: senderName);
            }).ToList();
        }

        /// <summary>
        /// Sends a test email
        /// </summary>
        /// <param name="messageTemplateId">Message template identifier</param>
        /// <param name="sendToEmail">Send to email</param>
        /// <param name="tokens">Tokens</param>
        /// <returns>Queued email identifier</returns>
        public virtual int SendTestEmail(int messageTemplateId, string sendToEmail, List<Token> tokens)
        {
            var messageTemplate = _messageTemplateService.GetMessageTemplateById(messageTemplateId);
            if (messageTemplate == null)
                throw new ArgumentException("Template cannot be loaded");

            //email account
            var emailAccount = GetEmailAccountOfMessageTemplate(messageTemplate);

            //event notification
            _eventPublisher.MessageTokensAdded(messageTemplate, tokens);

            return SendNotification(messageTemplate, emailAccount, tokens, sendToEmail, null);
        }

        /// <summary>
        /// Send notification
        /// </summary>
        /// <param name="messageTemplate">Message template</param>
        /// <param name="emailAccount">Email account</param>
        /// <param name="tokens">Tokens</param>
        /// <param name="toEmailAddress">Recipient email address</param>
        /// <param name="toName">Recipient name</param>
        /// <param name="attachmentFilePath">Attachment file path</param>
        /// <param name="attachmentFileName">Attachment file name</param>
        /// <param name="replyToEmailAddress">"Reply to" email</param>
        /// <param name="replyToName">"Reply to" name</param>
        /// <param name="fromEmail">Sender email. If specified, then it overrides passed "emailAccount" details</param>
        /// <param name="fromName">Sender name. If specified, then it overrides passed "emailAccount" details</param>
        /// <param name="subject">Subject. If specified, then it overrides subject of a message template</param>
        /// <returns>Queued email identifier</returns>
        public virtual int SendNotification(MessageTemplate messageTemplate,
            EmailAccount emailAccount, IEnumerable<Token> tokens,
            string toEmailAddress, string toName,
            string attachmentFilePath = null, string attachmentFileName = null,
            string replyToEmailAddress = null, string replyToName = null,
            string fromEmail = null, string fromName = null, string subject = null)
        {
            if (messageTemplate == null)
                throw new ArgumentNullException(nameof(messageTemplate));

            if (emailAccount == null)
                throw new ArgumentNullException(nameof(emailAccount));

            //retrieve localized message template data
            var bcc = messageTemplate.BccEmailAddresses;
            if (string.IsNullOrEmpty(subject))
                subject = messageTemplate.Subject;
            var body = messageTemplate.Body;

            //Replace subject and body tokens 
            var subjectReplaced = _tokenizer.Replace(subject, tokens, false);
            var bodyReplaced = _tokenizer.Replace(body, tokens, true);

            //limit name length
            toName = CommonHelper.EnsureMaximumLength(toName, 300);

            var email = new QueuedEmail
            {
                Priority = QueuedEmailPriority.High,
                From = !string.IsNullOrEmpty(fromEmail) ? fromEmail : emailAccount.Email,
                FromName = !string.IsNullOrEmpty(fromName) ? fromName : emailAccount.DisplayName,
                To = toEmailAddress,
                ToName = toName,
                ReplyTo = replyToEmailAddress,
                ReplyToName = replyToName,
                CC = string.Empty,
                Bcc = bcc,
                Subject = subjectReplaced,
                Body = bodyReplaced,
                AttachmentFilePath = attachmentFilePath,
                AttachmentFileName = attachmentFileName,
                AttachedDownloadId = messageTemplate.AttachedDownloadId,
                CreatedOnUtc = DateTime.UtcNow,
                EmailAccountId = emailAccount.Id,
                DontSendBeforeDateUtc = !messageTemplate.DelayBeforeSend.HasValue ? null
                    : (DateTime?)(DateTime.UtcNow + TimeSpan.FromHours(messageTemplate.DelayPeriod.ToHours(messageTemplate.DelayBeforeSend.Value)))
            };

            _queuedEmailService.InsertQueuedEmail(email);
            return email.Id;
        }

        #endregion

        #endregion
    }
}