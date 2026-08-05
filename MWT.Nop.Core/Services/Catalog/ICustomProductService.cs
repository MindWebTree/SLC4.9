using MWT.Nop.Core.Domain;
using MWT.Nop.Core.Domain.Catalog;
using MWTNop.Core.Domain.Catalog;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Catalog;
using Nop.Core.Infrastructure;
using Nop.Data;
using Nop.Services.Catalog;

namespace MWT.Nop.Core.Service.Catalog
{
    /// <summary>
    /// Product service
    /// </summary>
    public partial interface ICustomProductService : IProductService
    {
        Task<IList<ProductBasicInfo>> GetAllProducts();
    
      
        Task<IPagedList<Product>> CustomSearchProductsAsync(
int pageIndex = 0,
int pageSize = int.MaxValue,
IList<int> categoryIds = null,
IList<int> manufacturerIds = null,
int storeId = 0,
int vendorId = 0,
int warehouseId = 0,
ProductType? productType = null,
bool visibleIndividuallyOnly = false,
bool excludeFeaturedProducts = false,
decimal? priceMin = null,
decimal? priceMax = null,
int productTagId = 0,
string keywords = null,
bool searchDescriptions = false,
bool searchManufacturerPartNumber = true,
bool searchSku = true,
bool searchProductTags = false,
int languageId = 0,
IList<SpecificationAttributeOption> filteredSpecOptions = null,
ProductSortingEnum orderBy = ProductSortingEnum.Position,
bool showHidden = false,
bool? overridePublished = null);
        Task<IPagedList<Product>> CustomSearchProductsAsync(int[] ids, List<int> _specificationAttributes, ProductSortingEnum orderBy = ProductSortingEnum.Position,
               int pageIndex = 0,
        int pageSize = int.MaxValue, int categoryId = 0, int featuredId = 0, bool featuredListingDisplaySimilarOnTop = false, bool isMobileDevice = false, int kwTermId = 0, int questionAnswerId = 0);
        Task<IList<Product>> GetCollectionProductsBygroupAsync(int productId, int parentGroupedProductId,
            int storeId = 0, int vendorId = 0, bool showHidden = false);
        Task<List<int>> GetCollectionProductsByProductId1Async(int productId, bool showHidden = false);
        Task<List<int>> GetCollectionAssocitedProductsAsync(int productId, bool showHidden = false);
        Task<IList<CollectionProduct>> GetCollectionProductsByProductId1ListAsync(int productId, bool showHidden = false);
        Task<CollectionProduct> GetCollectionProductByIdAsync(int relatedProductId);
        Task UpdateCollectionProductAsync(CollectionProduct CollectionProduct);
        Task DeleteCollectionProductAsync(CollectionProduct CollectionProduct);
        Task InsertCollectionProductAsync(CollectionProduct collectionProduct);
        Task<(IList<Product> products, int parentProductId)> GetCollectionProductsAsync(int productId, int pageSize,
       int storeId = 0, int vendorId = 0, bool showHidden = false);


        #region Misc
        Task<IList<Product>> GetNewArrivalProductsAsync(int pageIndex, int pageSize);

        Task<(string offerText, string offerPlaceHolder, string discountAmount, decimal discountPercentage, DateTime? saleStartDate, DateTime? saleEndDate)> GetProductSaleOfferInfo(Product product, decimal? oldPrice, decimal? price);

        Task<List<ProductSpecificationAttribute>> SearchGetProductSpecificationAttributeAsync(
        int pageIndex = 0,
int pageSize = int.MaxValue,
IList<int> categoryIds = null,
IList<int> manufacturerIds = null,
int storeId = 0,
int vendorId = 0,
int warehouseId = 0,
ProductType? productType = null,
bool visibleIndividuallyOnly = false,
bool excludeFeaturedProducts = false,
decimal? priceMin = null,
decimal? priceMax = null,
int productTagId = 0,
string keywords = null,
bool searchDescriptions = false,
bool searchManufacturerPartNumber = true,
bool searchSku = true,
bool searchProductTags = false,
int languageId = 0,
IList<SpecificationAttributeOption> filteredSpecOptions = null,
ProductSortingEnum orderBy = ProductSortingEnum.Position,
bool showHidden = false,
bool? overridePublished = null,
bool showOutOfStock = false, int featuredId = 0, bool isMobileDevice = false, int kwTermId = 0, int questionAnswerId = 0);



        Task<IPagedList<Product>> SearchProductsAsync(int[] ids, List<int> _specificationAttributes, ProductSortingEnum orderBy = ProductSortingEnum.Position,
                 int pageIndex = 0,
          int pageSize = int.MaxValue, int categoryId = 0, int featuredId = 0, bool featuredListingDisplaySimilarOnTop = false, bool isMobileDevice = false, int kwTermId = 0, int questionAnswerId = 0);

        Task<IPagedList<Product>> SearchProductsWithVariablePageSizeAsync(int[] ids, List<int> _specificationAttributes, ProductSortingEnum orderBy = ProductSortingEnum.Position,
                       int pageIndex = 0,
                int pageSize = int.MaxValue, int firstPageSize = int.MaxValue, int subsequentPageSize =
           int.MaxValue, int categoryId = 0, int featuredId = 0, bool featuredListingDisplaySimilarOnTop =
           false, bool isMobileDevice = false, int kwTermId = 0, int questionAnswerId = 0);

        #endregion



        #region PairWith
        Task<List<int>> GetPairWithProductsByProductId1Async(int productId, bool showHidden = false);
        Task<IList<PairWithProduct>> GetPairWithProductsByProductId1ListAsync(int productId, bool showHidden = false);
        Task<PairWithProduct> GetPairWithProductByIdAsync(int relatedProductId);
        Task UpdatePairWithProductAsync(PairWithProduct pairwithProduct);
        Task DeletePairWithProductAsync(PairWithProduct pairwithProduct);
        PairWithProduct FindPairWithProduct(IList<PairWithProduct> source, int productId1, int productId2);
        Task InsertPairWithProductAsync(PairWithProduct pairwithProduct);
        #endregion


        Task<List<Product>> GetGroupProductsByEntity(int categoryId, int storeId);
        Task<IList<Product>> GetRecommendationProducts(int[]
            productIds, int numberOfRecommendedProducts);
        Task<IList<Product>> GetShoppingCartRecommendationProducts(int[]
            productIds, int numberOfRecommendedProducts); 
       Task UpdateProductWithoutEvent(Product product);
        Task<Product> GetProductBySearchTerm(string term);
        Task UpdateProductInventory(Product product);
        Task<int> GetProductInventory(Product product);
        Task<IPagedList<Product>> OverriddenSearchProductsAsync(
        int pageIndex = 0,
        int pageSize = int.MaxValue,
        IList<int> categoryIds = null,
        IList<int> manufacturerIds = null,
        int storeId = 0,
        int vendorId = 0,
        int warehouseId = 0,
        ProductType? productType = null,
        bool visibleIndividuallyOnly = false,
        bool excludeFeaturedProducts = false,
        decimal? priceMin = null,
        decimal? priceMax = null,
        int productTagId = 0,
        string keywords = null,
        bool searchDescriptions = false,
        bool searchManufacturerPartNumber = true,
        bool searchSku = true,
        bool searchProductTags = false,
        int languageId = 0,
        IList<SpecificationAttributeOption> filteredSpecOptions = null,
        ProductSortingEnum orderBy = ProductSortingEnum.Position,
        bool showHidden = false,
        bool? overridePublished = null);
         Task<IPagedList<Product>> GetAccessibleSearchProductsAsync(
 int pageIndex = 0,
 int pageSize = int.MaxValue,
 int customerId = 0,
 IList<int> categoryIds = null,
 IList<int> manufacturerIds = null,
 int storeId = 0,
 int vendorId = 0,
 int warehouseId = 0,
 ProductType? productType = null,
 bool visibleIndividuallyOnly = false,
 bool excludeFeaturedProducts = false,
 decimal? priceMin = null,
 decimal? priceMax = null,
 int productTagId = 0,
 string keywords = null,
 bool searchDescriptions = false,
 bool searchManufacturerPartNumber = true,
 bool searchSku = true,
 bool searchProductTags = false,
 int languageId = 0,
 IList<SpecificationAttributeOption> filteredSpecOptions = null,
 ProductSortingEnum orderBy = ProductSortingEnum.Position,
 bool showHidden = false,
 bool? overridePublished = null);
        #region FBT
        Task DeleteFBTProductAsync(FBTProduct FBTProduct);
        Task<IList<FBTProduct>> GetFBTProductsByProductId1Async(int productId,
            bool showHidden = false, bool showOutOfStock = true);
        Task<FBTProduct> GetFBTProductByIdAsync(int FBTProductId);
        Task UpdateFBTProductAsync(FBTProduct FBTProduct);
        Task InsertFBTProductAsync(FBTProduct FBTProduct);
        FBTProduct FindFBTProduct(IList<FBTProduct> source, int productId1, int productId2);
        Task<IList<Product>> GetServiceTypeProducts();


        #endregion




        Task<IPagedList<Product>> GetProductsByProductAtributeIdAsync(int productAttributeId
    , string search, int categoryId, int pageIndex = 0, int pageSize = int.MaxValue);
        Task<string> GetDefaultCombinationAttrXml(IList<ProductAttributeMapping> productAttributeMapping);
        Task< MWT.Nop.Core.Domain.StoreWideDiscount.StoreWideDiscount> GetProductSaleInfo(int productId);
          Task<(decimal, decimal, decimal, bool)> GetDefaultAttributePriceOfProduct(Product product);

    }
}
