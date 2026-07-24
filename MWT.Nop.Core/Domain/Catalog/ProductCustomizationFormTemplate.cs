using Nop.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MWT.Nop.Core.Domain.Catalog
{
    public class ProductCustomizationFormTemplate : BaseEntity
    {
        public string Name { get; set; }
        public string ViewPath { get; set; }
        public string MobileViewPath { get; set; }
        public int DisplayOrder { get; set; }
        public bool IsDefault { get; set; }
    }
}