using MWT.Nop.Core.Service.FAQModule;
using MWT.Plugin.Misc.MwtStorefront.Models.FAQModule;

namespace MWT.Plugin.Misc.MwtStorefront.Factories
{
    public partial class FaqModelFactory:IFaqModelFactory
    {
        #region Fields

        private readonly IFaqService _faqService;

        #endregion

        #region Ctor

        public FaqModelFactory(IFaqService faqService)
        {
            _faqService = faqService;
            _faqService = faqService;
        }

        #endregion

        #region Methods

        public async Task<List<FaqModel>> PrepareFaqListModelAsync(int entityId, string entityType)
        {
            List<FaqModel> model = new List<FaqModel>();
            var faqList = await _faqService.GetFaqByEntity(entityId, entityType);

            foreach (var relatedSearch in faqList)
            {
          
                    model.Add(new FaqModel()
                    {
                        Question = relatedSearch.Question,
                        Answer = relatedSearch.Answer
                    });
            }

            return model;

        }

        #endregion
    }
}
