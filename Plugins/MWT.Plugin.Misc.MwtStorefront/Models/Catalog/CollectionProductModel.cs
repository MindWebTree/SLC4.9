using Nop.Web.Models.Catalog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MWT.Plugin.Misc.MwtStorefront.Models.Catalog
{
    public record CollectionProductModel
    {
        public CollectionProductModel()
        {
            Products = new List<CustomProductOverviewModel>();
        }
        public List<CustomProductOverviewModel> Products { get; set; }

        public CustomProductOverviewModel MainCollection { get; set; }
        public bool IsCollectionPage { get; set; }
    }

    public record CollectionDetailProductModel
    {
        public CollectionDetailProductModel()
        {
            Products = new List<CustomProductDetailsModel>();
        }
        public CustomProductOverviewModel MainCollection { get; set; }
        public List<CustomProductDetailsModel> Products { get; set; }
        public bool IsCollectionPage { get; set; }
    }
}
