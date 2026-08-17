using Nop.Core;
using System;


namespace MWT.Nop.Core.Domain.CustomOrders
{
    public partial class CustomOrderShoppingCartItem : BaseEntity
    {
        public int? CustomerId { get; set; }
        public int ProductId { get; set; }
        public int StoreId { get; set; }
        public int ShoppingCartTypeId { get; set; }
        public string AttributesDescription { get; set; }
        public string AttributesXml { get; set; }
        public decimal CustomerEnteredPrice { get; set; }
        public int Quantity { get; set; }
        public DateTime? RentalStartDateUtc { get; set; }
        public DateTime? RentalEndDateUtc { get; set; }
        public DateTime CreatedOnUtc { get; set; }
        public DateTime UpdatedOnUtc { get; set; }
        public string Notes { get; set; }
   public string CustomAttributesDescription { get; set; }
        public int? OrderId { get; set; }
    }
}
