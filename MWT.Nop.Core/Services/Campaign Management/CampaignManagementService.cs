using LinqToDB.Data;
using MWT.Nop.Core.Domain.Campaign_Management;
using MWT.Nop.Core.Domain.Custom.Campaign_Management;
using Nop.Data;

namespace MWT.Nop.Core.Service.Campaign_Management
{
    public partial class CampaignManagementService : ICampaignManagementService
    {
        private readonly IRepository<CampaignOverView> _campaignOverViewRepository;
        private readonly IRepository<MWT_Campaign> _campaignRepository;
        private readonly IRepository<MWT_CampaignTargets> _campaignTargetRepository;
        private readonly IRepository<MWT_CampaignTemplates> _campaignTemplateRepository;
        private readonly IRepository<MWT_CampaignFrequency> _campaignFrequencyRepository;
        private readonly IRepository<MWT_UrlCondition> _urlConditionRepository;
        private readonly IRepository<MWT_Popup_Position> _popupPositionRepository;
        private readonly IRepository<CampaignTargetOverView> _campaignTargetOverViewRepository;
        private readonly IRepository<CampaignTargetingSave> _campaignTargetingSaverepository;
        private readonly IRepository<CampaignPopupResult> _campaignPopupResultSaverepository;
        private readonly IRepository<MailchimpCampaignIntegration> _mailchimpCampaignIntegrationrepository;
        public CampaignManagementService(IRepository<MWT_Campaign> campaignRepository, IRepository<MWT_CampaignTargets> campaignTargetRepository,
            IRepository<MWT_CampaignTemplates> campaignTemplateRepository, IRepository<CampaignOverView> campaignOverViewRepository,
            IRepository<MWT_CampaignFrequency> campaignFrequencyRepository, IRepository<MWT_UrlCondition> urlConditionRepository,
            IRepository<MWT_Popup_Position> popupPositionRepository, IRepository<CampaignTargetOverView> campaignTargetOverViewRepository,
            IRepository<CampaignTargetingSave> campaignTargetingSaverepository,
            IRepository<CampaignPopupResult> campaignPopupResultSaverepository, IRepository<MailchimpCampaignIntegration> mailchimpCampaignIntegrationrepository)
        {
            this._campaignRepository = campaignRepository;
            this._campaignTargetRepository = campaignTargetRepository;
            this._campaignTemplateRepository = campaignTemplateRepository;
            this._campaignOverViewRepository = campaignOverViewRepository;
            this._campaignFrequencyRepository = campaignFrequencyRepository;
            this._urlConditionRepository = urlConditionRepository;
            this._popupPositionRepository = popupPositionRepository;
            this._campaignOverViewRepository = campaignOverViewRepository;
            this._campaignTargetingSaverepository = campaignTargetingSaverepository;
            this._campaignTargetOverViewRepository = campaignTargetOverViewRepository;
            this._campaignPopupResultSaverepository = campaignPopupResultSaverepository;
            _mailchimpCampaignIntegrationrepository = mailchimpCampaignIntegrationrepository;
        }
        #region Methods
        public async Task CloneCampaign(MWT_Campaign campaign)
        {
            var targets = await _campaignTargetRepository.Table.Where(t => t.CampaignId == campaign.Id).ToListAsync();
            await _campaignRepository.InsertAsync(campaign);


            foreach (var target in targets)
            {
                target.CampaignId = campaign.Id;
                await _campaignTargetRepository.InsertAsync(target);
            }

        }

        public async Task DeleteCampaign(MWT_Campaign campaign)
        {
            await _campaignRepository.DeleteAsync(campaign);
        }

        public async Task<IList<CampaignOverView>> GetCampaign(string sortDirection, string sortBy)
        {
            return await _campaignOverViewRepository.EntityFromSqlAsync("MWT_GetPopupCampaigns",
                       new DataParameter()
                       {
                           DataType = LinqToDB.DataType.VarChar,
                           Value = sortBy,
                           Direction = System.Data.ParameterDirection.Input,
                           Name = "@SortBy",

                       },
                      new DataParameter()
                      {
                          DataType = LinqToDB.DataType.VarChar,
                          Value = sortDirection,
                          Direction = System.Data.ParameterDirection.Input,
                          Name = "@SortDirection",

                      });
        }

        public async Task<MWT_Campaign> GetCampaignByid(int id)
        {
            return await _campaignRepository.GetByIdAsync(id);
        }


        public async Task InsertCampaign(MWT_Campaign campaign)
        {
            await _campaignRepository.InsertAsync(campaign);
        }

        public async Task UpdateCampaign(MWT_Campaign campaign)
        {
            await _campaignRepository.UpdateAsync(campaign);
        }

        public async Task SaveCampaign(CampaignTargetingSave campaignTargetingSave)
        {
            await _campaignTargetingSaverepository.EntityFromSqlAsync("MWTSaveUpdatePopupCampaign",
                  new DataParameter()
                  {
                      DataType = LinqToDB.DataType.VarChar,
                      Value = campaignTargetingSave.Name,
                      Direction = System.Data.ParameterDirection.Input,
                      Name = "@popupName",

                  },
                   new DataParameter()
                   {
                       DataType = LinqToDB.DataType.Boolean,
                       Value = campaignTargetingSave.IsActive,
                       Direction = System.Data.ParameterDirection.Input,
                       Name = "@isActive",

                   },
                    new DataParameter()
                    {
                        DataType = LinqToDB.DataType.DateTime,
                        Value = campaignTargetingSave.StartDate,
                        Direction = System.Data.ParameterDirection.Input,
                        Name = "@startdate",

                    },
                     new DataParameter()
                     {
                         DataType = LinqToDB.DataType.DateTime,
                         Value = campaignTargetingSave.EndDate,
                         Direction = System.Data.ParameterDirection.Input,
                         Name = "@endDate",

                     },
                      new DataParameter()
                      {
                          DataType = LinqToDB.DataType.Boolean,
                          Value = campaignTargetingSave.TriggerOnExit,
                          Direction = System.Data.ParameterDirection.Input,
                          Name = "@toggleexitoffers",

                      },
                       new DataParameter()
                       {
                           DataType = LinqToDB.DataType.Boolean,
                           Value = campaignTargetingSave.IsExitPopup,
                           Direction = System.Data.ParameterDirection.Input,
                           Name = "@isexitpopup",

                       },
                       new DataParameter()
                       {
                           DataType = LinqToDB.DataType.Int32,
                           Value = campaignTargetingSave.TemplateId,
                           Direction = System.Data.ParameterDirection.Input,
                           Name = "@templateid",

                       },
                       new DataParameter()
                       {
                           DataType = LinqToDB.DataType.Int32,
                           Value = campaignTargetingSave.ResponseTemplateId,
                           Direction = System.Data.ParameterDirection.Input,
                           Name = "@responsetemplateid",

                       },
                        new DataParameter()
                        {
                            DataType = LinqToDB.DataType.Int32,
                            Value = campaignTargetingSave.TriggerOn,
                            Direction = System.Data.ParameterDirection.Input,
                            Name = "@triggersec",

                        },
                         new DataParameter()
                         {
                             DataType = LinqToDB.DataType.Int32,
                             Value = campaignTargetingSave.FrequencyId,
                             Direction = System.Data.ParameterDirection.Input,
                             Name = "@triggerfreq",

                         },
                          new DataParameter()
                          {
                              DataType = LinqToDB.DataType.Int32,
                              Value = campaignTargetingSave.DeviceType,
                              Direction = System.Data.ParameterDirection.Input,
                              Name = "@triggerdevice",

                          }, new DataParameter()
                          {
                              DataType = LinqToDB.DataType.Boolean,
                              Value = campaignTargetingSave.IsGlobal,
                              Direction = System.Data.ParameterDirection.Input,
                              Name = "@toggledecision",

                          }
                          , new DataParameter()
                          {
                              DataType = LinqToDB.DataType.VarChar,
                              Value = campaignTargetingSave.Conditions,
                              Direction = System.Data.ParameterDirection.Input,
                              Name = "@toggledecisionval",

                          }, new DataParameter()
                          {
                              DataType = LinqToDB.DataType.Int32,
                              Value = campaignTargetingSave.Position,
                              Direction = System.Data.ParameterDirection.Input,
                              Name = "@PositionID",

                          }, new DataParameter()
                          {
                              DataType = LinqToDB.DataType.Int32,
                              Value = campaignTargetingSave.IconPosition,
                              Direction = System.Data.ParameterDirection.Input,
                              Name = "@IconPositionID",

                          }, new DataParameter()
                          {
                              DataType = LinqToDB.DataType.VarChar,
                              Value = campaignTargetingSave.IconImageSrc,
                              Direction = System.Data.ParameterDirection.Input,
                              Name = "@IconImage",
                          }
                          , new DataParameter()
                          {
                              DataType = LinqToDB.DataType.VarChar,
                              Value = campaignTargetingSave.IconHtml,
                              Direction = System.Data.ParameterDirection.Input,
                              Name = "@IconHtml",
                          }
                  );
        }
        public async Task<MWT_CampaignTemplates> GetCampaignResponseTemplate(int campaignId)
        {
            return await (from template in _campaignTemplateRepository.Table
                          join campaign in _campaignRepository.Table
                          on template.Id equals campaign.ResponseTemplateId
                          where campaign.Id == campaignId
                          select template).FirstOrDefaultAsync();
        }

        public async Task<IList<MailchimpCampaignIntegration>> GetCampaignIntegration(int campaignId)
        {
            return await _mailchimpCampaignIntegrationrepository.EntityFromSqlAsync("MWT_getMailchimpData",
                new DataParameter()
                {
                    DataType = LinqToDB.DataType.Int32,
                    Value = campaignId,
                    Direction = System.Data.ParameterDirection.Input,
                    Name = "@campaignid",

                });

        }
        public async Task<IList<CampaignPopupResult>> GetCampaignPopup(string url, int deviceType, string ip, string userAgent, bool isExitPopup, bool enabledEmailExclusive)
        {
            return await _campaignPopupResultSaverepository.EntityFromSqlAsync("MWT_showCampaignPopup",
                  new DataParameter()
                  {
                      DataType = LinqToDB.DataType.VarChar,
                      Value = url,
                      Direction = System.Data.ParameterDirection.Input,
                      Name = "@currenturl",

                  },
                   new DataParameter()
                   {
                       DataType = LinqToDB.DataType.Int32,
                       Value = deviceType,
                       Direction = System.Data.ParameterDirection.Input,
                       Name = "@deviceType",

                   },
                    new DataParameter()
                    {
                        DataType = LinqToDB.DataType.VarChar,
                        Value = ip,
                        Direction = System.Data.ParameterDirection.Input,
                        Name = "@ipaddress",

                    }, new DataParameter()
                    {
                        DataType = LinqToDB.DataType.VarChar,
                        Value = userAgent,
                        Direction = System.Data.ParameterDirection.Input,
                        Name = "@userAgent",

                    }
                    , new DataParameter()
                    {
                        DataType = LinqToDB.DataType.Boolean,
                        Value = isExitPopup,
                        Direction = System.Data.ParameterDirection.Input,
                        Name = "@isExitPopUp",

                    }, new DataParameter()
                    {
                        DataType = LinqToDB.DataType.Boolean,
                        Value = enabledEmailExclusive,
                        Direction = System.Data.ParameterDirection.Input,
                        Name = "@EnabledEmailExclusive ",

                    });
        }


        public async Task SavePopupImpression(int customerid, string ip, string country, string city, int eventtype, string url, int campaignId,
             int deviceType, string userAgent)
        {
            await _campaignPopupResultSaverepository.EntityFromSqlAsync("mwt_SavePopupImpression",
              new DataParameter()
              {
                  DataType = LinqToDB.DataType.Int32,
                  Value = customerid,
                  Direction = System.Data.ParameterDirection.Input,
                  Name = "@customerid",

              }, new DataParameter()
              {
                  DataType = LinqToDB.DataType.VarChar,
                  Value = ip,
                  Direction = System.Data.ParameterDirection.Input,
                  Name = "@ipaddress",

              }, new DataParameter()
              {
                  DataType = LinqToDB.DataType.VarChar,
                  Value = country,
                  Direction = System.Data.ParameterDirection.Input,
                  Name = "@country",

              }
              , new DataParameter()
              {
                  DataType = LinqToDB.DataType.VarChar,
                  Value = city,
                  Direction = System.Data.ParameterDirection.Input,
                  Name = "@city",

              }, new DataParameter()
              {
                  DataType = LinqToDB.DataType.Int32,
                  Value = eventtype,
                  Direction = System.Data.ParameterDirection.Input,
                  Name = "@eventype"
              }, new DataParameter()
              {
                  DataType = LinqToDB.DataType.VarChar,
                  Value = url,
                  Direction = System.Data.ParameterDirection.Input,
                  Name = "@url"
              }, new DataParameter()
              {
                  DataType = LinqToDB.DataType.Int32,
                  Value = campaignId,
                  Direction = System.Data.ParameterDirection.Input,
                  Name = "@campaignId"
              },
               new DataParameter()
               {
                   DataType = LinqToDB.DataType.Int32,
                   Value = deviceType,
                   Direction = System.Data.ParameterDirection.Input,
                   Name = "@deviceType",

               }, new DataParameter()
               {
                   DataType = LinqToDB.DataType.VarChar,
                   Value = userAgent,
                   Direction = System.Data.ParameterDirection.Input,
                   Name = "@userAgent",

               }
                );
        }

        public async Task SavePopupConversion(int customerid, string ip, string country, string city, int eventtype, string url, int campaignId, string email, string phone,
        int deviceType, string userAgent)
        {
            await _campaignPopupResultSaverepository.EntityFromSqlAsync("MWT_SavePopupConversion",
              new DataParameter()
              {
                  DataType = LinqToDB.DataType.Int32,
                  Value = customerid,
                  Direction = System.Data.ParameterDirection.Input,
                  Name = "@customerid",

              }, new DataParameter()
              {
                  DataType = LinqToDB.DataType.VarChar,
                  Value = ip,
                  Direction = System.Data.ParameterDirection.Input,
                  Name = "@ipaddress",

              }, new DataParameter()
              {
                  DataType = LinqToDB.DataType.VarChar,
                  Value = country,
                  Direction = System.Data.ParameterDirection.Input,
                  Name = "@country",

              }
              , new DataParameter()
              {
                  DataType = LinqToDB.DataType.VarChar,
                  Value = city,
                  Direction = System.Data.ParameterDirection.Input,
                  Name = "@city",

              }, new DataParameter()
              {
                  DataType = LinqToDB.DataType.Int32,
                  Value = eventtype,
                  Direction = System.Data.ParameterDirection.Input,
                  Name = "@eventype"
              }, new DataParameter()
              {
                  DataType = LinqToDB.DataType.VarChar,
                  Value = url,
                  Direction = System.Data.ParameterDirection.Input,
                  Name = "@url"
              }, new DataParameter()
              {
                  DataType = LinqToDB.DataType.Int32,
                  Value = campaignId,
                  Direction = System.Data.ParameterDirection.Input,
                  Name = "@campaignId"
              }, new DataParameter()
              {
                  DataType = LinqToDB.DataType.VarChar,
                  Value = email,
                  Direction = System.Data.ParameterDirection.Input,
                  Name = "@email",

              }, new DataParameter()
              {
                  DataType = LinqToDB.DataType.VarChar,
                  Value = phone,
                  Direction = System.Data.ParameterDirection.Input,
                  Name = "@phone",

              },
               new DataParameter()
               {
                   DataType = LinqToDB.DataType.Int32,
                   Value = deviceType,
                   Direction = System.Data.ParameterDirection.Input,
                   Name = "@deviceType",

               }, new DataParameter()
               {
                   DataType = LinqToDB.DataType.VarChar,
                   Value = userAgent,
                   Direction = System.Data.ParameterDirection.Input,
                   Name = "@userAgent",

               }
                );
        }

        #endregion

        #region Frequency

        public async Task<List<MWT_CampaignFrequency>> CampaignFrequency()
        {
            return await this._campaignFrequencyRepository.Table.ToListAsync();
        }
        public async Task<List<MWT_UrlCondition>> UrlConditions()
        {
            return await this._urlConditionRepository.Table.ToListAsync();
        }
        public async Task<List<MWT_Popup_Position>> PopupConditions()
        {
            return await this._popupPositionRepository.Table.ToListAsync();
        }

        #endregion

        #region Campaign Target

        public async Task<IList<CampaignTargetOverView>> GetCampaignTargets(int campaignId)
        {
            return await _campaignTargetOverViewRepository.EntityFromSqlAsync("MWT_getTargetingPopupCampaigns",
                new DataParameter()
                {
                    DataType = LinqToDB.DataType.Int32,
                    Value = campaignId,
                    Direction = System.Data.ParameterDirection.Input,
                    Name = "@CampaignId",

                });
        }

        public async Task SaveCampaignTarget(CampaignTargetingSave campaignTargetingSave)
        {
            await _campaignTargetingSaverepository.EntityFromSqlAsync("MWT_updateTargetingPopupCampaigns",
                  new DataParameter()
                  {
                      DataType = LinqToDB.DataType.Int32,
                      Value = campaignTargetingSave.CampaignId,
                      Direction = System.Data.ParameterDirection.Input,
                      Name = "@campaignId",

                  },
                   new DataParameter()
                   {
                       DataType = LinqToDB.DataType.Boolean,
                       Value = campaignTargetingSave.IsActive,
                       Direction = System.Data.ParameterDirection.Input,
                       Name = "@isActive",

                   },
                    new DataParameter()
                    {
                        DataType = LinqToDB.DataType.DateTime,
                        Value = campaignTargetingSave.StartDate,
                        Direction = System.Data.ParameterDirection.Input,
                        Name = "@startdate",

                    },
                     new DataParameter()
                     {
                         DataType = LinqToDB.DataType.DateTime,
                         Value = campaignTargetingSave.EndDate,
                         Direction = System.Data.ParameterDirection.Input,
                         Name = "@endDate",

                     },
                      new DataParameter()
                      {
                          DataType = LinqToDB.DataType.Boolean,
                          Value = false,
                          Direction = System.Data.ParameterDirection.Input,
                          Name = "@toggleexitoffers",

                      },
                       new DataParameter()
                       {
                           DataType = LinqToDB.DataType.Boolean,
                           Value = campaignTargetingSave.IsExitPopup,
                           Direction = System.Data.ParameterDirection.Input,
                           Name = "@isexitpopup",

                       },
                        new DataParameter()
                        {
                            DataType = LinqToDB.DataType.Int32,
                            Value = campaignTargetingSave.TriggerOn,
                            Direction = System.Data.ParameterDirection.Input,
                            Name = "@triggersec",

                        },
                         new DataParameter()
                         {
                             DataType = LinqToDB.DataType.Int32,
                             Value = campaignTargetingSave.FrequencyId,
                             Direction = System.Data.ParameterDirection.Input,
                             Name = "@triggerfreq",

                         },
                          new DataParameter()
                          {
                              DataType = LinqToDB.DataType.Int32,
                              Value = campaignTargetingSave.DeviceType,
                              Direction = System.Data.ParameterDirection.Input,
                              Name = "@triggerdevice",

                          }, new DataParameter()
                          {
                              DataType = LinqToDB.DataType.Boolean,
                              Value = campaignTargetingSave.IsGlobal,
                              Direction = System.Data.ParameterDirection.Input,
                              Name = "@toggledecision",

                          }
                          , new DataParameter()
                          {
                              DataType = LinqToDB.DataType.VarChar,
                              Value = campaignTargetingSave.Conditions,
                              Direction = System.Data.ParameterDirection.Input,
                              Name = "@toggledecisionval",

                          }, new DataParameter()
                          {
                              DataType = LinqToDB.DataType.Int32,
                              Value = campaignTargetingSave.Position,
                              Direction = System.Data.ParameterDirection.Input,
                              Name = "@PositionID",

                          }, new DataParameter()
                          {
                              DataType = LinqToDB.DataType.Int32,
                              Value = campaignTargetingSave.IconPosition,
                              Direction = System.Data.ParameterDirection.Input,
                              Name = "@IconPositionID",

                          }, new DataParameter()
                          {
                              DataType = LinqToDB.DataType.VarChar,
                              Value = campaignTargetingSave.IconImageSrc,
                              Direction = System.Data.ParameterDirection.Input,
                              Name = "@IconImage",
                          }, new DataParameter()
                          {
                              DataType = LinqToDB.DataType.VarChar,
                              Value = campaignTargetingSave.IconHtml,
                              Direction = System.Data.ParameterDirection.Input,
                              Name = "@IconHtml",
                          }
                  );
        }

        #endregion

        #region Design templates

        public async Task<List<MWT_CampaignTemplates>> GetTemplates(bool? responseTemplate = null)
        {
            if (responseTemplate == null)
                return await _campaignTemplateRepository.Table.Where(t => (t.IsDeleted ?? false) == false).OrderByDescending(t => t.Id).ToListAsync();
            else
            {
                if (responseTemplate == true)
                    return await (from template in _campaignTemplateRepository.Table
                                  join campaign in _campaignRepository.Table
                                  on template.Id equals campaign.ResponseTemplateId
                                  where (template.IsDeleted ?? false) == false
                                  orderby template.Id descending
                                  select template).ToListAsync();
                else
                    return await (from template in _campaignTemplateRepository.Table
                                  join campaign in _campaignRepository.Table
                                  on template.Id equals campaign.TemplateId
                                  where (template.IsDeleted ?? false) == false
                                  orderby template.Id descending
                                  select template).ToListAsync();
            }
        }
        public async Task<MWT_CampaignTemplates> GetCampaignTemplateByid(int id)
        {
            return await _campaignTemplateRepository.GetByIdAsync(id);
        }
        public async Task InsertTemplate(MWT_CampaignTemplates template)
        {
            await _campaignTemplateRepository.InsertAsync(template);
        }
        public async Task UpdateTemplate(MWT_CampaignTemplates template)
        {
            await _campaignTemplateRepository.UpdateAsync(template);
        }


        #endregion
    }
}
