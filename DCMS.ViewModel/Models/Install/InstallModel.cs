using DCMS.Web.Framework.Models;
using DCMS.Web.Framework.Mvc.ModelBinding;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace DCMS.ViewModel.Models.Install
{
    public partial class InstallModel : BaseModel
    {
        public InstallModel()
        {
        }

        /// <summary>
        /// 经销商名称
        /// </summary>
        [DisplayName("经销商名称")]
        public string StoreName { get; set; }
        /// <summary>
        /// 邮箱
        /// </summary>
        [DisplayName("邮箱")]
        [RegularExpression(@"^[\w-]+(\.[\w-]+)*@([\w-]+\.)+[a-zA-Z]+$", ErrorMessage = "邮箱格式不正确")]
        public string StoreEmail { get; set; }

        /// <summary>
        /// 手机号
        /// </summary>
        [DisplayName("手机号")]
        public string StoreMobileNumber { get; set; }
        /// <summary>
        /// 管理账户名
        /// </summary>
        [DisplayName("管理账户名")]
        public string AdminUserName { get; set; }
        /// <summary>
        /// 管理密码
        /// </summary>
        [DisplayName("管理密码")]
        [DCMSTrim]
        [DataType(DataType.Password)]
        public string AdminPassword { get; set; }

        [DisplayName("确认密码")]
        [DCMSTrim]
        [DataType(DataType.Password)]
        public string ConfirmPassword { get; set; }


    }
}
