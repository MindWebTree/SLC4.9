//using MWTNop.Core.Domain.Catalog;
//using Nop.Core;
//using Nop.Core.Caching;
//using Nop.Core.Domain.Catalog;
//using Nop.Core.Domain.Discounts;
//using Nop.Core.Domain.Localization;
//using Nop.Core.Domain.Shipping;
//using Nop.Core.Infrastructure;
//using Nop.Data;
//using Nop.Services.Catalog;
//using Nop.Services.Customers;
//using Nop.Services.Localization;
//using Nop.Services.Security;
//using Nop.Services.Shipping.Date;
//using Nop.Services.Stores;
//using Nop.Services.Vendors;


//namespace MWT.Nop.Core.Service.Catalog
//{
 
//    public partial class CustomProductService : ProductService , ICustomProductService
//    { 
//        protected readonly IStaticCacheManager _staticCacheManager;
//        public CustomProductService(CatalogSettings catalogSettings, IAclService aclService, ICustomerService customerService, IDateRangeService dateRangeService, ILanguageService languageService, ILocalizationService localizationService, IProductAttributeParser productAttributeParser, IProductAttributeService productAttributeService, IRepository<Category> categoryRepository, IRepository<CrossSellProduct> crossSellProductRepository, IRepository<DiscountProductMapping> discountProductMappingRepository, IRepository<LocalizedProperty> localizedPropertyRepository, IRepository<Manufacturer> manufacturerRepository, IRepository<Product> productRepository, IRepository<ProductAttributeCombination> productAttributeCombinationRepository, IRepository<ProductAttributeMapping> productAttributeMappingRepository, IRepository<ProductCategory> productCategoryRepository, IRepository<ProductManufacturer> productManufacturerRepository, IRepository<ProductPicture> productPictureRepository, IRepository<ProductProductTagMapping> productTagMappingRepository, IRepository<ProductSpecificationAttribute> productSpecificationAttributeRepository, IRepository<ProductTag> productTagRepository, IRepository<ProductVideo> productVideoRepository, IRepository<ProductWarehouseInventory> productWarehouseInventoryRepository, IRepository<RelatedProduct> relatedProductRepository, IRepository<Shipment> shipmentRepository, IRepository<StockQuantityHistory> stockQuantityHistoryRepository, IRepository<TierPrice> tierPriceRepository, ISearchPluginManager searchPluginManager, IStaticCacheManager staticCacheManager, IVendorService vendorService, IStoreMappingService storeMappingService, IWorkContext workContext, LocalizationSettings localizationSettings) : base(catalogSettings, aclService, customerService, dateRangeService, languageService, localizationService, productAttributeParser, productAttributeService, categoryRepository, crossSellProductRepository, discountProductMappingRepository, localizedPropertyRepository, manufacturerRepository, productRepository, productAttributeCombinationRepository, productAttributeMappingRepository, productCategoryRepository, productManufacturerRepository, productPictureRepository, productTagMappingRepository, productSpecificationAttributeRepository, productTagRepository, productVideoRepository, productWarehouseInventoryRepository, relatedProductRepository, shipmentRepository, stockQuantityHistoryRepository, tierPriceRepository, searchPluginManager, staticCacheManager, vendorService, storeMappingService, workContext, localizationSettings)
//        {
//            _staticCacheManager = staticCacheManager;
//        }

//        public async Task<List<VariantCombination>> GetProductVariants(int productId)
//        {
//            var _variantcombination = EngineContext.Current.Resolve<IRepository<VariantCombination>>();
//            return await _staticCacheManager.GetAsync(_staticCacheManager.PrepareKeyForDefaultCache(CustomNopCatalogDefaults.ProductVariantsCacheKey, productId), async () => await _variantcombination.Table.Where(vc => vc.ProductId == productId).ToListAsync());
//        }

//    }

//}
