using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MWT.Plugin.Misc.MwtStorefront.Area.Admin.Factories;
using MWT.Plugin.Misc.MwtStorefront.Area.Admin.Factories.Integrity_Report;
using MWT.Plugin.Misc.MwtStorefront.Area.Admin.Factories.PostDelievery;
using MWT.Plugin.Misc.MwtStorefront.Area.Admin.Factories.QA;
using MWT.Plugin.Misc.MwtStorefront.Area.Admin.Factories.Utilities;
using MWT.Plugin.Misc.MwtStorefront.Areas.Admin.Factories;
using MWT.Plugin.Misc.MwtStorefront.Areas.Admin.Factories.Custom;
using Nop.Core.Infrastructure;

namespace MWT.Plugin.Misc.MwtStorefront.Area.Admin.Infrastructure
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

            services.AddScoped<ICustomBaseAdminModelFactory, CustomBaseAdminModelFactory>();
            services.AddScoped<IQuestionAnswerModelFactory, QuestionAnswerModelFactory>();
            services.AddScoped<ILandingPageModelFactory, LandingPageModelFactory>();
            services.AddScoped<ICustomFormModelFactory, CustomFormModelFactory>();
            services.AddScoped<IUtilitiesModelFactory, UtilitiesModelFactory>();
            services.AddScoped<ICampaignManagementModelFactory, CampaignManagementModelFactory>();
            services.AddScoped<IFaqModelFactory, FaqModelFactory>();
            services.AddScoped<IPostDeliveryQueueEmailModelFactory, PostDeliveryQueueEmailModelFactory>();
            services.AddScoped<IIntegrityReportModelFactory, IntegrityReportModelFactory>();


        }

        public void Configure(IApplicationBuilder application)
        {

        }

        /// <summary>
        /// Order of this dependency registrar implementation
        /// </summary>
        public int Order => 2;
    }
}
