namespace DCMS.Services.Logging
{
    using DCMS.Core.Caching;

    public static partial class DCMSLoggingDefaults
    {
        public static CacheKey ActivityTypeAllCacheKey => new CacheKey("DCMS.activitytype.all", ActivityTypePrefixCacheKey);
        public static string ActivityTypePrefixCacheKey => "DCMS.activitytype.";
    }
}