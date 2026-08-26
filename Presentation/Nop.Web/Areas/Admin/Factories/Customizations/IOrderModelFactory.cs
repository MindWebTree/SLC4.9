using Nop.Web.Areas.Admin.Models.Orders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nop.Web.Areas.Admin.Factories
{

    public partial interface IOrderModelFactory
    {
        Task<OrderListModel> PrepareCustomOrderListModelAsync(OrderSearchModel searchModel);
    }
}
