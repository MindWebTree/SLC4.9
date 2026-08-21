using MWT.Nop.Core.Service.Catalog;
using MWT.Nop.Core.Services.Catalog;
using MWT.Nop.Core.Services.Customers;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Logging;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Shipping;
using Nop.Core.Infrastructure;
using Nop.Data;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.Html;
using Nop.Services.Logging;
using Nop.Services.Orders;
using Nop.Services.Shipping;
using Nop.Services.Tax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MWT.Nop.Core.Services.Orders
{
    public partial class OrderExtendedService : OrderService, IOrderExtendedService
    {
        #region Fields

        private readonly IStaticCacheManager _staticCacheManager;
        private readonly ICurrencyService _currencyService;
        private readonly IShippingPluginManager _shippingPluginManager;
        private readonly ILogger _logger;
        private readonly IShoppingCartExtendedService _shoppingCartService;
        private readonly ICustomerExtendedService _customerService;
        private readonly IStoreContext _storeContext;
        private readonly ICustomProductAttributeFormatter _productAttributeFormatter;
        private readonly IRepository<Shipment> _shipmentRepository;
        private readonly IShippingService _shippingService;
        private readonly IAddressService _addressService;
        private readonly ICountryService _countryService;
        private readonly IStateProvinceService _stateProvinceService;
        private readonly IPriceCalculationService _priceCalculationService;
        private readonly ShoppingCartSettings _shoppingCartSettings;
        private readonly ShippingSettings _shippingSettings;
        private readonly IProductExtendedService _productExtendedService;
        private readonly IWarehouseService _warehouseService;
        private readonly ICustomProductAttributeParser  _productAttributeExtendedParser;

        #endregion

        #region     Ctor
        public OrderExtendedService(IHtmlFormatter htmlFormatter, IProductService productService, IRepository<Address> addressRepository, IRepository<Customer> customerRepository,
            IRepository<Order> orderRepository, IRepository<OrderItem> orderItemRepository, IRepository<OrderNote> orderNoteRepository, IRepository<Product> productRepository,
            IRepository<ProductWarehouseInventory> productWarehouseInventoryRepository, IRepository<RecurringPayment> recurringPaymentRepository,
            IRepository<RecurringPaymentHistory> recurringPaymentHistoryRepository, IShipmentService shipmentService,
            IStaticCacheManager staticCacheManager, ICurrencyService currencyService, IShippingPluginManager shippingPluginManager, ILogger logger,
            IShoppingCartExtendedService shoppingCartService, ICustomerExtendedService customerService,
            IStoreContext storeContext, ICustomProductAttributeFormatter productAttributeFormatter, IRepository<Shipment> shipmentRepository,
            IShippingService shippingService, IAddressService addressService, ICountryService countryService, IStateProvinceService stateProvinceService,
            IPriceCalculationService priceCalculationService, ShoppingCartSettings shoppingCartSettings, ShippingSettings shippingSettings, IProductExtendedService productExtendedService, IWarehouseService warehouseService
            , ICustomProductAttributeParser productAttributeExtendedParser) :
            base(htmlFormatter, productService, addressRepository, customerRepository, orderRepository, orderItemRepository, orderNoteRepository, productRepository,
                productWarehouseInventoryRepository, recurringPaymentRepository, recurringPaymentHistoryRepository, shipmentService)
        {
            _staticCacheManager = staticCacheManager;
            _currencyService = currencyService;
            _shippingPluginManager = shippingPluginManager;
            _logger = logger;
            _shoppingCartService = shoppingCartService;
            _customerService = customerService;
            _storeContext = storeContext;
            _productAttributeFormatter = productAttributeFormatter;
            _shipmentRepository = shipmentRepository;
            _shippingService = shippingService;
            _addressService = addressService;
            _countryService = countryService;
            _stateProvinceService = stateProvinceService;
            _priceCalculationService = priceCalculationService;
            _shoppingCartSettings = shoppingCartSettings;
            _shippingSettings = shippingSettings;
            _productExtendedService = productExtendedService;
            _warehouseService = warehouseService;
            _productAttributeExtendedParser = productAttributeExtendedParser;
        }

        #endregion

        #region Method

        public async Task<(int nofOfOrders, decimal amount)> GetCustomerStats(int customerId)
        {
            var query = _orderRepository.Table;
            query = query.Where(o => o.CustomerId == customerId);
            List<decimal> orders = await query.Select(o => o.OrderTotal).ToListAsync();
            return (orders.Count(), orders.Sum());
        }


        public async Task<Order> GetRecentOrderOfCustomer(int customerId)
        {
            var key = _staticCacheManager.PrepareKeyForDefaultCache(CustomNopCatalogDefaults.CustomerRecentOrderCacheKey,
             customerId);
            var query = _orderRepository.Table;
            query = query.Where(o => o.CustomerId == customerId).OrderByDescending(o => o.Id);

            return await _staticCacheManager
                  .GetAsync(key, async () => await query.FirstOrDefaultAsync());
        }

        public async Task<List<int>> GetNewOrdersIds()
        {
            var query = _orderRepository.Table.Where(o => (o.IsImported == null ? false : o.IsImported) == false).Select(o => o.Id);
            return await query.ToListAsync();
        }
        public virtual async Task<Order> GetOrderByGuidAsync(string orderGuid)
        {
            if (string.IsNullOrEmpty(orderGuid))
                return null;

            return await _orderRepository.Table
                .FirstOrDefaultAsync(o => o.OrderGuid.ToString().StartsWith(orderGuid));
        }

        public List<TaxInfo> GetTaxDetails(Order order)
        {
            var _currencyService = EngineContext.Current.Resolve<ICurrencyService>();
            var taxdetails = new List<TaxInfo>();
            if (string.IsNullOrEmpty(order.TaxInfo))
            {
                return new List<TaxInfo>();
            }
            else
            {
                foreach (var taxinfo in order.TaxInfo.Split(';'))
                {
                    var parts = taxinfo.Split(':');
                    if (parts.Length == 3)
                    {
                        TaxType type = string.Equals(parts[0], TaxType.GST.ToString()) ? TaxType.GST :
                                        (string.Equals(parts[0], TaxType.HST.ToString()) ? TaxType.HST : (string.Equals(parts[0], TaxType.PST.ToString()) ? TaxType.PST
                                        : (string.Equals(parts[0], TaxType.QST.ToString()) ? TaxType.QST : TaxType.Tax)
                                        ));

                        if (decimal.TryParse(parts[1], out decimal _taxRate) && decimal.TryParse(parts[2], out decimal _taxAmount))
                        {

                            taxdetails.Add(new TaxInfo()
                            {
                                Amount = _currencyService.ConvertCurrency(_taxAmount, order.CurrencyRate),
                                TaxType = type,
                                TaxRate = _taxRate
                            });
                        }

                    }
                }
            }
            return taxdetails;
        }
        public List<TaxInfo> GetTaxDetails(string taxInfo)
        {
            var _currencyService = EngineContext.Current.Resolve<ICurrencyService>();
            var taxdetails = new List<TaxInfo>();
            if (string.IsNullOrEmpty(taxInfo))
            {
                return new List<TaxInfo>();
            }
            else
            {
                foreach (var taxinfo in taxInfo.Split(';'))
                {
                    var parts = taxinfo.Split(':');
                    if (parts.Length == 3)
                    {
                        TaxType type = string.Equals(parts[0], TaxType.GST.ToString()) ? TaxType.GST :
                                        (string.Equals(parts[0], TaxType.HST.ToString()) ? TaxType.HST : (string.Equals(parts[0], TaxType.PST.ToString()) ? TaxType.PST
                                        : (string.Equals(parts[0], TaxType.QST.ToString()) ? TaxType.QST : TaxType.Tax)
                                        ));

                        if (decimal.TryParse(parts[1], out decimal _taxRate) && decimal.TryParse(parts[2], out decimal _taxAmount))
                        {

                            taxdetails.Add(new TaxInfo()
                            {
                                Amount = _taxAmount,
                                TaxType = type,
                                TaxRate = _taxRate
                            });
                        }

                    }
                }
            }
            return taxdetails;
        }

        #region Wgs Service
        public async Task<(decimal wgsCharges, decimal defaultWgsCharges, decimal surchargeAmount)> GetWgsCharges(Order order, decimal subTotal, bool isSurchargeApplicable = false)
        {

            decimal wgsCharges = 0;
            decimal defaultWgsCharges = 0;
            decimal surchargeAmount = 0;
            bool haveCartItems = false;
            if (order.CustomerId > 0)
            {
                var customer = await _customerService.GetCustomerByIdAsync(Convert.ToInt32(order.CustomerId));
                var storeId = (await _storeContext.GetCurrentStoreAsync()).Id;
                var shippingRateComputationMethods = await _shippingPluginManager
            .LoadActivePluginsAsync(customer, storeId, "");
                if (shippingRateComputationMethods.Any())
                { 

                    var result = new GetShippingOptionResponse();

                    var cart = await _shoppingCartService.GetShoppingCartAsync(customer, ShoppingCartType.ShoppingCart, storeId);
                    if (cart.Count == 0)
                    {
                        foreach (var item in await this.GetOrderItemsAsync(order.Id))
                        {
                            await _shoppingCartService.CustomAddToCartAsync(customer, await _productService.GetProductByIdAsync(item.ProductId), ShoppingCartType.ShoppingCart, storeId, item.AttributeDescription, item.PriceInclTax, null, null, item.Quantity, false);
                        }
                    }
                    else
                    {
                        haveCartItems = false;
                    }
                    cart = await _shoppingCartService.GetShoppingCartAsync(customer, ShoppingCartType.ShoppingCart, storeId);
                    //create a package
                    var (shippingOptionRequests, shippingFromMultipleLocations) = await CreateShippingOptionRequestsAsync(cart, await _customerService.GetCustomerShippingAddressAsync(customer)
                        , storeId);

                    if (isSurchargeApplicable)
                    {
                        foreach (var shippingOptionRequest in shippingOptionRequests)
                        {
                            shippingOptionRequest.IsSurchargeApplicable = isSurchargeApplicable;
                        }
                    }
                    result.ShippingFromMultipleLocations = shippingFromMultipleLocations;

                    foreach (var srcm in shippingRateComputationMethods)
                    {
                        //request shipping options (separately for each package-request)
                        IList<ShippingOption> srcmShippingOptions = null;
                        foreach (var shippingOptionRequest in shippingOptionRequests)
                        {
                            shippingOptionRequest.UseCustomSubtotal = true;
                            shippingOptionRequest.CustomSubtotal = subTotal;
                            var getShippingOptionResponse = await srcm.GetShippingOptionsAsync(shippingOptionRequest);

                            if (getShippingOptionResponse.Success)
                            {
                                //success
                                if (srcmShippingOptions == null)
                                {
                                    //first shipping option request
                                    srcmShippingOptions = getShippingOptionResponse.ShippingOptions;
                                }
                                else
                                {
                                    //get shipping options which already exist for prior requested packages for this scrm (i.e. common options)
                                    srcmShippingOptions = srcmShippingOptions
                                        .Where(existingso => getShippingOptionResponse.ShippingOptions.Any(newso => newso.Name == existingso.Name))
                                        .ToList();

                                    //and sum the rates
                                    foreach (var existingso in srcmShippingOptions)
                                    {
                                        existingso.Rate += getShippingOptionResponse
                                            .ShippingOptions
                                            .First(newso => newso.Name == existingso.Name)
                                            .Rate;
                                    }
                                }
                            }
                            else
                            {
                                //errors
                                foreach (var error in getShippingOptionResponse.Errors)
                                {
                                    result.AddError(error);
                                    await _logger.WarningAsync($"Shipping ({srcm.PluginDescriptor.FriendlyName}). {error}");
                                }
                                //clear the shipping options in this case
                                srcmShippingOptions = new List<ShippingOption>();
                                break;
                            }
                        }

                        //add this scrm's options to the result
                        if (srcmShippingOptions == null)
                            continue;

                        foreach (var so in srcmShippingOptions)
                        {
                            //set system name if not set yet
                            if (string.IsNullOrEmpty(so.ShippingRateComputationMethodSystemName))
                                so.ShippingRateComputationMethodSystemName = srcm.PluginDescriptor.SystemName;
                            if (_shoppingCartSettings.RoundPricesDuringCalculation)
                                so.Rate = await _priceCalculationService.RoundPriceAsync(so.Rate);
                            result.ShippingOptions.Add(so);
                        }
                    }

                    if (result.ShippingOptions.Any())
                    {
                        foreach (var option in result.ShippingOptions)
                        {
                            if (option.Name.ToLower().IndexOf("white glove service") >= 0
                              || option.Name.ToLower().IndexOf("wgs") >= 0

                                )
                            {
                                wgsCharges += option.Rate;
                                defaultWgsCharges += option.DefaultAmount;
                                surchargeAmount += option.SurchargeAmount;
                                break;
                            }
                        }
                    }



                }
                if (!haveCartItems)
                {
                    var cartItems = await _shoppingCartService.GetShoppingCartAsync(customer, ShoppingCartType.ShoppingCart, (await _storeContext.GetCurrentStoreAsync()).Id);
                    foreach (var cartItem in cartItems)
                    {
                        await _shoppingCartService.DeleteShoppingCartItemAsync(cartItem);
                    }
                }
            }


            return (wgsCharges, defaultWgsCharges, surchargeAmount);
        }
        public async Task<bool> IsSurchargeApplicable(Order order)
        {
            bool isSurchargeApplicable = false;
            try
            {
                foreach (var item in await this.GetOrderItemsAsync(order.Id))
                {
                    string attributeDescription = await _productAttributeFormatter.CustomFormatAttributesAsync(await _productService.GetProductByIdAsync(item.ProductId), item.AttributesXml);
                    int variantId = await _productExtendedService.GetVariantId(item.ProductId, attributeDescription);
                    if (variantId > 0)
                    {
                        var variantCombination = await _productExtendedService.GetProductVariants(item.ProductId);
                        if ((variantCombination.Where(v => v.VariantId == variantId).FirstOrDefault()?.EnableSurcharge ?? false))
                        {
                            isSurchargeApplicable = true;
                            break;
                        }
                    }
                }
            }
            catch (Exception exp)
            {
                var _logger = EngineContext.Current.Resolve<ILogger>();
                await _logger.InsertLogAsync(LogLevel.Error, "Wgs Additional surcharge check request failed.", $"Failed to process the request to check if surcharge is applicable on order items. Order Info :{order.Id}");
            }
            finally
            {

            }
            return isSurchargeApplicable;
        }

        public async Task<List<int>> GetLatestDeliveredOrderIdByCustomerEmail(string email)
        {
            return await _orderRepository.Table
            .Where(o => o.CustomerEmail == email && o.OrderStatusId == (int)OrderStatus.Complete)
            .OrderByDescending(o => o.CreatedOnUtc)
            .Select(o => o.Id)
            .Take(10)
            .ToListAsync();
        }

        public async Task<Order> GetOrderByTransactionId(string transactionId, string paymentMethod)
        {
            return await _orderRepository.Table
                     .Where(o => (o.CaptureTransactionId == transactionId || o.AuthorizationTransactionId == transactionId) && (o.PaymentMethodSystemName == paymentMethod || string.IsNullOrEmpty(paymentMethod))).FirstOrDefaultAsync();
        }


        #endregion

        #region Post Purchae Journey
        public async Task<IList<Order>> GetLast10DaysOrders()
        {
            return await _orderRepository.Table
           .Where(o => o.CreatedOnUtc > DateTime.Now.Date.AddDays(-11))
           .OrderByDescending(o => o.CreatedOnUtc)
           .ToListAsync();
        }

        #endregion
        #endregion
        #region Post Delivery Journey
        public async Task<IList<Order>> GetShippedOrdersForLastNDays(int day)
        {
            var _shipmentRepository = EngineContext.Current.Resolve<IRepository<Shipment>>();
            return await
                (from o in _orderRepository.Table
                 join s in _shipmentRepository.Table
                 on o.Id equals s.OrderId
                 where s.DeliveryDateUtc > DateTime.Now.Date.AddDays(-day)
                 orderby s.DeliveryDateUtc descending
                 select new Order { Id = o.Id, CreatedOnUtc = (DateTime)s.DeliveryDateUtc }).ToListAsync();
        }

        #endregion
        #region Utilities

        protected virtual async Task<(IList<GetShippingOptionRequest> shipmentPackages, bool shippingFromMultipleLocations)> CreateShippingOptionRequestsAsync(IList<ShoppingCartItem> cart,
Address shippingAddress, int storeId)
        {
          
            //if we always ship from the default shipping origin, then there's only one request
            //if we ship from warehouses ("ShippingSettings.UseWarehouseLocation" enabled),
            //then there could be several requests

            //key - warehouse identifier (0 - default shipping origin)
            //value - request
            var requests = new Dictionary<int, GetShippingOptionRequest>();

            //a list of requests with products which should be shipped separately
            var separateRequests = new List<GetShippingOptionRequest>();

            foreach (var sci in cart)
            {

             
                if (!await _shippingService.IsShipEnabledAsync(sci))
                    continue;

                var product = await _productService.GetProductByIdAsync(sci.ProductId);

                if (product == null || !product.IsShipEnabled)
                {
                    var associatedProducts = await (await _productAttributeExtendedParser.ParseProductAttributeValuesAsync(sci.AttributesXml))
                        .Where(attributeValue => attributeValue.AttributeValueType == AttributeValueType.AssociatedToProduct)
                        .SelectAwait(async attributeValue => await _productService.GetProductByIdAsync(attributeValue.AssociatedProductId)).ToListAsync();
                    product = associatedProducts.FirstOrDefault(associatedProduct => associatedProduct != null && associatedProduct.IsShipEnabled);
                }

                if (product == null)
                    continue;

                //warehouses
                Warehouse warehouse = null;
                if (_shippingSettings.UseWarehouseLocation)
                {
                    if (product.ManageInventoryMethod == ManageInventoryMethod.ManageStock &&
                        product.UseMultipleWarehouses)
                    {
                        var allWarehouses = new List<Warehouse>();
                        //multiple warehouses supported
                        foreach (var pwi in await _productService.GetAllProductWarehouseInventoryRecordsAsync(product.Id))
                        {
                            var tmpWarehouse = await _warehouseService.GetWarehouseByIdAsync(pwi.WarehouseId);
                            if (tmpWarehouse != null)
                                allWarehouses.Add(tmpWarehouse);
                        }

                        warehouse = await _warehouseService.GetNearestWarehouseAsync(shippingAddress, allWarehouses);
                    }
                    else
                    {
                        //multiple warehouses are not supported
                        warehouse = await _warehouseService.GetWarehouseByIdAsync(product.WarehouseId);
                    }
                }

                var warehouseId = warehouse?.Id ?? 0;

                if (requests.ContainsKey(warehouseId) && !product.ShipSeparately)
                {
                    //add item to existing request
                    requests[warehouseId].Items.Add(new GetShippingOptionRequest.PackageItem(new ShoppingCartItem(), product));
                }
                else
                {
                    //create a new request
                    var request = new GetShippingOptionRequest
                    {
                        //store
                        StoreId = storeId
                    };
                    //customer
                    request.Customer = await _customerService.GetShoppingCartCustomerAsync(cart);

                    //ship to
                    request.ShippingAddress = shippingAddress;
                    //ship from
                    Address originAddress = null;
                    if (warehouse != null)
                    {
                        //warehouse address
                        originAddress = await _addressService.GetAddressByIdAsync(warehouse.AddressId);
                        request.WarehouseFrom = warehouse;
                    }

                    if (originAddress == null)
                    {
                        //no warehouse address. in this case use the default shipping origin
                        originAddress = await _addressService.GetAddressByIdAsync(_shippingSettings.ShippingOriginAddressId);
                    }

                    if (originAddress != null)
                    {
                        request.CountryFrom = await _countryService.GetCountryByAddressAsync(originAddress);
                        request.StateProvinceFrom = await _stateProvinceService.GetStateProvinceByAddressAsync(originAddress);
                        request.ZipPostalCodeFrom = originAddress.ZipPostalCode;
                        request.CountyFrom = originAddress.County;
                        request.CityFrom = originAddress.City;
                        request.AddressFrom = originAddress.Address1;
                    }

                    //whether this product should be shipped separately from other ones
                    if (product.ShipSeparately)
                    {
                        //whether product items should be shipped separately
                        if (_shippingSettings.ShipSeparatelyOneItemEach)
                        {
                            //add item with overridden quantity 1
                            request.Items.Add(new GetShippingOptionRequest.PackageItem(sci, product, 1));

                            //create separate requests for all product quantity
                            for (var i = 0; i < sci.Quantity; i++)
                            {
                                separateRequests.Add(request);
                            }
                        }
                        else
                        {
                            //all of product items should be shipped in a single box, so create the single separate request 
                            request.Items.Add(new GetShippingOptionRequest.PackageItem(sci, product));
                            separateRequests.Add(request);
                        }
                    }
                    else
                    {
                        //usual request
                        request.Items.Add(new GetShippingOptionRequest.PackageItem(sci, product));
                        requests.Add(warehouseId, request);
                    }
                }
            }

            //multiple locations?
            //currently we just compare warehouses
            //but we should also consider cases when several warehouses are located in the same address
            var shippingFromMultipleLocations = requests.Select(x => x.Key).Distinct().Count() > 1;

            var result = requests.Values.ToList();
            result.AddRange(separateRequests);

            return (result, shippingFromMultipleLocations);
        }
        #endregion

    }
}
