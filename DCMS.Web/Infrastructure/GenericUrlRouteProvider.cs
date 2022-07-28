using DCMS.Web.Framework.Mvc.Routing;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace DCMS.Web.Infrastructure
{
    /// <summary>
    ///  提供通用路由
    /// </summary>
    public partial class GenericUrlRouteProvider : IRouteProvider
    {

        ///// <summary>
        ///// 注册路由
        ///// </summary>
        ///// <param name="routeBuilder">Route builder</param>
        //public void RegisterRoutes(IRouteBuilder routeBuilder)
        //{
        //    //and default one
        //    routeBuilder.MapRoute("Default", "{controller}/{action}/{id?}");
        //    //generic URLs
        //    //routeBuilder.MapRoute("GenericUrl", "{GenericSeName}",new { controller = "Common", action = "GenericUrl");
        //}

        /// <summary>
        /// 注册路由
        /// </summary>
        /// <param name="endpointRouteBuilder"></param>
        public void RegisterRoutes(IEndpointRouteBuilder endpointRouteBuilder)
        {
            //var pattern = "{SeName}";

            //endpointRouteBuilder.MapDynamicControllerRoute<SlugRouteTransformer>(pattern);

            //and default one
            endpointRouteBuilder.MapControllerRoute(
                name: "Default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            //generic URLs
            endpointRouteBuilder.MapControllerRoute(
                name: "GenericUrl",
                pattern: "{GenericSeName}",
                new { controller = "Common", action = "GenericUrl" });

            //define this routes to use in UI views (in case if you want to customize some of them later)
            //endpointRouteBuilder.MapControllerRoute("Product", pattern,
            //    new { controller = "Product", action = "ProductDetails" });

        }

        /// <summary>
        /// 优先级
        /// </summary>
        public int Priority
        {
            get { return -1000000; }
        }
    }
}
