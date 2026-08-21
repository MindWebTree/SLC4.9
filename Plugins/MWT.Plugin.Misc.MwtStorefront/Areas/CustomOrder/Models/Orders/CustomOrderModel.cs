using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Services.Payments;
using MWT.Plugin.Misc.MwtStorefront.Areas.CustomOrder.Models.Customers;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;
using System;
using System.Collections.Generic;
using MWT.Plugin.Misc.MwtStorefront.Models.Common;

namespace MWT.Plugin.Misc.MwtStorefront.Areas.CustomOrder.Models.Orders
{
    public partial record CustomOrderModel : BaseNopEntityModel
    {

        public string OrderNumberWithPrefix { get; set; }
        public CustomOrderModel()
        {
            orderTypes = new List<SelectListItem>();
            subOrderTypes = new List<SelectListItem>();
        }
        [NopResourceDisplayName("CustomOrder.Order.Fields.OrderTotal")]
        public string OrderTotal { get; set; }

        [NopResourceDisplayName("CustomOrder.Order.Fields.OrderTotal")]
        public string SubTotal { get; set; }

        public string SubTotalDiscountType { get; set; }

        public decimal? SubTotalDiscount { get; set; }


        [NopResourceDisplayName("CustomOrder.Order.Fields.TotalDiscount")]
        public string TotalDiscount { get; set; }

        [NopResourceDisplayName("CustomOrder.Order.Fields.CreatedOn")]
        public DateTime? CreatedOn { get; set; }

        [NopResourceDisplayName("CustomOrder.Order.Fields.IsCompleted")]
        public bool? IsCompleted { get; set; }

        [NopResourceDisplayName("CustomOrder.Order.Fields.PrivateOrderNotes")]
        public string PrivateOrderNotes { get; set; }

        [NopResourceDisplayName("CustomOrder.Order.Fields.InvoiceNote")]
        public string InvoiceNote { get; set; }

        [NopResourceDisplayName("CustomOrder.Order.Fields.SpecialInstructionsfromBuyer")]
        public string SpecialInstructionsfromBuyer { get; set; }

        [NopResourceDisplayName("CustomOrder.Order.Fields.LiveOrderNumber")]
        public int? LiveOrderNumber { get; set; }

        [NopResourceDisplayName("CustomOrder.Order.Fields.PurchaseOrderNumber")]
        public string PurchaseOrderNumber { get; set; }

        [NopResourceDisplayName("CustomOrder.Order.Fields.OrderTypeId")]
        public int? OrderTypeId { get; set; }

        [NopResourceDisplayName("CustomOrder.Order.Fields.HouzzFee")]
        public decimal? HouzzFee { get; set; }

        [NopResourceDisplayName("CustomOrder.Order.Fields.HouzzFeeType")]
        public string HouzzFeeType { get; set; }

        [NopResourceDisplayName("CustomOrder.Order.Fields.PromiseDayDate")]
        public DateTime? PromiseDayDate { get; set; }

        // created a sting model class for the exp Date to be display on the grid table for additional service
        [NopResourceDisplayName("CustomOrder.Order.Fields.PromiseDayDate")]
        public string FormattedPromiseDayDate { get; set; }

        [NopResourceDisplayName("CustomOrder.Order.Fields.AlreadyFee")]
        public decimal? AlreadyFee { get; set; }

        [NopResourceDisplayName("CustomOrder.Order.Fields.OrderTax")]
        public string OrderTax { get; set; }

        [NopResourceDisplayName("CustomOrder.Order.Fields.IsShipped")]
        public bool isShipped { get; set; }

        [NopResourceDisplayName("CustomOrder.Order.Fields.IsShipped")]
        public bool isDelivered { get; set; }

        [NopResourceDisplayName("CustomOrder.Order.Fields.CreatedOn")]
        public string FormattedCreatedOn { get; set; }

        public string CustomerName { get; set; }
        public string CustomerEmail { get; set; }

        public int NoOfItems { get; set; }

        public string Pending { get; set; }

        public List<ItemModel> Items { get; set; }

        public bool IsPartialOrder { get; set; }

        public string OrderStatus { get; set; }

        public int StatusId { get; set; }
        [NopResourceDisplayName("CustomOrder.Order.Fields.orderTypes")]
        public List<SelectListItem> orderTypes { get; set; }

        public List<SelectListItem> subOrderTypes { get; set; }

        public int SubOrderTypeId { get; set; }

        [NopResourceDisplayName("CustomOrder.Order.Fields.SubOrderTypeId")]
 

        public int? CustomerId { get; set; }

        [NopResourceDisplayName("CustomOrder.Order.Fields.Complementry.Wgs.Free")]
        public bool ComplementryWgsFree { get; set; }
        public decimal Wgs { get; set; }


        [NopResourceDisplayName("CustomOrder.Order.Fields.WgsAdjustmentNotes")]
        public string WgsAdjustmentNotes { get; set; }

        [NopResourceDisplayName("CustomOrder.Order.Fields.ShippingMethod")]
        public string ShippingMethod { get; set; }
        public decimal Shipping { get; set; }
        public string ShippingDiscountType { get; set; }
        public decimal? ShippingDiscount { get; set; }
        public bool EnableProductSearch { get; set; }
        public bool EnableCartSummary { get; set; }
        public bool DisplayCartSummary { get; set; }
        public bool EnableCustomerSearch { get; set; }

        public string SubTotalAdjustmentNotes { get; set; }
        public string ShippingAdjustmentNotes { get; set; }

        public CustomOrderShoppingCartItemModel customOrderShoppingCartItemModel { get; set; }
        public CustomOrderSummaryModel customOrderSummaryModel { get; set; }

        public AddressModel ShippingAddress { get; set; }
        public AddressModel BillingAddress { get; set; }
        public Dictionary<string, bool> ShippingMethods { get; set; }
        public string PayableAmount { get; set; }
        public bool ApplyTax { get; set; }

        public int ParentOrderId { get; set; }

        public bool IsDeleted { get; set; }
        public string DeletedBy { get; set; }
        public DateTime? DeletedOn { get; set; }

    }
}
