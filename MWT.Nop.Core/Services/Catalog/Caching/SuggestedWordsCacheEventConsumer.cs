using MWT.Nop.Core.Domain;
using MWT.Nop.Core.Services.ElasticSearch;
using Newtonsoft.Json;
using Nop.Services.Caching;
using Nop.Services.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MWT.Nop.Core.Services.Catalog.Caching
{
    public partial class SuggestedWordsCacheEventConsumer : CacheEventConsumer<SuggestedKeyword>
    {
        #region Fields

        private readonly IElasticSearchService _elasticSearchService;
        private readonly ISettingService _settingService;
        private readonly ISuggestedKeywordsService _suggestedKeywordsService;

        #endregion

        #region Ctor
        public SuggestedWordsCacheEventConsumer(
                IElasticSearchService elasticSearchService,
                ISettingService settingService,
                ISuggestedKeywordsService suggestedKeywordsService)
        {
            this._elasticSearchService = elasticSearchService;
            this._settingService = settingService;
            this._suggestedKeywordsService = suggestedKeywordsService;
        }

        #endregion
        /// <summary>
        /// Clear cache data
        /// </summary>
        /// <param name="entity">Entity</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        protected override async Task ClearCacheAsync(SuggestedKeyword entity, EntityEventType entityEventType)
        {
            string Es_SuggestedKeyWordIndexName = await _settingService.GetSettingByKeyAsync<string>("Es_SuggestedKeyWordIndexName");

            if (entityEventType == EntityEventType.Delete)
                await this._elasticSearchService.DeleteEntity("", Es_SuggestedKeyWordIndexName, entity.Id.ToString());
            else
            {
                var suggestedKeyword = await this._suggestedKeywordsService.GetSuggestedKeyWordById(entity.Id);
                await this._elasticSearchService.InsertDataToElasticSearch(JsonConvert.SerializeObject(suggestedKeyword), Es_SuggestedKeyWordIndexName, entity.Id.ToString());
            }
        }
    }
}
