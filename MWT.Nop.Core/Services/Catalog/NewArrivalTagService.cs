using MWT.Nop.Core.Services.Configuration;
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
    public partial class NewArrivalTagService : INewArrivalTagService
    {
        #region Fields

        private readonly IProductService _productService;
        private readonly IProductTagService _productTagService;
        private readonly IRepository<ProductProductTagMapping> _productTagMappingRepository;
        private readonly TagAutomationSettings _tagAutomationSettings;

        #endregion

        #region Ctor

        public NewArrivalTagService(
            IProductService productService,
            IProductTagService productTagService,
            IRepository<ProductProductTagMapping> productTagMappingRepository,
            TagAutomationSettings tagAutomationSettings)
        {
            _productService = productService;
            _productTagService = productTagService;
            _productTagMappingRepository = productTagMappingRepository;
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
        /// Assign New Arrival tag to products within configured days
        /// Remove from products older than configured days
        /// Tag name read from TagAutomationSettings.NewArrivalTagName
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
            // Read tag name and threshold from settings
            var newArrivalTagName = _tagAutomationSettings.NewArrivalTagName;
            var newArrivalDays = _tagAutomationSettings.NewArrivalDays;
            var cutoffDate = DateTime.UtcNow.AddDays(-newArrivalDays);

            // Paginate through all published products
            var products = new List<Product>();
            var pageNumber = 0;
            var haveProducts = true;

            do
            {
                var prds = await _productService.SearchProductsAsync(
                    visibleIndividuallyOnly: true,
                    overridePublished: true,
                    pageIndex: pageNumber,
                    pageSize: 50);

                products.AddRange(prds);

                if (prds.Count / 50 != 1 || prds.Count == 0)
                    haveProducts = false;

                pageNumber++;
            } while (haveProducts);

            foreach (var product in products)
            {
                if (skipProductIds.Contains(product.Id))
                    continue;
                var isWithinDays = product.CreatedOnUtc >= cutoffDate;
                var hasTag = await ProductHasTagAsync(product.Id, newArrivalTagName);

                if (isWithinDays && !hasTag)
                    await AddTagAsync(product, newArrivalTagName);
                else if (!isWithinDays && hasTag)
                    await RemoveTagAsync(product, newArrivalTagName);
            }
        }

        /// <summary>
        /// Recalculate DisplayOrder for all New Arrival products
        /// Latest launched = DisplayOrder 1 (top)
        /// Dual tag products (New Arrival + Bestseller) always at top
        /// Tag names read from TagAutomationSettings
        /// </summary>
        public async Task RecalculateQueueAsync()
        {
            // Read tag names from settings
            var newArrivalTagName = _tagAutomationSettings.NewArrivalTagName;
            var bestsellerTagName = _tagAutomationSettings.BestsellerTagName;

            var newArrivalTag = await GetTagByNameAsync(newArrivalTagName);
            if (newArrivalTag == null)
                return;

            // Paginate through all New Arrival tagged products
            var products = new List<Product>();
            var pageNumber = 0;
            var haveProducts = true;

            do
            {
                var prds = await _productService.SearchProductsAsync(
                    visibleIndividuallyOnly: true,
                    overridePublished: true,
                    productTagId: newArrivalTag.Id,
                    pageIndex: pageNumber,
                    pageSize: 50);

                products.AddRange(prds);

                if (prds.Count / 50 != 1 || prds.Count == 0)
                    haveProducts = false;

                pageNumber++;
            } while (haveProducts);

            // Build ranked list with dual tag check
            var productList = new List<(int ProductId, DateTime CreatedOnUtc, bool IsDualTag)>();

            foreach (var product in products)
            {
                if (product == null || product.Deleted || !product.Published)
                    continue;

                // Dual tag = also has Bestseller tag → goes to top
                var isBestseller = await ProductHasTagAsync(product.Id, bestsellerTagName);
                productList.Add((product.Id, product.CreatedOnUtc, isBestseller));
            }

            // Rank:
            // 1. Dual tag products first
            // 2. Latest CreatedOnUtc first
            var ranked = productList
                .OrderByDescending(p => p.IsDualTag)
                .ThenByDescending(p => p.CreatedOnUtc)
                .ToList();

            for (var i = 0; i < ranked.Count; i++)
                await UpdateQueueIdAsync(ranked[i].ProductId, newArrivalTag.Id, i + 1);
        }

        #endregion
    }
}
