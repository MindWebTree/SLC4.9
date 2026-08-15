using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MWT.Nop.Core.Services.Catalog
{
    public interface IBestsellerTagService
    {
        /// <summary>
        /// Recalculate Bestseller tags across all categories
        /// Min 3 orders in 180 days to qualify
        /// Max 7 per category
        /// Ranked by order count then price
        /// </summary>
        Task ProcessTagsAsync();

        /// <summary>
        /// Recalculate QueueId for Bestseller products
        /// Most orders = QueueId 1
        /// Tie broken by higher price
        /// Dual tag products always at top
        /// </summary>
        Task RecalculateQueueAsync();
    }
}
