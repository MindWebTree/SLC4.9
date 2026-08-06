
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;
using System.Collections.Generic;

namespace MWT.Plugin.Misc.MwtStorefront.Areas.Admin.Models.Campaign_Management
{
    public partial record DesignSettingmodel:BaseNopEntityModel
    {
        public DesignSettingmodel()
        {
            Templates = new List<SelectListItem>();
      

        }
        [NopResourceDisplayName("CampaignManagement.Campaign.Fields.Template")]
        public int? TemplateId { get; set; }
        [NopResourceDisplayName("CampaignManagement.Campaign.Fields.ResponseTemplate")]
        public int? ResponseTemplateId { get; set; }

        [NopResourceDisplayName("CampaignManagement.Campaign.Fields.SubscribedUser.SubscribedUserResponseTemplate")]
        public int? SubscribedUserResponseTemplateId { get; set; }

        [NopResourceDisplayName("CampaignManagement.Campaign.Fields.Title")]
        public string Name { get; set; }

     
        public List<SelectListItem> Templates { get; set; }
    }
}
