using DCMS.Core;
using DCMS.Core.Domain.Users;
namespace DCMS.Services.Users
{
    public interface IUserRegistrationService
    {
        DCMSStatusCode ValidateUser(ref User user, string usernameOrEmailOrMobileNumber, string password, bool isTrader = false, string appId = "", bool isPlatform = false);
        PasswordChangeResult ChangePassword(ChangePasswordRequest request);
        void SetUsername(User user, string newUsername);
    }
}