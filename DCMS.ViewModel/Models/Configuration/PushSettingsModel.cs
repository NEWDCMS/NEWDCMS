using DCMS.Web.Framework;
using DCMS.Web.Framework.Models;
using System.ComponentModel.DataAnnotations;


namespace DCMS.ViewModel.Models.Configuration
{
    // [Validator(typeof(PushValidator))]
    public partial class PushSettingsModel : BaseModel, ISettingsModel
    {
        public int ActiveStoreScopeConfiguration { get; set; }

        [HintDisplayName("主机", "主机")]
        public string Host { get; set; }

        [HintDisplayName("虚拟机", "虚拟机")]
        public string VirtualHost { get; set; }

        [HintDisplayName("用户名", "用户名")]
        public string UserName { get; set; }

        [DataType(DataType.Password)]
        [HintDisplayName("密码", "密码")]
        public string Password { get; set; }

        [HintDisplayName("盐", "盐")]
        public string SaltKey { get; set; }
    }
}
