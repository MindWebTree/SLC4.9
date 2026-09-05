using Microsoft.AspNetCore.Mvc;
using MWT.Nop.Core.Domain.Custom.Campaign_Management;
using MWT.Nop.Core.Service.Campaign_Management; 
using MWT.Plugin.Misc.MwtStorefront.Areas.Admin.Factories;
using MWT.Plugin.Misc.MwtStorefront.Areas.Admin.Models.Campaign_Management;
using Nop.Core.Infrastructure;
using Nop.Services.Messages;
using Nop.Web.Areas.Admin.Controllers;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using System.Dynamic;
using System.Globalization;
using System.Net;


namespace MWT.Plugin.Misc.MwtStorefront.Areas.Admin.Controllers
{

    public class CampaignManagementController : BaseAdminController
    {
        #region 

        private readonly ICampaignManagementModelFactory _campaignManagementModelFactory;
        private readonly ICampaignManagementService _campaignManagementService;
        private readonly INopFileProvider _fileProvider;
        private readonly INotificationService _notificationService;
        #endregion

        #region Ctor

        public CampaignManagementController(ICampaignManagementModelFactory campaignManagementModelFactory,
             ICampaignManagementService campaignManagementService, INopFileProvider fileProvider,
             INotificationService notificationService)
        {
            _campaignManagementModelFactory = campaignManagementModelFactory;
            _campaignManagementService = campaignManagementService;
            _fileProvider = fileProvider;
            _notificationService = notificationService;
        }

        #endregion

        #region Campaigns
        public virtual async Task<IActionResult> List(string sortDirection, string sortBy, string search, int status = 0)
        {
            return View(await _campaignManagementModelFactory.PrepareListOfCampaign(sortDirection, sortBy, search, status));
        }

        [HttpPost]
        public virtual async Task<IActionResult> UpdateCampaign(CampaignUpdateActionmodel campaignUpdateActionmodel)
        {
            var campaign = await _campaignManagementService.GetCampaignByid(campaignUpdateActionmodel.Id);

            try
            {
                if (campaign != null)
                {
                    if (campaignUpdateActionmodel.Actiontype == CampaignUpdateActiontype.StatusChange)
                    {
                        campaign.UpdatedOn = DateTime.Now;
                        campaign.IsActive = campaignUpdateActionmodel.Status;
                        await _campaignManagementService.UpdateCampaign(campaign);
                        TempData["Message"] = campaignUpdateActionmodel.Status ? "Campaign Activated Successfully" :
                            "Campaign De Activated Successfully";
                    }
                    else if (campaignUpdateActionmodel.Actiontype == CampaignUpdateActiontype.Delete)
                    {
                        campaign.IsDeleted = true;
                        campaign.IsActive = false;
                        await _campaignManagementService.UpdateCampaign(campaign);
                        TempData["Message"] = "Campaign Deleted Successfully";
                    }
                    else if (campaignUpdateActionmodel.Actiontype == CampaignUpdateActiontype.Clone)
                    {
                        await _campaignManagementService.CloneCampaign(campaign);
                        TempData["Message"] = "Campaign Cloned Successfully";

                    }
                }
            }
            catch (Exception exp)
            {

            }
            dynamic data = new ExpandoObject();
            data.Status = HttpStatusCode.OK;
            return Json(data);
        }


        public virtual async Task<IActionResult> CampaignCreate()
        {
            return View(await _campaignManagementModelFactory.PrepareCampaignModel(new CampaignCreateModel()));
        }
        [HttpPost]
        public virtual async Task<IActionResult> CampaignCreate(CampaignCreateModel model)
        {
            if (ModelState.IsValid)
            {
                CampaignTargetingSave domain = new CampaignTargetingSave();
                string popname = model.Name;
                int responsetemplateid = 0;
                int templateid = 0;
              //  var path = _fileProvider.GetAbsolutePath(@"Plugins/MWT.Plugin.Misc.MwtStorefront/Content/CampaignAssets/Template Css/TemplateDefaultStyle.txt");

                var path = Path.Combine(
    Directory.GetCurrentDirectory(),
    "Plugins",
    "MWT.Plugin.Misc.MwtStorefront",
    "Content",
    "CampaignAssets",
    "Template Css",
    "TemplateDefaultStyle.txt"
);
                string template_style = _fileProvider.ReadAllText(path, System.Text.Encoding.UTF8); // rading pre html

                var template_data = model.TemplateDesign;
                template_data += template_style;
                template_data += GenrateScriptsCustomTemplate(model);
                domain.Name = model.Name;
                var template = new MWT_CampaignTemplates()
                {
                    HtmlContent = template_data,
                    Name = popname,
                    CreatedOn = DateTime.Now,
                    UpdatedOn = DateTime.Now
                };
                await _campaignManagementService.InsertTemplate(template);
                templateid = template.Id;
                var response_template_data = model.ResponseTemplateDesign;
                if (response_template_data != null && response_template_data != "")
                {
                    response_template_data += template_style;
                    response_template_data += GenrateScriptsCustomTemplate(model);

                    template = new MWT_CampaignTemplates()
                    {
                        HtmlContent = response_template_data,
                        Name = popname,
                        CreatedOn = DateTime.Now,
                        UpdatedOn = DateTime.Now
                    };
                    await _campaignManagementService.InsertTemplate(template);
                    responsetemplateid = template.Id;
                }
                domain.TemplateId = templateid;
                domain.ResponseTemplateId = responsetemplateid;

                string[] formats = {"M/d/yyyy h:mm:ss tt", "M/d/yyyy h:mm tt",
                   "MM/dd/yyyy hh:mm:ss", "M/d/yyyy h:mm:ss",
                   "M/d/yyyy hh:mm tt", "M/d/yyyy hh tt",
                   "M/d/yyyy h:mm", "M/d/yyyy h:mm",
                   "MM/dd/yyyy hh:mm", "M/dd/yyyy hh:mm"};
                if (model.IsScheduled)
                {
                    DateTime date;
                    if (!DateTime.TryParseExact(model.StartDate == null ? "" : model.StartDate.ToString(), formats,
                                        new CultureInfo("en-US"),
                                        DateTimeStyles.None,
                                        out date))
                    {
                        ModelState.AddModelError("Start Date", "Start Date is not valid");
                        return View(await _campaignManagementModelFactory.PrepareCampaignModel(model));
                    }
                    if (!DateTime.TryParseExact(model.EndDate == null ? "" : model.EndDate.ToString(), formats,
                                        new CultureInfo("en-US"),
                                        DateTimeStyles.None,
                                        out date))
                    {
                        ModelState.AddModelError("Start Date", "Start Date is not valid");
                        return View(await _campaignManagementModelFactory.PrepareCampaignModel(model));
                    }
                    domain.StartDate = model.StartDate;
                    domain.EndDate = model.EndDate;
                }
                else
                {
                    domain.StartDate = null;
                    domain.EndDate = null;
                }


                string cond = "";
                if (model.IsGlobal)
                {
                    foreach (var target in model.Targets)
                    {
                        if (!string.IsNullOrEmpty(target.Url))
                        {
                            if (cond == "")
                                cond += target.ConditionId + "," + target.Url;
                            else
                                cond += "|" + target.ConditionId + "," + target.Url;
                        }
                    }
                }
                else
                    cond = "/";
                domain.Conditions = cond;
                domain.IsExitPopup = model.IsExitPopup;
                domain.TriggerOn = model.TriggerOn;
                domain.FrequencyId = model.FrequencyId;
                domain.DeviceType = model.DeviceType;
                domain.IsGlobal = model.IsGlobal;
                domain.Position = model.Position;
                domain.IconPosition = model.IconPosition;
                domain.TemplateId = templateid;
                domain.ResponseTemplateId = responsetemplateid;
                if (model.IconType == IconType.Image && model.HaveIconImage && model.IconImage != null && (model.IconImage.FileName.ToLower().Contains(".jpg") || model.IconImage.FileName.ToLower().Contains(".png")))
                {
                    string iconImage = string.Empty;
                    Random random = new Random();
                    iconImage = random.Next() + model.IconImage.FileName;
                    path = _fileProvider.GetAbsolutePath(@"\campaignassets\Uploads");
                    path = _fileProvider.Combine(path, iconImage);
                    using var fileStream = new FileStream(path, FileMode.Create);
                    model.IconImage.CopyTo(fileStream);
                    domain.IconImageSrc = "/Plugins/MWT.Plugin.Misc.MwtStorefront/Content/campaignassets/Uploads/" + iconImage;
                    domain.IconHtml = "";
                }
                else if (model.HaveIconImage && model.IconType == IconType.Html)
                {
                    domain.IconImageSrc = "";
                    domain.IconHtml = model.IconHtml;
                }
                else
                {
                    domain.IconImageSrc = "";
                    domain.IconHtml = "";
                }
                await _campaignManagementService.SaveCampaign(domain);
                _notificationService.SuccessNotification("Campaign Saved Successfully !!!");
                return RedirectToAction("List");
            }

            return View(await _campaignManagementModelFactory.PrepareCampaignModel(model));
        }

        #endregion

        #region Campaign Targets

        public virtual async Task<IActionResult> Settings(int campaignId)
        {
            var model = await _campaignManagementModelFactory.PrepareCampaignTargetModel(campaignId);
            if (model.CampaignId == 0)
                return RedirectToAction("List");
            return View(model);
        }


        [HttpPost]
        public virtual async Task<IActionResult> Settings(CampaignTargetModel model)
        {
            CampaignTargetingSave targetDomain = new CampaignTargetingSave();
            targetDomain.CampaignId = model.CampaignId;
            targetDomain.IsActive = true;

            if (!model.IsScheduled)
            {
                targetDomain.StartDate = null;
                targetDomain.EndDate = null;

            }
            else
            {
                targetDomain.StartDate = model.StartDate;
                targetDomain.EndDate = model.EndDate;
            }

            string cond = "";
            if (model.IsGlobal)
            {
                foreach (var target in model.Targets)
                {
                    if (!string.IsNullOrEmpty(target.Url))
                    {
                        if (cond == "")
                            cond += target.ConditionId + "," + target.Url;
                        else
                            cond += "|" + target.ConditionId + "," + target.Url;
                    }
                }
            }
            else
                cond = "/";
            targetDomain.Conditions = cond;
            targetDomain.IsExitPopup = model.IsExitPopup;
            targetDomain.TriggerOn = model.TriggerOn;
            targetDomain.FrequencyId = model.FrequencyId;
            targetDomain.DeviceType = model.DeviceType;
            targetDomain.IsGlobal = model.IsGlobal;
            targetDomain.Position = model.Position;
            targetDomain.IconPosition = model.IconPosition;
            if (model.HaveIconImage)
            {
                if (model.IconType == IconType.Image && model.IconImage != null && (model.IconImage.FileName.ToLower().Contains(".jpg") || model.IconImage.FileName.ToLower().Contains(".png")))
                {
                    string iconImage = string.Empty;
                    Random random = new Random();
                    iconImage = random.Next() + model.IconImage.FileName;
                    var path = _fileProvider.GetAbsolutePath(@"\campaignassets\Uploads");
                    path = _fileProvider.Combine(path, iconImage);
                    using var fileStream = new FileStream(path, FileMode.Create);
                    model.IconImage.CopyTo(fileStream);
                    targetDomain.IconImageSrc = "/Plugins/MWT.Plugin.Misc.MwtStorefront/Content/campaignassets/Uploads/" + iconImage;
                    targetDomain.IconHtml = "";
                }
                else if (model.IconType == IconType.Html)
                {
                    targetDomain.IconImageSrc = "";
                    targetDomain.IconHtml = model.IconHtml;
                }
                else
                {
                    if (model.IconType == IconType.Image)
                    {
                        targetDomain.IconImageSrc = model.IconImageSrc;
                    }
                    else
                    {
                        targetDomain.IconImageSrc = "";
                    }
                    targetDomain.IconHtml = "";
                }
            }
            else if (model.HaveIconImage)
                targetDomain.IconImageSrc = model.IconImageSrc;
            else
                targetDomain.IconImageSrc = "";

            await _campaignManagementService.SaveCampaignTarget(targetDomain);
            _notificationService.SuccessNotification("Campaign Updated Successfully !!!");

            return RedirectToAction("Settings", new { campaignId = model.CampaignId });
        }



        #endregion

        #region Templates
        public virtual async Task<IActionResult> DesignSetting(int campaignId)
        {
            var model = await _campaignManagementModelFactory.PrepareDesignSettingModel(campaignId);
            if (model.Id == 0)
                return RedirectToAction("List");
            return View(model);
        }
        [HttpPost]
        public virtual async Task<IActionResult> DesignSetting(DesignSettingmodel model)
        {
            if (model.Id == 0)
                return RedirectToAction("List");

            var campaign = await _campaignManagementService.GetCampaignByid(model.Id);
            if (campaign == null)
                return RedirectToAction("list");
            campaign.Title = model.Name;
            campaign.TemplateId = model.TemplateId;
            campaign.ResponseTemplateId = model.ResponseTemplateId;
            campaign.SubscribedUserResponseTemplateId = model.SubscribedUserResponseTemplateId;
            await _campaignManagementService.UpdateCampaign(campaign);

            _notificationService.SuccessNotification("Campaign Updated Successfully !!!");

            return RedirectToAction("DesignSetting", new { campaignId = model.Id });

        }

        public virtual async Task<IActionResult> Templates()
        {
            var templates = await _campaignManagementService.GetTemplates();

            List<DesignModel> lstModel = new List<DesignModel>();
            foreach (var template in templates)
            {
                lstModel.Add(template.ToModel<DesignModel>());
            }
            return View(lstModel);
        }


        public virtual async Task<IActionResult> TemplateCreate()
        {
            return View(new DesignModel());
        }

        public virtual async Task<IActionResult> TemplateEdit(int templateId)
        {
            var template = await _campaignManagementService.GetCampaignTemplateByid(templateId);
            if (template == null)
                return RedirectToAction("Templates");

            return View(template.ToModel<DesignModel>());
        }

        [HttpPost]
        public virtual async Task<IActionResult> TemplateUpdate(DesignModel model)
        {
            if (model.Id == 0)
            {
                var entity = model.ToEntity<MWT_CampaignTemplates>();
                entity.CreatedOn = DateTime.Now;
                entity.UpdatedOn = DateTime.Now;
                await _campaignManagementService.InsertTemplate(entity);

                _notificationService.SuccessNotification("Template Added Successfully !!!");
            }
            else
            {
                var template = await _campaignManagementService.GetCampaignTemplateByid(model.Id);
                if (template == null)
                    return RedirectToAction("Templates");
                template.Name = model.Name;
                template.HtmlContent = model.HtmlContent;
                template.UpdatedOn = DateTime.Now;
                await _campaignManagementService.UpdateTemplate(template);
                _notificationService.SuccessNotification("Template Updated Successfully !!!");
            }
            return RedirectToAction("Templates");
        }

        [HttpPost]
        public virtual async Task<IActionResult> UpdateTemplate(CampaignUpdateActionmodel campaignUpdateActionmodel)
        {
            var template = await _campaignManagementService.GetCampaignTemplateByid(campaignUpdateActionmodel.Id);

            try
            {
                if (template != null)
                {

                    if (campaignUpdateActionmodel.Actiontype == CampaignUpdateActiontype.Delete)
                    {
                        template.IsDeleted = true;
                        template.UpdatedOn = DateTime.Now;
                        await _campaignManagementService.UpdateTemplate(template);
                        TempData["Message"] = "Template Deleted Successfully";
                    }
                    else if (campaignUpdateActionmodel.Actiontype == CampaignUpdateActiontype.Clone)
                    {
                        template.Name = template.Name + " Clone";
                        await _campaignManagementService.InsertTemplate(template);
                        TempData["Message"] = "Template Cloned Successfully";

                    }
                }
            }
            catch (Exception exp)
            {

            }
            dynamic data = new ExpandoObject();
            data.Status = HttpStatusCode.OK;
            return Json(data);
        }


        #endregion

        #region Utilities
        public string GenrateScriptsCustomTemplate(CampaignCreateModel model)
        {
            string script = "<script type=\"text/javascript\">";
            script += "function eventAfterFormSubmission(){try{" + model.SubmitButtonClick + "}catch(err) {}}";
            script = script + "$(document).ready( function () {";
            try
            {
                if ((model.EmailTextBoxClick ?? "").Trim() != "")
                    script += "$('#campaignformemail').click(function(){try{" + model.EmailTextBoxClick + "}catch(err) {}});";
                if ((model.CloseButtonClick ?? "").Trim() != "")
                {
                    if (model.CampaignDeviceType == 1)
                        script += "$('.close-campaign-popup').click(function(){try{if($('#campaignformemail').length>0){" + model.CloseButtonClick + "}}catch(err) {}});";
                    else
                        script += "$('.campaignPopupClose').click(function(){try{if($('#campaignformemail').length>0){" + model.CloseButtonClick + "}}catch(err) {}});";
                }
                if ((model.ResponseCloseButtonClick ?? "").Trim() != "")
                {
                    if (model.CampaignDeviceType == 1)
                        script += "$('.close-campaign-popup').click(function(){try{if($('#campaignformemail').length>0){" + model.ResponseCloseButtonClick + "}}catch(err) {}});";
                    else
                        script += "$('.campaignPopupClose').click(function(){try{if($('#campaignformemail').length>0){" + model.ResponseCloseButtonClick + "}}catch(err) {}});";
                }
                script += "});</script>";
                return script;
            }
            catch { return ""; }
        }
        #endregion


    }
}
