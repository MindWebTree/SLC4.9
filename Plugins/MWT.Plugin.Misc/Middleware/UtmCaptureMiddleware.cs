using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using MWT.Nop.Core.Domain.Customers;
using MWT.Plugin.Misc.Domain;
using Nop.Core;
using Nop.Core.Domain.Customers;
using Nop.Services.Common;
using Nop.Services.Configuration;

namespace MWT.Plugin.Misc.Middleware
{
   
    public class UtmCaptureMiddleware
    {
        private readonly RequestDelegate _next;

       
        private static readonly (string QueryParam, string AttributeKey)[] TrackedParams =
        {
            ("gclid", CustomNopCustomerDefaults.GCLID),
            ("gad_campaignid", CustomNopCustomerDefaults.Campaign),
              ("utm_campaign", CustomNopCustomerDefaults.Campaign),
            ("utm_term", CustomNopCustomerDefaults.Term)
        };

        public UtmCaptureMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                if (context.Request.Method == "GET")
                {
                    var settingService = context.RequestServices.GetRequiredService<ISettingService>();
                    var settings = await settingService.LoadSettingAsync<UtmSettings>();

                    var workContext = context.RequestServices.GetRequiredService<IWorkContext>();
                    var genericAttributeService = context.RequestServices.GetRequiredService<IGenericAttributeService>();
                    var customer = await workContext.GetCurrentCustomerAsync();

                    foreach (var (queryParam, attrKey) in TrackedParams)
                    {
                        if (!context.Request.Query.TryGetValue(queryParam, out var value) || string.IsNullOrWhiteSpace(value))
                            continue;

                        if (settings.UseFirstTouchAttribution)
                        {
                            var existing = await genericAttributeService.GetAttributeAsync<string>(customer, attrKey);
                            if (!string.IsNullOrEmpty(existing))
                                continue; // first-touch: don't overwrite
                        }

                        await genericAttributeService.SaveAttributeAsync(customer, attrKey, value.ToString());
                    }
                }
            }
            catch
            {
                // tracking must never break the site — swallow and continue the pipeline
            }

            await _next(context);
        }
    }
}