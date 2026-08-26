using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace MWT.Plugin.Misc.MwtStorefront.Areas.Admin.Models.Utilities
{

    public partial record StoreWideDiscountModel : BaseNopEntityModel
    {
        [NopResourceDisplayName("Admin.Utilities.Marketing.Offer.Field.Name")]
        public string Name { get; set; }

        [NopResourceDisplayName("Admin.Utilities.Marketing.Offer.Field.StartDate")]
        public DateTime StartDate { get; set; } = DateTime.Today;

        [NopResourceDisplayName("Admin.Utilities.Marketing.Offer.Field.EndDate")]
        public DateTime EndDate { get; set; } = DateTime.Today;

        [NopResourceDisplayName("Admin.Utilities.Marketing.Offer.Field.IsProcessed")]
        public bool IsProcessed { get; set; }

        [NopResourceDisplayName("Admin.Utilities.Marketing.Offer.Field.Published")]
        public bool Published { get; set; }

        [NopResourceDisplayName("Admin.Utilities.Marketing.Offer.Field.CreatedBy")]
        public string Created_By { get; set; }

        [NopResourceDisplayName("Admin.Utilities.Marketing.Offer.Field.CreatedOn")]
        public DateTime CreatedOn { get; set; }

        [NopResourceDisplayName("Admin.Utilities.Marketing.Offer.Field.UpdatedOn")]
        public DateTime UpdatedOn { get; set; }
        public StoreWideDiscountSettingSearchModel StoreWideDiscountSettingSearchModel { get; set; }
    }
}
