using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nop.Services.Security;
public partial class StandardPermission
{
    public partial class CustomPermission
    {
        public const string CUSTOM_QA_VIEW = $"{nameof(CustomPermission)}.QAView";
        public const string CUSTOM_QA_CREATE_EDIT_DELETE = $"{nameof(CustomPermission)}.QACreateEditDelete";
        public const string CUSTOM_QA_PRODUCTS_VIEW = $"{nameof(CustomPermission)}.QAProductsView";
        public const string CUSTOM_QA_PRODUCTS_CREATE_EDIT_DELETE = $"{nameof(CustomPermission)}.QAProductsCreateEditDelete";
        public const string CUSTOM_FORM_VIEW = $"{nameof(CustomPermission)}.FORMView";
        public const string CUSTOM_FORM_CREATE_EDIT_DELETE = $"{nameof(CustomPermission)}.FORMCreateEditDelete";

        public const string CUSTOM_FAQ_CATEGORY_ACCESS = $"{nameof(CustomPermission)}.FAQCategoryAccess";
        public const string CUSTOM_FAQ_PRODUCT_ACCESS = $"{nameof(CustomPermission)}.FAQProductAccess";
        public const string CUSTOM_ACCESS_UTILITITES = $"{nameof(CustomPermission)}.AccessUtilities";
        public const string CUSTOM_ACCESS_CUSTOMORDER = $"{nameof(CustomPermission)}.CustomOrder";
        public const string CUSTOM_ACCESS_MANAGEKWTERMS = $"{nameof(CustomPermission)}.ManageKwTerms";
        public const string CUSTOM_ACCESS_MANAGESTAINS = $"{nameof(CustomPermission)}.ManageStains";
        public const string CUSTOM_ACCESS_MANAGEACL = $"{nameof(CustomPermission)}.ManageAcl";
        public const string CUSTOM_ACCESS_CATEGORY_INFO = $"{nameof(CustomPermission)}.AccessCategoryInfo";
        public const string CUSTOM_ACCESS_CATEGORY_DISPLAY = $"{nameof(CustomPermission)}.AccessCategoryDisplay";
        public const string CUSTOM_ACCESS_CATEGORY_MAPPING = $"{nameof(CustomPermission)}.AccessCategoryMappings";
        public const string CUSTOM_ACCESS_CATEGORY_SEO = $"{nameof(CustomPermission)}.AccessCategorySeo";
        public const string CUSTOM_ACCESS_CATEGORY_PRODUCTS = $"{nameof(CustomPermission)}.AccessCategoryProducts";
        public const string CUSTOM_ACCESS_CATEGORY_RELATED_SEARCH = $"{nameof(CustomPermission)}.AccessCategoryRelatedSearch";
        public const string CUSTOM_ACCESS_CATEGORY_QUICK_FILTER = $"{nameof(CustomPermission)}.AccessCategoryQuickFilter";
        public const string CUSTOM_ACCESS_CATEGORY_COLLECTION_LINK = $"{nameof(CustomPermission)}.AccessCategoryCollectionLink";
        public const string CUSTOM_ACCESS_CATEGORY_PRODUCT_FILTER = $"{nameof(CustomPermission)}.AccessCategoryProductsFilter";
        public const string CUSTOM_ACCESS_CATEGORY_SUGGESTED_KEYWORD = $"{nameof(CustomPermission)}.AccessCategorySuggestedKeyword";
        public const string CUSTOM_ACCESS_CATEGORY_FAQ = $"{nameof(CustomPermission)}.AccessCategoryFAQ";
        public const string CUSTOM_ACCESS_CATEGORY_ATC_RECOMMEND = $"{nameof(CustomPermission)}.AccessCategoryATCRecommend";


        #region Product

        public const string CUSTOM_ACCESS_PRODUCT_CREATE = $"{nameof(CustomPermission)}.AccessProductCreate";
        public const string CUSTOM_ACCESS_PRODUCT_DELETE = $"{nameof(CustomPermission)}.AccessProductDelete";
        public const string CUSTOM_ACCESS_PRODUCT_INFO = $"{nameof(CustomPermission)}.AccessProductInfo";
        public const string CUSTOM_ACCESS_PRODUCT_CUSTOM_INFO = $"{nameof(CustomPermission)}.AccessProductCustomInfo";
        public const string CUSTOM_ACCESS_PRODUCT_PRICE = $"{nameof(CustomPermission)}.AccessProductPrice";
        public const string CUSTOM_ACCESS_PRODUCT_SHIPPING = $"{nameof(CustomPermission)}.AccessProductShipping";
        public const string CUSTOM_ACCESS_PRODUCT_INVENTORY = $"{nameof(CustomPermission)}.AccessProductInventory";
        public const string CUSTOM_ACCESS_PRODUCT_MEDIA = $"{nameof(CustomPermission)}.AccessProductMedia";
        public const string CUSTOM_ACCESS_PRODUCT_DIMENSION_PICTURES = $"{nameof(CustomPermission)}.AccessProductDimensionPictures";
        public const string CUSTOM_ACCESS_PRODUCT_ATTRIBUTES = $"{nameof(CustomPermission)}.AccessProductAttributes";
        public const string CUSTOM_ACCESS_PRODUCT_VARIANTS = $"{nameof(CustomPermission)}.AccessProductVariants";
        public const string CUSTOM_ACCESS_PRODUCT_SPECIFICATION_ATTRIBUTES = $"{nameof(CustomPermission)}.AccessProductSpecificationAttributes";
        public const string CUSTOM_ACCESS_PRODUCT_GIFT_CARD = $"{nameof(CustomPermission)}.AccessProductGiftCard";
        public const string CUSTOM_ACCESS_PRODUCT_DOWNLOADABLE = $"{nameof(CustomPermission)}.AccessProductDownloadable";
        public const string CUSTOM_ACCESS_PRODUCT_RENTAL = $"{nameof(CustomPermission)}.AccessProductRental";
        public const string CUSTOM_ACCESS_PRODUCT_RECURRING = $"{nameof(CustomPermission)}.AccessProductRecurring";
        public const string CUSTOM_ACCESS_PRODUCT_SEO = $"{nameof(CustomPermission)}.AccessProductSeo";
        public const string CUSTOM_ACCESS_PRODUCT_FBT_PRODUCTS = $"{nameof(CustomPermission)}.AccessProductFBTProducts";
        public const string CUSTOM_ACCESS_PRODUCT_CROSS_SELLS = $"{nameof(CustomPermission)}.AccessProductCrossSells";
        public const string CUSTOM_ACCESS_PRODUCT_COLLECTION = $"{nameof(CustomPermission)}.AccessProductCollection";
        public const string CUSTOM_ACCESS_PRODUCT_PAIR_WITH = $"{nameof(CustomPermission)}.AccessProductPairWith";
        public const string CUSTOM_ACCESS_PRODUCT_RELATED_SEARCH = $"{nameof(CustomPermission)}.AccessProductRelatedSearch";
        public const string CUSTOM_ACCESS_PRODUCT_SUGGESTED_KEYWORD_MAPPING = $"{nameof(CustomPermission)}.AccessProductSuggestedKeywordMapping";
        public const string CUSTOM_ACCESS_PRODUCT_FAQ = $"{nameof(CustomPermission)}.AccessProductFAQ";
        public const string CUSTOM_ACCESS_PRODUCT_ATC_RECOMMENDATION = $"{nameof(CustomPermission)}.AccessProductATCRecommendation";
        public const string CUSTOM_ACCESS_PRODUCT_INFO_NAME= $"{nameof(CustomPermission)}.AccessProductInfoName";
        public const string CUSTOM_ACCESS_PRODUCT_INFO_DESCRIPTION= $"{nameof(CustomPermission)}.AccessProductInfoDescription";
        #endregion

    }
}

