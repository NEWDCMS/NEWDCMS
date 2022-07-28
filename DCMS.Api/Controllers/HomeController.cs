using Microsoft.AspNetCore.Mvc;

namespace DCMS.Api.Controllers
{
    /// <summary>
    /// 
    /// </summary>
    public class HomeController : Controller
    {
        /// <summary>
        /// Index
        /// </summary>
        /// <returns></returns>
        public ActionResult Index()
        {
            //var or = _rganizationService.GetOrganizations();
            return Redirect("/Swagger/ui/index");
        }
    }
}
