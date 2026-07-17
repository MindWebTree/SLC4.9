using Microsoft.AspNetCore.Mvc.Rendering;
using MWT.Nop.Plugin.MegaMenu.Domain.Enums;
using Nop.Web.Framework.Models;
using System;
using System.Collections.Generic;
using System.Text;



namespace MWT.Nop.Plugin.MegaMenu.Areas.Admin.Models
{
    public record MenuItemModel :
      BaseNopEntityModel,
      ILocalizedModel<MenuItemLocalizedModel>,
      ILocalizedModel
    {
        public MenuItemModel()
        {
            this.SubItems = (IList<MenuItemModel>)new List<MenuItemModel>();
            this.Locales = (IList<MenuItemLocalizedModel>)new List<MenuItemLocalizedModel>();
            this.SelectedCustomerRoleIds = (IList<int>)new List<int>();
            this.AvailableCustomerRoles = (IList<SelectListItem>)new List<SelectListItem>();
        }

        public MenuItemType Type { get; set; }

        public string TypeName { get; set; }

        public string Title { get; set; }

        public string Url { get; set; }

        public bool OpenInNewWindow { get; set; }

        public int DisplayOrder { get; set; }

        public string CssClass { get; set; }

        public int MaximumNumberOfEntities { get; set; }

        public int NumberOfBoxesPerRow { get; set; }

        public CatalogTemplate CatalogTemplate { get; set; }

        public int ImageSize { get; set; }
        public string ImageUrl { get; set; }
        public int EntityId { get; set; }

        public int PictureId { get; set; }

        public string WidgetZone { get; set; }

        public Decimal Width { get; set; }

        public int ParentMenuItemId { get; set; }

        public int? MenuId { get; set; }

        public int DepthLevel { get; set; }

        public bool IncludeInTopMenu { get; set; }

        public IList<MenuItemModel> SubItems { get; set; }

        public IList<MenuItemLocalizedModel> Locales { get; set; }

        public IList<int> SelectedCustomerRoleIds { get; set; }

        public IList<SelectListItem> AvailableCustomerRoles { get; set; }

        public bool IsMobile { get; set; }
     
    }
}
