using Nop.Core.Domain.Catalog;
using Nop.Services.Catalog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MWT.Nop.Core.Services.Catalog
{
    public partial interface ICustomProductAttributeFormatter: IProductAttributeFormatter
    {
        Task<string> CustomFormatAttributesAsync(Product product, string attributesXml);
    }
}
