using MWT.Nop.Core.Domain.Custom.Campaign_Management;
using MWT.Nop.Core.Service.Campaign_Management;
using MWT.Plugin.Misc.MwtStorefront.Areas.Admin.Models.Campaign_Management;

namespace MWT.Plugin.Misc.MwtStorefront.Areas.Admin.Factories
{
    public partial class CampaignManagementModelFactory : ICampaignManagementModelFactory
    {
        #region Fields

        private readonly ICampaignManagementService _campaignManagementService;

        #endregion

        #region Ctor

        public CampaignManagementModelFactory(ICampaignManagementService campaignManagementService)
        {
            _campaignManagementService = campaignManagementService;
        }

        #endregion

        public async Task<CampaignCreateModel> PrepareCampaignModel(CampaignCreateModel model)
        {

            var frequencies = await _campaignManagementService.CampaignFrequency();
            foreach (var frequency in frequencies)
            {
                model.CampaignFreQuency.Add(new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem()
                {
                    Text = frequency.Name,
                    Value = frequency.Id.ToString()

                });
            }

            var urlConditions = await _campaignManagementService.UrlConditions();
            foreach (var urlCondition in urlConditions)
            {
                model.UrlConditions.Add(new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem()
                {
                    Text = urlCondition.Name,
                    Value = urlCondition.Id.ToString()

                });
            }

            var popupPositions = await _campaignManagementService.PopupConditions();
            foreach (var popupPosition in popupPositions)
            {
                model.PopupPositions.Add(new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem()
                {
                    Text = popupPosition.PositionName,
                    Value = popupPosition.Id.ToString()

                });

                if (!string.IsNullOrEmpty(popupPosition.IconPositionName))
                {
                    model.IconPositions.Add(new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem()
                    {
                        Text = popupPosition.IconPositionName,
                        Value = popupPosition.Id.ToString()

                    });
                }
            }

            model.DeviceTypes.Add(new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem()
            {
                Text = "Desktop",
                Value = "1"

            });
            model.DeviceTypes.Add(new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem()
            {
                Text = "Desktop & Tablet",
                Value = "4"

            });
            model.DeviceTypes.Add(new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem()
            {
                Text = "Mobile",
                Value = "3"

            });
            model.DeviceTypes.Add(new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem()
            {
                Text = "Tablet",
                Value = "2"

            });
            model.DeviceTypes.Add(new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem()
            {
                Text = "All devices",
                Value = "5"

            });
            model.IconType = IconType.Image;
            return model;
        }
        public async Task<List<CampaignModel>> PrepareListOfCampaign(string sortDirection, string sortBy, string search, int status)
        {
            IList<CampaignOverView> campaigns = await _campaignManagementService.GetCampaign(sortDirection, sortBy);
            List<CampaignModel> campaignsModel = new List<CampaignModel>();
            foreach (var campaign in campaigns)
            {
                CampaignModel model = new CampaignModel();
                model.Conversions = campaign.Conversions;
                model.createdOn = campaign.createdOn;
                model.Title = campaign.Title;
                model.TemplateId = campaign.TemplateId;
                model.NoOfMonthCreated = campaign.NoOfMonthCreated;
                model.NoOfDayCreated = campaign.NoOfDayCreated;
                model.NoOfYearCreated = campaign.NoOfYearCreated;
                model.Html = campaign.Html;
                model.Impressions = campaign.Impressions;
                model.IsActive = campaign.IsActive;
                model.Id = campaign.Id;
                model.TimeSpan = datedifference(campaign.NoOfYearCreated, campaign.NoOfMonthCreated, campaign.NoOfDayCreated);
                campaignsModel.Add(model);
            }
            if (status != 0)
            {
                campaignsModel = campaignsModel.Where(c => c.IsActive == (status == 1 ? true : false)).ToList();
            }
            return campaignsModel;
        }

        public async Task<CampaignTargetModel> PrepareCampaignTargetModel(int campaignId)
        {
            CampaignTargetModel model = new CampaignTargetModel();
            var campaign = await _campaignManagementService.GetCampaignByid(campaignId);
            if (campaign != null)
            {
                var targetsOverViewModel = await _campaignManagementService.GetCampaignTargets(campaignId);
                model.IsExitPopup = campaign.IsExitPopUp == null ? false : Convert.ToBoolean(campaign.IsExitPopUp);
                model.IsGlobal = campaign.IsGlobal == null ? false : Convert.ToBoolean(campaign.IsGlobal);
                model.TriggerOn = campaign.TriggerOn;
                model.FrequencyId = campaign.FrequencyId;
                model.DeviceType = campaign.DeviceType;
                model.Position = campaign.PositionId;
                model.IconPosition = campaign.IconPositionId;
                model.IconType = IconType.Image;
                if (!string.IsNullOrEmpty(campaign.IconImage) || !string.IsNullOrEmpty(campaign.IconHtml))
                {
                    model.IconImageSrc = campaign.IconImage;
                    model.IconHtml = !string.IsNullOrEmpty(campaign.IconImage) ? "" : campaign.IconHtml;
                    model.IconType = !string.IsNullOrEmpty(campaign.IconImage) ? IconType.Image : IconType.Html;
                    model.HaveIconImage = true;
                }

                if (campaign.StartDate != null && campaign.EndDate != null)
                {
                    model.StartDate = campaign.StartDate;
                    model.EndDate = campaign.EndDate;
                    model.IsScheduled = true;
                }

                model.CampaignId = campaign.Id;

                foreach (var target in targetsOverViewModel)
                {
                    model.Targets.Add(new CampaignTargetOverViewModel()
                    {
                        ConditionId = target.ConditionId,
                        Name = target.Name,
                        Url = target.Url
                    });
                }
                var frequencies = await _campaignManagementService.CampaignFrequency();
                foreach (var frequency in frequencies)
                {
                    model.CampaignFreQuency.Add(new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem()
                    {
                        Text = frequency.Name,
                        Value = frequency.Id.ToString()

                    });
                }

                var urlConditions = await _campaignManagementService.UrlConditions();
                foreach (var urlCondition in urlConditions)
                {
                    model.UrlConditions.Add(new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem()
                    {
                        Text = urlCondition.Name,
                        Value = urlCondition.Id.ToString()

                    });
                }

                var popupPositions = await _campaignManagementService.PopupConditions();
                foreach (var popupPosition in popupPositions)
                {
                    model.PopupPositions.Add(new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem()
                    {
                        Text = popupPosition.PositionName,
                        Value = popupPosition.Id.ToString()

                    });

                    if (!string.IsNullOrEmpty(popupPosition.IconPositionName))
                    {
                        model.IconPositions.Add(new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem()
                        {
                            Text = popupPosition.IconPositionName,
                            Value = popupPosition.Id.ToString()

                        });
                    }
                }

                model.DeviceTypes.Add(new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem()
                {
                    Text = "Desktop",
                    Value = "1"

                });
                model.DeviceTypes.Add(new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem()
                {
                    Text = "Desktop & Tablet",
                    Value = "4"

                });
                model.DeviceTypes.Add(new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem()
                {
                    Text = "Mobile",
                    Value = "3"

                });
                model.DeviceTypes.Add(new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem()
                {
                    Text = "Tablet",
                    Value = "2"

                });
                model.DeviceTypes.Add(new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem()
                {
                    Text = "All devices",
                    Value = "5"

                });

            }

            return model;
        }

        public async Task<DesignSettingmodel> PrepareDesignSettingModel(int campaignId)
        {
            DesignSettingmodel model = new DesignSettingmodel();
            var campaign = await _campaignManagementService.GetCampaignByid(campaignId);
            if (campaign != null)
            {
                model.Id = campaign.Id;
                model.Name = campaign.Title;
                model.TemplateId = campaign.TemplateId;
                model.ResponseTemplateId = campaign.ResponseTemplateId;
                model.SubscribedUserResponseTemplateId = campaign.SubscribedUserResponseTemplateId;
                var templates = await _campaignManagementService.GetTemplates(null);
                model.Templates.Add(new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem()
                {
                    Text = "Please select template",
                    Value = ""
                });
                foreach (var template in templates)
                {
                    model.Templates.Add(new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem()
                    {
                        Text = template.Name,
                        Value = template.Id.ToString()
                    });
                }


            }
            return model;
        }

        #region Utilities

        protected string datedifference(int year_diff, int month_diff, int day_diff)
        {
            string heading;
            string subheading;
            if (year_diff > 0)
            {
                heading = year_diff.ToString();
                subheading = "YEARS";
            }
            else if (day_diff > 30)
            {
                heading = month_diff.ToString();
                subheading = "MONTHS";
            }
            else
            {
                heading = day_diff.ToString();
                subheading = "DAYS";
            }
            return "<label class='heading-center'>" + heading + "</label><label class='sub-heading-center'>" + subheading + "</label>";
        }


        #endregion
    }
}
