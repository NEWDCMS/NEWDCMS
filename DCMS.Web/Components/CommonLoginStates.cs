using DCMS.Web.Factories;
using DCMS.Web.Framework.Components;
using Microsoft.AspNetCore.Mvc;

namespace DCMS.Web.Components
{
    public class LoginStatesViewComponent : DCMSViewComponent
    {
        private readonly ICommonModelFactory _commonModelFactory;

        public LoginStatesViewComponent(ICommonModelFactory commonModelFactory)
        {
            _commonModelFactory = commonModelFactory;
        }

        public IViewComponentResult Invoke()
        {
            var model = _commonModelFactory.LoginStates();
            return View(model);
        }
    }
}
