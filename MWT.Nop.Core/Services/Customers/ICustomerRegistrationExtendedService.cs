using Nop.Core.Domain.Customers;
using Nop.Services.Customers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MWT.Nop.Core.Services.Customers
{
    public partial interface ICustomerRegistrationExtendedService: ICustomerRegistrationService
    {
        Task SignInCustomerAsync(Customer customer, bool isPersist = false);
    }
}
