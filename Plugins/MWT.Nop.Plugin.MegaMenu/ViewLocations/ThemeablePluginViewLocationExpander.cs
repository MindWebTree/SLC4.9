
using Microsoft.AspNetCore.Mvc.Razor;
using MWT.Nop.Plugin.MegaMenu.Services;
using System.Collections.Generic;

namespace MWT.Nop.Plugin.MegaMenu.ViewLocations
{
    public class ThemeablePluginViewLocationExpander : IViewLocationExpander
    {
        public void PopulateValues(ViewLocationExpanderContext context) => (context.ActionContext.HttpContext.RequestServices.GetService(typeof(IViewLocationsManager)) as IViewLocationExpander).PopulateValues(context);

        public IEnumerable<string> ExpandViewLocations(
          ViewLocationExpanderContext context,
          IEnumerable<string> viewLocations)
        {
            return (context.ActionContext.HttpContext.RequestServices.GetService(typeof(IViewLocationsManager)) as IViewLocationExpander).ExpandViewLocations(context, viewLocations);
        }
    }
}
