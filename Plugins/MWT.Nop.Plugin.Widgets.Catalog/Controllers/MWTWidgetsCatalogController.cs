using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using MWT.Nop.Plugin.Widgets.Catalog.Models;
using MWT.Nop.Plugin.Widgets.Catalog.Services;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Services.Catalog;
using Nop.Services.Configuration;
// using Nop.Services.Customizations.Custom.KW;
using Nop.Services.Security;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Infrastructure;
using Nop.Web.Framework.Models.Extensions;
using Nop.Web.Framework.Mvc;
using Nop.Web.Framework.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MWT.Nop.Plugin.Widgets.Catalog.Controllers
{
    [AuthorizeAdmin]
    [Area(AreaNames.ADMIN)]
    [AutoValidateAntiforgeryToken]
    public class MWTWidgetsCatalogController : BasePluginController
    {
        #region Fields 
        private readonly IPermissionService _permissionService; 
        private readonly ICategoryService _categoryService;
        private readonly IMWTEntityBannerService _imwtEntityBannerService;
        private readonly ISettingService _settingService; 
        // private readonly IKwTermService _kwTermService;

        #endregion

        public MWTWidgetsCatalogController(IPermissionService permissionService,
            ICategoryService categoryService,
             IMWTEntityBannerService imwtEntityBannerService,
             ISettingService settingService
            //IKwTermService kwTermService
            )
        {
            _permissionService = permissionService; 
            _categoryService = categoryService;
            _imwtEntityBannerService = imwtEntityBannerService;
            _settingService = settingService; 
            //kwTermService = kwTermService;
        }

        #region Methods

        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task<IActionResult> Configure(bool showtour = false)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermission.Configuration.MANAGE_WIDGETS))
                return AccessDeniedView();
            var model = new ConfigurationModel();
            var availableCategoryItems = await GetCategoryListAsync();
            availableCategoryItems.Insert(0, new SelectListItem { Text = "*", Value = "" });
            model.AvailableCategories = availableCategoryItems;
            return View("~/Plugins/MWT.Nop.Plugin.Widgets.Catalog/Views/Configure.cshtml", model);
        }

        [HttpPost]
        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task<IActionResult> AllBanners(ConfigurationModel searchModel, ConfigurationModel filter)
        {
            var records = await _imwtEntityBannerService.GetAllAsync(
            pageIndex: searchModel.Page - 1,
            pageSize: searchModel.PageSize,
            entityId: filter.SearchCategoryId
           );

            var gridModel = await new MWTEntityBannerListModel().PrepareToGridAsync(searchModel, records, () =>
            {
                return records.SelectAwait(async record =>
                {
                    var model = new MWTEntityBannerModel
                    {
                        EntityName = await _imwtEntityBannerService.GetEntityNamesMappedWithBanner(record.Id, record.EntityType),
                        WidgetZone = record.WidgetZone,
                        ActionLink = record.ActionLink,
                        BannerId = record.BannerId,
                        EntityType = record.EntityType,
                        Html = record.Html,
                        Id = record.Id,
                        MobileActionLink = record.MobileActionLink,
                        MobileBannerId = record.MobileBannerId,
                        MobileHtml = record.MobileHtml,
                        VideoUrl = record.VideoUrl,
                        MobileVideoUrl = record.MobileVideoUrl
                    };

                    return model;
                });

            });

            return Json(gridModel);
        }

        public async Task<IActionResult> AddBannerPopup()
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermission.Configuration.MANAGE_WIDGETS))
                return AccessDeniedView();
            var model = new MWTEntityBannerModel();
            var availableCategoryItems = await GetCategoryListAsync();
            availableCategoryItems.Insert(0, new SelectListItem { Text = "Please select Entity", Value = "" });
            model.AvailableCategories = availableCategoryItems;

            //var availableKwTerms = await GetKwTermListAsync();
            //availableKwTerms.Insert(0, new SelectListItem { Text = "Please select Entity", Value = "" });
            //model.AvailableKwTerms = availableKwTerms;


            model.AvailableWidgetZones = GetPostions();
            model.BannerType = "DesktopBanner";
            model.MobileBannerType = "MobileBanner";
            return View("~/Plugins/MWT.Nop.Plugin.Widgets.Catalog/Views/AddBanner.cshtml", model);
        }

        [HttpPost]
        public async Task<IActionResult> AddBannerPopup(MWTEntityBannerModel model)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermission.Configuration.MANAGE_WIDGETS))
                return AccessDeniedView();

            if ((ModelState.IsValid || (!ModelState.IsValid && ModelState["EntityIds"].Errors.Count > 0)) && model.EntityIds.Count > 0)
            {
                (bool isMapped, string entityName) = await _imwtEntityBannerService.IsWidgetZoneMappedWithEntityId(model.EntityIds, model.Id, model.WidgetZone, model.EntityType);
                if (isMapped)
                {
                    ModelState.AddModelError("", $" {entityName} Mapping Already Exist");
                    var availableCategoryItems = await GetCategoryListAsync();
                    availableCategoryItems.Insert(0, new SelectListItem { Text = "Please select Entity", Value = "" });
                    model.AvailableCategories = availableCategoryItems;

                    //var availableKwTerms = await GetKwTermListAsync();
                    //availableKwTerms.Insert(0, new SelectListItem { Text = "Please select Entity", Value = "" });
                    //model.AvailableKwTerms = availableKwTerms;

                    model.AvailableWidgetZones = GetPostions();
                    return View("~/Plugins/MWT.Nop.Plugin.Widgets.Catalog/Views/AddBanner.cshtml", model);
                }
                else
                {

                    model.BannerId = model.BannerType == "DesktopBanner" ? model.BannerId : 0;
                    model.Html = model.BannerType == "DesktopHtml" ? model.Html : null;
                    model.VideoUrl = model.BannerType == "DesktopVideo" ? model.VideoUrl : null;
                    model.MobileBannerId = model.MobileBannerType == "MobileBanner" ? model.MobileBannerId : 0;
                    model.MobileHtml = model.MobileBannerType == "MobileHTML" ? model.MobileHtml : null;
                    model.MobileVideoUrl = model.MobileBannerType == "MobileVideo" ? model.MobileVideoUrl : null;
                    if (model.WidgetZone == "categorydetails_middle_product_list" || model.WidgetZone == "categorydetails_third_product_list")
                    {
                        model.MobileVideoUrl = null;
                        model.VideoUrl = null;
                    }
                    var mwtEntityBanner = new Domain.MWTEntityBanner
                    {
                        ActionLink = model.ActionLink,
                        BannerId = model.BannerId,

                        EntityType = model.EntityType,
                        Html = model.Html,
                        VideoUrl = model.VideoUrl,
                        MobileActionLink = model.MobileActionLink,
                        MobileBannerId = model.MobileBannerId,
                        MobileHtml = model.MobileHtml,
                        WidgetZone = model.WidgetZone,
                        MobileVideoUrl = model.MobileVideoUrl
                    };
                    await _imwtEntityBannerService.Insert(mwtEntityBanner);

                    foreach (var entityId in model.EntityIds)
                    {
                        await _imwtEntityBannerService.InsertEntityBannerMapping(new Domain.MWTEntityBannerEntityMapping
                        {
                            BannerId = mwtEntityBanner.Id,
                            EntityId = entityId
                        });
                    }
                    ViewBag.RefreshPage = true;

                    await UpdateSettings(model.WidgetZone, model.BannerId, model.Html, model.MobileBannerId, model.MobileHtml, model.EntityType, model.EntityIds.ToArray());
                    return View("~/Plugins/MWT.Nop.Plugin.Widgets.Catalog/Views/AddBanner.cshtml", model);
                }

            }
            else
            {
                var availableCategoryItems = await GetCategoryListAsync();
                availableCategoryItems.Insert(0, new SelectListItem { Text = "Please select Entity", Value = "" });
                model.AvailableCategories = availableCategoryItems;

                //var availableKwTerms = await GetKwTermListAsync();
                //availableKwTerms.Insert(0, new SelectListItem { Text = "Please select Entity", Value = "" });
                //model.AvailableKwTerms = availableKwTerms;

                model.AvailableWidgetZones = GetPostions();
                if (model.EntityIds.Count == 0)
                {
                    ModelState.AddModelError("", "Please select Entities");
                }
                return View("~/Plugins/MWT.Nop.Plugin.Widgets.Catalog/Views/AddBanner.cshtml", model);
            }
        }




        public async Task<IActionResult> EditBannerPopup(int id)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermission.Configuration.MANAGE_WIDGETS))
                return AccessDeniedView();

            var sbw = await _imwtEntityBannerService.GetByIdAsync(id);
            if (sbw == null)
                //no record found with the specified id
                return RedirectToAction("Configure");
            var model = new MWTEntityBannerModel
            {
                Id = sbw.Id,
                ActionLink = sbw.ActionLink,
                BannerId = sbw.BannerId,
                EntityIds = await _imwtEntityBannerService.GetEntitiesMappedWithBanner(sbw.Id),
                EntityType = sbw.EntityType,
                Html = sbw.Html,
                VideoUrl = sbw.VideoUrl,
                MobileActionLink = sbw.MobileActionLink,
                MobileHtml = sbw.MobileHtml,
                WidgetZone = sbw.WidgetZone,
                MobileBannerId = sbw.MobileBannerId,
                MobileVideoUrl = sbw.MobileVideoUrl
            };
            var availableCategoryItems = await GetCategoryListAsync();
            availableCategoryItems.Insert(0, new SelectListItem { Text = "Please select Entity", Value = "" });
            model.AvailableCategories = availableCategoryItems;
            //var availableKwTerms = await GetKwTermListAsync();
            //availableKwTerms.Insert(0, new SelectListItem { Text = "Please select Entity", Value = "" });
            //model.AvailableKwTerms = availableKwTerms;
            model.AvailableWidgetZones = GetPostions();
            model.IsHtml = string.IsNullOrEmpty(sbw.Html) ? false : true;
            model.IsMobileHtml = string.IsNullOrEmpty(sbw.MobileHtml) ? false : true;
            return View("~/Plugins/MWT.Nop.Plugin.Widgets.Catalog/Views/EditBanner.cshtml", model);
        }

        [HttpPost]
        public async Task<IActionResult> EditBannerPopup(MWTEntityBannerModel model)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermission.Configuration.MANAGE_WIDGETS))
                return AccessDeniedView();

            var sbw = await _imwtEntityBannerService.GetByIdAsync(model.Id);
            if (sbw == null)
                //no record found with the specified id
                return RedirectToAction("Configure");
            if ((ModelState.IsValid || (!ModelState.IsValid && ModelState["EntityIds"].Errors.Count > 0)) && model.EntityIds.Count > 0)
            {
                (bool isMapped, string entityName) = await _imwtEntityBannerService.IsWidgetZoneMappedWithEntityId(model.EntityIds, model.Id, model.WidgetZone, model.EntityType);
                if (isMapped)
                {
                    ModelState.AddModelError("", $" {entityName} Mapping Already Exist");
                    var availableCategoryItems = await GetCategoryListAsync();
                    availableCategoryItems.Insert(0, new SelectListItem { Text = "Please select Entity", Value = "" });
                    model.AvailableCategories = availableCategoryItems;

                    //var availableKwTerms = await GetKwTermListAsync();
                    //availableKwTerms.Insert(0, new SelectListItem { Text = "Please select Entity", Value = "" });
                    //model.AvailableKwTerms = availableKwTerms;

                    model.AvailableWidgetZones = GetPostions();
                    return View("~/Plugins/MWT.Nop.Plugin.Widgets.Catalog/Views/EditBanner.cshtml", model);
                }
                else
                {
                    if (model.BannerType == "DesktopBanner")
                    {
                        model.VideoUrl = null;
                        model.Html = null;
                    }
                    else if (model.BannerType == "DesktopHtml")
                    {
                        model.BannerId = 0;
                        model.VideoUrl = null;
                    }

                    else if (model.BannerType == "DesktopVideo")
                    {
                        model.BannerId = 0;
                        model.Html = null;
                    }

                    if (model.MobileBannerType == "MobileBanner")
                    {
                        model.MobileVideoUrl = null;
                        model.MobileHtml = null;
                    }
                    else if (model.MobileBannerType == "MobileHTML")
                    {
                        model.MobileBannerId = 0;
                        model.MobileVideoUrl = null;
                    }

                    else if (model.MobileBannerType == "MobileVideo")
                    {
                        model.MobileBannerId = 0;
                        model.MobileHtml = null;
                    }
                    if (model.WidgetZone == "categorydetails_middle_product_list" || model.WidgetZone == "categorydetails_third_product_list")
                    {
                        model.MobileVideoUrl = null;
                        model.VideoUrl = null;
                    }

                    await _imwtEntityBannerService.Update(new Domain.MWTEntityBanner
                    {
                        Id = model.Id,
                        ActionLink = model.ActionLink,
                        BannerId = model.BannerId,

                        EntityType = model.EntityType,
                        Html = model.Html,
                        MobileActionLink = model.MobileActionLink,
                        MobileBannerId = model.MobileBannerId,
                        MobileHtml = model.MobileHtml,
                        WidgetZone = model.WidgetZone,
                        VideoUrl = model.VideoUrl,
                        MobileVideoUrl = model.MobileVideoUrl
                    });
                    var mappedEntities = (await _imwtEntityBannerService.GetEntitiesMappedWithBanner(model.Id)).ToArray();
                    ViewBag.RefreshPage = true;
                    if (model.WidgetZone != sbw.WidgetZone)
                    {
                        await UpdateSettings(sbw.WidgetZone, 0, "", 0, "", sbw.EntityType, mappedEntities);
                    }
                    await UpdateSettings(model.WidgetZone, model.BannerId, model.Html, model.MobileBannerId, model.MobileHtml, model.EntityType, model.EntityIds.ToArray());

                    await UpdateSettings(model.WidgetZone, 0, "", 0, "", sbw.EntityType, mappedEntities.Except(model.EntityIds.ToArray()).ToArray());

                    var toAdd = model.EntityIds.Except(mappedEntities);
                    var toRemove = mappedEntities.Except(model.EntityIds);

                    foreach (var item in toRemove)
                    {
                        await _imwtEntityBannerService.DeleteEntityBannerMapping(new Domain.MWTEntityBannerEntityMapping()
                        {
                            EntityId = item,
                            BannerId = model.Id,

                        });
                    }

                    foreach (var item in toAdd)
                    {
                        await _imwtEntityBannerService.InsertEntityBannerMapping(new Domain.MWTEntityBannerEntityMapping()
                        {
                            EntityId = item,
                            BannerId = model.Id
                        });
                    }

                    return View("~/Plugins/MWT.Nop.Plugin.Widgets.Catalog/Views/EditBanner.cshtml", model);
                }

            }
            else
            {
                var availableCategoryItems = await GetCategoryListAsync();
                availableCategoryItems.Insert(0, new SelectListItem { Text = "Please select Entity", Value = "" });
                model.AvailableCategories = availableCategoryItems;
                //var availableKwTerms = await GetKwTermListAsync();
                //availableKwTerms.Insert(0, new SelectListItem { Text = "Please select Entity", Value = "" });
                //model.AvailableKwTerms = availableKwTerms;
                model.AvailableWidgetZones = GetPostions();
                if (model.EntityIds.Count == 0)
                {
                    ModelState.AddModelError("", "Please select Entities");
                }
                return View("~/Plugins/MWT.Nop.Plugin.Widgets.Catalog/Views/EditBanner.cshtml", model);
            }
        }

        [HttpPost]
        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task<IActionResult> Delete(int id)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermission.Configuration.MANAGE_WIDGETS))
                return Content("Access denied");

            var sbw = await _imwtEntityBannerService.GetByIdAsync(id);
            if (sbw != null)
            {
                await _imwtEntityBannerService.Delete(sbw);
                var mappedEntities = (await _imwtEntityBannerService.GetEntitiesMappedWithBanner(id)).ToArray();
                await UpdateSettings(sbw.WidgetZone, 0, "", 0, "", sbw.EntityType, mappedEntities);
            }
            return Json(null);
        }
        #endregion

        #region Utilities

        private async Task<List<SelectListItem>> GetCategoryListAsync(bool showHidden = true)
        {

            var categories = await _categoryService.GetAllCategoriesAsync(showHidden: showHidden);
            var listItems = await categories.SelectAwait(async c => new SelectListItem
            {
                Text = await _categoryService.GetFormattedBreadCrumbAsync(c, categories),
                Value = c.Id.ToString()
            }).ToListAsync();
            var result = new List<SelectListItem>();
            //clone the list to ensure that "selected" property is not set
            foreach (var item in listItems)
            {
                result.Add(new SelectListItem
                {
                    Text = item.Text,
                    Value = item.Value
                });
            }

            return result;
        }
        //private async Task<List<SelectListItem>> GetKwTermListAsync(bool showHidden = true)
        //{

        //    var kwTerms = await _kwTermService.GetAllKwTermsAsync(showHidden: showHidden, pageIndex: 0, pageSize: int.MaxValue);
        //    var listItems = await kwTerms.SelectAwait(async k => new SelectListItem
        //    {
        //        Text = await _kwTermService.GetFormattedBreadCrumbAsync(k, kwTerms),
        //        Value = k.Id.ToString()
        //    }).ToListAsync();
        //    var result = new List<SelectListItem>();
        //    //clone the list to ensure that "selected" property is not set
        //    foreach (var item in listItems)
        //    {
        //        result.Add(new SelectListItem
        //        {
        //            Text = item.Text,
        //            Value = item.Value
        //        });
        //    }

        //    return result;
        //}

        protected List<SelectListItem> GetPostions()
        {
            var result = new List<SelectListItem>();
            result.Add(new SelectListItem { Text = PublicWidgetZones.CategoryDetailsTop.ToString(), Value = PublicWidgetZones.CategoryDetailsTop.ToString() });
            result.Add(new SelectListItem { Text = PublicWidgetZones.CategoryDetailsBottom.ToString(), Value = PublicWidgetZones.CategoryDetailsBottom.ToString() });
            result.Add(new SelectListItem { Text = PublicWidgetZones.CategoryDetailsProductListThirdPosition.ToString(), Value = PublicWidgetZones.CategoryDetailsProductListThirdPosition.ToString() });

            result.Add(new SelectListItem { Text = PublicWidgetZones.CategoryDetailsProductListNinthPosition.ToString(), Value = PublicWidgetZones.CategoryDetailsProductListNinthPosition.ToString() });
            result.Add(new SelectListItem { Text = PublicWidgetZones.CategoryDetailsProductListSixthPosition.ToString(), Value = PublicWidgetZones.CategoryDetailsProductListSixthPosition.ToString() });
            result.Add(new SelectListItem { Text = PublicWidgetZones.CategoryDetailsProductListMiddle.ToString(), Value = PublicWidgetZones.CategoryDetailsProductListMiddle.ToString() });
            return result;
        }

        private async Task UpdateSettings(string widget, int bannerId, string Html, int mobileBannerId, string mobileHtml, string entityType, int[] entityIds)
        {
            var keyValueDesktop = await _settingService.GetSettingByKeyAsync<string>($"MWTPluginWidgetsCatalogSetting.{entityType}.{widget}");
            var keyValueMobile = await _settingService.GetSettingByKeyAsync<string>($"MWTPluginWidgetsCatalogSetting.Mobile.{entityType}.{widget}");

            if (keyValueDesktop == null || keyValueDesktop == $"MWTPluginWidgetsCatalogSetting.{entityType}.{widget}")
                keyValueDesktop = "";

            var lst = keyValueDesktop.Split(',').ToList();
            if (bannerId == 0 && (string.IsNullOrEmpty(Html) || Html == "<p></p>"))
            {
                foreach (var entityId in entityIds)
                {
                    if (lst.Where(m => m == entityId.ToString()).Any())
                        lst.Remove(entityId.ToString());
                }

            }
            else
            {
                foreach (var entityId in entityIds)
                {
                    if (!lst.Where(m => m == entityId.ToString()).Any())
                        lst.Add(entityId.ToString());
                }
            }



            await _settingService.SetSettingAsync($"MWTPluginWidgetsCatalogSetting.{entityType}.{widget}", string.Join(',', lst.ToArray()), 0);

            if (keyValueMobile == null || keyValueMobile == $"MWTPluginWidgetsCatalogSetting.Mobile.{entityType}.{widget}")
                keyValueMobile = "";

            lst = keyValueMobile.Split(',').ToList();
            if (mobileBannerId == 0 && (string.IsNullOrEmpty(mobileHtml) || mobileHtml == "<p></p>"))
            {
                foreach (var entityId in entityIds)
                {
                    if (lst.Where(m => m == entityId.ToString()).Any())
                        lst.Remove(entityId.ToString());
                }
            }
            else
            {
                foreach (var entityId in entityIds)
                {
                    if (!lst.Where(m => m == entityId.ToString()).Any())
                        lst.Add(entityId.ToString());
                }
            }

            await _settingService.SetSettingAsync($"MWTPluginWidgetsCatalogSetting.Mobile.{entityType}.{widget}", string.Join(',', lst.ToArray()), 0);

        }

        #endregion

    }
}
