using Microsoft.Extensions.DependencyInjection;
using Nop.Core.Configuration;
using Nop.Core.Infrastructure;
using MWT.Plugin.Misc.MwtStorefront.Areas.CustomOrder.Factories;
using MWT.Nop.Core.Services.Customizations.CustomOrders;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Builder;
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
        }

        public void Configure(IApplicationBuilder application)
        {

        }

        public int Order => 3;
    }
}
