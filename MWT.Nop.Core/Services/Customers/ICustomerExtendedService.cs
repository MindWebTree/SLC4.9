using Nop.Core;
using Nop.Core.Domain.Customers;
using Nop.Services.Customers;
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
    public partial interface ICustomerExtendedService : ICustomerService
    {
        Task<bool> IsCustomerEligibleForMemberShipDiscount(Customer customer);
        Task<bool> IsCustomerPurchasedMembership(Customer customer);
        Task<bool> IsMemberShipAddedInCart(Customer customer);
        Task<IPagedList<Customer>> CustomGetAllCustomersAsync(string searchterm,
            int pageIndex = 0, int pageSize = int.MaxValue, bool getOnlyTotalCount = false);
        Task<IList<Customer>> CustomGetAllCustomersByStoreProcedureAsync(string searchterm);
        Task<string> GetCustomerEmailAsync(Customer customer, bool isBillingEmail = false,bool isShippingEmail = false);
        Task<string> GetCustomerPhoneAsync(Customer customer, bool isBillingPhone = false,bool isShippingPhone = false);
        Task<IList<Customer>> GetAllCategoryManagers();
        Task<string> GetExtendedCustomerFullNameAsync(Customer customer);
        Task<int> DeleteGuestCustomersAsync( bool onlyWithoutShoppingCart, int noOfCustomerTodelete);
    }
}
