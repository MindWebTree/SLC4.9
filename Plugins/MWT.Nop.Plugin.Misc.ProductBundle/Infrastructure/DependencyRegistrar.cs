
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MWT.Nop.Plugin.Misc.ProductBundle.Services;
using Nop.Core.Configuration;
using Nop.Core.Infrastructure;

namespace MWT.Nop.Plugin.Misc.ProductBundle.Infrastructure
{
    public class NopStartup : INopStartup
    {
        /// <summary>
        /// Register services and interfaces
        /// </summary>
        /// <param name="services">Collection of service descriptors</param>
        /// <param name="typeFinder">Type finder</param>
        /// <param name="appSettings">App settings</param>
   

        public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            services.AddScoped<IBundleService, BundleService>();
            services.AddScoped<IBundleLoggerService, BundleLoggerService>();
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
