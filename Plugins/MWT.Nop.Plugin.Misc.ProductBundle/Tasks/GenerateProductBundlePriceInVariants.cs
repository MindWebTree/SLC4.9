using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MWT.Nop.Plugin.Misc.ProductBundle.Domain;
using MWT.Nop.Plugin.Misc.ProductBundle.Domain.MWT.Nop.Plugin.Misc.ProductBundle.Domain;
using MWT.Nop.Plugin.Misc.ProductBundle.Services;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Customization.Catalog;
using Nop.Core.Infrastructure;
using Nop.Services.Catalog;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Tasks;

namespace Nop.Web.Customizations.Tasks
{
    public class GenerateProductBundlePriceInVariants : IScheduleTask
    {
        private readonly IProductService _productService;
        private readonly IBundleService _bundleService;
        private readonly IProductAttributeService _productAttributeService;
        private readonly ILocalizationService _localizationService;
        private readonly IProductAttributeParser _productAttributeParser;
        public GenerateProductBundlePriceInVariants(IProductService productService,
            IBundleService bundleService,
            IProductAttributeService productAttributeService,
            ILocalizationService localizationService,
            IProductAttributeParser productAttributeParser)
        {
            _productService = productService;
            _bundleService = bundleService;
            _productAttributeService = productAttributeService;
            _localizationService = localizationService;
            _productAttributeParser = productAttributeParser;
        }
        public virtual async System.Threading.Tasks.Task ExecuteAsync()
        {
            var _settingService = EngineContext.Current.Resolve<ISettingService>();

             int pageSize = 50;
            int pageIndex = 0;





            var productBundleSettings = await _settingService.LoadSettingAsync<ProductBundleSettings>();
            decimal discountPercentage = productBundleSettings.DiscountPercentage;

            while (true)
            {
                var pagedMainProducts = await _bundleService.GetBundleProductsAsync(pageIndex, pageSize);

                foreach (var mainProduct in pagedMainProducts)
                {

                    int id = mainProduct.Id;
                    await _bundleService.CalculateBundlePrice(mainProduct);
                }
              

                if (!pagedMainProducts.HasNextPage)
                    break;

                pageIndex++;
            }
            await _bundleService.ValidateAndRestoreIncompleteBundlesAsync();
        }






    }
}
