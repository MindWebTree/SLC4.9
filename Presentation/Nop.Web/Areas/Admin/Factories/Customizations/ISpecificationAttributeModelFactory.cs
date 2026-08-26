using Nop.Core.Domain.Catalog;
using Nop.Web.Areas.Admin.Models.Catalog;
using System.Threading.Tasks;

namespace Nop.Web.Areas.Admin.Factories
{
    public partial interface ISpecificationAttributeModelFactory
    {
        Task<SpecificationAttributeOptionProductListModel> PrepareSpecificationAttributeOptionProductListModelAsync(
        SpecificationAttributeOptionProductSearchModel searchModel, SpecificationAttributeOption specificationAttributeOption);
        Task<SpecificationAttributeOptionModel> CustomPrepareSpecificationAttributeOptionModelAsync(SpecificationAttributeOptionModel model,
              SpecificationAttribute specificationAttribute, SpecificationAttributeOption specificationAttributeOption,
              bool excludeProperties = false);

        Task<SpecificationAttributeProductListModel> CustomPrepareSpecificationAttributeProductListModelAsync(
       SpecificationAttributeProductSearchModel searchModel, SpecificationAttribute specificationAttribute);

        Task<SpecificationAttributeModel> CustomPrepareSpecificationAttributeModelAsync(SpecificationAttributeModel model, SpecificationAttribute specificationAttribute, bool excludeProperties = false);

        Task<SpecificationAttributeListModel> CustomPrepareCategorySpecificationAttributeListModelAsync(string entityType, int categoryId, SpecificationAttributeSearchModel searchModel, SpecificationAttributeGroup group);
        Task<SpecificationAttributeOptionListModel> CustomPrepareCategorySpecificationAttributeOptionListModelAsync(string entityType, SpecificationAttributeOptionSearchModel searchModel, SpecificationAttribute specificationAttribute);

        Task<SpecificationAttributeOptionProductListModel> CustomPrepareCategorySpecificationOptionUsedByProductsListModelAsync(SpecificationAttributeOptionProductSearchModel searchModel, SpecificationAttributeOption specificationAttributeOption);
    }
}
