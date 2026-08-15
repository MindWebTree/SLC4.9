using Microsoft.AspNetCore.Http;
using MWT.Nop.Core.Domain.Customers;
using MWT.Nop.Core.Domain.Zoho;
using MWT.Nop.Core.Infrastructure;
using MWT.Nop.Core.Services.Customers;
using MWT.Nop.Core.Services.IPLite;
using MWT.Nop.Core.Services.Zoho;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Nop.Core;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Logging;
using Nop.Data;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Customers;
using Nop.Services.Customizations.CustomOrders;
using Nop.Services.Directory;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Orders;
using System.Dynamic;
using System.Text;
using System.Text.RegularExpressions;

namespace MWT.Nop.Core.Service.Zoho
{
    public partial class ZohoService : IZohoService
    {
        #region Fields
        private readonly HttpClient _httpClient;
        private static string _Token = "ac50b4678eb291e4b7df0d1d5e477acd";
        private static DateTime _ExpiryDate;
        private readonly ISettingService _settingService;
        private readonly ILocalizationService _localizationService;
        private string _LeadOwner = "Raj Kumar";
        private readonly ILogger _logger;
        private readonly IWorkContext _workContext;
        private readonly IIPLiteService _iIPLiteService;
        private readonly IRepository<QueuedZohoCustomer> _queuedZohoCustomerRepository;
        private readonly ICustomerExtendedService _customerService;
        private readonly IAddressService _addressService;
        private readonly ICountryService _countryService;
        private readonly IStateProvinceService _stateProvinceService;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly IStoreContext _storeContext;
       private readonly ICustomOrderService _customOrderService;
        private readonly IOrderService _orderService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        #endregion

        public ZohoService(ISettingService settingService, ILocalizationService localizationService,
            HttpClient httpClient, ILogger logger, IWorkContext workContext, IIPLiteService iIPLiteService,
             IRepository<QueuedZohoCustomer> queuedZohoCustomerRepository, ICustomerExtendedService customerService,
             IAddressService addressService, ICountryService countryService,
        IStateProvinceService stateProvinceService, IGenericAttributeService genericAttributeService,
        IStoreContext storeContext, ICustomOrderService customOrderService
         IOrderService orderService, IHttpContextAccessor httpContextAccessor)
        {
            _settingService = settingService;
            _localizationService = localizationService;
            _httpClient = httpClient;
            _logger = logger;
            _workContext = workContext;
            _iIPLiteService = iIPLiteService;
            _queuedZohoCustomerRepository = queuedZohoCustomerRepository;
            _customerService = customerService;
            _addressService = addressService;
            _countryService = countryService;
            _stateProvinceService = stateProvinceService;
            _genericAttributeService = genericAttributeService;
            _storeContext = storeContext;
          _customOrderService = customOrderService;
            _orderService = orderService;
            _httpContextAccessor = httpContextAccessor;
        }
        #region Methods
        public async Task<string> SaveLead(ZohoDto zohoObj, string zohoLeadId)
        {
            try
            {
                string fakeCustomerEmails = await _settingService.GetSettingByKeyAsync<string>("Common.FakeCustomer.Emails");
                if (CustomCommonHelper.IsFakeCustomer(fakeCustomerEmails, zohoObj.Email ?? string.Empty))
                {
                    return string.Empty;
                }

                if (DateTime.Now.AddMinutes(10) >= _ExpiryDate)
                    await GetAuthenticationToken();
                dynamic obj = new ExpandoObject();

                (string CountryCode, string Country, string Region, string City, string Longitude, string Latitude, string ZipCode, string TimeZone) = await _iIPLiteService.getDetail(zohoObj.IPAddress);
                obj.GALead_ID = zohoObj.GALeadID;
                obj.Lead_Source = zohoObj.LeadSource;
                obj.Lead_Owner = _LeadOwner;
                obj.Lead_Status = zohoObj.LeadStatus;
                string firstName = removeSpecialCharacters(zohoObj.FirstName);
                string lastName = "";

                string[] arrName = firstName.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (arrName.Length > 1)
                {

                    firstName = string.Join(' ', arrName.Take(arrName.Length - 1));
                    lastName = arrName[arrName.Length - 1];
                }


                if (firstName != string.Empty)
                    obj.First_Name = firstName;
                if (lastName != string.Empty)
                    obj.Last_Name = lastName;
                else
                    obj.Last_Name = "-";
                if (!string.IsNullOrEmpty(zohoObj.Email))
                    obj.Email = zohoObj.Email;
                if (!string.IsNullOrEmpty(zohoObj.Phone))
                    obj.Phone = removeSpecialCharacters(zohoObj.Phone);

                if (City != string.Empty)
                    obj.City = removeSpecialCharacters(City);
                if (Country != string.Empty)
                    obj.Country = removeSpecialCharacters(Country);

                if (Region != string.Empty)
                    obj.State = removeSpecialCharacters(Region);

                if (!string.IsNullOrEmpty(zohoObj.ZipCode))
                    obj.Zip_Code = removeSpecialCharacters(zohoObj.ZipCode);
                else if (Region != string.Empty)
                    obj.Zip_Code = removeSpecialCharacters(Region);

                if (!string.IsNullOrEmpty(zohoObj.Description))
                    obj.Description = removeSpecialCharacters(zohoObj.Description) + zohoObj.Attachments;
                if (!string.IsNullOrEmpty(zohoObj.Message))
                    obj.Message = removeSpecialCharacters(zohoObj.Message);
                if (!string.IsNullOrEmpty(zohoObj.SKU))
                    obj.SKU = removeSpecialCharacters(zohoObj.SKU);
                if (!string.IsNullOrEmpty(zohoObj.ProductName))
                    obj.Product_Name = removeSpecialCharacters(zohoObj.ProductName);
                if (!string.IsNullOrEmpty(zohoObj.IPAddress))
                    obj.IP_Address = zohoObj.IPAddress;

                if (zohoObj.customer != null)
                {
                    string gclid = await _genericAttributeService.GetAttributeAsync<string>(zohoObj.customer, CustomNopCustomerDefaults.GCLID);
                    if (!string.IsNullOrEmpty(gclid))
                        obj.gclid = gclid;

                    string campaign = await _genericAttributeService.GetAttributeAsync<string>(zohoObj.customer, CustomNopCustomerDefaults.Campaign);
                    if (!string.IsNullOrEmpty(campaign))
                        obj.Ad_Campaign_Name = campaign;
                }

                if (!string.IsNullOrEmpty(zohoObj.Address))
                    obj.Address_Of_Customer = zohoObj.Address;

                if (zohoObj.Total > 0)
                {
                    obj.Amount = zohoObj.Total;
                }
                // object to json

                string request = "{\"data\":[" + Newtonsoft.Json.JsonConvert.SerializeObject(obj) + "	]}";
                request = request.Replace("\"gclid\"", "\"$gclid\"");
                var requestContent = new StringContent(request,
               Encoding.UTF8, MimeTypes.ApplicationJson);
                // end

                //oIPLite = null;
                if (!_httpClient.DefaultRequestHeaders.Contains("Authorization"))
                {
                    _httpClient.DefaultRequestHeaders.Add("Authorization", "Zoho-oauthtoken  " + _Token);
                }
                var response = !string.IsNullOrEmpty(zohoLeadId) ? await _httpClient.PutAsync("https://www.zohoapis.com/crm/v2/Leads/" + zohoLeadId, requestContent) : await _httpClient.PostAsync("https://www.zohoapis.com/crm/v2/Leads", requestContent);
                response.EnsureSuccessStatusCode();
                var content = await response.Content.ReadAsStringAsync();
                var jsonObject = JObject.Parse(content);
                var id = jsonObject["data"][0]["details"]["id"].ToString();

                return id;
            }
            catch (Exception ex)
            {
                await _logger.InsertLogAsync(LogLevel.Error, "Zoho: SaveLead", "Exception:   " + ex.Message,
                  await _workContext.GetCurrentCustomerAsync());

                return "";
            }
        }
        public async Task<List<QueuedZohoCustomer>> QueuedZohoCustomerListAsync()
        {
            return await (from queuedZohoCustomer in _queuedZohoCustomerRepository.Table
                          where queuedZohoCustomer.IsProcessed == false
                          && queuedZohoCustomer.NoOfTries < 4
                          orderby queuedZohoCustomer.Id
                          select queuedZohoCustomer).ToListAsync();
        }
        public async Task InsertQueuedZohoCustomerAsync(int customerId)
        {
            try
            {
                var customer = await _queuedZohoCustomerRepository.Table.Where(c => c.CustomerId == customerId).FirstOrDefaultAsync();
                if (customer == null)
                {
                    await _queuedZohoCustomerRepository.InsertAsync(new QueuedZohoCustomer()
                    {

                        CreatedOn = DateTime.UtcNow,
                        CustomerId = customerId,
                        IsProcessed = false,
                        NoOfTries = 0,
                        UpdatedOn = null,
                        ProcessedOn = null
                    });
                }
                else
                {
                    customer.NoOfTries = 0;
                    customer.ProcessedOn = null;
                    customer.IsProcessed = false;
                    await _queuedZohoCustomerRepository.UpdateAsync(customer);
                }
            }
            catch (Exception exp)
            {
                await _logger.InsertLogAsync(LogLevel.Error, "Zoho: Failed to Queued Customer", exp.Message);
            }
        }
        public async Task UpdateQueuedZohoCustomerAsync(QueuedZohoCustomer queuedZohoCustomer)
        {
            try
            {
                await _queuedZohoCustomerRepository.UpdateAsync(queuedZohoCustomer);
            }
            catch (Exception exp)
            {
                await _logger.InsertLogAsync(LogLevel.Error, "Zoho: Failed to Update Customer", exp.Message);
            }
        }
        public async Task SaveContact(QueuedZohoCustomer queuedCustomer)
        {
            bool isProcessed = true;
            string zohoRefid = queuedCustomer.ZohoReferenceId ?? "";
            var customer = await _customerService.GetCustomerByIdAsync(queuedCustomer.CustomerId);
            try
            {
                if (customer != null)
                {
                    var oZoho = new ZohoDto();
                    oZoho.GALeadID = new Random().Next(1, 100000).ToString();
                    oZoho.LeadSource = $"NEW STORE - Customer Registered";
                    string email = await this._customerService.GetCustomerEmail(customer);
                    oZoho.Email = email;
                    oZoho.ZipCode = "-";
                    string name = "", zipcode = "", phone = "", countryCode = "", stateCode = "", streetAddress = "", state = "", country = "", city = "";
                    name = (await this._customerService.GetCustomerFullNameAsync(customer)) ?? "";
                    oZoho.FullName = name;
                    if (!string.IsNullOrEmpty(email))
                    {
                        if (customer.ShippingAddressId.HasValue && customer.ShippingAddressId.Value > 0)
                        {
                            var address = await _addressService.GetAddressByIdAsync(Convert.ToInt32(customer.ShippingAddressId));


                            if (address.CountryId.HasValue && address.CountryId.Value > 0)
                            {
                                countryCode = (await _countryService.GetCountryByIdAsync(address.CountryId.Value))?.TwoLetterIsoCode ?? "";
                            }
                            if (address.StateProvinceId.HasValue && address.StateProvinceId.Value > 0)
                            {
                                stateCode = (await _countryService.GetCountryByIdAsync(address.StateProvinceId.Value))?.TwoLetterIsoCode ?? "";
                            }
                            phone = address.PhoneNumber;
                            oZoho.Address = streetAddress = address.Address1;
                            oZoho.City = city = address.City;
                            oZoho.State = state = stateCode;
                            oZoho.Country = country = address.County;
                            zipcode = address.ZipPostalCode;
                            oZoho.ZipCode = zipcode;
                            oZoho.Phone = String.IsNullOrEmpty(address.PhoneNumber) ? " - " : address.PhoneNumber;
                            oZoho.FirstName = address.FirstName ?? string.Empty;
                            oZoho.LastName = address.LastName ?? string.Empty;
                        }
                        else if (customer.BillingAddressId.HasValue && customer.BillingAddressId.Value > 0)
                        {
                            var address = await _addressService.GetAddressByIdAsync(Convert.ToInt32(customer.BillingAddressId));
                            zipcode = address.ZipPostalCode ?? "";
                            phone = address.PhoneNumber;
                            if (address.CountryId.HasValue && address.CountryId.Value > 0)
                            {
                                countryCode = (await _countryService.GetCountryByIdAsync(address.CountryId.Value))?.TwoLetterIsoCode ?? "";
                            }
                            if (address.StateProvinceId.HasValue && address.StateProvinceId.Value > 0)
                            {
                                stateCode = (await _countryService.GetCountryByIdAsync(address.StateProvinceId.Value))?.TwoLetterIsoCode ?? "";
                            }
                            phone = address.PhoneNumber;
                            oZoho.Address = streetAddress = address.Address1;
                            oZoho.City = city = address.City;
                            oZoho.State = state = stateCode;
                            oZoho.Country = country = address.County;
                            zipcode = address.ZipPostalCode;
                            oZoho.ZipCode = zipcode;
                            oZoho.Phone = String.IsNullOrEmpty(address.PhoneNumber) ? " - " : address.PhoneNumber;
                            oZoho.FirstName = address.FirstName ?? string.Empty;
                            oZoho.LastName = address.LastName ?? string.Empty;
                        }

                        if (string.IsNullOrEmpty(phone))
                        {
                            phone = await _genericAttributeService.GetAttributeAsync<string>(customer, "Phone");
                            oZoho.Phone = phone;
                        }
                        var splitedName = name.Split(' ', '\t');
                        var lastName = string.Join(" ", name.Split(' ').Skip(1));
                        if (string.IsNullOrEmpty(oZoho.FirstName))
                        {
                            oZoho.FirstName = splitedName[0];
                            oZoho.LastName = lastName;
                        }
                        string fakeCustomerEmails = await _settingService.GetSettingByKeyAsync<string>("Common.FakeCustomer.Emails");
                        if (!CustomCommonHelper.IsFakeCustomer(fakeCustomerEmails, email))
                        {

                            string jsonPayload = @"{
                            ""data"":[{                                
                                ""First_Name"": """ + splitedName[0] + @""",
                                ""Last_Name"": """ + lastName + @""",
                                ""Email"": """ + email + @""",
                                ""Other_Phone"": """ + phone + @""",
                                ""Phone"": """ + phone + @""",
                                ""Lead_Source"": ""Website"",
                                ""Other_Street"": """ + streetAddress + @""",
                                ""Other_City"": """ + city + @""",
                                ""Mailing_City"": """ + city + @""",
                                ""Mailing_State"": """ + state + @""",
                                ""Other_Zip"": """ + zipcode + @""",
                            ""Other_Country"": """ + country + @""",
                            ""Mailing_Country"": """ + country + @"""
                            }]}
                        ";

                            if (DateTime.Now.AddMinutes(10) >= _ExpiryDate)
                                await GetAuthenticationToken();

                            string authToken = _Token;
                            string crmUrl = "https://www.zohoapis.com/crm/v2/contacts" + (string.IsNullOrEmpty(zohoRefid) ? "" : ("/" + zohoRefid));

                            using (HttpClient httpClient = new HttpClient())
                            {
                                if (!_httpClient.DefaultRequestHeaders.Contains("Authorization"))
                                {
                                    httpClient.DefaultRequestHeaders.Add("Authorization", $"Zoho-oauthtoken {authToken}");
                                }
                                HttpContent content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");
                                HttpResponseMessage response = new HttpResponseMessage();

                                if (string.IsNullOrEmpty(zohoRefid))
                                {
                                    response = await httpClient.PostAsync(crmUrl, content);
                                }
                                else
                                {
                                    response = await httpClient.PutAsync(crmUrl, content);
                                }
                                if (!response.IsSuccessStatusCode)
                                {
                                    isProcessed = false;
                                }
                                else
                                {
                                    dynamic records = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());
                                    foreach (var record in records.data)
                                    {
                                        zohoRefid = record.details.id;
                                    }
                                }
                            }
                        }
                    }

                    //#region Save Lead

                    //await this.SaveLead(oZoho, null);


                    //#endregion
                }

                queuedCustomer.IsProcessed = isProcessed;
                queuedCustomer.NoOfTries = queuedCustomer.NoOfTries + 1;
                queuedCustomer.ProcessedOn = isProcessed ? DateTime.UtcNow : null;
                queuedCustomer.UpdatedOn = DateTime.UtcNow;
                queuedCustomer.ZohoReferenceId = zohoRefid;
                await this.UpdateQueuedZohoCustomerAsync(queuedCustomer);
            }
            catch (Exception ex)
            {

            }
        }
        public async Task<string> CreateUpdateOrderContactPotential(int orderId, string zohoPotentialId, Customer customer, string orderStatus, string description, string ipAddress, string glclidCookie, decimal orderTotal,
            int createdBy, string orderLink, bool isCustomOrder = false)
        {
            try
            {
                ZohoDto oZoho = new ZohoDto();
                oZoho.Email = await _customerService.GetCustomerEmail(customer);
                var address = await _addressService.GetAddressByIdAsync((int)customer.ShippingAddressId);
                oZoho.Phone = await _customerService.GetCustomerPhone(customer) ?? " - ";
                oZoho.LastName = oZoho.FirstName = oZoho.FullName = (await _customerService.GetCustomerFullNameAsync(customer)) ?? "".Trim();
                oZoho.ZipCode = address?.ZipPostalCode ?? " - ";

                var formattedAddress = new StringBuilder();
                if (address != null)
                {
                    if (!string.IsNullOrEmpty(address.Address1))
                        formattedAddress.AppendLine(address.Address1);

                    // Add second line of the address (optional, e.g., "Apt 101")
                    if (!string.IsNullOrEmpty(address.Address2))
                        formattedAddress.AppendLine(address.Address2);

                    // Add city, state, and postal code
                    if (!string.IsNullOrEmpty(address.City))
                        formattedAddress.Append(address.City);

                    if ((address.StateProvinceId ?? 0) > 0)
                    {
                        formattedAddress.Append(", " + (await _stateProvinceService.GetStateProvinceByIdAsync((int)address.StateProvinceId))?.Name ?? "");
                    }

                    if (!string.IsNullOrEmpty(address.ZipPostalCode))
                        formattedAddress.Append(" " + address.ZipPostalCode);
                    if ((address.CountryId ?? 0) > 0)
                    {
                        formattedAddress.Append(", " + (await _countryService.GetCountryByIdAsync((int)address.CountryId))?.Name ?? "");
                    }

                    formattedAddress.AppendLine();
                }

                // Add country

                oZoho.Address = formattedAddress.ToString();
                oZoho.Description = description;
                int min = 1;
                int max = 100000;
                Random random = new Random();
                oZoho.GALeadID = random.Next(min, max).ToString();

                oZoho.Message = "";
                if (ipAddress != null)
                    oZoho.IPAddress = ipAddress;

                if (customer != null)
                    oZoho.customer = customer;

                oZoho.LeadStatus = orderStatus;
                if (isCustomOrder)
                {
                    return await SavePotential(oZoho, zohoPotentialId, orderId, orderTotal, createdBy, orderLink);
                }
                else
                {
                    return await SaveContact(oZoho, zohoPotentialId, orderId);
                }
            }
            catch (Exception exp)
            {
                await _logger.InsertLogAsync(LogLevel.Error, "Zoho Order Potential", exp.Message);
                return "";
            }
        }

        #endregion

        #region Utilities
        private string removeSpecialCharacters(string pValue)
        {
            Regex re = new Regex(@"[\[\]\\\^\$\.\|\?\*\+\(\)\{\}%,;><!@#&!\-\+/]");
            return re.Replace(pValue, " ");
        }
        public async Task GetAuthenticationToken()
        {

            string url = "https://accounts.zoho.com/oauth/v2/token";
            var requestContent = new StringContent(string.Format("grant_type=refresh_token&client_id={0}&client_secret={1}&token_type=Bearer&refresh_token={2}&redirect_uri{3}",
                 await _settingService.GetSettingByKeyAsync<string>("Zoho_ClientID"), await _settingService.GetSettingByKeyAsync<string>("Zoho_Secret"),
                 await _settingService.GetSettingByKeyAsync<string>("Zoho_refresh_token"), await _settingService.GetSettingByKeyAsync<string>("Zoho_redirect_uri")),
              Encoding.UTF8, MimeTypes.ApplicationXWwwFormUrlencoded);
            var response = await _httpClient.PostAsync(url, requestContent);
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            dynamic obj = Newtonsoft.Json.JsonConvert.DeserializeObject(content);
            _Token = obj.access_token;
            _ExpiryDate = DateTime.Now.AddMinutes(40);
        }
        public async Task<ZohoDto> GetContactDetailsFromZoho(string email, string phone)
        {
            ZohoDto zohoObj = new ZohoDto();
            if (DateTime.Now.AddMinutes(10) >= _ExpiryDate)
                await GetAuthenticationToken();
            try
            {
                string query = $"?criteria=({(string.IsNullOrEmpty(email.Trim()) ? "" : "(Email:equals:" + email.Trim() + ")") + (!string.IsNullOrEmpty(phone.Trim()) && !string.IsNullOrEmpty(email.Trim()) ? "or" : "") + (string.IsNullOrEmpty(phone.Trim()) ? "" : "(Phone:equals:" + phone.Trim() + ")")})";
                if (!_httpClient.DefaultRequestHeaders.Contains("Authorization"))
                {
                    _httpClient.DefaultRequestHeaders.Add("Authorization", "Zoho-oauthtoken  " + _Token);
                }
                var response = await _httpClient.GetAsync("https://www.zohoapis.com/crm/v2/Contacts/search" + query);
                response.EnsureSuccessStatusCode();
                if (response.StatusCode == System.Net.HttpStatusCode.NoContent)
                    return null;
                var content = await response.Content.ReadAsStringAsync();
                dynamic obj = JsonConvert.DeserializeObject<dynamic>(content);
                foreach (var lead in obj.data)
                {
                    if (((string)lead.Email).Trim().ToLower() == email.Trim().ToLower() ||
                        ((string)lead.Phone).Trim().ToLower() == phone.Trim().ToLower()
                        )
                    {
                        zohoObj.Company = (string)lead.Company;
                        zohoObj.Email = (string)lead.Email;
                        zohoObj.ZipCode = (string)lead.Zip_Code;
                        zohoObj.City = (string)lead.City;
                        zohoObj.State = (string)lead.State;
                        zohoObj.FullName = (string)lead.Full_Name;
                        zohoObj.FirstName = (string)lead.First_Name;
                        zohoObj.LastName = (string)lead.Last_Name;
                        zohoObj.Phone = (string)lead.Phone;
                        zohoObj.Address = (string)lead.Address_Of_Customer;
                        break;
                    }
                }

            }
            catch (Exception ex)
            {
                await _logger.InsertLogAsync(LogLevel.Error, "Zoho Api", "Exception:   " + ex.Message,
                  await _workContext.GetCurrentCustomerAsync());
            }
            finally
            {

            }

            return zohoObj;
        }
        private async Task<string> SavePotential(ZohoDto zohoObj, string zohoLeadId, int orderNumber, decimal orderTotal, int createdBy, string orderLink)
        {
            try
            {
                string leadCreationDate = null;
                string leadClosingDate = null;

         
                if (!string.Equals(zohoObj.LeadStatus, "Paid"))
                {
                    var customOrder = await _customOrderService.GetById(orderNumber);
                    if (customOrder != null)
                    {
                        leadCreationDate = ((DateTime)customOrder.CreatedOn).ToString("yyyy-MM-dd");
                    }
                }
                else
                {
                    leadClosingDate = (await _orderService.GetOrderByIdAsync(orderNumber))?.CreatedOnUtc.ToString("yyyy-MM-dd") ?? null;

                }
                if (DateTime.Now.AddMinutes(10) >= _ExpiryDate)
                    await GetAuthenticationToken();
                dynamic obj = new ExpandoObject();

                if (leadClosingDate != null)
                {
                    obj.Closing_Date = leadClosingDate;
                }
                if (leadCreationDate != null)
                {
                    obj.Lead_Created_Date = leadCreationDate;
                }
                (string CountryCode, string Country, string Region, string City, string Longitude, string Latitude, string ZipCode, string TimeZone) = await _iIPLiteService.getDetail(zohoObj.IPAddress);
                obj.GALead_ID = zohoObj.GALeadID;
                obj.Lead_Source = "Website";

                if (createdBy != 0 && createdBy != -1)
                {
                    string ownerName = await _customerService.GetCustomerFullNameAsync(await _customerService.GetCustomerByIdAsync(createdBy));
                    string zohoOwners = await _settingService.GetSettingByKeyAsync<string>("Zoho.Lead.Owners");
                    if (zohoOwners != null)
                    {
                        foreach (string ownerInfo in zohoOwners.Split(','))
                        {
                            string[] parts = ownerInfo.Split('|');
                            if (parts.Length == 2)
                            {
                                if (ownerName.Equals(parts[0], StringComparison.InvariantCultureIgnoreCase))
                                {
                                    dynamic owner = new ExpandoObject();
                                    owner.id = parts[1];
                                    obj.Owner = owner;
                                    break;
                                }

                            }
                        }
                    }



                }

                obj.Stage = string.Equals(zohoObj.LeadStatus, "Paid") ? "Closed Won" : "Invoice Sent";
                obj.Deal_Name = zohoObj.FullName;
                obj.Order_Number = orderNumber;
                obj.Order_Link = string.Equals(zohoObj.LeadStatus, "Paid") ? $"{_storeContext.GetCurrentStore().Url}Receipt?orderid={orderNumber}" : orderLink;
                obj.Amount = orderTotal;
                string firstName = removeSpecialCharacters(zohoObj.FirstName);
                string lastName = "";

                string[] arrName = firstName.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (arrName.Length > 1)
                {

                    firstName = string.Join(' ', arrName.Take(arrName.Length - 1));
                    lastName = arrName[arrName.Length - 1];
                }


                if (firstName != string.Empty)
                    obj.First_Name = firstName;
                if (lastName != string.Empty)
                    obj.Last_Name = lastName;
                else
                    obj.Last_Name = "-";
                if (!string.IsNullOrEmpty(zohoObj.Email))
                    obj.Potential_Email = zohoObj.Email;
                if (!string.IsNullOrEmpty(zohoObj.Phone))
                    obj.Phone_Number = removeSpecialCharacters(zohoObj.Phone);

                if (City != string.Empty)
                    obj.City = removeSpecialCharacters(City);
                if (Country != string.Empty)
                    obj.Country = removeSpecialCharacters(Country);

                if (Region != string.Empty)
                    obj.State = removeSpecialCharacters(Region);

                if (!string.IsNullOrEmpty(zohoObj.ZipCode))
                    obj.Zip_Code = removeSpecialCharacters(zohoObj.ZipCode);
                else if (Region != string.Empty)
                    obj.Zip_Code = removeSpecialCharacters(Region);

                if (!string.IsNullOrEmpty(zohoObj.Description))
                    obj.Description = removeSpecialCharacters(zohoObj.Description) + zohoObj.Attachments;
                if (!string.IsNullOrEmpty(zohoObj.Message))
                    obj.Message = removeSpecialCharacters(zohoObj.Message);
                if (!string.IsNullOrEmpty(zohoObj.SKU))
                    obj.SKU = removeSpecialCharacters(zohoObj.SKU);
                if (!string.IsNullOrEmpty(zohoObj.ProductName))
                    obj.Product_Name = removeSpecialCharacters(zohoObj.ProductName);
                if (!string.IsNullOrEmpty(zohoObj.IPAddress))
                    obj.IP_Address = zohoObj.IPAddress;


                if (zohoObj.customer != null)
                {
                    string gclid = await _genericAttributeService.GetAttributeAsync<string>(zohoObj.customer, CustomNopCustomerDefaults.GCLID);
                    if (!string.IsNullOrEmpty(gclid))
                        obj.gclid = gclid;

                    string campaign = await _genericAttributeService.GetAttributeAsync<string>(zohoObj.customer, CustomNopCustomerDefaults.Campaign);
                    if (!string.IsNullOrEmpty(campaign))
                        obj.Ad_Campaign_Name = campaign;
                }

                var campaignCookie = _httpContextAccessor?.HttpContext?.Request?.Cookies["nop.utm.utm_campaign"];
                var campaignIdCookie = _httpContextAccessor?.HttpContext?.Request?.Cookies["nop.utm.gad_campaignid"];
                var keywordCookie = _httpContextAccessor?.HttpContext?.Request?.Cookies["nop.utm.keyword"];

                if (campaignCookie != null)
                {
                    obj.Ad_Campaign_Name = campaignCookie;
                }
                else
                {
                    if (campaignIdCookie != null)
                        obj.Ad_Campaign_Name = campaignIdCookie;
                }

                if (keywordCookie != null)
                    obj.Keyword = keywordCookie;

                if (!string.IsNullOrEmpty(zohoObj.Address))
                    obj.Address_Of_Customer = zohoObj.Address;

                // object to json

                string request = "{\"data\":[" + Newtonsoft.Json.JsonConvert.SerializeObject(obj) + "	]}";
                request = request.Replace("\"gclid\"", "\"$gclid\"");
                var requestContent = new StringContent(request,
               Encoding.UTF8, MimeTypes.ApplicationJson);
                // end

                //oIPLite = null;
                if (!_httpClient.DefaultRequestHeaders.Contains("Authorization"))
                {
                    _httpClient.DefaultRequestHeaders.Add("Authorization", "Zoho-oauthtoken  " + _Token);
                }
                var response = !string.IsNullOrEmpty(zohoLeadId) ? await _httpClient.PutAsync("https://www.zohoapis.com/crm/v2/Potentials/" + zohoLeadId, requestContent) : await _httpClient.PostAsync("https://www.zohoapis.com/crm/v2/Potentials", requestContent);
                response.EnsureSuccessStatusCode();
                var content = await response.Content.ReadAsStringAsync();
                var jsonObject = JObject.Parse(content);
                var id = jsonObject["data"][0]["details"]["id"].ToString();

                return id;
            }
            catch (Exception ex)
            {
                await _logger.InsertLogAsync(LogLevel.Error, "Zoho: SaveLead", "Exception:   " + ex.Message,
                  await _workContext.GetCurrentCustomerAsync());

                return "";
            }
        }

        private async Task<string> SaveContact(ZohoDto zohoObj, string zohoLeadId, int orderNumber)
        {
            try
            {

                if (DateTime.Now.AddMinutes(10) >= _ExpiryDate)
                    await GetAuthenticationToken();
                dynamic obj = new ExpandoObject();

                (string CountryCode, string Country, string Region, string City, string Longitude, string Latitude, string ZipCode, string TimeZone) = await _iIPLiteService.getDetail(zohoObj.IPAddress);
                obj.GALead_ID = zohoObj.GALeadID;
                obj.Lead_Source = "Website";
                obj.Lead_Owner = _LeadOwner;
                obj.Stage = string.Equals(zohoObj.LeadStatus, "Paid") ? "Closed Won" : "Invoice Sent";
                obj.Deal_Name = zohoObj.FullName;
                obj.Order_Number = orderNumber;
                obj.Order_Num = orderNumber;
                string firstName = removeSpecialCharacters(zohoObj.FirstName);
                string lastName = "";

                string[] arrName = firstName.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (arrName.Length > 1)
                {

                    firstName = string.Join(' ', arrName.Take(arrName.Length - 1));
                    lastName = arrName[arrName.Length - 1];
                }


                if (firstName != string.Empty)
                    obj.First_Name = firstName;
                if (lastName != string.Empty)
                    obj.Last_Name = lastName;
                else
                    obj.Last_Name = "-";
                if (!string.IsNullOrEmpty(zohoObj.Email))
                    obj.Email = zohoObj.Email;
                if (!string.IsNullOrEmpty(zohoObj.Phone))
                    obj.Phone = removeSpecialCharacters(zohoObj.Phone);

                if (City != string.Empty)
                    obj.Other_City = removeSpecialCharacters(City);
                if (Country != string.Empty)
                    obj.Other_Country = removeSpecialCharacters(Country);

                if (Region != string.Empty)
                    obj.Other_State = removeSpecialCharacters(Region);

                if (!string.IsNullOrEmpty(zohoObj.ZipCode))
                    obj.Other_Zip = removeSpecialCharacters(zohoObj.ZipCode);
                else if (Region != string.Empty)
                    obj.Other_Zip = removeSpecialCharacters(Region);

                if (!string.IsNullOrEmpty(zohoObj.Description))
                    obj.Description = removeSpecialCharacters(zohoObj.Description) + zohoObj.Attachments;
                if (!string.IsNullOrEmpty(zohoObj.Message))
                    obj.Message = removeSpecialCharacters(zohoObj.Message);
                if (!string.IsNullOrEmpty(zohoObj.SKU))
                    obj.SKU = removeSpecialCharacters(zohoObj.SKU);
                if (!string.IsNullOrEmpty(zohoObj.ProductName))
                    obj.Product_Name = removeSpecialCharacters(zohoObj.ProductName);
                if (!string.IsNullOrEmpty(zohoObj.IPAddress))
                    obj.IP_Address = zohoObj.IPAddress;

                if (zohoObj.customer != null)
                {
                    string gclid = await _genericAttributeService.GetAttributeAsync<string>(zohoObj.customer, CustomNopCustomerDefaults.GCLID);
                    if (!string.IsNullOrEmpty(gclid))
                        obj.gclid = gclid;

                    string campaign = await _genericAttributeService.GetAttributeAsync<string>(zohoObj.customer, CustomNopCustomerDefaults.Campaign);
                    if (!string.IsNullOrEmpty(campaign))
                        obj.Ad_Campaign_Name = campaign;
                }


                var campaignCookie = _httpContextAccessor?.HttpContext?.Request?.Cookies["nop.utm.utm_campaign"];
                var campaignIdCookie = _httpContextAccessor?.HttpContext?.Request?.Cookies["nop.utm.gad_campaignid"];
                var keywordCookie = _httpContextAccessor?.HttpContext?.Request?.Cookies["nop.utm.keyword"];

                if (campaignCookie != null)
                {
                    obj.Ad_Campaign_Name = campaignCookie;
                }
                else
                {
                    if (campaignIdCookie != null)
                        obj.Ad_Campaign_Name = campaignIdCookie;
                }

                if (keywordCookie != null)
                    obj.Keyword = keywordCookie;

                if (!string.IsNullOrEmpty(zohoObj.Address))
                    obj.Address_Of_Customer = zohoObj.Address;

                // object to json

                string request = "{\"data\":[" + Newtonsoft.Json.JsonConvert.SerializeObject(obj) + "	]}";
                request = request.Replace("\"gclid\"", "\"$gclid\"");
                var requestContent = new StringContent(request,
               Encoding.UTF8, MimeTypes.ApplicationJson);
                // end

                //oIPLite = null;
                if (!_httpClient.DefaultRequestHeaders.Contains("Authorization"))
                {
                    _httpClient.DefaultRequestHeaders.Add("Authorization", "Zoho-oauthtoken  " + _Token);
                }
                var response = !string.IsNullOrEmpty(zohoLeadId) ? await _httpClient.PutAsync("https://www.zohoapis.com/crm/v2/Contacts/" + zohoLeadId, requestContent) : await _httpClient.PostAsync("https://www.zohoapis.com/crm/v2/Contacts", requestContent);
                response.EnsureSuccessStatusCode();
                var content = await response.Content.ReadAsStringAsync();
                var jsonObject = JObject.Parse(content);
                var id = jsonObject["data"][0]["details"]["id"].ToString();

                return id;
            }
            catch (Exception ex)
            {
                await _logger.InsertLogAsync(LogLevel.Error, "Zoho: SaveLead", "Exception:   " + ex.Message,
                  await _workContext.GetCurrentCustomerAsync());

                return "";
            }
        }

        #endregion
    }
}