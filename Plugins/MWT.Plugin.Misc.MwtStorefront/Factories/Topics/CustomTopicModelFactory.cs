using MWT.Plugin.Misc.MwtStorefront.Models.Topics;
using Nop.Core;
using Nop.Core.Domain.Topics;
using Nop.Services.Localization;
using Nop.Services.Seo;
using Nop.Services.Topics;
using Nop.Web.Factories;
using Nop.Web.Models.Topics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MWT.Plugin.Misc.MwtStorefront.Factories.Topics
{
    public partial class CustomTopicModelFactory : TopicModelFactory, ICustomTopicModelFactory
    {
        public CustomTopicModelFactory(ILocalizationService localizationService, IStoreContext storeContext, ITopicService topicService, ITopicTemplateService topicTemplateService, IUrlRecordService urlRecordService) : base(localizationService, storeContext, topicService, topicTemplateService, urlRecordService)
        {
        }

        #region Methods
        public virtual async Task<CustomTopicModel> CustomPrepareTopicModelByIdAsync(Topic topic)
        {
            return await CustomPrepareTopicModelAsync(topic);
        }


        #endregion

        #region Utilities
        protected virtual async Task<CustomTopicModel> CustomPrepareTopicModelAsync(Topic topic)
        {
            if (topic == null)
                throw new ArgumentNullException(nameof(topic));

            var model = new CustomTopicModel
            {
                Id = topic.Id,
                SystemName = topic.SystemName,
                IncludeInSitemap = topic.IncludeInSitemap,
                IsPasswordProtected = topic.IsPasswordProtected,
                Title = topic.IsPasswordProtected ? string.Empty : await _localizationService.GetLocalizedAsync(topic, x => x.Title),
                Body = topic.IsPasswordProtected ? string.Empty : await _localizationService.GetLocalizedAsync(topic, x => x.Body),
                MetaKeywords = await _localizationService.GetLocalizedAsync(topic, x => x.MetaKeywords),
                MetaDescription = await _localizationService.GetLocalizedAsync(topic, x => x.MetaDescription),
                MetaTitle = await _localizationService.GetLocalizedAsync(topic, x => x.MetaTitle),
                SeName = await _urlRecordService.GetSeNameAsync(topic),
                TopicTemplateId = topic.TopicTemplateId,
                HideDefaultTitle = topic.HideDefaultTitle
            };

            return model;
        }
        #endregion

    }

}

