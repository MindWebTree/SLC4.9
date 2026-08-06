
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;
using System;

namespace MWT.Plugin.Misc.MwtStorefront.Areas.Admin.Models.FAQModule
{
    /// <summary>
    /// Represents a Faq Entity model
    /// </summary>
    public partial record AddFaqModel : BaseNopModel
    {
        [NopResourceDisplayName("Admin.Catalog.Faq.Fields.Entity.Question")]
        public string Question { get; set; }
        [NopResourceDisplayName("Admin.Catalog.Faq.Fields.Entity.Answer")]
        public string Answer { get; set; }
        [NopResourceDisplayName("Admin.Catalog.Faq.Fields.Entity.DisplayOrder")]
        public int DisplayOrder { get; set; }
        public string EntityType { get; set; }
        public int EntityId { get; set; }
    }
}