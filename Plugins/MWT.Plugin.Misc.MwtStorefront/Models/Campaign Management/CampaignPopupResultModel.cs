namespace MWT.Plugin.Misc.MwtStorefront.Models.Catalog.Campaign_Management
{
    public class CampaignPopupResultModel
    {
        public int CampaignId { get; set; }
        public string Title { get; set; }
        public bool showOnExit { get; set; }
        public int triggerOn_Secs { get; set; }
        public string htmlContent { get; set; }
        public int duration_Days { get; set; }
        public string message { get; set; }
        public bool static_message { get; set; }
        public bool isError { get; set; }
        public string positionStyle { get; set; }
        public string iconPositionStyle { get; set; }
        public string iconImage { get; set; }
        public bool isSideBarDisplayed { get; set; }
        public bool isUserSubscribed { get; set; }
        public bool isPopupDisplayed { get; set; }
        public string iconHtml { get; set; }
        public bool isEmailExclusiveOfferEnabled { get; set; }
    }
}
