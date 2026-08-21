using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;
using Nop.Web.Models.Checkout;

namespace MWT.Plugin.Misc.MwtStorefront.Areas.CustomOrder.Models.Orders
{

    public partial record CustomOrderSearchModel : BaseSearchModel
    {
        public CustomOrderSearchModel()
        {
            orderStatuses = new List<SelectListItem>();
            PaymentMethods = new List<CheckoutPaymentMethodModel.PaymentMethodModel>();
            DisplayAdditionalService = false;
        }

        #region Properties

        [NopResourceDisplayName("CustomOrder.List.Search")]
        public string SearchTerm { get; set; }

        [NopResourceDisplayName("CustomOrder.List.CustomerId")]
        public int CustomerId { get; set; }
        public IList<SelectListItem> orderStatuses { get; set; }

        [NopResourceDisplayName("CustomOrder.List.Search.StatusId")]
        public int searchStatusId { get; set; }

        public IList<CheckoutPaymentMethodModel.PaymentMethodModel> PaymentMethods { get; set; }

        public bool DisplayAdditionalService { get; set; }

        public bool IsArchived { get; set; }
        #endregion
    }
}
