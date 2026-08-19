using BundlerMinifier;
using MWT.Nop.Plugin.Misc.ProductBundle.Domain;
using MWT.Nop.Plugin.Misc.ProductBundle.Infrastructure.Cache;
using MWT.Nop.Plugin.Misc.ProductBundle.Services; // Ensure this matches where your IProductBundleService is
using Nop.Core.Caching;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Customization.Catalog;
using Nop.Core.Events;
using Nop.Data;
using Nop.Services.Catalog;
using Nop.Services.Events;
using Nop.Web.Areas.CustomOrder.Models.Product;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MWT.Nop.Plugin.Misc.ProductBundle.Consumer
{
    // created this static class it will guard from recrussion event fire 
    // We set true before updating so it will skipp imetiate loop
    public static class BundleEventContext
    {
        public static AsyncLocal<bool> SkipRecalculation = new();
    }
    public class BundlePriceEventConsumer :
         IConsumer<EntityUpdatedEvent<BundleConfiguration>>,
         IConsumer<EntityDeletedEvent<BundleConfiguration>>,
        IConsumer<EntityInsertedEvent<BundleItem>>,
        IConsumer<EntityUpdatedEvent<BundleItem>>,
        IConsumer<EntityDeletedEvent<BundleItem>>,
        IConsumer<EntityUpdatedEvent<VariantCombination>>,
        IConsumer<EntityInsertedEvent<VariantCombination>>,
    IConsumer<EntityUpdatedEvent<Product>>,
        IConsumer<EntityDeletedEvent<ProductAttributeMapping>>,
         IConsumer<EntityUpdatedEvent<ProductAttributeMapping>>,
        IConsumer<EntityDeletedEvent<ProductAttributeValue>>,
        IConsumer<EntityUpdatedEvent<ProductAttributeValue>>
  

    {
        private readonly IBundleService _bundleService;
        private readonly IProductService _productService;
        private readonly IRepository<BundleConfiguration> _bundleConfigRepository;
        private readonly IProductAttributeService _productAttributeService;
        private readonly IStaticCacheManager _staticCacheManager;


        public BundlePriceEventConsumer(
            IBundleService bundleService,
            IProductService productService,
            IRepository<BundleConfiguration> bundleConfigRepository,
            IProductAttributeService productAttributeService,
            IStaticCacheManager staticCacheManager)
        {
            _bundleService = bundleService;
            _productService = productService;
            _bundleConfigRepository = bundleConfigRepository;
            _productAttributeService = productAttributeService;
            _staticCacheManager = staticCacheManager;
        }
        //Handle BundleItems Crud Event
        public async Task HandleEventAsync(EntityInsertedEvent<BundleItem> eventMessage)
        {
            await RecalculateByBundleId(eventMessage.Entity.BundleId);
            var bundle = await _bundleService.GetBundleByIdAsync(eventMessage.Entity.BundleId);
            if (bundle != null)
                await _staticCacheManager.RemoveByPrefixAsync(NopBundleCatalogDefaults.BundleWidgetPrefixCacheKeyById, bundle.ProductId);
        }

        public async Task HandleEventAsync(EntityUpdatedEvent<BundleItem> eventMessage)
        {
            await RecalculateByBundleId(eventMessage.Entity.BundleId);
            var bundle = await _bundleService.GetBundleByIdAsync(eventMessage.Entity.BundleId);
            if (bundle != null)
                await _staticCacheManager.RemoveByPrefixAsync(NopBundleCatalogDefaults.BundleWidgetPrefixCacheKeyById, bundle.ProductId);
        }

        public async Task HandleEventAsync(EntityDeletedEvent<BundleItem> eventMessage)
        {
            await RecalculateByBundleId(eventMessage.Entity.BundleId);
            var bundle = await _bundleService.GetBundleByIdAsync(eventMessage.Entity.BundleId);
            if (bundle != null)
                await _staticCacheManager.RemoveByPrefixAsync(NopBundleCatalogDefaults.BundleWidgetPrefixCacheKeyById, bundle.ProductId);
        }

        // Handle affected bundle by product id when attributecombinationupdate
        public async Task HandleEventAsync(EntityUpdatedEvent<VariantCombination> eventMessage)
        {
            // guarding from recussion event fire 
            if (BundleEventContext.SkipRecalculation.Value)
                return;
            var bundles = await _bundleService.GetAffectedBundlesByProductId(eventMessage.Entity.ProductId);
            try
            {
                // true for guard event which can all from below method 
                BundleEventContext.SkipRecalculation.Value = true;
                foreach (var bundle in bundles.Where(x => x.IsActive))
                {
                    bool isValid = await this._bundleService.IsBundleValid(bundle);
                    if (isValid)
                        await _bundleService.CalculateSingleBundlePrice(bundle);
                    else await _bundleService.ValidateAndRestoreBundleVariantAsync(bundle);
                }
            }
            finally
            {
                BundleEventContext.SkipRecalculation.Value = false;
            }
        }
        public async Task HandleEventAsync(EntityInsertedEvent<VariantCombination> eventMessage)
        {
            // guarding from recussion event fire 
            if (BundleEventContext.SkipRecalculation.Value)
                return;
            var bundles = await _bundleService.GetAffectedBundlesByProductId(eventMessage.Entity.ProductId);
            try
            {
                // true for guard event which can all from below method 
                BundleEventContext.SkipRecalculation.Value = true;
                foreach (var bundle in bundles.Where(x => x.IsActive))
                {
                    bool isValid = await this._bundleService.IsBundleValid(bundle);
                    if (isValid)
                        await _bundleService.CalculateSingleBundlePrice(bundle);
                    else await _bundleService.ValidateAndRestoreBundleVariantAsync(bundle);
                }
            }
            finally
            {
                BundleEventContext.SkipRecalculation.Value = false;
            }
        }
        // Handle Bundle Updation
        public async Task HandleEventAsync(EntityUpdatedEvent<BundleConfiguration> eventMessage)
        {
            if (BundleEventContext.SkipRecalculation.Value)
                return;
            var config = await _bundleConfigRepository.GetByIdAsync(eventMessage.Entity.Id);


            bool isValid = await this._bundleService.IsBundleValid(config);
            if (isValid)
            {// we calculating bundle price single by single
                await _bundleService.CalculateSingleBundlePrice(config);
            }// if not valid then we restoring variant price 
            else await _bundleService.ValidateAndRestoreBundleVariantAsync(config);

            await _staticCacheManager.RemoveByPrefixAsync(NopBundleCatalogDefaults.BundleWidgetPrefixCacheKeyById, eventMessage.Entity.Id);
        }

        public async Task HandleEventAsync(EntityDeletedEvent<BundleConfiguration> eventMessage)
        {
        

            await _staticCacheManager.RemoveByPrefixAsync(NopBundleCatalogDefaults.BundleWidgetPrefixCacheKeyById, eventMessage.Entity.Id);
        }
        private async Task RecalculateByBundleId(int bundleId)
        {
            var config = await _bundleConfigRepository.GetByIdAsync(bundleId);
            if (config != null)
            {
                bool isValid = await this._bundleService.IsBundleValid(config);
                if (isValid)
                    await _bundleService.CalculateSingleBundlePrice(config);
                else await _bundleService.ValidateAndRestoreBundleVariantAsync(config);
            }
        }

        // Handle Product Update
        public async Task HandleEventAsync(EntityUpdatedEvent<Product> eventMessage)
        {
            await ProcessEvent(eventMessage.Entity.Id);
            await _productService.GetVariantPriceRange(eventMessage.Entity, false, true, false);
        }



        public async Task HandleEventAsync(EntityUpdatedEvent<ProductAttributeMapping> eventMessage)
        {
            await ProcessEvent(eventMessage.Entity.ProductId);
        }

        public async Task HandleEventAsync(EntityDeletedEvent<ProductAttributeMapping> eventMessage)
        {
            await ProcessEvent(eventMessage.Entity.ProductId);
        }
        public async Task HandleEventAsync(EntityUpdatedEvent<ProductAttributeValue> eventMessage)
        {
            ProductAttributeMapping mapping = await _productAttributeService.GetProductAttributeMappingByIdAsync(eventMessage.Entity.ProductAttributeMappingId);
            if (mapping != null)
                await ProcessEvent(mapping.ProductId);
        }

        public async Task HandleEventAsync(EntityDeletedEvent<ProductAttributeValue> eventMessage)
        {
            ProductAttributeMapping mapping = await _productAttributeService.GetProductAttributeMappingByIdAsync(eventMessage.Entity.ProductAttributeMappingId);
            if (mapping != null)
                await ProcessEvent(mapping.ProductId);
        }

        #region Utilities

        private async Task ProcessEvent(int productId)
        {
            try
            {
                await _staticCacheManager.RemoveByPrefixAsync(NopBundleCatalogDefaults.BundleWidgetPrefixCacheKeyById, productId);
                var productdetails = await _productService.GetProductByIdAsync(productId);
                if (BundleEventContext.SkipRecalculation.Value)
                    return;
                #region  Bundle Product

                if (productdetails.IsBundleProduct)
                {
                    var bundles = await _bundleService.GetActiveBundleByProductIdAsync(productdetails.Id);
                    foreach (var bundle in bundles)
                    {
                        bool isValid = await this._bundleService.IsBundleValid(bundle);
                        if (isValid)
                        {
                            await _bundleService.CalculateSingleBundlePrice(bundle);
                        }// if not valid then we restoring variant price 
                        else await _bundleService.ValidateAndRestoreBundleVariantAsync(bundle);
                    }
                }
                else
                {
                    BundleEventContext.SkipRecalculation.Value = true;
                    var bundles = await _bundleService.GetActiveBundleByProductIdAsync(productdetails.Id);
                    foreach (var bundle in bundles)
                    {
                        // first we restoring price of variant and then disabling 
                        await _bundleService.ValidateAndRestoreBundleVariantAsync(bundle);
                        //   await _bundleService.DisableBundle(bundle.VariantId);
                    }
                }

                #endregion

                #region Bundle Items

                var effectedBundles = await _bundleService.GetAffectedBundlesByProductId(productId);
                try
                {
                    // true for guard event which can all from below method 
                    BundleEventContext.SkipRecalculation.Value = true;
                    foreach (var bundle in effectedBundles.Where(x => x.IsActive))
                    {
                        bool isValid = await this._bundleService.IsBundleValid(bundle);
                        if (isValid)
                            await _bundleService.CalculateSingleBundlePrice(bundle);
                        else await _bundleService.ValidateAndRestoreBundleVariantAsync(bundle);
                    }
                }
                finally
                {
                    BundleEventContext.SkipRecalculation.Value = false;
                }

                #endregion

            }
            finally
            {
                BundleEventContext.SkipRecalculation.Value = false;
            }
        }

        #endregion
    }
}