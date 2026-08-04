using MWT.Nop.Core.Domain;
using MWT.Nop.Core.Services.Custom;
using MWT.Plugin.Misc.MwtStorefront.Areas.Admin.Models;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Framework.Models.Extensions;

namespace MWT.Plugin.Misc.MwtStorefront.Areas.Admin.Factories.Custom
{
    public partial class CustomFormModelFactory : ICustomFormModelFactory
    {
        #region Fields

        private readonly ICustomFormService _customFormService;

        #endregion

        #region Ctor

        public CustomFormModelFactory(ICustomFormService customFormService)
        {
            _customFormService = customFormService;
        }

        #endregion

        #region Methods
        public virtual Task<CustomFormSearchModel> PrepareCustomFormSearchModelAsync(CustomFormSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            //prepare page parameters
            searchModel.SetGridPageSize();

            return Task.FromResult(searchModel);
        }

        public virtual async Task<CustomFormListModel> PrepareCustomFormSearchListModelAsync(CustomFormSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            //get custom forms
            var customforms = await _customFormService.GetCustomforms(searchModel.Name, searchModel.Page - 1, searchModel.PageSize);

            //prepare list model
            var model = new CustomFormListModel().PrepareToGrid(searchModel, customforms, () =>
            {
                return customforms.Select(customform =>
                {
                    //fill in model values from the entity
                    var customformModel = customform.ToModel<CustomFormModel>();
                    customformModel.UseHtml = string.IsNullOrEmpty(customformModel.ThankYouPageLink);
                    return customformModel;
                });
            });

            return model;
        }


        public virtual CustomFormModel PrepareCustomFormModelAsync(CustomFormModel model, CustomForm customForm)
        {


            if (customForm != null)
            {
                //fill in model values from the entity
                if (model == null)
                {
                    model = customForm.ToModel<CustomFormModel>();
                    model.UseHtml = string.IsNullOrEmpty(customForm.ThankYouPageLink);
                }
            }

            return model;
        }

        public Task<CustomFormSearchModel> PrepareCustomFormEntrySearchModelAsync(CustomFormSearchModel searchModel, int id)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));
            searchModel.Id = id;
            //prepare page parameters
            searchModel.SetGridPageSize();

            return Task.FromResult(searchModel);
        }

        public async Task<CustomFormEntryListModel> PrepareCustomFormEntrySearchListModelAsync(CustomFormSearchModel searchModel, CustomForm form)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));


            //get custom forms
            var customformEntries = await _customFormService.GetCustomFormsEntries(searchModel.Id, searchModel.Page - 1, searchModel.PageSize);

            //prepare list model
            var model = await new CustomFormEntryListModel().PrepareToGridAsync(searchModel, customformEntries, () =>
            {
                return customformEntries.SelectAwait(async customformEntry =>
                {
                    var customformEntryModel = customformEntry.ToModel<CustomFormEntryModel>();
                    customformEntryModel.FormName = form.FormName;
                    customformEntryModel.CreatedOnUtc = customformEntry.CreatedOnUtc.ToString("dddd, dd MMMM yyyy HH:mm");
                    customformEntryModel.UpdatedOnUtc = customformEntry.UpdatedOnUtc.ToString("dddd, dd MMMM yyyy HH:mm");
                    var metas = await _customFormService.GetCustomFormEntryMetas(customformEntry.Id, 0, 100);

                    string firstName = metas.Where(m => m.MetaKey.Trim().Equals("firstName", StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault()?.MetaValue;
                    string lastName = metas.Where(m => m.MetaKey.Trim().Equals("lastName", StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault()?.MetaValue;
                    customformEntryModel.CustomerName = metas.Where(m => m.MetaKey.Trim().Equals("name", StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault()?.MetaValue;
                    if (string.IsNullOrEmpty(customformEntryModel.CustomerName))
                        customformEntryModel.CustomerName = (string.IsNullOrEmpty(firstName) ? "" : firstName + " ") + (string.IsNullOrEmpty(lastName) ? "" : lastName);

                    customformEntryModel.Email = metas.Where(m => m.MetaKey.Trim().Equals("email", StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault()?.MetaValue;

                    return customformEntryModel;
                });
            });

            return model;
        }

        public Task<CustomFormSearchModel> PrepareCustomFormEntryMetasSearchModelAsync(CustomFormSearchModel searchModel, int id)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));
            searchModel.Id = id;
            //prepare page parameters
            searchModel.SetGridPageSize();

            return Task.FromResult(searchModel);
        }

        public async Task<CustomFormEntryMetaListModel> PrepareCustomFormEntryMetasEntrySearchListModelAsync(CustomFormSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));


            //get custom forms
            var customformEntryMetas = await _customFormService.GetCustomFormEntryMetas(searchModel.Id, searchModel.Page - 1, searchModel.PageSize);

            //prepare list model
            var model = new CustomFormEntryMetaListModel().PrepareToGrid(searchModel, customformEntryMetas, () =>
            {
                return customformEntryMetas.Select(customformEntryMeta =>
                {
                    var customFormEntryMetaModel = customformEntryMeta.ToModel<CustomFormEntryMetaModel>();
                    return customFormEntryMetaModel;
                });
            });

            return model;
        }







        #endregion
    }
}
