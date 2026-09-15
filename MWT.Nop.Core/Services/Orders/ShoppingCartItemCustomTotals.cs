namespace MWT.Nop.Core.Services.Orders
{
    public class ShoppingCartItemCustomTotals
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string ItemTotal { get; set; }
        public string BuyMoreSaveMoreDiscount { get; set; }
        public string MembershipDiscount { get; set; }
        public string OfferDiscount { get; set; }
        public string OfferDiscountDefault { get; set; }
        public int Quantity { get; set; }
    }
}
