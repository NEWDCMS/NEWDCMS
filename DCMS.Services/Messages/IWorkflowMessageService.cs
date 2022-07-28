using System.Collections.Generic;
using DCMS.Core.Domain.Messages;
using DCMS.Core.Domain.Products;
using DCMS.Core.Domain.Users;
using DCMS.Core.Domain.Vendors;

namespace DCMS.Services.Messages
{
    public interface IWorkflowMessageService
    {
        IList<int> SendContactUsMessage(string senderEmail, string senderName, string subject, string body);
        IList<int> SendContactVendorMessage(Vendor vendor, string senderEmail, string senderName, string subject, string body);
        IList<int> SendNewVendorAccountApplyStoreOwnerNotification(User user, Vendor vendor);
        int SendNotification(MessageTemplate messageTemplate, EmailAccount emailAccount, IEnumerable<Token> tokens, string toEmailAddress, string toName, string attachmentFilePath = null, string attachmentFileName = null, string replyToEmailAddress = null, string replyToName = null, string fromEmail = null, string fromName = null, string subject = null);
        IList<int> SendQuantityBelowStoreOwnerNotification(Product product);
        int SendTestEmail(int messageTemplateId, string sendToEmail, List<Token> tokens);
        IList<int> SendUserEmailRevalidationMessage(User user);
        IList<int> SendUserEmailValidationMessage(User user);
        IList<int> SendUserPasswordRecoveryMessage(User user);
        IList<int> SendUserRegisteredNotificationMessage(User user);
        IList<int> SendUserWelcomeMessage(User user);
        IList<int> SendVendorInformationChangeNotification(Vendor vendor);
    }
}