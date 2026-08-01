using Nop.Web.Models.Catalog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MWT.Plugin.Misc.MwtStorefront.Models.Catalog
{
    public record class CustomSpecificationFilterModel : SpecificationFilterModel
    {
        public new IList<CustomSpecificationAttributeFilterModel> Attributes = new List<CustomSpecificationAttributeFilterModel>();
    }
}
