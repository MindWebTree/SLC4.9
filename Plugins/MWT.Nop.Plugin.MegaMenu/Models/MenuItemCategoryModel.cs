
using MWT.Nop.Plugin.MegaMenu.Areas.Admin.Models;
using System.Collections.Generic;

namespace MWT.Nop.Plugin.MegaMenu.Models
{
    public class MenuItemCategoryModel
    {
        public MenuItemCategoryModel() => this.Categories = (IList<MegaMenuCategoryModel>)new List<MegaMenuCategoryModel>();

        public MenuItemModel Item { get; set; }

        public IList<MegaMenuCategoryModel> Categories { get; set; }

        public int CategoryMenuItemIndex { get; set; }
    }
}
