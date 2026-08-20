using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Nop.Plugin.Payments.AuthorizeNetHosted.Models;
using Nop.Web.Framework.Mvc.Routing;

namespace Nop.Plugin.Payments.AuthorizeNetHosted.Infrastructure
{
    public class RouteProvider : IRouteProvider
    {
        public void RegisterRoutes(IEndpointRouteBuilder endpointRouteBuilder)
        {
            // Admin
            endpointRouteBuilder.MapControllerRoute(
                  name: "Plugin.Payments.AuthorizeNetHostedPayment.Configure",
                  pattern: "Admin/AuthorizeNetHostedPaymentSettings/Configure",
                  defaults: new { controller = "AuthorizeNetHostedPaymentSettings", action = "Configure" });

            endpointRouteBuilder.MapControllerRoute(
                name: AuthorizeNetHostedPaymentDefaults.WebhookHandlerRoute,
                pattern: AuthorizeNetHostedPaymentDefaults.WebhookUrl,
                defaults: new { controller = "AuthorizeNetHostedPayment", action = "WebhookEventsHandler" });

            endpointRouteBuilder.MapControllerRoute(
           name: AuthorizeNetHostedPaymentDefaults.WebhookHandlerRoute,
              pattern: "AuthorizeNetHosted/GetIframeToken",
           defaults: new { controller = "AuthorizeNetHostedPayment", action = "GetToken" });
        }

        public int Priority => -1;
    }
}
