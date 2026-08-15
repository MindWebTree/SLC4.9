using MWT.Nop.Core.Infrastructure;
using MWT.Nop.Core.Service;
using MWT.Nop.Core.Service.Campaign_Management;
using MWT.Nop.Core.Service.Catalog;
using MWT.Nop.Core.Service.Discount;
using MWT.Nop.Core.Service.FAQModule;
using MWT.Nop.Core.Service.Zoho;
using MWT.Nop.Core.Services;
using MWT.Nop.Core.Services.Catalog;
using MWT.Nop.Core.Services.CategoryCollection;
using MWT.Nop.Core.Services.Configuration;
using MWT.Nop.Core.Services.Custom;
using MWT.Nop.Core.Services.Customers;
using MWT.Nop.Core.Services.ElasticSearch;
using MWT.Nop.Core.Services.FeedBack;
using MWT.Nop.Core.Services.IPLite;
using MWT.Nop.Core.Services.KW;
using MWT.Nop.Core.Services.LandingPage_Management;
using MWT.Nop.Core.Services.MailChimp;
using MWT.Nop.Core.Services.Manage;
using MWT.Nop.Core.Services.Mandrill;
using MWT.Nop.Core.Services.Media;
using MWT.Nop.Core.Services.Message;
using MWT.Nop.Core.Services.Orders;
using MWT.Nop.Core.Services.QA;
using MWT.Nop.Core.Services.QuickFilters;
using MWT.Nop.Core.Services.Search;
using MWT.Nop.Core.Services.Search.RewardClaim;
using MWT.Nop.Core.Services.Seo;
using MWT.Nop.Core.Services.Shared;
using Nop.Core.Infrastructure;
using Nop.Services.Customizations.IpAddress;

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
            services.AddScoped<ISearchLogService, SearchLogService>();
            services.AddScoped<IProductExtendedService, ProductExtendedService>();
            services.AddScoped<ICustomProductAttributeParser, CustomProductAttributeParser>();
            services.AddScoped<ICustomProductAttributeService, CustomProductAttributeService>();

            services.AddScoped<IStoreWideDiscountService, StoreWideDiscountService>();
            services.AddScoped<ICustomizationFormSerivce, CustomizationFormSerivce>();
            services.AddScoped<IFeedService, FeedService>();
            services.AddScoped<ICustomSpecificationAttributeService, CustomSpecificationAttributeService>();
            services.AddScoped<IPictureExtendedService, PictureExtendedService>();
            services.AddScoped<IShoppingCartExtendedCartService, ShoppingCartExtendedCartService>();
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
            services.AddScoped<ICustomerExtendedService, CustomerExtendedService>();
            services.AddScoped<ILandingPageService, LandingPageService>();
            services.AddScoped<ICustomFormService, CustomFormService>();
            services.AddScoped<IFeedbackService, FeedbackService>();
            services.AddScoped<ICategoryCollectionLinkService, CategoryCollectionLinkService>();
            services.AddScoped<ITestimonialService, TestimonialService>();
            services.AddScoped<IRewardClaimService, RewardClaimService>();
            services.AddScoped<IManageService, ManageService>();


            services.AddScoped<IIpAddressService, IpAddressService>();
            services.AddScoped<IIPLiteService, IPLiteService>();

            #region  Campagin Management
            services.AddScoped<ICampaignManagementService, CampaignManagementService>();
            #endregion

            #region  Mailchimp

            services.AddScoped<IMailchimpService, MailchimpService>();
            services.AddScoped<IQueuedMailChimpCartSignUpService, QueuedMailChimpCartSignUpService>();
            services.AddScoped<IMailchimpSegmentsService, MailchimpSegmentsService>();
            services.AddScoped<IMandrillService, MandrillService>();
            #endregion

            #region  Message
            services.AddScoped<ICustomMessageTokenProvider, CustomMessageTokenProvider>();
            services.AddScoped<ICustomNewsLetterSubscriptionService, CustomNewsLetterSubscriptionService>();
            services.AddScoped<ICustomWorkflowMessageService, CustomWorkflowMessageService>();
            #endregion

            #region  Shared
            services.AddScoped<ICommonService, CommonService>();
            #endregion

            #region  Zoho

            services.AddScoped<IZohoService, ZohoService>();

            #endregion

            #region Faq
            services.AddScoped<IFaqService, FaqService>();

            #endregion
            #region NewsLetter

            services.AddScoped<ICustomNewsLetterSubscriptionService, CustomNewsLetterSubscriptionService>();
            #endregion

            #region ElasticSearch
            services.AddScoped<IElasticSearchHelpService, ElasticSearchHelpService>();
            services.AddScoped<ICustomBackInStockSubscriptionService, CustomBackInStockSubscriptionService>();
            services.AddScoped<ICustomCategoryService, CustomCategoryService>();
            services.AddScoped<ICustomizationFormSerivce, CustomizationFormSerivce>();
            services.AddScoped<ICustomRecentlyViewedProductsService, CustomRecentlyViewedProductsService>();
            services.AddScoped<ISuggestedKeywordsService, SuggestedKeywordsService>();
            services.AddScoped<IFuzzySearchService, FuzzySearchService>();
            services.AddScoped<IRelatedSearchService, RelatedSearchService>();
            #endregion
            #region QuickFilter
            services.AddScoped<IQuickFilterService, QuickFilterService>();

            #endregion
            #region Kw
            services.AddScoped<IKwTermService, KwTermService>();
            services.AddScoped<IKwTemplateService, KWTemplateService>();
            #endregion

            #region Template
            services.AddScoped<IProductTemplateSectionService, ProductTemplateSectionService>();

            #endregion
        }

    }
}
