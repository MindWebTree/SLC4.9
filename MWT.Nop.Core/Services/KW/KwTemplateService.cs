using MWT.Nop.Core.Domain.KW;
using Nop.Data;

namespace MWT.Nop.Core.Services.KW
{
    /// <summary>
    /// Kw template service
    /// </summary>
    public partial class KWTemplateService : IKwTemplateService
    {
        #region Fields

        private readonly IRepository<KwTermTemplate> _KwTemplateRepository;

        #endregion

        #region Ctor

        public KWTemplateService(IRepository<KwTermTemplate> KwTemplateRepository)
        {
            _KwTemplateRepository = KwTemplateRepository;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Delete Kw template
        /// </summary>
        /// <param name="KwTemplate">Kw template</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task DeleteKwTemplateAsync(KwTermTemplate KwTemplate)
        {
            await _KwTemplateRepository.DeleteAsync(KwTemplate);
        }

        /// <summary>
        /// Gets all Kw templates
        /// </summary>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the Kw templates
        /// </returns>
        public virtual async Task<IList<KwTermTemplate>> GetAllKwTemplatesAsync()
        {
            var templates = await _KwTemplateRepository.GetAllAsync(query =>
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
        /// <param name="KwTemplateId">Kw template identifier</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the Kw template
        /// </returns>
        public virtual async Task<KwTermTemplate> GetKwTemplateByIdAsync(int KwTemplateId)
        {
            return await _KwTemplateRepository.GetByIdAsync(KwTemplateId, cache => default);
        }

        /// <summary>
        /// Inserts Kw template
        /// </summary>
        /// <param name="KwTemplate">Kw template</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task InsertKwTemplateAsync(KwTermTemplate KwTemplate)
        {
            await _KwTemplateRepository.InsertAsync(KwTemplate);
        }

        /// <summary>
        /// Updates the Kw template
        /// </summary>
        /// <param name="KwTemplate">Kw template</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task UpdateKwTemplateAsync(KwTermTemplate KwTemplate)
        {
            await _KwTemplateRepository.UpdateAsync(KwTemplate);
        }

        #endregion
    }
}