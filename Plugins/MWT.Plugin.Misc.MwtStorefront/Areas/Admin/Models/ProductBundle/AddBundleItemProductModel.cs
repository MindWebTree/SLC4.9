using Nop.Web.Framework.Models;
using System.Collections.Generic;

namespace MWT.Plugin.Misc.MwtStorefront.Area.Admin.Models
{
    public partial record AddBundleItemProductModel : BaseNopModel
    {
        public AddBundleItemProductModel()
        {
            SelectedProducts = new Dictionary<int, BundleItemProductQuantityModel>();
        }
        public int BundleId { get; set; }
        public string btnId { get; set; }
        public IDictionary<int, BundleItemProductQuantityModel> SelectedProducts { get; set; }
    }

    public class BundleItemProductQuantityModel
    {
        public int VariantId { get; set; }
        public int Quantity { get; set; }
        public int ProductId { get; set; }
    }
}