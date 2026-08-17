using Nop.Core.Domain.Shipping;
using Nop.Core.Events;
using Nop.Services.Customers;
using Nop.Services.Events;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Orders;
using Nop.Services.Shipping;
using System;
using System.Threading.Tasks;

namespace Nop.Services.Customizations.Orders.Consumer
{
    public class ShipmentEventConsumer : IConsumer<EntityInsertedEvent<Shipment>>,
         IConsumer<EntityInsertedEvent<ShipmentItem>>, IConsumer<EntityUpdatedEvent<ShipmentItem>>,
          IConsumer<EntityUpdatedEvent<Shipment>>
    {

        #region Fields

        private readonly ICustomerActivityService _customerActivityService;
        private readonly ILocalizationService _localizationService;
        private readonly IOrderService _orderService;
        private readonly ICustomerService _customerService;
        private readonly IShipmentService _shipmentService;

        #endregion

        public ShipmentEventConsumer(ICustomerActivityService customerActivityService,
                            ILocalizationService localizationService, IOrderService orderService,
                            IShipmentService shipmentService, ICustomerService customerService)
        {
            _customerActivityService = customerActivityService;
            _localizationService = localizationService;
            _orderService = orderService;
            _shipmentService = shipmentService;
            _customerService = customerService;
        }


        public async Task HandleEventAsync(EntityInsertedEvent<Shipment> eventMessage)
        {
            var order = await _orderService.GetOrderByIdAsync(eventMessage.Entity.OrderId);
            var customer = await _customerService.GetCustomerByIdAsync(order.CustomerId);
            if (eventMessage.Entity.ShippedDateUtc.HasValue && eventMessage.Entity.ShippedDateUtc > DateTime.Now.AddSeconds(-10))
            {
                var customerInsertActivityFormat = await _localizationService.GetResourceAsync("CustomOrder.Activity.Order.Status");
                await _customerActivityService.InsertActivityAsync(customer, "CustomerRelatedActivity", string.Format(customerInsertActivityFormat, eventMessage.Entity.OrderId, ShippingStatus.Shipped.ToString(), Convert.ToDateTime(eventMessage.Entity.ShippedDateUtc).ToString("dd MMM yyyy"), order.ShippingMethod, eventMessage.Entity.TrackingNumber), null);

            }

            else if (eventMessage.Entity.DeliveryDateUtc.HasValue && eventMessage.Entity.DeliveryDateUtc > DateTime.Now.AddSeconds(-10))
            {
                var customerInsertActivityFormat = await _localizationService.GetResourceAsync("CustomOrder.Activity.Order.Status");
                await _customerActivityService.InsertActivityAsync(customer, "CustomerRelatedActivity", string.Format(customerInsertActivityFormat, eventMessage.Entity.OrderId, ShippingStatus.Delivered.ToString(), Convert.ToDateTime(eventMessage.Entity.DeliveryDateUtc).ToString("dd MMM yyyy"), order.ShippingMethod, eventMessage.Entity.TrackingNumber), null);
            }

        }

        public async Task HandleEventAsync(EntityUpdatedEvent<Shipment> eventMessage)
        {
            var order = await _orderService.GetOrderByIdAsync(eventMessage.Entity.OrderId);
            var customer = await _customerService.GetCustomerByIdAsync(order.CustomerId);

            if (eventMessage.Entity.ShippedDateUtc.HasValue && eventMessage.Entity.ShippedDateUtc > DateTime.UtcNow.AddSeconds(-10))
            {
                var customerInsertActivityFormat = await _localizationService.GetResourceAsync("CustomOrder.Activity.Order.Status");
                await _customerActivityService.InsertActivityAsync(customer, "CustomerRelatedActivity", string.Format(customerInsertActivityFormat, eventMessage.Entity.OrderId, ShippingStatus.Shipped.ToString(), Convert.ToDateTime(eventMessage.Entity.ShippedDateUtc).ToString("dd MMM yyyy"), order.ShippingMethod, eventMessage.Entity.TrackingNumber), null);

            }
            else if (eventMessage.Entity.DeliveryDateUtc.HasValue && eventMessage.Entity.DeliveryDateUtc > DateTime.UtcNow.AddSeconds(-10))
            {
                var customerInsertActivityFormat = await _localizationService.GetResourceAsync("CustomOrder.Activity.Order.Status");
                await _customerActivityService.InsertActivityAsync(customer, "CustomerRelatedActivity", string.Format(customerInsertActivityFormat, eventMessage.Entity.OrderId, ShippingStatus.Delivered.ToString(), Convert.ToDateTime(eventMessage.Entity.DeliveryDateUtc).ToString("dd MMM yyyy"), order.ShippingMethod, eventMessage.Entity.TrackingNumber), null);

            }

        }
        public async Task HandleEventAsync(EntityInsertedEvent<ShipmentItem> eventMessage)
        {
            //var shipment = await _shipmentService.GetShipmentByIdAsync(eventMessage.Entity.ShipmentId);
            //if (shipment == null)
            //    return;
            //var order = await _orderService.GetOrderByIdAsync(shipment.OrderId);
            //var customer = await _customerService.GetCustomerByIdAsync(order.CustomerId);
            //if (order.ShippingStatus == ShippingStatus.Shipped || order.ShippingStatus == ShippingStatus.PartiallyShipped)
            //{
            //    if (shipment.ShippedDateUtc.HasValue && shipment.ShippedDateUtc > DateTime.Now.AddSeconds(-10))
            //    {
            //        var customerInsertActivityFormat = await _localizationService.GetResourceAsync("CustomOrder.Activity.Order.Status");
            //        await _customerActivityService.InsertActivityAsync(customer, "CustomerRelatedActivity", string.Format(customerInsertActivityFormat, shipment.OrderId, order.ShippingStatus.ToString(), Convert.ToDateTime(shipment.ShippedDateUtc).ToString("dd MMM yyyy"), shipment.TrackingNumber), null);

            //    }
            //}
            //if (order.ShippingStatus == ShippingStatus.Delivered)
            //{
            //    if (shipment.DeliveryDateUtc.HasValue && shipment.DeliveryDateUtc > DateTime.Now.AddSeconds(-10))
            //    {
            //        var customerInsertActivityFormat = await _localizationService.GetResourceAsync("CustomOrder.Activity.Order.Status");
            //        await _customerActivityService.InsertActivityAsync(customer, "CustomerRelatedActivity", string.Format(customerInsertActivityFormat, shipment.OrderId, order.ShippingStatus.ToString(), Convert.ToDateTime(shipment.ShippedDateUtc).ToString("dd MMM yyyy"), shipment.TrackingNumber), null);

            //    }
            //}

        }
        public async Task HandleEventAsync(EntityUpdatedEvent<ShipmentItem> eventMessage)
        {
            //var shipment = await _shipmentService.GetShipmentByIdAsync(eventMessage.Entity.ShipmentId);
            //if (shipment == null)
            //    return;
            //var order = await _orderService.GetOrderByIdAsync(shipment.OrderId);
            //var customer = await _customerService.GetCustomerByIdAsync(order.CustomerId);
            //if (order.ShippingStatus == ShippingStatus.Shipped || order.ShippingStatus == ShippingStatus.PartiallyShipped)
            //{
            //    if (shipment.ShippedDateUtc.HasValue && shipment.ShippedDateUtc > DateTime.Now.AddSeconds(-10))
            //    {
            //        var customerInsertActivityFormat = await _localizationService.GetResourceAsync("CustomOrder.Activity.Order.Status");
            //        await _customerActivityService.InsertActivityAsync(customer, "CustomerRelatedActivity", string.Format(customerInsertActivityFormat, shipment.OrderId, order.ShippingStatus.ToString(), Convert.ToDateTime(shipment.ShippedDateUtc).ToString("dd MMM yyyy"), shipment.TrackingNumber), null);

            //    }
            //}
            //if (order.ShippingStatus == ShippingStatus.Delivered)
            //{
            //    if (shipment.DeliveryDateUtc.HasValue && shipment.DeliveryDateUtc > DateTime.Now.AddSeconds(-10))
            //    {
            //        var customerInsertActivityFormat = await _localizationService.GetResourceAsync("CustomOrder.Activity.Order.Status");
            //        await _customerActivityService.InsertActivityAsync(customer, "CustomerRelatedActivity", string.Format(customerInsertActivityFormat, shipment.OrderId, order.ShippingStatus.ToString(), Convert.ToDateTime(shipment.ShippedDateUtc).ToString("dd MMM yyyy"), shipment.TrackingNumber), null);

            //    }
            //}

        }
    }
}