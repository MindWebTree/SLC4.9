using MWT.Nop.Core.Domain.KW;
using Nop.Core.Caching;
using Nop.Services.Catalog;

namespace MWT.Nop.Core.Service.Catalog
{
    /// <summary>
    /// Represents default values related to catalog services
    /// </summary>
   


    public static partial class CustomNopCatalogDefaults
    {
        #region Products
        public static CacheKey ProductSaleInfoCacheKey => new CacheKey("Nop.product.Sale.Info.{0}");
        public static string ProductSaleInfoPrefix => "Nop.product.Sale.Info";
        public static CacheKey ProductsBasicInfoCacheKey => new CacheKey("Nop.product.Basic.Info.All");
        public static CacheKey CustomRelatedProductsCacheKey => new CacheKey("Nop.relatedproduct.byproduct.{0}-{1}");
        public static string RelatedPrefix => "Nop.relatedproduct.byproduct";
        public static string CollectionPrefix => "Nop.collectionproduct.byproduct";
        public static string CollectionProductsPrefix => "Nop.collectionproduct.byproduct.{0}";
        /// </remarks>
        public static CacheKey collectionProductsCacheKey => new CacheKey("Nop.collectionproduct.byproduct.{0}-{1}");

        public static CacheKey collectionProductsIdsCacheKey => new CacheKey("Nop.collectionproduct.byproduct.{0}-{1}.ids");
        public static string PairWithPrefix => "Nop.pairwithproduct.byproduct";
        public static string PairWithProductsPrefix => "Nop.pairwithproduct.byproduct.{0}";
        /// </remarks>
        public static CacheKey PairWithProductsCacheKey => new CacheKey("Nop.pairwithproduct.byproduct.{0}-{1}");

        public static CacheKey PairWithProductsIdsCacheKey => new CacheKey("Nop.pairwithproduct.byproduct.{0}-{1}.ids");

        public static CacheKey ProductDiscountCacheKey => new CacheKey("Nop.totals.productprice.{0}-{1}-{2}-{3}-{4}-{5}-{6}-{7}" );
        public static CacheKey ProductTemplateSectionsCacheKey => new CacheKey("Nop.product.Template.Sections.{0}");
        /// <summary>
        /// Gets a key pattern to clear cache
        /// </summary>
        /// <remarks>
        /// {0} : product id
        /// </remarks>



        #endregion

        #region RelatedSearch

        public static string ReleatedSearchPrefix => "Nop.ReleatedSearch.byentity.{0}";
        public static CacheKey ReleatedSearchCacheKey => new CacheKey("Nop.ReleatedSearch.byentity.{0}-{1}");

        public static CacheKey ReleatedSearchByProductIdsCacheKey => new CacheKey("Nop.ReleatedSearch.by.productIds.{0}");

        public static CacheKey ReleatedSearchDefaultForSearchpageCacheKey => new CacheKey("Nop.ReleatedSearch.deafult.forsearchPage");

        #endregion

        #region QuickFilter

        public static string QuickFilterPrefix => "Nop.QuickFilter.byentity";
        public static CacheKey QuickFilterCacheKey => new CacheKey("Nop.QuickFilter.byentity.{0}-{1}");

        #endregion


        #region CategoryCollectionlinks

        public static string CategoryCollectionLinkPrefix => "Nop.CategoryCollectionLink.byentity";
        public static CacheKey CategoryCollectionLinkCacheKey => new CacheKey("Nop.CategoryCollectionLink.byentityId.{0}");

        #endregion

        #region CategoriesFilters

        public static string FilterMappingByEntityPrefix => "Nop.FilterMapping.byentity.{0}";
        public static CacheKey FilterMappingByEntitybyfilterTypeCacheKey => new CacheKey("Nop.FilterMapping.byentity.{0}-{1}-{2}");
        public static CacheKey FilterMappingByEntityCacheKey => new CacheKey("Nop.FilterMapping.byentity.{0}-{1}");
        #endregion

        #region Specification Attribute

        public static CacheKey MainCategorySpecificationAttributeByProductCacheKey => new CacheKey("Nop.productspecificationattribute.byproduct.MainCategory.{0}-{1}");

        public static string SpecificationAttributesByGroupPrefix => "Nop.SpecificationAttribute-";
        public static CacheKey SpecificationAttributesByGroup => new CacheKey("Nop.SpecificationAttribute-{0}");

        public static string AllSpecificationOptionsPrefix => "Nop.SpecificationOptions.All";
        public static CacheKey AllSpecificationOptions => new CacheKey("Nop.SpecificationOptions.All");
        #endregion

        #region Membership

        public static CacheKey MemberShipPriceOfProduct => new CacheKey("Nop.pres.membershipprice.byproduct.-{0}");
        public static string MemberShipPricePrefixCacheKey => "Nop.pres.membershipprice.byproduct";


        #endregion

        #region FBQ Products
        public static CacheKey FBTProductsCacheKey => new CacheKey("Nop.fbtproduct.byproduct.{0}-{1}");

        /// <summary>
        /// Gets a key pattern to clear cache
        /// </summary>
        /// <remarks>
        /// {0} : product ID
        /// </remarks>
        public static string FBTProductsPrefix => "Nop.fbtproduct.byproduct.{0}";
        public static string FBTPrefix => "Nop.fbtproduct.byproduct";
        #endregion

        #region Search
        public static CacheKey SearchDefaultAutoCompleteCacheKey => new CacheKey("Nop.pres.sitemap.DefaultSearch.AutoComplete-{0}-{1}");
        public static string SearchDefaultAutoCompletePrefix => "Nop.pres.sitemap.DefaultSearch.AutoComplete-{0}";
        #endregion

        #region Categories
        public static CacheKey ProductCategoriesByProductExcludingRootLevelCategoriesCacheKey => new CacheKey("Nop.productcategory.byproduct.{0}-{1}-{2}-{3}-{4}");

        public static CacheKey CategorySuggestedKeywordsCacheKey => new CacheKey("Nop.Category.SuggestedKeywords");
        #endregion


        #region Offer

        public static string StoreWideProductDiscountInfoPrefix => "Nop.Product.StoreWideProductDiscountInfo";
        public static CacheKey StoreWideProductDiscountInfoCacheKey => new CacheKey("Nop.Product.StoreWideProductDiscountInfo.{0}");

        #endregion

        #region Kwterms
        public static CacheKey ProductKwTermsByProductExcludingRootLevelKwTermsCacheKey => new CacheKey("Nop.productkwterms.byproduct.{0}-{1}-{2}-{3}-{4}");
        public static string ProductKwTermsByProductPrefix => "Nop.productkwterms.byproduct.{0}";
        public static CacheKey ProductKwTermsByProductCacheKey => new CacheKey("Nop.productkwterms.byproduct.{0}-{1}-{2}-{3}");

        public static CacheKey KwTermsAllCacheKey => new CacheKey("Nop.KwTerms.all.{0}-{1}-{2}");

        public static CacheKey KwTermsByParentKwTermsCacheKey => new CacheKey("Nop.kwterms.byparent.{0}-{1}-{2}-{3}");
        public static string KwTermsByParentKwTermsPrefix => "Nop.kwterms.byparent.{0}";

        public static CacheKey KwTermsHomepageWithoutHiddenCacheKey => new CacheKey("Nop.kwterms.homepage.withouthidden-{0}-{1}");
        public static string KwTermsHomepagePrefix => "Nop.kwterms.homepage.withouthidden.{0}";

        public static CacheKey KwTermsHomepageCacheKey => new CacheKey("Nop.kwterms.homepage.");
        public static string KwTermsHomepageCache => "Nop.kwterms.homepage.{0}";

        public static CacheKey KwTermsChildIdsCacheKey => new CacheKey("Nop.kwterms.childids.{0}-{1}-{2}-{3}");
        public static string KwTermsChildIdsPrefix => "Nop.kwterms.childids.{0}";

        public static CacheKey KwTermsBreadcrumbCacheKey => new CacheKey("Nop.kwterms.breadcrumb.{0}-{1}-{2}-{3}");
        public static string KwTermsBreadcrumbPrefix => "Nop.kwterms.breadcrumb.{0}";


        public static CacheKey KwTermsCategoriesCacheKey => new CacheKey("Nop.KwTerm.Categories.{0}-{1}");

        /// <summary>
        /// Gets a key pattern to clear cache
        /// </summary>
        /// <remarks>
        /// {0} : parent category ID
        /// </remarks>
        public static string KwTermsCategoriesCachePrefix => "Nop.KwTerm.Categories";


        #endregion

        #region QuestionAnswers
        public static CacheKey QuestionAnswersAllCacheKey => new CacheKey("Nop.QuestionAnswers.all.{0}-{1}-{2}");
        public static CacheKey QuestionAnswersBreadcrumbCacheKey => new CacheKey("Nop.QuestionAnswers.breadcrumb.{0}-{1}-{2}-{3}");
        public static string QuestionAnswersBreadcrumbPrefix => "Nop.QuestionAnswers.breadcrumb.{0}";
        public static CacheKey RelatedQuestionAnswersCacheKey => new CacheKey("Nop.relatedquestionanswer.byquestionanswer.{0}-{1}");
        public static string RelatedQuestionAnswersPrefix => "Nop.relatedquestionanswer.byquestionanswer.{0}";

        #endregion
        #region AbandonedCard

        public static CacheKey AbandonedCartReminderCacheKey => new CacheKey("Nop.AbandonedCart.Reminder");

        #endregion

        #region Order
        public static CacheKey CustomerRecentOrderCacheKey => new CacheKey("Nop.Order.Recent.Customer.{0}");

        #endregion


        #region Variant


        public static CacheKey ProductVariantsCacheKey => new CacheKey("Nop.product.variants.{0}");
        public static CacheKey ProductVariantsPublishedCacheKey => new CacheKey("Nop.product.variants.Published.{0}");
        public static CacheKey ProductVariantByVariantIdCacheKey => new CacheKey("Nop.product.variant.by.variantid.{0}");

        #endregion
    }
}
