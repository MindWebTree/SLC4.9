using MWT.Nop.Core.Domain.Catalog;
using MWT.Nop.Core.Service.Catalog;
using Nop.Core.Caching;
using Nop.Services.Catalog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MWT.Nop.Core.Services.Catalog
{
    public interface IProductTemplateSectionService
    {
        Task<IList<ProductTemplateSection>> GetProductTemplateSectionsByTemplateIdAsync(int templateId);
    }
}
