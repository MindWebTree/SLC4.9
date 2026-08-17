using Microsoft.AspNetCore.Http;
using MWT.Nop.Core.Domain.CustomOrders;
using MWT.Nop.Core.Domain.Messages;
using MWT.Nop.Core.Services.Customers;
using MWT.Nop.Core.Services.MailChimp;
using MWT.Nop.Core.Services.Mandrill;
using MWT.Nop.Core.Services.Orders;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Logging;
using Nop.Core.Domain.Messages;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Stores;
using Nop.Core.Domain.Vendors;
using Nop.Core.Events;
using Nop.Core.Infrastructure;
using Nop.Services.Affiliates;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Customers;
using Nop.Services.Customizations.CustomOrders;
using Nop.Services.Customizations.IpAddress;
using Nop.Services.Events;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Messages;
using Nop.Services.Orders;
using Nop.Services.Payments;
using Nop.Services.Stores;
using System.Data;
using System.Net;

namespace MWT.Nop.Core.Services.Message
{

    public partial class CustomWorkflowMessageService : WorkflowMessageService, ICustomWorkflowMessageService
    {
        #region Fields

        private readonly ICustomMessageTokenProvider _customMessageTokenProvider;
        private readonly ICustomerExtendedService _customCustomerService;
        private readonly ILogger _logger;
        private readonly IMailchimpService _mailchimpService;
        private readonly IMandrillService _mandrillService;
        private readonly ISettingService _settingService;
        private readonly IShoppingCartService _shoppingCartService;
        private readonly ICustomOrderService _customOrderService;
        #endregion

        public CustomWorkflowMessageService(CommonSettings commonSettings, EmailAccountSettings emailAccountSettings,
            IAddressService addressService, IAffiliateService affiliateService, ICustomerService customerService,
            IEmailAccountService emailAccountService, IEventPublisher eventPublisher, ILanguageService languageService,
            ILocalizationService localizationService, IMessageTemplateService messageTemplateService,
            IMessageTokenProvider messageTokenProvider, IOrderService orderService, IProductService productService,
            IQueuedEmailService queuedEmailService, IStoreContext storeContext, IStoreService storeService, ITokenizer tokenizer,
            MessagesSettings messagesSettings, ICustomMessageTokenProvider customMessageTokenProvider,
            ICustomerExtendedService customCustomerService, ILogger logger,
            IMailchimpService mailchimpService, IMandrillService mandrillService, ISettingService settingService, IShoppingCartService shoppingCartService,
            ICustomOrderService customOrderService) : base(commonSettings, emailAccountSettings, addressService, affiliateService, customerService, emailAccountService, eventPublisher, languageService, localizationService, messageTemplateService, messageTokenProvider, orderService, productService, queuedEmailService, storeContext, storeService, tokenizer, messagesSettings)
        {
            _customMessageTokenProvider = customMessageTokenProvider;
            _customCustomerService = customCustomerService;
            _logger = logger;
            _mailchimpService = mailchimpService;
            _mandrillService = mandrillService;
            _settingService = settingService;
            _shoppingCartService = shoppingCartService;
            _customOrderService = customOrderService;
        }

        #region Methods

        #region Receipt
        public virtual async Task<string> OrderReceiptContentAsync(Order order, int languageId)
        {
            if (order == null)
                throw new ArgumentNullException(nameof(order));

            var store = await _storeService.GetStoreByIdAsync(order.StoreId) ?? await _storeContext.GetCurrentStoreAsync();
            languageId = await EnsureLanguageIsActiveAsync(languageId, store.Id);

            var messageTemplates = await GetActiveMessageTemplatesAsync(MessageTemplateExtendedSystemNames.CustomOrderPlacedReceipt, store.Id);
            if (!messageTemplates.Any())
                return "";

            //tokens
            var commonTokens = new List<Token>();
            await _customMessageTokenProvider.CustomAddOrderTokensAsync(commonTokens, order, languageId);
            await _customMessageTokenProvider.CustomAddCustomerTokensAsync(commonTokens, order.CustomerId);


            //email account
            var emailAccount = await GetEmailAccountOfMessageTemplateAsync(messageTemplates.FirstOrDefault(), languageId);

            var tokens = new List<Token>(commonTokens);
            await _customMessageTokenProvider.AddStoreTokensAsync(tokens, store, emailAccount,languageId);
            await _customMessageTokenProvider.AddStoreLogoToken(tokens);

            //event notification
            var body = await _localizationService.GetLocalizedAsync(messageTemplates.FirstOrDefault(), mt => mt.Body, languageId);
            var bodyReplaced = _tokenizer.Replace(body, tokens, true);
            return bodyReplaced;


        }

        #endregion

        #region Order Decline

        public async Task<List<int>> SendOrderDeclineMessage(ProcessPaymentRequest paymentRequest, IFormCollection form, Customer customer, int languageId, string errorMessage, int orderId = 0)
        {

            var _declinedOrderLogService = EngineContext.Current.Resolve<IDeclinedOrderLogService>();
            await _declinedOrderLogService.Insert(errorMessage);

            await this.SendCustomerOrderDeclineMessage(customer, languageId, orderId);
            var store = _storeContext.GetCurrentStore();
            var messageTemplates = await GetActiveMessageTemplatesAsync(MessageTemplateExtendedSystemNames.CustomOrderDeclineNotification, store.Id);
            var cart = await _shoppingCartService.GetShoppingCartAsync(customer, ShoppingCartType.ShoppingCart,
                          store.Id);


            if (!messageTemplates.Any() || customer == null || (cart.Count == 0 && orderId == 0))
                return new List<int>();

            var commonTokens = new List<Token>();
            await _customMessageTokenProvider.CustomAddOrderDeclineTokensAsync(commonTokens, paymentRequest, form, customer, cart, errorMessage, orderId, languageId);


            return await messageTemplates.SelectAwait(async messageTemplate =>
            {
                //email account
                var emailAccount = await GetEmailAccountOfMessageTemplateAsync(messageTemplate, languageId);

                var tokens = new List<Token>(commonTokens);
                await _customMessageTokenProvider.AddStoreTokensAsync(tokens, store, emailAccount, languageId);

                await _customMessageTokenProvider.AddStoreLogoToken(tokens);

                //event notification
                await _eventPublisher.MessageTokensAddedAsync(messageTemplate, tokens);

                var toEmail = emailAccount.Email;
                var toName = emailAccount.DisplayName;

                return await SendNotificationAsync(messageTemplate, emailAccount, languageId, tokens, toEmail, toName);
            }).ToListAsync();

        }

        public async Task<List<int>> SendCustomerOrderDeclineMessage(Customer customer, int languageId, int orderId)
        {


            var store = _storeContext.GetCurrentStore();
            var messageTemplates = await GetActiveMessageTemplatesAsync(MessageTemplateExtendedSystemNames.CustomCustomerOrderDeclineNotification, store.Id);
            var cart = await _shoppingCartService.GetShoppingCartAsync(customer, ShoppingCartType.ShoppingCart,
                          store.Id);
            if (!messageTemplates.Any() || customer == null || (cart.Count == 0 && orderId == 0))
                return new List<int>();

            var commonTokens = new List<Token>();
            await _customMessageTokenProvider.CustomAddCustomerOrderDeclineTokensAsync(commonTokens, customer, cart, orderId, languageId);

            var address = await _addressService.GetAddressByIdAsync(customer.BillingAddressId ?? customer.ShippingAddressId ?? 0);
            string toEmail = string.Empty;
            string toName = string.Empty;
            if (address != null && address.Id > 0)
            {
                toEmail = address.Email;
                toName = $"{address.FirstName} {address.LastName}";
            }
            else
            {
                toEmail = await _customCustomerService.GetCustomerEmail(customer);
                toName = await _customerService.GetCustomerFullNameAsync(customer);

            }
            return await messageTemplates.SelectAwait(async messageTemplate =>
            {
                //email account
                var emailAccount = await GetEmailAccountOfMessageTemplateAsync(messageTemplate, languageId);

                var tokens = new List<Token>(commonTokens);
                await _customMessageTokenProvider.AddStoreTokensAsync(tokens, store, emailAccount,languageId);

                await _customMessageTokenProvider.AddStoreLogoToken(tokens);

                //event notification
                await _eventPublisher.MessageTokensAddedAsync(messageTemplate, tokens);



                return await SendNotificationAsync(messageTemplate, emailAccount, languageId, tokens, toEmail, toName);
            }).ToListAsync();

        }
        #endregion
        public virtual async Task<IList<int>> CustomSendOrderPlacedStoreOwnerNotificationAsync(Order order, int languageId)
        {
            if (order == null)
                throw new ArgumentNullException(nameof(order));

            var store = await _storeService.GetStoreByIdAsync(order.StoreId) ?? await _storeContext.GetCurrentStoreAsync();
            languageId = await EnsureLanguageIsActiveAsync(languageId, store.Id);

            var messageTemplates = await GetActiveMessageTemplatesAsync(MessageTemplateExtendedSystemNames.CustomOrderPlacedStoreOwnerNotification, store.Id);
            if (!messageTemplates.Any())
                return new List<int>();

            //tokens
            var commonTokens = new List<Token>();
            await _customMessageTokenProvider.CustomAddOrderTokensAsync(commonTokens, order, languageId);
            await _customMessageTokenProvider.CustomAddCustomerTokensAsync(commonTokens, order.CustomerId);

            return await messageTemplates.SelectAwait(async messageTemplate =>
            {
                //email account
                var emailAccount = await GetEmailAccountOfMessageTemplateAsync(messageTemplate, languageId);

                var tokens = new List<Token>(commonTokens);
                await _customMessageTokenProvider.AddStoreTokensAsync(tokens, store, emailAccount, languageId);

                await _customMessageTokenProvider.AddStoreLogoToken(tokens);

                //event notification
                await _eventPublisher.MessageTokensAddedAsync(messageTemplate, tokens);

                var toEmail = emailAccount.Email;
                var toName = emailAccount.DisplayName;

                return await SendNotificationAsync(messageTemplate, emailAccount, languageId, tokens, toEmail, toName);
            }).ToListAsync();
        }


        public virtual async Task<IList<int>> CustomSendOrderPlacedCustomerNotificationAsync(Order order, int languageId,
    string attachmentFilePath = null, string attachmentFileName = null, bool customerOnly = false)
        {
            if (order == null)
                throw new ArgumentNullException(nameof(order));

            var store = await _storeService.GetStoreByIdAsync(order.StoreId) ?? await _storeContext.GetCurrentStoreAsync();
            languageId = await EnsureLanguageIsActiveAsync(languageId, store.Id);

            var messageTemplates = await GetActiveMessageTemplatesAsync(MessageTemplateExtendedSystemNames.CustomOrderPlacedCustomerNotification, store.Id);
            if (!messageTemplates.Any())
                return new List<int>();

            //tokens
            var commonTokens = new List<Token>();
            await _customMessageTokenProvider.CustomAddOrderTokensAsync(commonTokens, order, languageId);
            await _customMessageTokenProvider.CustomAddCustomerTokensAsync(commonTokens, order.CustomerId);

            List<int> orderPlacedStoreOwnerNotificationQueuedEmailIds = await messageTemplates.SelectAwait(async messageTemplate =>
            {
                //email account
                var emailAccount = await GetEmailAccountOfMessageTemplateAsync(messageTemplate, languageId);
                if (customerOnly)
                {
                    messageTemplate.BccEmailAddresses = null;
                }
                var tokens = new List<Token>(commonTokens);
                await _customMessageTokenProvider.AddStoreTokensAsync(tokens, store, emailAccount,languageId);
                await _customMessageTokenProvider.AddStoreLogoToken(tokens);

                //event notification
                await _eventPublisher.MessageTokensAddedAsync(messageTemplate, tokens);

                var billingAddress = await _addressService.GetAddressByIdAsync(order.BillingAddressId);

                var toEmail = billingAddress.Email;
                var toName = $"{billingAddress.FirstName} {billingAddress.LastName}";

                return await SendNotificationAsync(messageTemplate, emailAccount, languageId, tokens, toEmail, toName,
                    attachmentFilePath, attachmentFileName);
            }).ToListAsync();


            try
            {
                if (orderPlacedStoreOwnerNotificationQueuedEmailIds.Any())
                {

                    await _orderService.InsertOrderNoteAsync(new OrderNote
                    {
                        OrderId = order.Id,
                        Note = $"\"Order placed\" email (to {(customerOnly ? "customer" : "store owner")}) has been queued. Queued email identifiers: {string.Join(", ", orderPlacedStoreOwnerNotificationQueuedEmailIds)}.",
                        DisplayToCustomer = false,
                        CreatedOnUtc = DateTime.UtcNow
                    });

                }
            }
            catch { }
            {

            }





            return orderPlacedStoreOwnerNotificationQueuedEmailIds;
        }

        public virtual async Task<IList<int>> CustomSendOrderPlacedVendorNotificationAsync(Order order, Vendor vendor, int languageId)
        {
            if (order == null)
                throw new ArgumentNullException(nameof(order));

            if (vendor == null)
                throw new ArgumentNullException(nameof(vendor));

            var store = await _storeService.GetStoreByIdAsync(order.StoreId) ?? await _storeContext.GetCurrentStoreAsync();
            languageId = await EnsureLanguageIsActiveAsync(languageId, store.Id);

            var messageTemplates = await GetActiveMessageTemplatesAsync(MessageTemplateExtendedSystemNames.CustomOrderPlacedVendorNotification, store.Id);
            if (!messageTemplates.Any())
                return new List<int>();

            //tokens
            var commonTokens = new List<Token>();
            await _customMessageTokenProvider.CustomAddOrderTokensAsync(commonTokens, order, languageId, vendor.Id);
            await _customMessageTokenProvider.CustomAddCustomerTokensAsync(commonTokens, order.CustomerId);

            return await messageTemplates.SelectAwait(async messageTemplate =>
            {
                //email account
                var emailAccount = await GetEmailAccountOfMessageTemplateAsync(messageTemplate, languageId);

                var tokens = new List<Token>(commonTokens);
                await _customMessageTokenProvider.AddStoreTokensAsync(tokens, store, emailAccount, languageId);
                await _customMessageTokenProvider.AddStoreLogoToken(tokens);

                //event notification
                await _eventPublisher.MessageTokensAddedAsync(messageTemplate, tokens);

                var toEmail = vendor.Email;
                var toName = vendor.Name;

                return await SendNotificationAsync(messageTemplate, emailAccount, languageId, tokens, toEmail, toName);
            }).ToListAsync();
        }

        public virtual async Task<IList<int>> CustomSendOrderPlacedAffiliateNotificationAsync(Order order, int languageId)
        {
            if (order == null)
                throw new ArgumentNullException(nameof(order));

            var affiliate = await _affiliateService.GetAffiliateByIdAsync(order.AffiliateId);

            if (affiliate == null)
                throw new ArgumentNullException(nameof(affiliate));

            var store = await _storeService.GetStoreByIdAsync(order.StoreId) ?? await _storeContext.GetCurrentStoreAsync();
            languageId = await EnsureLanguageIsActiveAsync(languageId, store.Id);

            var messageTemplates = await GetActiveMessageTemplatesAsync(MessageTemplateExtendedSystemNames.CustomOrderPlacedAffiliateNotification, store.Id);
            if (!messageTemplates.Any())
                return new List<int>();

            //tokens
            var commonTokens = new List<Token>();
            await _customMessageTokenProvider.CustomAddOrderTokensAsync(commonTokens, order, languageId);
            await _customMessageTokenProvider.CustomAddCustomerTokensAsync(commonTokens, order.CustomerId);

            return await messageTemplates.SelectAwait(async messageTemplate =>
            {
                //email account
                var emailAccount = await GetEmailAccountOfMessageTemplateAsync(messageTemplate, languageId);

                var tokens = new List<Token>(commonTokens);
                await _customMessageTokenProvider.AddStoreTokensAsync(tokens, store, emailAccount, languageId);
                await _customMessageTokenProvider.AddStoreLogoToken(tokens);

                //event notification
                await _eventPublisher.MessageTokensAddedAsync(messageTemplate, tokens);

                var affiliateAddress = await _addressService.GetAddressByIdAsync(affiliate.AddressId);
                var toEmail = affiliateAddress.Email;
                var toName = $"{affiliateAddress.FirstName} {affiliateAddress.LastName}";

                return await SendNotificationAsync(messageTemplate, emailAccount, languageId, tokens, toEmail, toName);
            }).ToListAsync();
        }

        public virtual async Task<IList<int>> SendCustomizationFormMessageAsync(int languageId, string senderEmail,
        string senderName, string subject, string body)
        {
            var store = await _storeContext.GetCurrentStoreAsync();
            languageId = await EnsureLanguageIsActiveAsync(languageId, store.Id);

            var messageTemplates = await GetActiveMessageTemplatesAsync(MessageTemplateExtendedSystemNames.CustomizationFormLeads, store.Id);
            if (!messageTemplates.Any())
                return new List<int>();

            //tokens
            var commonTokens = new List<Token>
            {
                new Token("ContactUs.SenderEmail", senderEmail),
                new Token("ContactUs.SenderName", senderName),
                new Token("Subject", subject),
                new Token("CustomizationFormLeads.Body", body, true)
            };

            return await messageTemplates.SelectAwait(async messageTemplate =>
            {
                //email account
                var emailAccount = await GetEmailAccountOfMessageTemplateAsync(messageTemplate, languageId);

                var tokens = new List<Token>(commonTokens);
                await _customMessageTokenProvider.AddStoreTokensAsync(tokens, store, emailAccount, languageId);
                await _customMessageTokenProvider.AddStoreLogoToken(tokens);

                string fromEmail;
                string fromName;
                //required for some SMTP servers
                if (_commonSettings.UseSystemEmailForContactUsForm)
                {
                    fromEmail = emailAccount.Email;
                    fromName = emailAccount.DisplayName;
                    body = $"<strong>From</strong>: {WebUtility.HtmlEncode(senderName)} - {WebUtility.HtmlEncode(senderEmail)}<br /><br />{body}";
                }
                else
                {
                    fromEmail = senderEmail;
                    fromName = senderName;
                }

                //event notification
                await _eventPublisher.MessageTokensAddedAsync(messageTemplate, tokens);
                var _settingService = EngineContext.Current.Resolve<ISettingService>();
                string replyToEmail = await _settingService.GetSettingByKeyAsync<string>("store.email");
                var toEmail = emailAccount.Email;
                var toName = emailAccount.DisplayName;

                return await SendNotificationAsync(messageTemplate, emailAccount, languageId, tokens, senderEmail, senderName,
                    fromEmail: fromEmail,
                    fromName: fromName,
                    subject: subject, replyToEmailAddress: replyToEmail);
            }).ToListAsync();
        }


        public async Task<IList<int>> SendCustomFormMessageAsync(int languageId, string customerEmailAddress, string customerName, string bccaddress, string subject, string body)
        {
            var store = await _storeContext.GetCurrentStoreAsync();
            languageId = await EnsureLanguageIsActiveAsync(languageId, store.Id);

            //tokens
            var commonTokens = new List<Token>
            {  new Token("Subject", subject),
                new Token("CustomizationFormLeads.Body", body, true)
            };


            //email account
            var emailAccount = (await _emailAccountService.GetAllEmailAccountsAsync()).FirstOrDefault();

            string fromEmail;
            string fromName;
            //required for some SMTP servers

            fromEmail = emailAccount.Email;
            fromName = emailAccount.DisplayName;

            var toEmail = emailAccount.Email;
            var toName = emailAccount.DisplayName;

            var messageTemplate = new MessageTemplate()
            {
                AttachedDownloadId = 0,
                BccEmailAddresses = bccaddress,
                Body = body,
                DelayPeriod = MessageDelayPeriod.Hours,
                DelayPeriodId = 0,
                EmailAccountId = emailAccount.Id,
                IsActive = true,
                Name = "custmform",
                Subject = subject
            };
            string senderEmail = "";
            string senderName = "";
            if (!string.IsNullOrEmpty(customerEmailAddress))
            {
                toEmail = customerEmailAddress;
                toName = customerName;
                messageTemplate.BccEmailAddresses = "";
                var _settingService = EngineContext.Current.Resolve<ISettingService>();
                senderName = await _settingService.GetSettingByKeyAsync<string>("store.email");
            }

            await SendNotificationAsync(messageTemplate, emailAccount, languageId, new List<Token>(), toEmail, toName,
               fromEmail: fromEmail,
               fromName: fromName,
               subject: subject,
           replyToEmailAddress: senderEmail,
               replyToName: senderName);

            return new List<int>();
        }

        #region CustomOrder

        #region Receipts
        public async Task<string> CustomOrderInvoiceContentNotificationAsync(CustomOrder order, int languageId)
        {
            if (order == null)
                throw new ArgumentNullException(nameof(order));


            var store = (await _storeService.GetAllStoresAsync()).OrderBy(s => s.DisplayOrder).FirstOrDefault();

            languageId = await EnsureLanguageIsActiveAsync(languageId, store.Id);

            var messageTemplates = await GetActiveMessageTemplatesAsync(MessageTemplateExtendedSystemNames.CustomOrder_InvoiceReceipt, store.Id);
            if (!messageTemplates.Any())
                return "";

            //tokens
            var commonTokens = new List<Token>();
            await _customMessageTokenProvider.CustomOrderAddTokensAsync(commonTokens, order, languageId);
            await _customMessageTokenProvider.CustomAddCustomerTokensAsync(commonTokens, order.CustomerId == null ? 0 : Convert.ToInt32(order.CustomerId));
       

            //email account

            var emailAccount = await GetEmailAccountOfMessageTemplateAsync(messageTemplates.FirstOrDefault(), languageId);
            var tokens = new List<Token>(commonTokens);
            await _customMessageTokenProvider.AddStoreTokensAsync(tokens, store, emailAccount   , languageId);
            await _customMessageTokenProvider.AddStoreLogoToken(tokens);


            var body = await _localizationService.GetLocalizedAsync(messageTemplates.FirstOrDefault(), mt => mt.Body, languageId);
            var bodyReplaced = _tokenizer.Replace(body, tokens, true);
            return bodyReplaced;
        }

        public async Task<string> CustomOrderReceiptContentAsync(CustomOrder order, int languageId)
        {
            if (order == null)
                throw new ArgumentNullException(nameof(order));


            var store = (await _storeService.GetAllStoresAsync()).OrderBy(s => s.DisplayOrder).FirstOrDefault();

            languageId = await EnsureLanguageIsActiveAsync(languageId, store.Id);

            var messageTemplates = await GetActiveMessageTemplatesAsync(order.ParentOrderID > 0 ? MessageTemplateExtendedSystemNames.CustomOrder_WGS_Receipt : MessageTemplateExtendedSystemNames.CustomOrder_Receipt, store.Id);
            if (!messageTemplates.Any())
                return "";

            //tokens
            var commonTokens = new List<Token>();
            await _customMessageTokenProvider.CustomOrderAddTokensAsync(commonTokens, order, languageId);
            await _customMessageTokenProvider.CustomAddCustomerTokensAsync(commonTokens, order.CustomerId == null ? 0 : Convert.ToInt32(order.CustomerId));
           

            //email account
            var emailAccount = await GetEmailAccountOfMessageTemplateAsync(messageTemplates.FirstOrDefault(), languageId);

            var tokens = new List<Token>(commonTokens);
            await _customMessageTokenProvider.AddStoreTokensAsync(tokens, store, emailAccount, languageId);
            await _customMessageTokenProvider.AddStoreLogoToken(tokens);
            var body = await _localizationService.GetLocalizedAsync(messageTemplates.FirstOrDefault(), mt => mt.Body, languageId);
            var bodyReplaced = _tokenizer.Replace(body, tokens, true);
            return bodyReplaced;
        }

        #endregion

        public async Task<IList<int>> CustomOrder_SendPaymentLinkNotificationAsync(CustomOrder order, int languageId, string email = "")
        {
            if (order == null)
                throw new ArgumentNullException(nameof(order));

            string replyToEmail = null;
            var store = (await _storeService.GetAllStoresAsync()).OrderBy(s => s.DisplayOrder).FirstOrDefault();

            languageId = await EnsureLanguageIsActiveAsync(languageId, store.Id);
       
            var template = MessageTemplateExtendedSystemNames.CustomOrder_InvoiceNotification;
            var commonTokens = new List<Token>();
            //#region Additional Service

            if (order.ParentOrderID > 0)
            {
                var items = await _customOrderService.GetOrderItems(order.Id);
                if (items.Count == 1)
                {
                    var product = await _productService.GetProductByIdAsync(items.First().ProductId);
                    if (product.Name.StartsWith("wgs", StringComparison.InvariantCultureIgnoreCase))
                    {
                        var _settingService = EngineContext.Current.Resolve<ISettingService>();
                        template = MessageTemplateExtendedSystemNames.CustomOrder_AdditionalService_InvoiceNotification;
                        await _customMessageTokenProvider.WgsAdditionalServiceAddTokenAsync(commonTokens, order, languageId);
                        replyToEmail = await _settingService.GetSettingByKeyAsync<string>("store.support.email");
                    }
                }
            }

            //#endregion

            var messageTemplates = await GetActiveMessageTemplatesAsync(template, store.Id);
            if (!messageTemplates.Any())
                return new List<int>();

            //tokens

            await _customMessageTokenProvider.CustomOrderAddTokensAsync(commonTokens, order, languageId);
            await _customMessageTokenProvider.CustomAddCustomerTokensAsync(commonTokens, order.CustomerId == null ? 0 : Convert.ToInt32(order.CustomerId));

            return await messageTemplates.SelectAwait(async messageTemplate =>
            {
                //email account
                var emailAccount = await GetEmailAccountOfMessageTemplateAsync(messageTemplate, languageId);

                var tokens = new List<Token>(commonTokens);
                await _customMessageTokenProvider.AddStoreTokensAsync(tokens, store, emailAccount, languageId);
                await _customMessageTokenProvider.AddStoreLogoToken(tokens);

                //event notification
                await _eventPublisher.MessageTokensAddedAsync(messageTemplate, tokens);

                int billingAddressId = 0;
                int shippingAddressId = 0;
                if (order.LiveOrderNumber != null && order.LiveOrderNumber > 0)
                {
                    var _order = await _orderService.GetOrderByIdAsync(Convert.ToInt32(order.LiveOrderNumber));
                    billingAddressId = _order.BillingAddressId == null ? 0 : Convert.ToInt32(_order.BillingAddressId);
                    shippingAddressId = _order.ShippingAddressId == null ? 0 : Convert.ToInt32(_order.ShippingAddressId);
                }
                else if (order.CustomerId != null && order.CustomerId > 0)
                {
                    var customer = await _customerService.GetCustomerByIdAsync(Convert.ToInt32(order.CustomerId));
                    billingAddressId = customer.BillingAddressId == null ? 0 : Convert.ToInt32(customer.BillingAddressId);
                    shippingAddressId = customer.ShippingAddressId == null ? 0 : Convert.ToInt32(customer.ShippingAddressId);
                }

                var billingAddress = await _addressService.GetAddressByIdAsync(billingAddressId);
                if (billingAddress == null)
                    billingAddress = await _addressService.GetAddressByIdAsync(shippingAddressId);



                var toEmail = string.IsNullOrEmpty(email) ? string.IsNullOrEmpty(order.CustomerCCEmail) ? billingAddress?.Email : order.CustomerCCEmail : email;
                var toName = $"{billingAddress.FirstName} {billingAddress.LastName}";

                string bccEmail = string.Empty;
                if (!string.IsNullOrEmpty(order.CustomerCCEmail))
                    bccEmail = (string.IsNullOrEmpty(messageTemplate.BccEmailAddresses) ? "" : (messageTemplate.BccEmailAddresses + ";"))
                    + order.CustomerCCEmail;

                var _workContext = EngineContext.Current.Resolve<IWorkContext>();
                var currentCustomer = await _workContext.GetCurrentCustomerAsync();
                string customerEmail = string.Empty;
                if (await _customerService.IsRegisteredAsync(currentCustomer))
                {
                    customerEmail = await _customCustomerService.GetCustomerEmail(currentCustomer);
                }
                if (!string.IsNullOrEmpty(customerEmail))
                {
                    var _messageTemplate = new MessageTemplate();
                    _messageTemplate.BccEmailAddresses = string.IsNullOrEmpty(bccEmail) ? customerEmail : $"{bccEmail};{customerEmail}";
                    _messageTemplate.Subject = messageTemplate.Subject;
                    _messageTemplate.Body = messageTemplate.Body;
                    _messageTemplate.Id = messageTemplate.Id;
                    _messageTemplate.AttachedDownloadId = messageTemplate.AttachedDownloadId;
                    _messageTemplate.DelayBeforeSend = messageTemplate.DelayBeforeSend;
                    _messageTemplate.DelayPeriod = messageTemplate.DelayPeriod;
                    _messageTemplate.DelayPeriodId = messageTemplate.DelayPeriodId;
                    _messageTemplate.EmailAccountId = messageTemplate.EmailAccountId;
                    _messageTemplate.IsActive = messageTemplate.IsActive;
                    _messageTemplate.LimitedToStores = messageTemplate.LimitedToStores;
                    _messageTemplate.Name = messageTemplate.Name;

                    return await SendNotificationAsync(_messageTemplate, emailAccount, languageId, tokens, toEmail, toName,
                        null, null, replyToEmail);
                }
                else
                {
                    var _messageTemplate = new MessageTemplate();
                    _messageTemplate.BccEmailAddresses = bccEmail;
                    _messageTemplate.Subject = messageTemplate.Subject;
                    _messageTemplate.Body = messageTemplate.Body;
                    _messageTemplate.Id = messageTemplate.Id;
                    _messageTemplate.AttachedDownloadId = messageTemplate.AttachedDownloadId;
                    _messageTemplate.DelayBeforeSend = messageTemplate.DelayBeforeSend;
                    _messageTemplate.DelayPeriod = messageTemplate.DelayPeriod;
                    _messageTemplate.DelayPeriodId = messageTemplate.DelayPeriodId;
                    _messageTemplate.EmailAccountId = messageTemplate.EmailAccountId;
                    _messageTemplate.IsActive = messageTemplate.IsActive;
                    _messageTemplate.LimitedToStores = messageTemplate.LimitedToStores;
                    _messageTemplate.Name = messageTemplate.Name;

                    return await SendNotificationAsync(_messageTemplate, emailAccount, languageId, tokens, toEmail, toName,
                        null, null, replyToEmail);
                }
            }).ToListAsync();
        }

        public async Task<IList<int>> CustomOrder_SendCustomerNotificationAsync(CustomOrder order, int languageId, bool customerOnly = false)
        {
            if (order == null)
                throw new ArgumentNullException(nameof(order));


            var store = (await _storeService.GetAllStoresAsync()).OrderBy(s => s.DisplayOrder).FirstOrDefault();

            languageId = await EnsureLanguageIsActiveAsync(languageId, store.Id);
            var messageTemplates = await GetActiveMessageTemplatesAsync(order.ParentOrderID > 0 ?
                                  order.OrderTotal <= 0 ? MessageTemplateExtendedSystemNames.CustomOrder_WGS_Complementory_CustomerNotification : MessageTemplateExtendedSystemNames.CustomOrder_WGS_CustomerNotification : MessageTemplateExtendedSystemNames.CustomOrder_CustomerNotification, store.Id);


            if (!messageTemplates.Any())
                return new List<int>();

            //tokens
            var commonTokens = new List<Token>();
            await _customMessageTokenProvider.CustomOrderAddTokensAsync(commonTokens, order, languageId);
            await _customMessageTokenProvider.CustomAddCustomerTokensAsync(commonTokens, order.CustomerId == null ? 0 : Convert.ToInt32(order.CustomerId));
           


            List<int> orderPlacedStoreOwnerNotificationQueuedEmailIds = await messageTemplates.SelectAwait(async messageTemplate =>
            {
                //email account
                var emailAccount = await GetEmailAccountOfMessageTemplateAsync(messageTemplate, languageId);
                if (customerOnly)
                {
                    messageTemplate.BccEmailAddresses = null;
                }

                var tokens = new List<Token>(commonTokens);
                await _customMessageTokenProvider.AddStoreTokensAsync(tokens, store, emailAccount, languageId);
                await _customMessageTokenProvider.AddStoreLogoToken(tokens);

                //event notification
                await _eventPublisher.MessageTokensAddedAsync(messageTemplate, tokens);

                int billingAddressId = 0;
                if (order.LiveOrderNumber != null && order.LiveOrderNumber > 0)
                {
                    var _order = await _orderService.GetOrderByIdAsync(Convert.ToInt32(order.LiveOrderNumber));
                    billingAddressId = _order.BillingAddressId == null ? 0 : Convert.ToInt32(_order.BillingAddressId);
                }
                else if (order.CustomerId != null && order.CustomerId > 0)
                {
                    var customer = await _customerService.GetCustomerByIdAsync(Convert.ToInt32(order.CustomerId));
                    billingAddressId = customer.BillingAddressId == null ? 0 : Convert.ToInt32(customer.BillingAddressId);
                }

                var billingAddress = await _addressService.GetAddressByIdAsync(billingAddressId);

                var toEmail = billingAddress?.Email;
                var toName = $"{billingAddress.FirstName} {billingAddress.LastName}";

                return await SendNotificationAsync(messageTemplate, emailAccount, languageId, tokens, toEmail, toName,
                    null, null);
            }).ToListAsync();


            try
            {
                if (orderPlacedStoreOwnerNotificationQueuedEmailIds.Any())
                {
                    if (order.LiveOrderNumber != null && order.LiveOrderNumber > 0)
                    {
                        var _order = await _orderService.GetOrderByIdAsync(Convert.ToInt32(order.LiveOrderNumber));
                        await _orderService.InsertOrderNoteAsync(new OrderNote
                        {
                            OrderId = _order.Id,
                            Note = $"\"Order placed\" email (to store owner) has been queued. Queued email identifiers: {string.Join(", ", orderPlacedStoreOwnerNotificationQueuedEmailIds)}.",
                            DisplayToCustomer = false,
                            CreatedOnUtc = DateTime.UtcNow
                        });
                    }
                }
            }
            catch
            {

            }



            return orderPlacedStoreOwnerNotificationQueuedEmailIds;
        }
        public async Task<IList<int>> CustomOrder_SendPartialPaymentLinkNotificationAsync(CustomOrder order, int languageId, string email = "")
        {
            if (order == null)
                throw new ArgumentNullException(nameof(order));


            var store = (await _storeService.GetAllStoresAsync()).OrderBy(s => s.DisplayOrder).FirstOrDefault();

            languageId = await EnsureLanguageIsActiveAsync(languageId, store.Id);

            var messageTemplates = await GetActiveMessageTemplatesAsync(MessageTemplateExtendedSystemNames.CustomOrder_PartialOrderInvoiceNotification, store.Id);
            if (!messageTemplates.Any())
                return new List<int>();

            //tokens
            var commonTokens = new List<Token>();
            await _customMessageTokenProvider.CustomOrderAddTokensAsync(commonTokens, order, languageId);
            await _customMessageTokenProvider.CustomAddCustomerTokensAsync(commonTokens, order.CustomerId == null ? 0 : Convert.ToInt32(order.CustomerId));
     
            return await messageTemplates.SelectAwait(async messageTemplate =>
            {
                //email account
                var emailAccount = await GetEmailAccountOfMessageTemplateAsync(messageTemplate, languageId);

                var tokens = new List<Token>(commonTokens);
                await _customMessageTokenProvider.AddStoreTokensAsync(tokens, store, emailAccount, languageId);
                await _customMessageTokenProvider.AddStoreLogoToken(tokens);

                //event notification
                await _eventPublisher.MessageTokensAddedAsync(messageTemplate, tokens);

                int billingAddressId = 0;
                if (order.LiveOrderNumber != null && order.LiveOrderNumber > 0)
                {
                    var _order = await _orderService.GetOrderByIdAsync(Convert.ToInt32(order.LiveOrderNumber));
                    billingAddressId = _order.BillingAddressId == null ? 0 : Convert.ToInt32(_order.BillingAddressId);
                }
                else if (order.CustomerId != null && order.CustomerId > 0)
                {
                    var customer = await _customerService.GetCustomerByIdAsync(Convert.ToInt32(order.CustomerId));
                    billingAddressId = customer.BillingAddressId == null ? 0 : Convert.ToInt32(customer.BillingAddressId);
                }

                var billingAddress = await _addressService.GetAddressByIdAsync(billingAddressId);

                var toEmail = string.IsNullOrEmpty(email) ? billingAddress?.Email : email;
                var toName = $"{billingAddress.FirstName} {billingAddress.LastName}";

                var _workContext = EngineContext.Current.Resolve<IWorkContext>();
                string customerEmail = await _customCustomerService.GetCustomerEmail(await _workContext.GetCurrentCustomerAsync());
                if (!string.IsNullOrEmpty(customerEmail))
                {
                    var _messageTemplate = new MessageTemplate();
                    _messageTemplate.BccEmailAddresses = string.IsNullOrEmpty(messageTemplate.BccEmailAddresses) ? customerEmail : $"{messageTemplate.BccEmailAddresses};{customerEmail}";
                    _messageTemplate.Subject = messageTemplate.Subject;
                    _messageTemplate.Body = messageTemplate.Body;
                    _messageTemplate.Id = messageTemplate.Id;
                    _messageTemplate.AttachedDownloadId = messageTemplate.AttachedDownloadId;
                    _messageTemplate.DelayBeforeSend = messageTemplate.DelayBeforeSend;
                    _messageTemplate.DelayPeriod = messageTemplate.DelayPeriod;
                    _messageTemplate.DelayPeriodId = messageTemplate.DelayPeriodId;
                    _messageTemplate.EmailAccountId = messageTemplate.EmailAccountId;
                    _messageTemplate.IsActive = messageTemplate.IsActive;
                    _messageTemplate.LimitedToStores = messageTemplate.LimitedToStores;
                    _messageTemplate.Name = messageTemplate.Name;

                    return await SendNotificationAsync(_messageTemplate, emailAccount, languageId, tokens, toEmail, toName,
                        null, null);
                }
                else
                {
                    return await SendNotificationAsync(messageTemplate, emailAccount, languageId, tokens, toEmail, toName,
                        null, null);
                }
            }).ToListAsync();
        }


        //public async Task<IList<int>> CustomOrder_SendPartialOrderLinkNotificationAsync(CustomOrder order, int languageId)
        //{
        //    if (order == null)
        //        throw new ArgumentNullException(nameof(order));


        //    var store = (await _storeService.GetAllStoresAsync()).OrderBy(s => s.DisplayOrder).FirstOrDefault();

        //    languageId = await EnsureLanguageIsActiveAsync(languageId, store.Id);

        //    var messageTemplates = await GetActiveMessageTemplatesAsync(MessageTemplateExtendedSystemNames.CustomOrder_PartialOrderLinkNotification, store.Id);
        //    if (!messageTemplates.Any())
        //        return new List<int>();

        //    //tokens
        //    var commonTokens = new List<Token>();
        //    await _customMessageTokenProvider.CustomOrderAddTokensAsync(commonTokens, order, languageId);
        //    await _customMessageTokenProvider.CustomAddCustomerTokensAsync(commonTokens, order.CustomerId == null ? 0 : Convert.ToInt32(order.CustomerId));
        //    var _customOrderService = EngineContext.Current.Resolve<Nop.Services.Customizations.Phone_Order.ICustomOrderService>();
        //    return await messageTemplates.SelectAwait(async messageTemplate =>
        //    {
        //        //email account
        //        var emailAccount = await GetEmailAccountOfMessageTemplateAsync(messageTemplate, languageId);

        //        var tokens = new List<Token>(commonTokens);
        //        await _customMessageTokenProvider.AddStoreTokensAsync(tokens, store, emailAccount);

        //        //event notification
        //        await _eventPublisher.MessageTokensAddedAsync(messageTemplate, tokens);

        //        int billingAddressId = 0;
        //        if (order.LiveOrderNumber != null && order.LiveOrderNumber > 0)
        //        {
        //            var customer = await _orderService.GetOrderByIdAsync(Convert.ToInt32(order.LiveOrderNumber));
        //            billingAddressId = customer.BillingAddressId == null ? 0 : Convert.ToInt32(customer.BillingAddressId);
        //        }
        //        else if (order.CustomerId != null && order.CustomerId > 0)
        //        {
        //            var customer = await _customerService.GetCustomerByIdAsync(Convert.ToInt32(order.CustomerId));
        //            billingAddressId = customer.BillingAddressId == null ? 0 : Convert.ToInt32(customer.BillingAddressId);
        //        }

        //        var billingAddress = await _addressService.GetAddressByIdAsync(billingAddressId);

        //        var toEmail = billingAddress?.Email;
        //        var toName = $"{billingAddress.FirstName} {billingAddress.LastName}";

        //        return await SendNotificationAsync(messageTemplate, emailAccount, languageId, tokens, toEmail, toName,
        //            null, null);
        //    }).ToListAsync();
        //}
        #endregion
        #region Estimated Shipping Date Module

        public async Task<IList<int>> SendSupportNotificationZipCodeNotFound(int languageId, Store store
            , string zipCode, string ipAddress, string customerName, string customerEmail)
        {
            
            if (string.IsNullOrEmpty(zipCode))
            {
                await _logger.InsertLogAsync(LogLevel.Error, "Estimated Shipping Module Zip Code Empty", "");
                return new List<int>();
            }
            var _settingService = EngineContext.Current.Resolve<ISettingService>();
            var toEmail = await _settingService.GetSettingByKeyAsync<string>("Estimated.Delivery.Notification.Email");
            var toName = await _settingService.GetSettingByKeyAsync<string>("Estimated.Delivery.Notification.Name");
            toName = toName == null ? "" : toName;
            if (string.IsNullOrEmpty(toEmail))
            {
                await _logger.InsertLogAsync(LogLevel.Error, "Estomated Shipping Module toEmail Empty", "Please add Support email to Setting \"stimated.Delivery.Notification.Email\" ");
                return new List<int>();
            }
            var messageTemplates = await GetActiveMessageTemplatesAsync(MessageTemplateExtendedSystemNames.Support_Delivery_Estimation_NotFound_Notification, store.Id);
            if (!messageTemplates.Any())
                return new List<int>();

            //tokens
            var commonTokens = new List<Token>();
            _customMessageTokenProvider.CustomAddShippingToken(commonTokens, languageId, zipCode, ipAddress, customerName, customerEmail);


            return await messageTemplates.SelectAwait(async messageTemplate =>
            {
                //email account
                var emailAccount = await GetEmailAccountOfMessageTemplateAsync(messageTemplate, languageId);

                var tokens = new List<Token>(commonTokens);
                await _customMessageTokenProvider.AddStoreTokensAsync(tokens, store, emailAccount, languageId);
                await _customMessageTokenProvider.AddStoreLogoToken(tokens);

                //event notification
                await _eventPublisher.MessageTokensAddedAsync(messageTemplate, tokens);



                return await SendNotificationAsync(messageTemplate, emailAccount, languageId, tokens, toEmail, toName);
            }).ToListAsync();

        }

        #endregion


        #region Back In Stock Notification

        public virtual async Task<IList<int>> CustomSendBackInStockNotificationAsync(BackInStockSubscription subscription, int languageId, List<Token> tokens)
        {
            if (subscription == null)
                throw new ArgumentNullException(nameof(subscription));

            var customer = await _customerService.GetCustomerByIdAsync(subscription.CustomerId);

            if (customer == null)
                throw new ArgumentNullException(nameof(customer));

            //ensure that customer is registered (simple and fast way)
            if (!CommonHelper.IsValidEmail(subscription.Email))
                return new List<int>();

            var store = await _storeService.GetStoreByIdAsync(subscription.StoreId) ?? await _storeContext.GetCurrentStoreAsync();
            languageId = await EnsureLanguageIsActiveAsync(languageId, store.Id);

            var messageTemplates = await GetActiveMessageTemplatesAsync(MessageTemplateSystemNames.BACK_IN_STOCK_NOTIFICATION, store.Id);
            if (!messageTemplates.Any())
                return new List<int>();

            //tokens
            if (messageTemplates.Any())
            {
                var commonTokens = new List<Token>();
                commonTokens.AddRange(tokens);
                await _customMessageTokenProvider.AddCustomerTokensAsync(commonTokens, customer);


                var _mailchimpService = EngineContext.Current.Resolve<IMailchimpService>();
                if (await _mailchimpService.CartOperation(subscription.Email, "", "OutOfStock"))
                {
                    var _tokenizer = EngineContext.Current.Resolve<ITokenizer>();
                    var emailAccount = await GetEmailAccountOfMessageTemplateAsync(messageTemplates.FirstOrDefault(), languageId);
                    var alltokens = new List<Token>(commonTokens);
                    await _customMessageTokenProvider.AddStoreTokensAsync(alltokens, store, emailAccount,languageId);
                    var _mandrillService = EngineContext.Current.Resolve<IMandrillService>();

                    string subject = await _localizationService.GetLocalizedAsync(messageTemplates.FirstOrDefault(), mt => mt.Subject, languageId);
                    var body = await _localizationService.GetLocalizedAsync(messageTemplates.FirstOrDefault(), mt => mt.Body, languageId);

                    var subjectReplaced = _tokenizer.Replace(subject, alltokens, false);
                    var bodyReplaced = _tokenizer.Replace(body, alltokens, true);
                    var toName = await _customerService.GetCustomerFullNameAsync(customer);
                    DataTable dt = new DataTable();
                    dt.Columns.Add("email");
                    dt.Columns.Add("name");
                    dt.Rows.Add(subscription.Email, "");
                    await _mandrillService.SendEmail(bodyReplaced, subjectReplaced, dt, null, System.Net.WebRequestMethods.Http.Post);
                }
            }
            return new List<int>();
        }
        #endregion

        #endregion

        #region Customer

        public virtual async Task<IList<int>> OldCustomerSendCustomerPasswordRecoveryMessageAsync(Customer customer, int languageId)
        {
            if (customer == null)
                throw new ArgumentNullException(nameof(customer));

            var store = await _storeContext.GetCurrentStoreAsync();
            languageId = await EnsureLanguageIsActiveAsync(languageId, store.Id);

            var messageTemplates = await GetActiveMessageTemplatesAsync(MessageTemplateExtendedSystemNames.OldCustomerPasswordRecoveryMessage, store.Id);
            if (!messageTemplates.Any())
                return new List<int>();

            //tokens
            var commonTokens = new List<Token>();
            await _customMessageTokenProvider.AddCustomerTokensAsync(commonTokens, customer);

            return await messageTemplates.SelectAwait(async messageTemplate =>
            {
                //email account
                var emailAccount = await GetEmailAccountOfMessageTemplateAsync(messageTemplate, languageId);

                var tokens = new List<Token>(commonTokens);
                await _customMessageTokenProvider.AddStoreTokensAsync(tokens, store, emailAccount, languageId);

                //event notification
                await _eventPublisher.MessageTokensAddedAsync(messageTemplate, tokens);

                var toEmail = customer.Email;
                var toName = await _customerService.GetCustomerFullNameAsync(customer);

                return await SendNotificationAsync(messageTemplate, emailAccount, languageId, tokens, toEmail, toName);
            }).ToListAsync();
        }

        #endregion

        #region Wishlist

        public virtual async Task<IList<int>> CustomSendWishlistEmailAFriendMessageAsync(Customer customer, int languageId,
          string customerEmail, string friendsEmail, string personalMessage)
        {
            if (customer == null)
                throw new ArgumentNullException(nameof(customer));

            var store = await _storeContext.GetCurrentStoreAsync();
            languageId = await EnsureLanguageIsActiveAsync(languageId, store.Id);

            var messageTemplates = await GetActiveMessageTemplatesAsync(MessageTemplateExtendedSystemNames.CustomWishlistToFriendMessage, store.Id);
            if (!messageTemplates.Any())
                return new List<int>();

            //tokens
            var commonTokens = new List<Token>();
            await _customMessageTokenProvider.AddCustomerTokensAsync(commonTokens, customer);
            commonTokens.Add(new Token("Wishlist.PersonalMessage", personalMessage, true));
            commonTokens.Add(new Token("Wishlist.Email", customerEmail));

            return await messageTemplates.SelectAwait(async messageTemplate =>
            {
                //email account
                var emailAccount = await GetEmailAccountOfMessageTemplateAsync(messageTemplate, languageId);

                var tokens = new List<Token>(commonTokens);
                await _customMessageTokenProvider.AddStoreTokensAsync(tokens, store, emailAccount, languageId);

                //event notification
                await _eventPublisher.MessageTokensAddedAsync(messageTemplate, tokens);

                return await SendNotificationAsync(messageTemplate, emailAccount, languageId, tokens, friendsEmail, string.Empty);
            }).ToListAsync();
        }
        #endregion

        #region Abandoned Card

        public async Task<List<int>> SendSupportAbandonedCartEmailMessage(Customer customer, int[] cartItems, int languageId)
        {

            var store = _storeContext.GetCurrentStore();
            var messageTemplates = await GetActiveMessageTemplatesAsync(MessageTemplateExtendedSystemNames.CustomSupportAbandonedCartNotification, store.Id);
            var _shoppingCartService = EngineContext.Current.Resolve<IShoppingCartService>();
            var cart = await _shoppingCartService.GetShoppingCartAsync(customer, ShoppingCartType.ShoppingCart,
                          store.Id);
            if (!messageTemplates.Any() || customer == null || cart.Count == 0)
                return new List<int>();

            cart = cart.Where(c => cartItems.Contains(c.Id)).ToList();
            var commonTokens = new List<Token>();
            await _customMessageTokenProvider.CustomSupportAddAbandonedCartTokensAsync(commonTokens, customer, cart, languageId);

            var _settingService = EngineContext.Current.Resolve<ISettingService>();
            var storeEmail = await _settingService.GetSettingByKeyAsync<string>("store.email");
            return await messageTemplates.SelectAwait(async messageTemplate =>
            {
                //email account
                var emailAccount = await GetEmailAccountOfMessageTemplateAsync(messageTemplate, languageId);

                var tokens = new List<Token>(commonTokens);
                await _customMessageTokenProvider.AddStoreTokensAsync(tokens, store, emailAccount, languageId);

                await _customMessageTokenProvider.AddStoreLogoToken(tokens);

                //event notification
                await _eventPublisher.MessageTokensAddedAsync(messageTemplate, tokens);

                var toEmail = string.IsNullOrEmpty(storeEmail) ? emailAccount.Email : storeEmail;
                var toName = emailAccount.DisplayName;

                return await SendNotificationAsync(messageTemplate, emailAccount, languageId, tokens, toEmail, toName);
            }).ToListAsync();

        }
        public async Task<bool> SendAbandonedCartReminderNotificationAsync(Customer customer, string name, string email, int messageTemplateId, string cartLink, Product product, List<Product> relatedProducts, int languageId, string utmSource)
        {
            var messageTemplate = await _messageTemplateService.GetMessageTemplateByIdAsync(messageTemplateId);
            if (messageTemplate == null)
                return false;

            var store = _storeContext.GetCurrentStore();

            var tokens = new List<Token>();
            await _customMessageTokenProvider.CustomAddAbandonedCartTokensAsync(tokens, customer, cartLink, product, relatedProducts, languageId, utmSource);

            var emailAccount = await GetEmailAccountOfMessageTemplateAsync(messageTemplate, languageId);
            await _customMessageTokenProvider.AddStoreTokensAsync(tokens, store, emailAccount, languageId);
            await _customMessageTokenProvider.AddStoreLogoToken(tokens);
            await _eventPublisher.MessageTokensAddedAsync(messageTemplate, tokens);
            var toEmail = email;
            var toName = name;


            var bcc = await _localizationService.GetLocalizedAsync(messageTemplate, mt => mt.BccEmailAddresses, languageId);
            var subject = await _localizationService.GetLocalizedAsync(messageTemplate, mt => mt.Subject, languageId);
            var body = await _localizationService.GetLocalizedAsync(messageTemplate, mt => mt.Body, languageId);
            var subjectReplaced = _tokenizer.Replace(subject, tokens, false);
            var bodyReplaced = _tokenizer.Replace(body, tokens, true);

            DataTable dt = new DataTable();
            dt.Columns.Add(new DataColumn() { ColumnName = "email", DataType = typeof(string) });
            dt.Columns.Add(new DataColumn() { ColumnName = "name", DataType = typeof(string) });
            dt.Rows.Add(toEmail, name);
            if (!string.IsNullOrEmpty(bcc))
            {
                foreach (var bccEmail in bcc.Split(';'))
                {
                    dt.Rows.Add(bccEmail, bccEmail);
                }
            }
            var _mandrillService = EngineContext.Current.Resolve<IMandrillService>();
            await _mandrillService.SendEmail(bodyReplaced, subjectReplaced, dt, new Dictionary<string, string>(), "POST");

            return true;
        }

        #endregion

        #region Purchase Journey

        public async Task<(bool, string)> SendPurchaseJourneyNotificationAsync(Customer customer, List<Product> products, int productId, int categoryId, string templatetype, int messageTemplateId, string utm_params, int languageId)
        {
            var messageTemplate = await _messageTemplateService.GetMessageTemplateByIdAsync(messageTemplateId);
            var store = _storeContext.GetCurrentStore();
            var tokens = new List<Token>();
            var emailAccount = await GetEmailAccountOfMessageTemplateAsync(messageTemplate, languageId);
            await _customMessageTokenProvider.AddStoreTokensAsync(tokens, store, emailAccount, languageId);
            await _customMessageTokenProvider.AddStoreLogoToken(tokens);
            await _customMessageTokenProvider.CustomAddCustomerTokensAsync(tokens, customer.Id);

            if (products.Count > 0)
            {
                await _customMessageTokenProvider.CustomAddPurchaseJourneyTokenAsync(tokens, customer, products, productId, categoryId, templatetype, utm_params);
            }
            await _eventPublisher.MessageTokensAddedAsync(messageTemplate, tokens);
            var toEmail = await _customCustomerService.GetCustomerEmail(customer);
            var toName = await _customerService.GetCustomerFullNameAsync(customer);

            var _settingService = EngineContext.Current.Resolve<ISettingService>();

            if (await _settingService.GetSettingByKeyAsync<bool>("Purchase.Journey.Sandbox.Enabled"))
            {
                toEmail = await _settingService.GetSettingByKeyAsync<string>("Purchase.Journey.Sandbox.Email");
            }

            var bcc = await _localizationService.GetLocalizedAsync(messageTemplate, mt => mt.BccEmailAddresses, languageId);
            var subject = await _localizationService.GetLocalizedAsync(messageTemplate, mt => mt.Subject, languageId);
            var body = await _localizationService.GetLocalizedAsync(messageTemplate, mt => mt.Body, languageId);
            var subjectReplaced = _tokenizer.Replace(subject, tokens, false);
            var bodyReplaced = _tokenizer.Replace(body, tokens, true);

            DataTable dt = new DataTable();
            dt.Columns.Add(new DataColumn() { ColumnName = "email", DataType = typeof(string) });
            dt.Columns.Add(new DataColumn() { ColumnName = "name", DataType = typeof(string) });
            dt.Rows.Add(toEmail, toName);
            if (!string.IsNullOrEmpty(bcc))
            {
                foreach (var bccEmail in bcc.Split(';'))
                {
                    dt.Rows.Add(bccEmail, bccEmail);
                }
            }
            var _mandrillService = EngineContext.Current.Resolve<IMandrillService>();
            await _mandrillService.SendEmail(bodyReplaced, subjectReplaced, dt, new Dictionary<string, string>(), "POST");

            return (true, bodyReplaced);
        }
        #endregion

        #region Payment Issue
        public async Task<List<int>> SendSupportOrderTotalMismatchEmailMessage(
        Customer customer,
        int[] cartItems,
        int languageId,
        string transactionId,
        decimal paidAmount,
        decimal expectedAmount,
        string paymentMethod)
        {

            var store = _storeContext.GetCurrentStore();
            var messageTemplates = await GetActiveMessageTemplatesAsync(MessageTemplateExtendedSystemNames.OrderTotalMismatchSupportNotification, store.Id);
            var _shoppingCartService = EngineContext.Current.Resolve<IShoppingCartService>();
            var cart = await _shoppingCartService.GetShoppingCartAsync(customer, ShoppingCartType.ShoppingCart,
                          store.Id);
            if (!messageTemplates.Any() || customer == null || cart.Count == 0)
                return new List<int>();

            cart = cart.Where(c => cartItems.Contains(c.Id)).ToList();
            var commonTokens = new List<Token>();
            await _customMessageTokenProvider.CustomSupportAddAbandonedCartTokensAsync(commonTokens, customer, cart, languageId);
            await _customMessageTokenProvider.CustomSupportAddPaymentIssueTokensAsync(commonTokens, transactionId, paidAmount, expectedAmount, paymentMethod);
            var _settingService = EngineContext.Current.Resolve<ISettingService>();
            var storeEmail = await _settingService.GetSettingByKeyAsync<string>("store.email");
            return await messageTemplates.SelectAwait(async messageTemplate =>
            {
                //email account
                var emailAccount = await GetEmailAccountOfMessageTemplateAsync(messageTemplate, languageId);

                var tokens = new List<Token>(commonTokens);
                await _customMessageTokenProvider.AddStoreTokensAsync(tokens, store, emailAccount, languageId);

                await _customMessageTokenProvider.AddStoreLogoToken(tokens);

                //event notification
                await _eventPublisher.MessageTokensAddedAsync(messageTemplate, tokens);

                var toEmail = string.IsNullOrEmpty(storeEmail) ? emailAccount.Email : storeEmail;
                var toName = emailAccount.DisplayName;

                return await SendNotificationAsync(messageTemplate, emailAccount, languageId, tokens, toEmail, toName);
            }).ToListAsync();

        }


        public async Task<List<int>> SendSupportPendingOrderEmailMessage(
     Customer customer,
     int languageId,
     string transactionId,
     string orderId,
     bool isCustomOrder,
     int customOrderNumber,
     string paymentMethod
     )
        {

            var store = _storeContext.GetCurrentStore();
            var messageTemplates = await GetActiveMessageTemplatesAsync(MessageTemplateExtendedSystemNames.SupportPendingOrderMessage, store.Id);
            if (!messageTemplates.Any() || customer == null)
                return new List<int>();

            var commonTokens = new List<Token>();
            await _customMessageTokenProvider.AddCustomerTokensAsync(commonTokens, customer);
            await _customMessageTokenProvider.CustomAddPendingOrderTokens(commonTokens, transactionId, orderId, isCustomOrder, customOrderNumber, paymentMethod);
            var _settingService = EngineContext.Current.Resolve<ISettingService>();
            var storeEmail = await _settingService.GetSettingByKeyAsync<string>("store.email");
            return await messageTemplates.SelectAwait(async messageTemplate =>
            {
                //email account
                var emailAccount = await GetEmailAccountOfMessageTemplateAsync(messageTemplate, languageId);

                var tokens = new List<Token>(commonTokens);
                await _customMessageTokenProvider.AddStoreTokensAsync(tokens, store, emailAccount, languageId);

                await _customMessageTokenProvider.AddStoreLogoToken(tokens);

                //event notification
                await _eventPublisher.MessageTokensAddedAsync(messageTemplate, tokens);

                var toEmail = string.IsNullOrEmpty(storeEmail) ? emailAccount.Email : storeEmail;
                var toName = emailAccount.DisplayName;

                return await SendNotificationAsync(messageTemplate, emailAccount, languageId, tokens, toEmail, toName);
            }).ToListAsync();

        }

        #endregion
    }

}
