using MWT.Plugin.Misc.MwtStorefront.Models.Catalog;
using Nop.Web.Framework.Models;
using Nop.Web.Models.Catalog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MWT.Plugin.Misc.MwtStorefront.SearchBox
{
    public record SearchDefaultAutoCompleteModel : BaseNopEntityModel
    {
        public SearchDefaultAutoCompleteModel()
        {
            RecentlyViewedProducts = new List<CustomProductOverviewModel>();
            RecentSearchTerms = new List<string>();
        }
        public List<CustomProductOverviewModel> RecentlyViewedProducts { get; set; }
        public List<string> RecentSearchTerms { get; set; }
    }
}
