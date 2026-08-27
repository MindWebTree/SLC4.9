using MWT.Nop.Core.Services.ElasticSearch;
using MWT.Plugin.Misc.MwtStorefront.Models.Catalog;
using Newtonsoft.Json;
using Nop.Core.Domain.Catalog;
using Nop.Services.Caching;
using Nop.Services.Catalog;
using Nop.Services.Configuration;
using Nop.Services.Seo;

namespace MWT.Plugin.Misc.MwtStorefront.Infrastructure.EventConsumer
{
    public partial class CategoryCacheEventConsumer : CacheEventConsumer<Category>
    {

        #region Field

        private readonly ICategoryService _categoryService;
        private readonly IElasticSearchService _elasticSearchService;
        private readonly ISettingService _settingService;
        private readonly IUrlRecordService _urlRecordService;


        #endregion

        #region ctor

        public CategoryCacheEventConsumer(ICategoryService categoryService,
                                          IElasticSearchService elasticSearchService,
                                           ISettingService settingService,
                                            IUrlRecordService urlRecordService)
        {
            this._categoryService = categoryService;
            this._elasticSearchService = elasticSearchService;
            this._settingService = settingService;
            this._urlRecordService = urlRecordService;
        }

        #endregion
        /// <summary>
        /// Clear cache data
        /// </summary>
        /// <param name="entity">Entity</param>
        /// <param name="entityEventType">Entity event type</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        protected override async Task ClearCacheAsync(Category entity, EntityEventType entityEventType)
        {
            string Es_EntityCataLogIndexName = await _settingService.GetSettingByKeyAsync<string>("Es_EntityCataLogIndexName");
            if (entityEventType == EntityEventType.Delete)
                await this._elasticSearchService.DeleteEntity("", Es_EntityCataLogIndexName, entity.Id.ToString());
            else
            {
                var parentCategory = new Category();
                var category = await this._categoryService.GetCategoryByIdAsync(entity.Id);
                if (category != null)
                {
                    if (category.ParentCategoryId != 0)
                        parentCategory = await this._categoryService.GetCategoryByIdAsync(entity.ParentCategoryId);

                    EsEntitytModel esEntitytModel = new EsEntitytModel();
                    esEntitytModel.Published = category.Published;
                    esEntitytModel.DisplayOrder = category.DisplayOrder;
                    esEntitytModel.ParentEntityID = category.ParentCategoryId.ToString();
                    esEntitytModel.ParentEntityName = parentCategory == null ? "" : parentCategory.Name;
                    esEntitytModel.EntityID = category.Id.ToString();
                    esEntitytModel.EntityName = category.Name;
                    esEntitytModel.SEKeywords = "cat-" + category.Id;
                    esEntitytModel.EntityType = "Category";
                    esEntitytModel.SEName = await this._urlRecordService.GetSeNameAsync(category.Id, "Category", languageId: null, true);
                    await this._elasticSearchService.InsertDataToElasticSearch(JsonConvert.SerializeObject(esEntitytModel), Es_EntityCataLogIndexName, entity.Id.ToString());

                }

            }

        }
    }
}
