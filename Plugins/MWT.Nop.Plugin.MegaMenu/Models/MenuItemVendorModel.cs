
using MWT.Nop.Plugin.MegaMenu.Areas.Admin.Models;
using Nop.Web.Models.Catalog;
using System.Collections.Generic;

namespace MWT.Nop.Plugin.MegaMenu.Models
{
    public class MenuItemVendorModel
    {
        public MenuItemVendorModel() => this.Vendors = (IList<VendorModel>)new List<VendorModel>();

        public IList<VendorModel> Vendors { get; set; }

        public MenuItemModel Item { get; set; }

        public bool ShouldShowViewAllLink { get; set; }
    }
}
