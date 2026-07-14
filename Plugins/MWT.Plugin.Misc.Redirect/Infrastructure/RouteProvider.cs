    
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Nop.Web.Framework.Mvc.Routing;

namespace Nop.Plugin.Misc.Redirect.Infrastructure
{
    /// <summary>
    /// Represents plugin route provider
    /// </summary>
    public class RouteProvider : IRouteProvider
    {
        /// <summary>
        /// Register routes
        /// </summary>
        /// <param name="endpointRouteBuilder">Route builder</param>
        public void RegisterRoutes(IEndpointRouteBuilder endpointRouteBuilder)
        {
            endpointRouteBuilder.MapControllerRoute("Redirections", "Admin/Redirect/GetRedirections/",
            new { controller = "Redirect", action = "GetRedirections" });
            endpointRouteBuilder.MapControllerRoute("Redirections", "Admin/Redirect/RedirectUpdate/",
            new { controller = "Redirect", action = "RedirectUpdate" });
            endpointRouteBuilder.MapControllerRoute("Redirections", "Admin/Redirect/RedirectRemove/",
            new { controller = "Redirect", action = "RedirectRemove" });

        }

        /// <summary>
        /// Gets a priority of route provider
        /// </summary>
        public int Priority => 0;
    }
}
