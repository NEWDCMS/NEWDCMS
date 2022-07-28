using DCMS.Core.Domain.Chat;
using System.Collections.Generic;
using System.Collections.Concurrent;

namespace DCMS.Services.Chat
{
    public interface IChatUserService
    {
        IEnumerable<User> GetConnectedUsersExceptMe(User user);
        User GetUserById(int storeId, int userId);
        User GetUserById(int userId);
        IEnumerable<User> GetUsers(int? storeId);
        void InsertUser(User user);
        User LoginUser(int storeId, int userId, string connectionId, string name, string avatar, string mobileNumber, string openId);
        void LogOut(User user);
        bool NameValidation(string name);
        User RegisterUser(User user, string connectionId);
        void UpdateUser(User user);

        ConcurrentDictionary<string, string> CTUS();
        ConcurrentDictionary<string, string> UTCS();
    }
}