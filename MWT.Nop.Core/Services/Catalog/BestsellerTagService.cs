using MWT.Nop.Core.Service.Catalog;
using MWT.Nop.Core.Services.Configuration;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Orders;
using Nop.Data;
using Nop.Services.Catalog;

namespace MWT.Nop.Core.Services.Catalog
{
    public class BestsellerTagService : IBestsellerTagService
    {
        #region Fields

        private readonly IProductExtendedService _productService;
        private readonly IProductTagService _productTagService;
        private readonly ICategoryService _categoryService;
        private readonly IRepository<Order> _orderRepository;
        private readonly IRepository<OrderItem> _orderItemRepository;
        private readonly IRepository<ProductProductTagMapping> _productTagMappingRepository;
        private readonly ICustomSpecificationAttributeService _specificationAttributeService;
        private readonly TagAutomationSettings _tagAutomationSettings;

        #endregion

        #region Ctor

        public BestsellerTagService(
            IProductExtendedService productService,
            IProductTagService productTagService,
            ICategoryService categoryService,
            IRepository<Order> orderRepository,
            IRepository<OrderItem> orderItemRepository,
            IRepository<ProductProductTagMapping> productTagMappingRepository,
            ICustomSpecificationAttributeService specificationAttributeService,
            TagAutomationSettings tagAutomationSettings)
        {
            _productService = productService;
            _productTagService = productTagService;
            _categoryService = categoryService;
            _orderRepository = orderRepository;
            _orderItemRepository = orderItemRepository;
            _productTagMappingRepository = productTagMappingRepository;
            _specificationAttributeService = specificationAttributeService;
            _tagAutomationSettings = tagAutomationSettings;
        }

        #endregion

        #region Utilities

        /// <summary>
        /// Get tag by name — returns null if not found
        /// </summary>
        private async Task<ProductTag> GetTagByNameAsync(string tagName)
        {
            var allTags = await _productTagService.GetAllProductTagsAsync();
            return allTags.FirstOrDefault(t =>
                t.Name.Equals(tagName, StringComparison.OrdinalIgnoreCase));
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
        /// Add tag to product
        /// </summary>
        private async Task AddTagAsync(Product product, string tagName)
        {
            var currentTags = await _productTagService.GetAllProductTagsByProductIdAsync(product.Id);
            var tagNames = currentTags.Select(t => t.Name).ToList();

            if (!tagNames.Any(t => t.Equals(tagName, StringComparison.OrdinalIgnoreCase)))
            {
                tagNames.Add(tagName);
                await _productTagService.UpdateProductTagsAsync(product, tagNames.ToArray());
            }
        }

        /// <summary>
        /// Remove tag from product
        /// </summary>
        private async Task RemoveTagAsync(Product product, string tagName)
        {
            var currentTags = await _productTagService.GetAllProductTagsByProductIdAsync(product.Id);
            await _productTagService.UpdateProductTagsAsync(
                product,
                currentTags
                    .Where(t => !t.Name.Equals(tagName, StringComparison.OrdinalIgnoreCase))
                    .Select(t => t.Name)
                    .ToArray());
        }

        /// <summary>
        /// Get distinct order count for a product in last N days
        /// Counts orders not quantities
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
        /// Update DisplayOrder on ProductProductTagMapping
        /// </summary>
        private async Task UpdateQueueIdAsync(int productId, int tagId, int queueId)
        {
            var mapping = await _productTagMappingRepository.Table
                .FirstOrDefaultAsync(m =>
                    m.ProductId == productId &&
                    m.ProductTagId == tagId);

            if (mapping == null)
                return;

            mapping.DisplayOrder = queueId;
            await _productTagMappingRepository.UpdateAsync(mapping);
        }

        #endregion

        #region Methods

        /// <summary>
        /// Recalculate Bestseller tags across all categories
        /// Tag name and thresholds read from TagAutomationSettings
        ///
        /// Logic:
        /// 1. Build new eligible product list per category
        /// 2. Remove tag ONLY from products that no longer qualify
        /// 3. Add tag to newly qualifying products
        /// </summary>
        public async Task ProcessTagsAsync()
        {
            int[] skipProductIds = string.IsNullOrWhiteSpace(_tagAutomationSettings.SkipProductIds)
                ? Array.Empty<int>()
                : _tagAutomationSettings.SkipProductIds
                    .Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(id => int.TryParse(id.Trim(), out var parsed) ? parsed : 0)
                    .Where(id => id > 0)
                    .ToArray();
            // Read from settings
            var bestsellerTagName = _tagAutomationSettings.BestsellerTagName;
            var bestsellerDays = _tagAutomationSettings.BestsellerDays;
            var bestsellerMinOrders = _tagAutomationSettings.BestsellerMinOrders;
            var maxBestsellerPerCategory = _tagAutomationSettings.MaxBestsellerPerCategory;

            var bestsellerTag = await GetTagByNameAsync(bestsellerTagName);
            if (bestsellerTag == null)
                return;

            // Get current bestseller products via pagination
            var currentBestsellerProducts = new List<Product>();
            var pageNumber = 0;
            var haveProducts = true;

            do
            {
                var prds = await _productService.SearchProductsAsync(
                    visibleIndividuallyOnly: true,
                    overridePublished: true,
                    productTagId: bestsellerTag.Id,
                    pageIndex: pageNumber,
                    pageSize: 50);

                currentBestsellerProducts.AddRange(prds);

                if (prds.Count / 50 != 1 || prds.Count == 0)
                    haveProducts = false;

                pageNumber++;
            } while (haveProducts);

            // Build new eligible product IDs across all categories
            var newEligibleIds = new HashSet<int>();
            var categories = await _categoryService.GetAllCategoriesAsync();

            foreach (var category in categories)
            {
                var productCategories = await _categoryService
                    .GetProductCategoriesByCategoryIdAsync(category.Id);

                var productPerformance = new List<(int ProductId, int OrderCount, decimal Price)>();

                foreach (var pc in productCategories)
                {
                    var product = await _productService.GetProductByIdAsync(pc.ProductId);
                    if (product == null || product.Deleted || !product.Published || skipProductIds.Contains(product.Id))
                        continue;

                    // Only primary category products
                    var primaryCategoryId = await GetPrimaryCategoryIdAsync(pc.ProductId);
                    if (primaryCategoryId != category.Id)
                        continue;

                    var orderCount = await GetProductOrderCountAsync(pc.ProductId, bestsellerDays);

                    if (orderCount >= bestsellerMinOrders)
                        productPerformance.Add((pc.ProductId, orderCount, product.Price));
                }

                // Rank by order count then price — take max per category from settings
                var rankedProducts = productPerformance
                    .OrderByDescending(p => p.OrderCount)
                    .ThenByDescending(p => p.Price)
                    .Take(maxBestsellerPerCategory)
                    .Select(p => p.ProductId)
                    .ToList();

                foreach (var productId in rankedProducts)
                    newEligibleIds.Add(productId);
            }

            // Remove tag ONLY from products no longer eligible
            var toRemove = currentBestsellerProducts
                .Select(b => b.Id)
                .Except(newEligibleIds)
                .ToList();

            foreach (var productId in toRemove)
            {
                var product = await _productService.GetProductByIdAsync(productId);
                if (product == null || product.Deleted)
                    continue;

                await RemoveTagAsync(product, bestsellerTagName);
            }

            // Add tag ONLY to newly eligible products
            var toAdd = newEligibleIds
                .Except(currentBestsellerProducts.Select(b => b.Id))
                .ToList();

            foreach (var productId in toAdd)
            {
                var product = await _productService.GetProductByIdAsync(productId);
                if (product == null)
                    continue;

                await AddTagAsync(product, bestsellerTagName);
            }
        }

        /// <summary>
        /// Recalculate DisplayOrder for all Bestseller tagged products
        /// Most orders = DisplayOrder 1 (top)
        /// Tie broken by higher price
        /// Dual tag products (Bestseller + New Arrival) always at top
        /// Tag names read from TagAutomationSettings
        /// </summary>
        public async Task RecalculateQueueAsync()
        {
            // Read from settings
            var bestsellerTagName = _tagAutomationSettings.BestsellerTagName;
            var newArrivalTagName = _tagAutomationSettings.NewArrivalTagName;
            var bestsellerDays = _tagAutomationSettings.BestsellerDays;

            var bestsellerTag = await GetTagByNameAsync(bestsellerTagName);
            if (bestsellerTag == null)
                return;

            // Paginate through all bestseller products
            var products = new List<Product>();
            var pageNumber = 0;

            while (true)
            {
                var page = await _productService.SearchProductsAsync(
                    pageIndex: pageNumber,
                    pageSize: 50,
                    visibleIndividuallyOnly: true,
                    overridePublished: true,
                    productTagId: bestsellerTag.Id);

                products.AddRange(page);

                if (!page.HasNextPage)
                    break;

                pageNumber++;
            }

            // Build ranked list with metadata
            var productList = new List<(Product Product, int OrderCount, decimal Price, bool IsDualTag)>();

            foreach (var product in products)
            {
                if (product == null || product.Deleted || !product.Published)
                    continue;

                var orderCount = await GetProductOrderCountAsync(product.Id, bestsellerDays);
                var isNewArrival = await ProductHasTagAsync(product.Id, newArrivalTagName);

                productList.Add((product, orderCount, product.Price, isNewArrival));
            }

            // Rank:
            // 1. Dual tag products first (Bestseller + New Arrival)
            // 2. Highest order count
            // 3. Higher price (tie breaker)
            var ranked = productList
                .OrderByDescending(p => p.IsDualTag)
                .ThenByDescending(p => p.OrderCount)
                .ThenByDescending(p => p.Price)
                .ToList();

            for (var i = 0; i < ranked.Count; i++)
            {
                await UpdateQueueIdAsync(ranked[i].Product.Id, bestsellerTag.Id, i + 1);
                ranked[i].Product.BestSellerRank = i + 1;
                await _productService.UpdateProductWithoutEvent(ranked[i].Product);
            }
        }

        #endregion
    }
}
