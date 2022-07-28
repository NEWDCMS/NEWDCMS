namespace DCMS.Services.Security
{
    using DCMS.Core.Caching;

    public static partial class DCMSSecurityDefaults
    {

        public static CacheKey AclRecordByEntityIdNameCacheKey => new CacheKey("DCMS.aclrecord.entityid-name-{0}-{1}", AclRecordPrefixCacheKey);
        public static string AclRecordPrefixCacheKey => "DCMS.aclrecord.";



        public static CacheKey PermissionsAllowedCacheKey => new CacheKey("DCMS.permission.allowed-{0}-{1}", PermissionsPrefixCacheKey);
        public static CacheKey PermissionsAllByUserRoleIdCacheKey => new CacheKey("DCMS.permission.allbyuserroleid-{0}", PermissionsPrefixCacheKey);
        public static string PermissionsPrefixCacheKey => "DCMS.permission.";


        public static string RecaptchaApiUrl => "https://www.google.com/recaptcha/";
        public static string RecaptchaScriptPath => "api.js?onload=onloadCallback{0}&render=explicit{1}";
        public static string RecaptchaValidationPath => "api/siteverify?secret={0}&response={1}&remoteip={2}";

    }
}