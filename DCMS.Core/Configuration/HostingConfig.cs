namespace DCMS.Core.Configuration
{
    /// <summary>
    /// 表示启动宿主配置参数
    /// </summary>
    public partial class HostingConfig
    {
        /// <summary>
        /// 获取或设置自定义转发的http头 (e.g. CF-Connecting-IP, X-FORWARDED-PROTO, etc)
        /// </summary>
        public string ForwardedHttpHeader { get; set; }

        /// <summary>
        /// 是否使用 HTTP_CLUSTER_HTTPS
        /// </summary>
        public bool UseHttpClusterHttps { get; set; }

        /// <summary>
        /// 是否使用 HTTP_X_FORWARDED_PROTO
        /// </summary>
        public bool UseHttpXForwardedProto { get; set; }
    }
}