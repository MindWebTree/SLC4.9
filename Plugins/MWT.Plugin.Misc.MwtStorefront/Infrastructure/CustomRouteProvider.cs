    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Routing;
using MWT.Plugin.Misc.MwtStorefront.Infrastructure;
using Nop.Services.Installation;
    using Nop.Web.Framework.Mvc.Routing;
    using Nop.Web.Infrastructure;

    namespace MWT.Nop.Plugin.Widgets.Catalog.Infrastructure
    {
        /// <summary>
        /// Represents provider that provided basic routes
        /// </summary>
        public partial class CustomRouteProvider : BaseRouteProvider, IRouteProvider
        {
            #region Methods

            /// <summary>
            /// Register routes
            /// </summary>
            /// <param name="endpointRouteBuilder">Route builder</param>
            public void RegisterRoutes(IEndpointRouteBuilder endpointRouteBuilder)
            {
                var lang = GetLanguageRoutePattern();

                endpointRouteBuilder.MapControllerRoute(name: "GetCustomCategoryProducts",
                  pattern: $"category/products/",
                  defaults: new { controller = "Catalog", action = "GetCustomCategoryProducts" });

                endpointRouteBuilder.MapControllerRoute(name: "CustomOrders",
                 pattern: $"{lang}/order/history",
                 defaults: new { controller = "Order", action = "CustomOrders" });

                endpointRouteBuilder.MapControllerRoute(name: "CustomAddProductToCart-Details",
                  pattern: $"addproducttocart/details/{{productId:min(0)}}/{{shoppingCartTypeId:min(0)}}/{{formId?}}",
                  defaults: new { controller = "ShoppingCart", action = "CustomAddProductToCart_Details" });


                endpointRouteBuilder.MapControllerRoute(name: "CustomAddProductToCart-Collections",
                  pattern: $"addproducttocart/Collections/{{productIds}}/{{shoppingCartTypeId:min(0)}}/{{isFbt?}}/{{formId?}}",
                  defaults: new { controller = "ShoppingCart", action = "CustomAddProductToCart_Collection" });

                endpointRouteBuilder.MapControllerRoute(name: "CartItem-Delete",
                pattern: $"CartItem/Delete/{{shoppingCartRecId:min(0)}}/{{shoppingCartTypeId:min(0)}}",
                defaults: new { controller = "ShoppingCart", action = "DeleteCartItem" });

                endpointRouteBuilder.MapControllerRoute("Product", $"{lang}/product/{{id:min(1)}}/{{SeName?}}",
                   new { controller = "Product", action = "CustomProductDetails" });

                endpointRouteBuilder.MapControllerRoute("Product", $"{lang}/product/overview/{{id:min(1)}}",new { controller = "Product", action = "ProductOverview" });

                endpointRouteBuilder.MapControllerRoute("Product", $"{lang}/product/overview/{{id:min(1)}}",
              new { controller = "Product", action = "ProductOverview" });

                endpointRouteBuilder.MapControllerRoute("ProductwithVariantId", $"{lang}/product/{{productId}}/{{SeName?}}",
     new { controller = "Product", action = "CustomProductDetailsWithVariantId" });

                endpointRouteBuilder.MapControllerRoute("Category", $"{lang}/category/{{id:min(1)}}/{{SeName?}}",
                  new { controller = "Catalog", action = "CustomCategory" });

                endpointRouteBuilder.MapControllerRoute(name: "Manufacturer", $"{lang}/manufacturer/{{id:min(1)}}/{{SeName?}}",
                     defaults: new { controller = "Catalog", action = "CustomManufacturer" });

                endpointRouteBuilder.MapControllerRoute(name: "ProductSearchAutoComplete",
          pattern: $"catalog/searchtermautocomplete",
          defaults: new { controller = "Catalog", action = "CustomSearchTermAutoComplete" });

                endpointRouteBuilder.MapControllerRoute(name: "NewProductsRSS",
                  pattern: $"newproducts/rss",
                  defaults: new { controller = "Product", action = "CustomNewProductsRss" });

                endpointRouteBuilder.MapControllerRoute(name: "AddProductToCart-Catalog",
                pattern: $"addproducttocart/catalog/{{productId:min(0)}}/{{shoppingCartTypeId:min(0)}}/{{quantity:min(0)}}",
                defaults: new { controller = "ShoppingCart", action = "CustomAddProductToCart_Catalog" });

                endpointRouteBuilder.MapControllerRoute(name: "Sitemap",
                   pattern: $"{lang}/sitemap",
                   defaults: new { controller = "SiteMap", action = "ExtendedSitemap" });

                endpointRouteBuilder.MapControllerRoute(name: "sitemap.xml",
                  pattern: $"sitemap.xml",
                  defaults: new { controller = "SiteMap", action = "ExtendedSitemapXml" });

                 endpointRouteBuilder.MapControllerRoute(name: "picturesitemap.xml",
               pattern: $"picturesitemap.xml",
               defaults: new { controller = "SiteMap", action = "GeneratePictureSitemapXml" });

                endpointRouteBuilder.MapControllerRoute(name: "CheckoutOnePage",
           pattern: $"{lang}/onepagecheckout/",
           defaults: new { controller = "Checkout", action = "CustomOnePageCheckout" });

                endpointRouteBuilder.MapControllerRoute(name: "ProductSearchCustom",
                    pattern: $"{lang}/search/",
                    defaults: new { controller = "Catalog", action = "CustomSearch" });

                endpointRouteBuilder.MapControllerRoute(name: "CustomizationForm",
            pattern: $"customizationform/{{productId:min(0)}}",
            defaults: new { controller = "CustomizationForm", action = "CustomizationForm" });

                endpointRouteBuilder.MapControllerRoute(name: "CustomOrderDetails",
        pattern: $"{lang}/orderdetails/{{orderId:min(0)}}",
        defaults: new { controller = "Order", action = "CustomDetails" });


                endpointRouteBuilder.MapControllerRoute(name: "CheckoutCompleted",
                pattern: $"{lang}/checkout/completed/{{orderId:int?}}",
                defaults: new { controller = "Checkout", action = "CustomCompleted" });

                endpointRouteBuilder.MapControllerRoute(name: "Reviews",
                pattern: $"{lang}/reviews/{{productId}}",
                defaults: new { controller = "Product", action = "Reviews" });

                endpointRouteBuilder.MapControllerRoute(name: "CustomSubscribeNewsletter",
                  pattern: $"customsubscribenewsletter",
                  defaults: new { controller = "Newsletter", action = "CustomSubscribeNewsletter" });

                endpointRouteBuilder.MapControllerRoute(name: "api-orders",
                  pattern: $"api/orders",
                  defaults: new { controller = "OrderApi", action = "Get" });

                endpointRouteBuilder.MapControllerRoute(name: "api-order-update",
             pattern: $"api/order",
             defaults: new { controller = "OrderApi", action = "Update" });

                endpointRouteBuilder.MapControllerRoute(name: "api-order-details",
          pattern: $"api/order/details",
          defaults: new { controller = "OrderApi", action = "GetOrdDetails" });

                endpointRouteBuilder.MapControllerRoute(name: "api-customer-recent-order", pattern: $"/api/customers/orders/recent-delivered",
             defaults: new { controller = "OrderApi", action = "GetLatestDeliveredOrderIdByCustomerEmail" });

                endpointRouteBuilder.MapControllerRoute(name: "api-Mark-Order-As-Delivered",
     pattern: $"api/order/deliver",
     defaults: new { controller = "OrderApi", action = "Deliver" });

                endpointRouteBuilder.MapControllerRoute(name: "api-Send-Additional-Service-Notification",
          pattern: $"api/order/SendAdditionalServiceNotification",
          defaults: new { controller = "OrderApi", action = "SendAdditionalWgsServiceInvoice" });

                endpointRouteBuilder.MapControllerRoute(name: "api-Get-Status-Of-Additional-WgsService",
       pattern: $"api/order/GetStatusOfAdditionalWgsService",
       defaults: new { controller = "OrderApi", action = "GetStatusOfAdditionalWgsService" });


                endpointRouteBuilder.MapControllerRoute(name: "api-Get-Shipping-Methods",
    pattern: $"api/shipping-charges",
    defaults: new { controller = "OrderApi", action = "GetShippingMethods" });


                endpointRouteBuilder.MapControllerRoute(name: "product-feed",
      pattern: $"api/products/feed",
      defaults: new { controller = "ProductApi", action = "Feed" });

                endpointRouteBuilder.MapControllerRoute(name: "api-products",
    pattern: $"api/products/",
    defaults: new { controller = "ProductApi", action = "Products" });


                endpointRouteBuilder.MapControllerRoute(name: "api-products-main-image",
    pattern: $"api/product/{{productId:min(0)}}/image",
    defaults: new { controller = "ProductApi", action = "GetProductImage" });

                endpointRouteBuilder.MapControllerRoute(name: "api-variant-surcharge-applicable",
    pattern: $"/api/variants/{{variantid:min(0)}}/surcharge/applicable",
    defaults: new { controller = "ProductApi", action = "IsVariantSurchargeApplicable" });

                endpointRouteBuilder.MapControllerRoute(name: "api-product-variants",
    pattern: $"/api/product/variants",
    defaults: new { controller = "ProductApi", action = "GetProductVariants" });
                endpointRouteBuilder.MapControllerRoute(name: "api-manufacturer",
    pattern: $"/api/manufacturers",
    defaults: new { controller = "CatalogApi", action = "GetManufacturers" });

                endpointRouteBuilder.MapControllerRoute(name: "RebuildCart",
                  pattern: $"rebuildcart/{{invoiceId}}",
                  defaults: new { controller = "RebuildCart", action = "Index" });

                endpointRouteBuilder.MapControllerRoute(name: "Cart-Share",
                        pattern: $"cart/share/{{customerGuid?}}",
                        defaults: new { controller = "ShoppingCart", action = "Share" });

                endpointRouteBuilder.MapControllerRoute(name: "AbandonedList",
                pattern: $"abandonedcarts",
                defaults: new { controller = "RebuildCart", action = "AbandonedCardList" });


                endpointRouteBuilder.MapControllerRoute(name: "BackInStockSubscription",
            pattern: $"BackInStockSubscription",
            defaults: new { controller = "Product", action = "BackInStockSubscription" });

                endpointRouteBuilder.MapControllerRoute(name: "KWTerm", $"{lang}/kw/{{SeName?}}",
                  new { controller = "KwTerm", action = "Index" });

                endpointRouteBuilder.MapControllerRoute(name: "GetCustomCategoryProducts",
                pattern: $"kwterm/products/",
                defaults: new { controller = "KwTerm", action = "GetCustomCategoryProducts" });

                endpointRouteBuilder.MapControllerRoute(name: "QuestionAnswer", $"{lang}/answers/{{SeName}}",
                 new { controller = "QuestionAnswer", action = "Index" });

                endpointRouteBuilder.MapControllerRoute(name: "GetQuestionAnswerProducts",
                pattern: $"answer/products/",
                defaults: new { controller = "QuestionAnswer", action = "GetQuestionAnswerProducts" });


                endpointRouteBuilder.MapControllerRoute(name: "CustomWishlist",
                  pattern: $"{lang}/wishlist/{{customerGuid?}}",
                  defaults: new { controller = "ShoppingCart", action = "CustomWishlist" });



                endpointRouteBuilder.MapControllerRoute(name: "CustomerInfo",
                       pattern: $"{lang}/customer/info",
                       defaults: new { controller = "CustomerExtended", action = "CustomInfo" });

                endpointRouteBuilder.MapControllerRoute(name: "CustomerAddressAdd",
                    pattern: $"{lang}/customer/addressadd",
                    defaults: new { controller = "CustomerExtended", action = "CustomAddressAdd" }); ;


                endpointRouteBuilder.MapControllerRoute(name: "CustomerAddressEdit",
                       pattern: $"{lang}/customer/addressedit/{{addressId:min(0)}}",
                       defaults: new { controller = "CustomerExtended", action = "CustomAddressEdit" });


                endpointRouteBuilder.MapControllerRoute(name: "CartItem-Update",
                pattern: $"cartitem/update/{{shoppingCartId:min(0)}}/{{shoppingCartTypeId:min(0)}}",
                defaults: new { controller = "ShoppingCart", action = "UpdateCartItem" });

                endpointRouteBuilder.MapControllerRoute(name: "ReInitiateCheckout",
              pattern: $"ReInitiateCheckout/{{customerid}}",
              defaults: new { controller = "ReInitiateCheckout", action = "Index" });

                endpointRouteBuilder.MapControllerRoute(name: "RewardClaim",
       pattern: $"{lang}/rewardclaim",
       defaults: new { controller = "Reward", action = "Index" });

            #region ProductTag
            endpointRouteBuilder.MapControllerRoute(
              name: "TagCategoryProducts",
              pattern: $"tagcatalog/products",
              defaults: new
              {
                  controller = "TagCategory",
                  action = "GetProducts"
              }
          );
            endpointRouteBuilder.MapControllerRoute(
                name: "TagCategoryPage",
                pattern: "/{tagSlug}/{segmentSlug}",
                defaults: new { controller = "TagCategory", action = "Index" },
                constraints: new { tagSlug = new TagSlugConstraint() }
            );



            endpointRouteBuilder.MapControllerRoute(
                name: "TagPage",
                pattern: "{tagSlug}",
                defaults: new
                {
                    controller = "TagCategory",
                    action = "Index",
                    segmentSlug = "all"
                },
                constraints: new { tagSlug = new TagSlugConstraint() }
            );

            endpointRouteBuilder.MapControllerRoute(name: "tags.xml",
         pattern: $"tags.xml",
         defaults: new { controller = "SiteMap", action = "GenerateTagSitemapXml" });
            #endregion
            #region  SiteMap Version2



            endpointRouteBuilder.MapControllerRoute(name: "Sitemap_Products",
                 pattern: $"products_sitemap.xml",
                 defaults: new { controller = "SiteMap", action = "GenerateProductSitemapXml" });

                endpointRouteBuilder.MapControllerRoute(name: "Sitemap_Pages",
              pattern: $"pages_sitemap.xml",
              defaults: new { controller = "SiteMap", action = "GeneratePageSitemapXml" });

                endpointRouteBuilder.MapControllerRoute(name: "Sitemap_Categories",
        pattern: $"categories_sitemap.xml",
        defaults: new { controller = "SiteMap", action = "GenerateCategorySitemapXml" });

                endpointRouteBuilder.MapControllerRoute(name: "Sitemap_QuestionAnswer",
    pattern: $"qa_sitemap.xml",
    defaults: new { controller = "SiteMap", action = "GenerateQuestionAnswerSitemapXml" });

                endpointRouteBuilder.MapControllerRoute(name: "Sitemap_KwTerm",
    pattern: $"kw_sitemap.xml",
    defaults: new { controller = "SiteMap", action = "GenerateKwTermSitemapXml" });


                endpointRouteBuilder.MapControllerRoute(name: "google_feed",
    pattern: $"googlefeed.xml",
    defaults: new { controller = "Common", action = "CustomGoogleFeed" });


                endpointRouteBuilder.MapControllerRoute(name: "LandingPage", $"{lang}/landingpage/{{SeName?}}",
                  new { controller = "LandingPage", action = "Index" });

                #endregion

            }


            #endregion

            #region Properties

            /// <summary>
            /// Gets a priority of route provider
            /// </summary>
            public int Priority => 1;

            #endregion
        }
    }
