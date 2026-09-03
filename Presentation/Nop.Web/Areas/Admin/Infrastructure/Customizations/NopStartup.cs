using Nop.Core.Infrastructure;
using Nop.Web.Areas.Admin.Factories;
using Nop.Web.Areas.Admin.Factories.Customization;

namespace Nop.Web.Areas.Admin.Infrastructure.Customizations
{
    public class NopStartup : INopStartup
    {
        public int Order => 3;

        public void Configure(IApplicationBuilder application)
        {

        }

        public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            services.AddScoped<ICategoryCollectionLinkModelFactory, CategoryCollectionLinkModelFactory>();
            services.AddScoped<IQuickFilterModelFactory, QuickFilterModelFactory>();
            services.AddScoped<IFaqModelFactory, FaqModelFactory>();
            services.AddScoped<IKwTermModelFactory, KwTermModelFactory>();
            services.AddScoped<IRelatedSearchModelFactory, RelatedSearchModelFactory>();
        }
    }
}
