using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
namespace MWT.Nop.Core.Domain.CustomOrders
{
    public enum OrderTypes
    {
        [Display(Name = "Custom Order")]
        CustomOrder,
        [Display(Name = "Ebay Order")]
        EbayOrder,
        [Display(Name = "Already Paid")]
        AlreadyPaid,
        [Display(Name = "Houzz Order")]
        HouzzOrder
    }
    public enum SubOrderTypes
    {
        [Display(Name = "Replacement Order")]
        ReplacementOrder,
        [Display(Name = "Stain Samples for Sales Team")]
        StainSampleForTeam,
        [Display(Name = "Hardware")]
        Hardware,
        [Display(Name = "Leather Sample")]
        LeatherSample,
        [Display(Name = "Stain Sample")]
        StainSample,
        [Display(Name = "Gift For Feedback")]
        GiftForFeedback,
        [Display(Name = "Test Order")]
        TestOrder
    }
}
