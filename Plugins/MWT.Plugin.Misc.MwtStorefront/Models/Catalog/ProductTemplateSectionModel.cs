using Nop.Web.Framework.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MWT.Plugin.Misc.MwtStorefront.Models.Catalog
{
    public record ProductTemplateSectionModel : BaseNopEntityModel
    {
        public int Id { get; set; }
        public int TemplateId { get; set; }
        public string SectionKey { get; set; }
        public int SortOrder { get; set; }
        public bool IsVisible { get; set; }
        public string Config { get; set; }
    }
}
