using DCMS.Services.Configuration;
using DCMS.Services.Stores;
using DCMS.Web.Framework.Components;
using Microsoft.AspNetCore.Mvc;

namespace DCMS.Web.Components
{
    public class AclDisabledWarningViewComponent : DCMSViewComponent
    {

        private readonly ISettingService _settingService;
        private readonly IStoreService _storeService;

        public AclDisabledWarningViewComponent(
            ISettingService settingService,
            IStoreService storeService)
        {
            _settingService = settingService;
            _storeService = storeService;
        }

        public IViewComponentResult Invoke()
        {
            return View();
        }
    }
}
