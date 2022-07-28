namespace DCMS.Services.Messages
{
    public static partial class DCMSMessageDefaults
    {
        public static string MessageTemplatesAllCacheKey => "DCMS.messagetemplate.all-{0}";
        public static string MessageTemplatesByNameCacheKey => "DCMS.messagetemplate.name-{0}-{1}";
        public static string MessageTemplatesPrefixCacheKey => "DCMS.messagetemplate.";
        public static string NotificationListKey => "NotificationList";
        public static int MaxTries => 50;
    }
}