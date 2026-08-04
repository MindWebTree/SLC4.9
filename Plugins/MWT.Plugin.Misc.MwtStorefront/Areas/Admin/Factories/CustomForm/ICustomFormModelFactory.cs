using MWT.Nop.Core.Domain;
using MWT.Plugin.Misc.MwtStorefront.Areas.Admin.Models;

namespace MWT.Plugin.Misc.MwtStorefront.Areas.Admin.Factories.Custom
{
    public partial interface ICustomFormModelFactory
    {
        Task<CustomFormSearchModel> PrepareCustomFormSearchModelAsync(CustomFormSearchModel searchModel);
        Task<CustomFormListModel> PrepareCustomFormSearchListModelAsync(CustomFormSearchModel searchModel);
        CustomFormModel PrepareCustomFormModelAsync(CustomFormModel model, CustomForm customForm);

        Task<CustomFormSearchModel> PrepareCustomFormEntrySearchModelAsync(CustomFormSearchModel searchModel, int id);
        Task<CustomFormEntryListModel> PrepareCustomFormEntrySearchListModelAsync(CustomFormSearchModel searchModel,CustomForm form);
        Task<CustomFormSearchModel> PrepareCustomFormEntryMetasSearchModelAsync(CustomFormSearchModel searchModel, int id);
        Task<CustomFormEntryMetaListModel> PrepareCustomFormEntryMetasEntrySearchListModelAsync(CustomFormSearchModel searchModel);

    }
}
