
using MWT.Nop.Plugin.MegaMenu.Areas.Admin.Models;
using Nop.Web.Models.Catalog;
using System.Collections.Generic;

namespace MWT.Nop.Plugin.MegaMenu.Models
{
    public class MenuItemProductTagsModel
    {
        public MenuItemProductTagsModel() => this.ProductTags = (IList<ProductTagModel>)new List<ProductTagModel>();

        public IList<ProductTagModel> ProductTags { get; set; }

        public MenuItemModel Item { get; set; }

        public bool ShouldShowViewAllLink { get; set; }
    }
}
