using MWT.Nop.Core.Domain.FAQModule;
using Nop.Core;
using Nop.Data;

namespace MWT.Nop.Core.Service.FAQModule
{
    public partial class FaqService : IFaqService
    {
        private readonly IRepository<Faq> _faqRepository;
        public FaqService(IRepository<Faq> faqRepository)
        {
            _faqRepository = faqRepository;
        }

        #region Methods

        public async Task DeleteAsync(Faq obj)
        {
            obj.Deleted = true;
            await _faqRepository.UpdateAsync(obj);
        }

        public async Task<Faq> GetById(int Id)
        {
            return await _faqRepository.GetByIdAsync(Id, cache => default);
        }

        public async Task<IList<Faq>> GetFaqByEntity(int entityId, string entityType)
        {
            var query = from rs in _faqRepository.Table
                        where rs.EntityId == entityId &&
                        rs.EntityType == entityType && !rs.Deleted
                        orderby rs.DisplayOrder
                        select rs;
            return await query.ToListAsync();
        }

        public async Task<IPagedList<Faq>> GetFaqsByEntityIdAsync(int entityId, string entityType, int pageIndex = 0, int pageSize = int.MaxValue,
        bool showHidden = false)
        {
            if (entityId == 0)
                return null;
            IQueryable<Faq> query = null;

                query = from faq in _faqRepository.Table
                        where faq.EntityId == entityId && faq.EntityType == entityType && faq.Deleted==false
                        select faq;
         

            return await query.ToPagedListAsync(pageIndex, pageSize);
        }

        public async Task InsertAsync(Faq obj)
        {
            await _faqRepository.InsertAsync(obj);
        }

        public async Task UpdateAsync(Faq obj)
        {
            await _faqRepository.UpdateAsync(obj);
        }

        #endregion
    }
}
