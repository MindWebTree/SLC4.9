using MWT.Nop.Core.Domain;
using MWT.Nop.Core.Service.Catalog;
using Nop.Core.Caching;
using Nop.Services.Caching;
using Nop.Services.Catalog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MWT.Nop.Core.Services.Catalog.Caching
{
    public partial class SearchLogCacheEventConsumer : CacheEventConsumer<SearchLog>
    {
        #region Fields

        private readonly IStaticCacheManager _staticCacheManager;


        #endregion

        #region Ctor
        public SearchLogCacheEventConsumer(
               IStaticCacheManager staticCacheManager)
        {
            this._staticCacheManager = staticCacheManager;
        }

        #endregion
        /// <summary>
        /// Clear cache data
        /// </summary>
        /// <param name="entity">Entity</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        protected override async Task ClearCacheAsync(SearchLog entity)
        {
            await _staticCacheManager.RemoveByPrefixAsync(CustomNopCatalogDefaults.SearchDefaultAutoCompletePrefix, entity.CustomerId);
        }
    }
}
