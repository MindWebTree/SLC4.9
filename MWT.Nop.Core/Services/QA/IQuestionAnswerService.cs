using MWT.Nop.Core.Domain.QA;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Customization;
using Nop.Core.Domain.Customization.Catalog; 
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MWT.Nop.Core.Services.QA
{
    public partial interface IQuestionAnswerService
    {
        /// <summary>
        /// Delete QuestionAnswers
        /// </summary>
        /// <param name="QuestionAnswers">QuestionAnswers</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        Task DeleteQuestionAnswerAsync(QuestionAnswer questionAnswer);

        /// <summary>
        /// Gets all categories
        /// </summary>
        /// <param name="storeId">Store identifier; 0 if you want to get all records</param>
        /// <param name="showHidden">A value indicating whether to show hidden records</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the categories
        /// </returns>
        Task<IPagedList<QuestionAnswer>> GetAllQuestionAnswersAsync(string QuestionAnswerName = null, int storeId = 0, bool showHidden = false, int pageIndex = 0, int pageSize = 0, bool? overridePublished = null);

        /// <summary>
        /// Gets a QuestionAnswers
        /// </summary>
        /// <param name="QuestionAnswersId">QuestionAnswers identifier</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the QuestionAnswers
        /// </returns>
        Task<QuestionAnswer> GetQuestionAnswerByIdAsync(int questionAnswerId);

        /// <summary>
        /// Inserts QuestionAnswers
        /// </summary>
        /// <param name="QuestionAnswers">QuestionAnswers</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        Task InsertQuestionAnswerAsync(QuestionAnswer questionAnswer);

        /// <summary>
        /// Updates the QuestionAnswers
        /// </summary>
        /// <param name="QuestionAnswers">QuestionAnswers</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        Task UpdateQuestionAnswerAsync(QuestionAnswer questionAnswers);

        /// <summary>
        /// Delete a list of QuestionAnswers
        /// </summary>
        /// <param name="categories">QuestionAnswers</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        Task DeleteQuestionAnswerAsync(IList<QuestionAnswer> questionAnswers);

        /// <summary>
        /// Deletes a product QuestionAnswers mapping
        /// </summary>
        /// <param name="productQuestionAnswers">Product QuestionAnswers</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        Task DeleteProductQuestionAnswerAsync(ProductQuestionAnswer productQuestionAnswer);

        /// <summary>
        /// Gets product QuestionAnswers mapping collection
        /// </summary>
        /// <param name="QuestionAnswersId">QuestionAnswers identifier</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <param name="showHidden">A value indicating whether to show hidden records</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the product a QuestionAnswers mapping collection
        /// </returns>
        Task<IPagedList<ProductQuestionAnswer>> GetProductQuestionAnswersByQuestionAnswerIdAsync(int QuestionAnswersId,
            int pageIndex = 0, int pageSize = int.MaxValue, bool showHidden = false);




        /// <summary>
        /// Gets a product QuestionAnswers mapping 
        /// </summary>
        /// <param name="productQuestionAnswersId">Product QuestionAnswers mapping identifier</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the product QuestionAnswers mapping
        /// </returns>
        Task<ProductQuestionAnswer> GetProductQuestionAnswerByIdAsync(int productQuestionAnswerId);

        /// <summary>
        /// Inserts a product QuestionAnswers mapping
        /// </summary>
        /// <param name="productQuestionAnswers">>Product QuestionAnswers mapping</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        Task InsertProductQuestionAnswerAsync(ProductQuestionAnswer productQuestionAnswer);

        /// <summary>
        /// Updates the product QuestionAnswers mapping 
        /// </summary>
        /// <param name="productQuestionAnswers">>Product QuestionAnswers mapping</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        Task UpdateProductQuestionAnswerAsync(ProductQuestionAnswer productQuestionAnswers);

        /// <summary>
        /// Gets categories by identifier
        /// </summary>
        /// <param name="QuestionAnswersIds">QuestionAnswers identifiers</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the categories
        /// </returns>
        Task<IList<QuestionAnswer>> GetQuestionAnswersByIdsAsync(int[] QuestionAnswersIds);

        //TODO: migrate to an extension method
        /// <summary>
        /// Returns a ProductQuestionAnswers that has the specified values
        /// </summary>
        /// <param name="source">Source</param>
        /// <param name="productId">Product identifier</param>
        /// <param name="QuestionAnswersId">QuestionAnswers identifier</param>
        /// <returns>A ProductQuestionAnswers that has the specified values; otherwise null</returns>
        ProductQuestionAnswer FindProductQuestionAnswer(IList<ProductQuestionAnswer> source, int productId, int QuestionAnswersId);

        /// <summary>
        /// Get formatted QuestionAnswers breadcrumb 
        /// Note: ACL and store mapping is ignored
        /// </summary>
        /// <param name="QuestionAnswers">QuestionAnswers</param>
        /// <param name="allQuestionAnswers">All categories</param>
        /// <param name="separator">Separator</param>
        /// <param name="languageId">Language identifier for localization</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the formatted breadcrumb
        /// </returns>
        Task<string> GetFormattedBreadCrumbAsync(QuestionAnswer QuestionAnswer, IList<QuestionAnswer> allQuestionAnswer = null,
            string separator = ">>", int languageId = 0);

        /// <summary>
        /// Get QuestionAnswers breadcrumb 
        /// </summary>
        /// <param name="QuestionAnswers">QuestionAnswers</param>
        /// <param name="allQuestionAnswers">All categories</param>
        /// <param name="showHidden">A value indicating whether to load hidden records</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the QuestionAnswers breadcrumb 
        /// </returns>
        Task<IList<QuestionAnswer>> GetQuestionAnswerBreadCrumbAsync(QuestionAnswer QuestionAnswer, IList<QuestionAnswer> allQuestionAnswer = null, bool showHidden = false);


        #region related question answers

        /// <summary>
        /// Deletes a related question answer
        /// </summary>
        /// <param name="RelatedQuestionAnswer">related question answer</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        Task DeleteRelatedQuestionAnswerAsync(RelatedQuestionAnswer relatedQuestionAnswer);

        /// <summary>
        /// Gets related question answers by product identifier
        /// </summary>
        /// <param name="productId1">The first product identifier</param>
        /// <param name="showHidden">A value indicating whether to show hidden records</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the related question answers
        /// </returns>
        Task<IList<RelatedQuestionAnswer>> GetRelatedQuestionAnswersByQuestionAnswerId1Async(int questionAnswerId1, bool showHidden = false);

        /// <summary>
        /// Gets a related question answer
        /// </summary>
        /// <param name="RelatedQuestionAnswerId">related question answer identifier</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the related question answer
        /// </returns>
        Task<RelatedQuestionAnswer> GetRelatedQuestionAnswerByIdAsync(int relatedQuestionAnswerId);

        /// <summary>
        /// Inserts a related question answer
        /// </summary>
        /// <param name="RelatedQuestionAnswer">related question answer</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        Task InsertRelatedQuestionAnswerAsync(RelatedQuestionAnswer relatedQuestionAnswer);

        /// <summary>
        /// Updates a related question answer
        /// </summary>
        /// <param name="RelatedQuestionAnswer">related question answer</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        Task UpdateRelatedQuestionAnswerAsync(RelatedQuestionAnswer relatedQuestionAnswer);

        //TODO: migrate to an extension method
        /// <summary>
        /// Finds a related question answer item by specified identifiers
        /// </summary>
        /// <param name="source">Source</param>
        /// <param name="productId1">The first product identifier</param>
        /// <param name="productId2">The second product identifier</param>
        /// <returns>related question answer</returns>
        RelatedQuestionAnswer FindRelatedQuestionAnswer(IList<RelatedQuestionAnswer> source, int questionAnswerId1, int questionAnswerId2);

        #endregion

    }
}
