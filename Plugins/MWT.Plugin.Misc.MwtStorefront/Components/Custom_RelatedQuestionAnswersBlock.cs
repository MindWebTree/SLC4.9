using Microsoft.AspNetCore.Mvc;
using MWT.Nop.Core.Services.QA;
using MWT.Plugin.Misc.MwtStorefront.Models.QA;
using Nop.Services.Configuration;
using Nop.Services.Security;
using Nop.Services.Seo;
using Nop.Services.Stores;
using Nop.Web.Factories;
using Nop.Web.Framework.Components;

namespace MWT.Plugin.Misc.MwtStorefront.Components
{
    public class Custom_RelatedQuestionAnswersBlockViewComponent : NopViewComponent
    {
        private readonly IAclService _aclService;
        private readonly IProductModelFactory _productModelFactory;
        private readonly IQuestionAnswerService _questionAnswerService;
        private readonly IStoreMappingService _storeMappingService;
        private readonly ISettingService _settingService;
        private readonly IUrlRecordService _urlRecordService;

        public Custom_RelatedQuestionAnswersBlockViewComponent(IAclService aclService,
            IProductModelFactory productModelFactory,
            IQuestionAnswerService questionAnswerService,
            IStoreMappingService storeMappingService,
            ISettingService settingService,
            IUrlRecordService urlRecordService)
        {
            _aclService = aclService;
            _productModelFactory = productModelFactory;
            _questionAnswerService = questionAnswerService;
            _storeMappingService = storeMappingService;
            _settingService = settingService;
            _urlRecordService = urlRecordService;
        }

        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task<IViewComponentResult> InvokeAsync(int questionAnswerId)
        {
            int noOfRelatedQuestionAnswers = 10;

            try
            {
                noOfRelatedQuestionAnswers = await _settingService.GetSettingByKeyAsync<int>("CatalogSettings.RelatedQuestionAnswers");
            }
            catch
            {
            }
            //load and cache report
            var relatedQuestionAnswerIds = (await _questionAnswerService.GetRelatedQuestionAnswersByQuestionAnswerId1Async(questionAnswerId, false)).Select(q => q.QuestionAnswerId2).Distinct().Take(noOfRelatedQuestionAnswers);

            //load products
            var questionAnswers = await (await _questionAnswerService.GetQuestionAnswersByIdsAsync(relatedQuestionAnswerIds.ToArray()))
            //ACL and store mapping
            .WhereAwait(async p => await _aclService.AuthorizeAsync(p) && await _storeMappingService.AuthorizeAsync(p)).ToListAsync();


            if (!questionAnswers.Any())
                return Content(string.Empty);

            List<QuestionAnswerModel> listQuestionAnswerModel = new List<QuestionAnswerModel>();
            foreach(var questionAnswer in questionAnswers)
            {
                listQuestionAnswerModel.Add(new QuestionAnswerModel()
                {
                    Name = questionAnswer.Name,
                    Id = questionAnswer.Id,
                    SeName = await _urlRecordService.GetSeNameAsync(questionAnswer),
                });
            }
            return View(listQuestionAnswerModel);
        }
    }
}