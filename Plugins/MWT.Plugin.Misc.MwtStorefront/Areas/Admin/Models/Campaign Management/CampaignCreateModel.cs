using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;
using System;
using System.Collections.Generic;

namespace MWT.Plugin.Misc.MwtStorefront.Areas.Admin.Models.Campaign_Management
{
    public partial record CampaignCreateModel : BaseNopEntityModel
    {
        public CampaignCreateModel()
        {
            CampaignFreQuency = new List<SelectListItem>();
            UrlConditions = new List<SelectListItem>();
            PopupPositions = new List<SelectListItem>();
            IconPositions = new List<SelectListItem>();
            DeviceTypes = new List<SelectListItem>();
            Targets = new List<CampaignTargetOverViewModel>();
        }

        public List<SelectListItem> CampaignFreQuency { get; set; }
        public List<SelectListItem> UrlConditions { get; set; }
        public List<SelectListItem> PopupPositions { get; set; }
        public List<SelectListItem> IconPositions { get; set; }
        public List<SelectListItem> DeviceTypes { get; set; }
        [NopResourceDisplayName("CampaignManagement.Fields.ResponseTemplateDesign")]
        public string ResponseTemplateDesign { get; set; }
        [NopResourceDisplayName("CampaignManagement.Fields.TemplateDesign")]
        public string TemplateDesign { get; set; }
        [NopResourceDisplayName("CampaignManagement.Fields.SubmitButtonClick")]
        public string SubmitButtonClick { get; set; }
        [NopResourceDisplayName("CampaignManagement.Fields.CloseButtonClick")]
        public string CloseButtonClick { get; set; }
        [NopResourceDisplayName("CampaignManagement.Fields.ResponseCloseButtonClick")]
        public string ResponseCloseButtonClick { get; set; }
        [NopResourceDisplayName("CampaignManagement.Fields.EmailTextBoxClick")]
        public string EmailTextBoxClick { get; set; }
        [NopResourceDisplayName("CampaignManagement.Fields.Name")]
        public string Name { get; set; }


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
        public string IconImageSrc { get; set; }
        public IFormFile IconImage { get; set; }

        [NopResourceDisplayName("CampaignManagement.Fields.IconHtml")]
        public string IconHtml { get; set; }

        [NopResourceDisplayName("CampaignManagement.Fields.StartDate")]
        public DateTime? StartDate { get; set; }
        [NopResourceDisplayName("CampaignManagement.Fields.EndDate")]
        public DateTime? EndDate { get; set; }

        public List<CampaignTargetOverViewModel> Targets { get; set; }
        [NopResourceDisplayName("CampaignManagement.Fields.IsScheduled")]
        public bool IsScheduled { get; set; }
        [NopResourceDisplayName("CampaignManagement.Fields.IsGlobal")]
        public bool IsGlobal { get; set; }
        [NopResourceDisplayName("CampaignManagement.Fields.IsExitPopup")]
        public bool IsExitPopup { get; set; }
        [NopResourceDisplayName("CampaignManagement.Fields.HaveIconImage")]
        public bool HaveIconImage { get; set; }
        [NopResourceDisplayName("CampaignManagement.Fields.TriggerOnExit")]
        public bool TriggerOnExit { get; set; }
        [NopResourceDisplayName("CampaignManagement.Fields.DeviceType")]
        public int? CampaignDeviceType { get; set; }
        public IconType IconType { get; set; }

    }
}
