using Microsoft.AspNetCore.Mvc.Rendering;
using MWT.Nop.Core.Domain.Custom.LandingPage_Management;
using MWT.Nop.Core.Services.LandingPage_Management;
using MWT.Plugin.Misc.MwtStorefront.Area.Admin.Models.Catalog.LandingPage_Management;
using Nop.Core.Domain.Catalog;
using Nop.Services.Localization;
using Nop.Services.Seo;
using Nop.Web.Areas.Admin.Factories;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Framework.Extensions;
using Nop.Web.Framework.Factories;
using Nop.Web.Framework.Models.Extensions;
namespace MWT.Plugin.Misc.MwtStorefront.Area.Admin.Factories
{
    /// <summary>
    /// Represents the LandingPage model factory implementation
    /// </summary>
    public partial class LandingPageModelFactory : ILandingPageModelFactory
    {
        private readonly CatalogSettings _catalogSettings;
        private readonly IAclSupportedModelFactory _aclSupportedModelFactory;
        private readonly IBaseAdminModelFactory _baseAdminModelFactory;
        private readonly ILocalizationService _localizationService;
        private readonly ILocalizedModelFactory _localizedModelFactory;
        private readonly IStoreMappingSupportedModelFactory _storeMappingSupportedModelFactory;
        private readonly ILandingPageService _landingPageService;
        private readonly IUrlRecordService _urlRecordService;
        #region Ctor

        public LandingPageModelFactory(CatalogSettings catalogSettings,
            IAclSupportedModelFactory aclSupportedModelFactory,
            IBaseAdminModelFactory baseAdminModelFactory,
            ILocalizationService localizationService,
            ILocalizedModelFactory localizedModelFactory,
            IStoreMappingSupportedModelFactory storeMappingSupportedModelFactory,
            ILandingPageService landingPageService, IUrlRecordService urlRecordService)
        {
            _catalogSettings = catalogSettings;
            _aclSupportedModelFactory = aclSupportedModelFactory;
            _baseAdminModelFactory = baseAdminModelFactory;
            _localizationService = localizationService;
            _localizedModelFactory = localizedModelFactory;
            _storeMappingSupportedModelFactory = storeMappingSupportedModelFactory;
            _landingPageService = landingPageService;
            _urlRecordService = urlRecordService;
        }

        #endregion

        public async Task<LandingPageSearchModel> PrepareLandingPageSearchModelAsync(LandingPageSearchModel searchModel)
        {

            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            //prepare available stores
            await _baseAdminModelFactory.PrepareStoresAsync(searchModel.AvailableStores);

            searchModel.HideStoresList = _catalogSettings.IgnoreStoreLimitations || searchModel.AvailableStores.SelectionIsNotPossible();

            //prepare "published" filter (0 - all; 1 - published only; 2 - unpublished only)
            searchModel.AvailablePublishedOptions.Add(new SelectListItem
            {
                Value = "0",
                Text = await _localizationService.GetResourceAsync("Admin.Catalog.Categories.List.SearchPublished.All")
            });
            searchModel.AvailablePublishedOptions.Add(new SelectListItem
            {
                Value = "1",
                Text = await _localizationService.GetResourceAsync("Admin.Catalog.Categories.List.SearchPublished.PublishedOnly")
            });
            searchModel.AvailablePublishedOptions.Add(new SelectListItem
            {
                Value = "2",
                Text = await _localizationService.GetResourceAsync("Admin.Catalog.Categories.List.SearchPublished.UnpublishedOnly")
            });

            //prepare page parameters
            searchModel.SetGridPageSize();

            return searchModel;
        }

        public async Task<LandingPageListModel> PrepareLandingPageListModelAsync(LandingPageSearchModel searchModel)
        {

            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));
            //get categories
            var landingPages = await _landingPageService.GetAllLandingPagesAsync(landingPagesName: searchModel.SearchLandingPageName,
                showHidden: true,
                storeId: searchModel.SearchStoreId,
                pageIndex: searchModel.Page - 1, pageSize: searchModel.PageSize,
                overridePublished: searchModel.SearchPublishedId == 0 ? null : (bool?)(searchModel.SearchPublishedId == 1));

            //prepare grid model
            var model = await new LandingPageListModel().PrepareToGridAsync(searchModel, landingPages, () =>
            {

                return landingPages.SelectAwait(async landingPage =>
                {
                    var landingPageModel = new LandingPageModel();
                    landingPageModel = landingPage.ToModel(landingPageModel);
                    return landingPageModel;
                });
            });

            return model;

        }


        public async Task<LandingPageModel> PrepareLandingPageModelAsync(LandingPageModel model, LandingPage landingPageEntity, bool excludeProperties = false)
        {
            Func<LandingPageLocalizedModel, int, Task> localizedModelConfiguration = null;
            if (landingPageEntity != null)
            {
                // Fill in model values from the entity
                if (model == null)
                    model = new LandingPageModel();
                model= landingPageEntity.ToModel(model);
   
                model.SeName = await _urlRecordService.GetSeNameAsync(landingPageEntity, 0, true, false);

                localizedModelConfiguration = async (locale, languageId) =>
                {
                    locale.Name = await _localizationService.GetLocalizedAsync(landingPageEntity, entity => entity.Name, languageId, false, false);
                    locale.Description = await _localizationService.GetLocalizedAsync(landingPageEntity, entity => entity.Description, languageId, false, false);
                    locale.MetaKeywords = await _localizationService.GetLocalizedAsync(landingPageEntity, entity => entity.MetaKeywords, languageId, false, false);
                    locale.MetaDescription = await _localizationService.GetLocalizedAsync(landingPageEntity, entity => entity.MetaDescription, languageId, false, false);
                    locale.MetaTitle = await _localizationService.GetLocalizedAsync(landingPageEntity, entity => entity.MetaTitle, languageId, false, false);
                    locale.SeName = await _urlRecordService.GetSeNameAsync(landingPageEntity, languageId, false, false);
                };
               
                //prepare localized models
                if (!excludeProperties)
                    model.Locales = (IList<LandingPageLocalizedModel>)await _localizedModelFactory.PrepareLocalizedModelsAsync(localizedModelConfiguration);

                // ACL & Store mapping
                //  need to confirm
                await _aclSupportedModelFactory.PrepareModelCustomerRolesAsync(model);
                await _storeMappingSupportedModelFactory.PrepareModelStoresAsync(model, landingPageEntity, excludeProperties);
            }
            else
            {
                // Set default values for new model
                if (model == null)
                    model = new LandingPageModel();

                model.Published = true;
                model.CreatedOnUtc = DateTime.UtcNow;
                model.UpdatedOnUtc = DateTime.UtcNow;
            }
            return model;
        }

    }
}
