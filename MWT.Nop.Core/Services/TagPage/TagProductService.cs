using MWT.Nop.Core.Domain;
using MWT.Nop.Core.Services.Catalog;
using MWT.Nop.Core.Services.Configuration;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Data;
using Nop.Services.Security;
using Nop.Services.Stores;

namespace MWT.Nop.Core.Services.TagPage
{
    public partial class TagProductService : ITagProductService
    {
        #region Props
        private readonly IRepository<Product> _productRepository;
        private readonly IRepository<ProductProductTagMapping> _productTagMappingRepository;
        private readonly IRepository<ProductSpecificationAttribute> _productSpecificationAttributeRepository;
        private readonly IStoreMappingService _storeMappingService;
        private readonly IWorkContext _workContext;
        private readonly IAclService _aclService;
        private readonly IRepository<ProductWarehouseInventory> _productWarehouseInventoryRepository;
        private readonly IRepository<ProductCategory> _productCategoryRepository;
        private readonly IRepository<BestsellerPool> _bestSellerPoolRepository;
        private readonly TagAutomationSettings _tagAutomationSettings;

        #endregion

        #region Ctor

        public TagProductService(IRepository<Product> productRepository, IRepository<ProductProductTagMapping> productTagMappingRepository,
            IRepository<ProductSpecificationAttribute> productSpecificationAttributeRepository,
            IStoreMappingService storeMappingService,
           IWorkContext workContext, IAclService aclService,
           IRepository<ProductWarehouseInventory> productWarehouseInventoryRepository,
           IRepository<ProductCategory> productCategoryRepository,
           IRepository<BestsellerPool> bestSellerPoolRepository, TagAutomationSettings tagAutomationSettings)
        {
            _productRepository = productRepository;
            _productTagMappingRepository = productTagMappingRepository;
            _productSpecificationAttributeRepository = productSpecificationAttributeRepository;
            _storeMappingService = storeMappingService;
            _workContext = workContext;
            _aclService = aclService;
            _productWarehouseInventoryRepository = productWarehouseInventoryRepository;
            _productCategoryRepository = productCategoryRepository;
            _bestSellerPoolRepository = bestSellerPoolRepository;
            _tagAutomationSettings = tagAutomationSettings;
        }
        #endregion

        #region Methods
        public async Task<IPagedList<Product>> SearchProductsWithVariablePageSizeAsync(int[] ids, int tagId, List<int> _specificationAttributes, ProductSortingEnum orderBy = ProductSortingEnum.Position, int pageIndex = 0, int pageSize = int.MaxValue, bool isMobileDevice = false)
        {
          

            var productsQuery = _productRepository.Table.Where(entry => ids.Contains(entry.Id));

            if (_specificationAttributes.Any())
            {
                var productSpecificationQuery =
                      from pc in _productCategoryRepository.Table
                      where _specificationAttributes.Contains(pc.CategoryId)
                      orderby isMobileDevice ? pc.MobileDisplayOrder : pc.DisplayOrder
                      group pc by pc.ProductId into pc
                      select new
                      {
                          ProductId = pc.Key,
                          DisplayOrder = isMobileDevice ? pc.FirstOrDefault().MobileDisplayOrder : pc.FirstOrDefault().DisplayOrder
                      };

                if (_tagAutomationSettings.BestsellerTagId == tagId)
                {
                    productsQuery =
          from p in productsQuery
          join pc in productSpecificationQuery on p.Id equals pc.ProductId
          join bp in _bestSellerPoolRepository.Table on p.Id equals bp.ProductId
          orderby bp.DisplayOrder, pc.DisplayOrder, p.Name
          select p;
                }
                else
                    productsQuery =
            from p in productsQuery
            join pc in productSpecificationQuery on p.Id equals pc.ProductId
            join ptm in _productTagMappingRepository.Table on p.Id equals ptm.ProductId
            where ptm.ProductTagId == tagId
            orderby ptm.DisplayOrder, pc.DisplayOrder, p.Name
            select p;

            }
            else
            {
                if (_tagAutomationSettings.BestsellerTagId == tagId)
                    productsQuery =
                       from p in productsQuery
                       join bp in _bestSellerPoolRepository.Table on p.Id equals bp.ProductId

                       orderby bp.DisplayOrder, p.Name
                       select p;
                else
                    productsQuery =
                    from p in productsQuery
                    join ptm in _productTagMappingRepository.Table on p.Id equals ptm.ProductId
                    where ptm.ProductTagId == tagId
                    orderby ptm.DisplayOrder, p.Name
                    select p;
            }

            return await productsQuery.CustomOrderBy(orderBy).ToPagedListAsync(pageIndex, pageSize); ;

        }

        public virtual async Task<List<ProductSpecificationAttribute>> CustomSearchGetProductSpecificationAttributeAsync(
    IList<int> categoryIds = null,
   int storeId = 0,
    int vendorId = 0,
    int warehouseId = 0,
    ProductType? productType = null,
    bool visibleIndividuallyOnly = false,
    bool excludeFeaturedProducts = false,
   int productTagId = 0,
    int languageId = 0,
    bool? overridePublished = null,
    bool showOutOfStock = false, int featuredId = 0, bool isMobileDevice = false, bool showHidden = false)
        {

            var productsQuery = _productRepository.Table;
            if (featuredId != 0)
                productsQuery = productsQuery.Where(p => p.Id != featuredId);
            if (!showHidden)
                productsQuery = productsQuery.Where(p => p.Published);
            else if (overridePublished.HasValue)
                productsQuery = productsQuery.Where(p => p.Published == overridePublished.Value);

            if (!showOutOfStock)
                productsQuery = productsQuery.Where(p => p.TotalInventory >= 0);

            productsQuery = productsQuery.Where(p => p.VisiblityOncategoryPage == null || p.VisiblityOncategoryPage == 1);
            //apply store mapping constraints
            productsQuery = await _storeMappingService.ApplyStoreMapping(productsQuery, storeId);

            //apply ACL constraints
            if (!showHidden)
            {
                var customer = await _workContext.GetCurrentCustomerAsync();
                productsQuery = await _aclService.ApplyAcl(productsQuery, customer);
            }

            productsQuery =
                from p in productsQuery
                where !p.Deleted &&
                    (!visibleIndividuallyOnly || p.VisibleIndividually) &&
                    (vendorId == 0 || p.VendorId == vendorId) &&
                    (
                        warehouseId == 0 ||
                        (
                            !p.UseMultipleWarehouses ? p.WarehouseId == warehouseId :
                                _productWarehouseInventoryRepository.Table.Any(pwi => pwi.Id == warehouseId && pwi.ProductId == p.Id)
                        )
                    ) &&
                    (productType == null || p.ProductTypeId == (int)productType) &&
                    (showHidden || LinqToDB.Sql.Between(DateTime.UtcNow, p.AvailableStartDateTimeUtc ?? DateTime.MinValue, p.AvailableEndDateTimeUtc ?? DateTime.MaxValue))

                select p;



            if (categoryIds is not null)
            {
                if (categoryIds.Contains(0))
                    categoryIds.Remove(0);

                if (categoryIds.Any())
                {
                    var productCategoryQuery =
                        from pc in _productCategoryRepository.Table
                        where (!excludeFeaturedProducts || !pc.IsFeaturedProduct) &&
                            categoryIds.Contains(pc.CategoryId)
                        group pc by pc.ProductId into pc
                        select new
                        {
                            ProductId = pc.Key,
                            DisplayOrder = isMobileDevice ? pc.First().MobileDisplayOrder : pc.First().DisplayOrder
                        };

                    productsQuery =
                        from p in productsQuery
                        join pc in productCategoryQuery on p.Id equals pc.ProductId
                        orderby pc.DisplayOrder, p.Name
                        select p;
                }
            }




            if (productTagId > 0)
            {
     
                if (productTagId == _tagAutomationSettings.BestsellerTagId)
                {

                    productsQuery =
                       from p in productsQuery
                       join ptm in _bestSellerPoolRepository.Table on p.Id equals ptm.ProductId
                       select p;
                }
                else
                {
                    productsQuery =
                        from p in productsQuery
                        join ptm in _productTagMappingRepository.Table on p.Id equals ptm.ProductId
                        where ptm.ProductTagId == productTagId
                        select p;
                }
              
            }
            var query = from p in productsQuery
                        join psa in _productCategoryRepository.Table on p.Id equals psa.ProductId into psaj
                        from psaComb in psaj.DefaultIfEmpty()

                        select new ProductSpecificationAttribute
                        {
                            ProductId = p.Id,
                            SpecificationAttributeOptionId = psaComb == null ? 0 : psaComb.CategoryId,
                            DisplayOrder = psaComb == null ? 0 : isMobileDevice ? psaComb.MobileDisplayOrder : psaComb.DisplayOrder
                        };

            var products = await (query).ToListAsync();

            return products;

        }
        #endregion
    }
}
