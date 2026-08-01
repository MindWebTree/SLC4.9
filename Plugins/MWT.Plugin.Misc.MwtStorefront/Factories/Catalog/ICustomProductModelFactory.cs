using MWT.Plugin.Misc.MwtStorefront.Models.Catalog;
using MWT.Plugin.Misc.MwtStorefront.Models.ProductApi;
using MWTNop.Core.Domain.Catalog;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Orders; 
using Nop.Web.Factories;
using Nop.Web.Models.Catalog;
using Nop.Web.Models.Media;

namespace MWT.Plugin.Misc.MwtStorefront.Factories
{
    public  interface ICustomProductModelFactory: IProductModelFactory
    {
         
        Task<IEnumerable<CustomProductOverviewModel>> PrepareCustomProductOverviewModelsAsync(IEnumerable<Product> products,
              bool preparePriceModel = true, bool preparePictureModel = true,
              int? productThumbPictureSize = null, bool prepareSpecificationAttributes = false,
              bool forceRedirectionAfterAddingToCart = false, bool prepareShades = false, bool prepareCollectionSpecificationAttribute = false,
              bool prepareSizeShadeAggregation = false, bool prepareAlternatePictureModel = false, bool isCategorypage = false); 
        Task<List<GroupedProductConfigurationModel>> PrepareGroupedProductConfiguration(int productId);
        Task<string> GetProductSku(string sku, int id);
        Task<List<ProductApiDetailsModel>> PrepareApiProductListModelAsync(int pageNumber, int productId);
        Task<string> GetProductMainImage(int productId);
 

        Task<List<VariantCombination>> PrepareProductVariants(string sku);



    }
}
