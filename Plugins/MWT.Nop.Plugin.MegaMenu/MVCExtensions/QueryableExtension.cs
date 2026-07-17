using MWT.Nop.Plugin.MegaMenu.KendoUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;

namespace MWT.Nop.Plugin.MegaMenu.MVCExtensions
{
    public static class QueryableExtension
    {
        public static IQueryable<T> GridFilter<T, F>(this IQueryable<T> queryable, F filter) where F : GridFilters
        {
            if ((object)filter != null && filter.Logic != null && filter.Filters != null && filter.Filters.Count > 0)
            {
                IList<MWT.Nop.Plugin.MegaMenu.KendoUI.GridFilter> filters = filter.Filters;
                IList<object> values;
                string expression = filter.ToExpression(filters, out values);
                queryable = queryable.Where<T>(expression, values.ToArray<object>());
            }
            return queryable;
        }

        public static IQueryable<T> GridSort<T, S>(
          this IQueryable<T> queryable,
          IEnumerable<S> sort)
          where S : MWT.Nop.Plugin.MegaMenu.KendoUI.GridSort
        {
            if (sort == null || !sort.Any<S>())
                return queryable;
            string ordering = string.Join(",", sort.Select<S, string>((Func<S, string>)(s => s.ToExpression())));
            return (IQueryable<T>)queryable.OrderBy<T>(ordering);
        }

        public static IQueryable<T> GridPaging<T>(
          this IQueryable<T> queryable,
          int page,
          int pageSize,
          out int totalCount)
        {
            totalCount = 0;
            totalCount = Queryable.Count<T>(queryable);
            return Queryable.Take<T>(Queryable.Skip<T>(queryable, (page - 1) * pageSize), pageSize);
        }
    }
}
