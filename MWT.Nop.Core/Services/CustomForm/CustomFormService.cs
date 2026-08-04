using MWT.Nop.Core.Domain;
using Nop.Core;
using Nop.Data;
using Nop.Services.Catalog;
using System.Linq.Dynamic.Core;

namespace MWT.Nop.Core.Services.Custom
{
    public partial class CustomFormService : ICustomFormService
    {
        #region Fields

        private IRepository<CustomForm> _customFormRepository;
        private IRepository<CustomFormEntry> _customFormsEntryRepository;
        private IRepository<CustomFormEntryMeta> _customFormEntryMetaRepository;


        #endregion

        #region Ctor

        public CustomFormService(IRepository<CustomForm> customFormRepository, IRepository<CustomFormEntry> customFormsEntryRepository,
                                      IRepository<CustomFormEntryMeta> customFormEntryMetaRepository)
        {
            this._customFormRepository = customFormRepository;
            this._customFormsEntryRepository = customFormsEntryRepository;
            this._customFormEntryMetaRepository = customFormEntryMetaRepository;
        }

        #endregion
        public async Task DeleteCustomformAsync(CustomForm custForm)
        {
            custForm.Deleted = true;
            await _customFormRepository.UpdateAsync(custForm);
        }

        public async Task DeleteCustomFormEntryMetaAsync(CustomFormEntryMeta customFormEntryMeta)
        {
            await _customFormEntryMetaRepository.DeleteAsync(customFormEntryMeta);
        }

        public async Task DeleteCustomFormsEntryAsync(CustomFormEntry customFormsEntry)
        {
            await _customFormsEntryRepository.DeleteAsync(customFormsEntry);
        }

        public async Task<CustomForm> GetCustomformById(int Id)
        {
            return await _customFormRepository.GetByIdAsync(Id, cache => default);
        }

        public async Task<CustomFormEntryMeta> GetCustomFormEntryMetaById(int Id)
        {
            return await _customFormEntryMetaRepository.GetByIdAsync(Id, cache => default);
        }

        public async Task<IPagedList<CustomFormEntryMeta>> GetCustomFormEntryMetas(int entryId, int pageIndex, int pageSize)
        {
            var query = from c in _customFormEntryMetaRepository.Table
                        where c.EntryID == entryId
                        orderby c.Id
                        orderby c.Id descending
                        select c;

            var result = await query.ToPagedListAsync(pageIndex, pageSize);

            return result;
        }

        public async Task<IPagedList<CustomForm>> GetCustomforms(string searchString, int pageIndex, int pageSize)
        {
            var query = _customFormRepository.Table;
            query = _customFormRepository.Table.Where(m => !m.Deleted);
            if (!string.IsNullOrEmpty(searchString))
                query = query.Where(c => c.FormName.Contains(searchString));

            query = query.OrderByDescending(c => c.Id);

            var result = await query.ToPagedListAsync(pageIndex, pageSize);

            return result;
        }

        public async Task<CustomForm> GetCustomFormByName(string name)
        {
            var query = from customForm in _customFormRepository.Table
                        where customForm.FormName == name.ToLower().Trim()
                        && customForm.Deleted == false && customForm.Published == true
                        select customForm;

            return await query.FirstOrDefaultAsync();
        }

        public async Task<IPagedList<CustomFormEntry>> GetCustomFormsEntries(int formId, int pageIndex, int pageSize)
        {
            var query = from c in _customFormsEntryRepository.Table
                        where c.FormId == formId
                        orderby c.Id
                        orderby c.Id descending
                        select c;

            var result = await query.ToPagedListAsync(pageIndex, pageSize);

            return result;
        }

        public async Task<CustomFormEntry> GetCustomFormsEntryById(int Id)
        {
            return await _customFormsEntryRepository.GetByIdAsync(Id, cache => default);
        }

        public async Task InserCustomformAsync(CustomForm custForm)
        {
            await _customFormRepository.InsertAsync(custForm);
        }

        public async Task InsertCustomFormEntryMetaAsync(CustomFormEntryMeta customFormEntryMeta)
        {
            await _customFormEntryMetaRepository.InsertAsync(customFormEntryMeta);
        }

        public async Task InsertCustomFormsEntryAsync(CustomFormEntry customFormsEntry)
        {
            await _customFormsEntryRepository.InsertAsync(customFormsEntry);
        }

        public async Task UpdatCustomFormsEntryAsync(CustomFormEntry customFormsEntry)
        {
            await _customFormsEntryRepository.UpdateAsync(customFormsEntry);
        }

        public async Task UpdateCustomformAsync(CustomForm custForm)
        {
            await _customFormRepository.UpdateAsync(custForm);
        }

        public async Task UpdateCustomFormEntryMetaAsync(CustomFormEntryMeta customFormEntryMeta)
        {
            await _customFormEntryMetaRepository.UpdateAsync(customFormEntryMeta);
        }
    }
}
