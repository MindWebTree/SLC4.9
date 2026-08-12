using Nop.Core;

namespace MWT.Nop.Core.Domain.Catalog
{
    public class ProductTemplateSection : BaseEntity
    {
        public int TemplateId { get; set; }
        public string SectionKey { get; set; }
        public int SortOrder { get; set; }
        public bool IsVisible { get; set; }
        public string Config { get; set; }

    }
}
