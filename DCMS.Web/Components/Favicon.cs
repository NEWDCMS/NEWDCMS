using DCMS.Web.Factories;
using DCMS.Web.Framework.Components;
using Microsoft.AspNetCore.Mvc;

namespace DCMS.Web.Components
{
    public class FaviconViewComponent : DCMSViewComponent
    {
        private readonly ICommonModelFactory _commonModelFactory;

        public FaviconViewComponent(ICommonModelFactory commonModelFactory)
        {
            _commonModelFactory = commonModelFactory;
        }

        public IViewComponentResult Invoke()
        {
            var model = _commonModelFactory.PrepareFaviconAndAppIconsModel();
            if (string.IsNullOrEmpty(model.HeadCode))
            {
                return Content("");
            }

            return View(model);
        }
    }
}
