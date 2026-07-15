using Nop.Data;
using MWT.Tax.FixedOrByCountryStateZip.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Domain.Customers;

namespace MWT.Tax.FixedOrByCountryStateZip.Services
{
    public partial class TaxLogService : ITaxLogService
    {

        #region Fields

        private readonly IRepository<MWTTaxZarTransactionLog> _taxZarTransactionLogRepository;

        #endregion


        #region Ctor

        public TaxLogService(IRepository<MWTTaxZarTransactionLog> taxZarTransactionLogRepository)
        {
            this._taxZarTransactionLogRepository = taxZarTransactionLogRepository;
        }

        #endregion

        #region Methods
        public async Task InsertLog(MWTTaxZarTransactionLog log)
        {
            await this._taxZarTransactionLogRepository.InsertAsync(log);
        }


        public virtual async Task<IPagedList<MWTTaxZarTransactionLog>> Getlogs(int orderId, int pageIndex = 0, int pageSize = int.MaxValue)
        {
            var query = _taxZarTransactionLogRepository.Table;

            //filter by customer
            if (orderId != 0)
                query = query.Where(logItem => logItem.OrderId == orderId);


            query = query.OrderByDescending(logItem => logItem.CreatedDateUtc).ThenByDescending(logItem => logItem.Id);
            //return paged log
            return await query.ToPagedListAsync(pageIndex, pageSize);
        }
        #endregion
    }
}
