//using MWTNop.Core.Domain.Catalog;
//using Nop.Core;
//using Nop.Core.Caching;
//using Nop.Core.Domain.Catalog;
//using Nop.Core.Infrastructure;
//using Nop.Data;
//using Nop.Services.Catalog;

//namespace MWT.Nop.Core.Service.Catalog
//{
//    /// <summary>
//    /// Product service
//    /// </summary>
//    public partial interface ICustomProductService:IProductService
//    {

//        public async Task<List<VariantCombination>> GetProductVariants(int productId)
//        {
//            var _variantcombination = EngineContext.Current.Resolve<IRepository<VariantCombination>>();
//            return await _staticCacheManager.GetAsync(_staticCacheManager.PrepareKeyForDefaultCache(CustomNopCatalogDefaults.ProductVariantsCacheKey, productId), async () => await _variantcombination.Table.Where(vc => vc.ProductId == productId).ToListAsync());
//        }
//    }
//}
