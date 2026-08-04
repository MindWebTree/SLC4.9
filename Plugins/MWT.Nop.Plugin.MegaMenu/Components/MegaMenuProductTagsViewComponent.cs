using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Stores;
using Nop.Services.Catalog;
using Nop.Services.Customers;
using Nop.Services.Localization;
using Nop.Web.Framework.Models;
using Nop.Web.Models.Catalog;
using MWT.Nop.Plugin.MegaMenu.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using MWT.Nop.Plugin.MegaMenu.Areas.Admin.Models;
using MWT.Nop.Core.Services.Customers;


#nullable enable
namespace MWT.Nop.Plugin.MegaMenu.Components
{
    public class MegaMenuProductTagsViewComponent : BaseComponent
    {
        private readonly CatalogSettings _catalogSettings;
        private readonly ILocalizationService _localizationService;
        private readonly IProductTagService _productTagService;
        private readonly IStaticCacheManager _staticCacheManager;
        private readonly IStoreContext _storeContext;
        private readonly IWebHelper _webHelper;
        private readonly IWorkContext _workContext;

        private CacheKey MegaMenuModelKey => new CacheKey("nop.pres.mwt.megamenu-common-{0}-{1}-{2}-{3}-{4}");

        public MegaMenuProductTagsViewComponent(
          IWorkContext workContext,
          IStoreContext storeContext,
          ILocalizationService localizationService,
          IWebHelper webHelper,
          CatalogSettings catalogSettings,
          IProductTagService productTagService,
          IStaticCacheManager staticCacheManager)
        {
            this._workContext = workContext;
            this._storeContext = storeContext;
            this._localizationService = localizationService;
            this._catalogSettings = catalogSettings;
            this._webHelper = webHelper;
            this._staticCacheManager = staticCacheManager;
            this._productTagService = productTagService;
        }

        public async Task<IViewComponentResult> InvokeAsync(
          MenuItemModel menuItem)
        {
            MegaMenuProductTagsViewComponent tagsViewComponent = this;
            ICustomerService icustomerService = tagsViewComponent.CustomerService;
            int[] customerRoleIdsAsync = await icustomerService.GetCustomerRoleIdsAsync(await tagsViewComponent._workContext.GetCurrentCustomerAsync(), false);
            int[] numArray = customerRoleIdsAsync;
            IStaticCacheManager istaticCacheManager = tagsViewComponent.StaticCacheManager;
            CacheKey cacheKey = tagsViewComponent.MegaMenuModelKey;
            object obj1 = (object)numArray;
            object obj2 = (object)((BaseEntity)await tagsViewComponent._workContext.GetWorkingLanguageAsync()).Id;
            Store currentStoreAsync = await tagsViewComponent._storeContext.GetCurrentStoreAsync();
            CacheKey cacheKey1 = istaticCacheManager.PrepareKeyForDefaultCache(cacheKey, new object[5]
            {
        obj1,
        obj2,
        (object) ((BaseEntity) currentStoreAsync).Id,
        (object) tagsViewComponent._webHelper.IsCurrentConnectionSecured(),
        (object) menuItem.Id
            });

            MenuItemProductTagsModel async = await tagsViewComponent._staticCacheManager.GetAsync<MenuItemProductTagsModel>(cacheKey1, (Func<Task<MenuItemProductTagsModel>>)(async () =>
            {
                (IList<ProductTagModel> productTagModelList2, bool flag2) = await this.PrepareProductTagModelsAsync(menuItem);
                return new MenuItemProductTagsModel()
                {
                    ProductTags = productTagModelList2,
                    Item = menuItem,
                    ShouldShowViewAllLink = flag2
                };
            }));
            return ((ICollection<ProductTagModel>)async.ProductTags).Count != 0 ? (IViewComponentResult)tagsViewComponent.View<MenuItemProductTagsModel>("ProductTagsTemplate", async) : (IViewComponentResult)((ViewComponent)tagsViewComponent).Content("");
        }

        private async Task<(IList<ProductTagModel> megaMenuProductTagModels, bool shouldShowViewAllLink)> PrepareProductTagModelsAsync(
          MenuItemModel menuItem)
        {
            Store store = await this._storeContext.GetCurrentStoreAsync();
            ParameterExpression parameterExpression1;
            // ISSUE: method reference
            IList<ProductTag> source = (await this._productTagService.GetAllProductTagsAsync()).OrderBy(X => X.Id).ToList();
            bool shouldShowViewAllLink = false;
            if (((ICollection<ProductTag>)source).Count > menuItem.MaximumNumberOfEntities)
            {
                source = (IList<ProductTag>)((IEnumerable<ProductTag>)source).Take<ProductTag>(menuItem.MaximumNumberOfEntities).ToList<ProductTag>();
                shouldShowViewAllLink = true;
            }
            return ((IList<ProductTagModel>)await AsyncIEnumerableExtensions.SelectAwait<ProductTag, ProductTagModel>
                ((IEnumerable<ProductTag>)source, (Func<ProductTag, ValueTask<ProductTagModel>>)(async x =>
            {
                ProductTagModel productTagModel1 = new ProductTagModel();
                ((BaseNopEntityModel)productTagModel1).Id = ((BaseEntity)x).Id;
                ProductTagModel productTagModel2 = productTagModel1;
                ILocalizationService localizationService = this._localizationService;
                ProductTag productTag = x;
                int? nullable = new int?();
                productTagModel2.Name = await localizationService.GetLocalizedAsync<ProductTag, string>(productTag, x => x.Name, nullable, true, true);
                ProductTagModel productTagModel3 = productTagModel1;
                productTagModel3.SeName = await this.UrlRecordService.GetSeNameAsync<ProductTag>(x, new int?(), true, true);
                ProductTagModel productTagModel4 = productTagModel1;
                productTagModel4.ProductCount = await this._productTagService.GetProductCountByProductTagIdAsync(((BaseEntity)x).Id, ((BaseEntity)store).Id, false);
                ProductTagModel productTagModel = productTagModel1;

                return productTagModel;
            })).ToListAsync<ProductTagModel>(), shouldShowViewAllLink);
        }
    }
}
