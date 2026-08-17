using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MWT.Nop.Core.Service.Catalog;
using MWT.Nop.Core.Services.Catalog;
using MWT.Plugin.Misc.MwtStorefront.Factories;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Customization.Catalog;
using Nop.Services.Catalog;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Web.Factories;
using Nop.Web.Framework.Components;
using Nop.Web.Models.ShoppingCart;

namespace MWT.Plugin.Misc.MwtStorefront.Components
{
    public class CartSimilarProductsViewComponent : NopViewComponent
    {
        private readonly ICustomProductModelFactory _productModelFactory;
        private readonly IProductExtendedService _productService;
        private readonly ICustomSpecificationAttributeService _specificationAttributeService;
        private readonly ICategoryService _categoryService;
        private readonly IStoreContext _storeContext;
        private readonly ILocalizationService _localizationService;
        private readonly ISettingService _settingService;
        public CartSimilarProductsViewComponent(
                                                  ICustomProductModelFactory productModelFactory,
                                                  ICustomSpecificationAttributeService specificationAttributeService,
                                                  ICategoryService categoryService,
                                                  IProductExtendedService productService,
                                                  IStoreContext storeContext,
                                                  ILocalizationService localizationService,
                                                  ISettingService settingService
                                                  )
        {
            _specificationAttributeService = specificationAttributeService;
            _categoryService = categoryService;
            _productService = productService;
            _productModelFactory = productModelFactory;
            _storeContext = storeContext;
            _localizationService = localizationService;
            _settingService = settingService;
        }

        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task<IViewComponentResult> InvokeAsync(int productId, int parentGroupId, int? productThumbPictureSize)
        {
            int storeId = _storeContext.GetCurrentStore().Id;
            List<Product> products = new List<Product>();
            var product = await _productService.GetProductByIdAsync(productId);
            var productIds = new List<int>();
            string productRelation = string.Empty;

            ATCRecommendType aTCRecommendType = new ATCRecommendType();
            aTCRecommendType = product.ATCRecommendType;
            string heading = product.ATCRecommendHeading;

            string aTCRecommendedProductIds = product.ATCRecommendedProductIds;
            int mainCategoryId = await _specificationAttributeService.GetMainCategoryOfProduct(productId);
            var skipCategoryIdsSetting = ((await _settingService.GetSettingByKeyAsync<string>("Product.Skip.SimilarProducts.FallbackCategories")) ?? string.Empty).Split(',').Select(id =>
            {
                return int.TryParse(id.Trim(), out int parsedId) ? parsedId : 0;
            })
                                              .ToList();
            if (aTCRecommendType == ATCRecommendType.Other)
            {
                if (!string.IsNullOrEmpty(aTCRecommendedProductIds))
                {
                    productIds.AddRange(aTCRecommendedProductIds
                        .Split(',')
                        .Select(id => int.Parse(id.Trim())));
                }
                else
                {
                    aTCRecommendType = ATCRecommendType.None;
                }
            }

            if (aTCRecommendType == ATCRecommendType.None)
            {
                if (mainCategoryId > 0)
                {
                    var category = await _categoryService.GetCategoryByIdAsync(mainCategoryId);
                    aTCRecommendType = category.ATCRecommendType;
                    heading = category.ATCRecommendHeading;
                }
            }

            if (/*(aTCRecommendType == ATCRecommendType.None && !skipCategoryIdsSetting.Contains(mainCategoryId)) ||*/ aTCRecommendType == ATCRecommendType.SimilarItems)
            {
                productIds = (await _productService.GetRelatedProductsByProductId1Async(productId)).Select(x => x.ProductId2).ToList();
                products = (await _productService.GetProductsByIdsAsync(productIds.Where(m => m != productId).ToArray())).ToList();
                productRelation = "Related";
            }
            else if (aTCRecommendType == ATCRecommendType.Collection)
            {
                if (parentGroupId != 0)
                    products = (await _productService.GetAssociatedProductsAsync(parentGroupId, storeId)).ToList();
                products = products.Where(m => m.Id != productId).ToList();
                if (products.Count() == 0)
                {
                    productIds = await this._productService.GetCollectionAssocitedProductsAsync(productId, false);
                    //if (productIds.Count() == 0 && !skipCategoryIdsSetting.Contains(mainCategoryId))
                    //{
                    //    productIds = (await _productService.GetRelatedProductsByProductId1Async(productId)).Select(x => x.ProductId2).ToList();
                    //    productRelation = "Related";
                    //}
                    //else
                    if (productIds.Count() > 0)
                    {
                        productRelation = "Collection";
                        products = (await _productService.GetProductsByIdsAsync(productIds.Where(m => m != productId).ToArray())).ToList();
                    }
                }
                else
                {
                    productRelation = "Group";
                }

            }
            else
            {
                if (productIds.Count > 0)
                {
                    products = (await _productService.GetProductsByIdsAsync(productIds.Where(m => m != productId).ToArray())).ToList();
                    //if (products.Count() == 0 && !skipCategoryIdsSetting.Contains(mainCategoryId))
                    //{
                    //    productIds = (await _productService.GetRelatedProductsByProductId1Async(productId)).Select(x => x.ProductId2).ToList();
                    //    products = (await _productService.GetProductsByIdsAsync(productIds.Where(m => m != productId).ToArray())).ToList();
                    //}
                    productRelation = "Related";
                }
            }
            if (!products.Any())
                return Content(string.Empty);

            var model = (await _productModelFactory.PrepareCustomProductOverviewModelsAsync(products, true, true, productThumbPictureSize)).ToList();
            if (model.Count > 0)
            {
                model.FirstOrDefault().ProductRelation = string.IsNullOrEmpty(heading) ? await _localizationService.GetResourceAsync($"cart.popup.{productRelation}.Heading") : heading;
            }

            return View(model);

        }
    }
}