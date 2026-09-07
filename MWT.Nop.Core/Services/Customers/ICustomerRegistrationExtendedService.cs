using Nop.Core.Domain.Customers;
using Nop.Services.Customers;

namespace MWT.Nop.Core.Services.Customers
{
    public partial interface ICustomerRegistrationExtendedService: ICustomerRegistrationService
    {
        Task SignInCustomerAsync(Customer customer, bool isPersist = false);
    }
}
