using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;
using System;
using System.Collections.Generic;

namespace Nop.Web.Areas.Admin.Models.Customization.Custom.Campaign_Management
{
    public partial record CampaignTargetModel : BaseNopEntityModel
    {
        public CampaignTargetModel()
        {
            CampaignFreQuency = new List<SelectListItem>();
            UrlConditions = new List<SelectListItem>();
            PopupPositions = new List<SelectListItem>();
            IconPositions = new List<SelectListItem>();
            DeviceTypes = new List<SelectListItem>();
            Targets = new List<CampaignTargetOverView>();
        }


        public List<SelectListItem> CampaignFreQuency { get; set; }
        public List<SelectListItem> UrlConditions { get; set; }
        public List<SelectListItem> PopupPositions { get; set; }
        public List<SelectListItem> IconPositions { get; set; }
        public List<SelectListItem> DeviceTypes { get; set; }
        public int CampaignId { get; set; }

        [NopResourceDisplayName("CampaignManagement.Fields.TriggerOn")]
        public int? TriggerOn { get; set; }
        [NopResourceDisplayName("CampaignManagement.Fields.FrequencyId")]
        public int? FrequencyId { get; set; }
        [NopResourceDisplayName("CampaignManagement.Fields.DeviceType")]
        public int? DeviceType { get; set; }
        [NopResourceDisplayName("CampaignManagement.Fields.IconPosition")]
        public int? IconPosition { get; set; }
        [NopResourceDisplayName("CampaignManagement.Fields.Position")]
        public int? Position { get; set; }

        [NopResourceDisplayName("CampaignManagement.Fields.IconImageSrc")]
        public string IconImageSrc { get; set; }

        [NopResourceDisplayName("CampaignManagement.Fields.IconHtml")]
        public string IconHtml{ get; set; }
        public IFormFile IconImage { get; set; }
        [NopResourceDisplayName("CampaignManagement.Fields.StartDate")]
        public DateTime? StartDate { get; set; }
        [NopResourceDisplayName("CampaignManagement.Fields.EndDate")]
        public DateTime? EndDate { get; set; }

        public List<CampaignTargetOverView> Targets { get; set; }
        [NopResourceDisplayName("CampaignManagement.Fields.IsScheduled")]
        public bool IsScheduled { get; set; }
        [NopResourceDisplayName("CampaignManagement.Fields.IsGlobal")]
        public bool IsGlobal { get; set; }
        [NopResourceDisplayName("CampaignManagement.Fields.IsExitPopup")]
        public bool IsExitPopup { get; set; }
        [NopResourceDisplayName("CampaignManagement.Fields.HaveIconImage")]
        public bool HaveIconImage { get; set; }
        public string Conditions { get; set; }
        public IconType IconType { get; set; }
    }

    public enum IconType
    {
        Image=1,
        Html=2
    }
    public partial record CampaignTargetOverView
    {
        public string Url { get; set; }
        public int ConditionId { get; set; }
        public string Name { get; set; }
    }
}
