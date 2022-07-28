using DCMS.Core;
using DCMS.Core.Domain.Users;
using DCMS.Core.Http;
using DCMS.Services.Authentication;
using DCMS.Services.Common;
using DCMS.Services.Helpers;
using DCMS.Services.Users;
using Microsoft.AspNetCore.Http;
using System;

namespace DCMS.Web.Framework
{
    /// <summary>
    /// 工作上下文
    /// </summary>
    public partial class WebWorkContext : IWorkContext
    {
        private readonly IAuthenticationService _authenticationService;
        private readonly IUserService _userService;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IUserAgentHelper _userAgentHelper;

        private User _cachedUser;
        private User _originalUserIfImpersonated;

        public WebWorkContext(
            IAuthenticationService authenticationService,
            IUserService userService,
            IGenericAttributeService genericAttributeService,
            IHttpContextAccessor httpContextAccessor,
            IUserAgentHelper userAgentHelper)
        {
            _authenticationService = authenticationService;
            _userService = userService;
            _genericAttributeService = genericAttributeService;
            _httpContextAccessor = httpContextAccessor;
            _userAgentHelper = userAgentHelper;
        }


        /// <summary>
        /// 获取用户cookie
        /// </summary>
        /// <returns></returns>
        protected virtual string GetUserCookie()
        {
            var cookieName = $"{DCMSCookieDefaults.Prefix}{DCMSCookieDefaults.UserStoreCookie}{DCMSCookieDefaults.UserCookie}";
            return _httpContextAccessor.HttpContext?.Request?.Cookies[cookieName];
        }

        /// <summary>
        /// 设置用户cookie
        /// </summary>
        /// <param name="userGuid"></param>
        protected virtual void SetUserCookie(Guid userGuid, int store)
        {
            if (_httpContextAccessor.HttpContext?.Response == null)
            {
                return;
            }

            //删除当前cookie值
            var cookieName = $"{DCMSCookieDefaults.Prefix}{DCMSCookieDefaults.UserStoreCookie}{DCMSCookieDefaults.UserCookie}";
            _httpContextAccessor.HttpContext.Response.Cookies.Delete(cookieName);
            var storeCookieName = $"{DCMSCookieDefaults.Prefix}{DCMSCookieDefaults.UserStoreCookie}";

            //获取cookie过期日期
            var cookieExpires = 24 * 365; //TODO make configurable
            var cookieExpiresDate = DateTime.Now.AddHours(cookieExpires);

            //如果传递的guid为空，则将cookie设置为expired
            if (userGuid == Guid.Empty)
            {
                cookieExpiresDate = DateTime.Now.AddMonths(-1);
            }

            //设置新的cookie值
            var options = new CookieOptions
            {
                HttpOnly = true,
                Expires = cookieExpiresDate
            };

            //追加DCMS.Store
            _httpContextAccessor.HttpContext.Response.Cookies.Append(storeCookieName, store.ToString(), options);
            //追加DCMS.User
            _httpContextAccessor.HttpContext.Response.Cookies.Append(cookieName, userGuid.ToString(), options);

        }


        /// <summary>
        /// 获取当前用户
        /// </summary>
        public virtual User CurrentUser
        {
            get
            {
                //是否有缓存值
                if (_cachedUser != null)
                {
                    return _cachedUser;
                }

                User user = null;

                ////检查请求是否由后台（计划）任务发出
                //if (_httpContextAccessor.HttpContext == null ||
                //    _httpContextAccessor.HttpContext.Request.Path.Equals(new PathString($"/{DCMSTaskDefaults.ScheduleTaskPath}"), StringComparison.InvariantCultureIgnoreCase))
                //{
                //    //返回后台任务的内置用户记录
                //    user = _userService.GetUserBySystemName(0, DCMSTaskDefaults.BackgroundTaskUserName, true);
                //}

                //if (user == null || user.Deleted || !user.Active || user.RequireReLogin)
                //{
                //    //检查请求是否由搜索引擎发出，在这种情况下，返回搜索引擎的内置用户记录
                //    //if (_userAgentHelper.IsSearchEngine())
                //    //    user = _userService.GetUserBySystemName(DCMSTaskDefaults.SearchEngineUserName);
                //}

                if (user == null || user.Deleted || !user.Active || user.RequireReLogin)
                {
                    //尝试获取注册用户
                    user = _authenticationService.GetAuthenticatedUser();
                }

                //if (user != null && !user.Deleted && user.Active && !user.RequireReLogin)
                //{
            
                //}

                if (user == null || user.Deleted || !user.Active || user.RequireReLogin)
                {
                    //获取来宾用户
                    var userCookie = GetUserCookie();
                    if (!string.IsNullOrEmpty(userCookie))
                    {
                        if (Guid.TryParse(userCookie, out Guid userGuid))
                        {
                            //从cookie获取用户
                            var userByCookie = _userService.GetUserByGuid(user?.StoreId, userGuid);
                            if (userByCookie != null && _userService.IsRegistered(userByCookie))
                            {
                                user = userByCookie;
                            }
                        }
                    }
                }

                //不启用来宾
                if (user == null || user.Deleted || !user.Active || user.RequireReLogin)
                {
                    //如果不存在则创建来宾客户
                    //user = _userService.InsertGuestUser();
                    return null;
                }

                if (!user.Deleted && user.Active && !user.RequireReLogin)
                {
                    //设置用户cookie
                    SetUserCookie(Guid.Parse(user.UserGuid), user.StoreId);

                    //缓存找到的用户
                    _cachedUser = user;
                }



                return _cachedUser;
            }
            set
            {
                SetUserCookie(Guid.Parse(value.UserGuid), value.StoreId);
                _cachedUser = value;
            }
        }

        /// <summary>
        /// 获取原始用户（如果模拟了当前用户）
        /// </summary>
        public virtual User OriginalUserIfImpersonated
        {
            get { return _originalUserIfImpersonated; }
        }

        /// <summary>
        /// 是否具有管理域控
        /// </summary>
        public virtual bool IsAdmin { get; set; }


    }
}