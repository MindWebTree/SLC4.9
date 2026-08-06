using MWT.Nop.Core.Domain;
using Nop.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MWT.Nop.Core.Services.Catalog
{
    public partial interface ISuggestedKeywordsService
    {

        Task UpdateSuggestedKeyWordAsync(SuggestedKeyword suggestedKeywords);
        Task DeleteSuggestedKeyWordAsync(SuggestedKeyword suggestedKeywords);
        Task InsertSuggestedKeyWordAsync(SuggestedKeyword suggestedKeywords);
        Task<SuggestedKeyword> GetSuggestedKeyWordByKeyword(string keyword);
        Task<IPagedList<SuggestedKeyword>> GetAllSuggestedKeywordAsync(int pageIndex = 0, int pageSize = int.MaxValue);
        Task<SuggestedKeyword> GetSuggestedKeyWordById(int Id);
        Task UpdateProductSuggestedKeyWordMappingAsync(ProductSuggestedKeyword productSuggestedKeyword);
        Task DeleteProductSuggestedKeyWordMappingAsync(ProductSuggestedKeyword productSuggestedKeyword);
        Task InsertProductSuggestedKeyWordMappingAsync(ProductSuggestedKeyword productSuggestedKeyword);
        Task<List<SuggestedKeyword>> GetSuggestedKeywordsForProduct(int productId, bool showCustom = true);


        #region Category
        Task<IList<CategorySuggestedKeyword>> GetAllCategorySuggestedKeyword();
        Task<IList<CategorySuggestedKeyword>> GetCategorySuggestedKeyword(int categoryId);
        Task RegenrateSuggestedKeyWords(int productId);
        #endregion


        #region ICategorySuggestedKeywordService Code       
        Task<IPagedList<CategorySuggestedKeyword>> GetCategorySuggestedKeywords(int categoryId,
   int pageIndex = 0, int pageSize = int.MaxValue);

        Task<CategorySuggestedKeyword> GetCategorySuggestedKeywordById(int id);
        Task CreateCategorySuggestedKeyword(CategorySuggestedKeyword categorySuggestedKeyword);
        Task UpdateCategorySuggestedKeyword(CategorySuggestedKeyword categorySuggestedKeyword);
        Task DeleteCategorySuggestedKeyword(CategorySuggestedKeyword categorySuggestedKeyword);
        Task<bool> IsCategoryKeywordExist(int keywordId, string keyWord);

        #endregion
        #region Product Suggested Keyword Code
        Task<IPagedList<ProductSuggestedKeyword>> GetProductSuggestedKeyword(int productId,
   int pageIndex = 0, int pageSize = int.MaxValue);

        Task<ProductSuggestedKeyword> GetProductSuggestedKeywordById(int id);
        Task DeleteProductSuggestedKeyword(ProductSuggestedKeyword productSuggestedKeyword);
        Task<bool> IsProductKeywordExist(int keywordId, string keyword, int productId);

        #endregion

    }
}
