using DCMS.Core.Configuration;
using System.Collections.Generic;

namespace DCMS.Core.Domain.Security
{
    public class SecuritySettings : ISettings
    {

        //[Column(TypeName = "BIT(1)")]
        public bool ForceSslForAllPages { get; set; }

        public string EncryptionKey { get; set; }

        public List<string> AdminAreaAllowedIpAddresses { get; set; }

        /// <summary>
        /// 管理域是否应为公用存储启用xsrf保护
        /// </summary>
        //[Column(TypeName = "BIT(1)")]
        public bool EnableXsrfProtectionForAdminArea { get; set; }
        /// <summary>
        /// 是否应为公用存储启用xsrf保护
        /// </summary>
        //[Column(TypeName = "BIT(1)")]
        public bool EnableXsrfProtectionForPublicStore { get; set; }
        /// <summary>
        /// 是否在注册页上启用蜜罐技术
        /// </summary>
        //[Column(TypeName = "BIT(1)")]
        public bool HoneypotEnabled { get; set; }
        public string HoneypotInputName { get; set; }

        /// <summary>
        /// 是否允许在标题中使用非ascii字符
        /// </summary>
        //[Column(TypeName = "BIT(1)")]
        public bool AllowNonAsciiCharactersInHeaders { get; set; }
    }
}