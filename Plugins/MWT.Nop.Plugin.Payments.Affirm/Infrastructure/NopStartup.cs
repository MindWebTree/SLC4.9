using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MWT.Nop.Plugin.Payments.Affirm.Services;
using Nop.Core.Configuration;
using Nop.Core.Infrastructure;
//using Nop.Core.Infrastructure.DependencyManagement;
//using Nop.Services.Customizations.Custom;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MWT.Nop.Plugin.Payments.Affirm.Infrastructure
{
    public class NopStartup : INopStartup
    {
        /// <summary>
        /// Register services and interfaces
        /// </summary>
        /// <param name="services">Collection of service descriptors</param>
        /// <param name="typeFinder">Type finder</param>
        /// <param name="appSettings">App settings</param>
        //public virtual void Register(IServiceCollection services, ITypeFinder typeFinder, AppSettings appSettings)
        //{


        //}

        public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            services.AddScoped<ServiceManager>();
            services.AddScoped<CustomOrderServiceManager>();
            services.AddScoped<IAffirmService, AffirmService>();
        }

        public void Configure(IApplicationBuilder application)
        {

        }

        /// <summary>
        /// Order of this dependency registrar implementation
        /// </summary>
        public int Order => 1;
    }
}
