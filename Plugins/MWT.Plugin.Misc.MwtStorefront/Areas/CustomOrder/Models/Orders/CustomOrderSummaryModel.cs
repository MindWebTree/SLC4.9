using iTextSharp.text.rtf.parser;
using Nop.Core.Domain.Customers;
using Nop.Services.Tax;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;
using Nop.Web.Models.Checkout;
using OfficeOpenXml.FormulaParsing.Excel.Functions.DateTime;
using System;
using System.Collections.Generic;

namespace MWT.Plugin.Misc.MwtStorefront.Areas.CustomOrder.Models.Orders
{
    public record CustomOrderSummaryModel : BaseNopEntityModel
    {

        public CustomOrderSummaryModel()
        {
            PaymentMethods = new List<CheckoutPaymentMethodModel.PaymentMethodModel>();
            IsOrderExpired = false;
        }
        [NopResourceDisplayName("CustomOrder.Order.Fields.SubTotal")]
        public string SubTotal { get; set; }

        [NopResourceDisplayName("CustomOrder.Order.Fields.OrderTotal")]
        public string OrderTotal { get; set; }

        [NopResourceDisplayName("CustomOrder.Order.Fields.PayableAmount")]
        public string PayableAmount { get; set; }

        [NopResourceDisplayName("CustomOrder.Order.Fields.IntialPayment")]
        public string InitialPayment { get; set; }

        [NopResourceDisplayName("CustomOrder.Order.Fields.OrderType")]
        public string OrderType { get; set; }


        [NopResourceDisplayName("CustomOrder.Order.Fields.OrderType")]
        public string OrderStatus { get; set; }

        [NopResourceDisplayName("CustomOrder.Order.Fields.PendingPayment")]
        public string PendingPayment { get; set; }

        [NopResourceDisplayName("CustomOrder.Order.Fields.HouzzFee")]
        public string HouzzFee { get; set; }

        [NopResourceDisplayName("CustomOrder.Order.Fields.SubTotalDiscount")]
        public string SubTotalDiscount { get; set; }

        [NopResourceDisplayName("CustomOrder.Order.Fields.SubTotalDiscount")]
        public DiscountDetails SubTotalDiscountDetails { get; set; }

        [NopResourceDisplayName("CustomOrder.Order.Fields.Shipping")]
        public string Shipping { get; set; }

        [NopResourceDisplayName("CustomOrder.Order.Fields.ShippingDiscount")]
        public string ShippingDiscount { get; set; }

        public DiscountDetails ShippingDiscountDetails { get; set; }


        [NopResourceDisplayName("CustomOrder.Order.Fields.Wgs")]
        public string Wgs { get; set; }

        [NopResourceDisplayName("CustomOrder.Order.Fields.Complementry.Wgs.Free")]
        public bool ComplementryWgsFree { get; set; }
        public decimal NormalWgsCharges { get; set; }

        [NopResourceDisplayName("CustomOrder.Order.Fields.WgsAdjustmentNotes")]
        public string WgsAdjustmentNotes { get; set; }

        [NopResourceDisplayName("CustomOrder.Order.Fields.Tax")]
        public string Tax { get; set; }

        [NopResourceDisplayName("CustomOrder.Order.Fields.Taxes")]
        public List<TaxInfo> TaxInfo { get; set; }

        [NopResourceDisplayName("CustomOrder.Order.Fields.RawTaxInfo")]
        public string HtmlTaxInfo { get; set; }

        [NopResourceDisplayName("CustomOrder.Order.Fields.CustomDuty")]
        public string CustomDuty { get; set; }

        [NopResourceDisplayName("CustomOrder.Order.Fields.CustomDutyPercentage")]
        public decimal CustomDutyPercentage { get; set; }

        [NopResourceDisplayName("CustomOrder.Order.Fields.PrivateNotes")]
        public string PrivateNotes { get; set; }

        [NopResourceDisplayName("CustomOrder.Order.Fields.InvoiceNotes")]
        public string InvoiceNotes { get; set; }

        [NopResourceDisplayName("CustomOrder.Order.Fields.SpecialInstructionsfromBuyer")]
        public string SpecialInstructionsfromBuyer { get; set; }

        [NopResourceDisplayName("CustomOrder.Order.Fields.ShippingMethod")]
        public string ShippingMethod { get; set; }
        public IList<CheckoutPaymentMethodModel.PaymentMethodModel> PaymentMethods { get; set; }
        public Customer Customer { get; set; }
        public PaymentDetails PaymentDetails { get; set; }
        public bool FullPaid { get; set; }
        public bool IsOrderExpired { get; set; }
        public bool ApplyTax { get; set; }
        public string ZipCode { get; set; }
        public string Email { get; set; }
        public decimal TaxRate { get; set; }
        public int ParentOrderID { get; set; }

        public string Message { get; set; }
    }

    public class DiscountDetails
    {

        public decimal DiscountAmount { get; set; }

        public string TotalAdjustment { get; set; }
        public string ChargeType { get; set; }
        public string DiscountType { get; set; }
        public string Notes { get; set; }


    }

    public class PaymentDetails
    {
        public string PaymentMethod { get; set; }
        public string PaidBy { get; set; }
        public int LiveOrderNumber { get; set; }
        public DateTime PaymentDate { get; set; }

        public string TransactionId { get; set; }

        public string Card { get; set; }
    }
}
