using MWT.Plugin.Misc.MwtStorefront.Models.Common;
using Nop.Web.Framework.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static MWT.Plugin.Misc.MwtStorefront.Models.Api.ProductModel;

namespace MWT.Plugin.Misc.MwtStorefront.Models.Api
{

    public class TaxInfoModel
    {
        public decimal TaxRate { get; set; }
        public string TaxType { get; set; }
        public decimal Amount { get; set; }
    }

    public partial record ApiOrderDetailModel : BaseNopEntityModel
    {
        public ApiOrderDetailModel()
        {
            TaxRates = new List<ApiTaxRate>();
            GiftCards = new List<ApiGiftCard>();
            Items = new List<ApiOrderItemModel>();
            OrderNotes = new List<ApiOrderNote>();
            Shipments = new List<ApiShipmentBriefModel>();

            BillingAddress = new AddressExtendedModel();
            ShippingAddress = new AddressExtendedModel();
            PickupAddress = new AddressExtendedModel();

            CustomValues = new Dictionary<string, object>();
            TaxInfo = new List<TaxInfoModel>();
        }

        public string Email { get; set; }
        public string CustomOrderNumber { get; set; }

        public int ParentOrderID { get; set; }
        public Guid OrderGuid { get; set; }
        public DateTime CreatedOn { get; set; }

        public int CustomerId { get; set; }

        public bool SmsOptionSelected { get; set; }
        public string TransactionId { get; set; }
        public string OrderStatus { get; set; }
        public bool IsShippable { get; set; }
        public bool PickupInStore { get; set; }
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

        public decimal OrderSubtotal { get; set; }
        public decimal OrderSubTotalDiscount { get; set; }
        public decimal OrderShipping { get; set; }
        public decimal PaymentMethodAdditionalFee { get; set; }
        public string CheckoutAttributeInfo { get; set; }

        public bool PricesIncludeTax { get; set; }
        public bool DisplayTaxShippingInfo { get; set; }
        public decimal Tax { get; set; }

        public List<TaxInfoModel> TaxInfo { get; set; }

        public decimal CustomDutyPercentage { get; set; }

        public string CustomDuty { get; set; }
        public IList<ApiTaxRate> TaxRates { get; set; }
        public bool DisplayTax { get; set; }
        public bool DisplayTaxRates { get; set; }

        public decimal OrderTotalDiscount { get; set; }
        public int RedeemedRewardPoints { get; set; }
        public decimal RedeemedRewardPointsAmount { get; set; }
        public decimal OrderTotal { get; set; }

        public IList<ApiGiftCard> GiftCards { get; set; }

        public bool ShowSku { get; set; }
        public IList<ApiOrderItemModel> Items { get; set; }

        public IList<ApiOrderNote> OrderNotes { get; set; }

        public bool ShowVendorName { get; set; }

        public decimal MembershipFee { get; set; }
        public decimal MembershipFeeDiscount { get; set; }
        public decimal MembershipDiscountIncTax { get; set; }
        public decimal OfferDiscountIncTax { get; set; }
        public decimal BuyMoreSaveMoreDiscountIncTax { get; set; }
        public string SpecialInstructions { get; set; }
        public string CreditCardNumber { get; set; }
        public decimal AdditonalShippingCharges { get; set; }
        public string CustomerCurrencyCode { get; set; }


        #region Nested Classes

        public partial record ApiOrderItemModel : BaseNopEntityModel
        {
            public Guid OrderItemGuid { get; set; }
            public string Sku { get; set; }
            public int ProductId { get; set; }
            public string Name { get; set; }
            public string ProductSeName { get; set; }
            public decimal UnitPrice { get; set; }
            public decimal SubTotal { get; set; }
            public int Quantity { get; set; }
            public string AttributesDescription { get; set; }

            public string AttributeXml { get; set; }
            public string RentalInfo { get; set; }

            public string VendorName { get; set; }

            //downloadable product properties
            public int DownloadId { get; set; }
            public int LicenseId { get; set; }
            public string Picture { get; set; }

            public decimal ItemPriceIncTax { get; set; }
            public decimal MembershipDiscountIncTax { get; set; }
            public decimal OfferDiscountIncTax { get; set; }
            public decimal BuyMoreSaveMoreDiscountIncTax { get; set; }
            public string Notes { get; set; }

            public decimal TotalDiscount { get; set; }
            public decimal ItemTotal { get; set; }

            public int VariantId { get; set; }
            public List<CustomProductSpecificationModel> Specifications { get; set; }

            public List<string> DimensionImages { get; set; }
            public bool IsDeliveryGuaranteed { get; set; }

            public string EstimatedDeliveryDate { get; set; }
        }

        public partial record ApiTaxRate : BaseNopModel
        {
            public decimal Rate { get; set; }
            public decimal Value { get; set; }
        }

        public partial record ApiGiftCard : BaseNopModel
        {
            public string CouponCode { get; set; }
            public decimal Amount { get; set; }
        }

        public partial record ApiOrderNote : BaseNopEntityModel
        {
            public bool HasDownload { get; set; }
            public string Note { get; set; }
            public DateTime CreatedOn { get; set; }
        }

        public partial record ApiShipmentBriefModel : BaseNopEntityModel
        {
            public string TrackingNumber { get; set; }
            public DateTime? ShippedDate { get; set; }
            public DateTime? DeliveryDate { get; set; }
        }

        #endregion
    }

}
