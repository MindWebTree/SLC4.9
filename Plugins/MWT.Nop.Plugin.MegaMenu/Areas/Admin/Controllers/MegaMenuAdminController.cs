using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using MWT.Nop.Plugin.MegaMenu.Areas.Admin.Attributes;
using MWT.Nop.Plugin.MegaMenu.Areas.Admin.Models;
using MWT.Nop.Plugin.MegaMenu.Domain;
using MWT.Nop.Plugin.MegaMenu.Domain.Enums;
using MWT.Nop.Plugin.MegaMenu.Helpers;
using MWT.Nop.Plugin.MegaMenu.Services;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Cms;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Security;
using Nop.Core.Domain.Stores;
using Nop.Core.Infrastructure;
using Nop.Services.Catalog;
using Nop.Services.Configuration;
using Nop.Services.Customers;
using Nop.Services.Localization;
using Nop.Services.Security;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.Filters;
using System.Linq.Expressions;
using Category = Nop.Core.Domain.Catalog.Category;
using Nop.Web.Framework.Models.Extensions;
using MWT.Nop.Core.Services.Customers;


#nullable enable

namespace MWT.Nop.Plugin.MegaMenu.Areas.Admin.Controllers
{
    //[ManagePluginsAdminAuthorize("MWT.Nop.Plugin.MegaMenu", false)]
    public class MegaMenuAdminController :MWT.Nop.Plugin.MegaMenu.Areas.Admin.Controllers.BaseMWTAdminController
    {
        private MegaMenuSettings _megaMenuSettings;
        private readonly WidgetSettings _widgetSettings;
        private readonly IWorkContext _workContext;
        private readonly ISettingService _settingService;
        private readonly ILocalizationService _localizationService;
        private readonly IMenuService _menuService;
        private readonly IMenuItemService _menuItemService;
        private readonly IStaticCacheManager _cacheManager;
        private readonly ILocalizedEntityService _localizedEntityService;
        private readonly IInstallHelper _installHelper;
        private readonly IEntityWidgetMappingService _entityWidgetMappingService;
        private readonly ICustomerService _customerService;
        private readonly IAclService _aclService;
        private readonly CatalogSettings _catalogSettings;
        private readonly ILanguageService _languageService;
        private readonly ICategoryService _categoryService;

        public MegaMenuAdminController(
          MegaMenuSettings megaMenuSettings,
          WidgetSettings widgetSettings,
          IWorkContext workContext,
          ISettingService settingService,
          ILocalizationService localizationService,
          IMenuService menuService,
          IMenuItemService menuItemService,
          ILocalizedEntityService localizedEntityService,
          IInstallHelper installHelper,
          IEntityWidgetMappingService entityWidgetMappingService,
          ICustomerService customerService,
          IAclService aclService,
          CatalogSettings catalogSettings,
          ILanguageService languageService,
          ICategoryService categoryService)
        {
            this._cacheManager = EngineContext.Current.Resolve<IStaticCacheManager>();
            this._megaMenuSettings = megaMenuSettings;
            this._widgetSettings = widgetSettings;
            this._workContext = workContext;
            this._settingService = settingService;
            this._localizationService = localizationService;
            this._menuService = menuService;
            this._menuItemService = menuItemService;
            this._localizedEntityService = localizedEntityService;
            this._installHelper = installHelper;
            this._entityWidgetMappingService = entityWidgetMappingService;
            this._customerService = customerService;
            this._aclService = aclService;
            this._catalogSettings = catalogSettings;
            this._languageService = languageService;
            this._categoryService = categoryService;
        }

        public async Task<ActionResult> Settings()
        {
            int storeScope = await StoreContext.GetActiveStoreScopeConfigurationAsync();
            MegaMenuSettings entity = await _settingService.LoadSettingAsync<MegaMenuSettings>(storeScope);
            MegaMenuSettingsModel model = entity.ToModel<MegaMenuSettingsModel>();
            model.ActiveStoreScopeConfiguration = storeScope;
            if (storeScope > 0)
            {
                StoreScopeSettingsHelper<MegaMenuSettings> scopeSettingsHelper = new StoreScopeSettingsHelper<MegaMenuSettings>(entity, storeScope, _settingService);
                MegaMenuSettingsModel menuSettingsModel = model;
                Expression<Func<MegaMenuSettings, bool>> expression = (Expression<Func<MegaMenuSettings, bool>>)(x => x.Enabled);
                menuSettingsModel.Enabled_OverrideForStore = await scopeSettingsHelper.SettingExistsAsync<bool>(expression);
            }
            model.Enabled = model.Enabled && _widgetSettings.ActiveWidgetSystemNames.Contains("MWT.Nop.Plugin.MegaMenu");
            model.IsTrialVersion = false;
            return View(nameof(Settings), (object)model); ;
        }

        [HttpPost]
        public async Task<ActionResult> Settings(MegaMenuSettingsModel model)
        {

            _megaMenuSettings = model.ToEntity<MegaMenuSettings>();
            if (model.Enabled && !_widgetSettings.ActiveWidgetSystemNames.Contains("MWT.Nop.Plugin.MegaMenu"))
            {
                _widgetSettings.ActiveWidgetSystemNames.Add("MWT.Nop.Plugin.MegaMenu");
                await _settingService.SaveSettingAsync<WidgetSettings>(_widgetSettings, 0);
            }
            int storeScope = await StoreContext.GetActiveStoreScopeConfigurationAsync();
            await new StoreScopeSettingsHelper<MegaMenuSettings>(model.ToEntity<MegaMenuSettings>(), storeScope, _settingService).SaveStoreSettingAsync<bool>((model.Enabled_OverrideForStore ? 1 : 0) != 0, (Expression<Func<MegaMenuSettings, bool>>)(x => x.Enabled));
            await _settingService.ClearCacheAsync();
            model.ActiveStoreScopeConfiguration = storeScope;
            await _cacheManager.RemoveByPrefixAsync("nop.pres.mwt.megamenu", Array.Empty<object>());
            string resourceAsync = await _localizationService.GetResourceAsync("Admin.Configuration.Updated");
            SuccessNotification(resourceAsync);
            return RedirectToAction(nameof(Settings));
        }

        public ActionResult ManageMenus() => (ActionResult)((Controller)this).View(nameof(ManageMenus), (object)false);

        [HttpPost]
        public async Task<ActionResult> MenuList(MenuAdminSearchModel searchModel)
        {
            PagedList<MenuAdminModel> pagedList = new PagedList<MenuAdminModel>(await PrepareListModelAsync(), searchModel.Page - 1, searchModel.PageSize, new int?());
            MenuAdminListModel grid = ModelExtensions.PrepareToGrid<MenuAdminListModel, MenuAdminModel, MenuAdminModel>(new MenuAdminListModel(), (BaseSearchModel)searchModel, (IPagedList<MenuAdminModel>)pagedList, (Func<IEnumerable<MenuAdminModel>>)(() => (IEnumerable<MenuAdminModel>)pagedList));
            return Json(grid);
        }

        public async Task<ActionResult> MenuCreate()
        {
            MenuModel model = new MenuModel()
            {
                Enabled = true,
                Settings = new MegaMenuSettingsModel()
                {
                    IsTrialVersion = false
                }
            };
            MenuModel menuModel = model;
            menuModel.SupportedWidgetZones = await GetSupportedWidgetZonesAsync();
            return View("MegaMenu", (object)model);
        }

        [HttpPost]
        [ParameterBasedOnFormName("save-continue", "continueEditing")]
        public async Task<ActionResult> MenuCreate(
          MenuModel model,
          bool continueEditing)
        {
            if (ModelState.IsValid)
            {
                Menu menu = model.ToEntity<Menu>();
                await _menuService.CreateMenuAsync(menu);
                await _entityWidgetMappingService.InsertEntityWidgetMappingAsync(new EntityWidgetMapping()
                {
                    EntityId = menu.Id,
                    EntityType = MWT.Nop.Plugin.MegaMenu.Infrastructure.Constants.Plugin.EntityType,
                    DisplayOrder = 0,
                    WidgetZone = model.WidgetZone
                });
                string resourceAsync = await _localizationService.GetResourceAsync("MWT.MegaMenu.Admin.Menu.Created");
                SuccessNotification(resourceAsync);
                if (continueEditing)
                {
                    SaveSelectedTabName("",string.Empty,string.Empty, true);
                    return RedirectToAction("MenuEdit", (object)new
                    {
                        id = menu.Id
                    });
                }
            }
            return RedirectToAction("ManageMenus");
        }

        [RequestFormSizeLimit(2147483647, Order = -2147483648)]
        public async Task<ActionResult> MenuEdit(int id)
        {
            Menu menu = await _menuService.GetMenuByIdAsync(id);
            MenuModel model = menu != null ? menu.ToModel<MenuModel>() : throw new ArgumentException("No menu found with the specified id");
            model.WidgetZone = (await _entityWidgetMappingService.GetEntityWidgetMappingByEntityTypeAndEntityIdAsync(MWT.Nop.Plugin.MegaMenu.Infrastructure.Constants.Plugin.EntityType, menu.Id) ?? throw new ArgumentException("No widget mapping for the specified menu")).WidgetZone;
            MenuModel menuModel = model;
            menuModel.SupportedWidgetZones = await GetSupportedWidgetZonesAsync();
            menuModel = model;
            model.PredefinedPages = (IList<SelectListItem>)MWT.Nop.Plugin.MegaMenu.Infrastructure.Constants.Plugin.PredefinedPageTypes.Select<MenuItemType, SelectListItem>((Func<MenuItemType, SelectListItem>)(page => new SelectListItem()
            {
                Value = ((int)page).ToString(),
                Text = this.LocalizationService.GetLocalizedEnumAsync<MenuItemType>(page, new int?()).Result
            })).ToList<SelectListItem>();
            menuModel = model;
            menuModel.ShowAclDisabledWarning = await ShowAclDisabledWarningAsync();
            await PrepareStoresMappingModelAsync<Menu>(model.MappingToStores, menu, false);
            menuModel = model;
            menuModel.Items = await PrepareMenuItemsAsync(menu);
            return View("MegaMenu", (object)model);
        }

        [HttpPost]
        [ParameterBasedOnFormName("save-continue", "continueEditing")]
        [RequestFormSizeLimit(2147483647, Order = -2147483648)]
        public async Task<ActionResult> MenuEdit(
          MenuModel model,
          bool continueEditing,
          bool updateMenuItems)
        {
            Menu menu = await _menuService.GetMenuByIdAsync(model.Id);
            if (menu == null)
                throw new ArgumentException("No menu found with the specified id");
            if (((ControllerBase)this).ModelState.IsValid)
            {
                menu.Enabled = model.Enabled;
                menu.Name = model.Name;
                menu.CssClass = model.CssClass;
                menu.ShowDropdownsOnClick = model.ShowDropdownsOnClick;
                if (updateMenuItems)
                {
                    IList<MenuItem> itemsForMenuAsync = await _menuItemService.GetAllItemsForMenuAsync(menu.Id);
                    await UpdateMenuItemsAsync(menu.Id, (ICollection<MenuItem>)itemsForMenuAsync, model.Items);
                }
                await SaveStoreMappingsAsync<Menu>(menu, model.MappingToStores);
                await _menuService.UpdateMenuAsync(menu);
                EntityWidgetMapping andEntityIdAsync = await _entityWidgetMappingService.GetEntityWidgetMappingByEntityTypeAndEntityIdAsync(MWT.Nop.Plugin.MegaMenu.Infrastructure.Constants.Plugin.EntityType, menu.Id);
                andEntityIdAsync.WidgetZone = model.WidgetZone;
                await _entityWidgetMappingService.UpdateEntityWidgetMappingAsync(andEntityIdAsync);
                string resourceAsync = await _localizationService.GetResourceAsync("MWT.MegaMenu.Admin.Menu.Updated");
                SuccessNotification(resourceAsync);
            }
            if (!continueEditing)
                return RedirectToAction("ManageMenus");
            SaveSelectedTabName(string.Empty, string.Empty, string.Empty, true);
            return RedirectToAction(nameof(MenuEdit), (object)new
            {
                id = menu.Id
            });
        }

        public async Task<IList<MenuItemModel>> PrepareMenuItemsAsync(Menu menu)
        {
            await DeleteOrphanMenuItems();
            List<MenuItemModel> lstMenuItmModel = new List<MenuItemModel>();
            var items = (await this._menuItemService.GetAllItemsForMenuAsync(menu.Id)).Where(x => x.ParentMenuItemId == 0).OrderBy(x => x.DisplayOrder);
            foreach (var item in items)
            {
                MenuItemModel model = item.ToModel<MenuItemModel>();
                model.SubItems = await this.PrepareSubMenuItemsAsync(model);
                model.TypeName = await this.GetTypeNameAsync(model.Type);
                await this.PrepareAclModelAsync(item, model);
                await AddLocalesAsync(this._languageService, model.Locales);

                foreach (var locale in model.Locales)
                {
                    locale.Url = await this.LocalizationService.GetLocalizedAsync<MenuItem, string>(item, (Expression<Func<MenuItem, string>>)(mi => mi.Url), new int?(locale.LanguageId), false, false);
                    locale.Title = await this.LocalizationService.GetLocalizedAsync<MenuItem, string>(item, (Expression<Func<MenuItem, string>>)(mi => mi.Title), new int?(locale.LanguageId), false, false);
                }
                lstMenuItmModel.Add(model);
            }
            return lstMenuItmModel;
        }

        public async Task<ActionResult> MenuDelete(int id)
        {
            Menu menu = await _menuService.GetMenuByIdAsync(id);
            if (menu == null)
                throw new ArgumentException("No menu found with the specified id.");
            foreach (MenuItem menuItem in (IEnumerable<MenuItem>)await _menuItemService.GetAllItemsForMenuAsync(id))
                await SaveMenuItemAclAsync(menuItem, null, true);
            await _menuService.DeleteMenuAsync(menu);
            return EmptyJson;
        }

        [HttpPost]
        [ActionName("Delete")]
        public async Task<ActionResult> MenuDeleteConfirmed(int id)
        {
            Menu menu = await _menuService.GetMenuByIdAsync(id);
            if (menu == null)
                throw new ArgumentException("No menu found with the specified id.");
            foreach (MenuItem menuItem in (IEnumerable<MenuItem>)await _menuItemService.GetAllItemsForMenuAsync(id))
                await SaveMenuItemAclAsync(menuItem, (MenuItemModel)null, true);
            await _menuService.DeleteMenuAsync(menu);
            string resourceAsync = await _localizationService.GetResourceAsync("MWT.MegaMenu.Admin.Menu.Deleted");
            SuccessNotification(resourceAsync);
            return RedirectToAction("ManageMenus");
        }

        private async Task DeleteOrphanMenuItems()
        {
            foreach (MenuItem menuItem in (IEnumerable<MenuItem>)(await this._menuItemService.GetAllItemsAsync()).Where<MenuItem>((Func<MenuItem, bool>)(x => !x.MenuId.HasValue)).ToList<MenuItem>())
                await this._menuItemService.DeleteAsync(menuItem);
        }

        private async Task PrepareAclModelAsync(MenuItem menuItem, MenuItemModel model)
        {
            if (menuItem != null)
            {
                MenuItemModel menuItemModel = model;

                menuItemModel.SelectedCustomerRoleIds =  (await this._aclService.GetCustomerRoleIdsWithAccessAsync(menuItem.Id, nameof(MenuItem))).ToList();
            }
            foreach (CustomerRole customerRole in (IEnumerable<CustomerRole>)await this._customerService.GetAllCustomerRolesAsync(true))
                model.AvailableCustomerRoles.Add(new SelectListItem()
                {
                    Text = customerRole.Name,
                    Value = ((BaseEntity)customerRole).Id.ToString(),
                    Selected = model.SelectedCustomerRoleIds.Contains(((BaseEntity)customerRole).Id)
                });
        }

        private async Task<IList<MenuItemModel>> PrepareSubMenuItemsAsync(
          MenuItemModel model)
        {
            IList<MenuItem> list = (IList<MenuItem>)(await this._menuItemService.GetAllChildrenForMenuItemAsync(model.Id)).Where(m=>m.Id!=model.Id)
                .OrderBy<MenuItem, int>((Func<MenuItem, int>)(x => x.DisplayOrder)).ToList<MenuItem>();
            IList<MenuItemModel> menuItemModelList = (IList<MenuItemModel>)new List<MenuItemModel>();
            if (list.Count > 0)
                menuItemModelList = (IList<MenuItemModel>)await AsyncIEnumerableExtensions.SelectAwait<MenuItem, MenuItemModel>((IEnumerable<MenuItem>)list, (Func<MenuItem, ValueTask<MenuItemModel>>)(async x =>
                {
                    MenuItemModel menuItemModel1 = x.ToModel<MenuItemModel>();
                    MenuItemModel menuItemModel2 = menuItemModel1;
                    menuItemModel2.SubItems = await this.PrepareSubMenuItemsAsync(menuItemModel1);
                    menuItemModel2 = menuItemModel1;
                    menuItemModel2.TypeName = await this.GetTypeNameAsync(menuItemModel1.Type);
                    if (model.Type == MenuItemType.Categories && model.EntityId > 0)
                    {
                        Category categoryByIdAsync = await this._categoryService.GetCategoryByIdAsync(model.EntityId);
                        if (categoryByIdAsync != null)
                            model.IncludeInTopMenu = true;
                    }
                    await this.PrepareAclModelAsync(x, menuItemModel1);
                    await AddLocalesAsync<MenuItemLocalizedModel>(this._languageService, menuItemModel1.Locales, (Action<MenuItemLocalizedModel, int>)(async (locale, languageId) =>
                    {
                        MenuItemLocalizedModel itemLocalizedModel = locale;
                        ILocalizationService localizationService3 = this.LocalizationService;
                        MenuItem menuItem3 = x;
                        Expression<Func<MenuItem, string>> expression3 = (Expression<Func<MenuItem, string>>)(mi => mi.Url);
                        int? nullable3 = new int?(languageId);
                        itemLocalizedModel.Url = await localizationService3.GetLocalizedAsync<MenuItem, string>(menuItem3, expression3, nullable3, false, false);
                        itemLocalizedModel = locale;
                        ILocalizationService localizationService4 = this.LocalizationService;
                        MenuItem menuItem4 = x;
                        Expression<Func<MenuItem, string>> expression4 = (Expression<Func<MenuItem, string>>)(mi => mi.Title);
                        int? nullable4 = new int?(languageId);
                        itemLocalizedModel.Title = await localizationService4.GetLocalizedAsync<MenuItem, string>(menuItem4, expression4, nullable4, false, false);
                    }));
                    MenuItemModel menuItemModel = menuItemModel1;
                    return menuItemModel;
                })).ToListAsync<MenuItemModel>();
            return menuItemModelList;
        }

        private async Task<string> GetTypeNameAsync(MenuItemType itemType)
        {
            return MWT.Nop.Plugin.MegaMenu.Infrastructure.Constants.Plugin.PredefinedPageTypes.Contains(itemType) ? await 
                _localizationService.GetResourceAsync("MWT.MegaMenu.Admin.MenuItemType.Pages") :
                await LocalizationService.GetLocalizedEnumAsync<MenuItemType>(itemType, new int?());
        }

        private async Task UpdateMenuItemsAsync(
          int menuId,
          ICollection<MenuItem> currentMenuItems,
          IList<MenuItemModel> newMenuItemModels)
        {
            IList<MenuItem> itemsToDelete;
            MenuItem menuItem;
            if (newMenuItemModels.Count == 0)
            {
                itemsToDelete = (IList<MenuItem>)(await this._menuItemService.GetAllItemsForMenuAsync(menuId)).ToList<MenuItem>();
            }
            else
            {
                itemsToDelete = (IList<MenuItem>)currentMenuItems.Where<MenuItem>((Func<MenuItem, bool>)(cmi => !newMenuItemModels.Select<MenuItemModel, int>((Func<MenuItemModel, int>)(nmi => nmi.Id)).Contains<int>(cmi.Id))).ToList<MenuItem>();
                foreach (MenuItemModel newMenuItemModel in (IEnumerable<MenuItemModel>)newMenuItemModels)
                {
                    MenuItemModel menuItemModel = newMenuItemModel;
                    menuItem = await this._menuItemService.GetMenuItemByIdAsync(menuItemModel.Id);
                    if (menuItem != null)
                    {
                        menuItem = menuItemModel.ToEntity(menuItem);
                        menuItem.MenuId = new int?(menuId);
                        await this.SaveMenuItemAclAsync(menuItem, menuItemModel);
                        await this._menuItemService.UpdateAsync(menuItem);
                        await this.UpdateLocalesAsync(menuItem, menuItemModel);
                    }
                }
            }
            foreach (MenuItem menuItem1 in (IEnumerable<MenuItem>)itemsToDelete)
            {
                menuItem = menuItem1;
                await this.SaveMenuItemAclAsync(menuItem, null, true);
                await this._menuItemService.DeleteAsync(menuItem);
            }

        }

        private async Task<bool> ShowAclDisabledWarningAsync()
        {
            bool enabled = _catalogSettings.IgnoreAcl;
            if (!enabled)
            {
                foreach (Store store in (IEnumerable<Store>)await StoreService.GetAllStoresAsync())
                {
                    if (!enabled)
                        enabled = (await _settingService.LoadSettingAsync<CatalogSettings>(((BaseEntity)store).Id)).IgnoreAcl;
                }
            }
            return enabled;
        }

        private async Task<IList<MenuAdminModel>> PrepareListModelAsync()
        {
            IList<Menu> allMenusAsync = await this._menuService.GetAllMenusAsync();
            List<MenuAdminModel> menuAdminModelList = new List<MenuAdminModel>();
            foreach (Menu menu in (IEnumerable<Menu>)allMenusAsync)
            {
                MenuAdminModel menuAdminModel1 = new MenuAdminModel();
                menuAdminModel1.Id = menu.Id;
                menuAdminModel1.Name = menu.Name;
                menuAdminModel1.Enabled = menu.Enabled;
                menuAdminModel1.CssClass = menu.CssClass;
                MenuAdminModel menuAdminModel2 = menuAdminModel1;
                menuAdminModelList.Add(menuAdminModel2);
            }
            return menuAdminModelList;
        }

        private async Task<IList<SelectListItem>> GetSupportedWidgetZonesAsync() => (IList<SelectListItem>)(await this._installHelper.GetSupportedWidgetZonesAsync("MWT.Nop.Plugin.MegaMenu")).ToList<string>().Select<string, SelectListItem>((Func<string, SelectListItem>)(x => new SelectListItem()
        {
            Text = x.ToString(),
            Value = x.ToString()
        })).ToList<SelectListItem>();

        private async Task UpdateLocalesAsync(MenuItem menuItem, MenuItemModel model)
        {
            foreach (MenuItemLocalizedModel localized in (IEnumerable<MenuItemLocalizedModel>)model.Locales)
            {
                await this._localizedEntityService.SaveLocalizedValueAsync<MenuItem>(menuItem, (Expression<Func<MenuItem, string>>)(x => x.Url), localized.Url, localized.LanguageId);
                await this._localizedEntityService.SaveLocalizedValueAsync<MenuItem>(menuItem, (Expression<Func<MenuItem, string>>)(x => x.Title), localized.Title, localized.LanguageId);
            }
        }

        private async Task SaveMenuItemAclAsync(
          MenuItem menuItem,
          MenuItemModel model,
          bool deleteAclRecords = false)
        {
            if (model != null)
                menuItem.SubjectToAcl = model.SelectedCustomerRoleIds.Any<int>();
            IList<AclRecord> existingAclRecords = await this._aclService.GetAclRecordsAsync<MenuItem>(menuItem);
            foreach (CustomerRole customerRole1 in (IEnumerable<CustomerRole>)await this._customerService.GetAllCustomerRolesAsync(true))
            {
                CustomerRole customerRole = customerRole1;
                if (!deleteAclRecords && model.SelectedCustomerRoleIds.Contains(((BaseEntity)customerRole).Id))
                {
                    if (((IEnumerable<AclRecord>)existingAclRecords).Count<AclRecord>((Func<AclRecord, bool>)(acl => acl.CustomerRoleId == ((BaseEntity)customerRole).Id)) == 0)
                        await this._aclService.InsertAclRecordAsync<MenuItem>(menuItem, ((BaseEntity)customerRole).Id);
                }
                else
                {
                    AclRecord aclRecord = ((IEnumerable<AclRecord>)existingAclRecords).FirstOrDefault<AclRecord>((Func<AclRecord, bool>)(acl => acl.CustomerRoleId == ((BaseEntity)customerRole).Id));
                    if (aclRecord != null)
                        await this._aclService.DeleteAclRecordAsync(aclRecord);
                }
            }
        }
    }
}

