
using MWT.Nop.Plugin.MegaMenu.Areas.Admin.Models;
using Nop.Web.Models.Catalog;
using System.Collections.Generic;

namespace MWT.Nop.Plugin.MegaMenu.Models
{
    public class MenuItemManufacturerModel
    {
        public MenuItemManufacturerModel() => this.Manufacturers = (IList<ManufacturerModel>)new List<ManufacturerModel>();

        public IList<ManufacturerModel> Manufacturers { get; set; }

        public MenuItemModel Item { get; set; }

        public bool ShouldShowViewAllLink { get; set; }
    }
}
