using Nop.Core;
using Nop.Core.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MWT.Nop.Core.Domain.CustomOrders
{
    public partial class CustomOrder : BaseEntity
    {
        public CustomOrder()
        {
            ApplyTax = true;
            ComplementryWgsFree = false;
        }
        public int? CustomerId { get; set; }
        public decimal? OrderTotal { get; set; }
        public decimal? SubTotal { get; set; }
        public decimal? TotalDiscount { get; set; }
        public DateTime? CreatedOn { get; set; }
        public bool? IsCompleted { get; set; }
        public string PrivateOrderNotes { get; set; }
        public string InvoiceNote { get; set; }
        public string SpecialInstructionsfromBuyer { get; set; }
        public int? LiveOrderNumber { get; set; }
        public string PurchaseOrderNumber { get; set; }
        public int? OrderTypeId { get; set; }
        public int? SubOrderTypeId { get; set; }
        public decimal? HouzzFee { get; set; }
        public string HouzzFeeType { get; set; }
        public DateTime? PromiseDayDate { get; set; }
        public decimal? AlreadyFee { get; set; }
        public decimal? OrderTax { get; set; }
        public int StatusId { get; set; }
        public bool FullPaid { get; set; }
        public decimal? Wgs { get; set; }
        public bool ComplementryWgsFree { get; set; }
        public decimal? Shipping { get; set; }
        public string WgsAdjustmentNotes { get; set; }
        public string ShippingMethod { get; set; }
        public int ParentOrderID { get; set; }
        public string CustomerCCEmail { get; set; }
        public bool ApplyTax { get; set; }
        public decimal TaxRate { get; set; }
        public DateTime? Updatedon { get; set; }
        public string TaxInfo { get; set; }
        public decimal CustomDuty { get; set; }
        public bool NotInterested { get; set; }
        public decimal CustomDutyPercentage { get; set; }
        public string ZohoPotentialId { get; set; }
        public int CreatedBy { get; set; }
        public string PairedOrderIds { get; set; }
        public bool IsDeleted { get; set; }
        public int DeletedBy { get; set; }
        public DateTime? DeletedOn { get; set; }
    }
}
