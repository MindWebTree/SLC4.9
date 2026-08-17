using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MWT.Nop.Core.Services.Catalog
{
    public partial interface INewArrivalTagService
    {
        /// <summary>
        /// Assign New Arrival tag to products launched within 180 days
        /// Remove from products older than 180 days
        /// </summary>
        Task ProcessTagsAsync();

        /// <summary>
        /// Recalculate QueueId for New Arrival products
        /// Latest launched = QueueId 1
        /// Dual tag products always at top
        /// </summary>
        Task RecalculateQueueAsync();
    }
}
