namespace Nop.Web.Models.Checkout.Customizations
{
    public record CustomUpdateSectionJsonModel
    {
        public string name { get; set; }
        public string ancestorsection { get; set; }
        public string html { get; set; }
        public bool isChildSection { get; set; }
        public int? shippingAddressId { get; set; }
        public int? billingAddressId { get; set; }
        public bool loadParent { get; set; }
        public string ShippingAddress
        {
            get; set;
        }
        public bool IsStepCompleted { get; set; }
        public string errors { get; set; }
        public string TaxAmount { get; set; }
        public string Message { get; set; }
    }
}
