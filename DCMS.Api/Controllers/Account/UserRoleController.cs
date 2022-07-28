using DCMS.Core;
using DCMS.Core.Caching;
using DCMS.Core.Domain.Users;
using DCMS.Services.Authentication;
using DCMS.Services.Common;
using DCMS.Services.Logging;
using DCMS.Services.Security;
using DCMS.Services.Stores;
using DCMS.Services.Users;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using DCMS.Web.Framework.Mvc.Filters;
using Microsoft.AspNetCore.Authorization;

namespace DCMS.Api.Controllers
{

    /// <summary>
    /// 用于经销商员工管理
    /// </summary>
   [Authorize]
    [Route("api/v{version:apiVersion}/dcms/users")]
    public class UserRoleController : BaseAPIController
    {
        private readonly IAuthenticationService _authenticationService;
        private readonly IUserRegistrationService _userRegistrationService;
        private readonly IWorkContext _workContext;
        private readonly IStoreContext _storeContext;
        private readonly IUserService _userService;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly UserSettings _userSettings;
        private readonly IWebHelper _webHelper;
        private readonly IUserActivityService _userActivityService;
        
        private readonly IPermissionService _permissionService;
        private readonly IStoreService _storeService;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="workContext"></param>
        /// <param name="authenticationService"></param>
        /// <param name="userRegistrationService"></param>
        /// <param name="storeContext"></param>
        /// <param name="userService"></param>
        /// <param name="storeService"></param>
        /// <param name="genericAttributeService"></param>
        /// <param name="userSettings"></param>
        /// <param name="webHelper"></param>
        /// <param name="userActivityService"></param>
        /// <param name="cacheManager"></param>
        /// <param name="permissionService"></param>
        public UserRoleController(IWorkContext workContext,
            IAuthenticationService authenticationService,
            IUserRegistrationService userRegistrationService,
            IStoreContext storeContext,
            IUserService userService,
            IStoreService storeService,
            IGenericAttributeService genericAttributeService,
            UserSettings userSettings,
            IWebHelper webHelper,
            IUserActivityService userActivityService,
            IStaticCacheManager cacheManager,
            IPermissionService permissionService
           , ILogger<BaseAPIController> logger) : base(logger)
        {
            _workContext = workContext;
            _authenticationService = authenticationService;
            _userRegistrationService = userRegistrationService;
            _storeContext = storeContext;
            _userService = userService;
            _genericAttributeService = genericAttributeService;
            _userSettings = userSettings;
            _webHelper = webHelper;
            _userActivityService = userActivityService;
            
            _permissionService = permissionService;
            _storeService = storeService;
        }
    }
}