    using Nop.Plugin.Payments.AuthorizeNetHosted.Models;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    namespace Nop.Plugin.Payments.AuthorizeNetHosted.Services
    {
        public partial interface IAuthorizeNetWebHookService
        {
            Task<IList<AuthorizeNetWebHook>> GetAllWebhooksAsync();
            Task<AuthorizeNetWebHook> GetWebhookbyId(string webHookId);
            Task<string> CreateAsync(AuthorizeNetWebHook webhook);
            Task<AuthorizeNetWebHook> UpdateAsync(AuthorizeNetWebHook webhook);
            Task<bool> DeleteAsync(string webhookId);
        }
    }
