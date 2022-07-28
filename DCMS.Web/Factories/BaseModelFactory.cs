using DCMS.Core;
using DCMS.Core.Caching;
using DCMS.Core.Domain.Logging;
using DCMS.Core.Domain.Stores;
using DCMS.Core.Domain.Users;
using DCMS.Services;
using DCMS.Services.Helpers;
using DCMS.Services.Logging;
using DCMS.Services.Stores;
using DCMS.Services.Users;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;


namespace DCMS.Web.Factories
{

    public partial class BaseModelFactory : IBaseModelFactory
    {

        protected readonly IWorkContext _workContext;
        protected readonly IStoreContext _storeContext;
        private readonly IUserActivityService _UserActivityService;
        private readonly IUserService _UserService;
        private readonly IDateTimeHelper _dateTimeHelper;
        //private readonly IEmailAccountService _emailAccountService;
        
        private readonly IStoreService _storeService;

        public Store curStore => _storeContext.CurrentStore;
        public User user => _workContext.CurrentUser;


        public BaseModelFactory(
            IWorkContext workContext,
            IStoreContext storeContext,
            IUserActivityService UserActivityService,
            IUserService UserService,
            IDateTimeHelper dateTimeHelper,
            //IEmailAccountService emailAccountService,
            IStaticCacheManager cacheManager,
            IStoreService storeService
          )
        {
            _workContext = workContext;
            _storeContext = storeContext;
            _UserActivityService = UserActivityService;
            _UserService = UserService;
            _dateTimeHelper = dateTimeHelper;
            //_emailAccountService = emailAccountService;
            
            _storeService = storeService;

        }

        protected virtual void PrepareDefaultItem(IList<SelectListItem> items, bool withSpecialDefaultItem, string defaultItemText = null)
        {
            if (items == null)
            {
                throw new ArgumentNullException(nameof(items));
            }

            //whether to insert the first special item for the default value
            if (!withSpecialDefaultItem)
            {
                return;
            }

            //at now we use "0" as the default value
            const string value = "0";

            //prepare item text
            defaultItemText = defaultItemText ?? "All";

            //insert this default item at first
            items.Insert(0, new SelectListItem { Text = defaultItemText, Value = value });
        }



        /// <summary>
        /// 
        /// </summary>
        /// <param name="items"></param>
        /// <param name="withSpecialDefaultItem"></param>
        /// <param name="defaultItemText"></param>
        public virtual void PrepareActivityLogTypes(IList<SelectListItem> items, bool withSpecialDefaultItem = true, string defaultItemText = null)
        {
            if (items == null)
            {
                throw new ArgumentNullException(nameof(items));
            }

            //prepare available activity log types
            var availableActivityTypes = _UserActivityService.GetAllActivityTypes(0);
            foreach (var activityType in availableActivityTypes)
            {
                items.Add(new SelectListItem { Value = activityType.Id.ToString(), Text = activityType.Name });
            }

            //insert special item for the default value
            PrepareDefaultItem(items, withSpecialDefaultItem, defaultItemText);
        }

        public virtual void PrepareStores(IList<SelectListItem> items, bool withSpecialDefaultItem = true, string defaultItemText = null)
        {
            if (items == null)
            {
                throw new ArgumentNullException(nameof(items));
            }

            //prepare available stores
            var availableStores = _storeService.GetAllStores(true);
            foreach (var store in availableStores)
            {
                items.Add(new SelectListItem { Value = store.Id.ToString(), Text = store.Name });
            }

            //insert special item for the default value
            PrepareDefaultItem(items, withSpecialDefaultItem, defaultItemText);
        }

        public virtual void PrepareUserRoles(IList<SelectListItem> items, bool withSpecialDefaultItem = true, string defaultItemText = null)
        {
            if (items == null)
            {
                throw new ArgumentNullException(nameof(items));
            }

            //prepare available User roles
            var availableUserRoles = _UserService.GetAllUserRoles();
            foreach (var UserRole in availableUserRoles)
            {
                items.Add(new SelectListItem { Value = UserRole.Id.ToString(), Text = UserRole.Name });
            }

            //insert special item for the default value
            PrepareDefaultItem(items, withSpecialDefaultItem, defaultItemText);
        }
        public virtual void PrepareEmailAccounts(IList<SelectListItem> items, bool withSpecialDefaultItem = true, string defaultItemText = null)
        {
            if (items == null)
            {
                throw new ArgumentNullException(nameof(items));
            }

            //prepare available email accounts
            //var availableEmailAccounts = _emailAccountService.GetAllEmailAccounts();
            //foreach (var emailAccount in availableEmailAccounts)
            //{
            //    items.Add(new SelectListItem { Value = emailAccount.Id.ToString(), Text = $"{emailAccount.DisplayName} ({emailAccount.Email})"/>;
            //}

            //insert special item for the default value
            PrepareDefaultItem(items, withSpecialDefaultItem, defaultItemText);
        }


        public virtual void PrepareLogLevels(IList<SelectListItem> items, bool withSpecialDefaultItem = true, string defaultItemText = null)
        {
            if (items == null)
            {
                throw new ArgumentNullException(nameof(items));
            }

            //prepare available log levels
            var availableLogLevelItems = LogLevel.Debug.ToSelectList(false);
            foreach (var logLevelItem in availableLogLevelItems)
            {
                items.Add(logLevelItem);
            }

            //insert special item for the default value
            PrepareDefaultItem(items, withSpecialDefaultItem, defaultItemText);
        }

    }
}