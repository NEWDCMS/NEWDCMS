using System.ComponentModel.DataAnnotations.Schema;

namespace DCMS.Core.Domain.Messages
{
    /// <summary>
    /// 表示邮件账户
    /// </summary>
    public partial class EmailAccount : BaseEntity
    {


        /// <summary>
        /// 邮件地址
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// 显示名称
        /// </summary>
        public string DisplayName { get; set; }

        /// <summary>
        /// 主机
        /// </summary>
        public string Host { get; set; }

        /// <summary>
        /// 端口
        /// </summary>
        public int Port { get; set; }

        /// <summary>
        /// 用户名
        /// </summary>
        public string Username { get; set; }

        /// <summary>
        /// 口令
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// 是否启用SSL
        /// </summary>
        [Column(TypeName = "BIT(1)")]
        public bool EnableSsl { get; set; }

        /// <summary>
        /// 是的使用默认认证
        /// </summary>
        [Column(TypeName = "BIT(1)")]
        public bool UseDefaultCredentials { get; set; }

        /// <summary>
        /// 友好名称
        /// </summary>
        public string FriendlyName
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(DisplayName))
                {
                    return Email + " (" + DisplayName + ")";
                }

                return Email;
            }
        }
    }
}
