using MWT.Nop.Core.Domain.CustomOrders;
using Nop.Core.Domain.Orders;
using Nop.Services.Orders;
using Nop.Services.Payments;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MWT.Nop.Core.Services.Orders
{
    public partial interface IOrderProcessingExtendedService:IOrderProcessingService
    {
        Task CustomSendNotificationsAndSaveNotesAsync(Order order);
        Task<(PlaceOrderResult result, ProcessPaymentResult request)> CustomPlaceOrderAsync(ProcessPaymentRequest processPaymentRequest, CustomOrder customOrder, dynamic orderSummary,bool saveOrderDetails=true,int refOrderno=0,bool chargeFromInitialaOrder=false);
    }
}
