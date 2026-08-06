

using MWT.Plugin.Misc.MwtStorefront.Areas.Admin.Models.Campaign_Management;

namespace MWT.Plugin.Misc.MwtStorefront.Areas.Admin.Factories
{
    public partial interface ICampaignManagementModelFactory
    {
        Task<List<CampaignModel>> PrepareListOfCampaign(string sortDirection, string sortBy, string search, int status);
        Task<CampaignTargetModel> PrepareCampaignTargetModel(int campaignId);
        Task<DesignSettingmodel> PrepareDesignSettingModel(int campaignId);

        Task<CampaignCreateModel> PrepareCampaignModel(CampaignCreateModel model);
    }
}
