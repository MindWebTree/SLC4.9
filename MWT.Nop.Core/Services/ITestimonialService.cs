using MWT.Nop.Core.Domain;
using Nop.Core;

namespace MWT.Nop.Core.Service
{
    public partial interface ITestimonialService
    {
        Task<IPagedList<Testimonial>> SearchTestimonials(int orderId, int page, int pageSize);
        Task AddTestimonial(Testimonial testimonialDetails);
        Task<Testimonial> GetTestimonialByOrderId(int orderId);
    }
}