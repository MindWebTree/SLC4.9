
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Core;
using Nop.Plugin.Misc.Redirect.Domain;
using Nop.Plugin.Misc.Redirect.Enums;
using Nop.Plugin.Misc.Redirect.Models;
using Nop.Plugin.Misc.Redirect.Models.Redirections;
using Nop.Plugin.Misc.Redirect.Services;
using Nop.Services.Messages;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Models.Extensions;
using Nop.Web.Framework.Mvc.Filters;
using Nop.Web.Framework.Mvc.ModelBinding;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nop.Plugin.Misc.Redirect.Controllers
{
    [AutoValidateAntiforgeryToken]
    public class RedirectController : BasePluginController
    {

        #region Fields
        private readonly IRedirectionsService _redirectionsService;
		private readonly IDataIOService _dataIOService;
		private readonly INotificationService _notificationService; 
        #endregion

        #region Ctor

        public RedirectController(IRedirectionsService redirectionsService, IDataIOService dataIOService, INotificationService notificationService)
        {
            _redirectionsService = redirectionsService;
            _dataIOService = dataIOService;
            _notificationService = notificationService; 
        }

        #endregion


        #region Methods

        [AuthorizeAdmin]
        [Area(AreaNames.ADMIN)]
        [HttpPost, ActionName("GetRedirections")]
        [CheckPermission(Nop.Services.Security.StandardPermission.Configuration.MANAGE_SETTINGS)]
        public async Task<IActionResult> GetRedirections(RedirectionSearchModel searchModel)
        {
            var data =  (await _redirectionsService.GetAllRedirectionsAsync(searchModel));

            var model = new RedirectionsListModel().PrepareToGrid(searchModel, data,  () =>
            {
                return data.Select(l => l.ToModel<RedirectionModel>());
            });

            return Json(model);
        }

      

        [AuthorizeAdmin]
        [Area(AreaNames.ADMIN)]
        [HttpPost, ActionName("RedirectAdd")]
        [CheckPermission(Nop.Services.Security.StandardPermission.Configuration.MANAGE_SETTINGS)]
        public async Task<IActionResult> RedirectAdd(RedirectionModel model)
        {
            if (!ModelState.IsValid)
                return ErrorJson(ModelState.SerializeErrors());
            
            InsertRedirectionResult result = await _redirectionsService.InsertRedirectionsAsync(model.ToEntity<RedirectionRule>());
            if (result != InsertRedirectionResult.OK)
                return ErrorJson($"{result}");

            return Json(new { Result = true });
        }

        [AuthorizeAdmin]
        [Area(AreaNames.ADMIN)]
        [CheckPermission(Nop.Services.Security.StandardPermission.Configuration.MANAGE_SETTINGS)]
        public IActionResult Configure()
        {
            ConfigurationModel model = new ConfigurationModel()
            {
                SearchModel = new RedirectionSearchModel() { AvailablePageSizes = "10,20,30" }
            };

            foreach (RedirectionTypeEnum r in (RedirectionTypeEnum[])Enum.GetValues(typeof(RedirectionTypeEnum)))
            {
                model.AvailableTypes.Add(new SelectListItem() { Text = r.ToString(), Selected = !model.AvailableTypes.Any(), Value = r.ToString() });
            }
            
            return View("~/Plugins/MWT.Nop.Redirect/Views/Configure.cshtml", model); 
        }

        [AuthorizeAdmin]
        [Area(AreaNames.ADMIN)]
        [HttpPost, ActionName("Import")]
        [CheckPermission(Nop.Services.Security.StandardPermission.Configuration.MANAGE_SETTINGS)]
        public async Task<IActionResult> ImportAsync(IFormFile importexcelfile)
        {
			var csvText = new StringBuilder();
			using (var reader = new StreamReader(importexcelfile.OpenReadStream()))
			{
				while (reader.Peek() >= 0)
					csvText.AppendLine(await reader.ReadLineAsync());
			}

            var erros = await _dataIOService.Import(csvText.ToString());

            foreach(var er in erros)
            {
                switch(er.result)
                {
                    case InsertRedirectionResult.CSVNotValid:
                        _notificationService.ErrorNotification("CSV not valid");
                        break;
					case InsertRedirectionResult.Exist:
						_notificationService.ErrorNotification($"Error in line {er.line}: Match exist");
                        break;
					case InsertRedirectionResult.RegularExpressionNotValid:
						_notificationService.ErrorNotification($"Error in line {er.line}: Invalid regular expresion");
						break;
					case InsertRedirectionResult.Error:
						_notificationService.ErrorNotification($"Error");
						break;
				}
            }

            if (!erros.Any())
                _notificationService.Notification(NotifyType.Success, "OK");
				

			return Configure();
		}

		[AuthorizeAdmin]
		[Area(AreaNames.ADMIN)]
		[HttpGet, ActionName("Export")]
        [CheckPermission(Nop.Services.Security.StandardPermission.Configuration.MANAGE_SETTINGS)]
        public async Task<IActionResult> ExportAsync()
		{
            string csv = await _dataIOService.Export();
			byte[] bytes = Encoding.UTF8.GetBytes(csv,0, csv.Length);
			return File(bytes, MimeTypes.TextCsv, "export.csv");
		}


		[AuthorizeAdmin]
        [Area(AreaNames.ADMIN)]
        [HttpPost, ActionName("RedirectRemove")]
        [CheckPermission(Nop.Services.Security.StandardPermission.Configuration.MANAGE_SETTINGS)]
        public async Task<IActionResult> RedirectRemove(RedirectionModel model)
        {
            var entity = model.ToEntity<RedirectionRule>();
            await _redirectionsService.DeleteRedirectionAsync(entity);
            return Json(new { Result = true });
        }


        [AuthorizeAdmin]
        [Area(AreaNames.ADMIN)]
        [HttpPost, ActionName("Configure")]
        [FormValueRequired("save")]
        [CheckPermission(Nop.Services.Security.StandardPermission.Configuration.MANAGE_SETTINGS)]
        public IActionResult Configure(ConfigurationModel model)
        {
            if (!ModelState.IsValid)
                return  Configure();

            return Configure();
        }
        [AuthorizeAdmin]
        [Area(AreaNames.ADMIN)]
        [ActionName("RedirectUpdate")]
        [CheckPermission(Nop.Services.Security.StandardPermission.Configuration.MANAGE_SETTINGS)]
        public async Task<IActionResult> RedirectUpdate(RedirectionModel model)
        {
            if (!ModelState.IsValid)
                return ErrorJson(ModelState.SerializeErrors());

            InsertRedirectionResult result = await _redirectionsService.UpdateRedirectionsAsync(model.ToEntity<RedirectionRule>());
            if (result != InsertRedirectionResult.OK)
                return ErrorJson($"{result}");

            return Json(new { Result = true });
        }
        #endregion
    }
}