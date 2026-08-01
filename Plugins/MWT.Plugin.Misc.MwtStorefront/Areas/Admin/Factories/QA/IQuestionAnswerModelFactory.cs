using MWT.Nop.Core.Domain.QA;
using MWT.Plugin.Misc.MwtStorefront.Areas.Admin.Models.QA;
using System.Threading.Tasks;

namespace MWT.Plugin.Misc.MwtStorefront.Area.Admin.Factories.QA
{
    /// <summary>
    /// Represents the QuestionAnswers model factory implementation
    /// </summary>
    public partial interface IQuestionAnswerModelFactory
    {
        Task<QuestionAnswerSearchModel> PrepareQuestionAnswerSearchModelAsync(QuestionAnswerSearchModel searchModel);
        Task<QuestionAnswerListModel> PrepareQuestionAnswerListModelAsync(QuestionAnswerSearchModel searchModel);
        Task<QuestionAnswerModel> PrepareQuestionAnswerModelAsync(QuestionAnswerModel model, QuestionAnswer QuestionAnswer, bool excludeProperties = false);
        Task<AddProductToQuestionAnswerListModel> PrepareAddProductToQuestionAnswerListModelAsync(AddProductToQuestionAnswerSearchModel searchModel);
        Task<QuestionAnswerProductListModel> PrepareQuestionAnswerProductListModelAsync(QuestionAnswerProductSearchModel searchModel, QuestionAnswer QuestionAnswers);
        Task<AddProductToQuestionAnswerSearchModel> PrepareAddProductToQuestionAnswerSearchModelAsync(AddProductToQuestionAnswerSearchModel searchModel);

        #region Related

        Task<RelatedQuestionAnswerListModel> PrepareRelatedQuestionAnswerListModelAsync(RelatedQuestionAnswerSearchModel searchModel, QuestionAnswer questionAnswer);
        Task<AddRelatedQuestionAnswerSearchModel> PrepareAddRelatedQuestionAnswerSearchModelAsync(AddRelatedQuestionAnswerSearchModel searchModel);
        Task<AddRelatedQuestionAnswerListModel> PrepareAddRelatedQuestionAnswerListModelAsync(AddRelatedQuestionAnswerSearchModel searchModel);

        #endregion

    }
}
