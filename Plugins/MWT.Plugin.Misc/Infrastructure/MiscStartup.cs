using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MWT.Plugin.Misc.Middleware;
using Nop.Core.Infrastructure;

namespace MWT.Plugin.Misc.Infrastructure
{
    public class MiscStartup : INopStartup
    {
        // run after nopCommerce's core services/pipeline are set up
        public int Order => 101;

        public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
        }

        public void Configure(IApplicationBuilder application)
        {
            // this one line is what makes capture "site-wide" with zero page/template edits
            application.UseMiddleware<UtmCaptureMiddleware>();
        }
    }
}
