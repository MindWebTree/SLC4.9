using Nop.Core.Domain.Customization.Custom.StoreWideDiscount;
using Nop.Web.Areas.Admin.Models.Catalog;
using Nop.Web.Framework.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nop.Web.Areas.Admin.Models.Customization.Utilities
{
    public partial record StoreWideProductDiscountHistoryListModel : BasePagedListModel<StoreWideProductDiscountHistoryModel>
    {
    }
}
