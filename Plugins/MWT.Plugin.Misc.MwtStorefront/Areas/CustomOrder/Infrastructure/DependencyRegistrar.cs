using Microsoft.Extensions.DependencyInjection;
using Nop.Core.Configuration;
using Nop.Core.Infrastructure;
using Nop.Core.Infrastructure.DependencyManagement;
using Nop.Services.Customizations.Phone_Order;
using MWT.Plugin.Misc.MwtStorefront.Areas.CustomOrder.Factories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
namespace MWT.Plugin.Misc.MwtStorefront.Areas.CustomOrder.Infrastructure
{
    public class DependencyRegistrar : IDependencyRegistrar
    {
        public virtual void Register(IServiceCollection services, ITypeFinder typeFinder, AppSettings appSettings)
        {
            services.AddScoped<ICustomOrderService, CustomOrderService>();
            services.AddScoped<ICustomerModelFactory, CustomerModelFactory>();
            services.AddScoped<ICustomerActivityModelFactory, CustomerActivityModelFactory>();
            services.AddScoped<ICustomOrderModelFactory, CustomOrderModelFactory>();

        }

        public int Order => 3;
    }
}
