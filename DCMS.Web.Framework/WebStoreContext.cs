using DCMS.Core;
using DCMS.Core.Configuration;
using DCMS.Core.Domain.Stores;
using DCMS.Core.Http;
using DCMS.Core.Infrastructure;
using DCMS.Services.Common;
using DCMS.Services.Stores;
using Microsoft.AspNetCore.Http;
using Microsoft.Net.Http.Headers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;

namespace DCMS.Web.Framework
{
    /// <summary>
    /// 表示当前经销商站点上下文
    /// </summary>
    public partial class WebStoreContext : IStoreContext
    {

        private readonly IGenericAttributeService _genericAttributeService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IStoreService _storeService;
        private readonly DCMSConfig _config;

        private Store _cachedStore;
        private int? _cachedActiveStoreScopeConfiguration;


        /// <summary>
        /// 
        /// </summary>
        /// <param name="genericAttributeService"></param>
        /// <param name="httpContextAccessor"></param>
        /// <param name="storeService"></param>
        public WebStoreContext(IGenericAttributeService genericAttributeService,
            IHttpContextAccessor httpContextAccessor,
            DCMSConfig config,
            IStoreService storeService)
        {
            _genericAttributeService = genericAttributeService;
            _httpContextAccessor = httpContextAccessor;
            _storeService = storeService;
            _config = config;
        }

        /// <summary>
        /// 获取当前经销商站点
        /// </summary>
        public virtual Store CurrentStore
        {
            get
            {
                var cookies = _httpContextAccessor.HttpContext?.Request?.Cookies;
                string host = _httpContextAccessor.HttpContext?.Request?.Headers[HeaderNames.Host];
                string cookie = _httpContextAccessor.HttpContext?.Request?.Headers[HeaderNames.Cookie];
                string requestStore = _httpContextAccessor.HttpContext?.Request?.Query["store"];

                Store store = null;
                int storeId = 0;

                if (_config?.ManageStoreCode == "SYSTEM")
                {
                    store = _storeService.GetManageStore();
                    _cachedStore = store;
                    return _cachedStore;
                }
                else
                {

                    if (!string.IsNullOrEmpty(requestStore))
                    {
                        int.TryParse(requestStore, out storeId);

                        if (storeId == 0)
                        {
                            store = null;
                        }
                        else
                        {
                            store = _storeService.GetStoreById(storeId);
                        }

                        if (store != null)
                        {
                            _cachedStore = store;
                            return _cachedStore;
                        }
                    }


                    if (_httpContextAccessor.HttpContext != null && _httpContextAccessor.HttpContext.User != null)
                    {

                        var authenticateResult = _httpContextAccessor.HttpContext.User.Identity;
                        if (authenticateResult.IsAuthenticated)
                        {

                            var storeClaim = _httpContextAccessor.HttpContext.User.Claims.ToList().FirstOrDefault(claim => claim.Type == ClaimTypes.Uri
                        && claim.Issuer.Equals("jsdcms", StringComparison.InvariantCultureIgnoreCase));
                            if (storeClaim != null)
                            {
                                int.TryParse(storeClaim.Value.Replace(".jsdcms.com", ""), out storeId);
                            }

                            if (storeId == 0)
                            {
                                store = null;
                            }
                            else
                            {
                                store = _storeService.GetStoreById(storeId);
                            }

                            if (store != null)
                            {
                                _cachedStore = store;
                                return _cachedStore;
                            }
                        }
                    }

                    //Cookies //DCMS.user .DCMS.Antiforgery .DCMS.Authentication .DCMS.User
                    if (cookies != null)
                    {
                        var cookieName = $"{DCMSCookieDefaults.Prefix}{DCMSCookieDefaults.UserStoreCookie}";
                        int.TryParse(cookies[cookieName], out storeId);

                        if (storeId == 0)
                        {
                            store = null;
                        }
                        else
                        {
                            store = _storeService.GetStoreById(storeId);
                        }

                        if (store != null)
                        {
                            _cachedStore = store;
                            return _cachedStore;
                        }

                        //
                        int.TryParse(cookies[$"{DCMSCookieDefaults.Prefix}{DCMSCookieDefaults.UserCookie}"], out int userId);
                        if (store == null || storeId == 0)
                        {
                            store = _storeService.GetStoreByUserId(userId);
                            _cachedStore = store;
                            return _cachedStore;
                        }
                    }

                }

                return _cachedStore;
            }
        }


        public virtual IList<Store> Stores => _storeService.GetAllStores(true) ?? new List<Store>();


        /// <summary>
        /// 获取有效经销商站点域配置 
        /// </summary>
        public virtual int ActiveStoreScopeConfiguration
        {
            get
            {
                if (_cachedActiveStoreScopeConfiguration.HasValue)
                {
                    return _cachedActiveStoreScopeConfiguration.Value;
                }

                if (_storeService.GetAllStores(true).Count > 1)
                {
                    //不要通过构造函数注入IWorkContext，因为它会导致循环引用
                    var currentUser = EngineContext.Current.Resolve<IWorkContext>().CurrentUser;

                    //尝试从属性中获取经销商标识符
                    var storeId = _genericAttributeService
                        .GetAttribute<int>(currentUser, DCMSDefaults.AdminAreaStoreScopeConfigurationAttribute);

                    _cachedActiveStoreScopeConfiguration = _storeService.GetStoreById(storeId)?.Id ?? 0;
                }
                else
                {
                    _cachedActiveStoreScopeConfiguration = 0;
                }

                return _cachedActiveStoreScopeConfiguration ?? 0;
            }
        }
    }
}