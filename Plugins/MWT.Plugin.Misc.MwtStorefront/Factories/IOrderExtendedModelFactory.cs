using MWT.Plugin.Misc.MwtStorefront.Models.Api;
using MWT.Plugin.Misc.MwtStorefront.Models.Order;
using Nop.Core.Domain.Orders;
using Nop.Web.Factories;

namespace MWT.Plugin.Misc.MwtStorefront.Factories
{
    public partial interface IOrderExtendedModelFactory: IOrderModelFactory
    {
        Task<CustomerOrderExtendedListModel> CustomPrepareCustomerOrderListModelAsync(int? page, OrderHistoryPeriods limit);
        Task<OrderDetailsExtendedModel> PrepareCustomOrderDetailsModelAsync(Order order);
        Task PrintOrdersToPdfAsync(Stream stream, IList<Order> orders, int languageId = 0, int vendorId = 0);
        Task<List<int>> GetNewOrders();
        Task UpdateStatusOfOrder(int orderId, bool isImported);
        Task MarkOrderAsDelivered(DeliverOrderRequestModel model);
        Task<ApiOrderDetailModel> PrepareApiCustomOrderDetailsModelAsync(Order order);
        Task<CustomOrderApiDetailModel> PrepareApiCustomOrder_OrderDetailsModelAsync(Order order);
        Task<dynamic> ApiSendAdditionalWgsServiceInvoice(int orderNumber, string pairedOrderIds);
        Task<dynamic> ApiGetStatusOfAdditionalWgsService(int orderNumber);
        Task<TrackOrderModel> GetRecentOrderOfCustomer();
        Task<int> GetLatestDeliveredOrderIdByCustomerEmail(string email);
        Task<Dictionary<string, decimal>> GetShippingMethods(int variantId, decimal total, string zipcode, int countryId);

        #region Custom Order Conversion
        Task<CustomOrderConversionModel> PrepareCustomOrderConversionModel(Order order);
        #endregion

    }
}
