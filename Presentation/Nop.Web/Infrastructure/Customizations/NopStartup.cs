using MWT.Nop.Core.Infrastructure;
using MWT.Nop.Core.Service.Catalog;
using MWT.Nop.Core.Service.StoreWideDiscount;
using MWT.Nop.Core.Services.Catalog;
using MWT.Nop.Core.Services.Configuration;
using MWT.Nop.Core.Services.KW;
using MWT.Nop.Core.Services.Media;
using MWT.Nop.Core.Services.QA;
using MWT.Nop.Core.Services.Search;
using MWT.Nop.Core.Services.Seo;
using Nop.Core.Configuration;
using Nop.Core.Infrastructure;
using Nop.Services.Customizations.Custom;

namespace Nop.Web.Infrastructure.Customizations
{
    public class NopStartup : INopStartup
    {
        public int Order => 2;

        public void Configure(IApplicationBuilder application)
        {
 
        }

        public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            services.AddScoped<ViewComponentRenderHelper>();
            services.AddScoped<ISearchLogService,SearchLogService>();
            services.AddScoped<ICustomProductService, CustomProductService>();
            services.AddScoped<ICustomProductAttributeParser, CustomProductAttributeParser>();
            services.AddScoped<ICustomProductAttributeService, CustomProductAttributeService>();
            services.AddScoped<IVariantService, VariantService>();
            services.AddScoped<IStoreWideDiscountService,StoreWideDiscountService>();
            services.AddScoped<ICustomizationFormSerivce,CustomizationFormSerivce>();
            services.AddScoped<IFeedService,FeedService>();
            services.AddScoped<ICustomSpecificationAttributeService, CustomSpecificationAttributeService>();
            services.AddScoped<ICustomPictureService, CustomPictureService>();
            services.AddScoped<ICustomShoppingCartService, CustomShoppingCartService>();
            services.AddScoped<ICustomProductAttributeFormatter, CustomProductAttributeFormatter>();
            services.AddScoped<IGroupedProductConfigurationService, GroupedProductConfigurationService>();
            services.AddScoped<IKwTermService, KwTermService>();
            //
            services.AddScoped<IQuestionAnswerService, QuestionAnswerService>();
            services.AddScoped<IQuestionAnswerTemplateService, QuestionAnswerTemplateService>();
            services.AddScoped<IFiltersMappingByEntityService, FiltersMappingByEntityService>();
            services.AddScoped<ICustomUrlRecordService, CustomUrlRecordService>();
            services.AddScoped<ICustomCategoryService, CustomCategoryService>();
            services.AddScoped<ICustomWorkContext, CustomWorkContext>();
            services.AddScoped<IKwTemplateService, KWTemplateService>();
        }
         
    }
}
