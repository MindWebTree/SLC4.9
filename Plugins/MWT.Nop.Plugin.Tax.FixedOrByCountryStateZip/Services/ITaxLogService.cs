using MWT.Tax.FixedOrByCountryStateZip.Domain;
using Nop.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MWT.Tax.FixedOrByCountryStateZip.Services
{
    public partial interface ITaxLogService
    {
        Task InsertLog(MWTTaxZarTransactionLog log);
        Task<IPagedList<MWTTaxZarTransactionLog>> Getlogs(int orderId, int pageIndex = 0, int pageSize = int.MaxValue);
    }
}
