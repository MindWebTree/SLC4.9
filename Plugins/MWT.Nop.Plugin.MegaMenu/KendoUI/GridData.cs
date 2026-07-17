
using System.Collections.Generic;

namespace MWT.Nop.Plugin.MegaMenu.KendoUI
{
    public class GridData
    {
        public GridFilters Filter { get; set; }
        public IList<GridSort> Sort { get; set; }
        public int PageSize { get; set; }

        public int Page { get; set; }
    }
}

