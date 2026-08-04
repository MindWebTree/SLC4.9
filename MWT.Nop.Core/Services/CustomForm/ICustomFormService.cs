
using MWT.Nop.Core.Domain;
using Nop.Core;

namespace MWT.Nop.Core.Services.Custom
{
    public partial interface ICustomFormService
    {
        #region Customforms

        Task UpdateCustomformAsync(CustomForm custForm);
        Task DeleteCustomformAsync(CustomForm custForm);
        Task InserCustomformAsync(CustomForm custForm);
        Task<CustomForm> GetCustomformById(int Id);
        Task<IPagedList<CustomForm>> GetCustomforms(string searchString, int pageIndex, int pageSize);
        Task<CustomForm> GetCustomFormByName(string name);

        #endregion

        #region CustomFormEntry

        Task UpdatCustomFormsEntryAsync(CustomFormEntry customFormsEntry);
        Task DeleteCustomFormsEntryAsync(CustomFormEntry customFormsEntry);
        Task InsertCustomFormsEntryAsync(CustomFormEntry customFormsEntry);
        Task<CustomFormEntry> GetCustomFormsEntryById(int Id);
        Task<IPagedList<CustomFormEntry>> GetCustomFormsEntries(int formId, int pageIndex, int pageSize);

        #endregion

        #region CustomFormEntryMeta

        Task UpdateCustomFormEntryMetaAsync(CustomFormEntryMeta customFormEntryMeta);
        Task DeleteCustomFormEntryMetaAsync(CustomFormEntryMeta customFormEntryMeta);
        Task InsertCustomFormEntryMetaAsync(CustomFormEntryMeta customFormEntryMeta);
        Task<CustomFormEntryMeta> GetCustomFormEntryMetaById(int Id);
        Task<IPagedList<CustomFormEntryMeta>> GetCustomFormEntryMetas(int entryId, int pageIndex, int pageSize);

        #endregion
    }
}


