using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Stores;
using Nop.Core.Domain.Topics;
using Nop.Core.Domain.Vendors;
using Nop.Services.Catalog;
using Nop.Services.Customers;
using Nop.Services.Localization;
using Nop.Services.Security;
using Nop.Services.Stores;
using Nop.Services.Topics;
using Nop.Services.Vendors;

using MWT.Nop.Plugin.MegaMenu.Domain;
using MWT.Nop.Plugin.MegaMenu.Domain.Enums;
using MWT.Nop.Plugin.MegaMenu.Areas.Admin.Extensions;
using MWT.Nop.Plugin.MegaMenu.Infrastructure.Constants;
using MWT.Nop.Plugin.MegaMenu.Models;
using MWT.Nop.Plugin.MegaMenu.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using MWT.Nop.Plugin.MegaMenu.Helpers;
using MWT.Nop.Plugin.MegaMenu.Areas.Admin.Models;
using Nop.Services.Media;
using Nop.Core.Domain.Media;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;

namespace MWT.Nop.Plugin.MegaMenu.Components
{
    public class MegaMenuViewComponent : BaseComponent
    {
        private readonly IPictureService _pictureService;
        private readonly MegaMenuSettings _megaMenuSettings;
        private readonly IMenuService _menuService;
        private readonly IMenuItemService _menuItemService;
        private readonly ICategoryService _categoryService;
        private readonly IManufacturerService _manufacturerService;
        private readonly IVendorService _vendorService;
        private readonly ITopicService _topicService;
        private readonly IProductTagService _productTagService;
        private readonly IStoreContext _storeContext;
        private readonly IStoreMappingService _storeMappingService;
        private readonly IEntityWidgetMappingService _entityWidgetMappingService;
        private readonly IAclService _aclService;
        private readonly IWorkContext _workContext;
        private readonly IWebHelper _webHelper;

        public MegaMenuViewComponent(
          MegaMenuSettings megaMenuSettings,
          IMenuService menuService,
          IMenuItemService menuItemService,
          ICategoryService categoryService,
          IManufacturerService manufacturerService,
          IVendorService vendorService,
          ITopicService topicService,
          IProductTagService productTagService,
          IStoreContext storeContext,
          IStoreMappingService storeMappingService,
          IEntityWidgetMappingService entityWidgetMappingService,
          IAclService aclService,
          IWorkContext workContext,
          IWebHelper webHelper,
          IPictureService pictureService)
        {
            this._megaMenuSettings = megaMenuSettings;
            this._menuService = menuService;
            this._menuItemService = menuItemService;
            this._categoryService = categoryService;
            this._manufacturerService = manufacturerService;
            this._vendorService = vendorService;
            this._topicService = topicService;
            this._productTagService = productTagService;
            this._storeContext = storeContext;
            this._storeMappingService = storeMappingService;
            this._entityWidgetMappingService = entityWidgetMappingService;
            this._aclService = aclService;
            this._workContext = workContext;
            this._webHelper = webHelper;
            this._pictureService = pictureService;
        }

        public async Task<IViewComponentResult> InvokeAsync(string widgetZone)
        {
            MegaMenuViewComponent menuViewComponent = this;
            if (!menuViewComponent._megaMenuSettings.Enabled)
                return (IViewComponentResult)((ViewComponent)menuViewComponent).Content("");
            // ISSUE: reference to a compiler-generated method
            if (!this.StaticCacheManager.Get<IDictionary<string, bool>>(CacheKeys.MenuWidgetMappingsCacheKey, (Func<IDictionary<string, bool>>)(() => (IDictionary<string, bool>)(this._entityWidgetMappingService.GetAllEntityWidgetMappingsByEntityTypeAsync(MWT.Nop.Plugin.MegaMenu.Infrastructure.Constants.Plugin.EntityType).Result).GroupBy<EntityWidgetMapping, string>((Func<EntityWidgetMapping, string>)(x => x.WidgetZone)).ToDictionary<IGrouping<string, EntityWidgetMapping>, string, bool>((Func<IGrouping<string, EntityWidgetMapping>, string>)(x => x.Key), (Func<IGrouping<string, EntityWidgetMapping>, bool>)(y => true)))).ContainsKey(widgetZone))
                return (IViewComponentResult)((ViewComponent)menuViewComponent).Content("");
            MegaMenuWidgetModel megaMenuWidgetModel = await menuViewComponent.PreparePublicModelAsync(widgetZone);
            return megaMenuWidgetModel.MegaMenus.Count != 0 ? (IViewComponentResult)menuViewComponent.View<MegaMenuWidgetModel>("MegaMenu", megaMenuWidgetModel) : (IViewComponentResult)((ViewComponent)menuViewComponent).Content("");
        }

        private async Task<MegaMenuWidgetModel> PreparePublicModelAsync(
          string widgetZone)
        {
            MegaMenuWidgetModel megaMenuWidgetModel1 = new MegaMenuWidgetModel();
            MegaMenuWidgetModel megaMenuWidgetModel2 = megaMenuWidgetModel1;
            megaMenuWidgetModel2.MegaMenus = await this.GetDefaultCachedModelAsync(widgetZone);
            MegaMenuWidgetModel megaMenuWidgetModel3 = megaMenuWidgetModel1;
            megaMenuWidgetModel3.Theme = await ThemeHelper.GetPluginThemeAsync("MWT.Nop.Plugin.MegaMenu");
            return megaMenuWidgetModel1;
        }

        private async Task<IList<MenuModel>> GetDefaultCachedModelAsync(
          string widgetZone)
        {
            MegaMenuViewComponent menuViewComponent = this;
            ICustomerService icustomerService = menuViewComponent.CustomerService;
            int[] customerRoleIdsAsync = await icustomerService.GetCustomerRoleIdsAsync(await menuViewComponent._workContext.GetCurrentCustomerAsync(), false);
            icustomerService = (ICustomerService)null;
            int[] numArray = customerRoleIdsAsync;
            IStaticCacheManager istaticCacheManager = menuViewComponent.StaticCacheManager;
            CacheKey cacheKey = CacheKeys.MenuModelCacheKey;
            object obj1 = (object)widgetZone;
            object obj2 = (object)numArray;
            object obj3 = (object)((BaseEntity)await menuViewComponent._workContext.GetWorkingLanguageAsync()).Id;
            Store currentStoreAsync = await menuViewComponent._storeContext.GetCurrentStoreAsync();
            CacheKey cacheKey1 = istaticCacheManager.PrepareKeyForDefaultCache(cacheKey, new object[5]
            {
        obj1,
        obj2,
        obj3,
        (object) ((BaseEntity) currentStoreAsync).Id,
        (object) menuViewComponent._webHelper.IsCurrentConnectionSecured()
            });
            istaticCacheManager = (IStaticCacheManager)null;
            cacheKey = (CacheKey)null;
            obj1 = (object)null;
            obj2 = (object)null;
            obj3 = (object)null;
            return await menuViewComponent.StaticCacheManager.GetAsync<IList<MenuModel>>(cacheKey1, (Func<Task<IList<MenuModel>>>)(async () => await this.GetDefaultModelAsync(widgetZone)));
        }

        private async Task<IList<MenuModel>> GetDefaultModelAsync(string widgetZone)
        {
            IList<Menu> menus = (await this.GetAvailableMenusForWidgetZoneAsync(widgetZone)).Where(m=>m.Enabled).ToList();
            IList<MenuItem> allMenuItems = await this._menuItemService.GetAllItemsAsync();
            List<MenuModel> menuModels = new List<MenuModel>();
            if (menus.Count == 0)
                return (IList<MenuModel>)menuModels;
            foreach (Menu menu1 in (IEnumerable<Menu>)menus)
            {
                Menu menu = menu1;
                IList<MenuItem> list = (IList<MenuItem>)allMenuItems.Where<MenuItem>((Func<MenuItem, bool>)(x =>
                {
                    int? menuId = x.MenuId;
                    int id = menu.Id;
                    return menuId.GetValueOrDefault() == id & menuId.HasValue;
                })).OrderBy<MenuItem, int>((Func<MenuItem, int>)(x => x.DisplayOrder)).ToList<MenuItem>();
                menuModels.Add(await this.PrepareMenuModelAsync(menu, list));
            }
            return (IList<MenuModel>)menuModels;
        }

        private async Task<MenuModel> PrepareMenuModelAsync(
          Menu menu,
          IList<MenuItem> currentMenuItems)
        {
            MenuModel menuModel1 = menu.ToModel<MenuModel>();
            MenuModel menuModel2 = menuModel1;
            menuModel2.Items = await this.PrepareMenuItemsAsync(0, currentMenuItems);
            menuModel2 = (MenuModel)null;
            MenuModel menuModel = menuModel1;
            menuModel1 = (MenuModel)null;
            return menuModel;
        }

        private async Task<IList<MenuItemModel>> PrepareMenuItemsAsync(
          int parentMenuItemId,
          IList<MenuItem> currentMenuItems)
        {
            MegaMenuViewComponent menuViewComponent = this;
            List<MenuItemModel> menuItemModels = new List<MenuItemModel>();
            foreach (MenuItem menuItem1 in currentMenuItems.Where<MenuItem>((Func<MenuItem, bool>)(x => x.ParentMenuItemId == parentMenuItemId)))
            {
                MenuItem menuItem = menuItem1;
                if (await menuViewComponent._aclService.AuthorizeAsync<MenuItem>(menuItem))
                {
                    MenuItemModel menuItemModel1 = menuItem.ToModel<MenuItemModel>();

                    ILocalizationService localizationService1 = menuViewComponent.LocalizationService;

                    Expression<Func<MenuItem, string>> expression1 = (Expression<Func<MenuItem, string>>)(x => x.Title);
                    int? nullable1 = new int?();
                    menuItemModel1.Title = await localizationService1.GetLocalizedAsync<MenuItem, string>(menuItem, expression1, nullable1, true, true);


                    ILocalizationService localizationService2 = menuViewComponent.LocalizationService;

                    Expression<Func<MenuItem, string>> expression2 = (Expression<Func<MenuItem, string>>)(x => x.Url);
                    int? nullable2 = new int?();
                    menuItemModel1.Url = await localizationService2.GetLocalizedAsync<MenuItem, string>(menuItem1, expression2, nullable2, true, true);

                    bool shouldIncludeItem = true;
                    IUrlHelper helper;
                    switch (menuItem.Type)
                    {
                        case MenuItemType.Categories:
                            if (menuItemModel1.EntityId == 0)
                            {
                                shouldIncludeItem = menuItemModel1.MaximumNumberOfEntities >= 0;
                                break;
                            }
                            Category category = await menuViewComponent._categoryService.GetCategoryByIdAsync(menuItemModel1.EntityId);
                            bool flag1 = category != null && !category.Deleted && category.Published;
                            if (flag1)
                                flag1 = await menuViewComponent._aclService.AuthorizeAsync<Category>(category);
                            if (flag1)
                            {
                                ILocalizationService localizationService3 = menuViewComponent.LocalizationService;
                                Category category1 = category;
                                menuItemModel1.Title = await localizationService3.GetLocalizedAsync<Category, string>(category1, c => c.Name, null, true, true);

                                helper = ((ViewComponent)menuViewComponent).Url;
                                menuItemModel1.Url = helper.RouteUrl("Category", (object)new
                                {
                                    SeName = await menuViewComponent.UrlRecordService.GetSeNameAsync<Category>(category, new int?(), true, true),
                                    id = category.Id
                                });

                            }
                            else
                                shouldIncludeItem = false;

                            break;
                        case MenuItemType.Manufacturers:
                            if (menuItemModel1.EntityId == 0)
                            {
                                if (menuItemModel1.CatalogTemplate == CatalogTemplate.Simple)
                                    menuItemModel1.Url = UrlHelperExtensions.RouteUrl(((ViewComponent)menuViewComponent).Url, "ManufacturerList");
                                shouldIncludeItem = menuItemModel1.CatalogTemplate == CatalogTemplate.Simple || menuItemModel1.MaximumNumberOfEntities > 0;
                                break;
                            }
                            Manufacturer manufacturer = await menuViewComponent._manufacturerService.GetManufacturerByIdAsync(menuItemModel1.EntityId);
                            bool flag2 = manufacturer != null && !manufacturer.Deleted && manufacturer.Published;
                            if (flag2)
                                flag2 = await menuViewComponent._aclService.AuthorizeAsync<Manufacturer>(manufacturer);
                            if (flag2)
                            {
                                ILocalizationService localizationService4 = menuViewComponent.LocalizationService;
                                Manufacturer manufacturer1 = manufacturer;
                                menuItemModel1.Title = await localizationService4.GetLocalizedAsync<Manufacturer, string>(manufacturer1, m => m.Name, null, true, true);

                                helper = ((ViewComponent)menuViewComponent).Url;
                                menuItemModel1.Url = helper.RouteUrl("Manufacturer", (object)new
                                {
                                    SeName = await menuViewComponent.UrlRecordService.GetSeNameAsync<Manufacturer>(manufacturer, new int?(), true, true)
                                });

                            }
                            else
                                shouldIncludeItem = false;

                            break;
                        case MenuItemType.Vendors:
                            if (menuItemModel1.EntityId == 0)
                            {
                                if (menuItemModel1.CatalogTemplate == CatalogTemplate.Simple)
                                    menuItemModel1.Url = UrlHelperExtensions.RouteUrl(((ViewComponent)menuViewComponent).Url, "VendorList");
                                shouldIncludeItem = menuItemModel1.CatalogTemplate == CatalogTemplate.Simple || menuItemModel1.MaximumNumberOfEntities > 0;
                                break;
                            }
                            Vendor vendor = await menuViewComponent._vendorService.GetVendorByIdAsync(menuItemModel1.EntityId);
                            if (vendor != null && !vendor.Deleted && vendor.Active)
                            {
                                ILocalizationService localizationService5 = menuViewComponent.LocalizationService;
                                Vendor vendor1 = vendor;
                                menuItemModel1.Title = await localizationService5.GetLocalizedAsync<Vendor, string>(vendor1, v => v.Name, null, true, true);

                                helper = ((ViewComponent)menuViewComponent).Url;
                                menuItemModel1.Url = helper.RouteUrl("Vendor", (object)new
                                {
                                    SeName = await menuViewComponent.UrlRecordService.GetSeNameAsync<Vendor>(vendor, new int?(), true, true)
                                });

                            }
                            else
                                shouldIncludeItem = false;

                            break;
                        case MenuItemType.Topics:
                            if (menuItemModel1.EntityId == 0)
                            {
                                shouldIncludeItem = menuItemModel1.MaximumNumberOfEntities > 0;
                                break;
                            }
                            Topic topic = await menuViewComponent._topicService.GetTopicByIdAsync(menuItemModel1.EntityId);
                            bool flag3 = topic != null && topic.Published;
                            if (flag3)
                                flag3 = await menuViewComponent._aclService.AuthorizeAsync<Topic>(topic);
                            if (flag3)
                            {
                                ILocalizationService localizationService6 = menuViewComponent.LocalizationService;
                                Topic topic1 = topic;
                                menuItemModel1.Title = await localizationService6.GetLocalizedAsync<Topic, string>(topic1, t => t.Title, null, true, true);

                                helper = ((ViewComponent)menuViewComponent).Url;
                                menuItemModel1.Url = helper.RouteUrl("Topic", (object)new
                                {
                                    SeName = await menuViewComponent.UrlRecordService.GetSeNameAsync<Topic>(topic, new int?(), true, true)
                                });

                                shouldIncludeItem = true;
                            }
                            else
                                shouldIncludeItem = false;

                            break;
                        case MenuItemType.ProductTags:
                            if (menuItemModel1.EntityId == 0)
                            {
                                shouldIncludeItem = menuItemModel1.CatalogTemplate == CatalogTemplate.Simple || menuItemModel1.MaximumNumberOfEntities > 0;
                                break;
                            }
                            ProductTag productTag = await menuViewComponent._productTagService.GetProductTagByIdAsync(menuItemModel1.EntityId);
                            if (productTag != null)
                            {
                                ILocalizationService localizationService7 = menuViewComponent.LocalizationService;
                                ProductTag productTag1 = productTag;
                                menuItemModel1.Title = await localizationService7.GetLocalizedAsync<ProductTag, string>(productTag1, pt => pt.Name, null, true, true);

                                helper = ((ViewComponent)menuViewComponent).Url;
                                int id = ((BaseEntity)productTag).Id;
                                menuItemModel1.Url = helper.RouteUrl("ProductsByTag", (object)new
                                {
                                    productTagId = id,
                                    SeName = await menuViewComponent.UrlRecordService.GetSeNameAsync<ProductTag>(productTag, new int?(), true, true)
                                });

                            }
                            else
                                shouldIncludeItem = false;

                            break;
                        case MenuItemType.CustomLink:
                            if (menuItemModel1.PictureId != 0)
                            {
                                int pictureSize = 776;
                                if (menuItem.ImageSize != 0)
                                    pictureSize = menuItem.ImageSize;
                                menuItemModel1.ImageUrl = await this.PreparePictureModelAsync(menuItemModel1.PictureId, pictureSize, menuItemModel1.Title == "" ? menuItemModel1.Id.ToString() : menuItemModel1.Title);
                            }
                            break;

                    }
                    menuItemModel1.SubItems = await menuViewComponent.PrepareMenuItemsAsync(menuItemModel1.Id, currentMenuItems);

                    if (shouldIncludeItem)
                        menuItemModels.Add(menuItemModel1);


                }
            }
            IList<MenuItemModel> menuItemModelList = (IList<MenuItemModel>)menuItemModels;

            return menuItemModelList;
        }
        private async Task<string> PreparePictureModelAsync(
         int pictureId,
         int pictureSize,
         string name)
        {
            string imageUrl = "";
            imageUrl = await this._pictureService.GetPictureUrlAsync(pictureId, 0, true, (string)null, (PictureType)1);
            return imageUrl;
        }

        private async Task<IList<Menu>> GetAvailableMenusForWidgetZoneAsync(
      string widgetZone)
        {
            MegaMenuViewComponent menuViewComponent = this;

            var menus = await this._menuService.GetMenusByWidgetZoneAsync(widgetZone);
            IList<Menu> _menus = new List<Menu>();
            foreach (var _menu in menus)
            {
                if (!_menu.LimitedToStores || await this._storeMappingService.AuthorizeAsync<Menu>(_menu))
                    _menus.Add(_menu);
            }
            return _menus;

        }

    }
}
