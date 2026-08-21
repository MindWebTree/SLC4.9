using MWT.Plugin.Misc.MwtStorefront.Models.Api;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace MWT.Plugin.Misc.MwtStorefront.Areas.CustomOrder.Models.Orders
{
    public record ApiCustomOrderSummaryModel : BaseNopEntityModel
    {
        
        public ApiCustomOrderSummaryModel()
        {

            TaxInfo = new List<TaxInfoModel>();
        }
        [NopResourceDisplayName("CustomOrder.Order.Fields.SubTotal")]
        public decimal SubTotal { get; set; }

        [NopResourceDisplayName("CustomOrder.Order.Fields.OrderTotal")]
        public decimal OrderTotal { get; set; }

        [NopResourceDisplayName("CustomOrder.Order.Fields.PayableAmount")]
        public decimal PayableAmount { get; set; }

        [NopResourceDisplayName("CustomOrder.Order.Fields.IntialPayment")]
        public decimal InitialPayment { get; set; }

        [NopResourceDisplayName("CustomOrder.Order.Fields.OrderType")]
        public string OrderType { get; set; }


        [NopResourceDisplayName("CustomOrder.Order.Fields.OrderType")]
        public string OrderStatus { get; set; }

        [NopResourceDisplayName("CustomOrder.Order.Fields.PendingPayment")]
        public decimal PendingPayment { get; set; }

        [NopResourceDisplayName("CustomOrder.Order.Fields.HouzzFee")]
        public decimal HouzzFee { get; set; }

        [NopResourceDisplayName("CustomOrder.Order.Fields.SubTotalDiscount")]
        public decimal SubTotalDiscount { get; set; }

        [NopResourceDisplayName("CustomOrder.Order.Fields.SubTotalDiscount")]
        public ApiDiscountDetails SubTotalDiscountDetails { get; set; }

        [NopResourceDisplayName("CustomOrder.Order.Fields.Shipping")]
        public decimal Shipping { get; set; }

        [NopResourceDisplayName("CustomOrder.Order.Fields.ShippingDiscount")]
        public decimal ShippingDiscount { get; set; }

        public ApiDiscountDetails ShippingDiscountDetails { get; set; }


        [NopResourceDisplayName("CustomOrder.Order.Fields.Wgs")]
        public decimal Wgs { get; set; }


        public bool ComplementryWgsFree { get; set;  }

        public decimal NormalWgsCharges { get; set; }

        [NopResourceDisplayName("CustomOrder.Order.Fields.WgsAdjustmentNotes")]
        public string WgsAdjustmentNotes { get; set; }

        [NopResourceDisplayName("CustomOrder.Order.Fields.Tax")]
        public decimal Tax { get; set; }


        [NopResourceDisplayName("CustomOrder.Order.Fields.TaxInfo")]
        public List<TaxInfoModel> TaxInfo { get; set; }

        [NopResourceDisplayName("CustomOrder.Order.Fields.CustomDuty")]
        public decimal CustomDuty { get; set; }

        [NopResourceDisplayName("CustomOrder.Order.Fields.CustomDutyPercentage")]
        public decimal CustomDutyPercentage { get; set; }

        [NopResourceDisplayName("CustomOrder.Order.Fields.PrivateNotes")]
        public string PrivateNotes { get; set; }

        [NopResourceDisplayName("CustomOrder.Order.Fields.InvoiceNotes")]
        public string InvoiceNotes { get; set; }

        [NopResourceDisplayName("CustomOrder.Order.Fields.ShippingMethod")]
        public string ShippingMethod { get; set; }



     

        public ApiPaymentDetails PaymentDetails { get; set; }

        public bool FullPaid { get; set; }
        public bool IsOrderExpired{get;set;}

        public bool ApplyTax { get; set; }
    }

    public class ApiDiscountDetails
    {

        public decimal DiscountAmount { get; set; }

        public decimal TotalAdjustment { get; set; }
        public string ChargeType { get; set; }
        public string DiscountType { get; set; }
        public string Notes { get; set; }

    
    }

    public class ApiPaymentDetails
    {
        public string PaymentMethod { get; set; }
        public string PaidBy { get; set; }
        public int LiveOrderNumber { get; set; }
        public DateTime PaymentDate { get; set; }

        public string TransactionId { get; set; }

        public string Card { get; set; }
    }
}
