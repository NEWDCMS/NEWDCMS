using DCMS.Web.Framework.Mvc.Routing;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace DCMS.Web.Infrastructure
{
    /// <summary>
    /// 路由提供基类
    /// </summary>
    public partial class RouteProvider : IRouteProvider
    {
        /// <summary>
        /// 注册路由 3.1
        /// </summary>
        /// <param name="endpointRouteBuilder"></param>
        public void RegisterRoutes(IEndpointRouteBuilder endpointRouteBuilder)
        {

            endpointRouteBuilder.MapControllerRoute("Root", "/",
            new { controller = "Home", action = "Index" });

            //home page
            endpointRouteBuilder.MapControllerRoute("Homepage", "",
                new { controller = "Home", action = "Index" });

            //login
            endpointRouteBuilder.MapControllerRoute("Login", "login/",
                new { controller = "Account", action = "Login" });

            //register
            endpointRouteBuilder.MapControllerRoute("Register", "register/",
                new { controller = "Account", action = "Register" });

            //logout
            endpointRouteBuilder.MapControllerRoute("Logout", "logout/",
                new { controller = "Account", action = "Logout" });

            //error page
            endpointRouteBuilder.MapControllerRoute("Error", "error",
                new { controller = "Common", action = "Error" });

            //page not found
            endpointRouteBuilder.MapControllerRoute("PageNotFound", "page-not-found",
                new { controller = "Common", action = "PageNotFound" });

            //changepassword
            endpointRouteBuilder.MapControllerRoute("CustomerChangePassword", "user/changepassword",
                new { controller = "User", action = "ChangePassword" });

        }


        /// <summary>
        /// 优先级
        /// </summary>
        public int Priority
        {
            get { return 0; }
        }


    }
}
