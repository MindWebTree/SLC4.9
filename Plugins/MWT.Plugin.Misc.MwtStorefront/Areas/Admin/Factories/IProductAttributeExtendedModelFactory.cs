using Nop.Core.Domain.Catalog;
using Nop.Web.Areas.Admin.Factories;
using Nop.Web.Areas.Admin.Models.Catalog;

namespace MWT.Plugin.Misc.MwtStorefront.Areas.Admin.Factories
{
    public partial interface IProductAttributeExtendedModelFactory : IProductAttributeModelFactory
    {
        Task<PredefinedProductAttributeValueListModel> Stains(
            PredefinedProductAttributeValueSearchModel searchModel, ProductAttribute productAttribute);
        Task<ProductAttributeModel> CustomPrepareProductAttributeModelAsync(ProductAttributeModel model,
          ProductAttribute productAttribute, bool excludeProperties = false);

        Task<ProductAttributeProductListModel> CustomPrepareProductAttributeProductListModelAsync(ProductAttributeProductSearchModel searchModel,
        ProductAttribute productAttribute);
    }
}
