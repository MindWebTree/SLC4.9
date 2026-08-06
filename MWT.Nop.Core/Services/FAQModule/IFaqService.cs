using MWT.Nop.Core.Domain.FAQModule;
using Nop.Core;

namespace MWT.Nop.Core.Service.FAQModule
{
    public partial interface IFaqService
    {
        Task UpdateAsync(Faq obj);
        Task DeleteAsync(Faq obj);
        Task InsertAsync(Faq obj);
        Task<Faq> GetById(int Id);
        Task<IList<Faq>> GetFaqByEntity(int entityId, string entityType);
        Task<IPagedList<Faq>> GetFaqsByEntityIdAsync(int entityId, string entityType,
           int pageIndex = 0, int pageSize = int.MaxValue, bool showHidden = false);
    }
}
