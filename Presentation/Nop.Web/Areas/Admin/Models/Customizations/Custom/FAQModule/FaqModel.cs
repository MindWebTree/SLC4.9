

using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;
using System;

namespace Nop.Web.Areas.Admin.Models.Customization.Custom.FAQModule
{
    /// <summary>
    /// Represents a Faq Entity model
    /// </summary>
    public partial record FaqModel : BaseNopEntityModel
    {
        public string EntityType { get; set; }
        public int EntityId { get; set; }
        [NopResourceDisplayName("Admin.Catalog.Faq.Fields.Entity.Question")]
        public string Question { get; set; }
        [NopResourceDisplayName("Admin.Catalog.Faq.Fields.Entity.Answer")]
        public string Answer { get; set; }
        [NopResourceDisplayName("Admin.Catalog.Faq.Fields.Entity.Deleted")]
        public int DisplayOrder { get; set; }
        [NopResourceDisplayName("Admin.Catalog.Faq.Fields.Entity.CreatedOnUtc")]
        public DateTime CreatedOnUtc { get; set; }
        [NopResourceDisplayName("Admin.Catalog.Faq.Fields.Entity.UpdatedOnUtc")]
        public DateTime UpdatedOnUtc { get; set; }
    }
}