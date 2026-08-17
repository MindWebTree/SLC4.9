using MWT.Nop.Core.Infrastructure;
using MWT.Nop.Core.Service.Catalog;
using MWT.Nop.Core.Service.Zoho;
using MWT.Nop.Core.Services.AbandonedCarts;
using MWT.Nop.Core.Services.Customers;
using MWT.Nop.Core.Services.MailChimp;
using MWT.Nop.Core.Services.Message;
using Nop.Core;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.Logging;
using Nop.Services.Messages;
using Nop.Services.Orders;
using Nop.Services.ScheduleTasks;

namespace MWT.Plugin.Misc.MwtStorefront.Tasks
{
    public partial class AbandonedCardIScheduleTask : IScheduleTask
    {

        #region Fields

        private readonly IAbandonedCartService _abandonedCardService;
        private readonly ICustomerExtendedService _customerService;
        private readonly ICustomWorkflowMessageService _workflowMessageService;
        private readonly IWorkContext _workContext;
        private readonly ISettingService _settingService;
        private readonly IMailchimpService _mailchimpService;
        private readonly IZohoService _zohoService;
        private readonly IAddressService _addressService;
        private readonly ILogger _logger;
        private readonly ICountryService _countryService;
        private readonly IStateProvinceService _stateProvinceService;
        private readonly IShoppingCartService _shoppingCartService;
        private readonly IStoreContext _storeContext;
        private readonly IProductExtendedService _productService;

        #endregion

        #region ctor

        public AbandonedCardIScheduleTask(IAbandonedCartService abandonedCardService,
            ICustomerExtendedService customerService, ICustomWorkflowMessageService workflowMessageService,
            IWorkContext workContext, ISettingService settingService,
            IMailchimpService mailchimpService, IZohoService zohoService,
            IAddressService addressService, ILogger logger,
            ICountryService countryService, IStateProvinceService stateProvinceService,
            IShoppingCartService shoppingCartService, IStoreContext storeContext,
            IProductExtendedService productService)
        {
            this._abandonedCardService = abandonedCardService;
            this._customerService = customerService;
            this._workflowMessageService = workflowMessageService;
            this._workContext = workContext;
            this._settingService = settingService;
            this._mailchimpService = mailchimpService;
            this._zohoService = zohoService;
            this._addressService = addressService;
            this._logger = logger;
            this._countryService = countryService;
            this._stateProvinceService = stateProvinceService;
            this._shoppingCartService = shoppingCartService;
            this._storeContext = storeContext;
            this._productService = productService;
        }

        #endregion

        public virtual async System.Threading.Tasks.Task ExecuteAsync()
        {
            int interval = await _settingService.GetSettingByKeyAsync<int>("AbandonedCard.CurrentTime.Gap");
            var items = await _abandonedCardService.SyncAbandonedCartItems();
            string fakeCustomerEmails = await _settingService.GetSettingByKeyAsync<string>("Common.FakeCustomer.Emails");
            foreach (var customerid in items.Select(a => a.CustomerID).Distinct())
            {

                var customer = await this._customerService.GetCustomerByIdAsync(customerid);
                string email = await this._customerService.GetCustomerEmail(customer);
                string name = await this._customerService.GetCustomerFullNameAsync(customer);
                if (customer != null && !string.IsNullOrEmpty(email) && !CustomCommonHelper.IsFakeCustomer(fakeCustomerEmails, email))
                {
                    string phone = await this._customerService.GetCustomerPhone(customer);
                    List<string> tags = new List<string>();
                    tags.Add("Abandoned Cart");
                    await _mailchimpService.CartOperation(email, await this._customerService.GetCustomerFullNameAsync(customer), "", tags, "", "", "", "");

                    await this._workflowMessageService.SendSupportAbandonedCartEmailMessage(customer, items.Where(i => i.CustomerID == customerid).Select(i => i.Id).ToArray(), (await this._workContext.GetWorkingLanguageAsync()).Id
                         );


                //    #region Zoho

                //    var cart = await _shoppingCartService.GetShoppingCartAsync(customer, ShoppingCartType.ShoppingCart,
                //_storeContext.GetCurrentStore().Id);
                //    cart = cart.Where(c => items.Where(i => i.CustomerID == customerid).Select(i => i.Id).ToArray().Contains(c.Id)).ToList();
                //    bool isProcessed = true;

                //    if (cart.Count > 0)
                //    {
                //        try
                //        {
                //            Zoho oZoho = new Zoho();
                //            StringBuilder sbZohoDescription = new StringBuilder();
                //            oZoho.FullName = name;
                //            oZoho.FirstName = name.Split(' ')[0];
                //            oZoho.LastName = name.Split(' ').Length > 1 ? string.Join(' ', name.Split(' ').Skip(1)) : "";
                //            oZoho.Email = email;
                //            Random random = new Random();
                //            oZoho.GALeadID = random.Next(1, 100000).ToString();
                //            oZoho.LeadSource = "Cart Abandonment";
                //            oZoho.ZipCode = "-";
                //            oZoho.Phone = phone;
                //            if (customer.ShippingAddressId.HasValue && customer.ShippingAddressId.Value > 0)
                //            {
                //                var address = await _addressService.GetAddressByIdAsync(Convert.ToInt32(customer.ShippingAddressId));
                //                oZoho.ZipCode = address.ZipPostalCode;
                //                oZoho.Address = address.Address1;
                //                oZoho.Country = (await _countryService.GetCountryByIdAsync(address.CountryId.Value))?.Name ?? "";
                //                oZoho.State = (await _countryService.GetCountryByIdAsync(address.StateProvinceId.Value))?.Name ?? "";
                //            }
                //            else if (customer.BillingAddressId.HasValue && customer.BillingAddressId.Value > 0)
                //            {
                //                var address = await _addressService.GetAddressByIdAsync(Convert.ToInt32(customer.ShippingAddressId));
                //                oZoho.ZipCode = address.ZipPostalCode;
                //                oZoho.Address = address.Address1;
                //                oZoho.Country = (await _countryService.GetCountryByIdAsync(address.CountryId.Value))?.Name ?? "";
                //                oZoho.State = (await _countryService.GetCountryByIdAsync(address.StateProvinceId.Value))?.Name ?? "";

                //            }

                //            sbZohoDescription.Append("\n----------------Customer Details--------------\n");
                //            sbZohoDescription.Append($"\n----------------Customer Name: {name}--------------\n");
                //            sbZohoDescription.Append($"\n----------------Email: {email}--------------\n");
                //            sbZohoDescription.Append($"\n----------------Phone Number: {oZoho.Phone}--------------\n");

                //            sbZohoDescription.Append("\n----------------Cart Details--------------\n");
                //            sbZohoDescription.Append("\n-------------------------------------------\n");
                //            decimal price=0;
                //            foreach (var item in cart)
                //            {

                //                var product = await _productService.GetProductByIdAsync(item.ProductId);
                //                if (string.IsNullOrEmpty(oZoho.SKU) || product.Price> price)
                //                {
                //                    oZoho.SKU = product.Sku;
                //                    price = product.Price;
                //                }
                //                if (product != null)
                //                {
                //                    sbZohoDescription.Append("\n----------------------Item Details ---------------------\n");
                //                    sbZohoDescription.Append($"\n---------Sku: {product.Sku}----------------------------------\n");
                //                    sbZohoDescription.Append($"\n---------Name: {product.Name}----------------------------------\n");
                //                    sbZohoDescription.Append($"\n---------Quantity: {item.Quantity}----------------------------------\n");
                //                    sbZohoDescription.Append($"\n---------Price: {product.Price}----------------------------------\n");
                //                    sbZohoDescription.Append("\n----------------------End ---------------------\n");
                //                    sbZohoDescription.Append("\n-------------------------------------------\n");
                //                }
                //            }
                //            oZoho.Description = Convert.ToString(sbZohoDescription);
                //            await _zohoService.SaveLead(oZoho);
                //        }
                //        catch (Exception ex)
                //        {
                //            await _logger.InsertLogAsync(Core.Domain.Logging.LogLevel.Error, "Zoho: Abandoned lead", "Exception:   " + ex.Message);
                //        }
                //    }

                //    #endregion
                }
            }

        }


    }
}
