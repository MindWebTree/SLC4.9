using Nop.Web.Models.Catalog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MWT.Plugin.Misc.MwtStorefront.Models.Catalog
{
    public record class CustomSpecificationAttributeFilterModel : SpecificationAttributeFilterModel
    {
        public new IList<CustomSpecificationAttributeValueFilterModel> Values = new List<CustomSpecificationAttributeValueFilterModel>();


        public int DisplayOrder { get; set; }

        public bool DisplayOnTop { get; set; }
        public string Sename { get; set; }

    }
}
