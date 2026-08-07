using MWT.Plugin.Misc.MwtStorefront.Models.Catalog;
using Nop.Web.Models.Catalog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MWT.Plugin.Misc.MwtStorefront.Models.KW
{
    public record class KwTermModel : CategoryModel
    {
        public KwTermModel()
        {
            CatalogProductsModel = new CustomCatalogProductsModel();
        }
        public new IList<CustomProductOverviewModel> FeaturedProducts = new List<CustomProductOverviewModel>();
        public string GridLineViewPath { get; set; }
        public string FilterViewPath { get; set; }
        public string FilterViewPathForMobile { get; set; }
        public bool EnableInfiniteScroll { get; set; }
        public new CustomCatalogProductsModel CatalogProductsModel { get; set; }

        public IList<KwTermModel> KwTermBreadcrumb { get; set; }
    }
}
