using MWT.Nop.Core.Domain.AbandonedCarts;
using MWT.Nop.Core.Infrastructure;
using MWT.Nop.Core.Service.Zoho;
using MWT.Nop.Core.Services.AbandonedCarts;
using MWT.Nop.Core.Services.Customers;
using MWT.Nop.Core.Services.MailChimp;
using MWT.Nop.Core.Services.Zoho;
using Nop.Core;
using Nop.Core.Domain.Logging;
using Nop.Core.Domain.Orders;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.Logging;
using Nop.Services.Messages;
using Nop.Services.Orders;
using Nop.Services.ScheduleTasks;
using Nop.Web.Factories;
using System.Text;

namespace MWT.Plugin.Misc.MwtStorefront.Tasks
{
    public class Version3AbandonedCartTask : IScheduleTask
    {
        #region Fields

        private readonly IAbandonedCartService _abandonedCardService;
        private readonly ICustomerExtendedService _customerService;
        private readonly IWorkflowMessageService _workflowMessageService;
        private readonly IWorkContext _workContext;
        private readonly ISettingService _settingService;
        private readonly IMailchimpService _mailchimpService;
        private readonly IShoppingCartService _shoppingCartService;
        private readonly IStoreContext _storeContext;
        private readonly IOrderTotalCalculationService _orderTotalCalculationService;
        private readonly ICurrencyService _currencyService;
        private readonly IAddressService _addressService;
        private readonly ICountryService _countryService;
        private readonly IStateProvinceService _stateProvinceService;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly ILogger _logger;
        private readonly IProductService _productService;
        private readonly IProductModelFactory _productModelFactory;
        private bool enableLog = false;
        private int[] workingHours = { 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21 };
        private readonly IZohoService _zohoService;

        #endregion

        #region ctor

        public Version3AbandonedCartTask(IAbandonedCartService abandonedCardService,
            ICustomerExtendedService customerService, IWorkflowMessageService workflowMessageService,
            IWorkContext workContext, ISettingService settingService,
            IMailchimpService mailchimpService,
            IShoppingCartService shoppingCartService, IStoreContext storeContext,
            IOrderTotalCalculationService orderTotalCalculationService,
            ICurrencyService currencyService, IAddressService addressService,
            ICountryService countryService, IStateProvinceService stateProvinceService,
             IGenericAttributeService genericAttributeService, ILogger logger, IProductService productService,
             IProductModelFactory productModelFactory, IZohoService zohoService)
        {
            this._abandonedCardService = abandonedCardService;
            this._customerService = customerService;
            this._workflowMessageService = workflowMessageService;
            this._workContext = workContext;
            this._settingService = settingService;
            this._mailchimpService = mailchimpService;
            this._shoppingCartService = shoppingCartService;
            this._storeContext = storeContext;
            this._orderTotalCalculationService = orderTotalCalculationService;
            this._currencyService = currencyService;
            this._addressService = addressService;
            this._countryService = countryService;
            this._stateProvinceService = stateProvinceService;
            this._genericAttributeService = genericAttributeService;
            this._logger = logger;
            this._productService = productService;
            this._productModelFactory = productModelFactory;
            this._zohoService = zohoService;
        }

        #endregion

        public virtual async System.Threading.Tasks.Task ExecuteAsync()
        {
            enableLog = await _settingService.GetSettingByKeyAsync<bool>("AbandonedCard.Offer.Task.Enable.Log");
            bool isSandboxEnviromentEnabled = await _settingService.GetSettingByKeyAsync<bool>("AbandonedCard.Offer.Task.Enable.SandboxEnviroment");
            #region sync Abandonedcards

            int interval = await _settingService.GetSettingByKeyAsync<int>("AbandonedCard.CurrentTime.Gap");
            await _abandonedCardService.SyncAbandonedCarts(DateTime.UtcNow.AddMinutes(-interval));

            #endregion

            #region Abandoned Reminder Schedules

            var schedules = await _abandonedCardService.GetAbandonedCartReminderSchedules();


            #endregion

            #region AbandonedReminders

            var abandonedReminders = await _abandonedCardService.GetAbandonedReminders();

            await WriteLog(LogLevel.Information, "V3 Abanoned Cart Task", $"No Of Reminders found {abandonedReminders.Count}");

            #endregion

            foreach (var reminder in abandonedReminders)
            {
                #region ProcessInvalid Records

                var schedule = schedules.Where(s => s.Number == reminder.ReminderNumber + 1).FirstOrDefault();
                if (schedule == null)
                {
                    await _abandonedCardService.InsertAbandonedReminderHistory(new AbandonedReminderHistory()
                    {
                        AbandonedCartInvoiceID = reminder.Id,
                        ReminderNumber = reminder.ReminderNumber,
                        Isdeleted = false,
                        ReminderDate = reminder.LastProcessingDate
                    }, false
                    );
                    await WriteLog(LogLevel.Information, "V3 Abanoned Cart Task", $"Hard marked reminder as completed for record {Newtonsoft.Json.JsonConvert.SerializeObject(reminder)}");
                }
                #endregion

                var nextReminderDate = new DateTime();
                if (!isSandboxEnviromentEnabled)
                {
                    nextReminderDate = reminder.LastProcessingDate.AddHours(Convert.ToDouble(schedule.Hours));
                }
                else
                {
                    nextReminderDate = reminder.LastProcessingDate.AddMinutes(Convert.ToDouble(schedule.Hours));
                }

                await WriteLog(LogLevel.Information, "V3 Abanoned Cart Task", $"nextReminderDate  {nextReminderDate} {reminder.Id} {reminder.ReminderNumber}");
                if (nextReminderDate <= DateTime.Now && (isSandboxEnviromentEnabled || workingHours.Contains(DateTime.Now.Hour)))
                {
                    var customer = await this._customerService.GetCustomerByIdAsync(reminder.CustomerId);
                    if (customer != null)
                    {
                        string email = await this._customerService.GetCustomerEmail(customer);
                        string phone = await this._customerService.GetCustomerPhone(customer);
                        string name = (await this._customerService.GetCustomerFullNameAsync(customer)) ?? "";
                        if (!string.IsNullOrEmpty(email))
                        {

                            string fakeCustomerEmails = await _settingService.GetSettingByKeyAsync<string>("Common.FakeCustomer.Emails");
                            if (CustomCommonHelper.IsFakeCustomer(fakeCustomerEmails, email))
                            {
                                await _abandonedCardService.InsertAbandonedReminderHistory(new AbandonedReminderHistory()
                                {
                                    AbandonedCartInvoiceID = reminder.Id,
                                    ReminderNumber = reminder.ReminderNumber,
                                    Isdeleted = false,
                                    ReminderDate = reminder.LastProcessingDate
                                }, false
                                );
                                await WriteLog(LogLevel.Information, "V3 Abanoned Cart Task", $"Hard marked reminder as completed for record {Newtonsoft.Json.JsonConvert.SerializeObject(reminder)}, Fake Customer");
                                continue;
                            }
                            var cart = await _shoppingCartService.GetShoppingCartAsync(customer, ShoppingCartType.ShoppingCart, (await _storeContext.GetCurrentStoreAsync()).Id);
                            if (cart.Count > 0)
                            {
                                reminder.ReminderNumber = reminder.ReminderNumber + 1;
                                await WriteLog(LogLevel.Information, "V3 Abanoned Cart Task", $"Going to send reminder {reminder.ReminderNumber} for customer {email}");
                                await _abandonedCardService.SendAbandonedCartReminder(reminder, customer, name, email, phone, schedule.MessageTemplateId, cart, schedule.UtmSource);

                                #region Zoho
                                try
                                {
                                    ZohoDto oZoho = new ZohoDto();
                                    StringBuilder sbZohoDescription = new StringBuilder();
                                    oZoho.FullName = name;
                                    oZoho.FirstName = name.Split(' ')[0];
                                    oZoho.LastName = name.Split(' ').Length > 1 ? string.Join(' ', name.Split(' ').Skip(1)) : "";
                                    oZoho.Email = email;
                                    Random random = new Random();
                                    oZoho.GALeadID = random.Next(1, 100000).ToString();
                                    oZoho.LeadSource = "Cart Abandonment";
                                    oZoho.ZipCode = "-";
                                    oZoho.Phone = phone;
                                    if (customer.ShippingAddressId.HasValue && customer.ShippingAddressId.Value > 0)
                                    {
                                        var address = await _addressService.GetAddressByIdAsync(Convert.ToInt32(customer.ShippingAddressId));
                                        oZoho.ZipCode = address.ZipPostalCode;
                                        oZoho.Address = address.Address1;
                                        oZoho.Country = (await _countryService.GetCountryByIdAsync(address.CountryId.Value))?.Name ?? "";
                                        oZoho.State = (await _countryService.GetCountryByIdAsync(address.StateProvinceId.Value))?.Name ?? "";
                                    }
                                    else if (customer.BillingAddressId.HasValue && customer.BillingAddressId.Value > 0)
                                    {
                                        var address = await _addressService.GetAddressByIdAsync(Convert.ToInt32(customer.ShippingAddressId));
                                        oZoho.ZipCode = address.ZipPostalCode;
                                        oZoho.Address = address.Address1;
                                        oZoho.Country = (await _countryService.GetCountryByIdAsync(address.CountryId.Value))?.Name ?? "";
                                        oZoho.State = (await _countryService.GetCountryByIdAsync(address.StateProvinceId.Value))?.Name ?? "";

                                    }

                                    sbZohoDescription.Append("\n----------------Customer Details--------------\n");
                                    sbZohoDescription.Append($"\n----------------Customer Name: {name}--------------\n");
                                    sbZohoDescription.Append($"\n----------------Email: {email}--------------\n");
                                    sbZohoDescription.Append($"\n----------------Phone Number: {oZoho.Phone}--------------\n");

                                    sbZohoDescription.Append("\n----------------Cart Details--------------\n");
                                    sbZohoDescription.Append("\n-------------------------------------------\n");
                                    decimal price = 0;
                                    foreach (var item in cart)
                                    {

                                        var product = await _productService.GetProductByIdAsync(item.ProductId);
                                        if (string.IsNullOrEmpty(oZoho.SKU) || product.Price > price)
                                        {
                                            oZoho.SKU = product.Sku;
                                            price = product.Price;
                                        }
                                        if (product != null)
                                        {
                                            sbZohoDescription.Append("\n----------------------Item Details ---------------------\n");
                                            sbZohoDescription.Append($"\n---------Sku: {product.Sku}----------------------------------\n");
                                            sbZohoDescription.Append($"\n---------Name: {product.Name}----------------------------------\n");
                                            sbZohoDescription.Append($"\n---------Quantity: {item.Quantity}----------------------------------\n");
                                            sbZohoDescription.Append($"\n---------Price: {product.Price}----------------------------------\n");
                                            sbZohoDescription.Append("\n----------------------End ---------------------\n");
                                            sbZohoDescription.Append("\n-------------------------------------------\n");
                                        }
                                    }


                                    var (orderSubTotalDiscountAmountBase, _, subTotal, _, _) = await _orderTotalCalculationService.GetShoppingCartSubTotalAsync(cart, true);
                                    oZoho.Total = subTotal;
                                    oZoho.Description = Convert.ToString(sbZohoDescription);
                                    oZoho.customer = customer;
                                    await _zohoService.SaveLead(oZoho);
                                }
                                catch (Exception ex)
                                {
                                    await _logger.InsertLogAsync(LogLevel.Error, "Zoho: Abandoned lead", "Exception:   " + ex.Message);
                                }


                                #endregion
                            }
                        }

                    }
                }

            }
        }


        public async System.Threading.Tasks.Task WriteLog(LogLevel logLevel, string shortMessage, string longMessage)
        {
            if (enableLog)
            {
                await _logger.InsertLogAsync(logLevel, shortMessage, longMessage);
            }
        }
    }
}
