using MWT.Nop.Core.Domain.KW;
using Nop.Web.Areas.Admin.Models.Customization.Custom.KW;

namespace Nop.Web.Areas.Admin.Factories.Customization
{
    /// <summary>
    /// Represents the kwTerms model factory implementation
    /// </summary>
    public partial interface IKwTermModelFactory
    {
        Task<KwTermSearchModel> PrepareKwTermSearchModelAsync(KwTermSearchModel searchModel);
        Task<KwTermListModel> PrepareKwTermListModelAsync(KwTermSearchModel searchModel);
        Task<KwTermModel> PrepareKwTermModelAsync(KwTermModel model, KwTerm kwTerm, bool excludeProperties = false);
        Task<AddProductToKwTermListModel> CustomPrepareAddProductToKwTermListModelAsync(AddProductToKwTermSearchModel searchModel);
        Task<KwTermProductListModel> CustomPrepareKwTermProductListModelAsync(KwTermProductSearchModel searchModel, KwTerm kwTerms);
        Task<AddProductToKwTermSearchModel> PrepareAddProductToKwTermSearchModelAsync(AddProductToKwTermSearchModel searchModel);

        #region Categories

        Task<KwTermCategoryListModel> CustomPrepareKwTermCategoryListModelAsync(KwTermCategorySearchModel searchModel, KwTerm kwTerms);
        Task<AddCategoryToKwTermListModel> CustomPrepareAddCategoryToKwTermListModelAsync(AddCategoryToKwTermSearchModel searchModel);
        Task<AddCategoryToKwTermSearchModel> PrepareAddCategoryToKwTermSearchModelAsync(AddCategoryToKwTermSearchModel searchModel);

        #endregion
    }
}
