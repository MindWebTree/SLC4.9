using Microsoft.AspNetCore.Mvc;
using MWT.Plugin.Misc.MwtStorefront.Models.Catalog;
using Nop.Core.Domain.Catalog;
using Nop.Services.Catalog;
using Nop.Services.Seo;
using Nop.Web.Framework.Components;
using Nop.Web.Models.Catalog;

namespace MWT.Plugin.Misc.MwtStorefront.Components.Add
{
    public class BackInStockSubscriptionBlockViewComponent : NopViewComponent
    {
        private readonly CatalogSettings _catalogSettings;
        private readonly ICategoryService _categoryService;
        private readonly IUrlRecordService _urlRecordService;
        public BackInStockSubscriptionBlockViewComponent(
            CatalogSettings catalogSettings,
             ICategoryService categoryService,
             IUrlRecordService urlRecordService
            )
        {
            _catalogSettings = catalogSettings;
            _categoryService = categoryService;
            _urlRecordService = urlRecordService;
        }

        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task<IViewComponentResult> InvokeAsync(int productId)
        {
            BackInStockSubscribeExtendedModel model = new BackInStockSubscribeExtendedModel();
            var productCategories = await _categoryService.GetProductCategoriesByProductIdAsync(productId);
            if (productCategories.Any())
            {
                var category = await _categoryService.GetCategoryByIdAsync(productCategories[0].CategoryId);
                if (category != null)
                {
                    model.SeName = await _urlRecordService.GetSeNameAsync(category);
                    model.CategoryId = category.Id;
                }
            }
            model.ProductId = productId;
            return View(model);
        }
    }
}
