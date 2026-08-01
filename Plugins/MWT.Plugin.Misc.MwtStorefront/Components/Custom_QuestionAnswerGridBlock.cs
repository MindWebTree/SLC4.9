using Microsoft.AspNetCore.Mvc;
using MWT.Plugin.Misc.MwtStorefront.Factories.QA;
using MWT.Plugin.Misc.MwtStorefront.Models.QA;
using Nop.Web.Framework.Components;

namespace MWT.Plugin.Misc.MwtStorefront.Components
{
    public class Custom_QuestionAnswerGridBlockViewComponent : NopViewComponent
    {
        private readonly IQuestionAnswerModelFactory _questionAnswerModelFactory;
        public Custom_QuestionAnswerGridBlockViewComponent(IQuestionAnswerModelFactory questionAnswerModelFactory)
        {
            _questionAnswerModelFactory = questionAnswerModelFactory;
        }
        public async Task<IViewComponentResult> InvokeAsync(int pageSize)
        {
            string pageNumber = Request.Query["pagenumber"];
            int.TryParse(pageNumber, out int page);

            page = page == 0 ? 1 : page;
            QuestionAnswerSearchModel searchModel = new QuestionAnswerSearchModel();
            searchModel.Length = pageSize;
            searchModel.Start = (page - 1) * pageSize;

            return View(await _questionAnswerModelFactory.PrepareQuestionAnswerList(searchModel));
        }
    }
}
