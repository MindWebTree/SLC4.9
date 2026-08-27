using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace MWT.Plugin.Misc.MwtStorefront.Areas.Admin.Models.Utilities
{
    public partial record StoreWideProductDiscountHistoryModel : BaseNopEntityModel
    {
        [NopResourceDisplayName("Admin.Utilities.Marketing.Offer.Log.History.Field.ProductId")]
        public int ProductId { get; set; }
        [NopResourceDisplayName("Admin.Utilities.Marketing.Offer.Log.History.Field.Discount")]
        public decimal Discount { get; set; }

        [NopResourceDisplayName("Admin.Utilities.Marketing.Offer.Log.History.Field.InfoText")]
        public string InfoText { get; set; }
        [NopResourceDisplayName("Admin.Utilities.Marketing.Offer.Log.History.Field.InfoHelpText")]
        public string InfoHelpText { get; set; }
        [NopResourceDisplayName("Admin.Utilities.Marketing.Offer.Log.History.Field.CreatedOn")]
        public DateTime? CreatedOn { get; set; }
        [NopResourceDisplayName("Admin.Utilities.Marketing.Offer.Log.History.Field.UpdatedBy")]
        public string UpdatedBy { get; set; }

        [NopResourceDisplayName("Admin.Utilities.Marketing.Offer.Log.History.Field.StoreWideDiscountName")]
        public string Name { get; set; }
    }
}
