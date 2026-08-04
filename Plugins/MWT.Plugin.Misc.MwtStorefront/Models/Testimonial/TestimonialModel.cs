using Microsoft.AspNetCore.Http;
using Nop.Web.Framework.Mvc.ModelBinding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MWT.Plugin.Misc.MwtStorefront.Models
{
    public class TestimonialModel
    {
        [NopResourceDisplayName("Testimonial.Fields.OrderId")]
        public int OrderId { get; set; }

        [NopResourceDisplayName("Testimonial.Fields.VideoUrl")]
        public string VideoUrl { get; set; }

        [NopResourceDisplayName("Testimonial.Fields.Video")]
        public IFormFile Video { get; set; }

        [NopResourceDisplayName("Testimonial.Fields.IsConsent")]
        public bool IsConsent { get; set; }

        [NopResourceDisplayName("Testimonial.Fields.Email")]
        public string Email { get; set; }


    }
}
