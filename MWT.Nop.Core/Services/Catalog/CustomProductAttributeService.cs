using Nop.Core.Caching;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Media;
using Nop.Data;
using Nop.Services.Catalog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MWT.Nop.Core.Services.Catalog
{
    public partial class CustomProductAttributeService : ProductAttributeService, ICustomProductAttributeService
    {
        public CustomProductAttributeService(IRepository<Picture> pictureRepository, IRepository<PredefinedProductAttributeValue> predefinedProductAttributeValueRepository, IRepository<Product> productRepository, IRepository<ProductAttribute> productAttributeRepository, IRepository<ProductAttributeCombination> productAttributeCombinationRepository, IRepository<ProductAttributeCombinationPicture> productAttributeCombinationPictureRepository, IRepository<ProductAttributeMapping> productAttributeMappingRepository, IRepository<ProductAttributeValue> productAttributeValueRepository, IRepository<ProductAttributeValuePicture> productAttributeValuePictureRepository, IRepository<ProductPicture> productPictureRepository, IStaticCacheManager staticCacheManager) : base(pictureRepository, predefinedProductAttributeValueRepository, productRepository, productAttributeRepository, productAttributeCombinationRepository, productAttributeCombinationPictureRepository, productAttributeMappingRepository, productAttributeValueRepository, productAttributeValuePictureRepository, productPictureRepository, staticCacheManager)
        {
        }
        public virtual async Task UpdateProductAttributeValueWithoutEventAsync(ProductAttributeValue productAttributeValue)
        {
            await _productAttributeValueRepository.UpdateAsync(productAttributeValue, false);
        }
        public virtual async Task<IList<ProductAttributeCombination>> CustomGetAllProductAttributeCombinationsAsync(int productId)
        {
            if (productId == 0)
                return new List<ProductAttributeCombination>();

            return await _productAttributeCombinationRepository.GetAllAsync(query =>
            {
                return from c in query
                       orderby c.Id
                       where c.ProductId == productId
                       select c;
            });
        }
        public virtual async Task<IList<ProductAttributeMapping>> CustomGetProductAttributeMappingsByProductIdAsync(int productId)
        {
            var query = from pam in _productAttributeMappingRepository.Table
                        orderby pam.DisplayOrder, pam.Id
                        where pam.ProductId == productId
                        select pam;

            return await query.ToListAsync() ?? new List<ProductAttributeMapping>();
        }
        public virtual async Task<IList<ProductAttributeValue>> CustomGetProductAttributeValuesAsync(int productAttributeMappingId)
        {
            var key = _staticCacheManager.PrepareKeyForDefaultCache(NopCatalogDefaults.ProductAttributeValuesByAttributeCacheKey, productAttributeMappingId);

            var query = from pav in _productAttributeValueRepository.Table
                        orderby pav.DisplayOrder, pav.Id
                        where pav.ProductAttributeMappingId == productAttributeMappingId
                        select pav;

            return await query.ToListAsync();
        }
        public virtual async Task<IList<string>> Stains(int productAttributeId, string search)
        {
            var query = from ppav in _productAttributeValueRepository.Table
                        join pam in _productAttributeMappingRepository.Table
                        on ppav.ProductAttributeMappingId equals pam.Id
                        where pam.ProductAttributeId == productAttributeId
                        select ppav;
            if (!string.IsNullOrEmpty(search))
                query = query.Where(s => s.Name.Contains(search));


            return await query.Select(s => s.Name).Distinct().ToListAsync();

        }
        public virtual async Task CustomUpdateProductAttributeCombinationAsync(ProductAttributeCombination combination)
        {
            await _productAttributeCombinationRepository.UpdateAsync(combination, false);
        }


    }
}
