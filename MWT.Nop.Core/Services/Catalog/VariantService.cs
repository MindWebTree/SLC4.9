using MWT.Nop.Core.Domain.Catalog;
using MWT.Nop.Core.Service.Catalog;
using MWT.Nop.Core.Services.Seo;
using MWTNop.Core.Domain.Catalog;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Customization.Catalog;
using Nop.Core.Domain.Logging;
using Nop.Core.Infrastructure;
using Nop.Data;
using Nop.Services.Catalog;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Orders;
using Nop.Services.Seo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MWT.Nop.Core.Services.Catalog
{
    public partial class VariantService : IVariantService
    {
        protected readonly IStaticCacheManager _staticCacheManager;
        protected readonly IProductAttributeService _productAttributeService;
        protected readonly ILocalizationService _localizationService;
        protected readonly ICustomProductAttributeParser _customProductAttributeParser;
        protected readonly ICustomProductService _customproductService;
        private static readonly SemaphoreSlim _semaphoreProductVariantIdGenerate = new SemaphoreSlim(1, 1);
        protected readonly IRepository<Product> _productRepository;
        protected readonly ICustomUrlRecordService _urlRecordService;
        public VariantService(IStaticCacheManager staticCacheManager, IProductAttributeService productAttributeService
            , ILocalizationService localizationService, ICustomProductAttributeParser customProductAttributeParser,
             ICustomProductService customproductService, IRepository<Product> productRepository, ICustomUrlRecordService urlRecordService)
        {
            _staticCacheManager = staticCacheManager;
            _productAttributeService = productAttributeService;
            _localizationService = localizationService;
            _customProductAttributeParser = customProductAttributeParser;
            _customproductService = customproductService;
            _productRepository = productRepository;
            _urlRecordService = urlRecordService;
        }


        public async Task<int> GetProductVariantId(int productId)
        {
            var variantId = 0;
            try
            {
                var isVariantExist = false;
                var productAttributeMapping = await _productAttributeService.GetProductAttributeMappingsByProductIdAsync(productId);
                foreach (var attribute in productAttributeMapping)
                {
                    var productAttrubute = await _productAttributeService.GetProductAttributeByIdAsync(attribute.ProductAttributeId);

                    if (attribute.ShouldHaveValues())
                    {
                        //values
                        var attributeValues = await _productAttributeService.GetProductAttributeValuesAsync(attribute.Id);
                        if (productAttrubute.Name.Equals(await _localizationService.GetResourceAsync("Product.Attr.Size"), StringComparison.InvariantCultureIgnoreCase))
                        {
                            variantId = attributeValues.Where(v => v.IsPreSelected).FirstOrDefault()?.VariantId ?? 0;
                            if (variantId == 0)
                                variantId = attributeValues.FirstOrDefault()?.VariantId ?? 0;
                        }
                    }
                }
            }
            catch (Exception exp)
            {

            }
            return variantId;
        }

        public async Task<List<VariantCombination>> GetPublishedProductVariants(int productId)
        {
            var _variantcombination = EngineContext.Current.Resolve<IRepository<VariantCombination>>();
            return await _staticCacheManager.GetAsync(_staticCacheManager.PrepareKeyForDefaultCache(CustomNopCatalogDefaults.ProductVariantsPublishedCacheKey, productId), async () =>
            {

                var variantCombinations = await _variantcombination.Table.Where(vc => vc.ProductId == productId).ToListAsync();
                var variants = new List<VariantCombination>();
                foreach (var _variant in variantCombinations)
                {
                    bool published = true;
                    foreach (var attrValueId in (_variant.ProductAttributeValueIds ?? string.Empty).Split('-'))
                    {
                        int.TryParse(attrValueId, out int _attrValueId);
                        var productAttributeValue = await _productAttributeService.GetProductAttributeValueByIdAsync(_attrValueId);
                        if (productAttributeValue == null || !productAttributeValue.Published)
                        {
                            published = false;
                            break;
                        }

                    }
                    if (published)
                        variants.Add(_variant);

                }
                return variants;
            });
        }
        public async Task<List<VariantCombination>> GetProductVariants(int productId)
        {
            var _variantcombination = EngineContext.Current.Resolve<IRepository<VariantCombination>>();
            return await _staticCacheManager.GetAsync(_staticCacheManager.PrepareKeyForDefaultCache(CustomNopCatalogDefaults.ProductVariantsCacheKey, productId), async () => await _variantcombination.Table.Where(vc => vc.ProductId == productId).ToListAsync());
        }
        public async Task<int> GetVariantId(int productId, string attributes)
        {
            int variantId = await this.GetVariantIdFromAttributeDescription(productId, attributes);
            if (variantId == 0)
            {
                string size = "";

                if (!string.IsNullOrEmpty(attributes))
                {
                    string[] attrs = attributes.Split("<br /", StringSplitOptions.RemoveEmptyEntries);
                    foreach (var attr in attrs)
                    {
                        if (attr.Split(':').Length > 0)
                        {
                            if (attr.Split(':')[0].Equals("size", StringComparison.InvariantCultureIgnoreCase))
                            {
                                size = string.Join(':', attr.Split(":").Skip(1));
                                break;
                            }
                        }
                    }
                }
                variantId = await this.GetProductVariantIdBySize(productId, System.Net.WebUtility.HtmlDecode(size));
            }
            return variantId;
        }
        public async Task<int> GetProductVariantIdBySize(int productId, string size)
        {
            int defaultVariantId = 0;
            int variantId = 0;
            var productAttributeMapping = await _productAttributeService.GetProductAttributeMappingsByProductIdAsync(productId);
            foreach (var attribute in productAttributeMapping)
            {
                var productAttrubute = await _productAttributeService.GetProductAttributeByIdAsync(attribute.ProductAttributeId);

                if (attribute.ShouldHaveValues())
                {
                    //values
                    var attributeValues = await _productAttributeService.GetProductAttributeValuesAsync(attribute.Id);
                    if (productAttrubute.Name.Equals(await _localizationService.GetResourceAsync("Product.Attr.Size"), StringComparison.InvariantCultureIgnoreCase))
                    {
                        defaultVariantId = attributeValues.Where(v => v.IsPreSelected).FirstOrDefault()?.VariantId ?? 0;
                        if (defaultVariantId == 0)
                            defaultVariantId = attributeValues.FirstOrDefault()?.VariantId ?? 0;
                        foreach (var value in attributeValues)
                        {
                            if (value.Name.Trim().Equals(size.Trim(), StringComparison.InvariantCultureIgnoreCase))
                            {
                                variantId = value.VariantId;
                                if (variantId > 0)
                                    break;
                            }
                        }
                    }
                }
            }
            return variantId > 0 ? variantId : defaultVariantId;
        }
        public async Task<int> GenerateVariantIdAsync(ProductAttributeMapping mapping,
int productAttributeValueId, string productAttributeValue)
        {
            var attribute = await _productAttributeService.GetProductAttributeByIdAsync(mapping.ProductAttributeId);
            ProductVariant productVariant = new ProductVariant();
            if (!string.Equals(attribute.Name, await _localizationService.GetResourceAsync("Product.Attr.Stain"), StringComparison.InvariantCultureIgnoreCase))
            {
                productVariant = await this.GetProductVariantByAttributeValueIdAsync(productAttributeValueId);
                if (productVariant == null)
                {
                    productVariant = new ProductVariant();
                    productVariant = await GetProductVariantByAttributeValueAsync(mapping.ProductId, mapping.ProductAttributeId, productAttributeValue);
                    if (productVariant == null)
                    {
                        productVariant = new ProductVariant();
                        productVariant.ProductId = mapping.ProductId;
                        productVariant.ProductAttributeValueId = productAttributeValueId;
                        productVariant.ProductAttributeId = mapping.ProductAttributeId;
                        productVariant.AttributeValue = productAttributeValue;
                        productVariant.ProductAttributeValueIds = string.Empty;
                        await this.InsertProductVariantAsync(productVariant);
                    }
                    else
                    {
                        productVariant.ProductAttributeValueId = productAttributeValueId;
                        productVariant.ProductAttributeValueIds = string.Empty;
                        await this.UpdateProductVariantAsync(productVariant);
                    }

                }
                else
                {
                    if (!string.Equals(productAttributeValue, productVariant.AttributeValue))
                    {
                        productVariant.AttributeValue = productAttributeValue;
                        await this.UpdateProductVariantAsync(productVariant);
                    }
                }

                await GenerateVariantCombinations(mapping.ProductId);
            }
            return productVariant?.VariantId ?? 0;
        }
        public async Task<bool> GenerateVariantCombinations(int productId)
        {
            var productAttributesCombinations = await _productAttributeService.GetAllProductAttributeCombinationsAsync(productId);

            Dictionary<int, List<int>> dicAttrCombinations = new Dictionary<int, List<int>>();

            foreach (var _attrCombination in productAttributesCombinations)
            {
                dicAttrCombinations.Add(_attrCombination.Id, (await _customProductAttributeParser.ParseProductAttributeValuesAsync(_attrCombination.AttributesXml)).Select(x => x.Id).ToList());
            }
            var existingCombinations = await this.GetProductVariants(productId);
            var exisitngProductVariant = await this.GetAllProductVariantByProductIdAsync(productId);
            var product = await _customproductService.GetProductByIdAsync(productId);

            Dictionary<string, List<int>> combinations =
                await _customProductAttributeParser.CustomGenerateAllCombinationsAsync(product);

            List<int> productAttributeValueIds = combinations.Values.Where(combination => combination.Any()).Select(combination => combination.First()).Distinct().ToList();

            List<VariantCombination> lstCombinations = new List<VariantCombination>();
            foreach (var productAttributeValueId in productAttributeValueIds)
            {
                var _combinations = combinations.Where(kv => kv.Value.Contains(productAttributeValueId)).ToDictionary(kv => kv.Key, kv => kv.Value);
                var productAttributeValue = await _productAttributeService.GetProductAttributeValueByIdAsync(productAttributeValueId);
                foreach (var _combination in _combinations)
                {
                    lstCombinations.Add(new VariantCombination()
                    {
                        Combination = _combination.Key,
                        OldPrice = 0,
                        Price = 0,
                        ProductId = productId,
                        ProductAttributeValueIds = string.Join("-", _combination.Value),
                        Title = productAttributeValue?.VariantTitle ?? string.Empty
                    });
                    foreach (var attributeValueId in _combination.Value)
                    {
                        var productVariant = await GetProductVariantByAttributeValueIdAsync(attributeValueId);
                        if (productVariant == null)
                        {
                            var attributeValue = await _productAttributeService.GetProductAttributeValueByIdAsync(Convert.ToInt32(attributeValueId));
                            var productmapping = await _productAttributeService.GetProductAttributeMappingByIdAsync(attributeValue.ProductAttributeMappingId);
                            productVariant = new ProductVariant();
                            productVariant.AttributeValue = attributeValue.Name;
                            productVariant.ProductAttributeId = productmapping.ProductAttributeId;
                            productVariant.ProductAttributeValueId = Convert.ToInt32(attributeValueId);
                            productVariant.ProductId = productId;
                            productVariant.ProductAttributeValueIds = string.Empty;
                            await InsertProductVariantAsync(productVariant);
                            exisitngProductVariant.Add(productVariant);
                        }
                    }

                }
            }

            #region Delete all Combination that's no longer exist
            List<VariantCombination> itemsToRemove = new List<VariantCombination>();
            foreach (var existingCombination in existingCombinations)
            {
                var combination = GetVariantCombinationByProductAttributeValueIds(lstCombinations, existingCombination.ProductAttributeValueIds);
                if (combination == null)
                {
                    await DeleteVariantCombinationAsync(existingCombination);

                    // nsert Log
                    VariantCombinationLog variantcombinationlog = new VariantCombinationLog();
                    variantcombinationlog.DeletedOn = DateTime.UtcNow;
                    variantcombinationlog.IsDeleted = true;
                    variantcombinationlog.Combination = existingCombination.Combination;
                    variantcombinationlog.VariantId = existingCombination.VariantId;
                    variantcombinationlog.UpdatedOn = DateTime.UtcNow;
                    variantcombinationlog.Price = existingCombination.Price;
                    variantcombinationlog.OldPrice = existingCombination.OldPrice;
                    variantcombinationlog.Msrp = existingCombination.Msrp;
                    variantcombinationlog.ProductId = existingCombination.ProductId;
                    variantcombinationlog.ProductAttributeValueIds = existingCombination.ProductAttributeValueIds;
                    await InsertVariantCombinationLogAsync(variantcombinationlog);
                    itemsToRemove.Add(existingCombination);
                }
            }
            existingCombinations.RemoveAll(itemsToRemove.Contains);
            #endregion


            #region VariantId Assign
            foreach (var combination in lstCombinations)
            {
                var productVariant = GetVariantByProductAttributeValueIds(exisitngProductVariant, combination.ProductAttributeValueIds);
                if (productVariant != null)
                {

                    combination.VariantId = productVariant.VariantId;
                    var existcombination = GetVariantCombinationByProductAttributeValueIds(existingCombinations, combination.ProductAttributeValueIds);
                    if (existcombination != null)
                    {
                        if (combination.VariantId != productVariant.VariantId)
                        {
                            combination.Id = existcombination.Id;
                            combination.Price = existcombination.Price;
                            combination.OldPrice = existcombination.OldPrice;
                            combination.Msrp = existcombination.Msrp;
                            combination.Title = existcombination.Title;
                            await UpdateVariantCombinationAsync(combination);

                            // Log nsert 
                            VariantCombinationLog variantcombinationlog = new VariantCombinationLog();
                            variantcombinationlog.CreatedOn = DateTime.UtcNow;
                            variantcombinationlog.IsDeleted = false;
                            variantcombinationlog.Combination = combination.Combination;
                            variantcombinationlog.VariantId = existcombination.VariantId;
                            variantcombinationlog.UpdatedOn = DateTime.UtcNow;
                            variantcombinationlog.Price = combination.Price;
                            variantcombinationlog.OldPrice = combination.OldPrice;
                            variantcombinationlog.Msrp = combination.Msrp;
                            variantcombinationlog.ProductId = combination.ProductId;
                            variantcombinationlog.ProductAttributeValueIds = existcombination.ProductAttributeValueIds;
                            await InsertVariantCombinationLogAsync(variantcombinationlog);
                        }
                    }
                    else
                    {
                        combination.CreatedOn = DateTime.UtcNow;
                        combination.UpdatedOn = DateTime.UtcNow;
                        AssignPriceToProductAttributeCombination(combination, productAttributesCombinations, dicAttrCombinations, product);
                        await InsertVariantCombinationAsync(combination);
                        existingCombinations.Add(combination);
                    }
                }
                else
                {

                    var valueIdsSet = new HashSet<string>((combination.ProductAttributeValueIds ?? string.Empty).Split('-'));
                    var attributeValue = await _productAttributeService.GetProductAttributeValueByIdAsync(Convert.ToInt32(valueIdsSet.First()));
                    var productmapping = await _productAttributeService.GetProductAttributeMappingByIdAsync(attributeValue.ProductAttributeMappingId);
                    if (valueIdsSet.Count == 1)
                    {
                        productVariant = new ProductVariant();
                        productVariant.AttributeValue = attributeValue.Name;
                        productVariant.ProductAttributeId = productmapping.ProductAttributeId;
                        productVariant.ProductAttributeValueId = Convert.ToInt32(valueIdsSet.First());
                        productVariant.ProductId = productId;
                        productVariant.ProductAttributeValueIds = string.Empty;
                        await InsertProductVariantAsync(productVariant);
                        exisitngProductVariant.Add(productVariant);
                        combination.CreatedOn = DateTime.UtcNow;
                        combination.UpdatedOn = DateTime.UtcNow;
                        combination.VariantId = productVariant.VariantId;
                        AssignPriceToProductAttributeCombination(combination, productAttributesCombinations, dicAttrCombinations, product);
                        await InsertVariantCombinationAsync(combination);
                        existingCombinations.Add(combination);
                    }
                    else
                    {
                        productVariant = await GetProductVariantByAttributeValueIdAsync(Convert.ToInt32(valueIdsSet.First()));
                        if (productVariant == null)
                        {
                            productVariant = new ProductVariant();
                            productVariant.AttributeValue = attributeValue.Name;
                            productVariant.ProductAttributeId = productmapping.ProductAttributeId;
                            productVariant.ProductAttributeValueId = Convert.ToInt32(valueIdsSet.First());
                            productVariant.ProductId = productId;
                            productVariant.ProductAttributeValueIds = string.Empty;
                            await InsertProductVariantAsync(productVariant);
                            exisitngProductVariant.Add(productVariant);
                        }

                        bool isVariantIdAssigned = existingCombinations.Where(V => V.VariantId == productVariant.VariantId && V.ProductAttributeValueIds != combination.ProductAttributeValueIds).Any();

                        if (isVariantIdAssigned)
                        {
                            productVariant = new ProductVariant();
                            productVariant.AttributeValue = string.Empty;
                            productVariant.ProductAttributeId = 0;
                            productVariant.ProductAttributeValueId = 0;
                            productVariant.ProductId = productId;
                            productVariant.ProductAttributeValueIds = combination.ProductAttributeValueIds;
                            await InsertProductVariantAsync(productVariant);
                            exisitngProductVariant.Add(productVariant);

                            combination.VariantId = productVariant.VariantId;
                            combination.CreatedOn = DateTime.UtcNow;
                            combination.UpdatedOn = DateTime.UtcNow;
                            AssignPriceToProductAttributeCombination(combination, productAttributesCombinations, dicAttrCombinations, product);
                            await InsertVariantCombinationAsync(combination);
                            existingCombinations.Add(combination);
                        }
                        else
                        {
                            var existcombination = GetVariantCombinationByProductAttributeValueIds(existingCombinations, combination.ProductAttributeValueIds);
                            if (existcombination == null)
                            {

                                combination.VariantId = productVariant.VariantId;
                                combination.CreatedOn = DateTime.UtcNow;
                                combination.UpdatedOn = DateTime.UtcNow;
                                AssignPriceToProductAttributeCombination(combination, productAttributesCombinations, dicAttrCombinations, product);
                                await InsertVariantCombinationAsync(combination);
                                existingCombinations.Add(combination);
                            }
                        }


                    }
                }
            }
            #endregion

            return true;
        }


        protected async Task<ProductVariant> GetProductVariantByAttributeValueIdAsync(int attributevalueid)
        {
            var _productVariantRepository = EngineContext.Current.Resolve<IRepository<ProductVariant>>();
            return await _productVariantRepository.Table.Where(x => x.ProductAttributeValueId == attributevalueid).FirstOrDefaultAsync();
        }
        protected void AssignPriceToProductAttributeCombination(VariantCombination variantCombination, IList<ProductAttributeCombination> combinations, Dictionary<int, List<int>> dicAttrCombinations, Product product)
        {
            if (string.IsNullOrWhiteSpace(variantCombination.ProductAttributeValueIds))
                return;

            var attrValueIds = new HashSet<int>(
                (variantCombination.ProductAttributeValueIds ?? string.Empty)
                    .Split('-')
                    .Select(s => int.TryParse(s, out int value) ? value : (int?)null)
                    .Where(v => v.HasValue)
                    .Select(v => v.Value)
            );
            foreach (var dicAttCombination in dicAttrCombinations)
            {
                if (attrValueIds.IsSubsetOf(new HashSet<int>(dicAttCombination.Value)))
                {

                    var combination = combinations.Where(c => c.Id == dicAttCombination.Key).First();
                    variantCombination.OldPrice = (decimal)(combination?.OverriddenOldPrice ?? product.OldPrice);
                    variantCombination.Msrp = (decimal)(combination?.OverriddenMsrp ?? product.Msrp);
                    variantCombination.Price = (decimal)(combination?.OverriddenPrice ?? product.Price);
                    break;
                }
            }
            if (variantCombination.Price == 0)
            {
                variantCombination.Price = product.Price;
                variantCombination.OldPrice = product.OldPrice;
                variantCombination.Msrp = product.Msrp;
            }

        }
        public async Task<VariantCombination> GetVariantById(int id)
        {
            var _variantcombination = EngineContext.Current.Resolve<IRepository<VariantCombination>>();
            return await _variantcombination.GetByIdAsync(id);
        }
        public async Task UpdateVariant(VariantCombination variant, bool updateCombinations = true)
        {
            if (updateCombinations)
            {
                await UpdateAttributeCombinationPrices(variant);
            }
            await UpdateVariantCombinationAsync(variant);
        }
        protected async Task UpdateAttributeCombinationPrices(VariantCombination variant)
        {
            if (string.IsNullOrWhiteSpace(variant.ProductAttributeValueIds))
                return;

            var attrValueIds = new HashSet<int>(
                (variant.ProductAttributeValueIds ?? string.Empty)
                    .Split('-')
                    .Select(s => int.TryParse(s, out int value) ? value : (int?)null)
                    .Where(v => v.HasValue)
                    .Select(v => v.Value)
            );

            var productAttributesCombinations = await _productAttributeService.GetAllProductAttributeCombinationsAsync(variant.ProductId);

            foreach (var _attrCombination in productAttributesCombinations)
            {
                var combinationAttributeValueIds = (await _customProductAttributeParser.ParseProductAttributeValuesAsync(_attrCombination.AttributesXml)).Select(x => x.Id).ToList();
                if (new HashSet<int>(attrValueIds).IsSubsetOf(combinationAttributeValueIds))
                {
                    _attrCombination.OverriddenOldPrice = variant.OldPrice;
                    _attrCombination.OverriddenPrice = variant.Price;
                    _attrCombination.OverriddenMsrp = variant.Msrp;
                    await _productAttributeService.UpdateProductAttributeCombinationAsync(_attrCombination);
                }
            }

            var product = await _customproductService.GetProductByIdAsync(variant.ProductId);
            await _customproductService.UpdateProductAsync(product);
        }
        public async Task<VariantCombination> GetVariantByVariantId(int variantId)
        {
            var _variantcombination = EngineContext.Current.Resolve<IRepository<VariantCombination>>();
            return await _staticCacheManager.GetAsync(_staticCacheManager.PrepareKeyForDefaultCache(CustomNopCatalogDefaults.ProductVariantByVariantIdCacheKey, variantId),
                async () => await _variantcombination.Table.Where(vc => vc.VariantId == variantId).FirstOrDefaultAsync());

        }
        public async Task<(bool, decimal, decimal, decimal, decimal, decimal, decimal)> GetVariantPriceRange(Product product, bool createCombination = true, bool updateProduct = false, bool updateProductWithEvent = true, bool isVariantTask = true)
        {
            bool isVariantPrice = false;
            decimal minOldPrice = 0;
            decimal maxOldPrice = 0;
            decimal minPrice = 0;
            decimal maxPrice = 0;
            decimal minMsrp = 0;
            decimal maxMsrp = 0;
            var _loggerService = EngineContext.Current.Resolve<ILogger>();
            if (product.ProductType != ProductType.GroupedProduct)
            {
                var productAttributeMapping = await _productAttributeService.GetProductAttributeMappingsByProductIdAsync(product.Id);
                var _shoppingCartService = EngineContext.Current.Resolve<IShoppingCartService>();
                if (productAttributeMapping.Count() > 0)
                {
                    isVariantPrice = true;
                    #region Genrate Combination 
                    var combinations = await _productAttributeService.GetAllProductAttributeCombinationsAsync(product.Id);

                    decimal oldPrice = 0;
                    decimal price = 0;
                    decimal msrp = 0;
                    decimal defaulCombinationOldPrice = product.OldPrice;
                    decimal defaultCombinationPrice = product.Price;
                    decimal defaultCombinationMsrp = product.Msrp;
                    string defaultCombinationAttrXml = await _customproductService.GetDefaultCombinationAttrXml(productAttributeMapping);
                    foreach (var combination in combinations)
                    {
                        var attributes = await _customProductAttributeParser.ParseProductAttributeMappingsAsync(combination.AttributesXml);
                        if (!attributes.Any())
                            continue;

                        bool isAttributeValid = true;
                        foreach (var attribute in attributes)
                        {
                            if (isAttributeValid)
                            {
                                if (!attribute.ShouldHaveValues())
                                {
                                    isAttributeValid = false;
                                    break;
                                }

                                foreach (var attributeValue in _customProductAttributeParser.CustomParseValuesWithQuantity(combination.AttributesXml, attribute.Id))
                                {
                                    if (string.IsNullOrEmpty(attributeValue.Item1) || !int.TryParse(attributeValue.Item1, out var attributeValueId))
                                    {
                                        isAttributeValid = false;
                                        break;
                                    }
                                    var value = await _productAttributeService.GetProductAttributeValueByIdAsync(attributeValueId);
                                    if (value == null || !value.Published)
                                    {
                                        isAttributeValid = false;
                                        break;
                                    }
                                }
                            }
                        }
                        if (!isAttributeValid)
                            continue;
                        if (combination.AttributesXml == defaultCombinationAttrXml)
                        {
                            defaulCombinationOldPrice = (combination.OverriddenOldPrice ?? 0) == 0 ?
                                ((combination.OverriddenPrice ?? 0) == 0 ? defaulCombinationOldPrice : 0) : Convert.ToDecimal(combination.OverriddenOldPrice);
                            defaultCombinationPrice = combination.OverriddenPrice ?? defaultCombinationPrice;
                            defaultCombinationMsrp = (combination.OverriddenMsrp ?? 0) == 0 ?
                                ((combination.OverriddenPrice ?? 0) == 0 ? defaultCombinationMsrp : 0) : Convert.ToDecimal(combination.OverriddenMsrp);

                        }
                        oldPrice = (combination.OverriddenOldPrice ?? 0) == 0 ? product.OldPrice : combination.OverriddenOldPrice ?? 0;
                        price = (combination.OverriddenPrice ?? 0) == 0 ? product.Price : combination.OverriddenPrice ?? 0;
                        msrp = (combination.OverriddenMsrp ?? 0) == 0 ? product.Msrp : combination.OverriddenMsrp ?? 0;

                        minOldPrice = minOldPrice == 0 ? oldPrice :
                            (minOldPrice < oldPrice || oldPrice == 0
                            ? minOldPrice : oldPrice);

                        maxOldPrice = maxOldPrice == 0 ? oldPrice :
                           (maxOldPrice > oldPrice ? maxOldPrice : oldPrice);

                        minPrice = minPrice == 0 ? price :
                       (minPrice < price || price == 0 ? minPrice : price);


                        maxPrice = maxPrice == 0 ? price :
                           (maxPrice > price ? maxPrice : price);

                        minMsrp = minMsrp == 0 ? msrp :
                    (minMsrp < msrp || msrp == 0 ? minMsrp : msrp);
                        maxMsrp = maxMsrp == 0 ? msrp :
                           (maxMsrp > msrp ? maxMsrp : msrp);
                    }

                    #endregion

                    minMsrp = minMsrp == 0 ? product.Msrp : minMsrp;
                    maxMsrp = maxMsrp == 0 ? product.Msrp : maxMsrp;
                    minOldPrice = minOldPrice == 0 ? product.OldPrice : minOldPrice;
                    maxOldPrice = maxOldPrice == 0 ? product.OldPrice : maxOldPrice;
                    minPrice = minPrice == 0 ? product.Price : minPrice;
                    maxPrice = maxPrice == 0 ? product.Price : maxPrice;

                    #region Update Product
                    if (updateProduct && (
                        product.MinMsrp != minMsrp
                        || product.MaxMsrp != maxMsrp
                        || product.MinPrice != minPrice
                        || product.MaxPrice != maxPrice
                        || product.MinOldprice != minOldPrice
                        || product.MaxOldPrice != maxOldPrice))
                    {
                        product.MinMsrp = minMsrp == 0 ? product.Msrp : minMsrp;
                        product.MaxMsrp = maxMsrp == 0 ? product.Msrp : maxMsrp;
                        product.MinOldprice = minOldPrice == 0 ? product.OldPrice : minOldPrice;
                        product.MaxOldPrice = maxOldPrice == 0 ? product.OldPrice : maxOldPrice;
                        product.MinPrice = minPrice == 0 ? product.Price : minPrice;
                        product.MaxPrice = maxPrice == 0 ? product.Price : maxPrice;
                        product.Price = defaultCombinationPrice <= 0 ? product.Price : defaultCombinationPrice;
                        product.OldPrice = defaulCombinationOldPrice <= 0 ? product.OldPrice : defaulCombinationOldPrice;
                        product.Msrp = defaultCombinationMsrp <= 0 ? product.Msrp : defaultCombinationMsrp;
                        product.IsVariantProduct = true;
                        if (updateProductWithEvent)
                        {
                            await _customproductService.UpdateProductAsync(product);
                        }
                        else
                        {
                            await _customproductService.UpdateProductWithoutEvent(product);
                        }
                        if (isVariantTask)
                        {
                            await _loggerService.InsertLogAsync(LogLevel.Information, "Variant product Task ",
                                $"Product marked as variant product and Variant Price Update for Product info: {minPrice} - {maxPrice} -{minOldPrice}-{maxOldPrice}- {minMsrp}-{maxMsrp} " + product.Id);
                        }
                    }
                    else if (defaultCombinationPrice != product.Price || defaulCombinationOldPrice != product.OldPrice
                        || defaultCombinationMsrp != product.Msrp)
                    {

                        product.Price = defaultCombinationPrice <= 0 ? product.Price : defaultCombinationPrice;
                        product.OldPrice = defaulCombinationOldPrice <= 0 ? product.OldPrice : defaulCombinationOldPrice;
                        product.Msrp = defaultCombinationMsrp <= 0 ? product.Msrp : defaultCombinationMsrp;
                        if (updateProductWithEvent)
                        {
                            await _customproductService.UpdateProductAsync(product);
                        }
                        else
                        {
                            await _customproductService.UpdateProductWithoutEvent(product);
                        }
                    }

                    #endregion
                }
                else
                {
                    if (updateProduct && product.IsVariantProduct)
                    {
                        product.MinMsrp = minMsrp;
                        product.MaxMsrp = maxMsrp;
                        product.MinOldprice = minOldPrice;
                        product.MaxOldPrice = maxOldPrice;
                        product.MinPrice = minPrice;
                        product.MaxPrice = maxPrice;
                        product.IsVariantProduct = false;
                        if (updateProductWithEvent)
                        {
                            await _customproductService.UpdateProductAsync(product);
                        }
                        else
                        {
                            await _customproductService.UpdateProductWithoutEvent(product);

                        }
                        if (isVariantTask)
                        {
                            await _loggerService.InsertLogAsync(LogLevel.Information, "Variant product Task ",
                   $"Product marked as simple product" + product.Id);
                        }
                    }
                }
            }
            else if (updateProduct && product.IsVariantProduct)
            {
                await UpdateGroupProductPrice(product.Id);
            }
            return (isVariantPrice, minOldPrice, maxOldPrice, minPrice, maxPrice, minMsrp, maxMsrp);
        }


        public async Task<bool> UpdateGroupProductPrice(int productID)
        {

            var _storeContext = EngineContext.Current.Resolve<IStoreContext>();
            var products = await _customproductService.GetAssociatedProductsAsync(productID, (await _storeContext.GetCurrentStoreAsync()).Id);
            if (products.Count > 0)
            {

                decimal totalPrice = 0;
                decimal totalOldPrice = 0;
                decimal totalMsrp = 0;
                decimal readMsrp = 0;
                decimal minOldPrice = 0;
                decimal maxOldPrice = 0;
                decimal minPrice = 0;
                decimal maxPrice = 0;
                decimal minMsrp = 0;
                decimal maxMsrp = 0;
                bool isVariantPrice = false;
                bool isAttributeProduct = false;
                foreach (var _product in products)
                {
                    (bool _isVariantPrice, decimal _minOldPrice, decimal _maxOldPrice, decimal _minPrice, decimal _maxPrice,
                       decimal _minMsrp, decimal _maxMsrp)
                       = await GetVariantPriceRange(_product, false, false);
                    isVariantPrice = isVariantPrice == true ? true : _isVariantPrice;


                    minOldPrice = minOldPrice + (_minOldPrice == 0 ? _minPrice : _minOldPrice);
                    maxOldPrice = maxOldPrice + (_maxOldPrice == 0 ? _maxPrice : _maxOldPrice);

                    minPrice = minPrice + _minPrice;
                    maxPrice = maxPrice + _maxPrice;

                    minMsrp = minMsrp + (_minMsrp == 0 ? _minOldPrice == 0 ? _minPrice : _minOldPrice : _minMsrp);
                    maxMsrp = maxMsrp + (_maxMsrp == 0 ? _maxOldPrice == 0 ? _maxPrice : _maxOldPrice : _maxMsrp);

                    isAttributeProduct = false;
                    var result = await _customproductService.GetDefaultAttributePriceOfProduct(_product);
                    decimal msrp = result.Item1;
                    decimal oldPrice = result.Item2;
                    decimal price = result.Item3;
                    isAttributeProduct = result.Item4;

                    if (!isAttributeProduct)
                    {
                        totalOldPrice = totalOldPrice + (_product.OldPrice == 0 ? _product.Price : _product.OldPrice);
                        totalPrice = totalPrice + _product.Price;
                        totalMsrp = totalMsrp + (_product.Msrp == 0 ? _product.Price : _product.Msrp);
                        readMsrp = readMsrp + _product.Msrp;
                    }
                    else
                    {
                        totalOldPrice = totalOldPrice + (oldPrice == 0 ? price : oldPrice);
                        totalPrice = totalPrice + price;
                        totalMsrp = totalMsrp + (msrp == 0 ? price : msrp);
                        readMsrp = readMsrp + msrp;
                    }
                }
                var product = await _customproductService.GetProductByIdAsync(productID);
                product.OldPrice = totalOldPrice;
                product.Price = totalPrice;
                product.Msrp = readMsrp == 0 ? 0 : totalMsrp;
                product.IsVariantProduct = isAttributeProduct;
                product.MinMsrp = minMsrp;
                product.MaxMsrp = maxMsrp;
                product.MinPrice = minPrice;
                product.MaxPrice = maxPrice;
                product.MinOldprice = minOldPrice;
                product.MaxOldPrice = maxOldPrice;
                await _customproductService.UpdateProductAsync(product);
            }
            return true;
        }

        public async Task<bool> SyncGroupedProductsPrice()
        {
            try
            {
                var productIds = await (from prd in _productRepository.Table
                                        where prd.ProductTypeId == (int)ProductType.GroupedProduct
                                        select prd.Id).ToListAsync();

                foreach (var productId in productIds)
                {
                    await this.UpdateGroupProductPrice(productId);
                }
            }
            catch (Exception ex)
            {

            }
            return true;
        }





        //utilities

        public virtual async Task InsertProductVariantAsync(ProductVariant variant)
        {
            try
            {
                await _semaphoreProductVariantIdGenerate.WaitAsync();
                var _productVariantRepository = EngineContext.Current.Resolve<IRepository<ProductVariant>>();
                variant.VariantId = await GenerateVariantId();
                await _productVariantRepository.InsertAsync(variant);

            }
            finally
            {
                _semaphoreProductVariantIdGenerate.Release();
            }
        }
        protected virtual async Task UpdateProductVariantAsync(ProductVariant variant)
        {
            var _productVariantRepository = EngineContext.Current.Resolve<IRepository<ProductVariant>>();
            await _productVariantRepository.UpdateAsync(variant);
        }
        protected virtual async Task UpdateVariantCombinationAsync(VariantCombination variantcombination)
        {
            var _productVariantRepository = EngineContext.Current.Resolve<IRepository<VariantCombination>>();
            await _productVariantRepository.UpdateAsync(variantcombination);
        }
        protected virtual async Task DeleteVariantCombinationAsync(VariantCombination variantcombination)
        {
            var _productVariantRepository = EngineContext.Current.Resolve<IRepository<VariantCombination>>();
            await _productVariantRepository.DeleteAsync(variantcombination);
        }
        protected virtual async Task UpdateVariantCombinationLogAsync(VariantCombinationLog variantcombinationlog)
        {
            var _productVariantRepository = EngineContext.Current.Resolve<IRepository<VariantCombinationLog>>();
            await _productVariantRepository.UpdateAsync(variantcombinationlog);
        }
        protected async Task<ProductVariant> GetProductVariantByAttributeValueAsync(int productId, int productAttributeId, string attrValue)
        {
            var _productVariantRepository = EngineContext.Current.Resolve<IRepository<ProductVariant>>();
            return await _productVariantRepository.Table.Where(x => x.ProductAttributeId == productAttributeId && x.ProductId == productId && x.AttributeValue.ToLower() == attrValue.ToLower()).FirstOrDefaultAsync();
        }
        protected async Task<List<ProductVariant>> GetAllProductVariantByProductIdAsync(int productId)
        {
            var _productVariantRepository = EngineContext.Current.Resolve<IRepository<ProductVariant>>();
            return await _productVariantRepository.Table.Where(x => x.ProductId == productId).ToListAsync();
        }

        protected async Task InsertVariantCombinationAsync(VariantCombination vc)
        {
            var _variantcombination = EngineContext.Current.Resolve<IRepository<VariantCombination>>();
            await _variantcombination.InsertAsync(vc);
        }

        protected async Task<int> GenerateVariantId()
        {
            var _productVariantRepository = EngineContext.Current.Resolve<IRepository<ProductVariant>>();
            int variantId = (await _productVariantRepository.Table
     .Select(v => (int?)v.VariantId)
     .DefaultIfEmpty(0)
     .MaxAsync() ?? 0) + 1;

            return variantId;
        }
        protected VariantCombination GetVariantCombinationByProductAttributeValueIds(List<VariantCombination> combinations, string valueIds)
        {
            var valueIdsSet = new HashSet<string>((valueIds ?? string.Empty).Split('-'));

            foreach (var combination in combinations)
            {
                var combinationValueIdSet = new HashSet<string>((combination.ProductAttributeValueIds ?? string.Empty).Split('-'));

                // Check if both sets have exactly the same elements
                if (valueIdsSet.SetEquals(combinationValueIdSet))
                {
                    return combination;
                }
            }
            return null;
        }

        protected ProductVariant GetVariantByProductAttributeValueIds(List<ProductVariant> productVariants, string valueIds)
        {
            var valueIdsSet = new HashSet<string>((valueIds ?? string.Empty).Split('-')); // Convert input to HashSet

            foreach (var productVariant in productVariants)
            {

                if (valueIdsSet.Count == 1)
                {

                    if (productVariant.ProductAttributeValueId == Convert.ToInt32(valueIdsSet.First()))
                    {
                        return productVariant;
                    }
                }
                var combinationValueIdSet = new HashSet<string>((productVariant.ProductAttributeValueIds ?? string.Empty).Split('-')); // Convert combination to HashSet
                // Check if both sets have exactly the same elements
                if (valueIdsSet.SetEquals(combinationValueIdSet))
                {
                    return productVariant;
                }
            }
            return null;
        }

        protected async Task InsertVariantCombinationLogAsync(VariantCombinationLog vcLog)
        {
            var _variantcombination = EngineContext.Current.Resolve<IRepository<VariantCombinationLog>>();
            await _variantcombination.InsertAsync(vcLog);
        }
        public async Task<int> GetVariantIdFromAttributeDescription(int productId, string attributeDescription)
        {

            List<int> attributeValueIds = new List<int>();
            Dictionary<string, string> attributes = new Dictionary<string, string>();


            if (!string.IsNullOrEmpty(attributeDescription))
            {
                string[] attrs = attributeDescription.Split("<br />", StringSplitOptions.RemoveEmptyEntries);
                foreach (var attr in attrs)
                {
                    if (attr.Split(':').Length > 0)
                    {
                        if (!attributes.Where(a => a.Key.Trim() == string.Join(':', attr.Split(":")[0]).Trim()).Any())
                        {
                            attributes.Add(System.Net.WebUtility.HtmlDecode(System.Net.WebUtility.HtmlDecode(string.Join(':', attr.Split(":")[0])).Trim()), System.Net.WebUtility.HtmlDecode(string.Join(':', attr.Split(":").Skip(1))).Trim());
                        }
                    }
                }
            }

            var productAttributeMapping = await _productAttributeService.GetProductAttributeMappingsByProductIdAsync(productId);

            foreach (var mapping in productAttributeMapping)
            {
                var attribute = await _productAttributeService.GetProductAttributeByIdAsync(mapping.ProductAttributeId);
                if (!string.Equals(attribute.Name, await _localizationService.GetResourceAsync("Product.Attr.Stain"), StringComparison.InvariantCultureIgnoreCase))
                {
                    foreach (var attributeValue in await _productAttributeService.GetProductAttributeValuesAsync(mapping.Id))
                    {
                        if (attributes.Where(a => (a.Key.Trim() == attribute.Name || a.Key.Trim() == mapping.TextPrompt) && a.Value.Trim() == attributeValue.Name.Trim()).Any())
                        {
                            attributeValueIds.Add(attributeValue.Id);
                        }
                    }
                }
            }

            return (GetVariantCombinationByProductAttributeValueIds(await GetProductVariants(productId), string.Join("-", attributeValueIds.ToArray())))?.VariantId ?? 0;

        }



        public async Task<VariantCombination> GetVariantFromAttributeValues(int productId, List<int> attributeValueIds)
        {

            for (int i = attributeValueIds.Count() - 1; i >= 0; i--)
            {
                var pav = await _productAttributeService.GetProductAttributeValueByIdAsync(attributeValueIds[i]);
                var mapping = await _productAttributeService.GetProductAttributeMappingByIdAsync(pav.ProductAttributeMappingId);
                var attribute = await _productAttributeService.GetProductAttributeByIdAsync(mapping.ProductAttributeId);

                if (string.Equals(
                        attribute.Name,
                        await _localizationService.GetResourceAsync("Product.Attr.Stain"),
                        StringComparison.InvariantCultureIgnoreCase))
                {
                    attributeValueIds.RemoveAt(i);
                }
            }

            return (GetVariantCombinationByProductAttributeValueIds(await GetProductVariants(productId), string.Join("-", attributeValueIds)));
        }
        public async Task GenerateProductVariantSename(Product product)
        {
            var productVariantCombinations = await GetProductVariants(product.Id);

 
            foreach (var variantCombination in productVariantCombinations)
            {
                if (string.IsNullOrWhiteSpace(variantCombination.Title))
                    continue;
                if (variantCombination.Title.Trim().ToLower() == product.Name.Trim().ToLower())
                    continue;

                if (!string.IsNullOrEmpty(variantCombination.SeName))
                    continue;

                variantCombination.SeName = await _urlRecordService.ValidateSeNameAsync(variantCombination.VariantId, "VariantCombination", "", variantCombination.Title, false);
                await this.UpdateVariantCombinationAsync(variantCombination);
            }
        }

        public async Task<VariantCombination> GetItemVariantInfo(int productId, string attributesXml)
        {
            if (string.IsNullOrEmpty(attributesXml))
                return null;
            else
            {
                return await this.GetVariantFromAttributeValues(productId, (await _customProductAttributeParser.ParseProductAttributeValuesAsync(attributesXml)).Select(attrValues => attrValues.Id).ToList());
            }
        }

        public async Task<List<ProductAttributeCombination>> GetVariantCombinationAsync(
VariantCombination variant, IList<ProductAttributeCombination> productAttributeCombinations)
        {
            var variantValues = await _customProductAttributeParser
                .ParseProductAttributeValuesAsync(variant.Combination);

            var variantValueIds = variantValues
                .Select(v => v.Id)
                .ToList();

            if (!productAttributeCombinations.Any())
                return new List<ProductAttributeCombination>();

            List<ProductAttributeCombination> matchedCombinations =
                new List<ProductAttributeCombination>();

            foreach (var combination in productAttributeCombinations)
            {
                var comboValues = await _customProductAttributeParser
                    .ParseProductAttributeValuesAsync(combination.AttributesXml);

                var comboValueIds = comboValues
                    .Select(v => v.Id)
                    .ToList();

                if (variantValueIds.All(vId => comboValueIds.Contains(vId)) &&
                    comboValueIds.Count <= variantValueIds.Count + 1)
                {
                    matchedCombinations.Add(combination);
                }
            }

            return matchedCombinations
                .OrderByDescending(x => x.OverriddenPrice ?? 0)
                .ToList();
        }
    }
}
