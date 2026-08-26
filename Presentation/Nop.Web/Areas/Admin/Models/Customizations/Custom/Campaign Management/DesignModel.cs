using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Web.Areas.Admin.Models.Customization.Custom.Campaign_Management
{
    public partial record DesignModel:BaseNopEntityModel
    {
        [NopResourceDisplayName("CampaignManagement.Template.Fields.Template.Name")]
        public string Name { get; set; }

        [NopResourceDisplayName("CampaignManagement.Template.Fields.HtmlContent")]
        public string HtmlContent { get; set; }
    }
}
