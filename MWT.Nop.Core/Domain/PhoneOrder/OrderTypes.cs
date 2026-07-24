using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
namespace MWT.Nop.Core.Domain.PhoneOrder
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
}
