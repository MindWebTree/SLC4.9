using MWT.Nop.Core.Domain.KW;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Directory;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Payments;
using Nop.Core.Domain.Shipping;
using Nop.Core.Infrastructure;
using Nop.Data;
using Nop.Services.Catalog;
using Nop.Services.Directory;
using Nop.Services.Helpers;
using Nop.Services.Orders;
using Nop.Services.Stores;

namespace MWT.Nop.Core.Services.Orders
{
    public partial class OrderReportExtendedService : OrderReportService, IOrderReportExtendedService
    {
        #region Fields

        private readonly IRepository<ProductKwTerm> _productKwTermRepository;

        #endregion

        #region Ctor
        public OrderReportExtendedService(CurrencySettings currencySettings, ICurrencyService currencyService, IDateTimeHelper dateTimeHelper, IPriceFormatter priceFormatter,
            IRepository<Address> addressRepository, IRepository<Order> orderRepository, IRepository<OrderItem> orderItemRepository, IRepository<OrderNote> orderNoteRepository,
            IRepository<Product> productRepository, IRepository<ProductCategory> productCategoryRepository, IRepository<ProductManufacturer> productManufacturerRepository,
            IRepository<ProductWarehouseInventory> productWarehouseInventoryRepository, IStoreMappingService storeMappingService, IWorkContext workContext,
            IRepository<ProductKwTerm> productKwTermRepository) :
            base(currencySettings, currencyService, dateTimeHelper, priceFormatter, addressRepository, orderRepository, orderItemRepository, orderNoteRepository, productRepository, productCategoryRepository, productManufacturerRepository, productWarehouseInventoryRepository, storeMappingService, workContext)
        {
            _productKwTermRepository = productKwTermRepository;
        }

        #endregion

        #region Methods
        public virtual async Task<IPagedList<BestsellersReportLine>> CustomBestSellersReportAsync(
         int categoryId = 0,
           int kwTermId = 0,
         int manufacturerId = 0,
         int storeId = 0,
         int vendorId = 0,
         DateTime? createdFromUtc = null,
         DateTime? createdToUtc = null,
         OrderStatus? os = null,
         PaymentStatus? ps = null,
         ShippingStatus? ss = null,
         int billingCountryId = 0,
         OrderByEnum orderBy = OrderByEnum.OrderByQuantity,
         int pageIndex = 0,
         int pageSize = int.MaxValue,
         bool showHidden = false)
        {

            var bestSellers = CustomSearchOrderItems(categoryId, kwTermId, manufacturerId, storeId, vendorId, createdFromUtc, createdToUtc, os, ps, ss, billingCountryId, showHidden);

            var bsReport =
                //group by products
                from orderItem in bestSellers
                group orderItem by orderItem.ProductId into g
                select new BestsellersReportLine
                {
                    ProductId = g.Key,
                    TotalAmount = g.Sum(x => x.PriceExclTax),
                    TotalQuantity = g.Sum(x => x.Quantity)
                };

            bsReport = orderBy switch
            {
                OrderByEnum.OrderByQuantity => bsReport.OrderByDescending(x => x.TotalQuantity),
                OrderByEnum.OrderByTotalAmount => bsReport.OrderByDescending(x => x.TotalAmount),
                _ => throw new ArgumentException("Wrong orderBy parameter", nameof(orderBy)),
            };

            var result = await bsReport.ToPagedListAsync(pageIndex, pageSize);

            return result;
        }

        #endregion

        #region Utilities
        private IQueryable<OrderItem> CustomSearchOrderItems(
    int categoryId = 0,
     int kwtermId = 0,
    int manufacturerId = 0,
    int storeId = 0,
    int vendorId = 0,
    DateTime? createdFromUtc = null,
    DateTime? createdToUtc = null,
    OrderStatus? os = null,
    PaymentStatus? ps = null,
    ShippingStatus? ss = null,
    int billingCountryId = 0,
    bool showHidden = false)
        {
            int? orderStatusId = null;
            if (os.HasValue)
                orderStatusId = (int)os.Value;

            int? paymentStatusId = null;
            if (ps.HasValue)
                paymentStatusId = (int)ps.Value;

            int? shippingStatusId = null;
            if (ss.HasValue)
                shippingStatusId = (int)ss.Value;

            var orderItems = from orderItem in _orderItemRepository.Table
                             join o in _orderRepository.Table on orderItem.OrderId equals o.Id
                             join p in _productRepository.Table on orderItem.ProductId equals p.Id
                             join oba in _addressRepository.Table on o.BillingAddressId equals oba.Id
                             where (storeId == 0 || storeId == o.StoreId) &&
                                 (!createdFromUtc.HasValue || createdFromUtc.Value <= o.CreatedOnUtc) &&
                                 (!createdToUtc.HasValue || createdToUtc.Value >= o.CreatedOnUtc) &&
                                 (!orderStatusId.HasValue || orderStatusId == o.OrderStatusId) &&
                                 (!paymentStatusId.HasValue || paymentStatusId == o.PaymentStatusId) &&
                                 (!shippingStatusId.HasValue || shippingStatusId == o.ShippingStatusId) &&
                                 !o.Deleted && !p.Deleted &&
                                 (vendorId == 0 || p.VendorId == vendorId) &&
                                 (billingCountryId == 0 || oba.CountryId == billingCountryId) &&
                                 (showHidden || p.Published)
                             select orderItem;

            if (categoryId > 0)
            {
                orderItems = from orderItem in orderItems
                             join p in _productRepository.Table on orderItem.ProductId equals p.Id
                             join pc in _productCategoryRepository.Table on p.Id equals pc.ProductId
                             into p_pc
                             from pc in p_pc.DefaultIfEmpty()
                             where pc.CategoryId == categoryId
                             select orderItem;
            }
            if (kwtermId > 0)
            {

                orderItems = from orderItem in orderItems
                             join p in _productRepository.Table on orderItem.ProductId equals p.Id
                             join pk in _productKwTermRepository.Table on p.Id equals pk.ProductId
                             into p_pk
                             from pk in p_pk.DefaultIfEmpty()
                             where pk.KwTermId == kwtermId
                             select orderItem;
            }
            if (manufacturerId > 0)
            {
                orderItems = from orderItem in orderItems
                             join p in _productRepository.Table on orderItem.ProductId equals p.Id
                             join pm in _productManufacturerRepository.Table on p.Id equals pm.ProductId
                             into p_pm
                             from pm in p_pm.DefaultIfEmpty()
                             where pm.ManufacturerId == manufacturerId
                             select orderItem;
            }

            return orderItems;
        }
        #endregion
    }
}
