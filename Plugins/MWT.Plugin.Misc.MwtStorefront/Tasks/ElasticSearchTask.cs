using MWT.Nop.Core.Domain;
using MWT.Nop.Core.Services.Catalog;
using MWT.Nop.Core.Services.ElasticSearch;
using MWT.Plugin.Misc.MwtStorefront.Factories;
using MWT.Plugin.Misc.MwtStorefront.Models.Catalog;
using Newtonsoft.Json;
using Nop.Core.Domain.Catalog;
using Nop.Services.Catalog;
using Nop.Services.Configuration;
using Nop.Services.ScheduleTasks;
using Nop.Services.Seo;

namespace MWT.Plugin.Misc.MwtStorefront.Tasks 
{
    /// Represents a task for sending queued message 
    /// </summary>
    public partial class ElasticSearchTask : IScheduleTask
    {

        #region Fields

        private readonly IElasticSearchModelFactory _elasticSearchModelFactory;
        private readonly IElasticSearchService _elasticSearchService;
        private readonly ISettingService _settingService;
        private readonly IProductService _productService;
        private readonly ICategoryService _categoryService;
        private readonly IUrlRecordService _urlRecordService;
        private readonly ISuggestedKeywordsService _suggestedKeywordsService;
        string Es_ProductCatalogIndexName = "";
        string Es_SuggestedKeyWordIndexName = "";
        string Es_EntityCataLogIndexName = "";

        #endregion

        #region ctor

        public ElasticSearchTask(IElasticSearchModelFactory elasticSearchModelFactory,
                                 IElasticSearchService elasticSearchService,
                                 ISettingService settingService,
                                 IProductService productService,
                                 ICategoryService categoryService,
                                 IUrlRecordService urlRecordService,
                                 ISuggestedKeywordsService suggestedKeywordsService)
        {
            this._elasticSearchModelFactory = elasticSearchModelFactory;
            this._elasticSearchService = elasticSearchService;
            this._settingService = settingService;
            this._productService = productService;
            this._categoryService = categoryService;
            this._urlRecordService = urlRecordService;
            this._suggestedKeywordsService = suggestedKeywordsService;
        }

        #endregion

        public virtual async System.Threading.Tasks.Task ExecuteAsync()
        {
            Es_ProductCatalogIndexName = await _settingService.GetSettingByKeyAsync<string>("Es_ProductCatalogIndexName");
            Es_SuggestedKeyWordIndexName = await _settingService.GetSettingByKeyAsync<string>("Es_SuggestedKeyWordIndexName");
            Es_EntityCataLogIndexName = await _settingService.GetSettingByKeyAsync<string>("Es_EntityCataLogIndexName");

            await SyncProducts();

            await SynCategories();

            await SyncSuggestedKeyword();
        }

        #region Utilities
        public async System.Threading.Tasks.Task SyncProducts()
        {
            int pageIndex = 0;
            int pageSize = 50;
            bool hasNextPage = false;
            do
            {
                var result = await this._productService.SearchProductsAsync(pageIndex: pageIndex, pageSize: pageSize, showHidden: true);
                hasNextPage = result.HasNextPage;

                foreach (var product in result)
                {
                    await ProcessProduct(product);
                }
                pageIndex++;
            } while (hasNextPage);
        }

        private async System.Threading.Tasks.Task ProcessProduct(Product product)
        {
            if (product.Deleted || !product.Published || product.IsServiceTypeProduct)
                await this._elasticSearchService.DeleteEntity("", Es_ProductCatalogIndexName, product.Id.ToString());
            else
            {
                var productModel = await this._elasticSearchModelFactory.PrepareProductModelForElasticSearch(product);
                if (productModel.suggestedwords.Count > 0)
                {

                }
                await this._elasticSearchService.InsertDataToElasticSearch(JsonConvert.SerializeObject(productModel), Es_ProductCatalogIndexName, product.Id.ToString());
            }
        }

        public async System.Threading.Tasks.Task SynCategories()
        {
            int pageIndex = 0;
            int pageSize = 50;
            bool hasNextPage = false;
            do
            {
                var result = await this._categoryService.GetAllCategoriesAsync("", pageIndex: pageIndex, pageSize: pageSize, showHidden: true);
                hasNextPage = result.HasNextPage;

                foreach (var category in result)
                {
                    await ProcessCategory(category);
                }
                pageIndex++;
            } while (hasNextPage);
        }

        private async System.Threading.Tasks.Task ProcessCategory(Category category)
        {

            if (category.Deleted)
                await this._elasticSearchService.DeleteEntity("", Es_EntityCataLogIndexName, category.Id.ToString());
            else
            {
                var parentCategory = new Category();
                if (category != null)
                {
                    if (category.ParentCategoryId != 0)
                        parentCategory = await this._categoryService.GetCategoryByIdAsync(category.ParentCategoryId);

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
                    await this._elasticSearchService.InsertDataToElasticSearch(JsonConvert.SerializeObject(esEntitytModel), Es_EntityCataLogIndexName, category.Id.ToString());

                }

            }
        }

        public async System.Threading.Tasks.Task SyncSuggestedKeyword()
        {
            int pageIndex = 0;
            int pageSize = 50;
            bool hasNextPage = false;
            do
            {
                var result = await this._suggestedKeywordsService.GetAllSuggestedKeywordAsync(pageIndex: pageIndex, pageSize: pageSize);
                hasNextPage = result.HasNextPage;

                foreach (var suggestedkeyword in result)
                {
                    await ProcessSuggestedKeyword(suggestedkeyword);
                }
                pageIndex++;
            } while (hasNextPage);
        }

        private async System.Threading.Tasks.Task ProcessSuggestedKeyword(SuggestedKeyword suggestedkeyword)
        {
            await this._elasticSearchService.InsertDataToElasticSearch(JsonConvert.SerializeObject(suggestedkeyword), Es_SuggestedKeyWordIndexName, suggestedkeyword.Id.ToString());
        }


        #endregion

    }
}