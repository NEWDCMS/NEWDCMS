using DCMS.Web.Factories;
using DCMS.Web.Framework.Components;
using Microsoft.AspNetCore.Mvc;

namespace DCMS.Web.Components
{
    public class LeftSidebarViewComponent : DCMSViewComponent
    {
        private readonly ICommonModelFactory _commonModelFactory;

        public LeftSidebarViewComponent(ICommonModelFactory commonModelFactory)
        {
            _commonModelFactory = commonModelFactory;
        }

        public IViewComponentResult Invoke()
        {
            var model = _commonModelFactory.LeftSidebar();
            return View(model);
        }
    }
}
