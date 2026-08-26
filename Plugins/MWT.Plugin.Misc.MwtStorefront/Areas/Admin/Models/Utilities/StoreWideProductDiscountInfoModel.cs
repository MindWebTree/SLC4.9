using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace MWT.Plugin.Misc.MwtStorefront.Areas.Admin.Models.Utilities
{
    public partial record StoreWideProductDiscountInfoModel : BaseNopEntityModel
    {
        [NopResourceDisplayName("Admin.Utilities.Marketing.Offer.Log.Field.ProductId")]
        public int ProductId { get; set; }

        [NopResourceDisplayName("Admin.Utilities.Marketing.Offer.Log.Field.Discount")]
        public decimal Discount { get; set; }

        [NopResourceDisplayName("Admin.Utilities.Marketing.Offer.Log.Field.InfoText")]
        public string InfoText { get; set; }

        [NopResourceDisplayName("Admin.Utilities.Marketing.Offer.Log.Field.InfoHelpText")]
        public string InfoHelpText { get; set; }

        [NopResourceDisplayName("Admin.Utilities.Marketing.Offer.Log.Field.CreatedOn")]
        public DateTime? CreatedOn { get; set; }


        [NopResourceDisplayName("Admin.Utilities.Marketing.Offer.Log.Field.UpdatedOn")]
        public DateTime? UpdatedOn { get; set; }
    }
}
