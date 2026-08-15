using MWTNop.Core.Domain.Catalog;
using Nop.Web.Framework.Models;

namespace Nop.Web.Models.ShoppingCart
{
    public partial record ShoppingCartModel : BaseNopModel
    {
        public string OrderTotal { get; set; }
        public string SubTotal { get; set; }
        public bool? isConfirmationPage { get; set; }
        public decimal MemberShipDiscountValue { get; set; }
        public string MemberShipDiscount { get; set; }


        public bool isCustomerEligibleForMemberShipPrice { get; set; }

        public partial record ShoppingCartItemModel : BaseNopEntityModel
        {
            public decimal MemberShipDiscountValue { get; set; }
            public string MemberShipDiscount { get; set; }
            public decimal MemberShipPriceValue { get; set; }
            public string MemberShipPrice { get; set; }
            public string BuyMoreSaveMoreDiscount { get; set; }
            public string OldPrice { get; set; }
            public decimal OldPriceValue { get; set; }
            public string OfferDiscount { get; set; }
            public decimal OfferDiscountValue { get; set; }
            public string SpecialInstructions { get; set; }
            public string SubTotalIncludeDiscount { get; set; }
            public VariantCombination Variant { get; set; }
        }
    }
}
