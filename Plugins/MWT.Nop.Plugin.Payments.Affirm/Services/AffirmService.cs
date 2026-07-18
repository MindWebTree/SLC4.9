using Microsoft.AspNetCore.Http;
using MWT.Nop.Plugin.Payments.Affirm.Domain;
using MWT.Nop.Plugin.Payments.Affirm.Models;
using Newtonsoft.Json;
using Nop.Core;
using Nop.Core.Domain.Logging;
using Nop.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace MWT.Nop.Plugin.Payments.Affirm.Services
{
    public partial class AffirmService : IAffirmService
    {

        #region Fields

        AffirmCheckoutSettings _affirmSettings;
        private readonly HttpClient _httpClient;
        private readonly IRepository<AffirmLog> _affirmLogRepository;
        private readonly IWebHelper _webHelper;
        private readonly IWorkContext _workContext;
        private readonly IHttpContextAccessor _httpContextAccessor;

        #endregion

        #region Ctor

        public AffirmService(AffirmCheckoutSettings affirmSettings, HttpClient httpClient, IRepository<AffirmLog> affirmLogRepository, IWebHelper webHelper, IWorkContext workContext,
                               IHttpContextAccessor httpContextAccessor)
        {
            _affirmSettings = affirmSettings;
            _httpClient = httpClient;
            _affirmLogRepository = affirmLogRepository;
            _webHelper = webHelper;
            _workContext = workContext;
            _httpContextAccessor = httpContextAccessor;
        }

        #endregion


        #region Methods
        public async Task<AffirmResponseModel> CheckoutDetails(string token)
        {
            AffirmResponseModel model = new AffirmResponseModel();
            await _affirmLogRepository.InsertAsync(new AffirmLog()
            {
                CreatedOnUtc = DateTime.UtcNow,
                CustomerId = (await _workContext.GetCurrentCustomerAsync())?.Id ?? 0,
                FullMessage = $"CheckoutDetails Init {token}",
                IpAddress = _webHelper.GetCurrentIpAddress(),
                LogLevel = LogLevel.Information,
                ReferrerUrl = _webHelper.GetUrlReferrer(),
                PageUrl = _webHelper.GetRawUrl(_httpContextAccessor.HttpContext.Request),
                ShortMessage = $"CheckoutDetails Init {token}"

            });
            try
            {
                var affirmApiUrl = _affirmSettings.UseSandbox ? $"https://sandbox.affirm.com/api/v2/checkout/{token}" : $"https://api.affirm.com/api/v2/checkout/{token}";
                using (var client = new HttpClient())
                {
                    // Set up Basic Authentication
                    var byteArray = System.Text.Encoding.ASCII.GetBytes($"{_affirmSettings.PublicApiKey}:{_affirmSettings.PrivateApiKey}");
                    client.DefaultRequestHeaders.Authorization =
                        new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));

                    // Call the Affirm API
                    var response = await client.GetAsync(affirmApiUrl);

                    if (response.IsSuccessStatusCode)
                    {
                        // Read the response content as JSON
                        var responseBody = await response.Content.ReadAsStringAsync();
                        model = JsonConvert.DeserializeObject<AffirmResponseModel>(responseBody);
                        await _affirmLogRepository.InsertAsync(new AffirmLog()
                        {
                            CreatedOnUtc = DateTime.UtcNow,
                            CustomerId = (await _workContext.GetCurrentCustomerAsync())?.Id ?? 0,
                            FullMessage = responseBody,
                            IpAddress = _webHelper.GetCurrentIpAddress(),
                            LogLevel = LogLevel.Information,
                            ReferrerUrl = _webHelper.GetUrlReferrer(),
                            PageUrl = _webHelper.GetRawUrl(_httpContextAccessor.HttpContext.Request),
                            ShortMessage = $"Affirm Checkout Details {token}"

                        });

                    }
                    else
                    {
                        // Log error and handle failure
                        await _affirmLogRepository.InsertAsync(new AffirmLog()
                        {
                            CreatedOnUtc = DateTime.UtcNow,
                            CustomerId = (await _workContext.GetCurrentCustomerAsync())?.Id ?? 0,
                            FullMessage = await response.Content.ReadAsStringAsync(),
                            IpAddress = _webHelper.GetCurrentIpAddress(),
                            LogLevel = LogLevel.Error,
                            ReferrerUrl = _webHelper.GetUrlReferrer(),
                            PageUrl = _webHelper.GetRawUrl(_httpContextAccessor.HttpContext.Request),
                            ShortMessage = $"Failed to get CheckoutDetails {token}"

                        });

                    }
                }
            }
            catch (Exception ex)
            {
                await _affirmLogRepository.InsertAsync(new AffirmLog()
                {
                    CreatedOnUtc = DateTime.UtcNow,
                    CustomerId = (await _workContext.GetCurrentCustomerAsync())?.Id ?? 0,
                    FullMessage = ex.Message,
                    IpAddress = _webHelper.GetCurrentIpAddress(),
                    LogLevel = LogLevel.Error,
                    ReferrerUrl = _webHelper.GetUrlReferrer(),
                    PageUrl = _webHelper.GetRawUrl(_httpContextAccessor.HttpContext.Request),
                    ShortMessage = $"Failed to get CheckoutDetails {token}"

                });
            }
            return model;
        }

        public async Task<(string, string,string, string, HttpStatusCode)> CaptureTransaction(string token, Guid orderId, int orderTotal)
        {
            string captureResponse = string.Empty;
            (string authorizationTransactionId, string captureTransactionId, string authResponse, HttpStatusCode statusCode) = await AuthorizeTransaction(token, orderId, orderTotal);
            if (_affirmSettings.TransactMode == TransactMode.AuthorizeAndCapture)
            {
                if (statusCode != HttpStatusCode.OK)
                {
                    return (authorizationTransactionId, captureTransactionId, authResponse,captureResponse, statusCode);
                }
                else
                {
                    (_, captureTransactionId, captureResponse, statusCode) = await CaptureTransaction(token, orderId, orderTotal, authorizationTransactionId);
                }
            }

            return (authorizationTransactionId, captureTransactionId, authResponse, captureResponse, statusCode);
        }
        #endregion

        #region Utilities

        private async Task<(string, string, string, HttpStatusCode)> AuthorizeTransaction(string token, Guid orderId, int orderTotal)
        {
            string authorizationTransactionId = string.Empty;
            string captureTransactionId = string.Empty;
            HttpStatusCode statusCode = HttpStatusCode.OK;
            string response = string.Empty;
            await _affirmLogRepository.InsertAsync(new AffirmLog()
            {
                CreatedOnUtc = DateTime.UtcNow,
                CustomerId = (await _workContext.GetCurrentCustomerAsync())?.Id ?? 0,
                FullMessage = "Authorization Init",
                IpAddress = _webHelper.GetCurrentIpAddress(),
                LogLevel = LogLevel.Information,
                ReferrerUrl = _webHelper.GetUrlReferrer(),
                PageUrl = _webHelper.GetRawUrl(_httpContextAccessor.HttpContext.Request),
                ShortMessage = $"Authorization Transaction Init {token}"

            });
            try
            {
                var affirmApiUrl = _affirmSettings.UseSandbox ? $"https://sandbox.affirm.com/api/v1/transactions" : $"https://api.affirm.com/api/v1/transactions";
                using (var client = new HttpClient())
                {
                    var requestData = new
                    {
                        order_id = orderId,
                        amount = orderTotal,
                        merchant = new
                        {
                            name = _affirmSettings.FacingMerchantName,
                        },
                        transaction_id = token

                    };

                    var jsonRequest = JsonConvert.SerializeObject(requestData);
                    var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");
                    // Set up Basic Authentication
                    var byteArray = System.Text.Encoding.ASCII.GetBytes($"{_affirmSettings.PublicApiKey}:{_affirmSettings.PrivateApiKey}");
                    client.DefaultRequestHeaders.Authorization =
                        new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));

                    var apiResponse = await client.PostAsync(affirmApiUrl, content);
                    response = await apiResponse.Content.ReadAsStringAsync();
                    if (apiResponse.StatusCode == HttpStatusCode.OK)
                    {
                        var model = JsonConvert.DeserializeObject<AuthorizeCaptureModel>(response);
                        if (!string.Equals(model.status, "authorized", StringComparison.InvariantCulture))
                        {
                            await _affirmLogRepository.InsertAsync(new AffirmLog()
                            {
                                CreatedOnUtc = DateTime.UtcNow,
                                CustomerId = (await _workContext.GetCurrentCustomerAsync())?.Id ?? 0,
                                FullMessage = $"Authorization Failed {response}",
                                IpAddress = _webHelper.GetCurrentIpAddress(),
                                LogLevel = LogLevel.Error,
                                ReferrerUrl = _webHelper.GetUrlReferrer(),
                                PageUrl = _webHelper.GetRawUrl(_httpContextAccessor.HttpContext.Request),
                                ShortMessage = $"Authorization Transaction Failed Status  {model.status}"

                            });
                            statusCode = HttpStatusCode.InternalServerError;


                        }
                        else
                        {
                            authorizationTransactionId = model.id;
                            await _affirmLogRepository.InsertAsync(new AffirmLog()
                            {
                                CreatedOnUtc = DateTime.UtcNow,
                                CustomerId = (await _workContext.GetCurrentCustomerAsync())?.Id ?? 0,
                                FullMessage = $"Transaction Authorized {response}",
                                IpAddress = _webHelper.GetCurrentIpAddress(),
                                LogLevel = LogLevel.Information,
                                ReferrerUrl = _webHelper.GetUrlReferrer(),
                                PageUrl = _webHelper.GetRawUrl(_httpContextAccessor.HttpContext.Request),
                                ShortMessage = $"Transaction Authorized {token}"

                            });
                        }
                    }
                    else
                    {

                        await _affirmLogRepository.InsertAsync(new AffirmLog()
                        {
                            CreatedOnUtc = DateTime.UtcNow,
                            CustomerId = (await _workContext.GetCurrentCustomerAsync())?.Id ?? 0,
                            FullMessage = $"Authorization Failed {response}",
                            IpAddress = _webHelper.GetCurrentIpAddress(),
                            LogLevel = LogLevel.Error,
                            ReferrerUrl = _webHelper.GetUrlReferrer(),
                            PageUrl = _webHelper.GetRawUrl(_httpContextAccessor.HttpContext.Request),
                            ShortMessage = $"Authorization Transaction Failed {token}"

                        });

                        statusCode = apiResponse.StatusCode;
                    }

                }
            }
            catch (Exception exp)
            {
                statusCode = HttpStatusCode.InternalServerError;
                response = exp.Message;
                await _affirmLogRepository.InsertAsync(new AffirmLog()
                {
                    CreatedOnUtc = DateTime.UtcNow,
                    CustomerId = (await _workContext.GetCurrentCustomerAsync())?.Id ?? 0,
                    FullMessage = $"Authorization Failed {exp.Message}",
                    IpAddress = _webHelper.GetCurrentIpAddress(),
                    LogLevel = LogLevel.Error,
                    ReferrerUrl = _webHelper.GetUrlReferrer(),
                    PageUrl = _webHelper.GetRawUrl(_httpContextAccessor.HttpContext.Request),
                    ShortMessage = $"Authorization Transaction Failed {token}"

                });


            }

            await _affirmLogRepository.InsertAsync(new AffirmLog()
            {
                CreatedOnUtc = DateTime.UtcNow,
                CustomerId = (await _workContext.GetCurrentCustomerAsync())?.Id ?? 0,
                FullMessage = "Authorization Completed",
                IpAddress = _webHelper.GetCurrentIpAddress(),
                LogLevel = LogLevel.Information,
                ReferrerUrl = _webHelper.GetUrlReferrer(),
                PageUrl = _webHelper.GetRawUrl(_httpContextAccessor.HttpContext.Request),
                ShortMessage = $"Authorization Transaction Completed {token}"

            });
            return (authorizationTransactionId, captureTransactionId, response, statusCode);
        }


        private async Task<(string, string, string, HttpStatusCode)> CaptureTransaction(string token, Guid orderId, int orderTotal, string transactionId)
        {
            string authorizationTransactionId = transactionId;
            string captureTransactionId = string.Empty;
            HttpStatusCode statusCode = HttpStatusCode.OK;
            string response = string.Empty;
            await _affirmLogRepository.InsertAsync(new AffirmLog()
            {
                CreatedOnUtc = DateTime.UtcNow,
                CustomerId = (await _workContext.GetCurrentCustomerAsync())?.Id ?? 0,
                FullMessage = "Capture Init",
                IpAddress = _webHelper.GetCurrentIpAddress(),
                LogLevel = LogLevel.Information,
                ReferrerUrl = _webHelper.GetUrlReferrer(),
                PageUrl = _webHelper.GetRawUrl(_httpContextAccessor.HttpContext.Request),
                ShortMessage = $"Capture Transaction Init {token}"

            });
            try
            {
                var affirmApiUrl = _affirmSettings.UseSandbox ? $"https://sandbox.affirm.com/api/v1/transactions/{transactionId}/capture" : $"https://api.affirm.com/api/v1/transactions/{transactionId}/capture";
                using (var client = new HttpClient())
                {
                    var requestData = new
                    {
                        order_id = orderId,
                        amount = orderTotal,
                        merchant = new
                        {
                            name = _affirmSettings.FacingMerchantName,
                        },
                        transaction_id = token

                    };

                    var jsonRequest = JsonConvert.SerializeObject(requestData);
                    var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");
                    // Set up Basic Authentication
                    var byteArray = System.Text.Encoding.ASCII.GetBytes($"{_affirmSettings.PublicApiKey}:{_affirmSettings.PrivateApiKey}");
                    client.DefaultRequestHeaders.Authorization =
                        new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));

                    var apiResponse = await client.PostAsync(affirmApiUrl, content);
                    response = await apiResponse.Content.ReadAsStringAsync();
                    if (apiResponse.StatusCode == HttpStatusCode.OK)
                    {
                        var model = JsonConvert.DeserializeObject<AuthorizeCaptureModel>(response);
                        //if (!string.Equals(model.status, "authorized", StringComparison.InvariantCulture))
                        //{
                        //    await _affirmLogRepository.InsertAsync(new AffirmLog()
                        //    {
                        //        CreatedOnUtc = DateTime.UtcNow,
                        //        CustomerId = (await _workContext.GetCurrentCustomerAsync())?.Id ?? 0,
                        //        FullMessage = $"Capture Failed {response}",
                        //        IpAddress = _webHelper.GetCurrentIpAddress(),
                        //        LogLevel = LogLevel.Error,
                        //        ReferrerUrl = _webHelper.GetUrlReferrer(),
                        //        PageUrl = _webHelper.GetRawUrl(_httpContextAccessor.HttpContext.Request),
                        //        ShortMessage = $"Capture Transaction Failed Status  {model.status}"

                        //    });
                        //    statusCode = HttpStatusCode.InternalServerError;


                        //}
                        //else
                        //{
                            captureTransactionId = model.id;
                            await _affirmLogRepository.InsertAsync(new AffirmLog()
                            {
                                CreatedOnUtc = DateTime.UtcNow,
                                CustomerId = (await _workContext.GetCurrentCustomerAsync())?.Id ?? 0,
                                FullMessage = $"Transaction Captured {response}",
                                IpAddress = _webHelper.GetCurrentIpAddress(),
                                LogLevel = LogLevel.Information,
                                ReferrerUrl = _webHelper.GetUrlReferrer(),
                                PageUrl = _webHelper.GetRawUrl(_httpContextAccessor.HttpContext.Request),
                                ShortMessage = $"Transaction Captured {token}"

                            });
                      //  }

                    }
                    else
                    {

                        await _affirmLogRepository.InsertAsync(new AffirmLog()
                        {
                            CreatedOnUtc = DateTime.UtcNow,
                            CustomerId = (await _workContext.GetCurrentCustomerAsync())?.Id ?? 0,
                            FullMessage = $"Capture Failed {response}",
                            IpAddress = _webHelper.GetCurrentIpAddress(),
                            LogLevel = LogLevel.Error,
                            ReferrerUrl = _webHelper.GetUrlReferrer(),
                            PageUrl = _webHelper.GetRawUrl(_httpContextAccessor.HttpContext.Request),
                            ShortMessage = $"Capture Transaction Failed {token}"

                        });

                        statusCode = apiResponse.StatusCode;
                    }

                }
            }
            catch (Exception exp)
            {
                statusCode = HttpStatusCode.InternalServerError;
                response = exp.Message;
                await _affirmLogRepository.InsertAsync(new AffirmLog()
                {
                    CreatedOnUtc = DateTime.UtcNow,
                    CustomerId = (await _workContext.GetCurrentCustomerAsync())?.Id ?? 0,
                    FullMessage = $"Capture Failed {exp.Message}",
                    IpAddress = _webHelper.GetCurrentIpAddress(),
                    LogLevel = LogLevel.Error,
                    ReferrerUrl = _webHelper.GetUrlReferrer(),
                    PageUrl = _webHelper.GetRawUrl(_httpContextAccessor.HttpContext.Request),
                    ShortMessage = $"Capture Transaction Failed {token}"

                });


            }

            await _affirmLogRepository.InsertAsync(new AffirmLog()
            {
                CreatedOnUtc = DateTime.UtcNow,
                CustomerId = (await _workContext.GetCurrentCustomerAsync())?.Id ?? 0,
                FullMessage = "Capture Completed",
                IpAddress = _webHelper.GetCurrentIpAddress(),
                LogLevel = LogLevel.Information,
                ReferrerUrl = _webHelper.GetUrlReferrer(),
                PageUrl = _webHelper.GetRawUrl(_httpContextAccessor.HttpContext.Request),
                ShortMessage = $"Capture Transaction Completed {token}"

            });
            return (authorizationTransactionId, captureTransactionId, response, statusCode);
        }

        #endregion

    }

}

