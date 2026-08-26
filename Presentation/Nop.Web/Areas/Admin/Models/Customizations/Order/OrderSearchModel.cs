using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nop.Web.Areas.Admin.Models.Orders
{
    /// <summary>
    /// Represents an order search model
    /// </summary>
    public partial record OrderSearchModel : BaseSearchModel
    {
        [NopResourceDisplayName("Admin.Orders.List.GoDirectlyToNumberByGuid")]
        public string GoDirectlyToNumberByGuid { get; set; }
    }
}
