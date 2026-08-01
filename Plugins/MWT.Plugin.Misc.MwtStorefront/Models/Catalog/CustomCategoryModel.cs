using Nop.Web.Models.Catalog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MWT.Plugin.Misc.MwtStorefront.Models.Catalog
{
    public record CustomCategoryModel : CategoryModel
    {
        public string QuickFilterHeading { get; set; }
        public string GridLineViewPath { get; set; }
        public string FilterViewPath { get; set; }
        public string FilterViewPathForMobile { get; set; }
        public bool EnableInfiniteScroll { get; set; }
        public bool DisplayGridListOption { get; set; }
        public bool FeaturedListingDisplaySimilarOnTop { get; set; }
        public string AdditionalDescription { get; set; }
    }
}
