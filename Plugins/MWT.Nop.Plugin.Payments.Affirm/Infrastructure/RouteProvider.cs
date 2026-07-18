using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Nop.Web.Framework.Mvc.Routing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MWT.Nop.Plugin.Payments.Affirm
{
    public class RouteProvider : IRouteProvider
    {
        /// <summary>
        /// Register routes
        /// </summary>
        /// <param name="endpointRouteBuilder">Route builder</param>
        public void RegisterRoutes(IEndpointRouteBuilder endpointRouteBuilder)
        {
            endpointRouteBuilder.MapControllerRoute(AffirmCheckoutDefaults.ConfirmCallbackHandlerRoute,
                "Admin/Affirm/ConfirmCallbackHandler",
                new { controller = "Affirm", action = "ConfirmCallbackHandler" });

            endpointRouteBuilder.MapControllerRoute(AffirmCheckoutDefaults.CancelCallbackHandlerRoute,
               "Admin/Affirm/CancelCallbackHandler",
                new { controller = "Affirm", action = "CancelCallbackHandler" });


            endpointRouteBuilder.MapControllerRoute(AffirmCheckoutDefaults.CustomConfirmCallbackHandlerRoute,
               "Admin/Affirm/CustomOrderConfirmCallbackHandler",
               new { controller = "Affirm", action = "CustomOrderConfirmCallbackHandler" });

            endpointRouteBuilder.MapControllerRoute(AffirmCheckoutDefaults.CustomCancelCallbackHandlerRoute,
               "Admin/Affirm/CustomOrderCancelCallbackHandler",
                new { controller = "Affirm", action = "CustomOrderCancelCallbackHandler" });
        }

        /// <summary>
        /// Gets a priority of route provider
        /// </summary>
        public int Priority => 100;
    }
}
