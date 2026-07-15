using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MWT.Tax.FixedOrByCountryStateZip.Services
{
    public partial interface ITaxZarService
    {
        Task<(bool, bool, decimal, string,string, int, string)> GetTaxRate(int customerId, string zipcode);
    }
}
