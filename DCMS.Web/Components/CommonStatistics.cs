using DCMS.Core;
using DCMS.Services.Security;
using DCMS.Web.Factories;
using DCMS.Web.Framework.Components;
using Microsoft.AspNetCore.Mvc;

namespace DCMS.Web.Components
{

    public class StatisticsViewComponent : DCMSViewComponent
    {

        private readonly ICommonModelFactory _commonModelFactory;
        private readonly IPermissionService _permissionService;
        private readonly IWorkContext _workContext;


        public StatisticsViewComponent(ICommonModelFactory commonModelFactory,
            IPermissionService permissionService,
            IWorkContext workContext)
        {
            _commonModelFactory = commonModelFactory;
            _permissionService = permissionService;
            _workContext = workContext;
        }


        public IViewComponentResult Invoke()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageUsers) ||
                !_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
            {
                return Content(string.Empty);
            }

            //prepare model
            var model = _commonModelFactory.PrepareCommonStatisticsModel();

            return View(model);
        }
    }
}