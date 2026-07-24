using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MWT.Nop.Core.Infrastructure;
using MWT.Plugin.Misc.MwtStorefront.ViewLocations;
using Nop.Core.Configuration;
using Nop.Core.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MWT.Nop.Plugin.Widgets.Catalog.Infrastructure
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
            services.AddScoped<ViewComponentRenderHelper>();
            services.Configure<RazorViewEngineOptions>(options =>
            { 
                options.ViewLocationExpanders.Add(new MwtViewLocationExpander());
            });
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
