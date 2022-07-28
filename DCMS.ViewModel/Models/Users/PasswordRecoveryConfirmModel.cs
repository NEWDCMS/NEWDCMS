using DCMS.Web.Framework.Models;
using DCMS.Web.Framework.Mvc.ModelBinding;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;


namespace DCMS.ViewModel.Models.Users
{
    public partial class PasswordRecoveryConfirmModel : BaseModel
    {
        [DataType(DataType.Password)]
        [DCMSTrim]
        [DisplayName("新密码")]
        public string NewPassword { get; set; }

        [DCMSTrim]
        [DataType(DataType.Password)]
        [DisplayName("确认密码")]
        public string ConfirmNewPassword { get; set; }

        public bool DisablePasswordChanging { get; set; }
        public string Result { get; set; }
    }
}