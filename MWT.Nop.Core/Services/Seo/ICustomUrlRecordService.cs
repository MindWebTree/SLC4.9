using Nop.Core.Domain.Seo;
using Nop.Services.Seo;


namespace MWT.Nop.Core.Services.Seo
{
    public partial interface ICustomUrlRecordService : IUrlRecordService
    {
        Task<UrlRecord> GetByKwTermSlugAsync(string slug);
        Task<UrlRecord> GetByQuestionAnswerSlugAsync(string slug);
        Task<UrlRecord> GetByLandingPageSlugAsync(string slug);
    }
}
