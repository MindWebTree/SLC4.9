using Microsoft.Extensions.DependencyInjection;
using Nop.Core.Configuration;
using Nop.Core.Infrastructure; 
using MWT.Tax.FixedOrByCountryStateZip.Services;
using Nop.Services.Tax;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Builder;

namespace MWT.Tax.FixedOrByCountryStateZip.Infrastructure
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
            services.AddScoped<ITaxProvider, MWTFixedOrByCountryStateZipTaxProvider>();
            services.AddScoped<IMWTCountryStateZipService, MWTCountryStateZipService>();
            services.AddScoped<ITaxZarService, TaxZarService>();
            services.AddScoped<IZipTaxService, ZipTaxService>();
            services.AddScoped<ITaxLogService, TaxLogService>();
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