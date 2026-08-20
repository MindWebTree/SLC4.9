using MWT.Nop.Core.Domain.CustomOrders;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MWT.Nop.Core.Services.Customizations.CustomOrders
{
    public partial interface ICustomOrderService
    {
        Task UpdateAsync(CustomOrder customOrder);
        Task UpdateWithoutEventAsync(CustomOrder customOrder);
        Task DeleteAsync(CustomOrder customOrder);
        Task InsertAsync(CustomOrder customOrder);
        Task<CustomOrder> GetById(int Id);
        Task<CustomOrder> GetByParentLiveOrderNumber(int orderNumber);
        Task<List<CustomOrder>> GetByLiveOrderNumbers(IList<int> Ids);
        Task<CustomOrder> GetByOrderNumber(int Id);
        Task<IPagedList<CustomOrder>> SearchCustomorder(string searchterm, int customerId, int statusId,
              int pageIndex = 0, int pageSize = int.MaxValue, bool ispartial = false, bool displayAdditionalService = false, bool isDeleted = false);

        Task<CustomOrderOrderSummaryAdjustment> GetOrderSummaryAdjustment(int orderId);
        Task<List<CustomOrderStatus>> GetOrderStatuses();

        Task InsertCustomOrderOrderSummaryAdjustmentAsync(CustomOrderOrderSummaryAdjustment customOrderOrderSummaryAdjustment);

        Task UpdateCustomOrderOrderSummaryAdjustmentAsync(CustomOrderOrderSummaryAdjustment customOrderOrderSummaryAdjustment);

        #region Cart Item

        Task<List<CustomOrderShoppingCartItem>> GetOrderItems(int orderId);

        Task DeleteOrderItemAsync(int Id, int OrderId);

        Task InsertOrderItemAsync(CustomOrderShoppingCartItem customOrderShoppingCartItem);
        Task UpdateCartItemAsync(CustomOrderShoppingCartItem item);

        Task<CustomOrderPriceAdjustment> GetPriceAdjustmentsByCartId(int cartId);

        Task UpdatePriceAdjustmentAsync(CustomOrderPriceAdjustment priceAdj);

        Task InsertPriceAdjustmentAsync(CustomOrderPriceAdjustment customOrderPriceAdjustment);

        #endregion

        Task<List<CustomOrderOrderType>> GetOrderTypes();

        Task<decimal> GetTaxRate(int countryId, int stateId, string zip);

        // status Log
        Task<List<CustomorderOrderStatusLog>> GetOrderStatusLogs(int orderId);

        Task<CustomorderOrderStatusLog> GetStatusOfOrder(int orderId);

        Task InsertOrderStatusLogAsync(CustomorderOrderStatusLog customorderOrderStatusLog);

        Task UpdateOrderStatusLogAsync(CustomorderOrderStatusLog customorderOrderStatusLog);

        Task<decimal> GetPayableAmount(CustomOrder order);

        Task<bool> IsOrderPaid(CustomOrder order);

        // end

        #region Notes
        Task InsertOrderNotesLogAsync(CustomOrderNotesLog customorderOrderNotesLog);

        Task<List<CustomOrderNotesLog>> GetOrderNotesLogs(int orderId);

        #endregion


        #region Wgs Service

        Task<bool> IsSurchargeApplicable(CustomOrder order);

        #endregion

    }
}
