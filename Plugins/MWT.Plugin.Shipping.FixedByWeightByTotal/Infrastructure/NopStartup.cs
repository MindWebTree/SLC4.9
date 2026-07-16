using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MWT.Plugin.Shipping.FixedByWeightByTotal.Services;
using Nop.Core.Configuration;
using Nop.Core.Infrastructure;

namespace MWT.Plugin.Shipping.FixedByWeightByTotal.Infrastructure
{
    /// <summary>
    /// Dependency registrar
    /// </summary>
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
            services.AddScoped<IMWTShippingByWeightByTotalService, MWTShippingByWeightByTotalService>();
            services.AddScoped<IMWTShippingZoneService, MWTShippingZoneService>();
            services.AddScoped<IMWTExpectedDeliveryDateService, MWTExpectedDeliveryDateService>();
            services.AddScoped<IMWTEstimationDeliveryDateNotificationService, MWTEstimationDeliveryDateNotificationService>();
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