using DCMS.Core.Data;
using DCMS.Core.Domain.Common;
using DCMS.Core.Domain.Logging;
using DCMS.Core.Domain.Messages;
using DCMS.Core.Domain.Security;
using DCMS.Core.Domain.Stores;
using DCMS.Core.Domain.Users;

namespace DCMS.Services
{
    /// <summary>
    /// 服务基类
    /// </summary>
    public partial class BaseService
    {


        #region AUTH

        #region RW

        protected IRepository<RefreshToken> RefreshTokenRepository => _getter.RW<RefreshToken>(AUTH);
        protected IRepository<UserAttribute> UserAttributeRepository => _getter.RW<UserAttribute>(AUTH);
        protected IRepository<UserAttributeValue> UserAttributeValueRepository => _getter.RW<UserAttributeValue>(AUTH);
        protected IRepository<GenericAttribute> GaRepository => _getter.RW<GenericAttribute>(AUTH);
        protected IRepository<AclRecord> AclRecordRepository => _getter.RW<AclRecord>(AUTH);
        protected IRepository<ActivityLog> ActivityLogRepository => _getter.RW<ActivityLog>(AUTH);
        protected IRepository<ActivityLogType> ActivityLogTypeRepository => _getter.RW<ActivityLogType>(AUTH);
        protected IRepository<Branch> BranchRepository => _getter.RW<Branch>(AUTH);
        protected IRepository<DataChannelPermission> DataChannelPermissionsRepository => _getter.RW<DataChannelPermission>(AUTH);
        protected IRepository<Log> LogRepository => _getter.RW<Log>(AUTH);
        protected IRepository<Module> ModuleRepository => _getter.RW<Module>(AUTH);
        protected IRepository<ModuleRole> ModuleRoleRepository => _getter.RW<ModuleRole>(AUTH);
        protected IRepository<Partner> PartnersRepository => _getter.RW<Partner>(AUTH);
        protected IRepository<PermissionRecord> PermissionRecordRepository => _getter.RW<PermissionRecord>(AUTH);
        protected IRepository<PermissionRecordRoles> PermissionRecordRolesMappingRepository => _getter.RW<PermissionRecordRoles>(AUTH);
        protected IRepository<UserDistricts> UserDistrictsRepository => _getter.RW<UserDistricts>(AUTH);
        protected IRepository<UserGroup> UserGroupRepository => _getter.RW<UserGroup>(AUTH);
        protected IRepository<UserGroupUser> UserGroupUserRepository => _getter.RW<UserGroupUser>(AUTH);
        protected IRepository<UserGroupUserRole> UserGroupUserRoleRepository => _getter.RW<UserGroupUserRole>(AUTH);
        protected IRepository<UserRole> UserRoleRepository => _getter.RW<UserRole>(AUTH);
        protected IRepository<UserUserRole> UserUserRoleRepository => _getter.RW<UserUserRole>(AUTH);
        protected IRepository<User> UserRepository => _getter.RW<User>(AUTH);
        protected IRepository<UserPassword> UserPasswordRepository => _getter.RW<UserPassword>(AUTH);
        protected IRepository<PrivateMessage> PrivateMessageRepository => _getter.RW<PrivateMessage>(AUTH);
        protected IRepository<Corporations> CorporationsRepository => _getter.RW<Corporations>(AUTH);

        #endregion

        #region RO

        protected IRepository<RefreshToken> RefreshTokenRepository_RO => _getter.RW<RefreshToken>(AUTH);
        protected IRepositoryReadOnly<UserAttribute> UserAttributeRepository_RO => _getter.RO<UserAttribute>(AUTH);
        protected IRepositoryReadOnly<UserAttributeValue> UserAttributeValueRepository_RO => _getter.RO<UserAttributeValue>(AUTH);
        protected IRepositoryReadOnly<GenericAttribute> GaRepository_RO => _getter.RO<GenericAttribute>(AUTH);
        protected IRepositoryReadOnly<AclRecord> AclRecordRepository_RO => _getter.RO<AclRecord>(AUTH);
        protected IRepositoryReadOnly<ActivityLog> ActivityLogRepository_RO => _getter.RO<ActivityLog>(AUTH);
        protected IRepositoryReadOnly<ActivityLogType> ActivityLogTypeRepository_RO => _getter.RO<ActivityLogType>(AUTH);
        protected IRepositoryReadOnly<Branch> BranchRepository_RO => _getter.RO<Branch>(AUTH);
        protected IRepositoryReadOnly<DataChannelPermission> DataChannelPermissionsRepository_RO => _getter.RO<DataChannelPermission>(AUTH);
        protected IRepositoryReadOnly<Log> LogRepository_RO => _getter.RO<Log>(AUTH);
        protected IRepositoryReadOnly<Module> ModuleRepository_RO => _getter.RO<Module>(AUTH);
        protected IRepositoryReadOnly<ModuleRole> ModuleRoleRepository_RO => _getter.RO<ModuleRole>(AUTH);
        protected IRepositoryReadOnly<Partner> PartnersRepository_RO => _getter.RO<Partner>(AUTH);
        protected IRepositoryReadOnly<PermissionRecord> PermissionRecordRepository_RO => _getter.RO<PermissionRecord>(AUTH);
        protected IRepositoryReadOnly<PermissionRecordRoles> PermissionRecordRolesMappingRepository_RO => _getter.RO<PermissionRecordRoles>(AUTH);
        protected IRepositoryReadOnly<UserDistricts> UserDistrictsRepository_RO => _getter.RO<UserDistricts>(AUTH);
        protected IRepositoryReadOnly<UserGroup> UserGroupRepository_RO => _getter.RO<UserGroup>(AUTH);
        protected IRepositoryReadOnly<UserGroupUser> UserGroupUserRepository_RO => _getter.RO<UserGroupUser>(AUTH);
        protected IRepositoryReadOnly<UserGroupUserRole> UserGroupUserRoleRepository_RO => _getter.RO<UserGroupUserRole>(AUTH);
        protected IRepositoryReadOnly<UserRole> UserRoleRepository_RO => _getter.RO<UserRole>(AUTH);
        protected IRepositoryReadOnly<UserUserRole> UserUserRoleRepository_RO => _getter.RO<UserUserRole>(AUTH);
        protected IRepositoryReadOnly<User> UserRepository_RO => _getter.RO<User>(AUTH);
        protected IRepositoryReadOnly<UserPassword> UserPasswordRepository_RO => _getter.RO<UserPassword>(AUTH);
        protected IRepository<PrivateMessage> PrivateMessageRepository_RO => _getter.RW<PrivateMessage>(AUTH);
        protected IRepositoryReadOnly<Corporations> CorporationsRepository_RO => _getter.RO<Corporations>(AUTH);

        #endregion

        #endregion

    }

}
