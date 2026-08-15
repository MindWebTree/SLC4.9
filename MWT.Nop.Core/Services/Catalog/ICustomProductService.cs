using MWT.Nop.Core.Domain;
using MWT.Nop.Core.Domain.Catalog;
using MWT.Nop.Core.Domain.StoreWideDiscount;
using MWTNop.Core.Domain.Catalog;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Catalog;
using Nop.Core.Infrastructure;
using Nop.Data;
using Nop.Services.Catalog;

namespace MWT.Nop.Core.Service.Catalog
{
    public partial interface IProductExtendedService : IProductService
    {

        #region Products
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
        Task<(string offerText, string offerPlaceHolder, string discountAmount, decimal discountPercentage, DateTime? saleStartDate, DateTime? saleEndDate)> GetProductSaleOfferInfo(Product product, decimal? oldPrice, decimal? price);
        Task<List<ProductSpecificationAttribute>> CustomSearchGetProductSpecificationAttributeAsync(
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
        Task<IPagedList<Product>> CustomSearchProductsAsync(int[] ids, List<int> _specificationAttributes, ProductSortingEnum orderBy = ProductSortingEnum.Position,
                  int pageIndex = 0,
           int pageSize = int.MaxValue, int categoryId = 0, int featuredId = 0, bool featuredListingDisplaySimilarOnTop = false, bool isMobileDevice = false, int kwTermId = 0, int questionAnswerId = 0);
        Task<IPagedList<Product>> CustomSearchProductsWithVariablePageSizeAsync(int[] ids, List<int> _specificationAttributes, ProductSortingEnum orderBy = ProductSortingEnum.Position,
               int pageIndex = 0,
        int pageSize = int.MaxValue, int firstPageSize = int.MaxValue, int subsequentPageSize = int.MaxValue, int categoryId = 0, int featuredId = 0, bool featuredListingDisplaySimilarOnTop = false, bool isMobileDevice = false, int kwTermId = 0, int questionAnswerId = 0);
        Task<IList<int>> GetCustomRelatedProductsByProductId1Async(int productId, int pageSize, bool showHidden = false);
        Task UpdateProductWithoutEvent(Product product);
        Task<bool> IsNewAtcLayoutActive(Product product);
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

        #endregion

        #region ProductAttribute
        Task<IPagedList<Product>> CustomGetProductsByProductAtributeIdAsync(int productAttributeId
      , string search, int categoryId, int pageIndex = 0, int pageSize = int.MaxValue);

        #endregion

       

        #region Import UnPubshed Product Variant

        public Task ImportProductUnpubishedVariant(int productId);

        #endregion




        #region SaleInfo

        Task<StoreWideDiscount> GetProductSaleInfo(int productId);

        #endregion

        #region Collection
        Task<IList<Product>> CustomGetCollectionProductsBygroupAsync(int productId, int parentGroupedProductId,
                int storeId = 0, int vendorId = 0, bool showHidden = false);

        Task<List<int>> GetCollectionProductsByProductId1Async(int productId, bool showHidden = false);

        Task<List<int>> GetCollectionAssocitedProductsAsync(int productId, bool showHidden = false);

        Task<IList<CollectionProduct>> GetCollectionProductsByProductId1ListAsync(int productId, bool showHidden = false);
        Task<CollectionProduct> GetCollectionProductByIdAsync(int relatedProductId);
        Task UpdateCollectionProductAsync(CollectionProduct CollectionProduct);
        Task DeleteCollectionProductAsync(CollectionProduct CollectionProduct);
        CollectionProduct FindCollectionProduct(IList<CollectionProduct> source, int productId1, int productId2);
        Task InsertCollectionProductAsync(CollectionProduct collectionProduct);
        Task<(IList<Product> products, int parentProductId)> CustomGetCollectionProductsAsync(int productId, int pageSize,
       int storeId = 0, int vendorId = 0, bool showHidden = false);

        #endregion

        #region Feed
        Task<IPagedList<Product>> ProductsFeedAsync(int pageIndex = 0, int pageSize = int.MaxValue, int storeId = 0);

        #endregion

        #region Pair With

        Task<List<int>> GetPairWithProductsByProductId1Async(int productId, bool showHidden = false);

        Task<IList<PairWithProduct>> GetPairWithProductsByProductId1ListAsync(int productId, bool showHidden = false);
        Task<PairWithProduct> GetPairWithProductByIdAsync(int relatedProductId);
        Task UpdatePairWithProductAsync(PairWithProduct pairwithProduct);
        Task DeletePairWithProductAsync(PairWithProduct pairwithProduct);
        PairWithProduct
            FindPairWithProduct(IList<PairWithProduct> source, int productId1, int productId2);
        Task InsertPairWithProductAsync(PairWithProduct pairwithProduct);

        #endregion


        #region FBTProduct
        Task DeleteFBTProductAsync(FBTProduct fBTProduct);
        Task<IList<FBTProduct>> GetFBTProductsByProductId1Async(int productId1, bool showHidden = false, bool showOutOfStock = true);
        Task<FBTProduct> GetFBTProductByIdAsync(int fBTProductId);
        Task InsertFBTProductAsync(FBTProduct fBTProduct);
        Task UpdateFBTProductAsync(FBTProduct fBTProduct);
        FBTProduct FindFBTProduct(IList<FBTProduct> source, int productId1, int productId2);

        #endregion

        #region ProductVariant
        Task<VariantCombination> ValidateVariantID(int productId, int variantId);
        Task<int> GetProductDefaultVariantId(int productId);
        Task<int> GetVariantId(int productId, string attributes);
        Task<int> GetProductVariantIdBySize(int productId, string size);
        Task<(bool, decimal, decimal, decimal, decimal, decimal, decimal)> GetVariantPriceRange(Product product, bool createCombination = true, bool updateProduct = true
            , bool updateProductWithEvent = true, bool isVariantTask = true);
        Task<int> GenerateVariantIdAsync(ProductAttributeMapping mapping, int productAttributeValueId, string productAttributeValue);
        Task<bool> GenerateVariantCombinations(int productId);
        Task<List<VariantCombination>> GetProductVariants(int productId);
        Task<List<VariantCombination>> GetPublishedProductVariants(int productId);
        Task<VariantCombination> GetVariantById(int id);
        Task UpdateVariant(VariantCombination variant, bool updateCombinations = true);
        Task<int> GetVariantIdFromAttributeDescription(int productId, string attributeDescription);
        Task<VariantCombination> GetVariantByVariantId(int variantId);
        Task<VariantCombination> GetVariantFromAttributeValues(int productId, List<int> attributeValueIds);
        Task GenerateProductVariantSename(Product product);
        Task<VariantCombination> GetItemVariantInfo(int productId, string attributesXml);

        Task<List<ProductAttributeCombination>> GetVariantCombinationAsync(VariantCombination variant, IList<ProductAttributeCombination> productAttributeCombinations);

        #endregion

        #region Misc
        Task<bool> SyncGroupedProductsPrice();
        Task<bool> UpdateGroupProductPrice(int productId);
        Task<List<Product>> GetGroupProductsByEntity(int categoryId, int storeId);
        Task<IList<Product>> GetRecommendationProducts(int[] productIds, int numberOfRecommendedProducts);
        Task<IList<Product>> GetShoppingCartRecommendationProducts(int[] productIds, int numberOfRecommendedProducts);
        Task<IList<Product>> GetServiceTypeProducts();
        Task<IList<Product>> GetNewArrivalProductsAsync(int pageIndex, int pageSize);

        #endregion
    }
}
