using Nop.Web.Framework.Models;

namespace Nop.Web.Areas.Admin.Models.Customization.Custom.Campaign_Management
{
    public partial record CampaignUpdateActionmodel:BaseNopEntityModel
    {
        public CampaignUpdateActiontype Actiontype { get; set; }
        public bool Status { get; set; }


    }

    public enum CampaignUpdateActiontype
    {
        StatusChange,
        Clone,
        Delete
    }
}
