using MWT.Nop.Core.Services.Customers;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Discounts;
using Nop.Core.Infrastructure;
using Nop.Data;
using Nop.Services.Catalog;
using Nop.Services.Configuration;
using Nop.Services.Customers;
using Nop.Services.Localization;
using Nop.Services.Security;
using Nop.Services.Stores;


namespace MWT.Nop.Core.Services.Catalog
{
    public partial class CustomCategoryService : CategoryService , ICustomCategoryService
    {
        #region Fields
        private readonly ISettingService _settingService;
        #endregion
        public CustomCategoryService(IAclService aclService, ICustomerService customerService,
            ILocalizationService localizationService, IRepository<Category> categoryRepository,
            IRepository<DiscountCategoryMapping> discountCategoryMappingRepository, IRepository<Product> productRepository,
            IRepository<ProductCategory> productCategoryRepository, IStaticCacheManager staticCacheManager, IStoreContext storeContext,
            IStoreMappingService storeMappingService, IWorkContext workContext, ISettingService settingService) : base(aclService, customerService, localizationService, categoryRepository, 
                discountCategoryMappingRepository, productRepository, productCategoryRepository, staticCacheManager, storeContext, storeMappingService, workContext)
        {
            _settingService=settingService;
        }



        public async ValueTask<bool> IsMwtWidgetApplied(string widget, int entityID, string entityType, bool isMobileDevice)
        {
           
            string keyValue = await _settingService.GetSettingByKeyAsync<string>($"MWTPluginWidgetsCatalogSetting{(isMobileDevice ? ".Mobile" : "")}.{entityType}." + widget);
            if (!string.IsNullOrEmpty(keyValue))
            {
                return keyValue.Split(',').Where(m => m == entityID.ToString()).Any();
            }
            return false;
        }
    }
}
