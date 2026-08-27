using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MWT.Nop.Core.Infrastructure;
using MWT.Plugin.Misc.MwtStorefront.Areas.Admin.Factories;
using MWT.Plugin.Misc.MwtStorefront.Factories;
using MWT.Plugin.Misc.MwtStorefront.Factories.Catalog;
using MWT.Plugin.Misc.MwtStorefront.Factories.QA;
using MWT.Plugin.Misc.MwtStorefront.Factories.TagPage;
using MWT.Plugin.Misc.MwtStorefront.Factories.Topics;
using MWT.Plugin.Misc.MwtStorefront.Infrastructure;
using MWT.Plugin.Misc.MwtStorefront.ViewLocations;
using Nop.Core.Infrastructure;
using Nop.Data;
using Nop.Web.Framework.Mvc.Routing;

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
            services.AddScoped<ICustomCatalogModelFactory,CustomCatalogModelFactory>();
            services.AddScoped<ICustomProductModelFactory, CustomProductModelFactory>();
            services.AddScoped<IQuestionAnswerModelFactory, QuestionAnswerModelFactory>(); 
            services.AddScoped<ILandingPageModelFactory, LandingPageModelFactory>();
            services.AddScoped<ICustomTopicModelFactory, CustomTopicModelFactory>();
            services.AddScoped<IFaqModelFactory, FaqModelFactory>(); 
            services.AddScoped<IRelatedSearchModelFactory, RelatedSearchModelFactory>(); 
            services.AddScoped<IQuickFilterModelFactory, QuickFilterModelFactory>(); 
            services.AddScoped<IKwTermModelFactory, KwTermModelFactory>(); 
            services.AddScoped<ISiteMapExtendedModelFactory, SiteMapExtendedModelFactory>(); 
            services.AddScoped<ITagModelFactory, TagModelFactory>(); 
            services.AddScoped<IShoppingCartExtendedModelFactory, ShoppingCartExtendedModelFactory>(); 
            services.AddScoped<IProductBundleModelFactory, ProductBundleModelFactory>(); 
            services.AddScoped<IOrderExtendedModelFactory, OrderExtendedModelFactory>(); 
            services.AddScoped<IAddressExtendedModelFactory, AddressExtendedModelFactory>(); 
            services.AddScoped<ICheckoutExtendedModelFactory, CheckoutExtendedModelFactory>();
            services.AddScoped<IAbandonedCartModelFactory, AbandonedCartModelFactory>();
            services.AddScoped<IProductAttributeExtendedModelFactory, ProductAttributeExtendedModelFactory>();
            if (DataSettingsManager.IsDatabaseInstalled())
                services.AddScoped<SlugRouteExtendetTransformer>();
            services.Configure<RazorViewEngineOptions>(options =>
            { 
                options.ViewLocationExpanders.Add(new MwtViewLocationExpander());
            });
 ///

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
