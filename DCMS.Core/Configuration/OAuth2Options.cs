namespace DCMS.Core.Configuration
{
    /// <summary>
    /// 第三方OAuth2登录配置选项
    /// </summary>
    public class OAuth2Options
    {
        /// <summary>
        /// 获取或设置 本应用在第三方OAuth2系统中的客户端Id
        /// </summary>
        public string ClientId { get; set; }

        /// <summary>
        /// 获取或设置 本应用在第三方OAuth2系统中的客户端密钥
        /// </summary>
        public string ClientSecret { get; set; }

        /// <summary>
        /// 获取或设置 是否启用
        /// </summary>
        public bool Enabled { get; set; }
    }
}