using MWT.Nop.Core.Infrastructure;
using MWT.Nop.Core.Services.Search;
using Nop.Core.Configuration;
using Nop.Core.Infrastructure;

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
        }
         
    }
}
