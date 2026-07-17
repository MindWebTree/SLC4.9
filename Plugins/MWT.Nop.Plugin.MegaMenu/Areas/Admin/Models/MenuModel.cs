
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;
using System.Collections.Generic;
using System.Text;


#nullable enable
namespace MWT.Nop.Plugin.MegaMenu.Areas.Admin.Models
{
    public record MenuModel : BaseNopEntityModel
    {
        public MenuModel()
        {
            this.SupportedWidgetZones = (IList<SelectListItem>)new List<SelectListItem>();
            this.PredefinedPages = (IList<SelectListItem>)new List<SelectListItem>();
            this.Items = (IList<MenuItemModel>)new List<MenuItemModel>();
            this.Settings = new MegaMenuSettingsModel();
            this.MappingToStores = new StoreMappingModel();
        }

        [NopResourceDisplayName("MWT.MegaMenu.Admin.Menu.Enabled")]
        public bool Enabled { get; set; }

        [NopResourceDisplayName("MWT.MegaMenu.Admin.Menu.Name")]
        public string Name
        { get; set; }

        [NopResourceDisplayName("MWT.MegaMenu.Admin.Menu.CssClass")]
        public string CssClass { get; set; }

        [NopResourceDisplayName("MWT.MegaMenu.Admin.Menu.ShowDropdownsOnClick")]
        public bool ShowDropdownsOnClick { get; set; }

        [NopResourceDisplayName("MWT.MegaMenu.Admin.Menu.WidgetZone")]
        public string WidgetZone { get; set; }

        public bool ShowAclDisabledWarning { get; set; }

        public IList<SelectListItem> SupportedWidgetZones { get; set; }

        public IList<SelectListItem> PredefinedPages { get; set; }

        public IList<MenuItemModel> Items { get; set; }

        public MegaMenuSettingsModel Settings { get; set; }

        public StoreMappingModel MappingToStores { get; set; }

        [NopResourceDisplayName("Admin.Catalog.Products.Fields.LimitedToStores")]
        public bool LimitedToStores { get; set; }

    }
}
