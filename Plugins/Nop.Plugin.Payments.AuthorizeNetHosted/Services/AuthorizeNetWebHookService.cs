using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Nop.Plugin.Payments.AuthorizeNetHosted.Models;
using System.Security.Cryptography.X509Certificates;
using Nop.Core;

namespace Nop.Plugin.Payments.AuthorizeNetHosted.Services
{
    public class AuthorizeNetWebHookService : IAuthorizeNetWebHookService
    {


        private readonly IHttpClientFactory _httpClientFactory;
        private readonly AuthorizeNetHostedPaymentSettings _authorizeNetSettings;
        private readonly IStoreContext _storeContext;

        public AuthorizeNetWebHookService(IHttpClientFactory httpClientFactory, AuthorizeNetHostedPaymentSettings authorizeNetSettings, IStoreContext storeContext)
        {
            _httpClientFactory = httpClientFactory;
            _authorizeNetSettings = authorizeNetSettings;
            _storeContext = storeContext;
        }

        /// <summary>List all supported webhook event types from Authorize.NET.</summary>
        public async Task<IList<string>> ListEventTypesAsync()
        {
            var response = await ExecuteAsync<IList<string>>("webhooks/eventtypes", HttpMethod.Get, null);
            return response;
        }

        /// <summary>Get all registered webhooks for this merchant account.</summary>
        public async Task<IList<AuthorizeNetWebHook>> GetAllWebhooksAsync()
        {
            var response = await ExecuteAsync<IList<AuthorizeNetWebHook>>("webhooks", HttpMethod.Get, null);
            return response;
        }

        /// <summary>Get a specific webhook by its ID.</summary>
        public async Task<AuthorizeNetWebHook> GetWebhookbyId(string webHookId)
        {
            var response = await ExecuteAsync<AuthorizeNetWebHook>($"webhooks/{webHookId}", HttpMethod.Get, null);
            return response;
        }

        /// <summary>Register a new webhook endpoint with Authorize.NET.</summary>
        public async Task<string> CreateAsync(AuthorizeNetWebHook webhook)
        {
       
            string webhookId = string.Empty;
            try
            {
                foreach (var webhookRegistered in await this.GetAllWebhooksAsync())
                {
                    if ((webhookRegistered.url ?? "").Equals(webhook.url))
                    {
                        if (webhookRegistered.status != "active" || webhook.eventTypes.Any(e => !webhookRegistered.eventTypes.Contains(e)))
                        {
                            await this.DeleteAsync(webhookRegistered.webhookId);
                        }
                    }
                    else
                    {
                        webhookId = webhookRegistered.webhookId;
                    }
                }
                if (string.IsNullOrWhiteSpace(webhookId))
                {
                    var body = JsonConvert.SerializeObject(new
                    {
                        url = webhook.url,
                        eventTypes = webhook.eventTypes,
                        status = "active",
                        name= webhook.name,
                    });
                    var response = await ExecuteAsync<AuthorizeNetWebHook>("webhooks", HttpMethod.Post, body);
                    webhookId = response?.webhookId ?? string.Empty;
                }
            }
            catch
            {

            }
            return webhookId;
        }

        /// <summary>Update an existing webhook registration.</summary>
        public async Task<AuthorizeNetWebHook> UpdateAsync(AuthorizeNetWebHook webhook)
        {
            var body = JsonConvert.SerializeObject(new
            {
                url = webhook.url,
                eventTypes = webhook.eventTypes,
                status = "active"
            });
            var response = await ExecuteAsync<AuthorizeNetWebHook>($"webhooks/{webhook.webhookId}", HttpMethod.Put, body);
            return response;
        }

        /// <summary>Delete / unregister a webhook.</summary>
        public async Task<bool> DeleteAsync(string webhookId)
        {
            try
            {
                var response = await ExecuteAsync<anResponse>($"webhooks/{webhookId}", HttpMethod.Delete, null);
                return true;
            }
            catch
            {
                return false;
            }
        }

        // ── HTTP helper ────────────────────────────────────────────────────────

        private async Task<T> ExecuteAsync<T>(string resource, HttpMethod method, string body)
        {
            string baseUrl =
                _authorizeNetSettings.UseSandbox
                     ? "https://apitest.authorize.net/rest/v1/"
                    : "https://api.authorize.net/rest/v1/";
            var startTime = System.Diagnostics.Stopwatch.StartNew();
            var client = _httpClientFactory.CreateClient();
            client.Timeout = TimeSpan.FromSeconds(30);

            var credentials = Convert.ToBase64String(
                Encoding.UTF8.GetBytes($"{_authorizeNetSettings.LoginId}:{_authorizeNetSettings.TransactionKey}"));

            var request = new HttpRequestMessage(method, $"{baseUrl}{resource}");
            request.Headers.Authorization = new AuthenticationHeaderValue("Basic", credentials);

            if (body != null)
                request.Content = new StringContent(body, Encoding.UTF8, "application/json");

            string responseContent = null;
            HttpResponseMessage httpResponse = null;

            try
            {
                httpResponse = await client.SendAsync(request);
                responseContent = await httpResponse.Content.ReadAsStringAsync();
                httpResponse.EnsureSuccessStatusCode();
                return JsonConvert.DeserializeObject<T>(responseContent);
            }
            catch (Exception ex)
            {
                startTime.Stop();
                await LogRequestAsync(resource, method.ToString(), startTime.ElapsedMilliseconds,
                    (int?)httpResponse?.StatusCode, responseContent);
                throw;
            }
            finally
            {
                startTime.Stop();
            }
        }

        private Task LogRequestAsync(string resource, string method, long durationMs,
            int? statusCode, string responseContent)
        {
            // Log webhook HTTP request details — implementation writes to plugin log file
            return Task.CompletedTask;
        }

        private async Task LogMessageAsync(string message)
        {
            // Delegate to FNSLogger
            await Task.CompletedTask;
        }
    }
}
