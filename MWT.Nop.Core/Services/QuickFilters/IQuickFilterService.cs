using MWT.Nop.Core.Domain;

namespace MWT.Nop.Core.Services.QuickFilters
{
    public partial interface IQuickFilterService
    {
        Task UpdateAsync(QuickFilter quickFilter);
        Task DeleteAsync(QuickFilter quickFilter);
        Task InsertAsync(QuickFilter quickFilter);
        Task<QuickFilter> GetById(int Id);
        Task<IList<QuickFilter>> GetQuickFilterByEntity(int entityId, string entityType);
      
    }
}
