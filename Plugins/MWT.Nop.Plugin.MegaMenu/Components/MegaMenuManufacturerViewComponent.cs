
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Blogs;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Forums;
using Nop.Core.Domain.Media;
using Nop.Core.Domain.Stores;
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
using Nop.Web.Models.Catalog;
using Nop.Web.Models.Media;
using MWT.Nop.Plugin.MegaMenu.Domain.Enums;
using MWT.Nop.Plugin.MegaMenu.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MWT.Nop.Plugin.MegaMenu.Areas.Admin.Models;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;


#nullable enable
namespace MWT.Nop.Plugin.MegaMenu.Components
{
    public class MegaMenuManufacturerViewComponent : BaseComponent
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

        private CacheKey MegaMenuModelKey => new CacheKey("nop.pres.mwt.megamenu-common-{0}-{1}-{2}-{3}-{4}");

        public MegaMenuManufacturerViewComponent(
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
            MegaMenuManufacturerViewComponent manufacturerViewComponent = this;
            ICustomerService icustomerService = manufacturerViewComponent.CustomerService;
            int[] customerRoleIdsAsync = await icustomerService.GetCustomerRoleIdsAsync(await manufacturerViewComponent._workContext.GetCurrentCustomerAsync(), false);
            int[] numArray = customerRoleIdsAsync;
            IStaticCacheManager istaticCacheManager = manufacturerViewComponent.StaticCacheManager;
            CacheKey cacheKey = manufacturerViewComponent.MegaMenuModelKey;
            object obj1 = (object)numArray;
            object obj2 = (object)((BaseEntity)await manufacturerViewComponent._workContext.GetWorkingLanguageAsync()).Id;
            Store currentStoreAsync = await manufacturerViewComponent._storeContext.GetCurrentStoreAsync();
            CacheKey cacheKey1 = istaticCacheManager.PrepareKeyForDefaultCache(cacheKey, new object[5]
            {
        obj1,
        obj2,
        (object) ((BaseEntity) currentStoreAsync).Id,
        (object) manufacturerViewComponent._webHelper.IsCurrentConnectionSecured(),
        (object) menuItem.Id
            });

            MenuItemManufacturerModel async = await manufacturerViewComponent.StaticCacheManager.GetAsync<MenuItemManufacturerModel>(cacheKey1, (Func<Task<MenuItemManufacturerModel>>)(async () =>
            {
                (IList<ManufacturerModel> manufacturerModelList2, bool flag2) = await this.PrepareManufacturersModelAsync(menuItem);
                return new MenuItemManufacturerModel()
                {
                    Manufacturers = manufacturerModelList2,
                    Item = menuItem,
                    ShouldShowViewAllLink = flag2
                };
            }));
            if (((ICollection<ManufacturerModel>)async.Manufacturers).Count == 0)
                return (IViewComponentResult)((ViewComponent)manufacturerViewComponent).Content("");
            string str = "ManufacturerMenuTemplate." + menuItem.CatalogTemplate.ToString();
            return (IViewComponentResult)manufacturerViewComponent.View<MenuItemManufacturerModel>(str, async);
        }

        private async Task<(IList<ManufacturerModel> manufacturersModel, bool shouldShowViewAllLink)> PrepareManufacturersModelAsync(
          MenuItemModel menuItem)
        {
            List<ManufacturerModel> megaMenuManufacturerModels = new List<ManufacturerModel>();
            IManufacturerService imanufacturerService = this._manufacturerService;
            int numberOfEntities = menuItem.MaximumNumberOfEntities;
            IPagedList<Manufacturer> manufacturersAsync = await imanufacturerService.GetAllManufacturersAsync("", ((BaseEntity)await this._storeContext.GetCurrentStoreAsync()).Id, 0, numberOfEntities, false, new bool?());
            IPagedList<Manufacturer> manufacturers = manufacturersAsync;
            foreach (Manufacturer manufacturer in (IEnumerable<Manufacturer>)manufacturers)
            {
                ManufacturerModel manufacturerModel1 = new ManufacturerModel();
                manufacturerModel1.Id = manufacturer.Id;
                manufacturerModel1.MetaDescription = manufacturer.MetaDescription;
                manufacturerModel1.MetaKeywords = manufacturer.MetaKeywords;
                manufacturerModel1.MetaTitle = manufacturer.MetaTitle;
                manufacturerModel1.Name = manufacturer.Name;
                manufacturerModel1.Description = manufacturer.Description;
               

                //if (menuItem.CatalogTemplate == CatalogTemplate.WithPictures)
                //{
                //    ManufacturerModel manufacturerModel2 = manufacturerModel1;
                //    manufacturerModel2.PictureModel = await this.PreparePictureModelAsync(manufacturer.PictureId, menuItem.ImageSize, manufacturerModel1.Name);
                //    manufacturerModel2 = (ManufacturerModel)null;
                //}
                megaMenuManufacturerModels.Add(manufacturerModel1);
            }
            (IList<ManufacturerModel>, bool) valueTuple = ((IList<ManufacturerModel>)megaMenuManufacturerModels, manufacturers.TotalCount > menuItem.MaximumNumberOfEntities);
            return valueTuple;
        } 
    }
}
