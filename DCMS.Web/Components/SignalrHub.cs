using DCMS.Web.Factories;
using DCMS.Web.Framework.Components;
using Microsoft.AspNetCore.Mvc;


namespace DCMS.Web.Components
{
    public class SignalrHubViewComponent : DCMSViewComponent
    {
        private readonly ICommonModelFactory _commonModelFactory;

        public SignalrHubViewComponent(ICommonModelFactory commonModelFactory)
        {
            _commonModelFactory = commonModelFactory;
        }

        public IViewComponentResult Invoke()
        {

            return View();
        }
    }
}
