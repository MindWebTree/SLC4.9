using MWT.Nop.Core.Service.Catalog;
using MWT.Nop.Core.Services.Catalog;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Logging;
using Nop.Services.Catalog;
using Nop.Services.Configuration;
using Nop.Services.Logging;
using Nop.Services.ScheduleTasks;

namespace MWT.Plugin.Misc.MwtStorefront.Tasks
{
    public class PriceFilterVariantPriceRangeTaskIScheduleTask : IScheduleTask
    {
        #region fields

        private readonly IProductExtendedService _productService;
        private readonly ICustomSpecificationAttributeService _specificationAttributeService;
        private readonly ISettingService _settingService;
        private readonly ILogger _logger;
        IList<SpecificationAttributeOption> _options;
        int specificationAttributeId = 0;
        #endregion

        #region Ctor

        public PriceFilterVariantPriceRangeTaskIScheduleTask(IProductExtendedService productService,
            ICustomSpecificationAttributeService specificationAttributeService, ISettingService settingService,
            ILogger logger)
        {
            this._productService = productService;
            this._specificationAttributeService = specificationAttributeService; 
            this._settingService = settingService;
            this._logger = logger;
            this._options = new List<SpecificationAttributeOption>();
        }

        #endregion
        public async System.Threading.Tasks.Task ExecuteAsync()
        {
            await _logger.InsertLogAsync(LogLevel.Information, "Price Filter/VariantPrice Range task started", null);
            specificationAttributeId = await _settingService.GetSettingByKeyAsync<int>("Price Filter AttributeId");
            if (specificationAttributeId != 0)
                _options = await _specificationAttributeService.GetSpecificationAttributeOptionsBySpecificationAttributeAsync(specificationAttributeId);
            if (_options.Count == 0)
                await _logger.InsertLogAsync(LogLevel.Information, "Failed to get Price Filters", null);

            await ProcessProducts();
            await _logger.InsertLogAsync(LogLevel.Information, "Price Filter/VariantPrice Range task Completed", null);
        }


        #region ProcessProducts
        private async System.Threading.Tasks.Task<bool> ProcessProducts()
        {
            int pageIndex = 0;
            int pageSize = 50;
            bool hasNextPage = false;
            do
            {
                var result = await this._productService.SearchProductsAsync(pageIndex: pageIndex, pageSize: 
                    pageSize, showHidden: true);
                hasNextPage = result.HasNextPage;

                foreach (var product in result)
                {
                    (bool isVariantPrice, decimal minOldPrice, decimal maxOldPrice, decimal minPrice, decimal maxPrice, decimal minMsrp, decimal maxMsrp) = await ProcessProduct(product);
                    if (_options.Count > 0)
                        await PriceFilterMapping(product.Id, isVariantPrice ? minOldPrice : product.OldPrice, isVariantPrice ? maxOldPrice : product.OldPrice);

                }
                pageIndex++;
            } while (hasNextPage);

            return true;
        }
        private async System.Threading.Tasks.Task<(bool, decimal, decimal, decimal, decimal, decimal, decimal)> ProcessProduct(Product product)
        {
            return await _productService.GetVariantPriceRange(product);
        }

        private async System.Threading.Tasks.Task PriceFilterMapping(int productId, decimal minPrice, decimal maxPrice)
        {
            List<ProductSpecificationAttribute> prdAttributes = await this._specificationAttributeService.GetProductSpecificationAttributesByAttributeIdAsync(productId, specificationAttributeId);

            var elgibleOptions = await GetEligiblePriceFilters(minPrice, maxPrice);

            #region Delete NotElgible Filters

            var notEelgiblePrdAttrs = prdAttributes.Where(a => !elgibleOptions.Contains(a.SpecificationAttributeOptionId));
            foreach (var attr in notEelgiblePrdAttrs)
            {
                await this._specificationAttributeService.DeleteProductSpecificationAttributeAsync(attr);
            }


            #endregion

            #region Map Attributes

            var attrNeedToMap = elgibleOptions.Where(e => !prdAttributes.Select(a => a.SpecificationAttributeOptionId).Contains(e));
            foreach(var attr in attrNeedToMap)
            {
                await _specificationAttributeService.InsertProductSpecificationAttributeAsync(new ProductSpecificationAttribute()
                {
                    ProductId = productId,
                    AttributeTypeId = 0,
                    SpecificationAttributeOptionId = attr,
                    CustomValue = "",
                    AllowFiltering = true,
                    ShowOnProductPage = false,
                    DisplayOrder = 1
                }); 
            }

            #endregion
        }

        private async System.Threading.Tasks.Task<List<int>> GetEligiblePriceFilters(decimal minPrice, decimal maxPrice)
        {
            List<int> optionIds = new List<int>();
            foreach (var option in _options)
            {
                if (minPrice > option.LowerLimit && minPrice <= option.HigherLimit)
                    optionIds.Add(option.Id);
                else if (maxPrice > option.LowerLimit && maxPrice <= option.HigherLimit)
                    optionIds.Add(option.Id);
                else if (minPrice <= option.LowerLimit && maxPrice > option.HigherLimit)
                    optionIds.Add(option.Id);

            }
            return optionIds;
        }
        #endregion
    }
}
