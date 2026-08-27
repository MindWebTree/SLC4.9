using Microsoft.AspNetCore.Http;
using MWT.Nop.Core.Domain;
using MWT.Nop.Core.Domain.FAQModule;
using MWT.Nop.Core.Domain.QA;
using MWT.Nop.Core.Service.Catalog;
using MWT.Nop.Core.Service.Zoho;
using MWT.Nop.Core.Services.Customers;
using MWT.Nop.Core.Services.MailChimp;
using MWT.Plugin.Misc.MwtStorefront.Infrastructure.Cache;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Orders;
using Nop.Core.Events;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Events;
using Nop.Services.Helpers;
using Nop.Core.Domain.Media;
namespace MWT.Plugin.Misc.MwtStorefront.EventConsumer
{
    public partial class ModelCacheEventConsumer :
     //manufacturers
     IConsumer<EntityInsertedEvent<Manufacturer>>,
     IConsumer<EntityUpdatedEvent<Manufacturer>>,
     IConsumer<EntityDeletedEvent<Manufacturer>>,
    //categories
    IConsumer<EntityInsertedEvent<Category>>,
     IConsumer<EntityUpdatedEvent<Category>>,
     IConsumer<EntityDeletedEvent<Category>>,
     //product categories
     IConsumer<EntityInsertedEvent<ProductCategory>>,
     IConsumer<EntityDeletedEvent<ProductCategory>>,
     //products
     IConsumer<EntityUpdatedEvent<Product>>,
     IConsumer<EntityDeletedEvent<Product>>,
     //product tags
     IConsumer<EntityUpdatedEvent<ProductTag>>,
     IConsumer<EntityDeletedEvent<ProductTag>>,
     //SpecificationAttribute
     IConsumer<EntityUpdatedEvent<SpecificationAttribute>>,
     IConsumer<EntityDeletedEvent<SpecificationAttribute>>,
     //SpecificationOption
     IConsumer<EntityUpdatedEvent<SpecificationAttributeOption>>,
     IConsumer<EntityDeletedEvent<SpecificationAttributeOption>>,

     //SpecificationOption
     IConsumer<EntityUpdatedEvent<ProductSpecificationAttribute>>,
      IConsumer<EntityInsertedEvent<ProductSpecificationAttribute>>,
     IConsumer<EntityDeletedEvent<ProductSpecificationAttribute>>,
      IConsumer<EntityInsertedEvent<Order>>,
     IConsumer<EntityUpdatedEvent<Order>>,
     // FBTProducts 
     IConsumer<EntityUpdatedEvent<FBTProduct>>,
     IConsumer<EntityInsertedEvent<FBTProduct>>,
     IConsumer<EntityDeletedEvent<FBTProduct>>,

     IConsumer<EntityUpdatedEvent<ProductAttributeMapping>>,
     IConsumer<EntityInsertedEvent<ProductAttributeMapping>>,
     IConsumer<EntityDeletedEvent<ProductAttributeMapping>>,

      IConsumer<EntityUpdatedEvent<ProductAttributeValue>>,
     IConsumer<EntityInsertedEvent<ProductAttributeValue>>,
     IConsumer<EntityDeletedEvent<ProductAttributeValue>>,

     IConsumer<EntityUpdatedEvent<ProductAttributeCombination>>,
     IConsumer<EntityInsertedEvent<ProductAttributeCombination>>,
     IConsumer<EntityDeletedEvent<ProductAttributeCombination>>,
          IConsumer<EntityUpdatedEvent<GroupedProductConfiguration>>,
     IConsumer<EntityInsertedEvent<GroupedProductConfiguration>>,
     IConsumer<EntityDeletedEvent<GroupedProductConfiguration>>,
       IConsumer<EntityDeletedEvent<ShoppingCartItem>>,
            IConsumer<EntityInsertedEvent<ShoppingCartItem>>,
            IConsumer<EntityUpdatedEvent<ShoppingCartItem>>,
           IConsumer<EntityDeletedEvent<QuickFilter>>,
            IConsumer<EntityInsertedEvent<QuickFilter>>,
            IConsumer<EntityUpdatedEvent<QuickFilter>>,
     IConsumer<EntityDeletedEvent<RelatedSearch>>,
            IConsumer<EntityInsertedEvent<RelatedSearch>>,
            IConsumer<EntityUpdatedEvent<RelatedSearch>>,
         IConsumer<EntityDeletedEvent<CrossSellProduct>>,
            IConsumer<EntityInsertedEvent<CrossSellProduct>>,
            IConsumer<EntityUpdatedEvent<CrossSellProduct>>,
      IConsumer<CustomerRegisteredEvent>,
         IConsumer<EntityInsertedEvent<Picture>>,
     IConsumer<EntityUpdatedEvent<Picture>>,
     IConsumer<EntityDeletedEvent<Picture>>,
     IConsumer<EntityUpdatedEvent<QuestionAnswer>>,
          IConsumer<EntityDeletedEvent<Faq>>,
            IConsumer<EntityInsertedEvent<Faq>>,
            IConsumer<EntityUpdatedEvent<Faq>>
    {
        #region Fields

        private readonly CatalogSettings _catalogSettings;
        private readonly IStaticCacheManager _staticCacheManager;
        private readonly IProductExtendedService _productService;
        private readonly IMailchimpService _mailchimpService;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ICustomerExtendedService _customerService;
        private readonly IZohoService _zohoService;
        private readonly IWorkContext _workContext;
        //private readonly IMetaEventsService _metaEventService;
        private readonly IUserAgentHelper _userAgentHelper;
        private readonly IProductAttributeFormatter _productAttributeFormatter;
        private readonly IProductAttributeService _productAttributeService;

        #endregion

        #region Ctor

        public ModelCacheEventConsumer(CatalogSettings catalogSettings, IStaticCacheManager staticCacheManager,
            IProductExtendedService productService, IMailchimpService mailchimpService, IGenericAttributeService genericAttributeService,

            IHttpContextAccessor httpContextAccessor, ICustomerExtendedService customerService, IZohoService zohoService,
              IWorkContext workContext, /*IMetaEventsService metaEventService,*/
             IUserAgentHelper userAgentHelper,
              IProductAttributeFormatter productAttributeFormatter, IProductAttributeService productAttributeService)

        {
            _staticCacheManager = staticCacheManager;
            _catalogSettings = catalogSettings;
            _productService = productService;
            _mailchimpService = mailchimpService;
            _genericAttributeService = genericAttributeService;
            _httpContextAccessor = httpContextAccessor;
            _customerService = customerService;
            _zohoService = zohoService;
            _workContext = workContext;
            //_metaEventService = metaEventService;
            _userAgentHelper = userAgentHelper;
            _productAttributeFormatter = productAttributeFormatter;
            _productAttributeService = productAttributeService;
        }


        #endregion

        #region Methods


        #region CrossSell

        public async Task HandleEventAsync(EntityInsertedEvent<CrossSellProduct> eventMessage)
        {
            await _staticCacheManager.RemoveAsync(CustomNopModelCacheDefaults.CrossSellProductsCacheKey, eventMessage.Entity.ProductId1);
        }

        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task HandleEventAsync(EntityUpdatedEvent<CrossSellProduct> eventMessage)
        {
            await _staticCacheManager.RemoveAsync(CustomNopModelCacheDefaults.CrossSellProductsCacheKey, eventMessage.Entity.ProductId1);
        }

        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task HandleEventAsync(EntityDeletedEvent<CrossSellProduct> eventMessage)
        {
            await _staticCacheManager.RemoveAsync(CustomNopModelCacheDefaults.CrossSellProductsCacheKey, eventMessage.Entity.ProductId1);
        }

        #endregion

        #region RelatedSearch

        public async Task HandleEventAsync(EntityInsertedEvent<RelatedSearch> eventMessage)
        {
            await _staticCacheManager.RemoveByPrefixAsync(CustomNopModelCacheDefaults.RelatedSearchTermPrefix);
        }

        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task HandleEventAsync(EntityUpdatedEvent<RelatedSearch> eventMessage)
        {
            await _staticCacheManager.RemoveByPrefixAsync(CustomNopModelCacheDefaults.RelatedSearchTermPrefix);
        }

        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task HandleEventAsync(EntityDeletedEvent<RelatedSearch> eventMessage)
        {
            await _staticCacheManager.RemoveByPrefixAsync(CustomNopModelCacheDefaults.RelatedSearchTermPrefix);
        }

        #endregion

        #region Faq

        public async Task HandleEventAsync(EntityInsertedEvent<Faq> eventMessage)
        {
            await _staticCacheManager.RemoveByPrefixAsync(CustomNopModelCacheDefaults.FaqPrefix);
        }

        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task HandleEventAsync(EntityUpdatedEvent<Faq> eventMessage)
        {
            await _staticCacheManager.RemoveByPrefixAsync(CustomNopModelCacheDefaults.FaqPrefix);
        }

        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task HandleEventAsync(EntityDeletedEvent<Faq> eventMessage)
        {
            await _staticCacheManager.RemoveByPrefixAsync(CustomNopModelCacheDefaults.FaqPrefix);
        }

        #endregion


        #region QuickFilter

        public async Task HandleEventAsync(EntityInsertedEvent<QuickFilter> eventMessage)
        {
            await _staticCacheManager.RemoveByPrefixAsync(CustomNopModelCacheDefaults.QuickFiltersPrefix);
        }

        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task HandleEventAsync(EntityUpdatedEvent<QuickFilter> eventMessage)
        {
            await _staticCacheManager.RemoveByPrefixAsync(CustomNopModelCacheDefaults.QuickFiltersPrefix);
        }

        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task HandleEventAsync(EntityDeletedEvent<QuickFilter> eventMessage)
        {
            await _staticCacheManager.RemoveByPrefixAsync(CustomNopModelCacheDefaults.QuickFiltersPrefix);
        }

        #endregion

        #region Shopping Cart

        public async Task HandleEventAsync(EntityInsertedEvent<ShoppingCartItem> eventMessage)
        {


            await _staticCacheManager.RemoveByPrefixAsync(CustomNopModelCacheDefaults.RecommendationProductsPrefix, eventMessage.Entity.CustomerId);
        }

        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task HandleEventAsync(EntityUpdatedEvent<ShoppingCartItem> eventMessage)
        {

            await _staticCacheManager.RemoveByPrefixAsync(CustomNopModelCacheDefaults.RecommendationProductsPrefix, eventMessage.Entity.CustomerId);
        }

        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task HandleEventAsync(EntityDeletedEvent<ShoppingCartItem> eventMessage)
        {
            await _staticCacheManager.RemoveByPrefixAsync(CustomNopModelCacheDefaults.RecommendationProductsPrefix, eventMessage.Entity.CustomerId);
        }

        #endregion

        #region  Manufacturers

        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task HandleEventAsync(EntityInsertedEvent<Manufacturer> eventMessage)
        {
            await _staticCacheManager.RemoveByPrefixAsync(CustomNopModelCacheDefaults.MemberShipPricePrefixCacheKey);
        }

        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task HandleEventAsync(EntityUpdatedEvent<Manufacturer> eventMessage)
        {
            await _staticCacheManager.RemoveByPrefixAsync(CustomNopModelCacheDefaults.MemberShipPricePrefixCacheKey);
        }

        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task HandleEventAsync(EntityDeletedEvent<Manufacturer> eventMessage)
        {
            await _staticCacheManager.RemoveByPrefixAsync(CustomNopModelCacheDefaults.MemberShipPricePrefixCacheKey);
        }

        #endregion

        #region Categories


        public async Task HandleEventAsync(EntityInsertedEvent<Category> eventMessage)
        {
            await _staticCacheManager.RemoveByPrefixAsync(CustomNopModelCacheDefaults.MemberShipPricePrefixCacheKey);
        }
        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task HandleEventAsync(EntityUpdatedEvent<Category> eventMessage)
        {
            await _staticCacheManager.RemoveByPrefixAsync(CustomNopModelCacheDefaults.MemberShipPricePrefixCacheKey);
        }

        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task HandleEventAsync(EntityDeletedEvent<Category> eventMessage)
        {
            await _staticCacheManager.RemoveByPrefixAsync(CustomNopModelCacheDefaults.MemberShipPricePrefixCacheKey);
        }

        #endregion

        #region Product categories

        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task HandleEventAsync(EntityInsertedEvent<ProductCategory> eventMessage)
        {
            await _staticCacheManager.RemoveAsync(CustomNopModelCacheDefaults.MemberShipPriceOfProduct, eventMessage.Entity.ProductId);
        }

        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task HandleEventAsync(EntityDeletedEvent<ProductCategory> eventMessage)
        {
            await _staticCacheManager.RemoveAsync(CustomNopModelCacheDefaults.MemberShipPriceOfProduct, eventMessage.Entity.ProductId);
        }

        #endregion

        #region Products



        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task HandleEventAsync(EntityUpdatedEvent<Product> eventMessage)
        {
            await _staticCacheManager.RemoveAsync(CustomNopModelCacheDefaults.MemberShipPriceOfProduct, eventMessage.Entity.Id);
            await _staticCacheManager.RemoveByPrefixAsync(CustomNopModelCacheDefaults.CategoryGroupedProductsPrefix);
            await _staticCacheManager.RemoveByPrefixAsync(CustomNopModelCacheDefaults.HomePageFeaturedProductsPrefix);
            await _productService.UpdateProductInventory(eventMessage.Entity);
            await _staticCacheManager.RemoveByPrefixAsync(CustomNopModelCacheDefaults.ProductAttriburePrefixCacheKey, eventMessage.Entity.Id);
        }

        public async Task HandleEventAsync(EntityInsertedEvent<Product> eventMessage)
        {
            await _staticCacheManager.RemoveAsync(CustomNopModelCacheDefaults.MemberShipPriceOfProduct, eventMessage.Entity.Id);
            await _staticCacheManager.RemoveByPrefixAsync(CustomNopModelCacheDefaults.CategoryGroupedProductsPrefix);
            await _staticCacheManager.RemoveByPrefixAsync(CustomNopModelCacheDefaults.HomePageFeaturedProductsPrefix);
            await _productService.UpdateProductInventory(eventMessage.Entity);
            await _staticCacheManager.RemoveByPrefixAsync(CustomNopModelCacheDefaults.ProductAttriburePrefixCacheKey, eventMessage.Entity.Id);
        }

        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task HandleEventAsync(EntityDeletedEvent<Product> eventMessage)
        {
            await _staticCacheManager.RemoveAsync(CustomNopModelCacheDefaults.MemberShipPriceOfProduct, eventMessage.Entity.Id);
            await _staticCacheManager.RemoveByPrefixAsync(CustomNopModelCacheDefaults.HomePageFeaturedProductsPrefix);
            await _staticCacheManager.RemoveByPrefixAsync(CustomNopModelCacheDefaults.CategoryGroupedProductsPrefix);
        }

        #endregion

        #region Product tags

        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task HandleEventAsync(EntityInsertedEvent<ProductTag> eventMessage)
        {
            await _staticCacheManager.RemoveByPrefixAsync(CustomNopModelCacheDefaults.MemberShipPricePrefixCacheKey);
        }

        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task HandleEventAsync(EntityUpdatedEvent<ProductTag> eventMessage)
        {
            await _staticCacheManager.RemoveByPrefixAsync(CustomNopModelCacheDefaults.MemberShipPricePrefixCacheKey);
        }

        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task HandleEventAsync(EntityDeletedEvent<ProductTag> eventMessage)
        {
            await _staticCacheManager.RemoveByPrefixAsync(CustomNopModelCacheDefaults.MemberShipPricePrefixCacheKey);
        }

        #endregion


        #region specificationAttribute


        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task HandleEventAsync(EntityUpdatedEvent<SpecificationAttribute> eventMessage)
        {
            await _staticCacheManager.RemoveByPrefixAsync(CustomNopModelCacheDefaults.MemberShipPricePrefixCacheKey);
        }

        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task HandleEventAsync(EntityDeletedEvent<SpecificationAttribute> eventMessage)
        {
            await _staticCacheManager.RemoveByPrefixAsync(CustomNopModelCacheDefaults.MemberShipPricePrefixCacheKey);
        }

        #endregion

        #region SpecificationAttributeOption


        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task HandleEventAsync(EntityUpdatedEvent<SpecificationAttributeOption> eventMessage)
        {
            await _staticCacheManager.RemoveByPrefixAsync(CustomNopModelCacheDefaults.MemberShipPricePrefixCacheKey);
        }

        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task HandleEventAsync(EntityDeletedEvent<SpecificationAttributeOption> eventMessage)
        {
            await _staticCacheManager.RemoveByPrefixAsync(CustomNopModelCacheDefaults.MemberShipPricePrefixCacheKey);
        }

        #endregion


        #region ProductSpecificationAttribute


        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task HandleEventAsync(EntityUpdatedEvent<ProductSpecificationAttribute> eventMessage)
        {
            await _staticCacheManager.RemoveAsync(CustomNopModelCacheDefaults.MemberShipPriceOfProduct, eventMessage.Entity.ProductId);
        }

        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task HandleEventAsync(EntityDeletedEvent<ProductSpecificationAttribute> eventMessage)
        {
            await _staticCacheManager.RemoveAsync(CustomNopModelCacheDefaults.MemberShipPriceOfProduct, eventMessage.Entity.ProductId);
        }

        public async Task HandleEventAsync(EntityInsertedEvent<ProductSpecificationAttribute> eventMessage)
        {
            await _staticCacheManager.RemoveAsync(CustomNopModelCacheDefaults.MemberShipPriceOfProduct, eventMessage.Entity.ProductId);
        }

        #endregion

        #region Order

        public async Task HandleEventAsync(EntityInsertedEvent<Order> eventMessage)
        {

            await _staticCacheManager.RemoveByPrefixAsync(CustomNopModelCacheDefaults.BestSellerByCategoryPrefix);

            var customer = await _customerService.GetCustomerByIdAsync(eventMessage.Entity.CustomerId);
            await _staticCacheManager.RemoveAsync(CustomNopCatalogDefaults.CustomerRecentOrderCacheKey, eventMessage.Entity.CustomerId);
            if (customer != null)
            {
                string email = "";
                string name = "";

                if (!string.IsNullOrEmpty(customer.Email))
                    email = customer.Email;
                else
                    email = await _genericAttributeService.GetAttributeAsync<string>(customer, "Email");

                if (!string.IsNullOrEmpty(email))
                {
                    var address = await this._customerService.GetCustomerShippingAddressAsync(customer);
                    string firstName = address.FirstName??customer.FirstName;
                    string lastName = address.LastName ?? customer.LastName;
                    name = (firstName ?? "" + " " + lastName ?? "").Trim();
                    string url = _httpContextAccessor.HttpContext?.Request.Headers["Referer"];
                    string absoluteUrl = _httpContextAccessor.HttpContext?.Request.Headers["Referer"];
                    string userAgent = _httpContextAccessor.HttpContext?.Request.Headers["User-Agent"];
                    if (string.IsNullOrEmpty(name))
                        name = await _genericAttributeService.GetAttributeAsync<string>(eventMessage.Entity, "UserName");

                    await _mailchimpService.CustomerSignup(email, name ?? "", new System.Collections.Generic.List<string>()
                {
                    "SiteCustomer"
                }, url, userAgent);
                }
            }

        }
        public async Task HandleEventAsync(EntityUpdatedEvent<Order> eventMessage)
        {
            await _staticCacheManager.RemoveByPrefixAsync(CustomNopModelCacheDefaults.BestSellerByCategoryPrefix);

        }
        #endregion

        #region FBTProduct

        public async Task HandleEventAsync(EntityInsertedEvent<FBTProduct> eventMessage)
        {
            await _staticCacheManager.RemoveAsync(CustomNopModelCacheDefaults.FBTProductsCacheKey, eventMessage.Entity.ProductId1);

        }
        public async Task HandleEventAsync(EntityUpdatedEvent<FBTProduct> eventMessage)
        {
            await _staticCacheManager.RemoveAsync(CustomNopModelCacheDefaults.FBTProductsCacheKey, eventMessage.Entity.ProductId1);
        }

        public async Task HandleEventAsync(EntityDeletedEvent<FBTProduct> eventMessage)
        {
            await _staticCacheManager.RemoveAsync(CustomNopModelCacheDefaults.FBTProductsCacheKey, eventMessage.Entity.ProductId1);
        }


        #endregion

        #region Product Attribute
        public async Task HandleEventAsync(EntityUpdatedEvent<ProductAttributeMapping> eventMessage)
        {
            var product = await _productService.GetProductByIdAsync(eventMessage.Entity.ProductId);
            await _productService.GetVariantPriceRange(product, true, true);
            await _productService.UpdateProductInventory(product);
            await _staticCacheManager.RemoveAsync(CustomNopCatalogDefaults.ProductVariantsPublishedCacheKey, eventMessage.Entity.ProductId);
            await _staticCacheManager.RemoveAsync(CustomNopCatalogDefaults.ProductVariantByVariantIdCacheKey, eventMessage.Entity.ProductId);
            await _staticCacheManager.RemoveAsync(CustomNopModelCacheDefaults.ProductAttributeValidCombinationsByProductCacheKey, eventMessage.Entity.ProductId);
            await _staticCacheManager.RemoveByPrefixAsync(CustomNopModelCacheDefaults.ProductAttriburePrefixCacheKey, eventMessage.Entity.ProductId);

        }

        public async Task HandleEventAsync(EntityInsertedEvent<ProductAttributeMapping> eventMessage)
        {
            var product = await _productService.GetProductByIdAsync(eventMessage.Entity.ProductId);
            await _productService.GetVariantPriceRange(product, true, true);
            await _productService.UpdateProductInventory(product);
            await _staticCacheManager.RemoveAsync(CustomNopCatalogDefaults.ProductVariantsPublishedCacheKey, eventMessage.Entity.ProductId);
            await _staticCacheManager.RemoveAsync(CustomNopCatalogDefaults.ProductVariantByVariantIdCacheKey, eventMessage.Entity.ProductId);
            await _staticCacheManager.RemoveAsync(CustomNopModelCacheDefaults.ProductAttributeValidCombinationsByProductCacheKey, eventMessage.Entity.ProductId);
            await _staticCacheManager.RemoveByPrefixAsync(CustomNopModelCacheDefaults.ProductAttriburePrefixCacheKey, eventMessage.Entity.ProductId);
        }

        public async Task HandleEventAsync(EntityDeletedEvent<ProductAttributeMapping> eventMessage)
        {
            await _staticCacheManager.RemoveAsync(CustomNopModelCacheDefaults.ProductAttributeValidCombinationsByProductCacheKey, eventMessage.Entity.ProductId);
            await _staticCacheManager.RemoveByPrefixAsync(CustomNopModelCacheDefaults.ProductAttriburePrefixCacheKey, eventMessage.Entity.ProductId);
            var product = await _productService.GetProductByIdAsync(eventMessage.Entity.ProductId);
            await _productService.GetVariantPriceRange(product, true, true);
            await _staticCacheManager.RemoveAsync(CustomNopCatalogDefaults.ProductVariantsPublishedCacheKey, eventMessage.Entity.ProductId);
            await _staticCacheManager.RemoveAsync(CustomNopCatalogDefaults.ProductVariantByVariantIdCacheKey, eventMessage.Entity.ProductId);
        }


        public async Task HandleEventAsync(EntityUpdatedEvent<ProductAttributeValue> eventMessage)
        {

            var mapping = await _productAttributeService.GetProductAttributeMappingByIdAsync(eventMessage.Entity.ProductAttributeMappingId);
            if (mapping != null)
            {

                var product = await _productService.GetProductByIdAsync(mapping.ProductId);
                await _productService.GetVariantPriceRange(product, true, true);
                await _productService.UpdateProductInventory(product);
                await _staticCacheManager.RemoveAsync(CustomNopCatalogDefaults.ProductVariantsPublishedCacheKey, mapping.ProductId);
                await _staticCacheManager.RemoveAsync(CustomNopCatalogDefaults.ProductVariantByVariantIdCacheKey, mapping.ProductId);
                await _staticCacheManager.RemoveByPrefixAsync(CustomNopModelCacheDefaults.ProductAttriburePrefixCacheKey, product.Id);
            }
        }

        public async Task HandleEventAsync(EntityInsertedEvent<ProductAttributeValue> eventMessage)
        {
            var mapping = await _productAttributeService.GetProductAttributeMappingByIdAsync(eventMessage.Entity.ProductAttributeMappingId);
            if (mapping != null)
            {
                var product = await _productService.GetProductByIdAsync(mapping.ProductId);
                await _productService.GetVariantPriceRange(product, true, true);
                await _productService.UpdateProductInventory(product);
                await _staticCacheManager.RemoveAsync(CustomNopCatalogDefaults.ProductVariantsPublishedCacheKey, mapping.ProductId);
                await _staticCacheManager.RemoveAsync(CustomNopCatalogDefaults.ProductVariantByVariantIdCacheKey, mapping.ProductId);
                await _staticCacheManager.RemoveByPrefixAsync(CustomNopModelCacheDefaults.ProductAttriburePrefixCacheKey, product.Id);
            }
        }

        public async Task HandleEventAsync(EntityDeletedEvent<ProductAttributeValue> eventMessage)
        {
            var mapping = await _productAttributeService.GetProductAttributeMappingByIdAsync(eventMessage.Entity.ProductAttributeMappingId);
            if (mapping != null)
            {
                var product = await _productService.GetProductByIdAsync(mapping.ProductId);
                await _productService.GetVariantPriceRange(product, true, true);
                await _productService.UpdateProductInventory(product);
                await _staticCacheManager.RemoveAsync(CustomNopCatalogDefaults.ProductVariantsPublishedCacheKey, mapping.ProductId);
                await _staticCacheManager.RemoveAsync(CustomNopCatalogDefaults.ProductVariantByVariantIdCacheKey, mapping.ProductId);
                await _staticCacheManager.RemoveByPrefixAsync(CustomNopModelCacheDefaults.ProductAttriburePrefixCacheKey, product.Id);
            }

        }

        public async Task HandleEventAsync(EntityUpdatedEvent<ProductAttributeCombination> eventMessage)
        {
            await _staticCacheManager.RemoveAsync(CustomNopModelCacheDefaults.ProductAttributeValidCombinationsByProductCacheKey, eventMessage.Entity.ProductId);
            await _staticCacheManager.RemoveByPrefixAsync(CustomNopModelCacheDefaults.ProductAttributeCombinationPicturePrefixCacheKey);
            var product = await _productService.GetProductByIdAsync(eventMessage.Entity.ProductId);
            await _productService.GetVariantPriceRange(product, false, true);
            await _productService.UpdateProductInventory(product);
            await _staticCacheManager.RemoveByPrefixAsync(CustomNopModelCacheDefaults.ProductAttriburePrefixCacheKey, eventMessage.Entity.ProductId);
        }

        public async Task HandleEventAsync(EntityDeletedEvent<ProductAttributeCombination> eventMessage)
        {
            await _staticCacheManager.RemoveAsync(CustomNopModelCacheDefaults.ProductAttributeValidCombinationsByProductCacheKey, eventMessage.Entity.ProductId);
            await _staticCacheManager.RemoveByPrefixAsync(CustomNopModelCacheDefaults.ProductAttributeCombinationPicturePrefixCacheKey);
            var product = await _productService.GetProductByIdAsync(eventMessage.Entity.ProductId);
            await _productService.GetVariantPriceRange(product, false, true);
            await _staticCacheManager.RemoveByPrefixAsync(CustomNopModelCacheDefaults.ProductAttriburePrefixCacheKey, eventMessage.Entity.ProductId);
        }

        public async Task HandleEventAsync(EntityInsertedEvent<ProductAttributeCombination> eventMessage)
        {
            await _staticCacheManager.RemoveAsync(CustomNopModelCacheDefaults.ProductAttributeValidCombinationsByProductCacheKey, eventMessage.Entity.ProductId);
            await _staticCacheManager.RemoveByPrefixAsync(CustomNopModelCacheDefaults.ProductAttributeCombinationPicturePrefixCacheKey);
            var product = await _productService.GetProductByIdAsync(eventMessage.Entity.ProductId);
            await _productService.GetVariantPriceRange(product, false, true);
            await _productService.UpdateProductInventory(product);
            await _staticCacheManager.RemoveByPrefixAsync(CustomNopModelCacheDefaults.ProductAttriburePrefixCacheKey, eventMessage.Entity.ProductId);
        }


        #endregion

        #region GroupProductConfigurations


        public async Task HandleEventAsync(EntityInsertedEvent<GroupedProductConfiguration> eventMessage)
        {
            await _staticCacheManager.RemoveAsync(CustomNopModelCacheDefaults.AssociatedProductsConfigurationCacheKey, eventMessage.Entity.ProductId);
        }
        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task HandleEventAsync(EntityUpdatedEvent<GroupedProductConfiguration> eventMessage)
        {
            await _staticCacheManager.RemoveAsync(CustomNopModelCacheDefaults.AssociatedProductsConfigurationCacheKey, eventMessage.Entity.ProductId);
        }

        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task HandleEventAsync(EntityDeletedEvent<GroupedProductConfiguration> eventMessage)
        {
            await _staticCacheManager.RemoveAsync(CustomNopModelCacheDefaults.AssociatedProductsConfigurationCacheKey, eventMessage.Entity.ProductId);
        }



        #endregion

        #region Customer

        public async Task HandleEventAsync(CustomerRegisteredEvent eventMessage)
        {
            string url = _httpContextAccessor.HttpContext?.Request.Headers["Referer"];
            string absoluteUrl = _httpContextAccessor.HttpContext?.Request.Headers["Referer"];
            string userAgent = _httpContextAccessor.HttpContext?.Request.Headers["User-Agent"];
            string email = await _customerService.GetCustomerEmail(eventMessage.Customer);
            string name = await _customerService.GetCustomerFullNameAsync(eventMessage.Customer);
            await _mailchimpService.CustomerSignup(email, name ?? "", new System.Collections.Generic.List<string>()
                {
                    "WebRegistered"
                }, url, userAgent);


            await _zohoService.InsertQueuedZohoCustomerAsync(eventMessage.Customer.Id);

        }



        #endregion

        public async Task HandleEventAsync(EntityInsertedEvent<Picture> eventMessage)
        {
            await _staticCacheManager.RemoveByPrefixAsync(CustomNopModelCacheDefaults.QuestionAnswerPicturePrefixCacheKey);
        }

        public async Task HandleEventAsync(EntityUpdatedEvent<Picture> eventMessage)
        {
            await _staticCacheManager.RemoveByPrefixAsync(CustomNopModelCacheDefaults.QuestionAnswerPicturePrefixCacheKey);
        }

        public async Task HandleEventAsync(EntityDeletedEvent<Picture> eventMessage)
        {
            await _staticCacheManager.RemoveByPrefixAsync(CustomNopModelCacheDefaults.QuestionAnswerPicturePrefixCacheKey);
        }

        public async Task HandleEventAsync(EntityUpdatedEvent<QuestionAnswer> eventMessage)
        {
            await _staticCacheManager.RemoveByPrefixAsync(string.Format(CustomNopModelCacheDefaults.QuestionAnswerPicturePrefixCacheKeyById, eventMessage.Entity.Id));

            await _staticCacheManager.RemoveByPrefixAsync(CustomNopModelCacheDefaults.Picture_Url_Pattern_Key);
        }

        #endregion

        #region Address



        #endregion


    }
}
