using MWT.Nop.Core.Domain.KW;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MWT.Nop.Core.Services.KW
{
    public partial interface IKwTermService
    {
        /// <summary>
        /// Delete kwTerms
        /// </summary>
        /// <param name="kwTerms">KwTerms</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        Task DeleteKwTermAsync(KwTerm kwTerms);

        /// <summary>
        /// Gets all categories
        /// </summary>
        /// <param name="storeId">Store identifier; 0 if you want to get all records</param>
        /// <param name="showHidden">A value indicating whether to show hidden records</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the categories
        /// </returns>
        Task<IPagedList<KwTerm>> GetAllKwTermsAsync(string kwTermName = null, int storeId = 0, bool showHidden = false, int pageIndex = 0, int pageSize = 0, bool? overridePublished = null);

        /// <summary>
        /// Gets a kwTerms
        /// </summary>
        /// <param name="kwTermsId">KwTerms identifier</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the kwTerms
        /// </returns>
        Task<KwTerm> GetKwTermByIdAsync(int kwTermId);

        /// <summary>
        /// Inserts kwTerms
        /// </summary>
        /// <param name="kwTerms">KwTerms</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        Task InsertKwTermAsync(KwTerm kwTerm);

        /// <summary>
        /// Updates the kwTerms
        /// </summary>
        /// <param name="kwTerms">KwTerms</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        Task UpdateKwTermAsync(KwTerm kwTerms);

        /// <summary>
        /// Delete a list of kwTerms
        /// </summary>
        /// <param name="categories">kwTerms</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        Task DeleteKwTermAsync(IList<KwTerm> kwTerms);

        /// <summary>
        /// Deletes a product kwTerms mapping
        /// </summary>
        /// <param name="productKwTerms">Product kwTerms</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        Task DeleteProductKwTermAsync(ProductKwTerm productKwTerm);

        /// <summary>
        /// Gets product kwTerms mapping collection
        /// </summary>
        /// <param name="kwTermsId">KwTerms identifier</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <param name="showHidden">A value indicating whether to show hidden records</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the product a kwTerms mapping collection
        /// </returns>
        Task<IPagedList<ProductKwTerm>> GetProductKwTermsByKwTermIdAsync(int kwTermsId,
            int pageIndex = 0, int pageSize = int.MaxValue, bool showHidden = false);




        /// <summary>
        /// Gets a product kwTerms mapping 
        /// </summary>
        /// <param name="productKwTermsId">Product kwTerms mapping identifier</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the product kwTerms mapping
        /// </returns>
        Task<ProductKwTerm> GetProductKwTermByIdAsync(int productKwTermId);

        /// <summary>
        /// Inserts a product kwTerms mapping
        /// </summary>
        /// <param name="productKwTerms">>Product kwTerms mapping</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        Task InsertProductKwTermAsync(ProductKwTerm productKwTerm);

        /// <summary>
        /// Updates the product kwTerms mapping 
        /// </summary>
        /// <param name="productKwTerms">>Product kwTerms mapping</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        Task UpdateProductKwTermAsync(ProductKwTerm productKwTerms);

        /// <summary>
        /// Gets categories by identifier
        /// </summary>
        /// <param name="kwTermsIds">KwTerms identifiers</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the categories
        /// </returns>
        Task<IList<KwTerm>> GetkwTermsByIdsAsync(int[] kwTermsIds);

        //TODO: migrate to an extension method
        /// <summary>
        /// Returns a ProductKwTerms that has the specified values
        /// </summary>
        /// <param name="source">Source</param>
        /// <param name="productId">Product identifier</param>
        /// <param name="kwTermsId">KwTerms identifier</param>
        /// <returns>A ProductKwTerms that has the specified values; otherwise null</returns>
        ProductKwTerm FindProductKwTerm(IList<ProductKwTerm> source, int productId, int kwTermsId);

        /// <summary>
        /// Get formatted kwTerms breadcrumb 
        /// Note: ACL and store mapping is ignored
        /// </summary>
        /// <param name="kwTerms">KwTerms</param>
        /// <param name="allkwTerms">All categories</param>
        /// <param name="separator">Separator</param>
        /// <param name="languageId">Language identifier for localization</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the formatted breadcrumb
        /// </returns>
        Task<string> GetFormattedBreadCrumbAsync(KwTerm kwTerm, IList<KwTerm> allkwTerm = null,
            string separator = ">>", int languageId = 0);

        /// <summary>
        /// Get kwTerms breadcrumb 
        /// </summary>
        /// <param name="kwTerms">KwTerms</param>
        /// <param name="allkwTerms">All categories</param>
        /// <param name="showHidden">A value indicating whether to load hidden records</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the kwTerms breadcrumb 
        /// </returns>
        Task<IList<KwTerm>> GetKwTermBreadCrumbAsync(KwTerm kwTerm, IList<KwTerm> allkwTerm = null, bool showHidden = false);


        #region Categories

        Task<IPagedList<CategoryKwTerm>> GetCategoryKwTermsByKwTermIdAsync(int kwTermsId,
            int pageIndex = 0, int pageSize = int.MaxValue, bool showHidden = false);
        Task<CategoryKwTerm> GetCategoryKwTermByIdAsync(int categoryKwTermId);
        Task UpdateCategoryKwTermAsync(CategoryKwTerm categoryKwTerms);
        Task DeleteCategoryKwTermAsync(CategoryKwTerm categoryKwTerm);
        Task InsertCategoryKwTermAsync(CategoryKwTerm categoryKwTerm);
        Task<IList<Category>> GetCategoriesAsync(int kwTermId);

        #endregion

    }
}
