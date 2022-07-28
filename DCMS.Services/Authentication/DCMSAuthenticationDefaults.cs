using Microsoft.AspNetCore.Http;

namespace DCMS.Services.Authentication
{
    /// <summary>
    /// 认证服务默认配置值
    /// </summary>
    public static partial class DCMSAuthenticationDefaults
    {
        /// <summary>
        /// Authentication
        /// </summary>
        public static string AuthenticationScheme => "Authentication";

        /// <summary>
        /// 外部身份认证 ExternalAuthentication
        /// </summary>
        public static string ExternalAuthenticationScheme => "ExternalAuthentication";

        /// <summary>
        /// jsdcms
        /// </summary>
        public static string ClaimsIssuer => "jsdcms";

        /// <summary>
        /// /login
        /// </summary>
        public static PathString LoginPath => new PathString("/login");

        /// <summary>
        /// /logout
        /// </summary>
        public static PathString LogoutPath => new PathString("/logout");

        /// <summary>
        /// /page-not-found
        /// </summary>
        public static PathString AccessDeniedPath => new PathString("/page-not-found");

        /// <summary>
        /// The default value of the return URL parameter
        /// </summary>
        public static string ReturnUrlParameter => string.Empty;

        /// <summary>
        /// dcms.externalauth.errors
        /// </summary>
        public static string ExternalAuthenticationErrorsSessionKey => "dcms.externalauth.errors";
    }
}