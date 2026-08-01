using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Data;
using Nop.Services.Stores;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MWT.Nop.Core.Services.Catalog
{
    public partial class FeedService: IFeedService
    {
        private readonly IRepository<Product> _productRepository;
        private readonly IStoreMappingService _storeMappingService;
        public FeedService( IRepository<Product> productrepository, IStoreMappingService storeMappingService)
        {
            _productRepository = productrepository;
            _storeMappingService = storeMappingService;
        }
        public async Task<IPagedList<Product>> ProductsFeedAsync(
int pageIndex = 0,
int pageSize = int.MaxValue,
int storeId = 0)
        {
            //some databases don't support int.MaxValue
            if (pageSize == int.MaxValue)
                pageSize = int.MaxValue - 1;

            var productsQuery = _productRepository.Table;
            productsQuery = productsQuery.Where(p => p.Published);

            productsQuery = productsQuery.Where(p => !p.Deleted);
            //apply store mapping constraints
            productsQuery = await _storeMappingService.ApplyStoreMapping(productsQuery, storeId);
            productsQuery = productsQuery.Where(p => p.TotalInventory > 0);
            productsQuery = productsQuery.Where(p => p.VisiblityOncategoryPage == null || p.VisiblityOncategoryPage == 1);
            productsQuery = productsQuery.Where(p => !p.Sku.StartsWith("3000000"));
            productsQuery = productsQuery.OrderBy(p => p.Id);

            return await productsQuery.ToPagedListAsync(pageIndex, pageSize);

        }
    }
}
