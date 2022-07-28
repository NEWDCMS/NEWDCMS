using DCMS.Core;
using DCMS.Core.Caching;
using DCMS.Core.Domain.Common;
using DCMS.Core.Domain.Stores;
using DCMS.Core.Domain.Users;
using DCMS.Services.Common;
using DCMS.Services.Configuration;
using DCMS.ViewModel.Models.Home;
using System;


namespace DCMS.Web.Factories
{

    public partial class HomeModelFactory : IHomeModelFactory
    {
        private readonly IStoreContext _storeContext;
        private readonly AdminAreaSettings _adminAreaSettings;
        private readonly ISettingService _settingService;
        
        private readonly IWorkContext _workContext;
        private readonly DCMSHttpClient _dcmsHttpClient;

        public Store curStore => _storeContext.CurrentStore;
        public User user => _workContext.CurrentUser;


        public HomeModelFactory(AdminAreaSettings adminAreaSettings,
            ISettingService settingService,
            IStaticCacheManager cacheManager,
            IWorkContext workContext,
            IStoreContext storeContext,
            DCMSHttpClient dcmsHttpClient)
        {
            _adminAreaSettings = adminAreaSettings;
            _settingService = settingService;
            
            _workContext = workContext;
            _dcmsHttpClient = dcmsHttpClient;
            _storeContext = storeContext;
        }

        public virtual DashboardModel PrepareDashboardModel(DashboardModel model)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            return model;
        }
    }
}