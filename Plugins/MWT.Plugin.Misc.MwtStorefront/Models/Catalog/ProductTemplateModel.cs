using Nop.Web.Framework.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MWT.Plugin.Misc.MwtStorefront.Models.Catalog
{
    public record ProductTemplateModel : BaseNopEntityModel
    { 
        public string Name { get; set; } 
        public string ViewPath { get; set; }
        public int DisplayOrder { get; set; }  
        public string IgnoredProductTypes { get; set; }
        public string ProductAttributeViewName { get; set; }
        public string ConatinerClass { get; set; }
    }
}
