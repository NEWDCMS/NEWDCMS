using DCMS.ViewModel.Models.Users;
using DCMS.Web.Framework.Validators;

namespace DCMS.ViewModel.Validators.Users
{
    public class UserRoleValidator : BaseDCMSValidator<UserRoleModel>
    {
        public UserRoleValidator()
        {
            //RuleFor(x => x.Name).NotNull().WithMessage("角色名不能为空");
            //RuleFor(x => x.SystemName).NotNull().WithMessage("系统名称不能为空");
        }
    }
}