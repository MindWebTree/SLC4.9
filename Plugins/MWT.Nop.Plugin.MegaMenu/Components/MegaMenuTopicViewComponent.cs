using Microsoft.AspNetCore.Mvc;
using MWT.Nop.Plugin.MegaMenu.Areas.Admin.Models;
using MWT.Nop.Plugin.MegaMenu.Models;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Blogs;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Forums;
using Nop.Core.Domain.Localization;
using Nop.Core.Domain.Media;
using Nop.Core.Domain.Stores;
using Nop.Core.Domain.Topics;
using Nop.Core.Domain.Vendors;
using Nop.Core.Events;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Media;
using Nop.Services.Security;
using Nop.Services.Stores;
using Nop.Services.Tax;
using Nop.Services.Topics;
using Nop.Services.Vendors;
using Nop.Web.Factories;
using Nop.Web.Framework.Models;
using Nop.Web.Models.Topics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;


#nullable enable
namespace MWT.Nop.Plugin.MegaMenu.Components
{
    public class MegaMenuTopicViewComponent : BaseComponent
    {
        private readonly ICategoryService _categoryService;
        private readonly IManufacturerService _manufacturerService;
        private readonly IVendorService _vendorsService;
        private readonly IProductTagService _productTagService;
        private readonly IWorkContext _workContext;
        private readonly IStoreContext _storeContext;
        private readonly MediaSettings _mediaSettings;
        private readonly IPictureService _pictureService;
        private readonly ILocalizationService _localizationService;
        private readonly IAclService _aclService;
        private readonly ITopicService _topicService;
        private readonly IStoreService _storeService;
        private readonly IStoreMappingService _storeMappingService;
        private readonly IWebHelper _webHelper;
        private readonly CatalogSettings _catalogSettings;

        private CacheKey MegaMenuModelKey => new CacheKey("nop.pres.mwt.megamenu-common-{0}-{1}-{2}-{3}-{4}" );

        private CacheKey MegaMenuTopicsKey => new CacheKey("nop.pres.mwt.megamenu-topics-{0}-{1}-{2}");

        public MegaMenuTopicViewComponent(
          ICatalogModelFactory catalogModelFactory,
          IProductModelFactory productModelFactory,
          ICategoryService categoryService,
          IManufacturerService manufacturerService,
          IProductService productService,
          IVendorService vendorService,
          ICategoryTemplateService categoryTemplateService,
          IManufacturerTemplateService manufacturerTemplateService,
          IWorkContext workContext,
          IStoreContext storeContext,
          ITaxService taxService,
          ICurrencyService currencyService,
          IPictureService pictureService,
          ILocalizationService localizationService,
          IPriceCalculationService priceCalculationService,
          IPriceFormatter priceFormatter,
          IWebHelper webHelper,
          ISpecificationAttributeService specificationAttributeService,
          IProductTagService productTagService,
          IGenericAttributeService genericAttributeService,
          IAclService aclService,
          IStoreMappingService storeMappingService,
          IPermissionService permissionService,
          ICustomerActivityService customerActivityService,
          IEventPublisher eventPublisher,
          ISearchTermService searchTermService,
          IMeasureService measureService,
          MediaSettings mediaSettings,
          CatalogSettings catalogSettings,
          VendorSettings vendorSettings,
          BlogSettings blogSettings,
          ForumSettings forumSettings,
          IStoreService storeService,
          ITopicService topicService)
        {
            this._categoryService = categoryService;
            this._manufacturerService = manufacturerService;
            this._vendorsService = vendorService;
            this._workContext = workContext;
            this._storeContext = storeContext;
            this._mediaSettings = mediaSettings;
            this._pictureService = pictureService;
            this._localizationService = localizationService;
            this._topicService = topicService;
            this._productTagService = productTagService;
            this._aclService = aclService;
            this._storeMappingService = storeMappingService;
            this._catalogSettings = catalogSettings;
            this._storeService = storeService;
            this._webHelper = webHelper;
        }

        public async Task<IViewComponentResult> InvokeAsync(
          MenuItemModel menuItem)
        {
            MegaMenuTopicViewComponent topicViewComponent = this;
            MenuItemTopicModel menuItemTopicModel1 = new MenuItemTopicModel();
            menuItemTopicModel1.Item = menuItem;
            MenuItemTopicModel menuItemTopicModel2 = menuItemTopicModel1;
            menuItemTopicModel2.Topics = await topicViewComponent.PrepareTopicsAsync(menuItem.MaximumNumberOfEntities);
            MenuItemTopicModel menuItemTopicModel = menuItemTopicModel1;
            return ((ICollection<TopicModel>)menuItemTopicModel.Topics).Count != 0 ? (IViewComponentResult)topicViewComponent.View<MenuItemTopicModel>("TopicTemplate", menuItemTopicModel) : (IViewComponentResult)((ViewComponent)topicViewComponent).Content("");
        }

        private async Task<IList<TopicModel>> PrepareTopicsAsync(
          int maximumNumberOfEntities)
        {
            MegaMenuTopicViewComponent topicViewComponent = this;
            Store store = await topicViewComponent._storeContext.GetCurrentStoreAsync();
            ICustomerService icustomerService = topicViewComponent.CustomerService;
            int[] customerRoleIdsAsync = await icustomerService.GetCustomerRoleIdsAsync(await topicViewComponent._workContext.GetCurrentCustomerAsync(), false);
            int[] numArray = customerRoleIdsAsync;
            IStaticCacheManager istaticCacheManager = topicViewComponent.StaticCacheManager;
            CacheKey cacheKey = topicViewComponent.MegaMenuTopicsKey;
            object obj = (object)numArray;
            Language workingLanguageAsync = await topicViewComponent._workContext.GetWorkingLanguageAsync();
            CacheKey cacheKey1 = istaticCacheManager.PrepareKeyForDefaultCache(cacheKey, new object[3]
            {
        obj,
        (object) ((BaseEntity) workingLanguageAsync).Id,
        (object) ((BaseEntity) store).Id
            });


            return (IList<TopicModel>)await topicViewComponent.StaticCacheManager.GetAsync<List<TopicModel>>(cacheKey1, async () =>
            {
                return await this.PrepareTopicListModelAsync(maximumNumberOfEntities, store);
            });

        }

        private async Task<List<TopicModel>> PrepareTopicListModelAsync(int maximumNumberOfEntities, Store store)
        {
            List<TopicModel> lstTopicModel = new List<TopicModel>();

            var lstTopics = (await this._topicService.GetAllTopicsAsync(store.Id, false, false)).Take(maximumNumberOfEntities);
            foreach (var topic in lstTopics)
            {
                TopicModel topicModel1 = new TopicModel();
                ((BaseNopEntityModel)topicModel1).Id = ((BaseEntity)topic).Id;
                topicModel1.SystemName = topic.SystemName;
                topicModel1.IncludeInSitemap = topic.IncludeInSitemap;
                topicModel1.IsPasswordProtected = topic.IsPasswordProtected;

                string str1;
                if (topic.IsPasswordProtected)
                {
                    str1 = "";
                }
                else
                    str1 = await LocalizationService.GetLocalizedAsync<Topic, string>(topic, t => t.Title, null, true, true);
                topicModel1.Title = str1;

                string str2;
                if (topic.IsPasswordProtected)
                    str2 = "";
                else
                    str2 = await LocalizationService.GetLocalizedAsync<Topic, string>(topic, t => t.Body, null, true, true);
                topicModel1.Body = str2;

                topicModel1.MetaKeywords = await LocalizationService.GetLocalizedAsync<Topic, string>(topic, t => t.MetaKeywords, null, true, true);

                topicModel1.MetaDescription = await LocalizationService.GetLocalizedAsync<Topic, string>(topic, t => t.MetaDescription, null, true, true);

                topicModel1.MetaTitle = await LocalizationService.GetLocalizedAsync<Topic, string>(topic, t => t.MetaTitle, null, true, true);
                topicModel1.SeName = await this.UrlRecordService.GetSeNameAsync<Topic>(topic, new int?(), true, true);
                topicModel1.TopicTemplateId = topic.TopicTemplateId;
                lstTopicModel.Add(topicModel1);
            }

            return lstTopicModel;

        }
    }
}
