using Nop.Core.Domain.Catalog;
using Nop.Web.Areas.Admin.Models.Catalog;
using Nop.Web.Areas.Admin.Models.Customization.Custom;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nop.Web.Areas.Admin.Factories
{
    /// <summary>
    /// Represents the category model factory implementation
    /// </summary>
    public partial interface ICategoryModelFactory 
    {
        Task<CategoryListModel> PrepareCustomCategoryListModelAsync(CategorySearchModel searchModel);
        Task<FiltersMappingByEntityListModel> PrepareFiltersMappingByEntityListModelAsync(FiltersMappingByEntitySearchModel searchModel, int entityId, string entityType, string filterType);
        Task<AddProductToCategoryListModel> CustomPrepareAddProductToCategoryListModelAsync(AddProductToCategorySearchModel searchModel);
        Task<CategoryProductListModel> CustomPrepareCategoryProductListModelAsync(CategoryProductSearchModel searchModel, Category category);
        Task<CategorySpecificationOptionProductListModel> CustomPrepareCategorySpecificationOptionMappingAddPopupListModelAsync(CategorySpecificationOptionProductSearchModel searchModel, Category category);
        #region SuggestedKeyword

        Task<CategorySuggestedKeywordListModel> CustomPrepareSuggestedKeywordListModelAsync(CategorySuggestedKeywordSearchModel searchModel, int categoryId);
        Task<CategorySuggestedKeywordModel> GetCategorySuggestedKeywordById(int Id);
        Task CreateCategorySuggestedKeyword(CategorySuggestedKeywordModel categorySuggestedKeywordModel);
        Task UpdateCategorySuggestedKeyword(CategorySuggestedKeywordModel categorySuggestedKeywordModel);
        Task DeleteCategorySuggestedKeyword(CategorySuggestedKeywordModel categorySuggestedKeywordModel);
        Task<bool> IsCategoryKeyWordExist(int keywordId, string keyWord);

        #endregion
    }
}
