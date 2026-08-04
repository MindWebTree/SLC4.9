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
using Nop.Services.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MWT.Nop.Core.Services.Customers
{
    /// <summary>
    /// Customer service
    /// </summary>
    public partial class CustomCustomerService : CustomerService, ICustomCustomerService
    {
        public CustomCustomerService(CustomerSettings customerSettings, IEventPublisher eventPublisher, IGenericAttributeService genericAttributeService, INopDataProvider dataProvider, IRepository<Address> customerAddressRepository, IRepository<BlogComment> blogCommentRepository, IRepository<Customer> customerRepository, 
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
        public virtual async Task<string> GetCustomerEmail(Customer customer)
        {
            string email = customer.Email;
            if (string.IsNullOrEmpty(email))
            {
                var _addressService = EngineContext.Current.Resolve<IAddressService>();
                if (customer.BillingAddressId.HasValue)
                {
                    var address = await _addressService.GetAddressByIdAsync(Convert.ToInt32(customer.BillingAddressId));
                    email = address.Email;
                }
                if (customer.ShippingAddressId.HasValue && string.IsNullOrEmpty(email))
                {
                    var address = await _addressService.GetAddressByIdAsync(Convert.ToInt32(customer.ShippingAddressId));
                    email = address.Email;
                }
                if (string.IsNullOrEmpty(email))
                {
                    email = await _genericAttributeService.GetAttributeAsync<string>(customer, "Email");
                }
            }

            return email;
        }

        public virtual async Task<string> GetCustomerPhone(Customer customer)
        {

            string phonenumber = "";


            var _addressService = EngineContext.Current.Resolve<IAddressService>();
            if (customer.BillingAddressId.HasValue)
            {
                var address = await _addressService.GetAddressByIdAsync(Convert.ToInt32(customer.BillingAddressId));
                phonenumber = address.PhoneNumber;
            }
            if (customer.ShippingAddressId.HasValue && string.IsNullOrEmpty(phonenumber))
            {
                var address = await _addressService.GetAddressByIdAsync(Convert.ToInt32(customer.ShippingAddressId));
                phonenumber = address.PhoneNumber;
            }
            if (string.IsNullOrEmpty(phonenumber))
            {
                phonenumber = await _genericAttributeService.GetAttributeAsync<string>(customer, "Phone");
            }
            return phonenumber;

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
