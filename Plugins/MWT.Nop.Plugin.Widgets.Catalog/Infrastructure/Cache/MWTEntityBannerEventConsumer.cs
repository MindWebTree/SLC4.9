using MWT.Nop.Plugin.Widgets.Catalog.Domain;
using Nop.Core.Caching;
using Nop.Core.Events;
using Nop.Services.Caching;
using Nop.Services.Configuration;
using Nop.Services.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MWT.Nop.Plugin.Widgets.Catalog.Infrastructure.Cache
{


    public partial class MWTEntityBannerEventConsumer: CacheEventConsumer<MWTEntityBanner>
    {
    #region Methods
        protected override async Task ClearCacheAsync(MWTEntityBanner entity)
        {
            await RemoveByPrefixAsync(MWTCatalogWidgetsDefaults.MWTCatalogWidgetPrefix);
        }
   
    
        #endregion
    }
}
