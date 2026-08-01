using Microsoft.AspNetCore.Http;
using MWT.Plugin.Misc.MwtStorefront.Models.Catalog;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Forums;
using Nop.Core.Domain.Media;
using Nop.Core.Domain.Seo;
using Nop.Core.Domain.Vendors;
using Nop.Core.Events;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.FilterLevels;
using Nop.Services.Localization;
using Nop.Services.Media;
using Nop.Services.Seo;
using Nop.Services.Vendors;
using Nop.Web.Factories;
using Nop.Web.Framework.Mvc.Routing;
using Nop.Web.Infrastructure.Cache;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MWT.Plugin.Misc.MwtStorefront.Factories.Catalog
{
    public class CustomCatalogModelFactory : CatalogModelFactory,ICustomCatalogModelFactory
    {
        public CustomCatalogModelFactory(CatalogSettings catalogSettings, CustomerSettings customerSettings,
            ForumSettings forumSettings, ICategoryService categoryService,
            ICategoryTemplateService categoryTemplateService, ICurrencyService currencyService,
            ICustomerService customerService, IEventPublisher eventPublisher, IFilterLevelValueService filterLevelValueService, 
            IGenericAttributeService genericAttributeService, IHttpContextAccessor httpContextAccessor, IJsonLdModelFactory jsonLdModelFactory, 
            ILocalizationService localizationService, IManufacturerService manufacturerService, IManufacturerTemplateService manufacturerTemplateService, 
            INopUrlHelper nopUrlHelper, IPictureService pictureService, IProductModelFactory productModelFactory, IProductReviewService productReviewService,
            IProductService productService, IProductTagService productTagService, ISearchTermService searchTermService, ISpecificationAttributeService specificationAttributeService,
            IStaticCacheManager staticCacheManager, IStoreContext storeContext, IUrlRecordService urlRecordService, IVendorService vendorService, IWebHelper webHelper, 
            IWorkContext workContext, MediaSettings mediaSettings, SeoSettings seoSettings, VendorSettings vendorSettings) : base(catalogSettings, customerSettings, 
                forumSettings, categoryService, categoryTemplateService, currencyService, customerService, eventPublisher, filterLevelValueService, genericAttributeService,
                httpContextAccessor, jsonLdModelFactory, localizationService, manufacturerService, manufacturerTemplateService, nopUrlHelper, pictureService, 
                productModelFactory, productReviewService, productService, productTagService, searchTermService, specificationAttributeService, staticCacheManager, 
                storeContext, urlRecordService, vendorService, webHelper, workContext, mediaSettings, seoSettings, vendorSettings)
        {
        }

        public virtual async Task<List<CustomCategoryModel>> CustomPrepareHomepageCategoryModelsAsync()
        {
            var language = await _workContext.GetWorkingLanguageAsync();
            var customer = await _workContext.GetCurrentCustomerAsync();
            var customerRoleIds = await _customerService.GetCustomerRoleIdsAsync(customer);
            var store = await _storeContext.GetCurrentStoreAsync();
            var pictureSize = _mediaSettings.CategoryThumbPictureSize;

 
            var categoriesCacheKey = _staticCacheManager.PrepareKeyForDefaultCache(CustomNopModelCacheDefaults.CustomCategoryHomepageKey,
                store, customerRoleIds, pictureSize, language, _webHelper.IsCurrentConnectionSecured());

            var model = await _staticCacheManager.GetAsync(categoriesCacheKey, async () =>
            {
 
                var homepageCategories = (await _categoryService.GetAllCategoriesDisplayedOnHomepageAsync())
                                         .OrderBy(c => c.AlternateDisplayOrder)
                                         .ThenBy(c => c.Id);

                return await homepageCategories.SelectAwait(async category =>
                {
                    var catModel = new CustomCategoryModel
                    {
                        Id = category.Id,
                        Name = await _localizationService.GetLocalizedAsync(category, x => x.Name),
                        Description = await _localizationService.GetLocalizedAsync(category, x => x.Description),
                        MetaKeywords = await _localizationService.GetLocalizedAsync(category, x => x.MetaKeywords),
                        MetaDescription = await _localizationService.GetLocalizedAsync(category, x => x.MetaDescription),
                        MetaTitle = await _localizationService.GetLocalizedAsync(category, x => x.MetaTitle),
                        SeName = await _urlRecordService.GetSeNameAsync(category),
                        PictureModel = await PrepareCategoryPictureModelAsync(category)
                    };

                    return catModel;
                }).ToListAsync();
            });

            return model;
        }
    }
}
