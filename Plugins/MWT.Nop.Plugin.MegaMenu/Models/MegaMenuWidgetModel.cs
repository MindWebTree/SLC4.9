using MWT.Nop.Plugin.MegaMenu.Areas.Admin.Models;
using System;

using System.Collections.Generic;

namespace MWT.Nop.Plugin.MegaMenu.Models
{
    public class MegaMenuWidgetModel
    {
        public MegaMenuWidgetModel() => this.MegaMenus = (IList<MenuModel>)new List<MenuModel>();

        public IList<MenuModel> MegaMenus { get; set; }

        public string Theme { get; set; }
    }
}
