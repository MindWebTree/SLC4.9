using MWT.Nop.Core.Services.Catalog;
using MWTNop.Core.Domain.Catalog;
using Nop.Services.Catalog;
using Nop.Services.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MWT.Plugin.Misc.MwtStorefront.Factories.Catalog
{
    public class VariantModelFactory: IVariantModelFactory
    {
        private readonly IVariantService  _variantService;
        private readonly ICustomProductAttributeService  _customProductAttributeService;
        private readonly ILocalizationService _localizationService;
        public VariantModelFactory(IVariantService variantService , ICustomProductAttributeService customProductAttributeService,
            ILocalizationService localizationService)
        {
            _variantService = variantService;
            _customProductAttributeService = customProductAttributeService;
            _localizationService = localizationService;
        }
        public async Task<int> GetVariantIdBySize(int productId, string size)
        {
            int variantId = 0;
            var productAttributeMapping = await _customProductAttributeService.GetProductAttributeMappingsByProductIdAsync(productId);
            foreach (var attribute in productAttributeMapping)
            {
                var productAttrubute = await _customProductAttributeService.GetProductAttributeByIdAsync(attribute.ProductAttributeId);

                if (attribute.ShouldHaveValues())
                {
                    //values
                    var attributeValues = await _customProductAttributeService.GetProductAttributeValuesAsync(attribute.Id);
                    if (productAttrubute.Name.Equals(await _localizationService.GetResourceAsync("Product.Attr.Size"), StringComparison.InvariantCultureIgnoreCase))
                    {

                        if (attributeValues.Where(a =>
                        string.Equals(string.IsNullOrEmpty(a.QueryParameter) ? "N/A" : a.QueryParameter.Trim(), size.Trim(), StringComparison.InvariantCultureIgnoreCase) && a.Published == true && a.VariantId > 0).Any())
                            variantId = attributeValues.Where(a =>
                          string.Equals(string.IsNullOrEmpty(a.QueryParameter) ? "" : a.QueryParameter.Trim(), size.Trim(), StringComparison.InvariantCultureIgnoreCase) && a.Published == true && a.VariantId > 0).FirstOrDefault().VariantId;
                    }
                }
            }
            return variantId;
        }
        public async Task<VariantCombination> ValidateVariantID(int productId, int variantId)
        {
            VariantCombination variantCombination = (await _variantService.GetProductVariants(productId)).Where(v => v.VariantId == variantId).FirstOrDefault();
            if (variantCombination != null)
            {

                if (!string.IsNullOrEmpty(variantCombination.ProductAttributeValueIds))
                {
                    foreach (var attributeValueId in variantCombination.ProductAttributeValueIds
                                            .Split("-")
                                            .Select(id => int.Parse(id))
                                            .ToList())
                    {
                        if (!(await _customProductAttributeService.GetProductAttributeValueByIdAsync(attributeValueId))?.Published ?? false)
                        {
                            variantCombination = null;
                            break;
                        }
                    }
                }
            }
            return variantCombination;
        }
        public async Task<bool> IsVariantSurchargeApplicable(int variantId)
        {
            return (await _variantService.GetVariantByVariantId(variantId))?.EnableSurcharge ?? false;
        }

    }
}
