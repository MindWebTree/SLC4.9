using MWT.Plugin.Misc.MwtStorefront.Areas.CustomOrder.Models.Orders;
using MWT.Plugin.Misc.MwtStorefront.Models.Common;
using Nop.Web.Framework.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static MWT.Plugin.Misc.MwtStorefront.Models.Api.ApiOrderDetailModel;

namespace MWT.Plugin.Misc.MwtStorefront.Models.Api
{
    
    public partial record CustomOrderApiDetailModel : BaseNopEntityModel
    {
        public CustomOrderApiDetailModel()
        {
            TaxRates = new List<ApiTaxRate>();
            Shipments = new List<ApiShipmentBriefModel>();
            BillingAddress = new AddressExtendedModel();
            ShippingAddress = new AddressExtendedModel();
            PickupAddress = new AddressExtendedModel();
            Items = new List<ItemModel>();
            CustomValues = new Dictionary<string, object>();
        }
        public int CustomerId { get; set; }

        public int ParentOrderID { get; set; }

        public Guid OrderGuid { get; set; }
        public string PurchaseOrderNumber { get; set; }
        public string Email { get; set; }
        public string CustomOrderNumber { get; set; }

        public DateTime CreatedOn { get; set; }

        public string OrderStatus { get; set; }

        public string OrderType { get; set; }
        public string SubOrderType { get; set; }

        public bool IsCustomorder { get; set; }
        public AddressExtendedModel PickupAddress { get; set; }
        public string ShippingStatus { get; set; }
        public AddressExtendedModel ShippingAddress { get; set; }
        public string ShippingMethod { get; set; }
        public IList<ApiShipmentBriefModel> Shipments { get; set; }

        public AddressExtendedModel BillingAddress { get; set; }
        public string VatNumber { get; set; }
        public string PaymentMethod { get; set; }
        public string PaymentMethodStatus { get; set; }
        public bool CanRePostProcessPayment { get; set; }
        public Dictionary<string, object> CustomValues { get; set; }

        public DateTime? PromiseDate { get; set; }
        public string CheckoutAttributeInfo { get; set; }
        public IList<ApiTaxRate> TaxRates { get; set; }
        public string CreditCardNumber { get; set; }

        public string TransactionId { get; set; }
        public string CustomerCurrencyCode { get; set; }
        public ApiCustomOrderSummaryModel CustomOrderSummaryModel { get; set; }

        public bool TaxExempted { get; set; }
        public List<ItemModel> Items { get; set; }
        #region Nested Classes



        #endregion
        public string PairedOrderIds { get; set; }
    }



}
