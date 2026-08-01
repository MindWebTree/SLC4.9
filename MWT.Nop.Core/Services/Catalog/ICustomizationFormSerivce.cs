using MWT.Nop.Core.Domain.Catalog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MWT.Nop.Core.Services.Catalog
{
    public partial interface ICustomizationFormSerivce
    {
        Task<IList<ProductCustomizationFormTemplate>> GetAllProductCustomizationFormTemplatesAsync();
        Task<ProductCustomizationFormTemplate> GetProductCustomizationFormTemplateByIdAsync(int productCustomizationFormTemplateId);

    }
}
