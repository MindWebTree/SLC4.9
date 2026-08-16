using MWT.Nop.Core.Domain.Feed;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MWT.Nop.Core.Services.Feed
{
    public partial interface IGoogleFeedService
    {
        Task<GoogleCategory> GetFeedCategoryAsync(int categoryId, string productType);
    }
}
