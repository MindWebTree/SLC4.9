using MWT.Nop.Core.Domain;
using Nop.Core;
using Nop.Data;

namespace MWT.Nop.Core.Service
{
    public partial class TestimonialService : ITestimonialService
    {
        private readonly IRepository<Testimonial> _testimonialRepository;

        public TestimonialService(IRepository<Testimonial> testimonialRepository)
        {
            _testimonialRepository = testimonialRepository;
        }

        public async Task<IPagedList<Testimonial>> SearchTestimonials(int orderId, int pageNumber, int pageSize)
        {
            var testimonialQuery = _testimonialRepository.Table;

            if (orderId > 0)
                testimonialQuery = testimonialQuery.Where(p => p.OrderId == orderId);
            return await testimonialQuery.OrderBy(x => x.CreatedOn).ToPagedListAsync(pageNumber - 1, pageSize);
        }

        public async Task AddTestimonial(Testimonial testimonials)
        {
            await _testimonialRepository.InsertAsync(testimonials);

        }

        public async Task<Testimonial> GetTestimonialByOrderId(int orderId)
        {
            return await _testimonialRepository.Table.Where(x => x.OrderId == orderId).FirstOrDefaultAsync();
        }
    }
}