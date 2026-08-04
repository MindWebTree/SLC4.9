using MWT.Nop.Core.Domain.Custom.LandingPage_Management;
using MWT.Plugin.Misc.MwtStorefront.Models.LandingPage_Management;
using Nop.Services.Localization;
using Nop.Services.Seo;

namespace MWT.Plugin.Misc.MwtStorefront.Factories
{
    public partial class LandingPageModelFactory : ILandingPageModelFactory
    {
        #region Fields
        private readonly ILocalizationService _localizationService;
        private readonly IUrlRecordService _urlRecordService;
        #endregion

        #region Ctor

        public LandingPageModelFactory(
            ILocalizationService localizationService,
            IUrlRecordService urlRecordService)
        {
            _localizationService = localizationService;
            _urlRecordService = urlRecordService;
        }

        #endregion
        public virtual async Task<LandingPageModel> PrepareCustomLandingPageModelAsync(LandingPage landingPage)
        {
            if (landingPage == null)
                throw new ArgumentNullException(nameof(landingPage));
            var model = new LandingPageModel
            {
                Id = landingPage.Id,
                Name = await _localizationService.GetLocalizedAsync(landingPage, x => x.Name),
                Description = await _localizationService.GetLocalizedAsync(landingPage, x => x.Description),
                MetaKeywords = await _localizationService.GetLocalizedAsync(landingPage, x => x.MetaKeywords),
                MetaDescription = await _localizationService.GetLocalizedAsync(landingPage, x => x.MetaDescription),
                MetaTitle = await _localizationService.GetLocalizedAsync(landingPage, x => x.MetaTitle),
                SeName = await _urlRecordService.GetSeNameAsync(landingPage),
                PictureId = landingPage.PictureId,
                DisplayOrder = landingPage.DisplayOrder,
                Published = landingPage.Published,
                Deleted = landingPage.Deleted,

                PageContent = landingPage.PageContent,
            };
            return model;
        }
    }
}
