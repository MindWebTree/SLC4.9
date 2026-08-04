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
using Nop.Web.Framework.Models;
using Nop.Web.Models.Catalog;
using Nop.Web.Models.Media;
using MWT.Nop.Plugin.MegaMenu.Domain.Enums;
using MWT.Nop.Plugin.MegaMenu.Models;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using MWT.Nop.Plugin.MegaMenu.Areas.Admin.Models;
using MWT.Nop.Core.Services.Customers;


#nullable enable
namespace MWT.Nop.Plugin.MegaMenu.Components
{
    public class MegaMenuVendorViewComponent : BaseComponent
    { 
        private readonly IVendorService _vendorsService; 
        private readonly IWorkContext _workContext;
        private readonly IStoreContext _storeContext; 
        private readonly IWebHelper _webHelper; 

        private CacheKey MegaMenuModelKey => new CacheKey("nop.pres.mwt.megamenu-common-{0}-{1}-{2}-{3}-{4}" );

        public MegaMenuVendorViewComponent( 
          IVendorService vendorService, 
          IWorkContext workContext,
          IStoreContext storeContext, 
          IWebHelper webHelper)
        { 
            this._vendorsService = vendorService;
            this._workContext = workContext;
            this._storeContext = storeContext; 
            this._webHelper = webHelper;
        }

        public async Task<IViewComponentResult> InvokeAsync(
          MenuItemModel menuItem)
        {
            MegaMenuVendorViewComponent vendorViewComponent = this;
            ICustomerService icustomerService = vendorViewComponent.CustomerService;
            int[] customerRoleIdsAsync = await icustomerService.GetCustomerRoleIdsAsync(await vendorViewComponent._workContext.GetCurrentCustomerAsync(), false);
            int[] numArray = customerRoleIdsAsync;
            IStaticCacheManager istaticCacheManager = vendorViewComponent.StaticCacheManager;
            CacheKey cacheKey = vendorViewComponent.MegaMenuModelKey;
            object obj1 = (object)numArray;
            object obj2 = (object)((BaseEntity)await vendorViewComponent._workContext.GetWorkingLanguageAsync()).Id;
            Store currentStoreAsync = await vendorViewComponent._storeContext.GetCurrentStoreAsync();
            CacheKey cacheKey1 = istaticCacheManager.PrepareKeyForDefaultCache(cacheKey, new object[5]
            {
        obj1,
        obj2,
        (object) ((BaseEntity) currentStoreAsync).Id,
        (object) vendorViewComponent._webHelper.IsCurrentConnectionSecured(),
        (object) menuItem.Id
            });

            MenuItemVendorModel async = await vendorViewComponent.StaticCacheManager.GetAsync<MenuItemVendorModel>(cacheKey1, (Func<Task<MenuItemVendorModel>>)(async () =>
            {
                (IList<VendorModel> vendorModelList2, bool flag2) = await this.PrepareVendorModelsAsync(menuItem);
                return new MenuItemVendorModel()
                {
                    Item = menuItem,
                    Vendors = vendorModelList2,
                    ShouldShowViewAllLink = flag2
                };
            }));
            if (((ICollection<VendorModel>)async.Vendors).Count == 0)
                return (IViewComponentResult)((ViewComponent)vendorViewComponent).Content("");
            string str = "VendorMenuTemplate." + menuItem.CatalogTemplate.ToString();
            return (IViewComponentResult)vendorViewComponent.View<MenuItemVendorModel>(str, async);
        }

        private async Task<(IList<VendorModel> megaMenuVEndorsModels, bool shouldShowViewAllLink)> PrepareVendorModelsAsync(
          MenuItemModel menuItem)
        {
            MegaMenuVendorViewComponent vendorViewComponent = this;
            List<VendorModel> megaMenuVendorModels = new List<VendorModel>();
            IPagedList<Vendor> vendors = await vendorViewComponent._vendorsService.GetAllVendorsAsync("", "", 0, menuItem.MaximumNumberOfEntities, false);
            foreach (Vendor vendor1 in (IEnumerable<Vendor>)vendors)
            {
                VendorModel vendorModel1 = new VendorModel();
                ((BaseNopEntityModel)vendorModel1).Id = ((BaseEntity)vendor1).Id;
                VendorModel vendorModel2 = vendorModel1;
                ILocalizationService localizationService1 = vendorViewComponent.LocalizationService;
                Vendor vendor2 = vendor1;
                int? nullable1 = new int?();
                vendorModel2.Name = await localizationService1.GetLocalizedAsync<Vendor, string>(vendor2, v => v.Name, nullable1, true, true);
                VendorModel vendorModel3 = vendorModel1;
                ILocalizationService localizationService2 = vendorViewComponent.LocalizationService;
                Vendor vendor3 = vendor1;
                int? nullable2 = new int?();
                vendorModel3.Description = await localizationService2.GetLocalizedAsync<Vendor, string>(vendor3, v => v.Description, nullable2, true, true);
                VendorModel vendorModel4 = vendorModel1;
                ILocalizationService localizationService3 = vendorViewComponent.LocalizationService;
                Vendor vendor4 = vendor1;
                int? nullable3 = new int?();
                vendorModel4.MetaKeywords = await localizationService3.GetLocalizedAsync<Vendor, string>(vendor4, v => v.MetaKeywords, nullable3, true, true);
                VendorModel vendorModel5 = vendorModel1;
                ILocalizationService localizationService4 = vendorViewComponent.LocalizationService;
                Vendor vendor5 = vendor1;

                int? nullable4 = new int?();
                vendorModel5.MetaDescription = await localizationService4.GetLocalizedAsync<Vendor, string>(vendor5, v => v.MetaDescription, nullable4, true, true);
                VendorModel vendorModel6 = vendorModel1;
                ILocalizationService localizationService5 = vendorViewComponent.LocalizationService;
                Vendor vendor6 = vendor1;

                int? nullable5 = new int?();
                vendorModel6.MetaTitle = await localizationService5.GetLocalizedAsync<Vendor, string>(vendor6, v => v.MetaTitle, nullable5, true, true);
                VendorModel vendorModel7 = vendorModel1;
                vendorModel7.SeName = await vendorViewComponent.UrlRecordService.GetSeNameAsync<Vendor>(vendor1, new int?(), true, true);
                VendorModel vendorModel8 = vendorModel1;

                //if (menuItem.CatalogTemplate == CatalogTemplate.WithPictures)
                //{
                //    vendorModel1 = vendorModel8;
                //    vendorModel1.PictureModel = await vendorViewComponent.PreparePictureModelAsync(vendor1.PictureId, menuItem.ImageSize, vendorModel8.Name);
                //    vendorModel1 = (VendorModel)null;
                //}
                megaMenuVendorModels.Add(vendorModel8);

            }
            (IList<VendorModel>, bool) valueTuple = ((IList<VendorModel>)megaMenuVendorModels, vendors.TotalCount > menuItem.MaximumNumberOfEntities);
            return valueTuple;
        } 
    }
}
