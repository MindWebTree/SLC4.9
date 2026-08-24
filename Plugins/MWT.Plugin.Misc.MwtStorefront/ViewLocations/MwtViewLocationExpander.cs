using Microsoft.AspNetCore.Mvc.Razor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MWT.Plugin.Misc.MwtStorefront.ViewLocations
{
    public class MwtViewLocationExpander : IViewLocationExpander
    {
        public void PopulateValues(ViewLocationExpanderContext context)
        {
        }

        public IEnumerable<string> ExpandViewLocations(
            ViewLocationExpanderContext context,
            IEnumerable<string> viewLocations)
        {
            string[] locations;

            if (context.AreaName == "Admin")
            {
                locations = new[]
                {
            "/Plugins/MWT.Plugin.Misc.MwtStorefront/Areas/Admin/Views/{1}/{0}.cshtml",
            "/Plugins/MWT.Plugin.Misc.MwtStorefront/Areas/Admin/Views/Shared/{0}.cshtml",
            "/Plugins/MWT.Plugin.Misc.MwtStorefront/Areas/Admin/Views/Shared/Components/{1}/{0}.cshtml"
        };
            }
            else if (context.AreaName == "CustomOrder")
            {
                locations = new[]
                {
            "/Plugins/MWT.Plugin.Misc.MwtStorefront/Areas/CustomOrder/Views/{1}/{0}.cshtml",
            "/Plugins/MWT.Plugin.Misc.MwtStorefront/Areas/CustomOrder/Views/Shared/{0}.cshtml",
            "/Plugins/MWT.Plugin.Misc.MwtStorefront/Areas/CustomOrder/Views/Shared/Components/{1}/{0}.cshtml"
        };
            }
            else
            {
                locations = new[]
                {
            "/Plugins/MWT.Plugin.Misc.MwtStorefront/Views/{1}/{0}.cshtml",
            "/Plugins/MWT.Plugin.Misc.MwtStorefront/Views/Shared/{0}.cshtml",
            "/Plugins/MWT.Plugin.Misc.MwtStorefront/Views/Shared/Components/{1}/{0}.cshtml"
        };
            }

            return locations.Concat(viewLocations);
        }
    }
}
