using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;
using Nop.Web.Framework.Validators;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MWT.Nop.Plugin.Widgets.Catalog.Models
{ 
    public record MWTEntityBannerModel : BaseNopEntityModel
    { 
        public MWTEntityBannerModel()
        {
            AvailableCategories = new List<SelectListItem>();
            AvailableWidgetZones = new List<SelectListItem>();
            AvailableKwTerms = new List<SelectListItem>();
            EntityTypes = new List<SelectListItem>();
            EntityTypes.Add(new SelectListItem()
            {
                Text="Category",
                Value="Category"
            });
            EntityTypes.Add(new SelectListItem()
            {
                Text = "KwTerm",
                Value = "KwTerm"
            });
            EntityIds=new List<int>();
        }


        public int? EntityId { get; set; }

        [NopResourceDisplayName("MWT.Plugins.Widgets.Catalog.Fields.EntityId")]
    
        public IList<int> EntityIds { get; set; }

        [NopResourceDisplayName("MWT.Plugins.Widgets.Catalog.Fields.EntityName")]
        public string EntityName { get; set; }

        [NopResourceDisplayName("MWT.Plugins.Widgets.Catalog.Fields.EntityType")] 
        public string EntityType { get; set; }

        [NopResourceDisplayName("MWT.Plugins.Widgets.Catalog.Fields.WidgetZone")] 
        public string WidgetZone { get; set; }

        [NopResourceDisplayName("MWT.Plugins.Widgets.Catalog.Fields.ActionLink")]
        public string ActionLink { get; set; }

        [UIHint("Picture")]
        [NopResourceDisplayName("MWT.Plugins.Widgets.Catalog.Fields.BannerId")]
        public int BannerId { get; set; }

        [NopResourceDisplayName("MWT.Plugins.Widgets.Catalog.Fields.Html")]
        public string Html { get; set; }


        [NopResourceDisplayName("MWT.Plugins.Widgets.Catalog.Fields.MobileActionLink")]
        public string MobileActionLink { get; set; }

        [UIHint("Picture")]
        [NopResourceDisplayName("MWT.Plugins.Widgets.Catalog.Fields.MobileBannerId")]
        public int MobileBannerId { get; set; }

        [NopResourceDisplayName("MWT.Plugins.Widgets.Catalog.Fields.VideoUrl")]
        public string VideoUrl { get; set; }

        [NopResourceDisplayName("MWT.Plugins.Widgets.Catalog.Fields.MobileVideoUrl")]
        public string MobileVideoUrl { get; set; }

        [NopResourceDisplayName("MWT.Plugins.Widgets.Catalog.Fields.MobileHtml")]
        public string MobileHtml { get; set; }

        public IList<SelectListItem> AvailableCategories { get; set; }

        public IList<SelectListItem> AvailableKwTerms { get; set; }

        public IList<SelectListItem> EntityTypes { get; set; }
        public IList<SelectListItem> AvailableWidgetZones { get; set; }

        public bool IsMobileHtml { get; set; }

        public bool IsHtml { get; set; }

        public string bannerImage { get; set; }
        public string MobilebannerImage { get; set; }

        public string Title { get; set; }

        public string BannerType { get; set; }
        public string MobileBannerType { get; set; }
    }
}
