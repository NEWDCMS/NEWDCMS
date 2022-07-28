using DCMS.Core;
using DCMS.Core.Domain.Common;
using DCMS.Core.Domain.Security;
using DCMS.Core.Domain.Users;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq.Expressions;


namespace DCMS.Services.Users
{
    public interface IUserService
    {
        List<User> BindUserList(int storeId, string systemName, int curUserId = 0,bool selectFollower = false, bool isadmin = false);
        void ClearSystemCache(int? store);
        void ClearUserCache(int? store);
        void DeleteUser(User user);
        void DeleteUserDistricts(int userId, int districtId);
        void DeleteUserDistricts(UserDistricts userDistricts);
        void DeleteUserRole(UserRole userRole);
        IList<string> GetAllAdminUserMobileNumbersByStore(int storeId);
        List<User> GetAllAdminUsersByStoreIds(int[] storeIds);
        IList<UserDistricts> GetAllUserDistrictsByUserId(int? store, int userId);
        IList<User> GetAllUsers(int store = 0);
        IList<User> GetAllUsers(Expression<Func<User, User>> selector, int store = 0);
        Dictionary<int, string> GetAllUsers(int? store, string systemName);
        IList<string> GetAllUserMobileNumbersByUserIds(List<int> userIds);
        List<UserPercentages> GetAllUserPercentages(int? storeId);
        IList<UserRole> GetAllUserRoles(int? store = 0, bool showHidden = false);
        IPagedList<UserRole> GetAllUserRoles(int? store, bool showHidden = false, string name = "", int pageIndex = 0, int pageSize = int.MaxValue);
        IList<UserRole> GetAllUserRolesByStore(bool showHidden = false, int storeId = 0, bool isPlatform = false);
        IPagedList<User> GetAllUsers(DateTime? createdFromUtc = null, DateTime? createdToUtc = null, int[] userRoleIds = null, string email = null, string username = null, string realName = null, int branchid = 0, int store = 0, string phone = null, int pageIndex = 0, int pageSize = int.MaxValue);
        List<User> GetAllUsers(int? storeId);
        IList<User> GetAllUsersByPasswordFormat(PasswordFormat passwordFormat);
        UserPassword GetCurrentPassword(int userId);
        IList<ZTree> GetListZTreeSG();
        IList<ZTree> GetListZTreeVM(int roleId);
        string GetMobileNumberByUserId(int userId);
        IPagedList<User> GetOnlineUsers(DateTime lastActivityFromUtc, int[] userRoleIds, int pageIndex, int pageSize);
        List<User> GetStoreManagers(int? storeId);
        //void GetAllLeaderByUserId(int userId, List<int> leaders, int store = 0);
        List<int> GetAllSubordinateByUserId(int userId, int store);

        //List<int> GetSubordinate(int? store, int userId);
        List<int> GetSubordinate(int? store, int userId, string systemName);
        List<User> GetSubordinateUsers(int? store, int userId);

        List<int> GetSubordinate(int? store, int parentId);

        List<User> GetAllSubordinateUser(int? storeId, string subordinates,bool isAdmin = false);
        List<int> GetUsersByParentId(int? store, int pid);

        IList<PermissionRecord> GetUserAPPPermissionRecords(User user);
        IList<PermissionRecord> GetUserAPPPermissionRecordsByUser(User user);
        User GetUserByEmail(int? store, string email, bool noTracking = false, bool includeDelete = false);
        User GetUserByEmailNoCache(int? store, string email, bool noTracking = false, bool includeDelete = false);
        User GetUserByGuid(int? store, Guid userGuid, bool noTracking = false);
        User GetUserByGuidNoCache(int? store, Guid userGuid, bool noTracking = false);
        User GetUserById(int userId);
        User GetUserById(int? store, int userId, bool include = false);
        User GetUserByMobileNamber(int? store, string mobileNamber, bool noTracking = false, bool includeDelete = false);
        User GetUserByMobileNamberNoCache(int? store, string mobileNamber, bool noTracking = false, bool includeDelete = false);
        List<int> GetUserByroleId(int RoleId);
        List<User> GetUserByStoreIdUserRoleIds(int storeId, string userRoleIds);
        User GetUserBySystemName(int? store, string systemName, bool noTracking = false);
        User GetUserBySystemNameNoCache(int? store, string systemName, bool noTracking = false);
        List<User> GetUserBySystemRoleName(int storeId, string systemName);
        User GetUserByToken(string token);
        User GetUserByUsername(int? store, string username, bool noTracking = false, bool includeDelete = false);
        User GetUserByUsernameNoCache(int? store, string username, bool noTracking = false, bool includeDelete = false);
        UserDistricts GetUserDistrictsById(int userDistrictsId);
        decimal GetUserMaxAmountOfArrears(int? store, int userId);
        IList<Module> GetUserModuleRecords(int? store, User user, bool showMobile = false);
        IList<IntQueryType> GetUserModuleRecordsByUser(User user);
        Task<IList<IntQueryType>> GetUserModuleRecordsByUserAsync(User user);
        string GetUserName(int? store, int userId);
        IList<UserPassword> GetUserPasswords(int? userId = null, PasswordFormat? passwordFormat = null, int? passwordsToReturn = null);
        IList<PermissionRecord> GetUserPermissionRecords(User user);
        Task<IList<PermissionRecord>> GetUserPermissionRecordsAsync(User user);
        IList<PermissionRecord> GetUserPermissionRecordsByUser(User user);
        UserRole GetUserRoleById(int userRoleId);
        UserRole GetUserRoleBySystemName(int storeId, string systemName);
        UserRole GetUserRoleBySystemName(int? store, string systemName);
        int[] GetUserRoleIds(User user, bool showHidden = false);
        IList<Module> GetUserRoleModuleRecords(int? store, int userRoleId, bool showMobile = false);
        IList<PermissionRecord> GetUserRolePermissionRecords(int storeId, int userRoleId, int platform = 0);
        IList<UserRole> GetUserRolesByUser(int storeId, int userId);
        Dictionary<int, string> GetAllUserRolesByUser(int storeId);
        IList<User> GetUsersByIds(int? store, int[] userIds);
        IList<User> GetUsersByIds(int[] userIds);
        Dictionary<int, string> GetUsersDictsByIds(int storeId, int[] userIds);
        IList<User> GetUsersIdsByUsersIds(int store, int[] userIds, bool platform = false);
        string GetUUID(int? store, int userId);
        void InsertUser(User user);
        void InsertUserDistricts(UserDistricts userDistricts);
        void InsertUserPassword(UserPassword userPassword);
        void InsertUserRole(UserRole userRole);
        void InsertUserRoleMapping(int userId, int roleId, int storeI);
        bool IsAdmin(int? store, int userId, bool onlyActiveUserRoles = true);
        bool IsDelivers(User user, bool onlyActiveUserRoles = true);
        bool IsDistributor(User user, bool onlyActiveUserRoles = true);
        bool IsEmployee(User user, bool onlyActiveUserRoles = true);
        bool IsFinancialManager(User user, bool onlyActiveUserRoles = true);
        bool IsGuest(User user, bool onlyActiveUserRoles = true);
        bool IsInUserRole(int? store, int userId, string userRoleSystemName, bool onlyActiveUserRoles = true);
        bool IsMarketManager(User user, bool onlyActiveUserRoles = true);
        bool IsPasswordRecoveryLinkExpired(User user);
        bool IsPasswordRecoveryTokenValid(User user, string token);
        bool IsRegionManager(User user, bool onlyActiveUserRoles = true);
        bool IsRegistered(User user, bool onlyActiveCustomerRoles = true);
        bool IsSalesman(User user, bool onlyActiveUserRoles = true);
        bool PasswordIsExpired(User user);
        void RemoveUserRoleMapping(int userId, int roleId, int storeI);
        void UpdateUser(User user);
        void UpdateUserDistricts(UserDistricts userDistricts);
        void UpdateUserPassword(UserPassword userPassword);
        void UpdateUserRole(UserRole userRole);
        void UpdateUUID(int? store, int userId, string uuid);
        void AddToken(RefreshToken refreshToken);
    }
}