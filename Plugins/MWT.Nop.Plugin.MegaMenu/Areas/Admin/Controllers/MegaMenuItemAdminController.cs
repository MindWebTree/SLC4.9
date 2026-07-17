using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using MWT.Nop.Plugin.MegaMenu.Areas.Admin.Attributes;
using MWT.Nop.Plugin.MegaMenu.Areas.Admin.Components;
using MWT.Nop.Plugin.MegaMenu.Areas.Admin.Controllers;
using MWT.Nop.Plugin.MegaMenu.Areas.Admin.Extensions;
using MWT.Nop.Plugin.MegaMenu.Areas.Admin.Models;
using MWT.Nop.Plugin.MegaMenu.Domain;
using MWT.Nop.Plugin.MegaMenu.Domain.Enums;
using MWT.Nop.Plugin.MegaMenu.Infrastructure.Constants;
using MWT.Nop.Plugin.MegaMenu.KendoUI;
using MWT.Nop.Plugin.MegaMenu.Models;
using MWT.Nop.Plugin.MegaMenu.MVCExtensions;
using MWT.Nop.Plugin.MegaMenu.Services;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Topics;
using Nop.Core.Domain.Vendors;
using Nop.Services.Catalog;
using Nop.Services.Customers;
using Nop.Services.Localization;
using Nop.Services.Security;
using Nop.Services.Topics;
using Nop.Services.Vendors;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Framework.Controllers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;


#nullable enable

namespace MWT.Nop.Plugin.MegaMenu.Areas.Admin.Controllers
{
    //[ManagePluginsAdminAuthorize("MWT.Nop.Plugin.MegaMenu", false)]
    public class MegaMenuItemAdminController : BaseMWTAdminController
    {
        private readonly MegaMenuSettings _megaMenuSettings;
        private readonly IMenuService _menuService;
        private readonly IMenuItemService _menuItemService;
        private readonly ILanguageService _languageService;
        private readonly ICategoryService _categoryService;
        private readonly ITopicService _topicService;
        private readonly IManufacturerService _manufacturerService;
        private readonly IVendorService _vendorService;
        private readonly IProductTagService _productTagService;
        private readonly ILocalizationService _localizationService;
        private readonly ICustomerService _customerService;
        private readonly IAclService _aclService;
        private readonly IWorkContext _workContext;

        public MegaMenuItemAdminController(
          IMenuService menuService,
          IMenuItemService menuItemService,
          ILanguageService languageService,
          ICategoryService categoryService,
          MegaMenuSettings megaMenuSettings,
          ITopicService topicService,
          IManufacturerService manufacturerService,
          IVendorService vendorService,
          IProductTagService productTagService,
          ILocalizationService localizationService,
          ICustomerService customerService,
          IAclService aclService,
          IWorkContext workContext)
        {
            this._menuService = menuService;
            this._menuItemService = menuItemService;
            this._languageService = languageService;
            this._categoryService = categoryService;
            this._megaMenuSettings = megaMenuSettings;
            this._topicService = topicService;
            this._manufacturerService = manufacturerService;
            this._vendorService = vendorService;
            this._productTagService = productTagService;
            this._localizationService = localizationService;
            this._customerService = customerService;
            this._aclService = aclService;
            this._workContext = workContext;
        }

        [HttpPost]
        public async Task<ActionResult> AddMenuItem(MenuItemModel model)
        {
            //if (!ModelState.IsValid)   -- need to fix
            //    throw new InvalidOperationException("All of the fields are required.");
            MenuItem menuItem = model.ToEntity<MenuItem>();
            if (menuItem.Type == MenuItemType.Categories)
            {
                menuItem.ImageSize = _megaMenuSettings.CategoryImageSize;
                menuItem.NumberOfBoxesPerRow = _megaMenuSettings.NumberOfCategoriesPerRow;
                menuItem.MaximumNumberOfEntities = _megaMenuSettings.NumberOfCategories;
            }
            else if (menuItem.Type == MenuItemType.Manufacturers)
            {
                menuItem.ImageSize = _megaMenuSettings.ManufacturerImageSize;
                menuItem.NumberOfBoxesPerRow = _megaMenuSettings.NumberOfManufacturersPerRow;
                menuItem.MaximumNumberOfEntities = _megaMenuSettings.NumberOfManufacturers;
            }
            else if (menuItem.Type == MenuItemType.Vendors)
            {
                menuItem.ImageSize = _megaMenuSettings.VendorImageSize;
                menuItem.NumberOfBoxesPerRow = _megaMenuSettings.NumberOfVendorsPerRow;
                menuItem.MaximumNumberOfEntities = _megaMenuSettings.NumberOfVendors;
            }
            else if (menuItem.Type == MenuItemType.Topics)
                menuItem.MaximumNumberOfEntities = _megaMenuSettings.NumberOfTopics;
            else if (menuItem.Type == MenuItemType.ProductTags)
                menuItem.MaximumNumberOfEntities = _megaMenuSettings.NumberOfProductTags;
            await _menuItemService.CreateAsync(menuItem);
            MenuItemModel menuItemModel1 = menuItem.ToModel<MenuItemModel>();
            if (menuItemModel1.Type == MenuItemType.Column)
                menuItemModel1.DepthLevel = 1;
            if (menuItemModel1.Type == MenuItemType.Row)
                menuItemModel1.DepthLevel = 1;
            MenuItemModel menuItemModel2 = menuItemModel1;
            menuItemModel2.TypeName =await GetTypeNameAsync(menuItemModel1.Type);
            await AddLocalesAsync<MenuItemLocalizedModel>(_languageService, menuItemModel1.Locales,
                (Action<MenuItemLocalizedModel, int>)(async (locale, languageId) =>
            {
                MenuItemLocalizedModel itemLocalizedModel = locale;
                ILocalizationService localizationService1 = this.LocalizationService;
                MenuItem menuItem1 = menuItem;
                Expression<Func<MenuItem, string>> expression1 = (Expression<Func<MenuItem, string>>)(mi => mi.Url);
                int? nullable1 = new int?(languageId);
                itemLocalizedModel.Url = await localizationService1.GetLocalizedAsync<MenuItem, string>(menuItem1, expression1, nullable1, false, false);
                itemLocalizedModel = locale;
                ILocalizationService localizationService2 = this.LocalizationService;
                MenuItem menuItem2 = menuItem;
                Expression<Func<MenuItem, string>> expression2 = (Expression<Func<MenuItem, string>>)(mi => mi.Title);
                int? nullable2 = new int?(languageId);
                itemLocalizedModel.Title = await localizationService2.GetLocalizedAsync<MenuItem, string>(menuItem2, expression2, nullable2, false, false);
            }));
            await PrepareAclModelAsync(null, menuItemModel1);
            string stringAsync = await RenderViewComponentToStringAsync(typeof(MegaMenuItemTemplateAdminViewComponent), (object)new
            {
                model = menuItemModel1
            });
            return Content(stringAsync);
        }

        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<ActionResult> GetCategories([FromBody] GridData gridData)
        {
            MegaMenuItemAdminController itemAdminController = this;
            IList<Category> allCategories = (IList<Category>)((IEnumerable<Category>)await itemAdminController._categoryService.GetAllCategoriesAsync(0, false)).ToList<Category>();
            int totalCount;
            List<Category> list = ((IEnumerable<Category>)QueryableExtension.GridPaging<Category>(QueryableExtension.GridFilter<Category, GridFilters>(itemAdminController._categoryService.GetAllCategoriesAsQueryable(), gridData.Filter), gridData.Page, gridData.PageSize, out totalCount)).ToList<Category>();
            DataSourceResult dataSourceResult1 = new DataSourceResult();
            DataSourceResult dataSourceResult2 = dataSourceResult1;
            Func<Category, ValueTask<EntityGridModel>> func = (Func<Category, ValueTask<EntityGridModel>>)(async x =>
            {
                EntityGridModel categories = new EntityGridModel();
                categories.Id = ((BaseEntity)x).Id;
                EntityGridModel entityGridModel = categories;
                entityGridModel.Name = await this._categoryService.GetFormattedBreadCrumbAsync(x, allCategories, ">>", 0);
                return categories;
            });
            dataSourceResult2.Data = (IEnumerable)await AsyncIEnumerableExtensions.SelectAwait<Category, EntityGridModel>((IEnumerable<Category>)list, func).ToListAsync<EntityGridModel>();
            dataSourceResult1.Total = totalCount;
            DataSourceResult data = dataSourceResult1;
             return (ActionResult)((Controller)itemAdminController).Json((object)data);
        }

        [HttpPost]
        [IgnoreAntiforgeryToken]
        public ActionResult GetTopics([FromBody] GridData gridData)
        {
            int num;
            IList<Topic> list = (IList<Topic>)((IEnumerable<Topic>)QueryableExtension.GridPaging<Topic>(QueryableExtension.GridFilter<Topic, GridFilters>(this._topicService.GetAllTopicsAsQueryable(), gridData.Filter), gridData.Page, gridData.PageSize, out num)).ToList<Topic>();
            return (ActionResult)((Controller)this).Json((object)new DataSourceResult()
            {
                Data = (IEnumerable)((IEnumerable<Topic>)list).Select<Topic, EntityGridModel>((Func<Topic, EntityGridModel>)(x =>
                {
                    return new EntityGridModel()
                    {
                        Id = ((BaseEntity)x).Id,
                        Name = string.Format("{0} ({1})", (object)x.Title, (object)x.SystemName)
                    };
                })),
                Total = num
            });
        }

        [HttpPost]
        [IgnoreAntiforgeryToken]
        public ActionResult GetManufacturers([FromBody] GridData gridData)
        {
            int num;
            IList<Manufacturer> list = (IList<Manufacturer>)((IEnumerable<Manufacturer>)QueryableExtension.GridPaging<Manufacturer>(QueryableExtension.GridFilter<Manufacturer, GridFilters>(this._manufacturerService.GetAllManufacturersAsQueryable(), gridData.Filter), gridData.Page, gridData.PageSize, out num)).ToList<Manufacturer>();
            return (ActionResult)((Controller)this).Json((object)new DataSourceResult()
            {
                Data = (IEnumerable)((IEnumerable<Manufacturer>)list).Select<Manufacturer, EntityGridModel>((Func<Manufacturer, EntityGridModel>)(x =>
                {
                    return new EntityGridModel()
                    {
                        Id = ((BaseEntity)x).Id,
                        Name = x.Name
                    };
                })),
                Total = num
            });
        }

        [HttpPost]
        [IgnoreAntiforgeryToken]
        public ActionResult GetVendors([FromBody] GridData gridData)
        {
            int num;
            IList<Vendor> list = (IList<Vendor>)((IEnumerable<Vendor>)QueryableExtension.GridPaging<Vendor>
                (QueryableExtension.GridFilter<Vendor, GridFilters>
                (this._vendorService.GetAllVendorsAsQueryable(), gridData.Filter), gridData.Page, gridData.PageSize, out num).
                Where(v=>v.Active)).ToList<Vendor>();
            return (ActionResult)((Controller)this).Json((object)new DataSourceResult()
            {
                Data = (IEnumerable)((IEnumerable<Vendor>)list).Select<Vendor, EntityGridModel>((Func<Vendor, EntityGridModel>)(x =>
                {
                    return new EntityGridModel()
                    {
                        Id = ((BaseEntity)x).Id,
                        Name = x.Name
                    };
                })),
                Total = num
            });
        }

        [HttpPost]
        [IgnoreAntiforgeryToken]
        public ActionResult GetProductTags([FromBody] GridData gridData)
        {
            int num;
            IList<ProductTag> list = (IList<ProductTag>)((IEnumerable<ProductTag>)QueryableExtension.GridPaging<ProductTag>(QueryableExtension.GridFilter<ProductTag, GridFilters>(this._productTagService.GetAllProductTagsAsQueryable(), gridData.Filter), gridData.Page, gridData.PageSize, out num)).ToList<ProductTag>();
            return (ActionResult)((Controller)this).Json((object)new DataSourceResult()
            {
                Data = (IEnumerable)((IEnumerable<ProductTag>)list).Select<ProductTag, EntityGridModel>((Func<ProductTag, EntityGridModel>)(x =>
                {
                    return new EntityGridModel()
                    {
                        Id = ((BaseEntity)x).Id,
                        Name = x.Name
                    };
                })),
                Total = num
            });
        }

        private async Task<IList<MenuItemModel>> PrepareSubMenuItemsAsync(
          MenuItemModel model)
        {
            IList<MenuItem> list = (IList<MenuItem>)(await this._menuItemService.GetAllChildrenForMenuItemAsync(model.Id)).OrderBy<MenuItem, int>((Func<MenuItem, int>)(x => x.DisplayOrder)).ToList<MenuItem>();
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

        private async Task PrepareAclModelAsync(MenuItem menuItem, MenuItemModel model)
        {
            if (menuItem != null)
            {
                MenuItemModel menuItemModel = model;
                menuItemModel.SelectedCustomerRoleIds = (await this._aclService.GetCustomerRoleIdsWithAccessAsync(menuItem.Id, nameof(MenuItem))).ToList();
            
            }
            foreach (CustomerRole customerRole in (IEnumerable<CustomerRole>)await this._customerService.GetAllCustomerRolesAsync(true))
                model.AvailableCustomerRoles.Add(new SelectListItem()
                {
                    Text = customerRole.Name,
                    Value = ((BaseEntity)customerRole).Id.ToString(),
                    Selected = model.SelectedCustomerRoleIds.Contains(((BaseEntity)customerRole).Id)
                });
        }

        private async Task<string> GetTypeNameAsync(MenuItemType itemType)
        {
            MegaMenuItemAdminController itemAdminController = this;
            return MWT.Nop.Plugin.MegaMenu.Infrastructure.Constants.Plugin.PredefinedPageTypes.Contains(itemType) ? await itemAdminController._localizationService.GetResourceAsync("MWT.MegaMenu.Admin.MenuItemType.Pages") : await itemAdminController.LocalizationService.GetLocalizedEnumAsync<MenuItemType>(itemType, new int?());
        }
    }
}
