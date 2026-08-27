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
        public const string CUSTOM_QA_PRODUCTS_CREATE_EDIT_DELETE = $"{nameof(Catalog)}.QAProductsCreateEditDelete";
        public const string CUSTOM_FORM_VIEW = $"{nameof(CustomPermission)}.FORMView";
        public const string CUSTOM_FORM_CREATE_EDIT_DELETE = $"{nameof(CustomPermission)}.FORMCreateEditDelete";
         
        public const string CUSTOM_FAQ_CATEGORY_ACCESS= $"{nameof(CustomPermission)}.FAQCategoryAccess";
        public const string CUSTOM_FAQ_PRODUCT_ACCESS= $"{nameof(CustomPermission)}.FAQProductAccess";
        public const string CUSTOM_ACCESS_UTILITITES = $"{nameof(CustomPermission)}.AccessUtilities";
        public const string CUSTOM_ACCESS_CUSTOMORDER = $"{nameof(CustomPermission)}.CustomOrder";
        public const string CUSTOM_ACCESS_MANAGEKWTERMS = $"{nameof(CustomPermission)}.ManageKwTerms";
        public const string CUSTOM_ACCESS_MANAGESTAINS = $"{nameof(CustomPermission)}.ManageStains";
        public const string CUSTOM_ACCESS_MANAGEACL= $"{nameof(CustomPermission)}.ManageAcl";
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




    }
}

