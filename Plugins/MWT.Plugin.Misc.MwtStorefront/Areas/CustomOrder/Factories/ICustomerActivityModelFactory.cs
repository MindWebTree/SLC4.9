using MWT.Plugin.Misc.MwtStorefront.Areas.CustomOrder.Models.Customers;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MWT.Plugin.Misc.MwtStorefront.Areas.CustomOrder.Factories
{
    public partial interface ICustomerActivityModelFactory
    {
        Task<List<CustomerActivityModel>> PrepareCustomerActivityListModel(int customerID);
    }
}
