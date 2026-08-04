using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using Nop.Core;
using Nop.Core.Http;
using Nop.Services.Configuration;
using Nop.Services.Customizations.Mandrill;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace MWT.Nop.Core.Services.Mandrill
{
    public partial class MandrillService:IMandrillService
    {
        #region Fields

        private readonly ISettingService _settingService;
        private readonly IHttpClientFactory _httpClientFactory;

        #endregion

        #region Ctor
        public MandrillService(IHttpClientFactory httpClientFactory,
            ISettingService settingService)
        {
            this._settingService = settingService;
            this._httpClientFactory = httpClientFactory;
        }

        #endregion


        public async Task SendEmail(string html, string subject, DataTable dtTo, Dictionary<string, string> parameters, string httpmethod)
        {
            var emailContent = await GenerateMandrillMessageRequest(html, subject, dtTo, parameters);
            if (emailContent.message != null)
            {
                await ApiCall(await this._settingService.GetSettingByKeyAsync<string>("Mandril.Host") + "/messages/send.json", JsonConvert.SerializeObject(emailContent), httpmethod);
                // Api Code
            }
        }

        private async Task ApiCall(string endPoint, string json, string httpmethod)
        {
            try
            {
                if (httpmethod == System.Net.WebRequestMethods.Http.Post)
                {
                    var httpClient = _httpClientFactory.CreateClient(NopHttpDefaults.DefaultHttpClient);
                    var requestContent = new StringContent(json,
          Encoding.UTF8, MimeTypes.ApplicationJson);
                    var response = await httpClient.PostAsync(endPoint, requestContent);
                    response.EnsureSuccessStatusCode();
                    var content = await response.Content.ReadAsStringAsync();

                }
            }
            catch (Exception exp) { }
        }

        private async Task<Mandril_Email_Request_Format> GenerateMandrillMessageRequest(string html, 
            string subject, DataTable dtTo, Dictionary<string, string> parameters)
        {
            Mandril_Email_Request_Format emailContent = new Mandril_Email_Request_Format();
            emailContent.key = await _settingService.GetSettingByKeyAsync<string>("Mandril_Key");

            if (dtTo.Rows.Count > 0)
            {
                Mandrill_Message_Format message = new Mandrill_Message_Format();
                message.html = html;
                message.subject = subject;
          
                message.from_email = await _settingService.GetSettingByKeyAsync<string>("Mandril_From_Email");
                message.from_name = await _settingService.GetSettingByKeyAsync<string>("Mandril_From_Name");
                var toAddresses = new Mandrill_Message_To[dtTo.Rows.Count];
                for (var i = 0; i < dtTo.Rows.Count; i++)
                {
                    toAddresses[i] = new Mandrill_Message_To()
                    {
                        email = dtTo.Rows[i]["email"].ToString(),
                        name = dtTo.Rows[i]["name"].ToString()
                    };
                }
                message.to = toAddresses;

                message.headers = new Mandrill_Message_headers();
                message.headers.ReplyTo = await _settingService.GetSettingByKeyAsync<string>("Mandril_Reply_To");
                if (parameters != null && parameters.Count() > 0)
                {
                    var globalvars = new global_merge_vars[parameters.Count];
                    for (int i = 0; i < parameters.Count; i++)
                    {
                        globalvars[i] = new global_merge_vars() { name = parameters.ElementAt(i).Key, content = parameters.ElementAt(i).Value };
                    }
                    message.global_merge_vars = globalvars;
                }

                emailContent.message = message;
            }
            return emailContent;
        }
    }
}
