
using Nop.Core;
using Nop.Core.Domain.Localization;
using Nop.Core.Domain.Security;
using MWT.Nop.Plugin.MegaMenu.Domain.Enums;
using System;

namespace MWT.Nop.Plugin.MegaMenu.Domain
{
    public class MenuItem : BaseEntity, ILocalizedEntity, IAclSupported
    {
        public MenuItemType Type { get; set; }

        public string Title { get; set; }

        public string Url { get; set; }

        public bool OpenInNewWindow { get; set; }

        public int DisplayOrder { get; set; }

        public string CssClass { get; set; }

        public int MaximumNumberOfEntities { get; set; }

        public int NumberOfBoxesPerRow { get; set; }

        public CatalogTemplate CatalogTemplate { get; set; }

        public int ImageSize { get; set; }

        public int EntityId { get; set; }

        public string WidgetZone { get; set; }

        public Decimal Width { get; set; }

        public int ParentMenuItemId { get; set; }

        public int? MenuId { get; set; }

        public bool SubjectToAcl { get; set; }

        public int PictureId { get;set; }

        public ImageType ImageType { get; set; }
    }
}
