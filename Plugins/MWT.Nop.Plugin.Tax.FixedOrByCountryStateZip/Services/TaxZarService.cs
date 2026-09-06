using MWT.Tax.FixedOrByCountryStateZip.Domain;
using Newtonsoft.Json;
using Nop.Core.Http;
using Nop.Services.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace MWT.Tax.FixedOrByCountryStateZip.Services
{
    public partial class TaxZarService : ITaxZarService
    {
        #region Fields
        private readonly TaxZarSettings _taxZarSettings;
        private readonly ITaxLogService _transactionLogService;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ISettingService _settingService;
        #endregion

        #region Ctor

        public TaxZarService(TaxZarSettings taxZarSettings, ITaxLogService transactionLogService,
            IHttpClientFactory httpClientFactory, ISettingService settingService)
        {
            this._transactionLogService = transactionLogService;
            this._httpClientFactory = httpClientFactory;
            this._taxZarSettings = taxZarSettings;
            this._settingService = settingService;

        }

        #endregion

        #region Methods
        public async Task<(bool, bool, decimal, string,string, int, string)> GetTaxRate(int customerId, string zipcode)
        {
            bool isRequesdtProcessed = false;
            bool shippingTaxable = false;
            decimal taxRate = 0;
            int statusCode = 0;
            string url = "";
            MWTTaxZarTransactionLog log = new MWTTaxZarTransactionLog();
            string response = "";
            try
            {
                if (!string.IsNullOrEmpty(zipcode))
                {
                    string apiLink = _taxZarSettings.ApiLink;
                    string token = _taxZarSettings.Token;
                    if (!string.IsNullOrEmpty(apiLink) && !string.IsNullOrEmpty(token))
                    {
                        if (apiLink.Substring(0, apiLink.Length - 1) == "/")
                            apiLink = apiLink.Substring(0, apiLink.Length - 1);
                        url = apiLink + "/" + zipcode;
                        var _httpClient = _httpClientFactory.CreateClient(NopHttpDefaults.DefaultHttpClient);
                        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _taxZarSettings.Token);
                        response = await _httpClient.GetStringAsync(url);

                        dynamic obj = JsonConvert.DeserializeObject(response);
                        statusCode = 200;
                        if (obj.rate != null)
                        {
                            isRequesdtProcessed = true;
                            decimal.TryParse((string)obj.rate.combined_rate, out taxRate);
                            taxRate = taxRate * 100;

                            if (obj.rate.state != null)
                            {
                                var taxableStates = await this._settingService.GetSettingByKeyAsync<string>("Taxable.States.Codes");
                                if (!string.IsNullOrEmpty(taxableStates))
                                {
                                    if (!taxableStates.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries).Where(s => s.Trim().Equals(
                                         ((string)obj.rate.state).Trim(), StringComparison.InvariantCultureIgnoreCase)).Any())
                                        taxRate = 0;
                                }
                            }
                            if (obj.rate.freight_taxable != null)
                                bool.TryParse((string)obj.rate.freight_taxable, out shippingTaxable);
                        }
                    }
                    else
                        response = string.IsNullOrEmpty(apiLink) ? "Please provide Api link" : "Please provide Token";
                }
                else
                    response = "zip code empty";
            }
            catch (Exception ex)
            {
                statusCode = 500;
                response = ex.Message;
            }
            
            if (!string.IsNullOrEmpty(zipcode))
                await _transactionLogService.InsertLog(new MWTTaxZarTransactionLog()
                {
                    CreatedDateUtc = DateTime.UtcNow,
                    CustomerId = customerId,
                    ResponseMessage = response,
                    StatusCode = statusCode,
                    RequestMessage = zipcode,
                    Url = url,
                    OrderId = 0
                });
            return (isRequesdtProcessed, shippingTaxable, taxRate, response,"", statusCode, url);
        }

        #endregion
    }
}
