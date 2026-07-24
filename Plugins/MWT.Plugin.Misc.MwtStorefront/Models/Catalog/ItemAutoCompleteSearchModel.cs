using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MWT.Plugin.Misc.MwtStorefront.Catalog
{
    public record ItemAutoCompleteSearchModel
    {
        public string Name { get; set; }
        public string Url { get; set; }
        public string Header { get; set; }
        public bool IsRecentSearch { get; set; }
        public List<SearchRecentlyViewedProduct> Products { get; set; }
    }
    public class SearchRecentlyViewedProduct
    {
        public string Name { get; set; }
        public string ImageUrl { get; set; }
        public string Url { get; set; }
 
    }
}
