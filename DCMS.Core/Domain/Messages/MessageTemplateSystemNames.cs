namespace DCMS.Core.Domain.Messages
{
    /// <summary>
    /// Represents message template system names
    /// </summary>
    public static partial class MessageTemplateSystemNames
    {
        #region User

        /// <summary>
        /// Represents system name of notification about new registration
        /// </summary>
        public const string UserRegisteredNotification = "NewUser.Notification";

        /// <summary>
        /// Represents system name of user welcome message
        /// </summary>
        public const string UserWelcomeMessage = "User.WelcomeMessage";

        /// <summary>
        /// Represents system name of email validation message
        /// </summary>
        public const string UserEmailValidationMessage = "User.EmailValidationMessage";

        /// <summary>
        /// Represents system name of email revalidation message
        /// </summary>
        public const string UserEmailRevalidationMessage = "User.EmailRevalidationMessage";

        /// <summary>
        /// Represents system name of password recovery message
        /// </summary>
        public const string UserPasswordRecoveryMessage = "User.PasswordRecovery";

        #endregion

        #region Order

        /// <summary>
        /// Represents system name of notification vendor about placed order
        /// </summary>
        public const string OrderPlacedVendorNotification = "OrderPlaced.VendorNotification";

        /// <summary>
        /// Represents system name of notification store owner about placed order
        /// </summary>
        public const string OrderPlacedStoreOwnerNotification = "OrderPlaced.StoreOwnerNotification";

        /// <summary>
        /// Represents system name of notification affiliate about placed order
        /// </summary>
        public const string OrderPlacedAffiliateNotification = "OrderPlaced.AffiliateNotification";

        /// <summary>
        /// Represents system name of notification store owner about paid order
        /// </summary>
        public const string OrderPaidStoreOwnerNotification = "OrderPaid.StoreOwnerNotification";

        /// <summary>
        /// Represents system name of notification user about paid order
        /// </summary>
        public const string OrderPaidUserNotification = "OrderPaid.UserNotification";

        /// <summary>
        /// Represents system name of notification vendor about paid order
        /// </summary>
        public const string OrderPaidVendorNotification = "OrderPaid.VendorNotification";

        /// <summary>
        /// Represents system name of notification affiliate about paid order
        /// </summary>
        public const string OrderPaidAffiliateNotification = "OrderPaid.AffiliateNotification";

        /// <summary>
        /// Represents system name of notification user about placed order
        /// </summary>
        public const string OrderPlacedUserNotification = "OrderPlaced.UserNotification";

        /// <summary>
        /// Represents system name of notification user about sent shipment
        /// </summary>
        public const string ShipmentSentUserNotification = "ShipmentSent.UserNotification";

        /// <summary>
        /// Represents system name of notification user about delivered shipment
        /// </summary>
        public const string ShipmentDeliveredUserNotification = "ShipmentDelivered.UserNotification";

        /// <summary>
        /// Represents system name of notification user about completed order
        /// </summary>
        public const string OrderCompletedUserNotification = "OrderCompleted.UserNotification";

        /// <summary>
        /// Represents system name of notification user about cancelled order
        /// </summary>
        public const string OrderCancelledUserNotification = "OrderCancelled.UserNotification";

        /// <summary>
        /// Represents system name of notification store owner about refunded order
        /// </summary>
        public const string OrderRefundedStoreOwnerNotification = "OrderRefunded.StoreOwnerNotification";

        /// <summary>
        /// Represents system name of notification user about refunded order
        /// </summary>
        public const string OrderRefundedUserNotification = "OrderRefunded.UserNotification";

        /// <summary>
        /// Represents system name of notification user about new order note
        /// </summary>
        public const string NewOrderNoteAddedUserNotification = "User.NewOrderNote";

        /// <summary>
        /// Represents system name of notification store owner about cancelled recurring order
        /// </summary>
        public const string RecurringPaymentCancelledStoreOwnerNotification = "RecurringPaymentCancelled.StoreOwnerNotification";

        /// <summary>
        /// Represents system name of notification user about cancelled recurring order
        /// </summary>
        public const string RecurringPaymentCancelledUserNotification = "RecurringPaymentCancelled.UserNotification";

        /// <summary>
        /// Represents system name of notification user about failed payment for the recurring payments
        /// </summary>
        public const string RecurringPaymentFailedUserNotification = "RecurringPaymentFailed.UserNotification";

        #endregion

        #region Newsletter

        /// <summary>
        /// Represents system name of subscription activation message
        /// </summary>
        public const string NewsletterSubscriptionActivationMessage = "NewsLetterSubscription.ActivationMessage";

        /// <summary>
        /// Represents system name of subscription deactivation message
        /// </summary>
        public const string NewsletterSubscriptionDeactivationMessage = "NewsLetterSubscription.DeactivationMessage";

        #endregion

        #region To friend

        /// <summary>
        /// Represents system name of 'Email a friend' message
        /// </summary>
        public const string EmailAFriendMessage = "Service.EmailAFriend";

        /// <summary>
        /// Represents system name of 'Email a friend' message with wishlist
        /// </summary>
        public const string WishlistToFriendMessage = "Wishlist.EmailAFriend";

        #endregion

        #region Return requests

        /// <summary>
        /// Represents system name of notification store owner about new return request
        /// </summary>
        public const string NewReturnRequestStoreOwnerNotification = "NewReturnRequest.StoreOwnerNotification";

        /// <summary>
        /// Represents system name of notification user about new return request
        /// </summary>
        public const string NewReturnRequestUserNotification = "NewReturnRequest.UserNotification";

        /// <summary>
        /// Represents system name of notification user about changing return request status
        /// </summary>
        public const string ReturnRequestStatusChangedUserNotification = "ReturnRequestStatusChanged.UserNotification";

        #endregion

        #region Forum

        /// <summary>
        /// Represents system name of notification about new forum topic
        /// </summary>
        public const string NewForumTopicMessage = "Forums.NewForumTopic";

        /// <summary>
        /// Represents system name of notification about new forum post
        /// </summary>
        public const string NewForumPostMessage = "Forums.NewForumPost";

        /// <summary>
        /// Represents system name of notification about new private message
        /// </summary>
        public const string PrivateMessageNotification = "User.NewPM";

        #endregion

        #region Misc

        /// <summary>
        /// Represents system name of notification store owner about applying new vendor account
        /// </summary>
        public const string NewVendorAccountApplyStoreOwnerNotification = "VendorAccountApply.StoreOwnerNotification";

        /// <summary>
        /// Represents system name of notification vendor about changing information
        /// </summary>
        public const string VendorInformationChangeNotification = "VendorInformationChange.StoreOwnerNotification";

        /// <summary>
        /// Represents system name of notification about gift card
        /// </summary>
        public const string GiftCardNotification = "GiftCard.Notification";

        /// <summary>
        /// Represents system name of notification store owner about new product review
        /// </summary>
        public const string ProductReviewStoreOwnerNotification = "Product.ProductReview";

        /// <summary>
        /// Represents system name of notification user about product review reply
        /// </summary>
        public const string ProductReviewReplyUserNotification = "ProductReview.Reply.UserNotification";

        /// <summary>
        /// Represents system name of notification store owner about below quantity of product
        /// </summary>
        public const string QuantityBelowStoreOwnerNotification = "QuantityBelow.StoreOwnerNotification";

        /// <summary>
        /// Represents system name of notification store owner about below quantity of product attribute combination
        /// </summary>
        public const string QuantityBelowAttributeCombinationStoreOwnerNotification = "QuantityBelow.AttributeCombination.StoreOwnerNotification";

        /// <summary>
        /// Represents system name of notification store owner about submitting new VAT
        /// </summary>
        public const string NewVatSubmittedStoreOwnerNotification = "NewVATSubmitted.StoreOwnerNotification";

        /// <summary>
        /// Represents system name of notification store owner about new blog comment
        /// </summary>
        public const string BlogCommentNotification = "Blog.BlogComment";

        /// <summary>
        /// Represents system name of notification store owner about new news comment
        /// </summary>
        public const string NewsCommentNotification = "News.NewsComment";

        /// <summary>
        /// Represents system name of notification user about product receipt
        /// </summary>
        public const string BackInStockNotification = "User.BackInStock";

        /// <summary>
        /// Represents system name of 'Contact us' message
        /// </summary>
        public const string ContactUsMessage = "Service.ContactUs";

        /// <summary>
        /// Represents system name of 'Contact vendor' message
        /// </summary>
        public const string ContactVendorMessage = "Service.ContactVendor";

        #endregion
    }
}