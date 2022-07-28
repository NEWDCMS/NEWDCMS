using DCMS.Core.Domain.Users;
using DCMS.ViewModel.Models.Users;
using System.Collections.Generic;

namespace DCMS.Web.Factories
{
    public interface IUserModelFactory
    {
        IList<UserAttributeModel> PrepareCustomUserAttributes(User user, string overrideAttributesXml = "");
        LoginModel PrepareLoginModel(bool? checkoutAsGuest);
        PasswordRecoveryConfirmModel PreparePasswordRecoveryConfirmModel();
        PasswordRecoveryModel PreparePasswordRecoveryModel();
        UserAvatarModel PrepareUserAvatarModel(UserAvatarModel model);
        UserModel PrepareUserInfoModel(UserModel model, User user, bool excludeProperties, string overrideCustomUserAttributesXml = "");
    }
}