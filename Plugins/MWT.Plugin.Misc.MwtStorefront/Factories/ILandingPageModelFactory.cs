using MWT.Nop.Core.Domain.Custom.LandingPage_Management;
using MWT.Plugin.Misc.MwtStorefront.Models.LandingPage_Management;

namespace MWT.Plugin.Misc.MwtStorefront.Factories
{
    public partial interface ILandingPageModelFactory
    {
        Task<LandingPageModel> PrepareCustomLandingPageModelAsync(LandingPage landingPage);
    }
}
