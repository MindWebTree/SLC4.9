using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nop.Core.Infrastructure;
using Nop.Plugin.Payments.AuthorizeNetHosted.Logging;
using Nop.Plugin.Payments.AuthorizeNetHosted.Models;
using Nop.Plugin.Payments.AuthorizeNetHosted.Services;

namespace Nop.Plugin.Payments.AuthorizeNetHosted.Infrastructure
{
    public class NopStartup : INopStartup
    { 

        public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            services.AddScoped<IAuthorizeNetManager, AuthorizeNetManager>();
            services.AddScoped<IAuthorizeNetWebHookService, AuthorizeNetWebHookService>();
            services.AddScoped<PaymentLogger>(provider =>
            {
                var settings = provider.GetRequiredService<AuthorizeNetHostedPaymentSettings>();
                var fileProvider = provider.GetRequiredService<INopFileProvider>();

                var logDir = fileProvider.MapPath("~/Plugins/Payments.AuthorizeNetHosted/Logs");
                return new PaymentLogger(settings.ShowDebugInfo, logDir);
            });
        }

        public void Configure(IApplicationBuilder application)
        {
 
        }

        public int Order => int.MaxValue;
    }
}
