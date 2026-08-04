using MWT.Nop.Core.Domain.Campaign_Management;
using MWT.Nop.Core.Domain.Custom.Campaign_Management;

namespace MWT.Nop.Core.Service.Campaign_Management
{
    public partial interface ICampaignManagementService
    {
        #region Campaign
        Task<MWT_Campaign> GetCampaignByid(int id);
        Task DeleteCampaign(MWT_Campaign campaign);
        Task InsertCampaign(MWT_Campaign id);
        Task UpdateCampaign(MWT_Campaign id);
        Task CloneCampaign(MWT_Campaign campaign);
        Task<IList<CampaignOverView>> GetCampaign(string sortDirection, string sortBy);
        Task SaveCampaign(CampaignTargetingSave campaignTargetingSave);

        Task<MWT_CampaignTemplates> GetCampaignResponseTemplate(int campaignId);
        Task<IList<CampaignPopupResult>> GetCampaignPopup(string url, int deviceType, string ip, string userAgent, bool isExitPopup, bool enabledEmailExclusive);

        Task SavePopupImpression(int customerid, string ip, string country, string city, int eventtype, string url, int campaignId,
            int deviceType, string userAgent);

        Task SavePopupConversion(int customerid, string ip, string country, string city, int eventtype, string url, int campaignId,
       string email,string phone, int deviceType, string userAgent);

        Task<IList<MailchimpCampaignIntegration>> GetCampaignIntegration(int campaignId);
        #endregion

        #region Campaign template
        Task<MWT_CampaignTemplates> GetCampaignTemplateByid(int id);


        #endregion

        #region Frequency

        Task<List<MWT_CampaignFrequency>> CampaignFrequency();
        Task<List<MWT_UrlCondition>> UrlConditions();
        Task<List<MWT_Popup_Position>> PopupConditions();

        #endregion

        #region Campaign Target 

        Task<IList<CampaignTargetOverView>> GetCampaignTargets(int campaignId);
        Task SaveCampaignTarget(CampaignTargetingSave campaignTargetingSave);

        #endregion

        #region Design Template
        Task<List<MWT_CampaignTemplates>> GetTemplates(bool? responseTemplate = null);
        Task InsertTemplate(MWT_CampaignTemplates template);
        Task UpdateTemplate(MWT_CampaignTemplates template);
        #endregion
    }
}
