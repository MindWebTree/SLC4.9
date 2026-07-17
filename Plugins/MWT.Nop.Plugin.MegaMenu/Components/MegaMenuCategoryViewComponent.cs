using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Media;
using Nop.Core.Domain.Stores;
using Nop.Services.Catalog;
using Nop.Services.Customers;
using Nop.Services.Localization;
using Nop.Services.Media;
using Nop.Services.Security;
using Nop.Services.Stores;
using Nop.Web.Framework.Models;
using Nop.Web.Models.Catalog;
using Nop.Web.Models.Media;
using MWT.Nop.Plugin.MegaMenu.Domain.Enums;
using MWT.Nop.Plugin.MegaMenu.Helpers;
using MWT.Nop.Plugin.MegaMenu.Infrastructure.Constants;
using MWT.Nop.Plugin.MegaMenu.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using MWT.Nop.Plugin.MegaMenu.Areas.Admin.Models;


namespace MWT.Nop.Plugin.MegaMenu.Components
{
    public class MegaMenuCategoryViewComponent : BaseComponent
    {
        private readonly ICategoryService _categoryService;
        private readonly IWorkContext _workContext;
        private readonly IStoreContext _storeContext;
        private readonly MediaSettings _mediaSettings;
        private readonly IPictureService _pictureService;
        private readonly ILocalizationService _localizationService;
        private readonly IAclService _aclService;
        private readonly IStoreService _storeService;
        private readonly IStoreMappingService _storeMappingService;
        private readonly IWebHelper _webHelper;
        private readonly CatalogSettings _catalogSettings;
        private IMegaMenuCategoryCounterHelper _megaMenuCategoryCounterHelper;

        public MegaMenuCategoryViewComponent(
          IWorkContext workContext,
          IStoreContext storeContext,
          ILocalizationService localizationService,
          IWebHelper webHelper,
          CatalogSettings catalogSettings,
          IStaticCacheManager staticCacheManager,
          ICategoryService categoryService,
          MediaSettings mediaSettings,
          IPictureService pictureService,
          IAclService aclService,
          IStoreMappingService storeMappingService,
          IStoreService storeService,
          IMegaMenuCategoryCounterHelper megaMenuCategoryCounterHelper)
        {
            this._categoryService = categoryService;
            this._workContext = workContext;
            this._storeContext = storeContext;
            this._mediaSettings = mediaSettings;
            this._pictureService = pictureService;
            this._localizationService = localizationService;
            this._aclService = aclService;
            this._storeMappingService = storeMappingService;
            this._catalogSettings = catalogSettings;
            this._storeService = storeService;
            this._webHelper = webHelper;
            this._megaMenuCategoryCounterHelper = megaMenuCategoryCounterHelper;
        }

        public async Task<IViewComponentResult> InvokeAsync(MenuItemModel menuItem,
          bool isResponsive = false)
        {
            MegaMenuCategoryViewComponent categoryViewComponent = this;
            ICustomerService icustomerService = categoryViewComponent.CustomerService;
            int[] customerRoleIdsAsync = await icustomerService.GetCustomerRoleIdsAsync(await categoryViewComponent._workContext.GetCurrentCustomerAsync(), false);
            icustomerService = (ICustomerService)null;
            int[] numArray = customerRoleIdsAsync;
            IStaticCacheManager istaticCacheManager = categoryViewComponent.StaticCacheManager;
            CacheKey cacheKey = CacheKeys.MegaMenuCategoriesKey;
            object obj1 = (object)numArray;
            object obj2 = (object)((BaseEntity)await categoryViewComponent._workContext.GetWorkingLanguageAsync()).Id;
            Store currentStoreAsync = await categoryViewComponent._storeContext.GetCurrentStoreAsync();
            CacheKey cacheKey1 = istaticCacheManager.PrepareKeyForDefaultCache(cacheKey, new object[6]
            {
        obj1,
        obj2,
        (object) ((BaseEntity) currentStoreAsync).Id,
        (object) categoryViewComponent._webHelper.IsCurrentConnectionSecured(),
        (object) isResponsive,
        (object) menuItem.Id
            });
        
            MenuItemCategoryModel async = await categoryViewComponent.StaticCacheManager.GetAsync<MenuItemCategoryModel>(cacheKey1, (Func<Task<MenuItemCategoryModel>>)(async () =>
            {
                MenuItemCategoryModel itemCategoryModel1 = new MenuItemCategoryModel();
                itemCategoryModel1.Item = menuItem;
                MenuItemCategoryModel itemCategoryModel2 = itemCategoryModel1;
                itemCategoryModel2.Categories = await this.PrepareCategoriesModelAsync(menuItem, isResponsive);
                MenuItemCategoryModel itemCategoryModel = itemCategoryModel1;
                itemCategoryModel2 = (MenuItemCategoryModel)null;
                itemCategoryModel1 = (MenuItemCategoryModel)null;
                return itemCategoryModel;
            }));
            if (async.Item.EntityId == 0 && async.Categories.Count == 0)
                return (IViewComponentResult)((ViewComponent)categoryViewComponent).Content("");
            if (async.Item.EntityId != 0 && async.Item.MenuId.HasValue && !isResponsive)
            {
                int menuId = async.Item.MenuId.Value;
                async.CategoryMenuItemIndex = categoryViewComponent._megaMenuCategoryCounterHelper.GetCategoryCount(menuId);
            }
            string str = "CategoryMenuTemplate." + menuItem.CatalogTemplate.ToString();
            return (IViewComponentResult)categoryViewComponent.View<MenuItemCategoryModel>(str, async);
        }

        private async Task<IList<MegaMenuCategoryModel>> PrepareCategoriesModelAsync(
          MenuItemModel menuItem,
          bool isResponsive)
        {
            List<MegaMenuCategoryModel> megaMenuCategoriesModels = new List<MegaMenuCategoryModel>();// Fetch all categories without filtering by the legacy IncludeInTopMenu flag
            IList<Category> allCategories = await _categoryService.GetAllCategoriesAsync(showHidden: false);
           // IList<Category> allCategories = (IList<Category>)((IEnumerable<Category>)await this._categoryService.GetAllCategoriesAsync(0, false)).Where<Category>((Func<Category, bool>)(c => c.IncludeInTopMenu)).ToList<Category>();
            if (!this._catalogSettings.IgnoreAcl)
                allCategories = (IList<Category>)await AsyncIEnumerableExtensions.WhereAwait<Category>((IEnumerable<Category>)allCategories, (Func<Category, ValueTask<bool>>)(async c => await this._aclService.AuthorizeAsync<Category>(c))).ToListAsync<Category>();
            bool flag = !this._catalogSettings.IgnoreStoreLimitations;
            if (flag)
                flag = ((ICollection<Store>)await this._storeService.GetAllStoresAsync()).Count > 1;
            if (flag)
                allCategories = (IList<Category>)await AsyncIEnumerableExtensions.WhereAwait<Category>((IEnumerable<Category>)allCategories, (Func<Category, ValueTask<bool>>)(async c => await this._storeMappingService.AuthorizeAsync<Category>(c))).ToListAsync<Category>();
            IList<Category> list = (IList<Category>)((IEnumerable<Category>)allCategories).Where<Category>((Func<Category, bool>)(c => c.ParentCategoryId == menuItem.EntityId)).ToList<Category>();
            //if (!isResponsive && menuItem.CatalogTemplate == CatalogTemplate.List && ((ICollection<Category>)list).Count > menuItem.MaximumNumberOfEntities)
            //    list = (IList<Category>)((IEnumerable<Category>)list).Take<Category>(menuItem.MaximumNumberOfEntities).ToList<Category>();
            foreach (Category category in (IEnumerable<Category>)list)
                megaMenuCategoriesModels.Add(await this.BuildCategoryModelRecursiveAsync(menuItem, category, allCategories, 1));
            IList<MegaMenuCategoryModel> menuCategoryModelList = (IList<MegaMenuCategoryModel>)megaMenuCategoriesModels;
            megaMenuCategoriesModels = (List<MegaMenuCategoryModel>)null;
            allCategories = (IList<Category>)null;
            return menuCategoryModelList;
        }

        private async Task<MegaMenuCategoryModel> BuildCategoryModelRecursiveAsync(
          MenuItemModel menuItem,
          Category category,
          IList<Category> allCategories,
          int level)
        {
            MegaMenuCategoryViewComponent categoryViewComponent = this;
            MegaMenuCategoryModel megaMenuCategoryModel = new MegaMenuCategoryModel();
            if ((menuItem.EntityId == 0 && level > 2 || menuItem.EntityId != 0 && level > 3))
                return megaMenuCategoryModel;
            CategoryModel categoryModel = megaMenuCategoryModel.CategoryModel;
            ILocalizationService localizationService = categoryViewComponent._localizationService;
        
           
            // ISSUE: method reference
              int? nullable = new int?();
            categoryModel.Name = await localizationService.GetLocalizedAsync<Category, string>(category, x=>x.Name, nullable, true, true);
            categoryModel = (CategoryModel)null;
            ((BaseNopEntityModel)megaMenuCategoryModel.CategoryModel).Id = ((BaseEntity)category).Id;
            megaMenuCategoryModel.CategoryModel.MetaDescription = category.MetaDescription;
            megaMenuCategoryModel.CategoryModel.MetaKeywords = category.MetaKeywords;
            megaMenuCategoryModel.CategoryModel.MetaTitle = category.MetaTitle;
            //if (menuItem.CatalogTemplate == CatalogTemplate.WithPictures)
            //{
            //    int pictureSize = categoryViewComponent._mediaSettings.ProductThumbPictureSize;
            //    if (menuItem.ImageSize != 0)
            //        pictureSize = menuItem.ImageSize;
            //    categoryModel = megaMenuCategoryModel.CategoryModel;
            //    categoryModel.PictureModel = await categoryViewComponent.PreparePictureModelAsync(category.PictureId, pictureSize, megaMenuCategoryModel.CategoryModel.Name);
            //    categoryModel = (CategoryModel)null;
            //    megaMenuCategoryModel.CategoryModel.Description = category.Description;
            //}
            categoryModel = megaMenuCategoryModel.CategoryModel;
            categoryModel.SeName = await categoryViewComponent.UrlRecordService.GetSeNameAsync<Category>(category, new int?(), true, true);
            categoryModel = (CategoryModel)null;
            IList<Category> list = (IList<Category>)((IEnumerable<Category>)((IEnumerable<Category>)allCategories).Where<Category>((Func<Category, bool>)(c => c.ParentCategoryId == ((BaseEntity)category).Id)).OrderBy<Category, int>((Func<Category, int>)(x => x.DisplayOrder))).ToList<Category>();
            int count = ((ICollection<Category>)list).Count;
            bool flag = level == 1 && menuItem.MaximumNumberOfEntities == 0;
            if (count > menuItem.MaximumNumberOfEntities || !flag)
                list = (IList<Category>)((IEnumerable<Category>)list).Take<Category>(menuItem.MaximumNumberOfEntities).ToList<Category>();
            if (count > menuItem.MaximumNumberOfEntities && !flag)
                megaMenuCategoryModel.ShouldShowViewAllLink = true;
            if (((IEnumerable<Category>)list).Any<Category>())
            {
                foreach (Category category2 in (IEnumerable<Category>)list)
                {
                    IList<MegaMenuCategoryModel> menuCategoryModelList = megaMenuCategoryModel.SubCategories;
                    menuCategoryModelList.Add(await categoryViewComponent.BuildCategoryModelRecursiveAsync(menuItem, category2, allCategories, level + 1));
                    menuCategoryModelList = (IList<MegaMenuCategoryModel>)null;
                }
            }
            return megaMenuCategoryModel;
        }

        private async Task<PictureModel> PreparePictureModelAsync(
          int pictureId,
          int pictureSize,
          string name)
        {
            PictureModel pictureModel1 = new PictureModel();
            PictureModel pictureModel2 = pictureModel1;
            pictureModel2.ImageUrl = await this._pictureService.GetPictureUrlAsync(pictureId, pictureSize, true, (string)null, (PictureType)1);
            pictureModel2 = (PictureModel)null;
            pictureModel2 = pictureModel1;
            pictureModel2.AlternateText = string.Format(await this._localizationService.GetResourceAsync("Media.Category.ImageAlternateTextFormat"), (object)name);
            pictureModel2 = (PictureModel)null;
            pictureModel2 = pictureModel1;
            pictureModel2.Title = string.Format(await this._localizationService.GetResourceAsync("Media.Category.ImageLinkTitleFormat"), (object)name);
            pictureModel2 = (PictureModel)null;
            PictureModel pictureModel = pictureModel1;
            pictureModel1 = (PictureModel)null;
            return pictureModel;
        }
    }
}
