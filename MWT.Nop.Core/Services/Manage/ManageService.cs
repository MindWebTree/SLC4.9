using FluentMigrator.Infrastructure;
using Newtonsoft.Json;
using Nop.Core.Domain.Logging;
using Nop.Core.Http;
using Nop.Services.Configuration;
using Nop.Services.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace MWT.Nop.Core.Services.Manage
{
    public partial class ManageService : IManageService
    {
        #region Fields

        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger _logger;
        private readonly ISettingService _settingService;

        #endregion

        #region Ctor

        public ManageService(IHttpClientFactory httpClientFactory, ILogger logger, ISettingService settingService)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
            _settingService = settingService;
        }


        #endregion

        #region Methods
        public async Task MarkWgsAsPaid(int orderNo, string email, decimal orderTotal, string pairedOrderIds)
        {
            try
            {
                string manageApiLink = await _settingService.GetSettingByKeyAsync<string>("Manage.Api.EndPoint");
                string manageApiKey = await _settingService.GetSettingByKeyAsync<string>("Manage.Api.ApiKey");
                string manageToken = await _settingService.GetSettingByKeyAsync<string>("Manage.Api.Token");
                var _httpClient = _httpClientFactory.CreateClient(NopHttpDefaults.DefaultHttpClient);
                _httpClient.DefaultRequestHeaders.Add("apikey", manageApiKey);
                if (!_httpClient.DefaultRequestHeaders.Contains("Authorization"))
                    _httpClient.DefaultRequestHeaders.Add("Authorization", $"Basic {manageToken}");


                // Create the JSON payload
                var payload = new
                {
                    OrderID = orderNo,
                    EmailID = email,
                    Service = "WGS",
                    Amount = orderTotal,
                    PairedOrderIds = pairedOrderIds
                };

                string jsonPayload = System.Text.Json.JsonSerializer.Serialize(payload);

                // Create the request content
                HttpContent content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

                // Send the POST request
                HttpResponseMessage response = await _httpClient.PostAsync($"{manageApiLink}order-service/sync-service", content);

                if (response.IsSuccessStatusCode)
                {
                    string responseBody = await response.Content.ReadAsStringAsync();
                    await _logger.InsertLogAsync(LogLevel.Information, $"Manage Wgs Order Synced {responseBody}", await response.Content.ReadAsStringAsync());
                }
                else
                {
                    await _logger.InsertLogAsync(LogLevel.Error, "Failed to update WGS Order status on Manage", await response.Content.ReadAsStringAsync());
                }
            }
            catch (Exception exp)
            {
                await _logger.InsertLogAsync(LogLevel.Error, "Failed to update WGS Order status on Manage", exp.Message);

            }
        }

        public async Task<(bool isValid, string message)> ValidateWgsOrder(int orderNo)
        {
            bool isValid = true;
            string message = string.Empty;
            try
            {
                string manageApiLink = await _settingService.GetSettingByKeyAsync<string>("Manage.Api.EndPoint");
                string manageApiKey = await _settingService.GetSettingByKeyAsync<string>("Manage.Api.ApiKey");
                string manageToken = await _settingService.GetSettingByKeyAsync<string>("Manage.Api.Token");

                var _httpClient = _httpClientFactory.CreateClient(NopHttpDefaults.DefaultHttpClient);
                _httpClient.DefaultRequestHeaders.Add("apikey", manageApiKey);
                if (!_httpClient.DefaultRequestHeaders.Contains("Authorization"))
                    _httpClient.DefaultRequestHeaders.Add("Authorization", $"Basic {manageToken}");

                HttpResponseMessage response = await _httpClient.GetAsync($"{manageApiLink}order-service/validate-service-purchase/{orderNo}");
                if (response.IsSuccessStatusCode)
                {

                    dynamic responseObject = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());
                    if ((string)responseObject.Status != "Valid")
                    {
                        isValid = false;
                        message = (string)responseObject.Message;
                    }


                }
            }
            catch (Exception exp)
            {
                await _logger.InsertLogAsync(LogLevel.Error, "Failed to Validate WGS Order" +
                    "", exp.Message);

            }
            return (isValid, message);
        }

        public async Task<List<(int orderID, string serviceName)>> GetPairedOrders(int orderNo)
        {
            List<(int orderID, string serviceName)> pairOrders = new List<(int orderID, string serviceName)>();
            try
            {
                string manageApiLink = await _settingService.GetSettingByKeyAsync<string>("Manage.Api.EndPoint");
                string manageApiKey = await _settingService.GetSettingByKeyAsync<string>("Manage.Api.ApiKey");
                string manageToken = await _settingService.GetSettingByKeyAsync<string>("Manage.Api.Token");
                var _httpClient = _httpClientFactory.CreateClient(NopHttpDefaults.DefaultHttpClient);
                _httpClient.DefaultRequestHeaders.Add("apikey", manageApiKey);
                if (!_httpClient.DefaultRequestHeaders.Contains("Authorization"))
                    _httpClient.DefaultRequestHeaders.Add("Authorization", $"Basic {manageToken}");
                var payload = new
                {
                    OrderID = orderNo,
                    Service = "WGS Service",
                };

                string jsonPayload = System.Text.Json.JsonSerializer.Serialize(payload);

                // Create the request content
                HttpContent content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

                // Send the POST request
                HttpResponseMessage response = await _httpClient.PostAsync($"{manageApiLink}order-service/get-pairorder", content);

                if (response.IsSuccessStatusCode)
                {

                    dynamic responseObject = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());
                    foreach (var item in responseObject)
                    {
                        pairOrders.Add(((int)item.OrderID, (string)item.ServiceName));
                    }


                }

            }
            catch (Exception exp)
            {
                await _logger.InsertLogAsync(LogLevel.Error, "Failed to Pair Orders" +
                    "", exp.Message);

            }
            return pairOrders;
        }

        #region RewardClaim

        public async Task<(string Comment, string ErrorMessage)> GetOrderFeedback(int orderId, string email)
        {
            string manageApiLink = await _settingService.GetSettingByKeyAsync<string>("Manage.Api.EndPoint");
            string manageApiKey = await _settingService.GetSettingByKeyAsync<string>("Manage.Api.ApiKey");
            string manageToken = await _settingService.GetSettingByKeyAsync<string>("Manage.Api.Token");
            string comment = string.Empty;
            string errorMessage = string.Empty;
            try
            {
                var _httpClient = _httpClientFactory.CreateClient(NopHttpDefaults.DefaultHttpClient);
                string apiUrl = $"{manageApiLink}sierra/feedback/get_order_feedback/{orderId}/{email}";
                _httpClient.DefaultRequestHeaders.Add("apikey", manageApiKey);
                if (!_httpClient.DefaultRequestHeaders.Contains("Authorization"))
                    _httpClient.DefaultRequestHeaders.Add("Authorization", $"Basic {manageToken}");
                HttpResponseMessage httpResponse = await _httpClient.GetAsync(apiUrl);
                string response = await httpResponse.Content.ReadAsStringAsync();
                if (!httpResponse.IsSuccessStatusCode)
                {
                    await _logger.InsertLogAsync(LogLevel.Error,
                        $"Failed to get feedback for order {orderId}-{email}", response);
                    return (string.Empty, $"API call failed with status code {httpResponse.StatusCode}");
                }


                var feedback = JsonConvert.DeserializeObject<dynamic>(response);
                if (feedback != null)
                {
                    comment = feedback.Comment;
                    errorMessage = feedback.ErrorMessage;
                }

                return (comment, errorMessage);
            }
            catch (Exception ex)
            {
                await _logger.InsertLogAsync(LogLevel.Error, $"Failed to get feedback for order {orderId}-{email}", ex.Message);
                return (comment, ex.Message);
            }
        }

        public async Task<(bool IsSaved, string ErrorMessage)> SaveOrderFeedbackRewardClaim(int orderId, string emailID, string comment, string giftProductName, bool isGoogleReviewed, bool isInstagramPost, bool isSharedVideo)
        {
            bool isSaved = false;
            string errorMessage = string.Empty;
            string manageApiLink = await _settingService.GetSettingByKeyAsync<string>("Manage.Api.EndPoint");
            string manageApiKey = await _settingService.GetSettingByKeyAsync<string>("Manage.Api.ApiKey");
            string manageToken = await _settingService.GetSettingByKeyAsync<string>("Manage.Api.Token");

            // Prepare payload
            var payload = new
            {
                OrderID = orderId,
                EmailID = emailID,
                GiftProductName = giftProductName,
                IsGoogleReviewed = isGoogleReviewed,
                IsInstagramPost = isInstagramPost,
                IsSharedVideo = isSharedVideo,
                Comment = comment
            };
            try
            {
                var _httpClient = _httpClientFactory.CreateClient(NopHttpDefaults.DefaultHttpClient);
                _httpClient.DefaultRequestHeaders.Add("apikey", manageApiKey);
                if (!_httpClient.DefaultRequestHeaders.Contains("Authorization"))
                    _httpClient.DefaultRequestHeaders.Add("Authorization", $"Basic {manageToken}");

                HttpContent content = new StringContent(System.Text.Json.JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync($"{manageApiLink}sierra/feedback/save_feedback_gift", content);
                string apiResultstring = await response.Content.ReadAsStringAsync();

                var deserializedResponse = System.Text.Json.JsonSerializer.Deserialize<dynamic>(apiResultstring);
                if (deserializedResponse.TryGetProperty("IsSaved", out JsonElement isSavedElement))
                    isSaved = deserializedResponse.GetProperty("IsSaved").GetBoolean();
                if (deserializedResponse.TryGetProperty("ErrorMessage", out JsonElement ErrorMessage))
                    errorMessage = deserializedResponse.GetProperty("ErrorMessage").GetString();
                return (isSaved, errorMessage);
            }
            catch (Exception ex)
            {
                await _logger.InsertLogAsync(LogLevel.Error, "Exception in Save Order Feedback Reward Clain", ex.Message);
                return (isSaved, ex.Message);
            }
        }

        public async Task SyncPendingOrderPartialPayment(int orderId, string transactionid, decimal total, DateTime paidOn, string paymentgateway)
        {
            string manageApiLink = await _settingService.GetSettingByKeyAsync<string>("Manage.Api.EndPoint");
            string manageApiKey = await _settingService.GetSettingByKeyAsync<string>("Manage.Api.ApiKey");
            string manageToken = await _settingService.GetSettingByKeyAsync<string>("Manage.Api.Token");
            var payload = new
            {
                OrderID = orderId,
                Transactionid = transactionid,
                Payment = total,
                Paidon = paidOn,
                paymentgateway = paymentgateway
            };
            string request = System.Text.Json.JsonSerializer.Serialize(payload);
            try
            {
                var _httpClient = _httpClientFactory.CreateClient(NopHttpDefaults.DefaultHttpClient);
                _httpClient.DefaultRequestHeaders.Add("apikey", manageApiKey);
                if (!_httpClient.DefaultRequestHeaders.Contains("Authorization"))
                    _httpClient.DefaultRequestHeaders.Add("Authorization", $"Basic {manageToken}");
                HttpContent content = new StringContent(request, Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync($"{manageApiLink}order-service/customer-payment", content);
                string apiResultstring = await response.Content.ReadAsStringAsync();
                await _logger.InsertLogAsync(LogLevel.Information, $"Partial Order {request}", System.Text.Json.JsonSerializer.Deserialize<dynamic>(apiResultstring));

            }
            catch (Exception ex)
            {
                await _logger.InsertLogAsync(LogLevel.Error, $"Partial Order {request}", ex.Message);
            }
        }
        #endregion

        #endregion
    }
}