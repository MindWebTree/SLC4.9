using MWT.Nop.Core.Service.Catalog;
using MWT.Nop.Core.Services.Integrity_Report;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Orders;
using Nop.Services.Catalog;
using Nop.Services.Media;
using Nop.Services.Orders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MWT.Plugin.Misc.MwtStorefront.Factories
{
    public partial class ProductIntegrityReportModelFactory : IProductIntegrityReportModelFactory
    {
        #region Fields

        private readonly IProductIntegrityReportService _productIntegrityReportService;
        private readonly IPictureService _pictureService;
        private readonly IProductExtendedService _productService;
        private readonly IProductAttributeService _productAttributeService;
        private readonly IShoppingCartService _shoppingCartService;
        private readonly IProductAttributeParser _productAttributeParser;
        private readonly IWorkContext _workContext;

        #endregion

        #region Ctor

        public ProductIntegrityReportModelFactory(IProductIntegrityReportService productIntegrityReportService,
            IPictureService pictureService,
  IProductExtendedService productService,
            IProductAttributeService productAttributeService,
            IShoppingCartService shoppingCartService,
          IProductAttributeParser productAttributeParser,
          IWorkContext workContext)
        {
            this._productIntegrityReportService = productIntegrityReportService;
            this._pictureService = pictureService;
            this._productService = productService;
            this._shoppingCartService = shoppingCartService;
            this._productAttributeParser = productAttributeParser;
            this._productAttributeService = productAttributeService;
            this._workContext = workContext;
        }

        #endregion
        public virtual async Task<List<int>> GenerateProductIntegrityReportAsync(List<Product> prouctList)
        {
            var attributeProducts = await _productIntegrityReportService.GetUnpublishedOrAttributeMissingProductsAsync(prouctList);
            var priceCombinationProducts = await GetVariantAndAttributeCombinationMissMatchPriceAsync(prouctList);
            var combinationProducts = new List<(Product Product, string AttributeNames)>();

            foreach (var product in prouctList)
            {
                var allAttributesXml = await _productAttributeParser.GenerateAllCombinationsAsync(product, true, null);
                foreach (var attributesXml in allAttributesXml)
                {
                    var existingCombination = await _productAttributeParser.FindProductAttributeCombinationAsync(product, attributesXml);
                    if (existingCombination != null)
                        continue;

                    var warnings = new List<string>();
                    warnings.AddRange(await _shoppingCartService.GetShoppingCartItemAttributeWarningsAsync(await _workContext.GetCurrentCustomerAsync(),
                        ShoppingCartType.ShoppingCart, product, 1, attributesXml, true, true));
                    if (warnings.Count != 0)
                        continue;

                    combinationProducts.Add((
                       product,
                       "Issue in Product Combination Setup"
                   ));
                    break;
                }

            }

            var combined = new List<(Product product, string errorReason)>();
            combined.AddRange(attributeProducts);
            combined.AddRange(priceCombinationProducts);
            combined.AddRange(combinationProducts);

            combined = combined
      .GroupBy(x => x.product.Id)
      .Select(g => (
          Product: g.First().product,
          AttributeNames: string.Join(", ",
              g.Select(x => x.errorReason)
               .Where(s => !string.IsNullOrWhiteSpace(s))
               .Distinct()
          )
      ))
      .ToList();


            List<int> Ids = combined.Select(s => s.product.Id).ToList();
            await _productIntegrityReportService.Upsert(combined);
            return Ids;
        }

        private async Task<List<(Product Product, string ErrorReason)>> GetVariantAndAttributeCombinationMissMatchPriceAsync(List<Product> prouctList)
        {
            var priceCombinationProducts = new List<(Product Product, string ErrorReason)>();

            foreach (var product in prouctList)
            {
                bool isPriceError = false;
                var variantCombinations = await _productService.GetProductVariants(product.Id);

                var productAttributesCombinations = await _productAttributeService.GetAllProductAttributeCombinationsAsync(product.Id);

                foreach (var variantCombination in variantCombinations)
                {
                    if (string.IsNullOrWhiteSpace(variantCombination.ProductAttributeValueIds))
                        continue;

                    var attrValueIds = new HashSet<int>(
                        (variantCombination.ProductAttributeValueIds ?? string.Empty)
                            .Split('-')
                            .Select(s => int.TryParse(s, out int value) ? value : (int?)null)
                            .Where(v => v.HasValue)
                            .Select(v => v.Value)
                    );

                    foreach (var _attrCombination in productAttributesCombinations)
                    {
                        var combinationAttributeValueIds = (await _productAttributeParser.ParseProductAttributeValuesAsync(_attrCombination.AttributesXml)).Select(x => x.Id).ToList();
                        if (new HashSet<int>(attrValueIds).IsSubsetOf(combinationAttributeValueIds))
                        {

                            if ((_attrCombination.OverriddenOldPrice ?? 0) != (variantCombination.OldPrice ?? 0)
                                || _attrCombination.OverriddenPrice != variantCombination.Price
                               )
                            {
                                priceCombinationProducts.Add((product, "Price Discrepancy"));
                                isPriceError = true;
                                break;
                            }
                        }
                    }
                }
            }
            return priceCombinationProducts;
        }

    }
}
