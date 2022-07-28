using DCMS.Core.Configuration;

namespace DCMS.Core.Domain.Security
{
    /// <summary>
    /// 代理设置
    /// </summary>
    public partial class ProxySettings : ISettings
    {
        /// <summary>
        /// 是否应使用代理连接
        /// </summary>
        //[Column(TypeName = "BIT(1)")]
        public bool Enabled { get; set; }

        /// <summary>
        /// 代理地址
        /// </summary>
        public string Address { get; set; }

        /// <summary>
        /// 代理端口
        /// </summary>
        public string Port { get; set; }

        /// <summary>
        /// 用户名
        /// </summary>
        public string Username { get; set; }

        /// <summary>
        /// 密码
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// 是否为本地地址绕过代理服务器
        /// </summary>
        //[Column(TypeName = "BIT(1)")]
        public bool BypassOnLocal { get; set; }

        /// <summary>
        /// 是否随请求发送授权头
        /// </summary>
        //[Column(TypeName = "BIT(1)")]
        public bool PreAuthenticate { get; set; }
    }
}