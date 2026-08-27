using MWT.Nop.Core.Service.FAQModule;
using Nop.Web.Areas.Admin.Models.FAQModule;
using Nop.Web.Framework.Models.Extensions;

namespace Nop.Web.Areas.Admin.Factories
{
    public partial class FaqModelFactory : IFaqModelFactory
    {

        #region Fields

        private readonly IFaqService _faqService;

        #endregion

        #region Ctor

        public FaqModelFactory(IFaqService faqService)
        {
            _faqService = faqService;
        }

        #endregion

        #region Methods

        public async Task<FaqListModel> PrepareFaqListModelAsync(FaqSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));



            var faqList = (await _faqService
                .GetFaqsByEntityIdAsync(entityId: searchModel.EntityId, entityType: searchModel.EntityType)).ToPagedList(searchModel);

            var model = await new FaqListModel().PrepareToGridAsync(searchModel, faqList, () =>
            {
                return faqList.SelectAwait(async faq =>
                {
                    FaqModel faqModel = new FaqModel();
                    faqModel.Id = faq.Id;
                    faqModel.EntityId = faq.EntityId;
                    faqModel.EntityType = faq.EntityType;
                    faqModel.Question = faq.Question;
                    faqModel.Answer = faq.Answer;
                    faqModel.DisplayOrder = faq.DisplayOrder;
                    faqModel.CreatedOnUtc = faq.CreatedOnUtc;
                    faqModel.UpdatedOnUtc = faq.UpdatedOnUtc;
                    return faqModel;
                });
            });
            return model;
        }

        #endregion
    }
}
