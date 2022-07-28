using Microsoft.AspNetCore.Mvc;

namespace DCMS.Web.Controllers
{
    public partial class KeepAliveController : Controller
    {
        /// <summary>
        /// 请求保持活动页面用以自我激活（禁止删除）
        /// </summary>
        /// <returns></returns>
        public virtual IActionResult Index()
        {
            return Content("I am alive!");
        }
    }
}