using DCMS.Core;
using DCMS.Services.Logging;
using DCMS.Services.Messages;
using DCMS.Services.Settings;
using DCMS.Web.Framework.Mvc.Filters;
using DCMS.Web.Framework.Security;
using Microsoft.AspNetCore.Mvc;

namespace DCMS.Web.Controllers
{
    public partial class HomeController : BasePublicController
    {
        private IAccountingService _accountingService;
        public HomeController(
           INotificationService notificationService,
           IStoreContext storeContext,
           ILogger loggerService,
           IAccountingService accountingService,
           IWorkContext workContext) : base(workContext, loggerService, storeContext, notificationService)
        {
            _accountingService = accountingService;
        }

        [HttpsRequirement(SslRequirement.No)]
        //[CheckAccessPublicStore(true)]
        public virtual IActionResult Index()
        {
            return View();
        }

    }
}


