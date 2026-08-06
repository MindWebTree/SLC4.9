using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace MWT.Plugin.Misc.MwtStorefront.Areas.Admin.Models.Campaign_Management
{
    public partial record DesignModel:BaseNopEntityModel
    {
        [NopResourceDisplayName("CampaignManagement.Template.Fields.Template.Name")]
        public string Name { get; set; }

        [NopResourceDisplayName("CampaignManagement.Template.Fields.HtmlContent")]
        public string HtmlContent { get; set; }
    }
}
