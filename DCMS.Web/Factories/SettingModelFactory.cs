using DCMS.Core;
using DCMS.Core.Caching;
using DCMS.Core.Domain.Common;
using DCMS.Services.Common;
using DCMS.Services.Configuration;
using DCMS.ViewModel.Models.Configuration;


namespace DCMS.Web.Factories
{

    public partial class SettingModelFactory : ISettingModelFactory
    {

        private readonly AdminAreaSettings _adminAreaSettings;
        private readonly ISettingService _settingService;
        
        private readonly IWorkContext _workContext;
        private readonly DCMSHttpClient _nopHttpClient;
        private readonly IGenericAttributeService _genericAttributeService;

        public SettingModelFactory(AdminAreaSettings adminAreaSettings,
            ISettingService settingService,
            IStaticCacheManager cacheManager,
            IWorkContext workContext,
            IGenericAttributeService genericAttributeService,
            DCMSHttpClient nopHttpClient)
        {
            _adminAreaSettings = adminAreaSettings;
            _settingService = settingService;
            
            _workContext = workContext;
            _nopHttpClient = nopHttpClient;
            _genericAttributeService = genericAttributeService;
        }



        public virtual SettingModeModel PrepareSettingModeModel(string modeName)
        {
            var model = new SettingModeModel
            {
                ModeName = modeName,
                Enabled = _genericAttributeService.GetAttribute<bool>(_workContext.CurrentUser, modeName)
            };

            return model;
        }

    }
}