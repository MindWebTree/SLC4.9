using Microsoft.AspNetCore.Http;
using Nop.Core.Domain.Catalog;
using Nop.Services.Catalog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MWT.Nop.Core.Services.Catalog
{
    public partial interface ICustomProductAttributeParser:IProductAttributeParser
    {
        Task<Dictionary<string, List<int>>> CustomGenerateAllCombinationsAsync(Product product);
        Task<string> CustomParseProductAttributesAsync(Product product, IFormCollection form, List<string> errors, string formId);
        IList<Tuple<string, string>> CustomParseValuesWithQuantity(string attributesXml, int productAttributeMappingId);
        Task<IList<int>> CustomParseProductAttributeValuesAsync(string attributesXml, int productAttributeMappingId = 0);
    }
}
