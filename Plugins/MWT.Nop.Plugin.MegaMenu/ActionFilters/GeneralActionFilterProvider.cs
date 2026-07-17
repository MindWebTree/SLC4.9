using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using MWT.Nop.Plugin.MegaMenu.Services;
using System.Reflection;


#nullable enable
namespace MWT.Nop.Plugin.MegaMenu.ActionFilters
{
    public class GeneralActionFilterProvider : IFilterProvider
    {
        private readonly
#nullable disable
    string _defaultAllowedThemeName;
        private readonly string _allowedThemesSettingsKey;
        private readonly IThemeService _themeService;
        private readonly IList<IControllerActionFilterFactory> _actionFilterFactories = (IList<IControllerActionFilterFactory>)new List<IControllerActionFilterFactory>();

        public GeneralActionFilterProvider(
            IThemeService themeService,
          string defaultAllowedThemeName = "",
          string allowedThemesSettingsKey = ""
          )
        {
            this._defaultAllowedThemeName = defaultAllowedThemeName;
            this._allowedThemesSettingsKey = allowedThemesSettingsKey;
            this._themeService = themeService;
        } 

        public int Order => 0;

        public virtual void Add(IControllerActionFilterFactory actionFilterFactory) => this._actionFilterFactories.Add(actionFilterFactory);

        public virtual void AddRange(
          ICollection<IControllerActionFilterFactory> actionFilterFactories)
        {
            foreach (IControllerActionFilterFactory actionFilterFactory in (IEnumerable<IControllerActionFilterFactory>)actionFilterFactories)
                this._actionFilterFactories.Add(actionFilterFactory);
        }

        public void OnProvidersExecuted(FilterProviderContext context)
        {
            if (!this.ShouldExecuteFiltersForCurrentDesktopThemeAsync().Result)
                return;
            ControllerActionDescriptor actionDescriptor = context.ActionContext.ActionDescriptor as ControllerActionDescriptor;
            TypeInfo controllerTypeInfo = actionDescriptor.ControllerTypeInfo;
            foreach (IControllerActionFilterFactory actionFilterFactory in (IEnumerable<IControllerActionFilterFactory>)this._actionFilterFactories)
            {
                if (string.Equals(actionFilterFactory.ControllerName, actionDescriptor.ControllerName, StringComparison.OrdinalIgnoreCase) && string.Equals(actionFilterFactory.ActionName, actionDescriptor.ActionName, StringComparison.OrdinalIgnoreCase))
                {
                    ActionFilterAttribute actionFilterAttribute = actionFilterFactory.GetActionFilterAttribute();
                    context.Results.Add(new FilterItem(new FilterDescriptor((IFilterMetadata)actionFilterAttribute, FilterScope.Global), (IFilterMetadata)actionFilterAttribute));
                }
            }
        }

        public void OnProvidersExecuting(FilterProviderContext context)
        {
        }

        private async Task<bool> ShouldExecuteFiltersForCurrentDesktopThemeAsync() => await _themeService.CheckActiveThemeAsync(this._defaultAllowedThemeName, this._allowedThemesSettingsKey);
    }
}
