


using Microsoft.AspNetCore.Mvc;
using MWT.Nop.Plugin.Widgets.Catalog.Infrastructure.Cache;
using MWT.Nop.Plugin.Widgets.Catalog.Models;
using MWT.Nop.Plugin.Widgets.Catalog.Services;
using Nop.Core.Caching;
using Nop.Services.Catalog;
using Nop.Services.Media;
using Nop.Web.Framework.Components;
using Nop.Web.Framework.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MWT.Nop.Plugin.Widgets.Catalog.Components
{
    [ViewComponent(Name = "mwtCatalogWidget")]
    public class mwtCatalogWidgetViewComponent : NopViewComponent
    {
        #region Fields

        private readonly IMWTEntityBannerService _iMWTEntityBannerService;
        private readonly IStaticCacheManager _staticCacheManager;
        private readonly IPictureService _pictureService;
        private readonly ICategoryService _categoryService;

        #endregion
        public mwtCatalogWidgetViewComponent(IMWTEntityBannerService iMWTEntityBannerService, IStaticCacheManager staticCacheManager,
            IPictureService pictureService, ICategoryService categoryService)
        {
            this._iMWTEntityBannerService = iMWTEntityBannerService;
            this._staticCacheManager = staticCacheManager;
            this._pictureService = pictureService;
            this._categoryService = categoryService;
        }
        public async Task<IViewComponentResult> InvokeAsync(string widgetZone, object additionalData)
        {
            string type = additionalData.GetType().Name;
            var cacheKey = _staticCacheManager.PrepareKeyForDefaultCache(MWTCatalogWidgetsDefaults.MWTCatalogWidgetCacheKey, type.Contains("Category") ? "Category" : "KwTerm",((BaseNopEntityModel)additionalData).Id, widgetZone);
            MWTEntityBannerModel result = await _staticCacheManager.GetAsync(cacheKey, async () =>
            {
                var obj = await _iMWTEntityBannerService.GetWidgetByEntityId(((BaseNopEntityModel)additionalData).Id, widgetZone == "categorydetails_top_video" ? "categorydetails_top" : widgetZone, type.Contains("Category") ? "Category" : "KwTerm");
                if (obj == null)
                    return null;
                else
                {
                    var title = (await _categoryService.GetCategoryByIdAsync(((BaseNopEntityModel)additionalData).Id))?.Name;

                    var model = new MWTEntityBannerModel
                    {
                        Id = obj.Id,
                        ActionLink = obj.ActionLink,
                        BannerId = obj.BannerId,
                        EntityId = ((BaseNopEntityModel)additionalData).Id,
                        EntityType = obj.EntityType,
                        Html = obj.Html,
                        VideoUrl = obj.VideoUrl,
                        MobileActionLink = obj.MobileActionLink,
                        MobileHtml = obj.MobileHtml,
                        MobileVideoUrl = obj.MobileVideoUrl,
                        WidgetZone = widgetZone == "categorydetails_top_video" ? "categorydetails_top_video" : obj.WidgetZone,
                        MobileBannerId = obj.MobileBannerId,
                        Title = title
                    };
                    if (obj.BannerId != 0)
                    {
                        model.bannerImage = await _pictureService.GetPictureUrlAsync(
                          obj.BannerId, 0, false);
                    }
                    if (obj.MobileBannerId != 0)
                    {
                        model.MobilebannerImage = await _pictureService.GetPictureUrlAsync(
                          obj.MobileBannerId, 0, false);
                    }

                    return model;
                }
            });
            if (result == null)
                return Content(string.Empty);
            return View("~/Plugins/MWT.Nop.Plugin.Widgets.Catalog/Views/PublicInfo.cshtml", result);
        }
    }
}
