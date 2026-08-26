using Nop.Web.Framework.Mvc.ModelBinding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nop.Web.Areas.Admin.Models.Customization.Utilities
{
    public record MarketingModel
    {
        [NopResourceDisplayName("Admin.Utilities.Marketing.Field.EnableBuyMoreSaveMoreDiscount")]
        public bool EnableBuyMoreSaveMoreDiscount { get; set; }
        [NopResourceDisplayName("Admin.Utilities.Marketing.Field.BuyMoreSaveMoreDiscountConfiguration")]
        public string BuyMoreSaveMoreDiscountConfiguration { get; set; }

        [NopResourceDisplayName("Admin.Utilities.Marketing.Field.ShowBuyMoreSaveMoreBanner")]
        public bool ShowBuyMoreSaveMoreBanner { get; set; }
        [NopResourceDisplayName("Admin.Utilities.Marketing.Field.HeaderStripContent")]
        public string HeaderStripContent { get; set; }

        [NopResourceDisplayName("Admin.Utilities.Marketing.Field.LimitedOfferContent")]
        public string LimitedOfferContent { get; set; }

        [NopResourceDisplayName("Admin.Utilities.Marketing.Field.BuyMoreSaveMoreContent")]
        public string BuyMoreSaveMoreContent { get; set; }

        [NopResourceDisplayName("Admin.Utilities.Marketing.Field.EnableEmailExclusiveOffer")]
        public bool EnableEmailExclusiveOffer { get; set; }
    }
}
