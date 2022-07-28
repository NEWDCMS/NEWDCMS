
namespace DCMS.Core.Http
{
    /// <summary>
    /// 表示与HTTP功能相关的默认值
    /// </summary>
    public static partial class DCMSHttpDefaults
    {
        /// <summary>
        /// 默认HTTP 客户端
        /// </summary>
        public static string DefaultHttpClient => "default";

        /// <summary>
        /// 是否使用POST将客户端重定向到新位置的值。
        /// </summary>
        public static string IsPostBeingDoneRequestItem => "dcms.IsPOSTBeingDone";

        /// <summary>
        /// HTTP_CLUSTER_HTTPS 头
        /// </summary>
        public static string HttpClusterHttpsHeader => "HTTP_CLUSTER_HTTPS";

        /// <summary>
        /// HTTP_X_FORWARDED_PROTO 头
        /// </summary>
        public static string HttpXForwardedProtoHeader => "X-Forwarded-Proto";

        /// <summary>
        /// X-FORWARDED-FOR 头
        /// </summary>
        public static string XForwardedForHeader => "X-FORWARDED-FOR";
    }
}