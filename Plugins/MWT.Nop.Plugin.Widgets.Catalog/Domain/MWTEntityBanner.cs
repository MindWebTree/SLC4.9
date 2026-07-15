using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Core;
using Nop.Web.Framework.Mvc.ModelBinding;
namespace MWT.Nop.Plugin.Widgets.Catalog.Domain
{
    public partial class MWTEntityBanner : BaseEntity
    {
        public string EntityType { get; set; }
        public string WidgetZone { get; set; }
        public string ActionLink { get; set; }
        public int BannerId { get; set; }
        public string Html { get; set; }
        public string MobileActionLink { get; set; }
        public int MobileBannerId { get; set; }
        public string MobileHtml { get; set; }
        public string VideoUrl { get; set; }
        public string MobileVideoUrl { get; set; }
    }
}
