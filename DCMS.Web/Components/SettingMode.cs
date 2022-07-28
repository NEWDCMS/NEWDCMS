using DCMS.Web.Factories;
using DCMS.Web.Framework.Components;
using Microsoft.AspNetCore.Mvc;

namespace DCMS.Web.Components
{

    public class SettingModeViewComponent : DCMSViewComponent
    {

        private readonly ISettingModelFactory _settingModelFactory;
        public SettingModeViewComponent(ISettingModelFactory settingModelFactory)
        {
            _settingModelFactory = settingModelFactory;
        }



        public IViewComponentResult Invoke(string modeName = "settings-advanced-mode")
        {
            //prepare model
            var model = _settingModelFactory.PrepareSettingModeModel(modeName);

            return View(model);
        }
    }
}