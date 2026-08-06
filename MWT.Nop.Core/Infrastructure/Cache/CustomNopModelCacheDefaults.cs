using Nop.Core.Caching;

namespace Nop.Web.Infrastructure.Cache
{


    public static partial class CustomNopModelCacheDefaults
    {
        public static CacheKey Picture_Url_Key = new CacheKey("Nop.Question.Answer.Picture.Url-{0}-{1}");

        public const string Picture_Url_Pattern_Key = "Nop.Question.Answer.Picture.Url";
        public static CacheKey NewArrivalCacheKey => new CacheKey("Nop.pres.sitemap.seo-NewArrival");
        public static CacheKey MemberShipPriceOfProduct => new CacheKey("Nop.pres.membershipprice.byproduct.-{0}");
        public static string MemberShipPricePrefixCacheKey => "Nop.pres.membershipprice.byproduct";

        public static CacheKey CategoryGroupedProductsCacheKey => new CacheKey("Nop.pres.Category.GroupedProducts-{0}");
        public static string CategoryGroupedProductsPrefix => "Nop.pres.Category.GroupedProducts";

        public static CacheKey BestSellerByCategoryCacheKey => new CacheKey("Nop.pres.BestSeller.ByCategory-{0}-{1} ");

        public static CacheKey BestSellerByKwTermCacheKey => new CacheKey("Nop.pres.BestSeller.ByKwTerm-{0}-{1} ");

        public static string BestSellerByCategoryPrefix => "Nop.pres.BestSeller.ByCategory";
        public static CacheKey CustomProductDetailsPicturesModelKey => new CacheKey("Nop.pres.product.picture-{0}-{1}-{2}-{3}-{4}-{5}-{6}");

        public static CacheKey CustomProductDefaultPictureModelKey => new CacheKey("Nop.pres.product.detailspictures-{0}-{1}-{2}-{3}-{4}-{5}-{6}");

        public static CacheKey FBTProductsCacheKey => new CacheKey("Nop.pres.FBTproducts-{0} "     );
        public static string FBTProductsPrefix => "Nop.pres.FBTproducts";
        public static string FBTPrefix => "Nop.fbtproduct.byproduct";
        public static CacheKey TopsellerCacheKey => new CacheKey("Nop.pres.TopSeller-{0}");

        public static string ProductListingByProductIdsPrefix => "Nop.pres.product.Listing.by.productids";
        public static CacheKey ProductListingByProductIds => new CacheKey("Nop.pres.product.Listing.by.productids-{0}");

        public static CacheKey HomePageFeaturedProductsCacheKey => new CacheKey("Nop.pres.product.home.featured.products-{0}");

        public static string HomePageFeaturedProductsPrefix => "Nop.pres.product.home.featured.products";

        public static CacheKey ProductReviews => new CacheKey("Nop.pres.product.reviews-{0}-{1}-{2}");

        public static string RecommendationProductsPrefix => "Nop.pres.recommendation.products-{0}";
        public static CacheKey RecommendationProductsCacheKey => new CacheKey("Nop.pres.recommendation.products-{0}-{1}");
        public static string CrossSellProductsPrefix => "Nop.pres.CrossSell.Products";
        public static CacheKey CrossSellProductsCacheKey => new CacheKey("Nop.pres.CrossSell.Products-{0}");
        #region Category

        public static CacheKey CustomCategoryHomepageKey => new CacheKey("Nop.pres.category.homepage.custom-{0}-{1}-{2}-{3}-{4}");
        public static string QuickFiltersPrefix => "Nop.pres.Quick.Filters";
        public static CacheKey QuickFilterCacheKey => new CacheKey("Nop.pres.Quick.Filters-{0}-{1}");

        public static CacheKey CategoryTopSellingProductsCacheKey => new CacheKey("Nop.pres.Category.TopSellingProducts-{0}-{1}");
        public static string RelatedSearchTermPrefix => "Nop.pres.Related.SearchTerm";
        public static CacheKey RelatedSearchTermCacheKey => new CacheKey("Nop.pres.Related.SearchTerm-{0}-{1}");

        #endregion
        #region Search

        public static CacheKey SearchResultProductsDefaultCacheKey => new CacheKey("Nop.pres.Search_result.Default-{0}");
        public static CacheKey SearchResultProductsAlternateCacheKey => new CacheKey("Nop.pres.Search_result.Alternate-{0} ");
        public static CacheKey SearchResultProductsSuggestedCacheKey => new CacheKey("Nop.pres.Search_result.Sub.Alternate-{0} ");
        public static string SeacrhResultProductPrefix => "Nop.pres.Search_result";
        #endregion

        #region Associated Products
        public static CacheKey AssociatedProductsConfigurationCacheKey => new CacheKey(
            "Nop.pres.AssociatedProducts.Configuration-{0}");
        #endregion

        #region SiteMap
        public static CacheKey SitemapImageModelKey => new CacheKey("Nop.pres.sitemap.Image-{0}-{1}-{2}-{3}");
        public static CacheKey KwTermSitemapSeoModelKey => new CacheKey("Nop.pres.sitemap.kwterm-{0}-{1}-{2}-{3}");
        public static CacheKey QuestionAnswerSitemapSeoModelKey => new CacheKey("Nop.pres.sitemap.questionanswer-{0}-{1}-{2}-{3}");

        #endregion

        #region Question Answer
        public static CacheKey QuestionAnswerPictureModelKey => new CacheKey("Nop.pres.QuestionAnswer.picture-{0}-{1}-{2}-{3}-{4}-{5}");
        public static string QuestionAnswerPicturePrefixCacheKey => "Nop.pres.QuestionAnswer.picture";
        public static string QuestionAnswerPicturePrefixCacheKeyById => "Nop.pres.QuestionAnswer.picture-{0}-";

        #endregion

        #region Sitemap

        public static CacheKey CustomSitemapSeoModelKey => new CacheKey("Nop.pres.sitemap.seo-{0}-{1}-{2}");
        #endregion

        #region Faq

        public static string FaqPrefix => "Nop.pres.Faq";
        public static CacheKey FaqCacheKey => new CacheKey("Nop.pres.Faq-{0}-{1}");

        #endregion

        public static CacheKey ProductAttributePictureGalleryModelKey => new CacheKey("Nop.pres.productattribute.picture.Gallery-{0}-{1}-{2}");
        public static CacheKey ProductAttributeCombinationPictureGalleryModelKey => new CacheKey("Nop.pres.productattribute.combination.picture.Gallery-{0}-{1}-{2}");
        public static string ProductAttributeCombinationPicturePrefixCacheKey => "Nop.pres.productattribute.combination.picture";
        public static CacheKey ProductAttributeModelKey => new CacheKey("Nop.pres.productattribute-{0}-{1}-{2}-{3}-{4}");
        public static CacheKey FeaturedProductAttributeModelKey => new CacheKey("Nop.pres.productattribute-{0}-{1}-{2}-{3}-{4}-Featured");
        public static string ProductAttriburePrefixCacheKey => "Nop.pres.productattribute-{0}";
        public static CacheKey ProductAttributeValidCombinationsByProductCacheKey => new CacheKey("Nop.pres.productattributecombination.valid.byproduct.{0}");

        #region Cart
        public static CacheKey CartGalleryPictureModelKey => new CacheKey("Nop.pres.cart.picture-gallery-{0}-{1}-{2}-{3}-{4}-{5}-{6}");

        #endregion
    }
}
