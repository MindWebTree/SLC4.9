using MWT.Nop.Core.Domain.Custom.LandingPage_Management;
using MWT.Plugin.Misc.MwtStorefront.Area.Admin.Models.Catalog.LandingPage_Management;

namespace MWT.Plugin.Misc.MwtStorefront.Area.Admin.Factories
{
    /// <summary>
    /// Represents the QuestionAnswers model factory implementation
    /// </summary>
    public partial interface ILandingPageModelFactory
    {
        Task<LandingPageSearchModel> PrepareLandingPageSearchModelAsync(LandingPageSearchModel searchModel);
        Task<LandingPageListModel> PrepareLandingPageListModelAsync(LandingPageSearchModel searchModel);
        Task<LandingPageModel> PrepareLandingPageModelAsync(LandingPageModel model, LandingPage landingPage, bool excludeProperties = false);
    }
}
