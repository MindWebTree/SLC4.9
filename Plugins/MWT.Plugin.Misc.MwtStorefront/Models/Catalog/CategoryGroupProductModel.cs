using Nop.Web.Models.Catalog;
using System.Collections.Generic;

namespace MWT.Plugin.Misc.MwtStorefront.Models.Catalog
{
    public partial record CategoryGroupProductModel
    {
        public CategoryGroupProductModel()
        {
            GroupProducts = new List<GroupProduct>();
        }
        public List<GroupProduct> GroupProducts { get; set; }
    }

    public partial record GroupProduct
    {
        public GroupProduct()
        {
            AssociatedProducts = new List<ProductOverviewModel>();
        }
        public ProductOverviewModel Product { get; set; }
        public List<ProductOverviewModel> AssociatedProducts { get; set; }
    }
}
