using DCMS.Core;
using DCMS.Core.Domain.Security;
using System.Collections.Generic;

namespace DCMS.Services.Security
{
    /// <summary>
    /// 标准权限提供
    /// </summary>
    public partial class StandardPermissionProvider : IPermissionProvider
    {

        #region 管理平台系统初始权限

        /// <summary>
        /// 访问管理区域
        /// </summary>
        public static readonly PermissionRecord AccessAdminPanel = new PermissionRecord
        {
            Name = "访问管理后台",
            SystemName = "AccessAdminPanel",
            Module = new Module() { Name = "Standard" }
        };


        /// <summary>
        /// 访问终端管理区域
        /// </summary>
        public static readonly PermissionRecord AccessClientAdminPanel = new PermissionRecord
        {
            Name = "访问终端管理区域",
            SystemName = "AccessClientAdminPanel",
            Module = new Module() { Name = "Client" }
        };

        /// <summary>
        /// 允许客户模拟
        /// </summary>
        public static readonly PermissionRecord AllowUserImpersonation = new PermissionRecord
        {
            Name = "管理后台.允许客户模拟",
            SystemName = "AllowUserImpersonation",
            Module = new Module()
            { Name = "Users" }
        };

        /// <summary>
        /// 产品管理
        /// </summary>
        public static readonly PermissionRecord ManageProducts = new PermissionRecord
        {
            Name = "管理后台.产品管理",
            SystemName = "ManageProducts",
            Module = new Module()
            { Name = "Catalog" }
        };

        /// <summary>
        /// 分类管理
        /// </summary>
        public static readonly PermissionRecord ManageCategories = new PermissionRecord
        {
            Name = "管理后台.分类管理",
            SystemName = "ManageCategories",
            Module = new Module()
            { Name = "Catalog" }
        };


        /// <summary>
        /// 用户管理
        /// </summary>
        public static readonly PermissionRecord ManageUsers = new PermissionRecord
        {
            Name = "管理后台.用户管理",
            SystemName = "ManageUsers",
            Module = new Module()
            { Name = "Users" }
        };

        /// <summary>
        /// 用户角色管理
        /// </summary>
        public static readonly PermissionRecord ManageUserRoles = new PermissionRecord
        {
            Name = "管理后台.用户角色管理",
            SystemName = "ManageUserRoles",
            Module = new Module()
            { Name = "Users" }
        };

        /// <summary>
        /// 经销商管理
        /// </summary>
        public static readonly PermissionRecord ManageDistributors = new PermissionRecord
        {
            Name = "管理后台.经销商管理",
            SystemName = "ManageDistributors",
            Module = new Module()
            { Name = "Users" }
        };

        /// <summary>
        /// 消息模板
        /// </summary>
        public static readonly PermissionRecord ManageMessageTemplates = new PermissionRecord
        {
            Name = "管理后台.消息模板",
            SystemName = "ManageMessageTemplates",
            Module = new Module()
            { Name = "ContentManagement" }
        };

        /// <summary>
        /// 配置管理
        /// </summary>
        public static readonly PermissionRecord ManageSettings = new PermissionRecord
        {
            Name = "管理后台.配置管理",
            SystemName = "ManageSettings",
            Module = new Module()
            { Name = "Configuration" }
        };

        /// <summary>
        /// 管理活动日志
        /// </summary>
        public static readonly PermissionRecord ManageActivityLog = new PermissionRecord
        {
            Name = "管理后台.管理活动日志",
            SystemName = "ManageActivityLog",
            Module = new Module()
            { Name = "Configuration" }
        };

        /// <summary>
        /// 访问控制管理
        /// </summary>
        public static readonly PermissionRecord ManageAcl = new PermissionRecord
        {
            Name = "管理后台. 访问控制管理",
            SystemName = "ManageACL",
            Module = new Module()
            { Name = "Configuration" }
        };


        /// <summary>
        /// 管理邮件账户
        /// </summary>
        public static readonly PermissionRecord ManageEmailAccounts = new PermissionRecord
        {
            Name = "管理后台.管理邮件账户",
            SystemName = "ManageEmailAccounts",
            Module = new Module()
            { Name = "Configuration" }
        };

        /// <summary>
        /// 管理系统日志
        /// </summary>
        public static readonly PermissionRecord ManageSystemLog = new PermissionRecord
        {
            Name = "管理后台.管理系统日志",
            SystemName = "ManageSystemLog",
            Module = new Module()
            { Name = "Configuration" }
        };

        /// <summary>
        /// 管理消息队列
        /// </summary>
        public static readonly PermissionRecord ManageMessageQueue = new PermissionRecord
        {
            Name = "管理后台.管理消息队列",
            SystemName = "ManageMessageQueue",
            Module = new Module()
            { Name = "Configuration" }
        };

        /// <summary>
        /// 管理维护
        /// </summary>
        public static readonly PermissionRecord ManageMaintenance = new PermissionRecord
        {
            Name = "管理后台.管理维护",
            SystemName = "ManageMaintenance",
            Module = new Module()
            { Name = "Configuration" }
        };

        /// <summary>
        /// 管理计划任务
        /// </summary>
        public static readonly PermissionRecord ManageScheduleTasks = new PermissionRecord
        {
            Name = "管理后台.管理计划任务",
            SystemName = "ManageScheduleTasks",
            Module = new Module()
            { Name = "Configuration" }
        };

        /// <summary>
        /// 产品属性
        /// </summary>
        public static readonly PermissionRecord ManageAttributes = new PermissionRecord
        {
            Name = "管理后台.管理商品属性",
            SystemName = "ManageAttributes",
            Module = new Module()
            { Name = "Product" }
        };

        #endregion

        #region 经销商系统初始权限


        public static readonly PermissionRecord PublicStoreAllowNavigation = new PermissionRecord
        {
            Name = "公共区域",
            SystemName = "PublicStoreAllowNavigation",
            Code = 9998,
            Module = new Module() { Name = "Client" }
        };

        public static readonly PermissionRecord AccessClosedStore = new PermissionRecord
        {
            Name = "访问关闭站点",
            SystemName = "AccessClosedStore",
            Code = 9999,
            Module = new Module() { Name = "Client" }
        };

        public static readonly PermissionRecord UserRoleView = new PermissionRecord
        {
            Name = "权限设置",
            SystemName = "UserRoleView",
            Code = (int)AccessGranularityEnum.UserRoleView,
            Module = new Module() { Name = "Permission" }
        };

        public static readonly PermissionRecord UserRoleAdd = new PermissionRecord
        {
            Name = "权限更改",
            SystemName = "UserRoleAdd",
            Code = (int)AccessGranularityEnum.UserRoleAdd,
            Module = new Module() { Name = "Permission" }
        };


        #endregion

        /// <summary>
        /// 获取管理平台权限
        /// </summary>
        /// <returns></returns>
        public virtual IEnumerable<PermissionRecord> GetPermissions()
        {
            return new[]
            {
                AccessAdminPanel,
                AllowUserImpersonation,
                ManageProducts,
                ManageCategories,
                ManageUsers,
                ManageUserRoles,
                ManageDistributors,
                ManageMessageTemplates,
                ManageSettings,
                ManageActivityLog,
                ManageAcl,
                ManageEmailAccounts,
                ManageSystemLog,
                ManageMessageQueue,
                ManageMaintenance,
                ManageScheduleTasks,
                ManageAttributes
            };
        }

        /// <summary>
        /// 获取经销商管理员默认权限
        /// </summary>
        /// <returns></returns>
        public virtual IEnumerable<DefaultPermissionRecord> GetStoreDefaultPermissions()
        {
            return new[]{
                new DefaultPermissionRecord
                {
                    UserRoleSystemName = DCMSDefaults.Administrators,
                    PermissionRecords = new[]
                    {
                        PublicStoreAllowNavigation,
                        AccessClosedStore
                    }
                }
            };
        }

        /// <summary>
        /// 获取角色默认权限
        /// </summary>
        /// <returns></returns>
        public virtual IEnumerable<DefaultPermissionRecord> GetDefaultPermissions()
        {
            return new[]
            {
                new DefaultPermissionRecord
                {
                    UserRoleSystemName = DCMSDefaults.Administrators,
                    PermissionRecords = new[]
                    {
                            AccessAdminPanel,
                            AllowUserImpersonation,
                            ManageProducts,
                            ManageCategories,
                            ManageUsers,
                            ManageUserRoles,
                            ManageDistributors,
                            ManageMessageTemplates,
                            ManageSettings,
                            ManageActivityLog,
                            ManageAcl,
                            ManageEmailAccounts,
                            ManageSystemLog,
                            ManageMessageQueue,
                            ManageMaintenance,
                            ManageScheduleTasks,
                            ManageAttributes
                    }
                },
                new DefaultPermissionRecord
                {
                    UserRoleSystemName = DCMSDefaults.MarketManagers,
                    PermissionRecords = new[]
                    {
                            AccessAdminPanel,
                            AllowUserImpersonation,
                            ManageProducts,
                            ManageCategories,
                            ManageUsers,
                            ManageUserRoles,
                            ManageDistributors,
                            ManageMessageTemplates,
                            ManageSettings,
                            ManageActivityLog,
                            ManageAcl,
                            ManageEmailAccounts,
                            ManageSystemLog,
                            ManageMessageQueue,
                            ManageMaintenance,
                            ManageScheduleTasks,
                            ManageAttributes
                    }
                }
            };
        }
    }
}