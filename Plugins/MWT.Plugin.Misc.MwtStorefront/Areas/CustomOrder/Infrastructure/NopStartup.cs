using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MWT.Nop.Core.Services.Customizations.CustomOrders;
using MWT.Plugin.Misc.MwtStorefront.Areas.CustomOrder.Factories;
using MWT.Plugin.Misc.MwtStorefront.Areas.CustomOrder.Services; 
using Nop.Core.Infrastructure;
namespace MWT.Plugin.Misc.MwtStorefront.Areas.CustomOrder.Infrastructure
{
    public class NopStartup : INopStartup
    {
        public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            services.AddScoped<ICustomOrderService, CustomOrderService>();
            services.AddScoped<ICustomerModelFactory, CustomerModelFactory>();
            services.AddScoped<ICustomerActivityModelFactory, CustomerActivityModelFactory>();
            services.AddScoped<ICustomOrderModelFactory, CustomOrderModelFactory>();
            services.AddScoped<ICustomOrderMenuService, CustomOrderMenuService>();
        }

        public void Configure(IApplicationBuilder application)
        {

        }

        public int Order => 3;
    }
}
