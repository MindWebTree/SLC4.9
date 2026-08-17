using MWT.Nop.Core.Domain;
using MWT.Nop.Core.Domain.CustomOrders;
using MWT.Nop.Core.Services.Configuration;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Orders;
using Nop.Data;
using Nop.Services.Catalog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MWT.Nop.Core.Services.Catalog
{
    public class BestsellerPoolService : IBestsellerPoolService
    {
        #region Fields

        private readonly IProductService _productService;
        private readonly IProductTagService _productTagService;
        private readonly ICategoryService _categoryService;
        private readonly IRepository<Order> _orderRepository;
        private readonly IRepository<CustomOrder> _customOrderRepository;
        private readonly IRepository<OrderItem> _orderItemRepository;
        private readonly IRepository<BestsellerPool> _bestsellerPoolRepository;
        private readonly ICustomSpecificationAttributeService _specificationAttributeService;
        private readonly TagAutomationSettings _tagAutomationSettings;

        #endregion

        #region Ctor

        public BestsellerPoolService(
            IProductService productService,
            IProductTagService productTagService,
            ICategoryService categoryService,
            IRepository<Order> orderRepository,
            IRepository<OrderItem> orderItemRepository,
            IRepository<BestsellerPool> bestsellerPoolRepository,
            ICustomSpecificationAttributeService specificationAttributeService,
            TagAutomationSettings tagAutomationSettings,
            IRepository<CustomOrder> customOrderRepository)
        {
            _productService = productService;
            _productTagService = productTagService;
            _categoryService = categoryService;
            _orderRepository = orderRepository;
            _orderItemRepository = orderItemRepository;
            _bestsellerPoolRepository = bestsellerPoolRepository;
            _specificationAttributeService = specificationAttributeService;
            _tagAutomationSettings = tagAutomationSettings;
            _customOrderRepository = customOrderRepository;
        }

        #endregion

        #region Utilities

        /// <summary>
        /// Get distinct order count for a product in last N days
        /// </summary>
        private async Task<int> GetProductOrderCountAsync(int productId, int days)
        {
            var fromDate = DateTime.UtcNow.AddDays(-days);

            return await (
                from oi in _orderItemRepository.Table
                join o in _orderRepository.Table on oi.OrderId equals o.Id
                where oi.ProductId == productId
                      && !o.Deleted
                      && o.CreatedOnUtc >= fromDate
                      && o.OrderTotal > 100
                select o.Id
            ).Distinct().CountAsync();
        }

        /// <summary>
        /// Get primary category ID for a product
        /// </summary>
        private async Task<int> GetPrimaryCategoryIdAsync(int productId)
        {
            return await _specificationAttributeService.GetMainCategoryOfProduct(productId);
        }

        /// <summary>
        /// Check if product has a specific tag
        /// </summary>
        private async Task<bool> ProductHasTagAsync(int productId, string tagName)
        {
            var tags = await _productTagService.GetAllProductTagsByProductIdAsync(productId);
            return tags.Any(t => t.Name.Equals(tagName, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Get all existing pool entries
        /// </summary>
        private async Task<IList<BestsellerPool>> GetExistingPoolAsync()
        {
            return await _bestsellerPoolRepository.GetAllAsync(q => q);
        }

        #endregion

        #region Methods

        /// <summary>
        /// Rebuild the bestseller pool
        /// Tag names and limits read from TagAutomationSettings
        ///
        /// Steps:
        /// 1. Get all Bestseller tagged products with performance data
        /// 2. Group by primary category — take max N per category (from settings)
        /// 3. Rank final pool by dual tag, order count, price
        /// 4. Smart diff — delete ineligible, insert new, update display order
        /// </summary>
        public async Task RebuildPoolAsync()
        {
            // Read from settings
            var bestsellerTagName = _tagAutomationSettings.BestsellerTagName;
            var newArrivalTagName = _tagAutomationSettings.NewArrivalTagName;
            var bestsellerDays = _tagAutomationSettings.BestsellerDays;
            var maxPerCategoryInPool = _tagAutomationSettings.MaxBestsellerPoolPerCategory;

            // Step 1 — Get bestseller tag
            var allTags = await _productTagService.GetAllProductTagsAsync();
            var bestsellerTag = allTags.FirstOrDefault(t =>
                t.Name.Equals(bestsellerTagName, StringComparison.OrdinalIgnoreCase));

            if (bestsellerTag == null)
                return;

            // Step 2 — Get all Bestseller tagged products using pagination
            var bestsellerProducts = new List<Product>();
            var pageNumber = 0;

            while (true)
            {
                var page = await _productService.SearchProductsAsync(
                    pageIndex: pageNumber,
                    pageSize: 50,
                    visibleIndividuallyOnly: true,
                    overridePublished: true,
                    productTagId: bestsellerTag.Id);

                bestsellerProducts.AddRange(page);

                if (!page.HasNextPage)
                    break;

                pageNumber++;
            }

            // Step 3 — Build performance data including dual tag check
            var allPerformance = new List<(int ProductId, int CategoryId, int OrderCount, decimal Price, bool IsDualTag)>();

            foreach (var product in bestsellerProducts)
            {
                if (product == null || product.Deleted || !product.Published)
                    continue;

                var primaryCategoryId = await GetPrimaryCategoryIdAsync(product.Id);
                if (primaryCategoryId == 0)
                    continue;

                var orderCount = await GetProductOrderCountAsync(product.Id, bestsellerDays);
                var isNewArrival = await ProductHasTagAsync(product.Id, newArrivalTagName);

                allPerformance.Add((product.Id, primaryCategoryId, orderCount, product.Price, isNewArrival));
            }

            // Step 4 — Group by category take max N per category from settings
            var poolEntries = new List<(int ProductId, int CategoryId, int OrderCount, decimal Price, bool IsDualTag)>();
            var byCategory = allPerformance.GroupBy(p => p.CategoryId);

            foreach (var categoryGroup in byCategory)
            {
                var taken = categoryGroup
                    .OrderByDescending(p => p.IsDualTag)
                    .ThenByDescending(p => p.OrderCount)
                    .ThenByDescending(p => p.Price)
                    .Take(maxPerCategoryInPool)
                    .ToList();

                poolEntries.AddRange(taken);
            }

            // Step 5 — Rank final combined pool
            var rankedPool = poolEntries
                .OrderByDescending(p => p.IsDualTag)
                .ThenByDescending(p => p.OrderCount)
                .ThenByDescending(p => p.Price)
                .ToList();

            // Step 6 — Smart diff
            var existingPool = await GetExistingPoolAsync();
            var existingProductIds = existingPool.Select(e => e.ProductId).ToHashSet();
            var newProductIds = rankedPool.Select(p => p.ProductId).ToHashSet();

            // Delete entries no longer in pool
            var toDelete = existingPool
                .Where(e => !newProductIds.Contains(e.ProductId))
                .ToList();

            foreach (var entry in toDelete)
                await _bestsellerPoolRepository.DeleteAsync(entry);

            // Insert or update remaining entries
            for (var i = 0; i < rankedPool.Count; i++)
            {
                var item = rankedPool[i];
                var newDisplayOrder = i + 1;
                var existing = existingPool.FirstOrDefault(e => e.ProductId == item.ProductId);

                if (existing != null)
                {
                    if (existing.DisplayOrder != newDisplayOrder)
                    {
                        existing.DisplayOrder = newDisplayOrder;
                        await _bestsellerPoolRepository.UpdateAsync(existing);
                    }
                }
                else
                {
                    await _bestsellerPoolRepository.InsertAsync(new BestsellerPool
                    {
                        ProductId = item.ProductId,
                        CategoryId = item.CategoryId,
                        DisplayOrder = newDisplayOrder
                    });
                }
            }
        }

        /// <summary>
        /// Get all products in the bestseller pool ordered by DisplayOrder
        /// </summary>
        public async Task<IList<BestsellerPool>> GetPoolProductsAsync()
        {
            return await _bestsellerPoolRepository.GetAllAsync(
                q => q.OrderBy(p => p.DisplayOrder));
        }

        #endregion
    }
}
