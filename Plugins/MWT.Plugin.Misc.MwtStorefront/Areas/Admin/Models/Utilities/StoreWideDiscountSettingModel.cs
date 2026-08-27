using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace MWT.Plugin.Misc.MwtStorefront.Areas.Admin.Models.Utilities
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
