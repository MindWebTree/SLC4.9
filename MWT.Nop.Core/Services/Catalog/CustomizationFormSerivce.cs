using MWT.Nop.Core.Domain.Catalog;
using Nop.Core.Infrastructure;
using Nop.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MWT.Nop.Core.Services.Catalog
{
    public partial class CustomizationFormSerivce : ICustomizationFormSerivce
    {
        private readonly IRepository<ProductCustomizationFormTemplate> _productCustomiztionFormTemplateRepository;
        public CustomizationFormSerivce(IRepository<ProductCustomizationFormTemplate> productCustomiztionFormTemplateRepository)
        {
            _productCustomiztionFormTemplateRepository = productCustomiztionFormTemplateRepository;
        }
        public async Task<IList<ProductCustomizationFormTemplate>> GetAllProductCustomizationFormTemplatesAsync()
        {

            var templates = await _productCustomiztionFormTemplateRepository.GetAllAsync(query =>
            {
                return from pt in query
                       orderby pt.DisplayOrder, pt.Id
                       select pt;
            }, cache => default);

            return templates;
        }
        public async Task<ProductCustomizationFormTemplate> GetProductCustomizationFormTemplateByIdAsync(int productCustomizationFormTemplateId)
        {

            return await _productCustomiztionFormTemplateRepository.GetByIdAsync(productCustomizationFormTemplateId, cache => default);
        }


    }
}
