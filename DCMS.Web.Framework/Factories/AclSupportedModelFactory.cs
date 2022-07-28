using DCMS.Core;
using DCMS.Core.Domain.Security;
using DCMS.Services.Security;
using DCMS.Services.Users;
using DCMS.Web.Framework.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Linq;

namespace DCMS.Web.Framework.Factories
{
    /// <summary>
    /// 表示支持访问控制列表（ACL）的模型工厂的基本实现
    /// </summary>
    public partial class AclSupportedModelFactory : IAclSupportedModelFactory
    {


        private readonly IAclService _aclService;
        private readonly IUserService _userService;


        public AclSupportedModelFactory(IAclService aclService,
            IUserService userService)
        {
            _aclService = aclService;
            _userService = userService;
        }



        /// <summary>
        /// 为传递的模型选定的和所有可用的用户角色
        /// </summary>
        /// <typeparam name="TModel">ACL supported model type</typeparam>
        /// <param name="model">Model</param>
        public virtual void PrepareModelUserRoles<TModel>(TModel model) where TModel : IAclSupportedModel
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            //有效角色
            var availableRoles = _userService.GetAllUserRoles(showHidden: true);
            model.AvailableUserRoles = availableRoles.Select(role => new SelectListItem
            {
                Text = role.Name,
                Value = role.Id.ToString(),
                Selected = model.SelectedUserRoleIds.Contains(role.Id)
            }).ToList();
        }

        /// <summary>
        /// 为通过ACL映射传递的模型选定的和所有可用的用户角色
        /// </summary>
        /// <typeparam name="TModel">ACL supported model type</typeparam>
        /// <typeparam name="TEntity">ACL supported entity type</typeparam>
        /// <param name="model">Model</param>
        /// <param name="entity">Entity</param>
        /// <param name="ignoreAclMappings">Whether to ignore existing ACL mappings</param>
        public virtual void PrepareModelUserRoles<TModel, TEntity>(TModel model, TEntity entity, bool ignoreAclMappings)
            where TModel : IAclSupportedModel where TEntity : BaseEntity, IAclSupported
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            //具有授权访问权限的用户角色
            if (!ignoreAclMappings && entity != null)
            {
                model.SelectedUserRoleIds = _aclService.GetUserRoleIdsWithAccess(entity).ToList();
            }

            PrepareModelUserRoles(model);
        }


    }
}