using MWT.Nop.Core.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MWT.Nop.Core.Services.Catalog
{
    public partial interface IBestsellerPoolService
    {
        /// <summary>
        /// Rebuild the bestseller pool
        /// Max 5 products per category
        /// Combined and ranked by order count then price
        /// </summary>
        Task RebuildPoolAsync();

        /// <summary>
        /// Get all products in the bestseller pool ordered by DisplayOrder
        /// </summary>
        Task<IList<BestsellerPool>> GetPoolProductsAsync();
    }
}
