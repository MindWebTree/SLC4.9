using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nop.Web.Areas.Admin.Models.Customization.Utilities
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
