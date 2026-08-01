using MWT.Nop.Core;
using Nop.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nop.Data.Customization.Extensions
{
    public static class CustomAsyncIQueryableExtensions
    {
        public static async Task<IPagedList<T>> CustomToPagedListAsync<T>(this IQueryable<T> source, int pageIndex, int pageSize, int firstPageSize, int subsequentPageSize, bool getOnlyTotalCount = false)
        {
            if (source == null)
                return new PagedList<T>(new List<T>(), pageIndex, pageSize);

            // Min allowed page size is 1
            pageSize = Math.Max(pageSize, 1);
            firstPageSize = Math.Max(firstPageSize, 1);
            subsequentPageSize = Math.Max(subsequentPageSize, 1);

            var count = await source.CountAsync();
            var data = new List<T>();

            if (!getOnlyTotalCount)
            {
                int skip;
                int take;

                if (pageIndex == 0)
                {
                    // First page: start at 0, take 16
                    skip = 0;
                    take = firstPageSize;
                }
                else
                {
                    // Page 1+: skip first page's 16, then skip (pageIndex - 1) * 9
                    skip = firstPageSize + (pageIndex - 1) * subsequentPageSize;
                    take = subsequentPageSize;
                }

                data.AddRange(await source.Skip(skip).Take(take).ToListAsync());
            }

            // Pass the effective page size for the current page into PagedList
            int effectivePageSize = pageIndex == 0 ? firstPageSize : subsequentPageSize;

            return new CustomPagedList<T>(data, pageIndex, pageSize, firstPageSize, subsequentPageSize, count);
        }
    }
}