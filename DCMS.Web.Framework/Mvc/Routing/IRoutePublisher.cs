using Microsoft.AspNetCore.Routing;

namespace DCMS.Web.Framework.Mvc.Routing
{
    /// <summary>
    /// 3.1
    /// </summary>
    public interface IRoutePublisher
    {
        /// <summary>
        /// Register routes
        /// </summary>
        /// <param name="endpointRouteBuilder">Route builder</param>
        void RegisterRoutes(IEndpointRouteBuilder endpointRouteBuilder);
    }

    //2.2
    //public interface IRoutePublisher
    //{
    //    /// <summary>
    //    /// Register routes
    //    /// </summary>
    //    /// <param name="routeBuilder">Route builder</param>
    //    void RegisterRoutes(IRouteBuilder routeBuilder);
    //}
}
