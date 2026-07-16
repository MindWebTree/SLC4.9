
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Nop.Web.Framework;
using Nop.Web.Framework.Mvc.Routing;

namespace MWT.Plugin.Shipping.FixedByWeightByTotal.Infrastructure
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
            endpointRouteBuilder.MapControllerRoute("Plugin.MWT.Shipping.FixedByWeightByTotal.ManageExpectedDeliveryDates", "Admin/Shipping/ManageExpectedDeliveryDates",
                new { controller = "MWTFixedByWeightByTotal", action = "ManageExpectedDeliveryDates", area = AreaNames.ADMIN });
        }

        /// <summary>
        /// Gets a priority of route provider
        /// </summary>
        public int Priority => 1; //set a value that is greater than the default one in Nop.Web to override routes
    }
}