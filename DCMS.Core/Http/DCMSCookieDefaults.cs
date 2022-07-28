
namespace DCMS.Core.Http
{
    /// <summary>
    /// 表示与cookie相关的默认值
    /// </summary>
    public static partial class DCMSCookieDefaults
    {
        public static string Prefix => ".DCMS";

        public static string UserStoreCookie => ".Store";

        public static string UserCookie => ".User";

        /// <summary>
        /// Gets a cookie name of the antiforgery
        /// </summary>
        public static string AntiforgeryCookie => ".Antiforgery";

        /// <summary>
        /// Gets a cookie name of the session state
        /// </summary>
        public static string SessionCookie => ".Session";

        /// <summary>
        /// Gets a cookie name of the temp data
        /// </summary>
        public static string TempDataCookie => ".TempData";

        /// <summary>
        /// Gets a cookie name of the installation language
        /// </summary>
        public static string InstallationLanguageCookie => ".InstallationLanguage";

        /// <summary>
        /// Gets a cookie name of the compared products
        /// </summary>
        public static string ComparedProductsCookie => ".ComparedProducts";

        /// <summary>
        /// Gets a cookie name of the recently viewed products
        /// </summary>
        public static string RecentlyViewedProductsCookie => ".RecentlyViewedProducts";

        /// <summary>
        /// Gets a cookie name of the authentication
        /// </summary>
        public static string AuthenticationCookie => ".Authentication";

        /// <summary>
        /// Gets a cookie name of the external authentication
        /// </summary>
        public static string ExternalAuthenticationCookie => ".ExternalAuthentication";

        /// <summary>
        /// Gets a cookie name of the Eu Cookie Law Warning
        /// </summary>
        public static string IgnoreEuCookieLawWarning => ".IgnoreEuCookieLawWarning";

    }
}