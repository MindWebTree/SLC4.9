using Microsoft.AspNetCore.Http;
using MWT.Nop.Core.Service.Catalog;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Catalog;
using Nop.Core.Infrastructure;
using Nop.Core.Security;
using Nop.Services.Catalog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MWT.Nop.Core.Services.Catalog
{
    public partial class CustomRecentlyViewedProductsService : RecentlyViewedProductsService,ICustomRecentlyViewedProductsService
    {
        public CustomRecentlyViewedProductsService(CatalogSettings catalogSettings, CookieSettings cookieSettings, IHttpContextAccessor httpContextAccessor, IProductService productService, IWebHelper webHelper) : base(catalogSettings, cookieSettings, httpContextAccessor, productService, webHelper)
        {
        }

        public virtual async Task CustomAddProductToRecentlyViewedListAsync(int productId)
        {
            if (_httpContextAccessor.HttpContext?.Response == null)
                return;

            //whether recently viewed products is enabled
            if (!_catalogSettings.RecentlyViewedProductsEnabled)
                return;

            //get list of recently viewed product identifiers
            var productIds = GetRecentlyViewedProductsIds();

            //whether product identifier to add already exist
            if (!productIds.Contains(productId))
            {
                productIds.Insert(0, productId);
                var _staticCacheManager = EngineContext.Current.Resolve<IStaticCacheManager>();
                var _workContext = EngineContext.Current.Resolve<IWorkContext>();
                var customer = await _workContext.GetCurrentCustomerAsync();
                if (customer != null)
                {
                    await _staticCacheManager.RemoveByPrefixAsync(CustomNopCatalogDefaults.SearchDefaultAutoCompletePrefix, customer.Id);
                }
            }

            //limit list based on the allowed number of the recently viewed products
            productIds = productIds.Take(_catalogSettings.RecentlyViewedProductsNumber).ToList();

            //set cookie
            await AddRecentlyViewedProductsCookieAsync(productIds);
        }
    }
}
