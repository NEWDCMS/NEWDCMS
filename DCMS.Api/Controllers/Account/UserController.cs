using DCMS.Api.Infrastructure.Mapper.Extensions;
using DCMS.Core;
using DCMS.Services.Users;
using DCMS.ViewModel.Models.Users;
using DCMS.Web.Framework.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Swashbuckle.AspNetCore.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace DCMS.Api.Controllers
{
    /// <summary>
    /// 用于经销商员工管理
    /// </summary>
    [Authorize]
    [Route("api/v{version:apiVersion}/dcms/users")]
    public class UserController : BaseAPIController
    {
        private readonly IUserService _userService;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="userService"></param>

        public UserController(IUserService userService, ILogger<BaseAPIController> logger) : base(logger)
        {
            _userService = userService;
        }

        /// <summary>
        /// 根据角色获取经销商用户
        /// </summary>
        /// <param name="store"></param>
        /// <param name="roleName"></param>
        /// <param name="ids"></param>
        /// <returns></returns>
        [HttpGet("getBusinessUsers/{store}")]
        [SwaggerOperation("getBusinessUsers")]
        public async Task<APIResult<IList<BusinessUserModel>>> GetBusinessUsers(int? store, [FromQuery] int[] ids, string roleName = "")
        {
            if (!store.HasValue || store.Value == 0)
                return this.Error<BusinessUserModel>(Resources.ParameterError);

            return await Task.Run(() =>
            {
                try
                {
                    var model = new List<BusinessUserModel>();
                    model = _userService.GetUserBySystemRoleName(store ?? 0, roleName).Select(u => u.ToModel<BusinessUserModel>()).ToList();
                    return this.Successful(Resources.Successful, model);
                }
                catch (Exception ex)
                {
                    return this.Error<BusinessUserModel>(ex.Message);
                }
            });
        }


        /// <summary>
        /// 获取下级用户
        /// </summary>
        /// <param name="store"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        [HttpGet("getSubUsers/{store}")]
        [SwaggerOperation("getSubUsers")]
        public async Task<APIResult<IList<BusinessUserModel>>> GetSubUsers(int? store, int userId)
        {
            if (!store.HasValue || store.Value == 0)
                return this.Error<BusinessUserModel>(Resources.ParameterError);

            return await Task.Run(() =>
            {
                try
                {
                    var model = new List<BusinessUserModel>();
                    var user = _userService.GetUserById(userId);
                    model = _userService.GetAllSubordinateUser(store ?? 0, user.Subordinates,user.IsSystemAccount).Select(u => u.ToModel<BusinessUserModel>()).ToList();
                    model.Add(user.ToModel<BusinessUserModel>());
                    //model = _userService.GetSubordinateUsers(store ?? 0, userId).Select(u => u.ToModel<BusinessUserModel>()).ToList();
                    return this.Successful(Resources.Successful, model);
                }
                catch (Exception ex)
                {
                    return this.Error<BusinessUserModel>(ex.Message);
                }
            });
        }
    }
}