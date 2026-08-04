using Microsoft.AspNetCore.Mvc;
using MWT.Nop.Core.Services.CategoryCollection;
using MWT.Plugin.Misc.MwtStorefront.Models.Custom;
using Nop.Services.Localization;
using Nop.Web.Framework.Components;

namespace MWT.Plugin.Misc.MwtStorefront.Components
{
    public class HeadLinksViewComponent : NopViewComponent
    {
        #region Fields

        private readonly ILocalizationService _localizationService;
        private readonly ICategoryCollectionLinkService _categoryCollectionLinkService;

        #endregion

        #region Ctor

        public HeadLinksViewComponent(
            ILocalizationService localizationService,
            ICategoryCollectionLinkService categoryCollectionLinkService)
        {
            _localizationService = localizationService;
            _categoryCollectionLinkService = categoryCollectionLinkService;
        }

        #endregion

        #region Methods
        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task<IViewComponentResult> InvokeAsync(int categoryId, string catSeName, string currentPageurl)
        {
            if (categoryId == 0)
                return Content("");

            var collectionLinks = await _categoryCollectionLinkService.GetCategoryCollectionLinkByEntityId(categoryId);
            if (!collectionLinks.Any())
                return Content("");

            HeadLinkModel model = new HeadLinkModel();
            model.Links.Add(new CollectionLinkModel()
            {
                Link = catSeName.ToLower().Trim() == currentPageurl.ToLower().Trim() ? "" : catSeName,
                Title = await _localizationService.GetResourceAsync("Catalog.Heading.Products"),
                IsActive = catSeName.ToLower().Trim() == currentPageurl.ToLower().Trim() ? true : false
            });

            foreach (var link in collectionLinks)
            {
                model.Links.Add(new CollectionLinkModel()
                {
                    Link = link.Link.ToLower().Trim() == currentPageurl.ToLower().Trim() ? "" : link.Link.Trim(),
                    Title = link.Title,
                    IsActive = link.Link.ToLower().Trim() == currentPageurl.ToLower().Trim() ? true : false
                });
            }
            return View(model);
        }

        #endregion
    }
}

