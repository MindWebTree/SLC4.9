
using Nop.Web.Infrastructure;

namespace MWT.Nop.Plugin.MegaMenu.Infrastructure
{
    internal class RouteProvider : BaseRouteProvider
    {
        public RouteProvider()
          : base()
        {
        }

        protected override string PluginSystemName => "MWT.Nop.Plugin.MegaMenu";
    }
}
