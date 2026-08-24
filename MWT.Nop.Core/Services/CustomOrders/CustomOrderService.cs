using MWT.Nop.Core.Domain.CustomOrders;
using MWT.Nop.Core.Service.Catalog;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Payments;
using Nop.Core.Infrastructure;
using Nop.Data;
using Nop.Services.Catalog;
using Nop.Services.Customers;
using Nop.Services.Payments;
using System.Dynamic;

namespace MWT.Nop.Core.Services.Customizations.CustomOrders
{
    public partial class CustomOrderService : ICustomOrderService
    {

        #region Fields

        private readonly IRepository<CustomOrder> _customOrderRepository;
        private readonly IRepository<CustomOrderShoppingCartItem> _customOrderShoppingCartItemRepository;
        private readonly IRepository<CustomOrderPriceAdjustment> _customPriceAdjustmentRepository;
        private readonly IRepository<CustomorderOrderStatusLog> _customorderOrderStatusLog;
        private readonly IRepository<CustomOrderNotesLog> _customorderNotesLog;
        private readonly IRepository<GenericAttribute> _gaRepository;
        private readonly IRepository<Customer> _customerRepository;
        private readonly IRepository<CustomOrderStatus> _customOrderStatusRepository;
        private readonly IRepository<CustomOrderOrderType> _customOrderOrderTypeRepository;
        private readonly IRepository<CustomOrderOrderSummaryAdjustment> _customOrderOrderSummaryAdjustment;

        private readonly IRepository<TaxRate> _taxRateRepository;
        private readonly IProductExtendedService _productService;
        private readonly IProductAttributeFormatter _productAttributeFormatter;
        private readonly IWorkContext _workContext;
        private readonly ICustomerService _customerService;
        private readonly IPriceFormatter _priceFormatter;
        #endregion

        #region Ctor

        public CustomOrderService(IRepository<CustomOrder> customOrderRepository,
                                 IRepository<CustomOrderShoppingCartItem> customOrderShoppingCartItemRepository,
                                 IRepository<CustomOrderPriceAdjustment> customPriceAdjustmentRepository,
                                 IRepository<CustomorderOrderStatusLog> customorderOrderStatusLog,
                               IRepository<GenericAttribute> gaRepository,
                               IRepository<Customer> customerRepository,
                               IRepository<CustomOrderStatus> customOrderStatusRepository,
                               IRepository<CustomOrderOrderType> customOrderOrderTypeRepository,
                               IRepository<CustomOrderOrderSummaryAdjustment> customOrderOrderSummaryAdjustment,
                               IRepository<TaxRate> taxRateRepository,
                               IRepository<CustomOrderNotesLog> customorderNotesLog,
                               IProductExtendedService productService,
                               IProductAttributeFormatter productAttributeFormatter,
                               IWorkContext workContext, ICustomerService customerService, IPriceFormatter priceFormatter
                               )
        {
            this._customOrderRepository = customOrderRepository;
            this._customOrderShoppingCartItemRepository = customOrderShoppingCartItemRepository;
            this._customPriceAdjustmentRepository = customPriceAdjustmentRepository;
            this._customorderOrderStatusLog = customorderOrderStatusLog;
            this._gaRepository = gaRepository;
            this._customerRepository = customerRepository;
            this._customOrderStatusRepository = customOrderStatusRepository;
            this._customOrderOrderTypeRepository = customOrderOrderTypeRepository;
            this._customOrderOrderSummaryAdjustment = customOrderOrderSummaryAdjustment;
            this._taxRateRepository = taxRateRepository;
            this._customorderNotesLog = customorderNotesLog;
            this._productService = productService;
            this._productAttributeFormatter = productAttributeFormatter;
            this._workContext = workContext;
            this._customerService = customerService;
            this._priceFormatter = priceFormatter;
        }

        #endregion

        #region Methods

        #region Orders
        public async Task<CustomOrder> GetByParentLiveOrderNumber(int orderNumber)
        {
            return await _customOrderRepository.Table.Where(o => o.ParentOrderID == orderNumber).FirstOrDefaultAsync();
        }
        public async Task<List<CustomOrder>> GetByLiveOrderNumbers(IList<int> Ids)
        {
            return await _customOrderRepository.Table.Where(o => Ids.Contains(o.LiveOrderNumber ?? 0)).ToListAsync();
        }
        public async Task<CustomOrder> GetById(int Id)
        {
            return await _customOrderRepository.GetByIdAsync(Id, cache => default);
        }

        public async Task DeleteAsync(CustomOrder customOrder)
        {
            await _customOrderRepository.DeleteAsync(customOrder);
        }

        public async Task InsertAsync(CustomOrder customOrder)
        {
            await _customOrderRepository.InsertAsync(customOrder);
        }
        public async Task UpdateAsync(CustomOrder customOrder)
        {
            customOrder.Updatedon = DateTime.Now;
            await _customOrderRepository.UpdateAsync(customOrder);
        }
        public async Task UpdateWithoutEventAsync(CustomOrder customOrder)
        {
            await _customOrderRepository.UpdateAsync(customOrder, false);
        }
        public async Task<CustomOrder> GetByOrderNumber(int orderNumber)
        {
            return await _customOrderRepository.Table.Where(m => m.LiveOrderNumber == orderNumber).FirstOrDefaultAsync();
        }

        public async Task<IPagedList<CustomOrder>> SearchCustomorder(string searchterm, int customerId, int statusId,
      int pageIndex = 0, int pageSize = int.MaxValue, bool ispartial = false, bool displayAdditionalService = false, bool isDeleted = false)
        {
            if (!string.IsNullOrWhiteSpace(searchterm))
                searchterm = searchterm.Trim();
            var orders = await _customOrderRepository.GetAllPagedAsync(async query =>
    {
        var _orders = _customOrderRepository.Table.Where(o => o.StatusId != 0 && o.IsDeleted == isDeleted);
        var statuses = await this.GetOrderStatuses();
        var status = statuses.Where(m => m.Id == statusId).FirstOrDefault()?.Name;
        if (statusId != 0 && status != "PartialPaid")
            _orders = _orders.Where(o => o.StatusId == statusId);
        if (displayAdditionalService)
            _orders = _orders.Where(o => o.ParentOrderID > 0);
        else
            _orders = _orders.Where(o => o.ParentOrderID == 0);
        if (customerId != 0)
            _orders = _orders.Where(o => o.CustomerId == customerId);
        if (ispartial || status == "PartialPaid")
        {
            int orderTypeId = 0;
            var orderTypes = await GetOrderTypes();
            var orderTypeIdObj = orderTypes.Where(m => String.Compare(m.Name, OrderTypes.CustomOrder.ToString(), StringComparison.OrdinalIgnoreCase) == 0).FirstOrDefault();
            if (orderTypeIdObj != null)
                orderTypeId = orderTypeIdObj.Id;
            _orders = _orders.Where(o =>
                       o.OrderTypeId == orderTypeId
                      && o.AlreadyFee != null && o.AlreadyFee > 0
                      && (o.FullPaid == false || o.FullPaid == null)
                      && (o.LiveOrderNumber != null && o.LiveOrderNumber > 0)
                      );


        }

        if (!string.IsNullOrWhiteSpace(searchterm))
        {
            var orders = _orders.Where(o => o.Id.ToString().Contains(searchterm)
            || o.LiveOrderNumber.ToString().Contains(searchterm)
            || o.ParentOrderID.ToString().Contains(searchterm)
            ).Select(m => m.Id);

            orders = orders.Union(query
                .Join(_customerRepository.Table, x => x.CustomerId, y => y.Id,
                    (x, y) => new { Order = x, Customer = y })
                .Where(z => z.Customer.FirstName != null &&
                            (z.Customer.FirstName.Contains(searchterm, StringComparison.OrdinalIgnoreCase)
                            || z.Customer.LastName.Contains(searchterm, StringComparison.OrdinalIgnoreCase)
                            || z.Customer.ZipPostalCode.Contains(searchterm, StringComparison.OrdinalIgnoreCase)
                            ))
                .Select(z => z.Customer.Id));


            orders = orders.Union(query
 .Join(_customerRepository.Table, x => x.CustomerId, y => y.Id,
(x, y) => new { Customer = x, Attribute = y })
.Where(c => c.Attribute.Email.Contains(searchterm, StringComparison.OrdinalIgnoreCase))
.Select(z => z.Customer.Id));


            _orders =
           from o in _orders
           from oid in LinqToDB.LinqExtensions.InnerJoin(orders, oid => oid == o.Id)

           select o;
        }



        _orders = _orders.OrderByDescending(c => c.Id);

        return _orders;
    }, pageIndex, pageSize, false);

            return orders;
        }

        public async Task<CustomOrderOrderSummaryAdjustment> GetOrderSummaryAdjustment(int orderId)
        {
            return await _customOrderOrderSummaryAdjustment.Table.Where(m => m.OrderId == orderId).FirstOrDefaultAsync();
        }

        public async Task InsertCustomOrderOrderSummaryAdjustmentAsync(CustomOrderOrderSummaryAdjustment customOrderOrderSummaryAdjustment)
        {
            await _customOrderOrderSummaryAdjustment.InsertAsync(customOrderOrderSummaryAdjustment);
        }

        public async Task UpdateCustomOrderOrderSummaryAdjustmentAsync(CustomOrderOrderSummaryAdjustment customOrderOrderSummaryAdjustment)
        {
            await _customOrderOrderSummaryAdjustment.UpdateAsync(customOrderOrderSummaryAdjustment);
        }

        #endregion Orders

        #region Order Items

        public async Task<List<CustomOrderShoppingCartItem>> GetOrderItems(int orderId)
        {
            return (await _customOrderShoppingCartItemRepository.Table.Where(m => m.OrderId == orderId).OrderByDescending(m => m.Id).ToListAsync());
        }

        public async Task<CustomOrderShoppingCartItem> GetOrderItemById(int orderItemId)
        {
            return await _customOrderShoppingCartItemRepository.GetByIdAsync(orderItemId, cache => default, true, true);
        }

        public async Task DeleteOrderItemAsync(int Id, int OrderId)
        {
            var priceAdjs = await _customPriceAdjustmentRepository.Table.Where(m => m.ShoppingCartRecID == Id && m.OrderId == OrderId).ToListAsync();
            foreach (var priceAdj in priceAdjs)
            {
                await _customPriceAdjustmentRepository.DeleteAsync(priceAdj);
            }

            var customOrderShoppingCartItem = await _customOrderShoppingCartItemRepository.Table.Where(m => m.Id == Id && m.OrderId == OrderId).FirstOrDefaultAsync();
            if (customOrderShoppingCartItem != null)
                await _customOrderShoppingCartItemRepository.DeleteAsync(customOrderShoppingCartItem);
        }

        public async Task InsertOrderItemAsync(CustomOrderShoppingCartItem customOrderShoppingCartItem)
        {
            await _customOrderShoppingCartItemRepository.InsertAsync(customOrderShoppingCartItem);
        }

        public async Task UpdateCartItemAsync(CustomOrderShoppingCartItem customOrderShoppingCartItem)
        {
            await _customOrderShoppingCartItemRepository.UpdateAsync(customOrderShoppingCartItem);
        }
        public async Task UpdatePriceAdjustmentAsync(CustomOrderPriceAdjustment priceAdj)
        {
            await _customPriceAdjustmentRepository.UpdateAsync(priceAdj);
        }

        public async Task InsertPriceAdjustmentAsync(CustomOrderPriceAdjustment priceAdj)
        {
            await _customPriceAdjustmentRepository.InsertAsync(priceAdj);
        }

        #endregion

        #region Price Adjustments

        public async Task<List<CustomOrderPriceAdjustment>> GetPriceAdjustments(int orderId)
        {
            return (await _customPriceAdjustmentRepository.Table.Where(m => m.OrderId == orderId).OrderBy(m => m.Id).ToListAsync());
        }

        public async Task<CustomOrderPriceAdjustment> GetPriceAdjustmentsByCartId(int cartId)
        {
            return await _customPriceAdjustmentRepository.Table.Where(m => m.ShoppingCartRecID == cartId).FirstOrDefaultAsync();
        }


        public async Task DeletePriceAdjustmentOfItemAsync(CustomOrderPriceAdjustment customOrderPriceAdjustment)
        {
            await _customPriceAdjustmentRepository.DeleteAsync(customOrderPriceAdjustment);
        }

        public async Task InsertPriceAdjustmentOfItemAsync(CustomOrderPriceAdjustment customOrderPriceAdjustment)
        {
            await _customPriceAdjustmentRepository.InsertAsync(customOrderPriceAdjustment);
        }


        public async Task UpdatePriceAdjustmentOfItemAsync(CustomOrderPriceAdjustment customOrderPriceAdjustment)
        {
            await _customPriceAdjustmentRepository.UpdateAsync(customOrderPriceAdjustment);
        }

        #endregion

        #region OrderStatusLog

        public async Task<List<CustomorderOrderStatusLog>> GetOrderStatusLogs(int orderId)
        {
            return (await _customorderOrderStatusLog.Table.Where(m => m.OrderId == orderId).OrderByDescending(m => m.Id).ToListAsync());
        }

        public async Task<CustomorderOrderStatusLog> GetStatusOfOrder(int orderId)
        {
            return (await _customorderOrderStatusLog.Table.Where(m => m.OrderId == orderId).OrderByDescending(m => m.Id).FirstOrDefaultAsync());
        }

        public async Task DeleteOrderStatusLogAsync(CustomorderOrderStatusLog customorderOrderStatusLog)
        {
            await _customorderOrderStatusLog.DeleteAsync(customorderOrderStatusLog);
        }

        public async Task InsertOrderStatusLogAsync(CustomorderOrderStatusLog customorderOrderStatusLog)
        {
            await _customorderOrderStatusLog.InsertAsync(customorderOrderStatusLog);
        }

        public async Task UpdateOrderStatusLogAsync(CustomorderOrderStatusLog customorderOrderStatusLog)
        {
            await _customorderOrderStatusLog.UpdateAsync(customorderOrderStatusLog);
        }
        public async Task<decimal> GetPayableAmount(CustomOrder order)
        {
            decimal orderTotal = order.OrderTotal == null ? 0 : (Convert.ToDecimal(order.OrderTotal));
            decimal payableAmount = orderTotal;
            var items = await this.GetOrderItems(order.Id);
            var orderStatuses = await this.GetOrderStatuses();
            string orderStatus = orderStatuses.Where(o => o.Id == order.StatusId).FirstOrDefault()?.Name;
            var orderTypes = await this.GetOrderTypes();
            string orderType = (orderTypes.Where(o => o.Id == order.OrderTypeId)).FirstOrDefault()?.Name;
            decimal houzzFee = 0;
            if (items.Count == 0 &&
                    orderStatus != MWT.Nop.Core.Domain.CustomOrders.OrderStatus.Paid.ToString())
                return 0;
            if (orderType == OrderTypes.HouzzOrder.ToString() && order.HouzzFee != null && order.HouzzFee > 0)
            {
                houzzFee = order.HouzzFeeType == DiscountType.Percentage.ToString() ? (orderTotal * Convert.ToDecimal(order.HouzzFee)) / 100 : Convert.ToDecimal(order.HouzzFee);
                orderTotal = orderTotal - houzzFee;
                payableAmount = orderTotal;
            }
            if ((orderType == OrderTypes.AlreadyPaid.ToString() || orderType == OrderTypes.CustomOrder.ToString())
                && order.AlreadyFee != null && order.AlreadyFee > 0)
            {
                decimal initialPayment = (orderTotal * Convert.ToDecimal(order.AlreadyFee)) / 100;
                decimal pendingPayment = orderTotal - initialPayment;
                if (order.LiveOrderNumber == null || order.LiveOrderNumber == 0)
                    payableAmount = initialPayment;
                else if (!order.FullPaid)
                    payableAmount = pendingPayment;
                else
                    payableAmount = 0;

            }
            var priceCalculationService = EngineContext.Current.Resolve<IPriceCalculationService>();
            return await priceCalculationService.RoundPriceAsync(payableAmount);
        }

        public async Task<bool> IsOrderPaid(CustomOrder order)
        {
            var orderStatuses = await this.GetOrderStatuses();
            string orderStatus = orderStatuses.Where(o => o.Id == order.StatusId).FirstOrDefault()?.Name;
            return orderStatus == MWT.Nop.Core.Domain.CustomOrders.OrderStatus.Paid.ToString() && order.FullPaid ? true : false;
        }
        #endregion

        #region notes
        public async Task InsertOrderNotesLogAsync(CustomOrderNotesLog customorderOrderNotesLog)
        {
            await _customorderNotesLog.InsertAsync(customorderOrderNotesLog);
        }

        public async Task<List<CustomOrderNotesLog>> GetOrderNotesLogs(int orderId)
        {
            return await _customorderNotesLog.Table.Where(n => n.OrderId == orderId).OrderByDescending(o => o.CreatedOn).ToListAsync();
        }

        #endregion

        #region CustomOrderStatus

        public async Task<List<CustomOrderStatus>> GetOrderStatuses()
        {
            // Cache Pending
            return await _customOrderStatusRepository.Table.ToListAsync();
        }

        #endregion


        #region CustomOrderStatus

        public async Task<List<CustomOrderOrderType>> GetOrderTypes()
        {
            // Cache Pending
            return await _customOrderOrderTypeRepository.Table.ToListAsync();
        }

        #endregion

        #region Calculate Tax

        public async Task<decimal> GetTaxRate(int countryId, int stateId, string zip)
        {
            var obj = await _taxRateRepository.Table.Where(t => t.Zip.ToLower() == zip.Trim().ToLower() && t.CountryId == countryId && t.StateProvinceId == stateId).FirstOrDefaultAsync();
            if (obj == null)
                obj = await _taxRateRepository.Table.Where(t => t.CountryId == countryId && t.StateProvinceId == stateId).FirstOrDefaultAsync();
            if (obj == null)
                return 0;
            else
                return obj.Percentage;


        }

        #endregion

        #region Wgs Service

        public async Task<bool> IsSurchargeApplicable(CustomOrder order)
        {
            bool isSurchargeApplicable = false;
            try
            {
                foreach (var item in await this.GetOrderItems(order.Id))
                {
                    if (!string.IsNullOrEmpty(item.AttributesDescription))
                    {
                        int variantId = await _productService.GetVariantId(item.ProductId, item.AttributesDescription);
                        if (variantId > 0)
                        {
                            var variantCombination = await _productService.GetProductVariants(item.ProductId);
                            if ((variantCombination.Where(v => v.VariantId == variantId).FirstOrDefault()?.EnableSurcharge ?? false))
                            {
                                isSurchargeApplicable = true;
                                break;
                            }
                        }
                    }
                }
            }
            catch
            {
            }
            return isSurchargeApplicable;
        }

        #endregion


        public async Task<dynamic> PrepareBriefOderSummaryModel(int orderId, bool isCustomerPaying = false)
        {
            dynamic model = new ExpandoObject();
            var order = await this.GetById(orderId);
            if (order != null)
            {
                model.ApplyTax = order.ApplyTax;
                var items = await this.GetOrderItems(orderId);
                var orderStatuses = await this.GetOrderStatuses();
                model.Id = orderId;
                model.OrderStatus = orderStatuses.Where(o => o.Id == order.StatusId).FirstOrDefault()?.Name;
                model.FullPaid = order.FullPaid;
                if (items.Count == 0 &&
                    model.OrderStatus != Nop.Core.Domain.CustomOrders.OrderStatus.Paid.ToString())
                    return null;

                decimal orderTotal = order.OrderTotal == null ? 0 : (Convert.ToDecimal(order.OrderTotal));
                decimal subtotal = order.SubTotal == null ? 0 : (Convert.ToDecimal(order.SubTotal));
                decimal houzzFee = 0;
                decimal initialPayment = 0;
                decimal pendingPayment = 0;
                decimal payableAmount = orderTotal;
                var orderTypes = await this.GetOrderTypes();
                model.OrderType = (orderTypes.Where(o => o.Id == order.OrderTypeId)).FirstOrDefault()?.Name;
                model.ShippingMethod = order.ShippingMethod;
                model.WgsAdjustmentNotes = order.WgsAdjustmentNotes;

                var customer = new Customer();
                if (order.CustomerId != null)
                    customer = await this._customerService.GetCustomerByIdAsync(Convert.ToInt32(order.CustomerId));
                model.Customer = customer;

                if (model.OrderType == OrderTypes.HouzzOrder.ToString() && order.HouzzFee != null && order.HouzzFee > 0)
                {
                    houzzFee = order.HouzzFeeType == DiscountType.Percentage.ToString() ? (orderTotal * Convert.ToDecimal(order.HouzzFee)) / 100 : Convert.ToDecimal(order.HouzzFee);
                    orderTotal = orderTotal - houzzFee;
                    payableAmount = orderTotal;
                }
                if ((model.OrderType == OrderTypes.AlreadyPaid.ToString() || model.OrderType == OrderTypes.CustomOrder.ToString())
                    && order.AlreadyFee != null && order.AlreadyFee > 0)
                {
                    initialPayment = (orderTotal * Convert.ToDecimal(order.AlreadyFee)) / 100;
                    pendingPayment = orderTotal - initialPayment;
                    if (order.LiveOrderNumber == null || order.LiveOrderNumber == 0)
                        payableAmount = initialPayment;
                    else if (!order.FullPaid)
                        payableAmount = pendingPayment;
                    else
                        payableAmount = 0;

                }

                model.SubTotal = await _priceFormatter.FormatPriceAsync(subtotal);
                model.OrderTotal = await _priceFormatter.FormatPriceAsync(orderTotal);
                model.PayableAmount = await _priceFormatter.FormatPriceAsync(payableAmount);
                if (initialPayment > 0)
                    model.InitialPayment = await _priceFormatter.FormatPriceAsync(initialPayment);
                if (pendingPayment > 0)
                    model.PendingPayment = await _priceFormatter.FormatPriceAsync(pendingPayment);
                if (houzzFee > 0)
                    model.HouzzFee = await _priceFormatter.FormatPriceAsync(houzzFee);

                model.ComplementryWgsFree = order.ComplementryWgsFree;
                if (order.Wgs != null && order.Wgs > 0)
                {
                    model.Wgs = await _priceFormatter.FormatPriceAsync(Convert.ToDecimal(order.Wgs));
                }
                else if (order.ComplementryWgsFree)
                {
                    model.Wgs = await _priceFormatter.FormatPriceAsync(0);
                }


                if (order.Shipping != null && order.Shipping > 0)
                    model.Shipping = await _priceFormatter.FormatPriceAsync(Convert.ToDecimal(order.Shipping));

                // Discount 
                var orderSummaryAdj = await this.GetOrderSummaryAdjustment(order.Id);
                if (orderSummaryAdj != null)
                {
                    #region SubTotalAdj

                    string subTotalDiscountType = orderSummaryAdj.SubTotalDiscountType == null ? "" :
                       (orderSummaryAdj.SubTotalDiscountType == DiscountType.Percentage.ToString() ? DiscountType.Percentage.ToString() : DiscountType.Fixed.ToString());
                    string totalAdjustment = "";
                    decimal discountAmount = orderSummaryAdj.SubtotalDiscount == null ? 0 : Convert.ToDecimal(orderSummaryAdj.SubtotalDiscount);
                    if (discountAmount != 0)
                    {
                        totalAdjustment = await _priceFormatter.FormatPriceAsync(
                            subTotalDiscountType == DiscountType.Percentage.ToString() ?
                            (subtotal * discountAmount) / 100
                            : discountAmount);
                    }

                    dynamic subTotalDiscountDetails = new ExpandoObject();
                    subTotalDiscountDetails.DiscountType = subTotalDiscountType;
                    subTotalDiscountDetails.DiscountAmount = discountAmount;
                    subTotalDiscountDetails.TotalAdjustment = totalAdjustment;
                    subTotalDiscountDetails.ChargeType = orderSummaryAdj.SubtotalChargeType == null ? "" : (orderSummaryAdj.SubtotalChargeType == ChargeType.Subtract.ToString() ?
                        ChargeType.Subtract.ToString() : ChargeType.Add.ToString());
                    subTotalDiscountDetails.Notes = orderSummaryAdj.SubTotalAdjustmentNotes;

                    model.SubTotalDiscountDetails = subTotalDiscountDetails;


                    #endregion

                    #region ShippingAdj

                    string shippingDiscountType = orderSummaryAdj.ShippingDiscountType == null ? "" :
                       (orderSummaryAdj.ShippingDiscountType == DiscountType.Percentage.ToString() ? DiscountType.Percentage.ToString() : DiscountType.Fixed.ToString());

                    discountAmount = orderSummaryAdj.ShippingDiscount == null ? 0 : Convert.ToDecimal(orderSummaryAdj.ShippingDiscount);
                    totalAdjustment = "";
                    if (discountAmount != 0)
                    {
                        totalAdjustment = await _priceFormatter.FormatPriceAsync(shippingDiscountType == DiscountType.Percentage.ToString() ?
                            ((order.Shipping == null ? 0 : Convert.ToDecimal(order.Shipping)) * discountAmount) / 100
                            : discountAmount);
                    }
                    dynamic shippingDiscountDetails = new ExpandoObject();

                    shippingDiscountDetails.DiscountType = shippingDiscountType;
                    shippingDiscountDetails.DiscountAmount = discountAmount;
                    shippingDiscountDetails.TotalAdjustment = totalAdjustment;
                    shippingDiscountDetails.ChargeType = orderSummaryAdj.ShippingChargeType == null ? "" : (orderSummaryAdj.ShippingChargeType == ChargeType.Subtract.ToString() ?
                        ChargeType.Subtract.ToString() : ChargeType.Add.ToString());
                    shippingDiscountDetails.Notes = orderSummaryAdj.ShippingAdjustmentNotes;

                    model.ShippingDiscountDetails = shippingDiscountDetails;


                    #endregion
                }

                model.Tax = order.OrderTax == null ? "" : await _priceFormatter.FormatPriceAsync(Convert.ToDecimal(order.OrderTax));
                model.CustomDuty = order.CustomDuty <= 0 ? "" : await _priceFormatter.FormatPriceAsync(order.CustomDuty);
                model.CustomDutyPercentage = order.CustomDutyPercentage;
                model.TaxRate = order.TaxRate > 0 ? order.TaxRate : 0;

                model.InvoiceNotes = order.InvoiceNote;
                model.SpecialInstructionsfromBuyer = order.SpecialInstructionsfromBuyer;
                model.PrivateNotes = order.PrivateOrderNotes;
                model.ParentOrderID = order.ParentOrderID;

                // end
            }


            return model;

        }
        #endregion Methods





    }
}
