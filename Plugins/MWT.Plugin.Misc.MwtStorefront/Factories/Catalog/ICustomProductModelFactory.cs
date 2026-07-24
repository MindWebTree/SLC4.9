using MWT.Plugin.Misc.MwtStorefront.Models.Catalog;
using Nop.Core.Domain.Catalog;
using Nop.Web.Factories;
using Nop.Web.Models.Catalog;
namespace MWT.Plugin.Misc.MwtStorefront.Factories
{
    public  interface ICustomProductModelFactory: IProductModelFactory
    {
        Task<IEnumerable<CustomProductOverviewModel>> PrepareCustomProductOverviewModelsAsync(IEnumerable<Product> products,
               bool preparePriceModel = true, bool preparePictureModel = true,
               int? productThumbPictureSize = null, bool prepareSpecificationAttributes = false,
               bool forceRedirectionAfterAddingToCart = false, bool prepareShades = false, bool prepareCollectionSpecificationAttribute = false,
               bool prepareSizeShadeAggregation = false, bool prepareAlternatePictureModel = false, bool isCategorypage = false);


    }
}
