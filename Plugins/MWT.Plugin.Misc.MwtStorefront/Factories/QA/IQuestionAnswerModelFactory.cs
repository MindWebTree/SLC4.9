using MWT.Nop.Core.Domain.QA;
using MWT.Plugin.Misc.MwtStorefront.Models.Catalog;
using MWT.Plugin.Misc.MwtStorefront.Models.QA;
using MWT.Plugin.Misc.MwtStorefront.Models.Seo;
using Nop.Web.Models.Catalog;

namespace MWT.Plugin.Misc.MwtStorefront.Factories.QA
{
    public partial interface IQuestionAnswerModelFactory
    {
        Task<(string templateViewPath, string listingViewPath, string filterViewPath, string FilterViewPathForMobile, int pictureSize, bool isHorizontal)> PrepareQuestionAnswerTemplateViewPathAsync(int templateId);
        Task<QuestionAnswerModel> PrepareQuestionAnswerModelAsync(QuestionAnswer QuestionAnswer, CustomCatalogProductsCommand command, string queryString, int pictureSize);
        Task<CustomCatalogProductsModel> PrepareQuestionAnswerProductsModelAsync(QuestionAnswer QuestionAnswer, CustomCatalogProductsCommand command, string queryString = "", int categoryId = 0, bool forSections = false, int pictureSize = 0);
        Task<QuestionAnswerListModel> PrepareQuestionAnswerList(QuestionAnswerSearchModel searchModel);
    }
}
