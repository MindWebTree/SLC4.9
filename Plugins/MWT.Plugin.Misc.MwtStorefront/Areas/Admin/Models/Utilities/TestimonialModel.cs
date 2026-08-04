using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MWT.Plugin.Misc.MwtStorefront.Areas.Admin.Models.Utilities
{
    public partial record TestimonialModel : BaseNopEntityModel
    {
        [NopResourceDisplayName("Admin.Testimonial.Fields.OrderId")]
        public int OrderId { get; set; }

        [NopResourceDisplayName("Admin.Testimonial.Fields.Customer.Email")]
        public string CustomerEmail { get; set; }

        [NopResourceDisplayName("Admin.Testimonial.Fields.Customer.Name")]
        public string CustomerName { get; set; }


        [NopResourceDisplayName("Admin.Testimonial.Fields.CreatedOn")]
        public DateTime CreatedOn { get; set; }
        
        [NopResourceDisplayName("Admin.Testimonial.Fields.VideoUrl")]
        public string VideoUrl { get; set; }
        
        [NopResourceDisplayName("Admin.Testimonial.Fields.IsConsent")]
        public bool IsConsent { get; set; }

    }
}
