using Nop.Web.Framework.Models;

namespace MWT.Plugin.Misc.MwtStorefront.Areas.Admin.Models.Campaign_Management
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
