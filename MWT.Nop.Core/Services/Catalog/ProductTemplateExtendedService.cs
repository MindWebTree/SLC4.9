using MWT.Nop.Core.Domain.Catalog;
using MWT.Nop.Core.Service.Catalog;
using Nop.Core.Caching;
using Nop.Core.Domain.Catalog;
using Nop.Data;
using Nop.Services.Catalog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MWT.Nop.Core.Services.Catalog
{
    public class ProductTemplateSectionService : IProductTemplateSectionService
    {
        private readonly IRepository<ProductTemplateSection> _productTemplateSectionrepository;
        private readonly IStaticCacheManager _staticCacheManager;

        public ProductTemplateSectionService(
            IRepository<ProductTemplateSection> productTemplateSectionrepository, IStaticCacheManager staticCacheManager)
        {
            _productTemplateSectionrepository = productTemplateSectionrepository;
            _staticCacheManager = staticCacheManager;
        }

        public virtual async Task<IList<ProductTemplateSection>> GetProductTemplateSectionsByTemplateIdAsync(int templateId)
        {
            var query = from pts in _productTemplateSectionrepository.Table
                        where pts.TemplateId == templateId
                        orderby pts.SortOrder, pts.Id
                        select pts;
            var productTemplateSections = await _staticCacheManager.GetAsync(_staticCacheManager.PrepareKeyForDefaultCache(CustomNopCatalogDefaults.ProductTemplateSectionsCacheKey, templateId), async () => await query.ToListAsync());
            return productTemplateSections;
        }
    }
}
