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
        public override async Task<IList<ProductAttributeCombination>> GetAllProductAttributeCombinationsAsync(int productId)
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
        public override async Task<IList<ProductAttributeMapping>> GetProductAttributeMappingsByProductIdAsync(int productId)
        {
            var query = from pam in _productAttributeMappingRepository.Table
                        orderby pam.DisplayOrder, pam.Id
                        where pam.ProductId == productId
                        select pam;

            return await query.ToListAsync() ?? new List<ProductAttributeMapping>();
        }
       

    }
}
