using System.ComponentModel;

namespace DCMS.Core
{
    public enum PasswordFormat : int
    {
        /// <summary>
        /// 未加密
        /// </summary>
        [Description("未加密")]
        Clear = 0,

        /// <summary>
        /// 哈希
        /// </summary>
        [Description("哈希")]
        Hashed = 1,

        /// <summary>
        /// Encrypted加密
        /// </summary>
        [Description("Encrypted加密")]
        Encrypted = 2
    }


    public enum UserNameFormat
    {
        [Description("邮箱")]
        ShowEmails = 1,

        [Description("用户名")]
        ShowUsernames = 2,

        [Description("实名")]
        ShowFullNames = 3,

        [Description("手机号")]
        ShowMobliNumber = 4
    }
}