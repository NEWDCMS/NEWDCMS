using DCMS.Core;
using DCMS.Core.Domain.Security;
using DCMS.Core.Domain.Users;
using System.Collections.Generic;

namespace DCMS.Services.Security
{

    public partial interface IPermissionService
    {

        void DeletePermissionRecord(PermissionRecord permission);
        IList<PermissionRecord> GetAllPermissionRecordsByStore(int? store);


        PermissionRecord GetPermissionRecordById(int permissionId);

        /// <summary>
        /// 验证当前权限码是否存在
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        bool CheckPermissionRecordCode(int code, int permissionId = 0);

        PermissionRecord GetPermissionRecordBySystemName(string systemName);

        IPagedList<PermissionRecord> GetAllPermissionRecords(string name, string systemName, int? store, int pageIndex = 0, int pageSize = int.MaxValue);
        IList<PermissionRecord> GetAllPermissionRecordsByModuleId(int? store, int? moduleId, bool showMobile);
        IList<PermissionRecord> GetAllPermissionRecords();

        IList<PermissionRecord> GetPermissionRecordByIds(int[] ids);

        void InsertPermissionRecord(PermissionRecord permission);

        void InsertPermissionRecord(int? store, PermissionRecord[] permissions);
        void UpdatePermissionRecord(PermissionRecord permission);

        void InstallPermissions(IPermissionProvider permissionProvider);


        void UninstallPermissions(IPermissionProvider permissionProvider);

        bool Authorize(PermissionRecord permission);

        bool Authorize(PermissionRecord permission, User user);

        bool Authorize(string permissionRecordSystemName, User user);

        bool ManageAuthorize();



        DataChannelPermission GetDataChannelPermissionById(int permissionId);
        IList<DataChannelPermission> GetDataChannelPermissionByIds(int[] sIds);
        IPagedList<DataChannelPermission> GetAllDataChannelPermissions(int? store, int? roleId, int pageIndex = 0, int pageSize = int.MaxValue);
        IList<DataChannelPermission> GetAllDataChannelPermissionsByStore(int? store);
        IList<DataChannelPermission> GetAllDataChannelPermissionsByRoleId(int? store, int? roleId);
        void InsertDataChannelPermission(DataChannelPermission permission);
        void UpdateDataChannelPermission(DataChannelPermission permission);
        void DeleteDataChannelPermission(DataChannelPermission permission);
        bool CheckExist(int? store, int? roleId);


        IPagedList<PermissionRecordRoles> GetPermissionRecordRolesByPermissionId(int permissionId, int? storeId, int pageIndex, int pageSize);
        IList<PermissionRecordRoles> GetPermissionRecordRolesByPermissionId(int? store, int permissionId);
        IPagedList<PermissionRecordRoles> GetPermissionRecordRolesByUserRoleId(int userRoleId, int? storeId, int pageIndex, int pageSize);
        IList<PermissionRecordRoles> GetPermissionRecordRolesByUserRoleId(int? store, int userRoleId);
        PermissionRecordRoles GetPermissionRecordRolesById(int permissionRecordRolesId);
        void InsertPermissionRecordRoles(PermissionRecordRoles permissionRecordRoles);
        void UpdatePermissionRecordRoles(PermissionRecordRoles permissionRecordRoles);
        void DeletePermissionRecordRoles(PermissionRecordRoles permissionRecordRoles);

        /// <summary>
        /// 获取用户权限码
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        List<string> GetUserAuthorizeCodesByUserId(int storeId, int userId, bool platform = false);

        /// <summary>
        /// 权限保存
        /// </summary>
        /// <param name="storeId">经销商</param>
        /// <param name="userId">用户</param>
        /// <param name="roleId">权限Id</param>
        /// <param name="pcModuleIds">PC端选择模块Id</param>
        /// <param name="pcPermissionIds">PC端选择权限Id</param>
        /// <param name="appModuleIds">APP端选择模块Id</param>
        /// <param name="appPermissionIds">APP端选择权限Id</param>
        /// <param name="dataChannelPermission">数据和频道</param>
        /// <returns></returns>
        BaseResult PermissionsSave(int storeId, int userId, int roleId, List<int> pcModuleIds, List<int> pcPermissionIds, List<int> appModuleIds, List<int> appPermissionIds, DataChannelPermission dataChannelPermission);

        BaseResult ManagePermissionsSave(int storeId, int roleId, List<int> pcModuleIds, List<int> pcPermissionIds);

    }
}