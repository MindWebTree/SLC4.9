using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nop.Web.Areas.Admin.Models.Customization.Utilities
{
    public partial record StoreWideDiscountSettingModel : BaseNopEntityModel
    {
        public StoreWideDiscountSettingModel()
        {
            SelectedCategoryIds = new List<int>();
            AvailableCategories = new List<SelectListItem>();
        }
        [NopResourceDisplayName("Admin.Utilities.Marketing.Offer.Field.SelectedCategoryIds")]
        public IList<int> SelectedCategoryIds { get; set; }
        public string CategoryIds { get; set; }
        public IList<SelectListItem> AvailableCategories { get; set; }

        [NopResourceDisplayName("Admin.Utilities.Marketing.Offer.Field.FullStore")]
        public bool FullStore { get; set; }

        [NopResourceDisplayName("Admin.Utilities.Marketing.Offer.Field.DiscountPercentage")]
        public decimal Discount { get; set; }

        [NopResourceDisplayName("Admin.Utilities.Marketing.Offer.Field.OfferText")]
        public string InfoText { get; set; }

        [NopResourceDisplayName("Admin.Utilities.Marketing.Offer.Field.OfferHelpText")]
        public string InfoHelpText { get; set; }

        [NopResourceDisplayName("Admin.Utilities.Marketing.Offer.Field.StoreWideDiscountId")]
        public int StoreWideDiscountId { get; set; }


        [NopResourceDisplayName("Admin.Utilities.Marketing.Offer.Field.Threshold")]
        public decimal Threshold { get; set; }
        public string ProductIds { get; set; }
    }
}
