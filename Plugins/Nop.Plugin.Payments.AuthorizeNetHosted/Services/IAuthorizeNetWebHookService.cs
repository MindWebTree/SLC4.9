using Nop.Plugin.Payments.AuthorizeNetHosted.Models;

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
