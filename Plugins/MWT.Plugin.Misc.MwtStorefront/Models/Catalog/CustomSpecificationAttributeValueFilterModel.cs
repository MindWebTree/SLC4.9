using Nop.Web.Models.Catalog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MWT.Plugin.Misc.MwtStorefront.Models.Catalog
{
    public record class CustomSpecificationAttributeValueFilterModel: SpecificationAttributeValueFilterModel
    {
        public int Count { get; set; }
        public int DisplayOrder { get; set; }
        public string Sename { get; set; }
    }
}
