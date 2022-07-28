using Microsoft.AspNetCore.Routing;

namespace DCMS.Web.Framework.Mvc.Routing
{
    /// <summary>
    /// 3.1
    /// </summary>
    public interface IRouteProvider
    {
        /// <summary>
        /// Register routes
        /// </summary>
        /// <param name="endpointRouteBuilder">Route builder</param>
        void RegisterRoutes(IEndpointRouteBuilder endpointRouteBuilder);

        /// <summary>
        /// Gets a priority of route provider
        /// </summary>
        int Priority { get; }
    }

    //2.2
    //public interface IRouteProvider
    //{
    //    /// <summary>
    //    /// Register routes
    //    /// </summary>
    //    /// <param name="routeBuilder">Route builder</param>
    //    void RegisterRoutes(IRouteBuilder routeBuilder);

    //    /// <summary>
    //    /// Gets a priority of route provider
    //    /// </summary>
    //    int Priority { get; }
    //}
}
