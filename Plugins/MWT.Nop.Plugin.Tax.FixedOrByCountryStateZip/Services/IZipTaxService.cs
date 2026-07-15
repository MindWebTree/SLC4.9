using Nop.Core.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MWT.Tax.FixedOrByCountryStateZip.Services
{
    public partial interface IZipTaxService
    {
        Task<(bool, bool, decimal, string, string, int, string)> GetTaxRate(int customerId, Address address);
    }
}
