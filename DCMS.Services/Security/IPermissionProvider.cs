using DCMS.Core.Domain.Security;
using System.Collections.Generic;

namespace DCMS.Services.Security
{
    /// <summary>
    /// 权限提供器
    /// </summary>
    public interface IPermissionProvider
    {
        /// <summary>
        /// 获取管理平台权限
        /// </summary>
        /// <returns>Permissions</returns>
        IEnumerable<PermissionRecord> GetPermissions();

        /// <summary>
        /// 获取管理平台角色默认权限
        /// </summary>
        /// <returns>Default permissions</returns>
        IEnumerable<DefaultPermissionRecord> GetDefaultPermissions();


        /// <summary>
        /// 获取经销商管理员默认权限
        /// </summary>
        /// <returns>Permissions</returns>
        IEnumerable<DefaultPermissionRecord> GetStoreDefaultPermissions();
    }
}
