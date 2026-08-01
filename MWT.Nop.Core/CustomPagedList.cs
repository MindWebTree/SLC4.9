using Nop.Core;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MWT.Nop.Core
{
    /// <summary>
    /// Paged list
    /// </summary>
    /// <typeparam name="T">T</typeparam>
    [Serializable]
    public class CustomPagedList<T> : List<T>, IPagedList<T>
    {
        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="source">source</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <param name="totalCount">Total count</param>
        public CustomPagedList(IList<T> source, int pageIndex, int pageSize, int firstPageSize, int subsequentPageSize, int? totalCount = null)
        {
            // Min allowed page size is 1
            pageSize = Math.Max(pageSize, 1);
            firstPageSize = Math.Max(firstPageSize, 1);
            subsequentPageSize = Math.Max(subsequentPageSize, 1);

            TotalCount = totalCount ?? source.Count;

            // Calculate TotalPages accounting for different first page size
            if (TotalCount <= firstPageSize)
            {
                TotalPages = 1;
            }
            else
            {
                TotalPages = 1 + (int)Math.Ceiling((TotalCount - firstPageSize) / (double)subsequentPageSize);
            }

            PageSize = pageSize;
            PageIndex = pageIndex;

            if (totalCount != null)
            {
                // Data already sliced externally (from ToPagedListAsync)
                AddRange(source);
            }
            else
            {
                // Fallback: slice here if no totalCount provided
                int skip = pageIndex == 0
                    ? 0
                    : firstPageSize + (pageIndex - 1) * subsequentPageSize;

                int take = pageIndex == 0 ? firstPageSize : subsequentPageSize;

                AddRange(source.Skip(skip).Take(take));
            }
        }

        /// <summary>
        /// Page index
        /// </summary>
        public int PageIndex { get; }

        /// <summary>
        /// Page size
        /// </summary>
        public int PageSize { get; }

        /// <summary>
        /// Total count
        /// </summary>
        public int TotalCount { get; }

        /// <summary>
        /// Total pages
        /// </summary>
        public int TotalPages { get; }

        /// <summary>
        /// Has previous page
        /// </summary>
        public bool HasPreviousPage => PageIndex > 0;

        /// <summary>
        /// Has next page
        /// </summary>
        public bool HasNextPage => PageIndex + 1 < TotalPages;
    }
}