using System.Collections.Generic;
using System.Threading.Tasks;
using MWT.Nop.Core.Domain.QA;
using Nop.Core.Domain.Catalog; 

namespace MWT.Nop.Core.Services.QA
{
    /// <summary>
    /// Kw template service interface
    /// </summary>
    public partial interface IQuestionAnswerTemplateService
    {
        /// <summary>
        /// Delete Kw template
        /// </summary>
        /// <param name="QuestionAnswerTemplate">Kw template</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        Task DeleteQuestionAnswerTemplateAsync(QuestionAnswerTemplate QuestionAnswerTemplate);

        /// <summary>
        /// Gets all Kw templates
        /// </summary>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the Kw templates
        /// </returns>
        Task<IList<QuestionAnswerTemplate>> GetAllQuestionAnswerTemplatesAsync();

        /// <summary>
        /// Gets a Kw template
        /// </summary>
        /// <param name="QuestionAnswerTemplateId">Kw template identifier</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the Kw template
        /// </returns>
        Task<QuestionAnswerTemplate> GetQuestionAnswerTemplateByIdAsync(int QuestionAnswerTemplateId);

        /// <summary>
        /// Inserts Kw template
        /// </summary>
        /// <param name="QuestionAnswerTemplate">Kw template</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        Task InsertQuestionAnswerTemplateAsync(QuestionAnswerTemplate QuestionAnswerTemplate);

        /// <summary>
        /// Updates the Kw template
        /// </summary>
        /// <param name="QuestionAnswerTemplate">Kw template</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        Task UpdateQuestionAnswerTemplateAsync(QuestionAnswerTemplate QuestionAnswerTemplate);
    }
}
