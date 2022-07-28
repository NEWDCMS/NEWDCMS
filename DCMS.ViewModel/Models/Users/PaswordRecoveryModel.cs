using DCMS.Web.Framework.Models;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;


namespace DCMS.ViewModel.Models.Users
{
    public partial class PasswordRecoveryModel : BaseModel
    {
        [DataType(DataType.EmailAddress)]
        [DisplayName("邮箱")]
        public string Email { get; set; }

        public string Result { get; set; }

        public bool DisplayCaptcha { get; set; }
    }
}