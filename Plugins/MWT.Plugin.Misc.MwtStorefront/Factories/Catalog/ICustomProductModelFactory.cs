using MWT.Plugin.Misc.MwtStorefront.Models.Api;
using MWT.Plugin.Misc.MwtStorefront.Models.Catalog;
using MWT.Plugin.Misc.MwtStorefront.Models.Media;
using MWTNop.Core.Domain.Catalog;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Orders;
using Nop.Web.Factories;
using Nop.Web.Models.Catalog;
using Nop.Web.Models.Media;

namespace MWT.Plugin.Misc.MwtStorefront.Factories
{
    public partial interface ICustomProductModelFactory:IProductModelFactory
    {
        Task<int> GetVariantIdBySize(int productId, string size);
        Task<VariantCombination> ValidateVariantID(int productId, int variantId);
        Task<IEnumerable<CustomProductOverviewModel>> PrepareCustomProductOverviewModelsAsync(IEnumerable<Product> products,
                      bool preparePriceModel = true, bool preparePictureModel = true,
                      int? productThumbPictureSize = null, bool prepareSpecificationAttributes = false,
                      bool forceRedirectionAfterAddingToCart = false, bool prepareShades = false, bool prepareCollectionSpecificationAttribute = false,
                      bool prepareSizeShadeAggregation = false, bool prepareAlternatePictureModel = false, bool isCategorypage = false);

        Task<IEnumerable<CustomProductOverviewModel>> PrepareCustomProductOverviewDetailInfoModelAsync(IEnumerable<Product> products,
     bool preparePriceModel = true, bool preparePictureModel = true,
     int? productThumbPictureSize = null, bool prepareSpecificationAttributes = false,
     bool forceRedirectionAfterAddingToCart = false, bool prepareShades = false, bool prepareCollectionSpecificationAttribute = false,
     bool prepareSizeShadeAggregation = false, bool prepareAlternatePictureModel = false, int variantId = 0);

        Task<List<CustomProductOverviewModel.ProductAttributeModelWithImage>> PrepareCustomProductAttributeModelWithImageAsync(int productId, string attributeName, int? productThumbPictureSize = null);
        Task<CustomProductDetailsModel> PrepareCustomProductDetailsModelAsync(Product product,
                        ShoppingCartItem updatecartitem = null, bool isAssociatedProduct = false, int variantId = 0);

        Task<CustomizationFormModel> PrepareCustomizationFormModelAsync(Product product);
        Task<CategoryGroupProductModel> GetCategoryGroupedProducts(int categoryId);
        Task<List<GroupedProductConfigurationModel>> PrepareGroupedProductConfiguration(int productId);
        Task<List<ProductModel>> PrepareProductFeed(int pageNumber, int pageSize);
        Task<List<ProductModel>> PrepareProductFeedVersion2(int pageNumber, int pageSize);
        Task<CustomPictureModel> PrepareCustomProductOverviewPictureModelAsync(Product product, int? productThumbPictureSize = null, bool isCategorypage = false);


        #region Feed

        Task<IList<ProductSpecificationAttributeModel>> CustomfeedPrepareProductSpecificationAttributeModelAsync(Product product, SpecificationAttributeGroup group);

        Task<IList<ProductSpecificationAttributeModel>> PrepareCustomProductSpecificationAttributeModelAsync(Product product, SpecificationAttributeGroup group);
        #endregion

        #region Api Factory Methods
        Task<List<ProductApiDetailsModel>> PrepareApiProductListModelAsync(int pageNumber, int productId);
        Task<string> GetProductMainImage(int productId);
        Task<bool> IsVariantSurchargeApplicable(int variantId);

        Task<List<VariantCombination>> PrepareProductVariants(string sku);

        #endregion




    }
}
