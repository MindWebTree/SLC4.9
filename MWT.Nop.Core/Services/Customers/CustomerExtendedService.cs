using LinqToDB.Data;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Blogs;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Forums;
using Nop.Core.Domain.Logging;
using Nop.Core.Domain.News;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Polls;
using Nop.Core.Domain.Tax;
using Nop.Core.Events;
using Nop.Core.Infrastructure;
using Nop.Data;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Customers;
using Nop.Services.Localization;
using Nop.Services.Logging;

namespace MWT.Nop.Core.Services.Customers
{
    /// <summary>
    /// Customer service
    /// </summary>
    public partial class CustomerExtendedService : CustomerService, ICustomerExtendedService
    {
        public CustomerExtendedService(CustomerSettings customerSettings, IEventPublisher eventPublisher, IGenericAttributeService genericAttributeService, INopDataProvider dataProvider, IRepository<Address> customerAddressRepository, IRepository<BlogComment> blogCommentRepository, IRepository<Customer> customerRepository,
            IRepository<CustomerAddressMapping> customerAddressMappingRepository, IRepository<CustomerCustomerRoleMapping>
            customerCustomerRoleMappingRepository, IRepository<CustomerPassword> customerPasswordRepository,
            IRepository<CustomerRole> customerRoleRepository, IRepository<ForumPost> forumPostRepository,
            IRepository<ForumTopic> forumTopicRepository, IRepository<GenericAttribute> gaRepository, IRepository<NewsComment>
            newsCommentRepository, IRepository<Order> orderRepository, IRepository<ProductReview> productReviewRepository,
            IRepository<ProductReviewHelpfulness> productReviewHelpfulnessRepository, IRepository<PollVotingRecord> pollVotingRecordRepository,
            IRepository<ShoppingCartItem> shoppingCartRepository, IShortTermCacheManager shortTermCacheManager,
            IStaticCacheManager staticCacheManager, IStoreContext storeContext, ShoppingCartSettings shoppingCartSettings, TaxSettings taxSettings) : base(customerSettings, eventPublisher, genericAttributeService, dataProvider, customerAddressRepository, blogCommentRepository, customerRepository, customerAddressMappingRepository, customerCustomerRoleMappingRepository, customerPasswordRepository, customerRoleRepository, forumPostRepository, forumTopicRepository, gaRepository, newsCommentRepository, orderRepository, productReviewRepository, productReviewHelpfulnessRepository, pollVotingRecordRepository, shoppingCartRepository, shortTermCacheManager, staticCacheManager, storeContext, shoppingCartSettings, taxSettings)
        {
        }

        public async Task<bool> IsCustomerEligibleForMemberShipDiscount(Customer customer)
        {
            bool result = false;

            var _settingService = EngineContext.Current.Resolve<ISettingService>();
            var memberShipRole = await _settingService.GetSettingByKeyAsync<string>("MemberShip.Role.Name");
            if (memberShipRole == "MemberShip.Role.Name")
            {
                var _logger = EngineContext.Current.Resolve<ILogger>();
                await _logger.InsertLogAsync(LogLevel.Error, "Membership Program: Setting missed  \"MemberShip.Role.Name\" for Membership Program", "Membership wil not work without this setting.");
            }

            if (await IsRegisteredAsync(customer) && await IsInCustomerRoleAsync(customer, memberShipRole))
                result = true;
            else
            {
                var storeID = (await _storeContext.GetCurrentStoreAsync()).Id;
                result = await _genericAttributeService.GetAttributeAsync<bool>(customer, NopCustomerDefaults.MemberShipLabelAttribute, storeID);
            }
            return result;
        }

        public async Task<bool> IsCustomerPurchasedMembership(Customer customer)
        {
            bool result = false;
            var _settingService = EngineContext.Current.Resolve<ISettingService>();
            var memberShipRole = await _settingService.GetSettingByKeyAsync<string>("MemberShip.Role.Name");
            if (memberShipRole == "MemberShip.Role.Name")
            {
                var _logger = EngineContext.Current.Resolve<ILogger>();
                await _logger.InsertLogAsync(LogLevel.Error, "Membership Program: Setting missed  \"MemberShip.Role.Name\" for Membership Program", "Membership wil not work without this setting.");
            }

            if (await IsRegisteredAsync(customer) && await IsInCustomerRoleAsync(customer, memberShipRole))
                result = true;
            return result;
        }

        public async Task<bool> IsMemberShipAddedInCart(Customer customer)
        {
            bool result = false;
            var _settingService = EngineContext.Current.Resolve<ISettingService>();
            var isMembershipEnabled = await _settingService.GetSettingByKeyAsync<bool>("ismembershipenabled");
            if (isMembershipEnabled)
            {
                var memberShipRole = await _settingService.GetSettingByKeyAsync<string>("MemberShip.Role.Name");
                if (memberShipRole == "MemberShip.Role.Name")
                {
                    var _logger = EngineContext.Current.Resolve<ILogger>();
                    await _logger.InsertLogAsync(LogLevel.Error, "Membership Program: Setting missed  \"MemberShip.Role.Name\" for Membership Program", "Membership wil not work without this setting.");
                }

                if (await IsRegisteredAsync(customer) && await IsInCustomerRoleAsync(customer, memberShipRole))
                    result = false;
                else
                {
                    var storeID = (await _storeContext.GetCurrentStoreAsync()).Id;
                    result = await _genericAttributeService.GetAttributeAsync<bool>(customer, NopCustomerDefaults.MemberShipLabelAttribute, storeID);
                }
            }
            return result;
        }

        public async Task<IPagedList<Customer>> CustomGetAllCustomersAsync(string searchterm,
         int pageIndex = 0, int pageSize = int.MaxValue, bool getOnlyTotalCount = false)
        {
            if (!string.IsNullOrWhiteSpace(searchterm))
                searchterm = searchterm.Trim();
            var customers = await _customerRepository.GetAllPagedAsync(query =>
            {
                IQueryable<int> customers;
                customers = from c in _customerRepository.Table
                            join _customerRole in _customerCustomerRoleMappingRepository.Table
                            on c.Id equals _customerRole.CustomerId

                            where _customerRole.CustomerRoleId == 3 && (!string.IsNullOrWhiteSpace(searchterm) ? c.Email.Contains(searchterm)
                            : true)
                            select c.Id;


                if (!string.IsNullOrWhiteSpace(searchterm))
                {

                    customers = customers.Union(query
                        .Join(_customerAddressRepository.Table, x => x.ShippingAddressId, y => y.Id,
                            (x, y) => new { Customer = x, Attribute = y })
                        .Where(z =>
                                    z.Attribute.FirstName.Contains(searchterm, StringComparison.OrdinalIgnoreCase)
                                    || z.Attribute.LastName.Contains(searchterm, StringComparison.OrdinalIgnoreCase)
                                     || z.Attribute.Email.Contains(searchterm, StringComparison.OrdinalIgnoreCase)
                                        || z.Attribute.PhoneNumber.Contains(searchterm, StringComparison.OrdinalIgnoreCase)
                                    )
                        .Select(z => z.Customer.Id));

                    //customers = customers.Union(query
                    //                     .Join(_gaRepository.Table, x => x.Id, y => y.EntityId,
                    //                         (x, y) => new { Customer = x, Attribute = y })
                    //                     .Where(z => z.Attribute.KeyGroup == nameof(Customer) &&
                    //                                 z.Attribute.Key == NopCustomerDefaults.LastNameAttribute &&
                    //                                 z.Attribute.Value.Contains(searchterm, StringComparison.OrdinalIgnoreCase))
                    //                     .Select(z => z.Customer.Id));



                    //customers = customers.Union(query
                    //      .Join(_gaRepository.Table, x => x.Id, y => y.EntityId,
                    //        (x, y) => new { Customer = x, Attribute = y })
                    //    .Where(z => z.Attribute.KeyGroup == nameof(Customer) &&
                    //                z.Attribute.Key == NopCustomerDefaults.ZipPostalCodeAttribute &&
                    //                z.Attribute.Value.Contains(searchterm, StringComparison.OrdinalIgnoreCase))
                    //    .Select(z => z.Customer.Id));
                }

                query =
              from c in query
              from cid in LinqToDB.LinqExtensions.InnerJoin(customers, cid => cid == c.Id)
              where !c.Deleted
              select c;


                query = query.OrderByDescending(c => c.CreatedOnUtc);

                return query;
            }, pageIndex, pageSize, getOnlyTotalCount);

            return customers;
        }
        public async Task<IList<Customer>> CustomGetAllCustomersByStoreProcedureAsync(string searchterm)
        {
            if (!string.IsNullOrWhiteSpace(searchterm))
                searchterm = searchterm.Trim();
            searchterm = $"%{searchterm}%";

            var customers = await _customerRepository.EntityFromSqlAsync("SP_SearchCustomer", new DataParameter[] { new DataParameter()
                       {
                           DataType = LinqToDB.DataType.VarChar,
                           Value = searchterm,
                           Direction = System.Data.ParameterDirection.Input,
                           Name = "@searchTem",
                        } });

            return customers;
        }


        public virtual async Task<string> GetCustomerEmailAsync(
     Customer customer,
     bool isBillingEmail = false,
     bool isShippingEmail = false)
        {
            var addressService = EngineContext.Current.Resolve<IAddressService>();

            string email = null;

            // Billing email requested - Billing Address has first priority
            if (isBillingEmail && customer.BillingAddressId.HasValue)
            {
                var address = await addressService.GetAddressByIdAsync(customer.BillingAddressId.Value);
                email = address?.Email;
            }

            // Shipping email requested - Shipping Address has first priority
            if (isShippingEmail && string.IsNullOrEmpty(email) && customer.ShippingAddressId.HasValue)
            {
                var address = await addressService.GetAddressByIdAsync(customer.ShippingAddressId.Value);
                email = address?.Email;
            }

            // Default priority when neither billing nor shipping is explicitly requested
            if (!isBillingEmail && !isShippingEmail)
            {
                // Customer email
                email = customer.Email;

                // Generic attribute
                if (string.IsNullOrEmpty(email))
                {
                    email = await _genericAttributeService.GetAttributeAsync<string>(
                        customer, "Email");
                }

                // Shipping address
                if (string.IsNullOrEmpty(email) && customer.ShippingAddressId.HasValue)
                {
                    var address = await addressService.GetAddressByIdAsync(
                        customer.ShippingAddressId.Value);

                    email = address?.Email;
                }

                // Billing address
                if (string.IsNullOrEmpty(email) && customer.BillingAddressId.HasValue)
                {
                    var address = await addressService.GetAddressByIdAsync(
                        customer.BillingAddressId.Value);

                    email = address?.Email;
                }
            }
            else
            {
                // If billing/shipping email was requested but address email
                // was not available, fall back to Customer Email.
                if (string.IsNullOrEmpty(email))
                    email = customer.Email;

                // Then generic attribute
                if (string.IsNullOrEmpty(email))
                {
                    email = await _genericAttributeService.GetAttributeAsync<string>(
                        customer, "Email");
                }

                // Finally check the remaining address
                if (string.IsNullOrEmpty(email))
                {
                    if (isBillingEmail && customer.ShippingAddressId.HasValue)
                    {
                        var address = await addressService.GetAddressByIdAsync(
                            customer.ShippingAddressId.Value);

                        email = address?.Email;
                    }
                    else if (isShippingEmail && customer.BillingAddressId.HasValue)
                    {
                        var address = await addressService.GetAddressByIdAsync(
                            customer.BillingAddressId.Value);

                        email = address?.Email;
                    }
                }
            }

            return email;
        }

        public virtual async Task<string> GetCustomerPhoneAsync(
      Customer customer,
      bool isBillingPhone = false,
      bool isShippingPhone = false)
        {
            var addressService = EngineContext.Current.Resolve<IAddressService>();

            string phoneNumber = null;

            // Billing phone requested - Billing Address has first priority
            if (isBillingPhone && customer.BillingAddressId.HasValue)
            {
                var address = await addressService.GetAddressByIdAsync(
                    customer.BillingAddressId.Value);

                phoneNumber = address?.PhoneNumber;
            }

            // Shipping phone requested - Shipping Address has first priority
            if (isShippingPhone && string.IsNullOrEmpty(phoneNumber) &&
                customer.ShippingAddressId.HasValue)
            {
                var address = await addressService.GetAddressByIdAsync(
                    customer.ShippingAddressId.Value);

                phoneNumber = address?.PhoneNumber;
            }

            // Default priority:
            // Customer → Shipping → Billing
            if (!isBillingPhone && !isShippingPhone)
            {
                // Customer phone
                phoneNumber = customer.Phone;

                // Shipping address
                if (string.IsNullOrEmpty(phoneNumber) &&
                    customer.ShippingAddressId.HasValue)
                {
                    var address = await addressService.GetAddressByIdAsync(
                        customer.ShippingAddressId.Value);

                    phoneNumber = address?.PhoneNumber;
                }

                // Billing address
                if (string.IsNullOrEmpty(phoneNumber) &&
                    customer.BillingAddressId.HasValue)
                {
                    var address = await addressService.GetAddressByIdAsync(
                        customer.BillingAddressId.Value);

                    phoneNumber = address?.PhoneNumber;
                }
            }
            else
            {
                // Customer phone as fallback
                if (string.IsNullOrEmpty(phoneNumber))
                    phoneNumber = customer.Phone;

                // Remaining address as final fallback
                if (string.IsNullOrEmpty(phoneNumber))
                {
                    if (isBillingPhone && customer.ShippingAddressId.HasValue)
                    {
                        var address = await addressService.GetAddressByIdAsync(
                            customer.ShippingAddressId.Value);

                        phoneNumber = address?.PhoneNumber;
                    }
                    else if (isShippingPhone && customer.BillingAddressId.HasValue)
                    {
                        var address = await addressService.GetAddressByIdAsync(
                            customer.BillingAddressId.Value);

                        phoneNumber = address?.PhoneNumber;
                    }
                }
            }

            return phoneNumber;
        }
        public virtual async Task<string> GetExtendedCustomerFullNameAsync(Customer customer)
        {
            ArgumentNullException.ThrowIfNull(customer);

            var firstName = customer.FirstName;
            var lastName = customer.LastName;

            var fullName = string.Empty;
            if (!string.IsNullOrWhiteSpace(firstName) && !string.IsNullOrWhiteSpace(lastName))
            {
                var format = await EngineContext.Current.Resolve<ILocalizationService>().GetResourceAsync("Customer.FullNameFormat");
                fullName = string.Format(format, firstName, lastName);
            }
            else
            {
                var _addressService = EngineContext.Current.Resolve<IAddressService>();
                if (customer.BillingAddressId.HasValue)
                {
                    var address = await _addressService.GetAddressByIdAsync(Convert.ToInt32(customer.BillingAddressId));
                    firstName = address?.FirstName;
                    lastName = address?.LastName;
                }
                else if (customer.ShippingAddressId.HasValue)
                {
                    var address = await _addressService.GetAddressByIdAsync(Convert.ToInt32(customer.ShippingAddressId));
                    firstName = address?.FirstName;
                    lastName = address?.LastName;
                }
                if (!string.IsNullOrWhiteSpace(firstName) && !string.IsNullOrWhiteSpace(lastName))
                {
                    var format = await EngineContext.Current.Resolve<ILocalizationService>().GetResourceAsync("Customer.FullNameFormat");
                    fullName = string.Format(format, firstName, lastName);
                }
                else if (!string.IsNullOrWhiteSpace(firstName))
                    fullName = firstName;

                else if (!string.IsNullOrWhiteSpace(lastName))
                    fullName = lastName;
            }

            return fullName;
        }

        public virtual async Task<IList<Customer>> GetAllCategoryManagers()
        {
            // Get the role ID
            var roleId = await _customerRoleRepository.Table
                .Where(r => r.Name == "Category Manager")
                .Select(r => r.Id)
                .FirstOrDefaultAsync();

            var categoryManagers = await (from c in _customerRepository.Table
                                          join crm in _customerCustomerRoleMappingRepository.Table
                                              on c.Id equals crm.CustomerId
                                          where crm.CustomerRoleId == roleId
                                          select c)
                                    .Distinct()
                                    .ToListAsync();
            return categoryManagers;
        }
    }
}
