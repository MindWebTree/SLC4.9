using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Collections.Generic;

namespace MWT.Nop.Plugin.MegaMenu.ActionFilters
{
    public class GlobalActionFiltersProvider : IFilterProvider
    {
        public List<IFilterProvider> Providers { get; private set; }

        public GlobalActionFiltersProvider() => this.Providers = new List<IFilterProvider>();

        public int Order => 0;

        public void OnProvidersExecuted(FilterProviderContext context) => this.Providers.ForEach((Action<IFilterProvider>)(provider => provider.OnProvidersExecuted(context)));

        public void OnProvidersExecuting(FilterProviderContext context)
        {
        }
    }
}
