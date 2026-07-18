using Nop.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MWT.Nop.Plugin.Payments.Affirm.Data.Domain
{
    public partial class PaymentMethodSession : BaseEntity
    {
        public int CustomerId { get; set; }

        public bool IsCustomOrder { get; set; }
        public string CartItems { get; set; }
        public decimal? CartTotal { get; set; }
        public string ShippingMethod { get; set; }

        public decimal ShipingTotal { get; set; }
        public string Coupon { get; set; }
        public decimal TaxTotal { get; set; }
        public decimal CustomDutyTotal { get; set; }
        public decimal DiscountTotal { get; set; }
        public decimal OrderTotal { get; set; }
        public string ZipCode { get; set; }
        public int StatusId { get; set; }
        public RequestStatus Status
        {
            get => (RequestStatus)StatusId;
            set => StatusId = (int)value;
        }
        public string Country { get; set; }
        public int LiveOrderNumber { get; set; }
        public string Checkout_Token { get; set; }
        public DateTime CreatedOnUtc { get; set; }
        public DateTime UpdatedOnUtc { get; set; }

        public string Message { get; set; }
        public int CustomOrderId { get; set; }


    }

    public enum RequestStatus
    {
        /// <summary>
        /// Debug
        /// </summary>
        Init = 1,

        /// <summary>
        /// Information
        /// </summary>
        Cancelled = 2,

        /// <summary>
        /// Warning
        /// </summary>
        Completed = 3,



        /// <summary>
        /// Error
        /// </summary>
        Error = 4,

        Response = 5,


    }
}
