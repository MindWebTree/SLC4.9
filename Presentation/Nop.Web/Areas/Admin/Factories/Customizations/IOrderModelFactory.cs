using Nop.Web.Areas.Admin.Models.Orders;

namespace Nop.Web.Areas.Admin.Factories
{

    public partial interface IOrderModelFactory
    {
        Task<OrderListModel> PrepareCustomOrderListModelAsync(OrderSearchModel searchModel);
    }
}
