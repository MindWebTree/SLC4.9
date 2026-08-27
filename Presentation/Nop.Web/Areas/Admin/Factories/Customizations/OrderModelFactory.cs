using MWT.Nop.Core.Domain.CustomOrders;
using MWT.Nop.Core.Services.Customizations.CustomOrders;
using Nop.Core.Infrastructure;
using Nop.Services.Customizations.Phone_Order;
using Nop.Web.Areas.Admin.Models.Orders;
using Nop.Web.Framework.Models.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nop.Web.Areas.Admin.Factories
{
    /// <summary>
    /// Represents the order model factory implementation
    /// </summary>
    public partial class OrderModelFactory : IOrderModelFactory
    {
         
        public virtual async Task<OrderListModel> PrepareCustomOrderListModelAsync(OrderSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            //get parameters to filter orders
            var orderStatusIds = (searchModel.OrderStatusIds?.Contains(0) ?? true) ? null : searchModel.OrderStatusIds.ToList();
            var paymentStatusIds = (searchModel.PaymentStatusIds?.Contains(0) ?? true) ? null : searchModel.PaymentStatusIds.ToList();
            var shippingStatusIds = (searchModel.ShippingStatusIds?.Contains(0) ?? true) ? null : searchModel.ShippingStatusIds.ToList();
            if (await _workContext.GetCurrentVendorAsync() != null)
                searchModel.VendorId = (await _workContext.GetCurrentVendorAsync()).Id;
            var startDateValue = !searchModel.StartDate.HasValue ? null
                : (DateTime?)_dateTimeHelper.ConvertToUtcTime(searchModel.StartDate.Value, await _dateTimeHelper.GetCurrentTimeZoneAsync());
            var endDateValue = !searchModel.EndDate.HasValue ? null
                : (DateTime?)_dateTimeHelper.ConvertToUtcTime(searchModel.EndDate.Value, await _dateTimeHelper.GetCurrentTimeZoneAsync()).AddDays(1);
            var product = await _productService.GetProductByIdAsync(searchModel.ProductId);
            var filterByProductId = product != null && (await _workContext.GetCurrentVendorAsync() == null || product.VendorId == (await _workContext.GetCurrentVendorAsync()).Id)
                ? searchModel.ProductId : 0;

            //get orders
            var orders = await _orderService.SearchOrdersAsync(storeId: searchModel.StoreId,
                vendorId: searchModel.VendorId,
                productId: filterByProductId,
                warehouseId: searchModel.WarehouseId,
                paymentMethodSystemName: searchModel.PaymentMethodSystemName,
                createdFromUtc: startDateValue,
                createdToUtc: endDateValue,
                osIds: orderStatusIds,
                psIds: paymentStatusIds,
                ssIds: shippingStatusIds,
                billingPhone: searchModel.BillingPhone,
                billingEmail: searchModel.BillingEmail,
                billingLastName: searchModel.BillingLastName,
                billingCountryId: searchModel.BillingCountryId,
                orderNotes: searchModel.OrderNotes,
                pageIndex: searchModel.Page - 1, pageSize: searchModel.PageSize);

            var _custiomOrderService = EngineContext.Current.Resolve<ICustomOrderService>();
            List<CustomOrder> customOrders = new List<CustomOrder>();
            var orderTypes = await _custiomOrderService.GetOrderTypes();
            if (orders.Count > 0)
            {
                customOrders = await _custiomOrderService.GetByLiveOrderNumbers(orders.Select(o => o.Id).ToList());
            }

            //prepare list model
            var model = await new OrderListModel().PrepareToGridAsync(searchModel, orders, () =>
            {
                //fill in model values from the entity
                return orders.SelectAwait(async order =>
                {
                    var billingAddress = await _addressService.GetAddressByIdAsync(order.BillingAddressId);

                    //fill in model values from the entity
                    var orderModel = new OrderModel
                    {
                        Id = order.Id,
                        OrderStatusId = order.OrderStatusId,
                        PaymentStatusId = order.PaymentStatusId,
                        ShippingStatusId = order.ShippingStatusId,
                        CustomerEmail = billingAddress.Email,
                        CustomerFullName = $"{billingAddress.FirstName} {billingAddress.LastName}",
                        CustomerId = order.CustomerId,
                        CustomOrderNumber = order.CustomOrderNumber
                    };

                    if (customOrders.Where(o => o.LiveOrderNumber == order.Id).Any())
                    {
                        var customOrder = customOrders.Where(o => o.LiveOrderNumber == order.Id).First();
                        var ordertype = orderTypes.Where(t => t.Id == customOrder.OrderTypeId).FirstOrDefault();
                        if (ordertype != null)
                        {
                            if (ordertype.Name.Equals("AlreadyPaid", StringComparison.InvariantCultureIgnoreCase))
                            {
                                if (orderTypes.Where(t => t.Id == customOrder.SubOrderTypeId).FirstOrDefault() != null)
                                {
                                    orderModel.OrderType = orderTypes.Where(t => t.Id == customOrder.SubOrderTypeId).FirstOrDefault().Name;
                                }
                                else
                                {
                                    orderModel.OrderType = ordertype.Name;
                                }
                            }
                            else
                                orderModel.OrderType = ordertype.Name;
                        }
                    }
                    if (string.IsNullOrEmpty(orderModel.OrderType))
                        orderModel.OrderType = "Direct";

                    if (order.CustomerId != 0)
                    {
                        var orderCustomer = await _customerService.GetCustomerByIdAsync(order.CustomerId);
                        if (orderCustomer != null)
                        {
                            bool isRegistered = await _customerService.IsRegisteredAsync(orderCustomer);
                            orderModel.CustomerFullName += $"({(isRegistered ? "registered" : "Guest")})";
                        }
                    }
                    //convert dates to the user time
                    orderModel.CreatedOn = await _dateTimeHelper.ConvertToUserTimeAsync(order.CreatedOnUtc, DateTimeKind.Utc);

                    //fill in additional values (not existing in the entity)
                    orderModel.StoreName = (await _storeService.GetStoreByIdAsync(order.StoreId))?.Name ?? "Deleted";
                    orderModel.OrderStatus = await _localizationService.GetLocalizedEnumAsync(order.OrderStatus);
                    orderModel.PaymentStatus = await _localizationService.GetLocalizedEnumAsync(order.PaymentStatus);
                    orderModel.ShippingStatus = await _localizationService.GetLocalizedEnumAsync(order.ShippingStatus);
                    orderModel.OrderTotal = await _priceFormatter.FormatPriceAsync(order.OrderTotal, true, false);

                    return orderModel;
                });
            });

            return model;
        }
    }
}
