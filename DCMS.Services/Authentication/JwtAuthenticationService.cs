using DCMS.Core.Domain.Users;
using DCMS.Services.Users;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Security.Claims;

namespace DCMS.Services.Authentication
{
    /// <summary>
    /// 表示使用JWT中间件进行身份验证的服务
    /// </summary>
    public partial class JwtAuthenticationService : IAuthenticationService
    {
        private readonly UserSettings _userSettings;
        private readonly IUserService _userService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private User _cachedUser;

        public JwtAuthenticationService(UserSettings userSettings,
            IUserService userService,
            IHttpContextAccessor httpContextAccessor)
        {
            _userSettings = userSettings;
            _userService = userService;
            _httpContextAccessor = httpContextAccessor;
        }


        /// <summary>
        /// 登录
        /// </summary>
        /// <param name="user">用户</param>
        /// <param name="isPersistent">验证会话是否跨多个请求持久化</param>
        public virtual void SignIn(User user, bool isPersistent)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Uri, $"{user.StoreId}.jsdcms.com", ClaimValueTypes.String, DCMSAuthenticationDefaults.ClaimsIssuer)
            };


            if (!string.IsNullOrEmpty(user.Username))
            {
                claims.Add(new Claim(ClaimTypes.Name, user.Username, ClaimValueTypes.String, DCMSAuthenticationDefaults.ClaimsIssuer));
            }

            if (!string.IsNullOrEmpty(user.MobileNumber))
            {
                claims.Add(new Claim(ClaimTypes.MobilePhone, user.MobileNumber, ClaimValueTypes.String, DCMSAuthenticationDefaults.ClaimsIssuer));
            }

            if (!string.IsNullOrEmpty(user.Email))
            {
                claims.Add(new Claim(ClaimTypes.Email, user.Email, ClaimValueTypes.Email, DCMSAuthenticationDefaults.ClaimsIssuer));
            }

            //create principal for the current authentication scheme
            var userIdentity = new ClaimsIdentity(claims, DCMSAuthenticationDefaults.AuthenticationScheme);
            var userPrincipal = new ClaimsPrincipal(userIdentity);

            //sign in
            //await _httpContextAccessor.HttpContext.SignInAsync(DCMSAuthenticationDefaults.AuthenticationScheme, userPrincipal, authenticationProperties);

            //cache authenticated user
            _cachedUser = user;
        }

        /// <summary>
        /// 注销
        /// </summary>
        public virtual void SignOut()
        {
            //_cachedUser = null;
            //await _httpContextAccessor.HttpContext.SignOutAsync(DCMSAuthenticationDefaults.AuthenticationScheme);
        }

        /// <summary>
        /// 获取经过身份验证的用户
        /// </summary>
        /// <returns>User</returns>
        public virtual User GetAuthenticatedUser()
        {
            if (_cachedUser != null)
            {
                return _cachedUser;
            }

            AuthenticateResult authenticateResult = null;
            try
            {
                //try to get authenticated user identity
                authenticateResult = _httpContextAccessor?.HttpContext?.AuthenticateAsync(DCMSAuthenticationDefaults.AuthenticationScheme).Result;
                if (!(authenticateResult?.Succeeded ?? false))
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                return null;
            }

            User user = null;
            if (user == null && authenticateResult != null)
            {
                //try to get user by username
                var usernameClaim = authenticateResult.Principal.FindFirst(claim => claim.Type == ClaimTypes.Name
                    && claim.Issuer.Equals(DCMSAuthenticationDefaults.ClaimsIssuer, StringComparison.InvariantCultureIgnoreCase));
                if (usernameClaim != null)
                {
                    user = _userService.GetUserByUsername(0, usernameClaim.Value, true);
                }
            }


            if (user == null && authenticateResult != null)
            {
                //try to get user by email 
                var emailClaim = authenticateResult.Principal.FindFirst(claim => claim.Type == ClaimTypes.Email
                    && claim.Issuer.Equals(DCMSAuthenticationDefaults.ClaimsIssuer, StringComparison.InvariantCultureIgnoreCase));
                if (emailClaim != null)
                {
                    user = _userService.GetUserByEmail(0, emailClaim.Value, true);
                }
            }


            if (user == null && authenticateResult != null)
            {
                //try to get user by mobileNumber
                var mobileClaim = authenticateResult.Principal.FindFirst(claim => claim.Type == ClaimTypes.MobilePhone
                    && claim.Issuer.Equals(DCMSAuthenticationDefaults.ClaimsIssuer, StringComparison.InvariantCultureIgnoreCase));
                if (mobileClaim != null)
                {
                    user = _userService.GetUserByMobileNamber(0, mobileClaim.Value, true);
                }
            }

            if (user == null || !user.Active || user.RequireReLogin || user.Deleted) //|| !user.IsRegistered()
            {
                return null;
            }

            //cache authenticated user
            _cachedUser = user;

            return _cachedUser;
        }


    }
}