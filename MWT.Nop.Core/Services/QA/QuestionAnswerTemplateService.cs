using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MWT.Nop.Core.Domain.QA;
using Nop.Core.Domain.Catalog; 
using Nop.Data;

namespace MWT.Nop.Core.Services.QA
{
    /// <summary>
    /// Kw template service
    /// </summary>
    public partial class QuestionAnswerTemplateService : IQuestionAnswerTemplateService
    {
        #region Fields

        private readonly IRepository<QuestionAnswerTemplate> _QuestionAnswerTemplateRepository;

        #endregion

        #region Ctor

        public QuestionAnswerTemplateService(IRepository<QuestionAnswerTemplate> QuestionAnswerTemplateRepository)
        {
            _QuestionAnswerTemplateRepository = QuestionAnswerTemplateRepository;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Delete Kw template
        /// </summary>
        /// <param name="QuestionAnswerTemplate">Kw template</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task DeleteQuestionAnswerTemplateAsync(QuestionAnswerTemplate QuestionAnswerTemplate)
        {
            await _QuestionAnswerTemplateRepository.DeleteAsync(QuestionAnswerTemplate);
        }

        /// <summary>
        /// Gets all Kw templates
        /// </summary>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the Kw templates
        /// </returns>
        public virtual async Task<IList<QuestionAnswerTemplate>> GetAllQuestionAnswerTemplatesAsync()
        {
            var templates = await _QuestionAnswerTemplateRepository.GetAllAsync(query =>
            {
                return from pt in query
                       orderby pt.DisplayOrder, pt.Id
                       select pt;
            }, cache => default);

            return templates;
        }

        /// <summary>
        /// Gets a Kw template
        /// </summary>
        /// <param name="QuestionAnswerTemplateId">Kw template identifier</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the Kw template
        /// </returns>
        public virtual async Task<QuestionAnswerTemplate> GetQuestionAnswerTemplateByIdAsync(int QuestionAnswerTemplateId)
        {
            return await _QuestionAnswerTemplateRepository.GetByIdAsync(QuestionAnswerTemplateId, cache => default);
        }

        /// <summary>
        /// Inserts Kw template
        /// </summary>
        /// <param name="QuestionAnswerTemplate">Kw template</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task InsertQuestionAnswerTemplateAsync(QuestionAnswerTemplate QuestionAnswerTemplate)
        {
            await _QuestionAnswerTemplateRepository.InsertAsync(QuestionAnswerTemplate);
        }

        /// <summary>
        /// Updates the Kw template
        /// </summary>
        /// <param name="QuestionAnswerTemplate">Kw template</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task UpdateQuestionAnswerTemplateAsync(QuestionAnswerTemplate QuestionAnswerTemplate)
        {
            await _QuestionAnswerTemplateRepository.UpdateAsync(QuestionAnswerTemplate);
        }

        #endregion
    }
}