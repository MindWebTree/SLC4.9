using Nop.Web.Framework.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nop.Web.Areas.Admin.Models.Customization.Utilities
{
    public partial record StoreWideDiscountSettingSearchModel : BaseSearchModel
    {
        public int StoreWideDiscountId { get; set; }
    }
}
