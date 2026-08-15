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
    public partial interface ICustomerExtendedService: ICustomerService
    {
        Task<bool> IsCustomerEligibleForMemberShipDiscount(Customer customer);

        Task<bool> IsCustomerPurchasedMembership(Customer customer);

        Task<bool> IsMemberShipAddedInCart(Customer customer);

        Task<IPagedList<Customer>> CustomGetAllCustomersAsync(string searchterm,
            int pageIndex = 0, int pageSize = int.MaxValue, bool getOnlyTotalCount = false);

      //  Task<IList<Customer>> CustomGetAllCustomersByStoreProcedureAsync(string searchterm);

        Task<string> GetCustomerEmail(Customer customer);

        Task<string> GetCustomerPhone(Customer customer);
        Task<IList<Customer>> GetAllCategoryManagers();
    }
}
