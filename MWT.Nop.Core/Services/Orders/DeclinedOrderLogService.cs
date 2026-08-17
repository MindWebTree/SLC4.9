using MWT.Nop.Core.Domain;
using MWT.Nop.Core.Services.Customers;
using Nop.Core;
using Nop.Core.Domain.Orders;
using Nop.Data;
using Nop.Services.Customers;
using Nop.Services.Orders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MWT.Nop.Core.Services.Orders
{
    public partial class DeclinedOrderLogService : IDeclinedOrderLogService
    {

        #region Fields

        private readonly IRepository<DeclinedOrderLog> _declinedOrderLogRepository;
        private readonly IWorkContext _workContext;
        private readonly ICustomerExtendedService _customerService;
        private readonly IShoppingCartService _shoppingCartService;
        private readonly IStoreContext _storeContext;
        private readonly IOrderTotalCalculationService _orderTotalCalculationService;
        #endregion

        #region  Ctor

        public DeclinedOrderLogService(IRepository<DeclinedOrderLog> declinedOrderLogRepository, IWorkContext workContext,
            ICustomerExtendedService customerService, IShoppingCartService shoppingCartService, IStoreContext storeContext, IOrderTotalCalculationService orderTotalCalculationService)
        {
            _declinedOrderLogRepository = declinedOrderLogRepository;
            _workContext = workContext;
            _customerService = customerService;
            _shoppingCartService = shoppingCartService;
            _storeContext = storeContext;
            _orderTotalCalculationService = orderTotalCalculationService;
        }

        #endregion

        #region Methods
        public async Task Insert(string error)
        {
            try
            {
                error = error ?? string.Empty;
                string paymentMethod = string.Empty;
                var customer = await _workContext.GetCurrentCustomerAsync();
                var cart = await _shoppingCartService.GetShoppingCartAsync(customer, ShoppingCartType.ShoppingCart,
                             (await _storeContext.GetCurrentStoreAsync())?.Id ?? 0);
                var (orderSubTotalDiscountAmountBase, _, subTotal, _, _) = await _orderTotalCalculationService.GetShoppingCartSubTotalAsync(cart, true);


                var errParts = error.Split(':');
                if (errParts.Length > 1)
                    paymentMethod = errParts[1].Replace("Error", "").Trim();

                await _declinedOrderLogRepository.InsertAsync(new DeclinedOrderLog()
                {
                    CreatedOn = DateTime.UtcNow,
                    CustomerId = customer?.Id ?? 0,
                    Email = await _customerService.GetCustomerEmail(customer),
                    Message = error,
                    OrderTotal = subTotal,
                    PaymentMethod = paymentMethod,
                    ShoppingCartIds = string.Join('-', cart.Select(c => c.Id))
                });
            }
            catch { }
        }

        #endregion
    }
}
