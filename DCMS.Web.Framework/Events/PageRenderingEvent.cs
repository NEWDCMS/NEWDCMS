using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Routing;
using System.Collections.Generic;
using System.Linq;

namespace DCMS.Web.Framework.Events
{
    /// <summary>
    /// 页面呈现事件
    /// </summary>
    public class PageRenderingEvent
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="helper"></param>
        /// <param name="overriddenRouteName"></param>
        public PageRenderingEvent(IHtmlHelper helper, string overriddenRouteName = null)
        {
            Helper = helper;
            OverriddenRouteName = overriddenRouteName;
        }

        /// <summary>
        /// HTML helper
        /// </summary>
        public IHtmlHelper Helper { get; private set; }

        /// <summary>
        /// 覆盖 route name
        /// </summary>
        public string OverriddenRouteName { get; private set; }

        public IEnumerable<string> GetRouteNames()
        {
            //如果指定了重写的路由名称，则使用它
            //当某些自定义页使用自定义路由时，我们使用它指定自定义路由名称。但是我们仍然需要调用这个事件
            if (!string.IsNullOrEmpty(OverriddenRouteName))
            {
                return new List<string>() { OverriddenRouteName };
            }

            var matchedRoutes = Helper.ViewContext.RouteData.Routers.OfType<INamedRouter>();
            return matchedRoutes.Select(r => r.Name);
        }
    }
}
