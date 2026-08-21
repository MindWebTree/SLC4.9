using Nop.Core;
using Nop.Web.Framework.Mvc.ModelBinding;
using System.ComponentModel.DataAnnotations;
using MWT.Plugin.Misc.MwtStorefront.Areas.CustomOrder.Models.Customers;
using MWT.Nop.Core.Domain.CustomOrders;
using Nop.Core.Domain.Catalog;
namespace MWT.Plugin.Misc.MwtStorefront.Areas.CustomOrder.Models.Orders
{
    public class AdditionalServiceModel : BaseEntity
    {
        public AdditionalServiceModel()
        {
            Status = OrderStatus.SavedDraft;
            ServiceProducts = new List<Product>();
        }

        [NopResourceDisplayName("CustomOrder.AdditionalService.Fields.OrderId"), Required]
        public int OrderId { get; set; }

        [NopResourceDisplayName("CustomOrder.AdditionalService.Fields.PairedOrderIds")]
        public string PairedOrderIds { get; set; }

        public Dictionary<int, dynamic> PairedOrders { get; set; }

        [EmailAddress(ErrorMessage = "Invalid email address")]
        [NopResourceDisplayName("CustomOrder.AdditionalService.Fields.EmailID"), Required]
        public string Email { get; set; }

        [NopResourceDisplayName("CustomOrder.AdditionalService.Fields.ServicePrice"), Required]
        public decimal? ServicePrice { get; set; }
        [NopResourceDisplayName("CustomOrder.AdditionalService.Fields.DiscountPrice")]
        public decimal? DiscountPrice { get; set; }
        [NopResourceDisplayName("CustomOrder.AdditionalService.Fields.Comment")]
        public string Comment { get; set; }

        [NopResourceDisplayName("CustomOrder.AdditionalService.Fields.Exp")]
        public DateTime? Exp { get; set; }
        [Required]
        public List<Product> ServiceProducts { get; set; }
        public int Serviceproductid { get; set; }
        public OrderStatus Status { get; set; }
        public AddressModel ShippingAddress { get; set; }
        public AddressModel BillingAddress { get; set; }
        public bool ApplyTax { get; set; }
        public decimal Tax { get; set; }
        public PaymentDetails PaymentDetails { get; set; }
        public string OrderTotal { get; set; }
        public string OrderSubTotal { get; set; }
        public int CustomerId { get; set; }
        public decimal DefaultWgsCharges { get; set; }
        public decimal SurchargeAmount { get; set; }
    }


}
