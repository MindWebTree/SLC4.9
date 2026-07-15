
using Newtonsoft.Json;
using MWT.Tax.FixedOrByCountryStateZip.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http.Headers;
using Nop.Core.Domain.Common;
using Nop.Services.Configuration;
using Nop.Services.Directory;

namespace MWT.Tax.FixedOrByCountryStateZip.Services
{
    public partial class ZipTaxService : IZipTaxService
    {
        #region Fields
        private readonly TaxZarSettings _taxZarSettings;
        private readonly ITaxLogService _transactionLogService;
        private readonly HttpClient _httpClient;
        private readonly ISettingService _settingService;
        private readonly IStateProvinceService _stateProvinceService;
        #endregion

        #region Ctor

        public ZipTaxService(TaxZarSettings taxZarSettings, ITaxLogService transactionLogService,
            HttpClient httpClient, ISettingService settingService,
            IStateProvinceService stateProvinceService)
        {
            this._transactionLogService = transactionLogService;
            this._httpClient = httpClient;
            this._taxZarSettings = taxZarSettings;
            this._settingService = settingService;
            this._stateProvinceService = stateProvinceService;

        }

        #endregion

        #region Methods
        public async Task<(bool, bool, decimal, string, string, int, string)> GetTaxRate(int customerId, Address address)
        {
            bool isRequestProcessed = false;
            bool shippingTaxable = false;
            decimal taxRate = 0;
            int statusCode = 0;
            string url = "";
            string response = "zip code empty";
            string apiLink = _taxZarSettings.ZipTaxApiLink;
            string zipTaxKey = _taxZarSettings.ZipTaxKey;
            string taxRateInfo = "";
            if (string.IsNullOrEmpty(apiLink) || string.IsNullOrEmpty(zipTaxKey))
            {
                response = string.IsNullOrEmpty(apiLink) ? "Please provide Api link" : "Please provide Token";
            }
            else if (!string.IsNullOrEmpty(address?.ZipPostalCode ?? ""))
            {
                string city = string.IsNullOrEmpty(address.City) ? "" : address.City.Trim(); ;
                if (apiLink.Substring(0, apiLink.Length - 1) == "/")
                    apiLink = apiLink.Substring(0, apiLink.Length - 1);
                apiLink = apiLink + "?key=" + zipTaxKey;

                #region GetTaxRate By Address
                string _address = (address.Address1 ?? "") + ",";
                _address = _address + (string.IsNullOrEmpty(address.City) ? "" : (" " + address.City + ","));
                _address = _address + (address.StateProvinceId > 0 ? (" " + (await _stateProvinceService.GetStateProvinceByIdAsync(Convert.ToInt32(address.StateProvinceId)))?.Abbreviation ?? "") : "") + " " + address.ZipPostalCode;

                (isRequestProcessed, shippingTaxable, taxRate, response, taxRateInfo, statusCode, url) = await GetTaxRateFromZipTaxService(customerId,
                    address.ZipPostalCode, city, apiLink + "&address=" + _address);

                if (!isRequestProcessed)
                {
                    (isRequestProcessed, shippingTaxable, taxRate, response, taxRateInfo, statusCode, url) = await GetTaxRateFromZipTaxService(customerId, address.ZipPostalCode, city, apiLink + "&postalcode=" + address.ZipPostalCode);
                }

                #endregion
            }

            return (isRequestProcessed, shippingTaxable, taxRate, response, taxRateInfo, statusCode, url);

        }
        #endregion
        #region Private Methods
        public async Task<(bool, bool, decimal, string, string, int, string)> GetTaxRateFromZipTaxService(int customerId, string zipCode, string city, string url)
        {
            bool isRequestProcessed = false;
            bool shippingTaxable = false;
            decimal taxRate = 0;
            int statusCode = 0;
            string taxRateInfo = "";
            MWTTaxZarTransactionLog log = new MWTTaxZarTransactionLog();
            string response = "";

            try
            {
                response = await (await _httpClient.GetAsync(url)).Content.ReadAsStringAsync();
                dynamic obj = JsonConvert.DeserializeObject(response);
                statusCode = 200;
                List<TaxRateInfo> lstTaxRateInfo = new List<TaxRateInfo>();
                string geoCity = "";
                string geoPostalCode = "";
                if (obj.results != null)
                {
                    foreach (var result in obj.results)
                    {
                        bool _shippingTaxable = false;
                        decimal _taxRate = 0;
                        decimal.TryParse((string)result.taxSales, out _taxRate);
                        _taxRate = _taxRate * 100;
                        if (result.geoState != null)
                        {
                            var taxableStates = await this._settingService.GetSettingByKeyAsync<string>("Taxable.States.Codes");
                            if (!string.IsNullOrEmpty(taxableStates))
                            {
                                if (!taxableStates.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries).Where(s => s.Trim().Equals(
                                     ((string)result.geoState).Trim(), StringComparison.InvariantCultureIgnoreCase)).Any())
                                    _taxRate = 0;
                            }
                        }
                        if (result.txbFreight != null)
                            _shippingTaxable = ((string)result.txbFreight) == "Y" ? true : false;

                        geoCity = "";
                        geoPostalCode = "";
                        try
                        {
                            geoCity = string.IsNullOrEmpty((string)result.geoCity) ? "" : (string)result.geoCity;
                        }
                        catch
                        {

                        }
                        try
                        {
                            geoPostalCode = string.IsNullOrEmpty((string)result.geoPostalCode) ? "" : (string)result.geoPostalCode;
                        }
                        catch
                        {

                        }
                        lstTaxRateInfo.Add(new TaxRateInfo()
                        {
                            shippingTaxable = _shippingTaxable,
                            TaxRate = _taxRate,
                            TaxObject = JsonConvert.SerializeObject(result),
                            City = geoCity,
                            PostalCode = geoPostalCode
                        });
                    }
                }
                if (lstTaxRateInfo.Count > 0)
                {


                    var taxRates = lstTaxRateInfo.Where(t => string.Equals(t.PostalCode.Trim(), zipCode.Trim(), StringComparison.InvariantCultureIgnoreCase)
                    && string.Equals(t.City.Trim(), city.Trim(), StringComparison.InvariantCultureIgnoreCase)
                    );
                    if (taxRates.Count() == 0)
                        taxRates = lstTaxRateInfo.Where(t => string.Equals(t.PostalCode.Trim(), zipCode.Trim(), StringComparison.InvariantCultureIgnoreCase));

                    if (taxRates.Count() > 0)
                    {
                        isRequestProcessed = true;
                        taxRate = taxRates.OrderByDescending(t => t.TaxRate).FirstOrDefault().TaxRate;
                        shippingTaxable = taxRates.OrderByDescending(t => t.TaxRate).FirstOrDefault().shippingTaxable;
                        taxRateInfo = taxRates.OrderByDescending(t => t.TaxRate).FirstOrDefault().TaxObject;
                    }

                }

            }
            catch (Exception ex)
            {
                statusCode = 500;
                response = ex.Message;
            }


            await _transactionLogService.InsertLog(new MWTTaxZarTransactionLog()
            {
                CreatedDateUtc = DateTime.UtcNow,
                CustomerId = customerId,
                ResponseMessage = response,
                StatusCode = statusCode,
                RequestMessage = zipCode,
                Url = url,
                OrderId = 0,
                TaxRateInfo = taxRateInfo
            });
            return (isRequestProcessed, shippingTaxable, taxRate, response, taxRateInfo, statusCode, url);
        }

        #endregion


    }

    public class TaxRateInfo
    {
        public decimal TaxRate { get; set; }
        public string City { get; set; }
        public bool shippingTaxable { get; set; }
        public string PostalCode { get; set; }
        public string TaxObject { get; set; }
    }
}
