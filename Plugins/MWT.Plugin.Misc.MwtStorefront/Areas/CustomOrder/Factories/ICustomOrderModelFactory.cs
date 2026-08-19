using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Customers;
using Nop.MWT.Nop.Core.Domain.CustomOrders;
using Nop.Core.Domain.Orders;
using Nop.Services.Tax;
using MWT.Plugin.Misc.MwtStorefront.Areas.CustomOrder.Models.Customers;
using MWT.Plugin.Misc.MwtStorefront.Areas.CustomOrder.Models.Orders;
using MWT.Plugin.Misc.MwtStorefront.Areas.CustomOrder.Models.Product;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MWT.Plugin.Misc.MwtStorefront.Areas.CustomOrder.Factories
{
    public partial interface ICustomOrderModelFactory
    {
        Task<CustomerStatsModel> PrepareCustomerStatsModel(int customerId);
        Task<List<CustomOrderOrderTypeModel>> PrepareOrderTypesModel();
        Task<CustomOrderListModel> PrepareCustomOrderListModelAsync(CustomOrderSearchModel searchModel, bool isPartialOrders);
        Task<CustomOrderSearchModel> PrepareCustomerOrderSearchModelAsync(CustomOrderSearchModel searchModel);
        Task<CustomOrderCustomerSectionModel> PrepareCustomerSection(int orderId, int customerId = 0, bool byCustomerId = false);
        Task<List<CustomerModel>> PrepareCustomerListModelAsync(string searchTerm);
        Task<CustomOrderModel> PrepareCustomerOrderModel(Nop.MWT.Nop.Core.Domain.CustomOrders.CustomOrder order);
        Task<CustomOrderOrderTypeSectionModel> PrepareOrderTypeSectionModel(int orderId);
        Task UpdateOrderType(int orderId, int statusId, int SubOrderTypeId);
        Task UpdateOrderTypeDetails(CustomOrderOrderTypeSectionModel model, bool isReset);
        Task UpdateCustomer(int customerId, int orderId);
        Task<List<ProductOverviewModel>> PrepareProductListModelAsync(string searchTerm);
        Task<CustomOrderShoppingCartItemModel> PrepareCartModel(int orderId);
        Task AddUpdateCartItems(List<ItemModel> Items, int orderId, bool isItemNotes = false);
        Task DeleteCartItem(ItemModel item, int orderId);
        Task UpdateOrderSections(CustomOrderModel order, OrderTypeUpdate orderTypeUpdate, int customerId = 0);

        Task<bool> IsOrderPaid(Nop.MWT.Nop.Core.Domain.CustomOrders.CustomOrder order);
        Task<bool> ProductSearchEnableDisable(Nop.MWT.Nop.Core.Domain.CustomOrders.CustomOrder order);
        Task<bool> CustomerSearchEnableDisable(Nop.MWT.Nop.Core.Domain.CustomOrders.CustomOrder order);
        Task<CustomOrderSummaryModel> PrepareOderSummaryModel(int orderId, bool isCustomerPaying = false);
        Task<CustomOrderModel> PrepareBriefSummaryOfOrder(int orderId);
        Task UpdateOrderTotal(int orderId, bool updateWgsCharges = false);
        Task<bool> ValidateOrder(int orderId, int customerId);
        Task<bool> ValidateOrderCustomerDetails(int orderId, string emailAddress, string zipCode);

        Task<(decimal wgsCharges, decimal defaultWgsCharges, decimal surchargeAmount)> GetWgsCharges(Nop.MWT.Nop.Core.Domain.CustomOrders.CustomOrder order, decimal subTotal, bool isSurchargeApplicable = false);

        Task ArchiveCustomOrderAsync(int orderId);
        Task RestoreCustomOrderAsync(int orderId);

        #region Additioal Services

        Task<AdditionalServiceModel> PrepareAdditionalServiceModel(AdditionalServiceModel model,
              Nop.MWT.Nop.Core.Domain.CustomOrders.CustomOrder Customorder = null,
            Order order = null,

            MWT.Nop.Core.Domain.CustomOrders.OrderStatus status = MWT.Nop.Core.Domain.CustomOrders.OrderStatus.SavedDraft);
        Task<(Dictionary<int, dynamic> pairedOrders, string validPairedOrders, decimal orderSubTotal, decimal orderTotal, bool isSurchargeApplicable)> GetAdditionalServiceOrderInfo(Nop.MWT.Nop.Core.Domain.CustomOrders.OrderStatus status, Order order, Nop.MWT.Nop.Core.Domain.CustomOrders.CustomOrder customOrder, string pairedOrderIds);

        Task AdditionalServiceSendInvoice(Nop.MWT.Nop.Core.Domain.CustomOrders.CustomOrder order, decimal previousWgsAmount, decimal previousWgsDiscount, decimal currentWgsAmount, decimal currentWgsDiscount, decimal defaultWgsCharges = 0, decimal surchargeAmount = 0);
        Task<TaxTotalResult> GetOrderTax(Customer customer, decimal total, decimal shippingCharges);


        #endregion
        #region Api
        Task<ApiCustomOrderSummaryModel> PrepareApiOderSummaryModel(int orderId, bool isCustomerPaying = false);
        #endregion
    }
}
