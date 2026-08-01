using MWT.Nop.Core.Service.Catalog;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Logging;
using Nop.Core.Infrastructure;
using Nop.Data;
using Nop.Services.Catalog;
using Nop.Services.Configuration;
using Nop.Services.Logging;
using Nop.Services.Security;
using Nop.Services.Stores;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MWT.Nop.Core.Services.Catalog
{
    public partial class CustomSpecificationAttributeService :  SpecificationAttributeService, ICustomSpecificationAttributeService
    {
        public CustomSpecificationAttributeService(CatalogSettings catalogSettings, IAclService aclService, ICategoryService categoryService, IRepository<Product> productRepository, IRepository<ProductCategory> productCategoryRepository, IRepository<ProductManufacturer> productManufacturerRepository, IRepository<ProductSpecificationAttribute> productSpecificationAttributeRepository, IRepository<SpecificationAttribute> specificationAttributeRepository, IRepository<SpecificationAttributeOption> specificationAttributeOptionRepository, IRepository<SpecificationAttributeGroup> specificationAttributeGroupRepository, IStoreContext storeContext, IStoreMappingService storeMappingService, IStaticCacheManager staticCacheManager, IWorkContext workContext) : base(catalogSettings, aclService, categoryService, productRepository, productCategoryRepository, productManufacturerRepository, productSpecificationAttributeRepository, specificationAttributeRepository, specificationAttributeOptionRepository, specificationAttributeGroupRepository, storeContext, storeMappingService, staticCacheManager, workContext)
        {
        }
        #region Methods
        public virtual async Task<List<ProductSpecificationAttribute>> GetProductSpecificationAttributesByAttributeIdAsync(
                   int productId, int specificationAttributeId = 0)
        {

            var query = _productSpecificationAttributeRepository.Table;
            query = query.Where(psa => psa.ProductId == productId);

            var key = _staticCacheManager.PrepareKeyForDefaultCache(NopCatalogDefaults.ProductSpecificationAttributeByProductCacheKey,
              productId, specificationAttributeId, "coll-null", "coll-null", "coll-null");


            query = from psa in query
                    join sao in _specificationAttributeOptionRepository.Table
                        on psa.SpecificationAttributeOptionId equals sao.Id
                    join sa in _specificationAttributeRepository.Table
                        on sao.SpecificationAttributeId equals sa.Id
                    where sa.Id == specificationAttributeId
                    select psa;

            var productSpecificationAttributes = await _staticCacheManager.GetAsync(key, async () => await query.ToListAsync());

            return productSpecificationAttributes;
        }

        public virtual async Task<List<SpecificationAttribute>> GetAllSpecificationAttributesAsync()
        {
            var _settingService = EngineContext.Current.Resolve<ISettingService>();
            int filterGroupId = await _settingService.GetSettingByKeyAsync<int>("Specification.Filter.Id");

            var query = from sa in _specificationAttributeRepository.Table
                        where sa.SpecificationAttributeGroupId == filterGroupId
                        orderby sa.DisplayOrder, sa.Id
                        select sa;


            return await _staticCacheManager.GetAsync(_staticCacheManager.PrepareKeyForDefaultCache(CustomNopCatalogDefaults.SpecificationAttributesByGroup, filterGroupId), async () => await query.ToListAsync());


        }

        public virtual async Task<List<SpecificationAttributeOption>> GetAllSpecificationOptionsAsync()
        {
            var query = from sa in _specificationAttributeOptionRepository.Table
                        orderby sa.DisplayOrder, sa.Id
                        select sa;

            return await _staticCacheManager.GetAsync(_staticCacheManager.PrepareKeyForDefaultCache(CustomNopCatalogDefaults.AllSpecificationOptions), async () => await query.ToListAsync());
        }


        public async Task<int> GetMainCategoryOfProduct(int productId)
        {
            int maincategoryId = 0;
            try
            {
                var _settingService = EngineContext.Current.Resolve<ISettingService>();
                var mainCategorySpecificationAttributeId = await _settingService.GetSettingByKeyAsync<int>("mainCategorySpecificationAttributeId");
                if (mainCategorySpecificationAttributeId != 0)
                {
                    var cacheKey = _staticCacheManager.PrepareKeyForDefaultCache(CustomNopCatalogDefaults.MainCategorySpecificationAttributeByProductCacheKey,
   productId, mainCategorySpecificationAttributeId);
                    maincategoryId = await _staticCacheManager.GetAsync(cacheKey, async () =>
                    {
                        int _mainCategoryid = 0;
                        var _specOptionAttributeOption = await (from _prdSpecAttribute in _productSpecificationAttributeRepository.Table
                                                                join specOptionAttributeOption in _specificationAttributeOptionRepository.Table
                                                                on _prdSpecAttribute.SpecificationAttributeOptionId
                                                                equals specOptionAttributeOption.Id
                                                                join _specificationAttribute in _specificationAttributeRepository.Table
                                                                on specOptionAttributeOption.SpecificationAttributeId equals _specificationAttribute.Id

                                                                where _prdSpecAttribute.ProductId == productId && _specificationAttribute.Id
                                                               == mainCategorySpecificationAttributeId
                                                                select specOptionAttributeOption).FirstOrDefaultAsync();
                        if (_specOptionAttributeOption != null)
                            int.TryParse(_specOptionAttributeOption.Name, out _mainCategoryid);
                        return _mainCategoryid;
                    });
                }
            }
            catch (Exception exp)
            {
                var _iLogger = EngineContext.Current.Resolve<ILogger>();
                await _iLogger.InsertLogAsync(LogLevel.Error,
                    "Failed to get Main Categoty Attribute, Function: GetMainCategorySpecificationAttribute",
                    exp.Message);
            }
            return maincategoryId;
        }


        public virtual async Task<IPagedList<ProductSpecificationAttribute>> GetProductsBySpecificationAttributeOptionIdAsync(int specificationAttributeOptionId, int pageIndex, int pageSize)
        {
            var query = from psa in _productSpecificationAttributeRepository.Table
                        where psa.SpecificationAttributeOptionId == specificationAttributeOptionId
                        orderby psa.DisplayOrder
                        select psa;

            return await query.ToPagedListAsync(pageIndex, pageSize);
        }

        public virtual async Task<IPagedList<ProductSpecificationAttribute>> GetProductsSearchBySpecificationAttributeOptionIdAsync(int specificationAttributeOptionId, string search, int categoryId, int pageIndex, int pageSize)
        {
            var productsQuery = _productRepository.Table;



            if (categoryId != 0)
            {
                var productCategoryQuery =
                       from pc in _productCategoryRepository.Table
                       where pc.CategoryId == categoryId
                       select pc.ProductId;
                productsQuery =
                 from p in productsQuery
                 join pc in productCategoryQuery on p.Id equals pc
                 select p;
            }


            if (!string.IsNullOrEmpty(search))
            {


                IQueryable<int> productsByKeywords;
                productsByKeywords =
                        from p in _productRepository.Table
                        where p.Name.Contains(search) || p.Sku == search || p.Id.ToString() == search
                        select p.Id;
                productsQuery =
                      from p in productsQuery
                      from pbk in LinqToDB.LinqExtensions.InnerJoin(productsByKeywords, pbk => pbk == p.Id)
                      select p;
            }

            var productSpecificationQuery = from psa in _productSpecificationAttributeRepository.Table
                                            where psa.SpecificationAttributeOptionId == specificationAttributeOptionId
                                            select psa;


            var query =
                from p in productsQuery
                join pc in productSpecificationQuery on p.Id equals pc.ProductId
                orderby pc.DisplayOrder
                select pc;

            return await query.ToPagedListAsync(pageIndex, pageSize);
        }

        public virtual async Task<List<ProductSpecificationAttribute>> GetProductSpecificationAttributeBySpecificationAttributeOptionIdAndProductIdAsync(int specificationAttributeOptionId, int productiD)
        {
            var query = from psa in _productSpecificationAttributeRepository.Table
                        where psa.SpecificationAttributeOptionId == specificationAttributeOptionId
                        && psa.ProductId == productiD
                        orderby psa.DisplayOrder
                        select psa;

            return await query.ToListAsync();
        }

        public virtual async Task<IPagedList<Product>> CustomGetProductsBySpecificationAttributeIdAsync
      (int specificationAttributeId, string search, int categoryId, int pageIndex, int pageSize)
        {

            var productsQuery = _productRepository.Table;



            if (categoryId != 0)
            {
                var productCategoryQuery =
                       from pc in _productCategoryRepository.Table
                       where pc.CategoryId == categoryId
                       select pc.ProductId;
                productsQuery =
                 from p in productsQuery
                 join pc in productCategoryQuery on p.Id equals pc
                 select p;
            }


            if (!string.IsNullOrEmpty(search))
            {


                IQueryable<int> productsByKeywords;
                productsByKeywords =
                        from p in _productRepository.Table
                        where p.Name.Contains(search) || p.Sku == search || p.Id.ToString() == search
                        select p.Id;
                productsQuery =
                      from p in productsQuery
                      from pbk in LinqToDB.LinqExtensions.InnerJoin(productsByKeywords, pbk => pbk == p.Id)
                      select p;
            }


            var productSpecificationQuery = from psa in _productSpecificationAttributeRepository.Table
                                            join spao in _specificationAttributeOptionRepository.Table on psa.SpecificationAttributeOptionId equals spao.Id
                                            where spao.SpecificationAttributeId == specificationAttributeId
                                            select psa;


            var query =
                from p in productsQuery
                join ps in productSpecificationQuery on p.Id equals ps.ProductId
                join so in _specificationAttributeOptionRepository.Table
                on ps.SpecificationAttributeOptionId equals so.Id

                select new Product { Id = p.Id, Name = p.Name, Sku = so.Name, ProductTemplateId = so.Id };

            return await query.ToPagedListAsync(pageIndex, pageSize);


        }

        public virtual async Task<List<ProductSpecificationAttribute>> CustomGetProductsSpecificationAttributeOptionByCategoryWiseAsync(
        int categoryId, int specificationAttributeOptionId = 0)
        {
            var query = from psa in _productSpecificationAttributeRepository.Table
                        join pcm in _productCategoryRepository.Table
                            on psa.ProductId equals pcm.ProductId
                        where psa.SpecificationAttributeOptionId == specificationAttributeOptionId
                              && pcm.CategoryId == categoryId
                        select psa;

            return await query.Distinct().ToListAsync();
        }
        #endregion
    }
}
