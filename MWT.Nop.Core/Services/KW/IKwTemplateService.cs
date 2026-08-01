using MWT.Nop.Core.Domain.KW;

namespace MWT.Nop.Core.Services.KW
{  /// <summary>
    /// Kw template service interface
    /// </summary>
    public partial interface IKwTemplateService
    {
        /// <summary>
        /// Delete Kw template
        /// </summary>
        /// <param name="KwTemplate">Kw template</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        Task DeleteKwTemplateAsync(KwTermTemplate KwTemplate);

        /// <summary>
        /// Gets all Kw templates
        /// </summary>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the Kw templates
        /// </returns>
        Task<IList<KwTermTemplate>> GetAllKwTemplatesAsync();

        /// <summary>
        /// Gets a Kw template
        /// </summary>
        /// <param name="KwTemplateId">Kw template identifier</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the Kw template
        /// </returns>
        Task<KwTermTemplate> GetKwTemplateByIdAsync(int KwTemplateId);

        /// <summary>
        /// Inserts Kw template
        /// </summary>
        /// <param name="KwTemplate">Kw template</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        Task InsertKwTemplateAsync(KwTermTemplate KwTemplate);

        /// <summary>
        /// Updates the Kw template
        /// </summary>
        /// <param name="KwTemplate">Kw template</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        Task UpdateKwTemplateAsync(KwTermTemplate KwTemplate);


      
    }
}
