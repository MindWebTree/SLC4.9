using Autofac;
using Microsoft.Extensions.DependencyInjection;
using Nop.Core.Configuration;
using Nop.Core.Infrastructure;
using Nop.Plugin.Payments.AuthorizeNetHosted.Logging;
using Nop.Plugin.Payments.AuthorizeNetHosted.Models;
using Nop.Plugin.Payments.AuthorizeNetHosted.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nop.Plugin.Payments.AuthorizeNetHosted.Infrastructure
{
    public class DependencyRegistrar : Nop.Core.Infrastructure.DependencyManagement.IDependencyRegistrar
    {
        
        public virtual void Register(IServiceCollection services, ITypeFinder typeFinder, AppSettings appSettings)
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
       

        public int Order => 1;
    }
}
