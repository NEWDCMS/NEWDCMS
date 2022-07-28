using DCMS.ViewModel.Models.Users;
using DCMS.Web.Framework.Validators;
using FluentValidation;

namespace DCMS.ViewModel.Validators.Users
{
    public class UserValidator : BaseDCMSValidator<UserModel>
    {
        public UserValidator()
        {
            //大家在这里添加需要验证的字段，例如：
            RuleFor(x => x.Username).NotEmpty().WithMessage("用户名不能为空");
            RuleFor(x => x.Email).NotEmpty().WithMessage("邮箱不能为空");
            RuleFor(x => x.MobileNumber).NotEmpty().WithMessage("手机号不能为空");
        }
    }


    public class LoginValidator : BaseDCMSValidator<LoginModel>
    {
        public LoginValidator()
        {
            RuleFor(x => x.UserName).NotEmpty().WithMessage("请输入用户名、邮箱或者手机号登录!");
            RuleFor(x => x.Password).NotEmpty().WithMessage("请输入密码!");
            //CaptchaCode
            //RuleFor(x => x.CaptchaCode).NotEmpty().WithMessage("请输入验证码，大小写不区分！");
        }
    }

    public class PassWordChangeValidator : BaseDCMSValidator<PassWordChangeModel>
    {
        public PassWordChangeValidator()
        {
            //Password
            RuleFor(x => x.NewPassword).NotEmpty().WithMessage("请输入新密码!");
            RuleFor(x => x.Password).NotEmpty().WithMessage("请输入旧密码!");
            RuleFor(x => x.AgainPassword).NotEmpty().WithMessage("请再次输入密码!");
            RuleFor(x => x.AgainPassword).Equal(x => x.NewPassword).WithMessage("两次输入密码不一致");
        }
    }

}