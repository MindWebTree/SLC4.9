using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using MWT.Nop.Core.Domain.CustomOrders;
using MWT.Nop.Core.Service.Catalog;
using MWT.Nop.Core.Service.Zoho;
using MWT.Nop.Core.Services.Catalog;
using MWT.Nop.Core.Services.Customers;
using MWT.Nop.Core.Services.Manage;
using MWT.Nop.Core.Services.Media;
using MWT.Nop.Core.Services.Message;
using MWT.Nop.Core.Services.Orders;
using MWT.Plugin.Misc.MwtStorefront.Areas.CustomOrder.Models.Common;
using MWT.Plugin.Misc.MwtStorefront.Areas.CustomOrder.Models.Customers;
using MWT.Plugin.Misc.MwtStorefront.Areas.CustomOrder.Models.Orders;
using MWT.Plugin.Misc.MwtStorefront.Areas.CustomOrder.Models.Products;
using MWT.Plugin.Misc.MwtStorefront.Models.Api;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Media;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Payments;
using Nop.Core.Domain.Shipping;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Customers;
using Nop.Services.Customizations.CustomOrders;
using Nop.Services.Directory;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Messages;
using Nop.Services.Orders;
using Nop.Services.Payments;
using Nop.Services.Security;
using Nop.Services.Shipping;
using Nop.Services.Tax;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Framework.Models.Extensions;
using Nop.Web.Models.Checkout;
using System.Dynamic;

namespace MWT.Plugin.Misc.MwtStorefront.Areas.CustomOrder.Factories
{
    public partial class CustomOrderModelFactory : ICustomOrderModelFactory
    {

        #region Fields

        private readonly IOrderExtendedService _orderService;
        private readonly IProductExtendedService _productService;
        private readonly IPriceFormatter _priceFormatter;
        private ICustomOrderService _customOrderService;
        private IPictureExtendedService _pictureService;
        private readonly ILocalizationService _localizationService;
        private readonly MediaSettings _mediaSettings;
        private readonly ICustomerExtendedService _customerService;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly ICustomerModelFactory _customerModelFactory;
        private readonly IProductAttributeService _productAttributeService;
        private readonly IProductAttributeParser _productAttributeParser;
        private readonly IWorkContext _workContext;
        private readonly ICustomWorkflowMessageService _workflowMessageService;
        private readonly ISettingService _settingService;
        private readonly IPaymentPluginManager _paymentPluginManager;
        private readonly PaymentSettings _paymentSettings;
        private readonly IEncryptionService _encryptionService;
        private readonly ICustomerActivityService _customerActivityService;
        private readonly IShippingPluginManager _shippingPluginManager;
        private readonly IStoreContext _storeContext;
        private readonly ShippingSettings _shippingSettings;
        private readonly IShippingService _shippingService;
        private readonly IShoppingCartExtendedService _shoppingCartService;
        private readonly IAddressService _addressService;
        private readonly ICountryService _countryService;
        private readonly IStateProvinceService _stateProvinceService;
        private readonly ILogger _logger;
        private readonly IPriceCalculationService _priceCalculationService;
        private readonly ShoppingCartSettings _shoppingCartSettings;
        protected readonly ITaxPluginManager _taxPluginManager;
        private readonly IOrderTotalCalculationExtendedService _orderTotalCalculationService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IZohoService _zohoService;
        private readonly IManageService _manageService;
        private readonly IWarehouseService _warehouseService;

        #endregion

        #region Ctor

        public CustomOrderModelFactory(
            IOrderExtendedService orderService,
            IProductExtendedService productService,
            IPriceFormatter priceFormatter,
            ICustomOrderService customOrderService,
             IPictureExtendedService pictureService,
             ILocalizationService localizationService,
             MediaSettings mediaSettings,
             ICustomerExtendedService customerService,
             IGenericAttributeService genericAttributeService,
             ICustomerModelFactory customerModelFactory,
             IProductAttributeService productAttributeService,
             IProductAttributeParser productAttributeParser,
             IWorkContext workContext,
             ICustomWorkflowMessageService workflowMessageService,
             ISettingService settingService,
             IPaymentPluginManager paymentPluginManager,
             PaymentSettings paymentSettings,
             IEncryptionService encryptionService,
             ICustomerActivityService customerActivityService,
             IShippingPluginManager shippingPluginManager,
             IStoreContext storeContext,
             ShippingSettings shippingSettings,
             IShippingService shippingService,
             IShoppingCartExtendedService shoppingCartService,
             IAddressService addressService,
             ICountryService countryService,
             IStateProvinceService stateProvinceService,
             ILogger logger,
             IPriceCalculationService priceCalculationService,
             ShoppingCartSettings shoppingCartSettings,
             ITaxPluginManager taxPluginManager,
             IOrderTotalCalculationExtendedService orderTotalCalculationService,
             IHttpContextAccessor httpContextAccessor,
             IZohoService zohoService,
             IManageService manageService,
             IWarehouseService warehouseService)
        {
            this._orderService = orderService;
            this._productService = productService;
            this._priceFormatter = priceFormatter;
            this._customOrderService = customOrderService;
            this._pictureService = pictureService;
            this._localizationService = localizationService;
            this._mediaSettings = mediaSettings;
            this._customerService = customerService;
            this._genericAttributeService = genericAttributeService;
            this._customerModelFactory = customerModelFactory;
            this._productAttributeService = productAttributeService;
            this._productAttributeParser = productAttributeParser;
            this._workContext = workContext;
            this._workflowMessageService = workflowMessageService;
            this._settingService = settingService;
            this._paymentPluginManager = paymentPluginManager;
            this._paymentSettings = paymentSettings;
            this._encryptionService = encryptionService;
            this._customerActivityService = customerActivityService;
            this._shippingPluginManager = shippingPluginManager;
            this._storeContext = storeContext;
            this._shippingSettings = shippingSettings;
            this._shippingService = shippingService;
            this._shoppingCartService = shoppingCartService;
            this._addressService = addressService;
            this._countryService = countryService;
            this._stateProvinceService = stateProvinceService;
            this._logger = logger;
            this._priceCalculationService = priceCalculationService;
            this._shoppingCartSettings = shoppingCartSettings;
            this._taxPluginManager = taxPluginManager;
            this._orderTotalCalculationService = orderTotalCalculationService;
            this._httpContextAccessor = httpContextAccessor;
            this._zohoService = zohoService;
            this._manageService = manageService;
            _warehouseService = warehouseService;
        }

        #endregion

        #region methods

        public async Task<CustomerStatsModel> PrepareCustomerStatsModel(int customerId)
        {
            CustomerStatsModel model = new CustomerStatsModel();
            var customerStats = await _orderService.GetCustomerStats(customerId);
            model.NoOforders = customerStats.nofOfOrders;
            model.Spent = await _priceFormatter.FormatPriceAsync(customerStats.amount);

            var latestOrder = await _orderService.GetRecentOrderOfCustomer(customerId);
            if (latestOrder != null)
                model.LatestOrder = await this.PrepareCustomOrderModelByOrderDomain(latestOrder);
            else
            {
                model.LatestOrder = new CustomOrderModel();
                model.LatestOrder.CustomerId = customerId;
            }
            return model;
        }
        protected async Task<List<ItemModel>> BindOfOrderItemsByOrderItemDomain(int orderId)
        {
            List<ItemModel> items = new List<ItemModel>();
            var orderItems = await _orderService.GetOrderItemsAsync(orderId);
            foreach (var orderItem in orderItems)
            {
                ItemModel item = new ItemModel();
                item.AttributesDescription = orderItem.AttributeDescription;
                item.Quantity = orderItem.Quantity;
                item.Id = orderItem.Id;
                var product = await _productService.GetProductByIdAsync(orderItem.ProductId);
                item.Name = product.Name;
                item.CustomAttributesDescription = orderItem.CustomAttributesDescription;
                item.Notes = orderItem.Notes;
                item.Quantity = orderItem.Quantity;
                item.Price = await _priceFormatter.FormatPriceAsync(orderItem.ItemPriceIncTax +
                                                             orderItem.OfferDiscountIncTax +
                                                             orderItem.MembershipDiscountIncTax +
                                                             orderItem.BuyMoreSaveMoreDiscountIncTax);
                item.Picture = (await this.PrepareCartItemPictureModelAsync(orderItem.AttributesXml, product, _mediaSettings.CategoryThumbPictureSize,
                    true, product.Name))?.ImageUrl;
                items.Add(item);

            }

            return items;
        }


        public async Task<CustomOrderSearchModel> PrepareCustomerOrderSearchModelAsync(CustomOrderSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            searchModel.SetGridPageSize();

            var orderStatuses = await this._customOrderService.GetOrderStatuses();
            searchModel.orderStatuses.Add(new SelectListItem
            {
                Value = "--",
                Text = "Status: All"
            });
            foreach (var orderStatus in orderStatuses)
            {
                if (!orderStatus.Name.Equals("OrderSaved", StringComparison.InvariantCultureIgnoreCase))
                    searchModel.orderStatuses.Add(new SelectListItem { Value = orderStatus.Id.ToString(), Text = await _localizationService.GetResourceAsync("customorder.status" + orderStatus.Name) });
            }
            var paymentMethods = await (await _paymentPluginManager
     .LoadActivePluginsAsync(null, 0, 0))
     .Where(pm => pm.PaymentMethodType == PaymentMethodType.Standard)
     .ToListAsync();

            foreach (var pm in paymentMethods)
            {
                if (pm.PluginDescriptor.SystemName.ToLower().IndexOf("authorize") >= 0)
                {
                    var pmModel = new CheckoutPaymentMethodModel.PaymentMethodModel
                    {
                        Name = await _localizationService.GetLocalizedFriendlyNameAsync(pm, (await _workContext.GetWorkingLanguageAsync()).Id),
                        Description = _paymentSettings.ShowPaymentMethodDescriptions ? await pm.GetPaymentMethodDescriptionAsync() : string.Empty,
                        PaymentMethodSystemName = pm.PluginDescriptor.SystemName,
                        LogoUrl = await _paymentPluginManager.GetPluginLogoUrlAsync(pm)
                    };
                    searchModel.PaymentMethods.Add(pmModel);
                }
            }
            return searchModel;
        }

        public virtual async Task<CustomOrderListModel> PrepareCustomOrderListModelAsync(CustomOrderSearchModel searchModel, bool isPartialOrders)
        {
            var orders = await _customOrderService.SearchCustomorder(searchterm: searchModel.SearchTerm, customerId: searchModel.CustomerId, statusId: searchModel.searchStatusId,
                     pageIndex: searchModel.Page - 1, pageSize: searchModel.PageSize, isPartialOrders, searchModel.DisplayAdditionalService, searchModel.IsArchived);

            var orderTypes = await this._customOrderService.GetOrderTypes();
            var statusIdAlreadyPaidoBJ = orderTypes.Where(m => String.Compare(m.Name, OrderTypes.AlreadyPaid.ToString(), StringComparison.OrdinalIgnoreCase) == 0).FirstOrDefault();
            var alreadyPaidStatusId = statusIdAlreadyPaidoBJ == null ? 0 : statusIdAlreadyPaidoBJ.Id;


            //prepare list model
            var model = await new CustomOrderListModel().PrepareToGridAsync(searchModel, orders, () =>
            {
                return orders.SelectAwait(async order =>
                {
                    var _order = await PrepareCustomerOrderModel(order, alreadyPaidStatusId);
                    string orderTypePrefix = "";
                    var orderType = orderTypes.Where(t => t.Id == order.OrderTypeId).FirstOrDefault();
                    if (orderType != null && orderType.Name != "AlreadyPaid" && !string.IsNullOrEmpty(orderType.Name))
                        orderTypePrefix = orderType.Prefix;
                    else
                    {
                        orderType = orderTypes.Where(t => t.Id == order.SubOrderTypeId).FirstOrDefault();
                        if (orderType != null)
                            orderTypePrefix = orderType.Prefix;
                    }
                    _order.OrderNumberWithPrefix = orderTypePrefix + order.Id;
                    return _order;
                });
            });
            return model;
        }

        #region Steps

        #region Order Steps
        public virtual async Task<CustomOrderModel> PrepareCustomerOrderModel(MWT.Nop.Core.Domain.CustomOrders.CustomOrder order)
        {
            var model = order.ToModel<CustomOrderModel>();
            var orderTypes = (await _customOrderService.GetOrderTypes());


            foreach (var orderType in orderTypes)
            {
                if (orderType.ParentId == 0)
                    model.orderTypes.Add(new SelectListItem { Value = orderType.Id.ToString(), Text = orderType.Name, Selected = orderType.Id == order.OrderTypeId ? true : false });
            }
            foreach (var orderType in orderTypes)
            {
                if (orderType.ParentId != 0)
                    model.subOrderTypes.Add(new SelectListItem { Value = orderType.Id.ToString(), Text = orderType.Name, Selected = orderType.Id == order.SubOrderTypeId ? true : false });
            }
            model.SubOrderTypeId = order.SubOrderTypeId ?? 0;
            if (order.StatusId != 0)
            {
                var statuses = await _customOrderService.GetOrderStatuses();
                model.OrderStatus = statuses.Where(m => m.Id == order.StatusId).FirstOrDefault()?.Name;
            }

            model.EnableCustomerSearch = model.OrderStatus == Nop.Core.Domain.CustomOrders.OrderStatus.Paid.ToString() ? false : await this.CustomerSearchEnableDisable(order);
            model.EnableProductSearch = model.OrderStatus == Nop.Core.Domain.CustomOrders.OrderStatus.Paid.ToString() ? false : await this.ProductSearchEnableDisable(order);
            model.DisplayCartSummary = (await this._customOrderService.GetOrderItems(order.Id)).Count > 0 ? true : false;
            model.ShippingMethods = await this.GetShippingMethods();
            model.EnableCartSummary = !(await IsOrderPaid(order));


            return model;
        }
        public virtual async Task<CustomOrderOrderTypeSectionModel> PrepareOrderTypeSectionModel(int orderId)
        {
            var order = await _customOrderService.GetById(orderId);
            CustomOrderOrderTypeSectionModel model = new CustomOrderOrderTypeSectionModel();
            model.Id = order.Id;
            model.PurchaseOrderNumber = order.PurchaseOrderNumber;
            model.LiveOrderNumber = order.LiveOrderNumber;
            model.AlreadyFee = order.AlreadyFee;
            model.HouzzFee = order.HouzzFee;
            model.HouzzFeeType = order.HouzzFeeType;
            model.PromiseDayDate = order.PromiseDayDate;
            if (order.OrderTypeId != 0)
            {
                var orderTypes = await _customOrderService.GetOrderTypes();
                model.OrderType = orderTypes.Where(t => t.Id == order.OrderTypeId).FirstOrDefault()?.Name;
                orderTypes = orderTypes.Where(t => t.ParentId == order.OrderTypeId).ToList();

                if (orderTypes.Count > 0)
                {

                    model.subOrderTypes.Add(new SelectListItem
                    {
                        Value = "",
                        Text = "Please select"
                    });
                }

                foreach (var orderType in orderTypes)
                {
                    model.subOrderTypes.Add(new SelectListItem { Value = orderType.Id.ToString(), Text = orderType.Name, Selected = orderType.Id == order.SubOrderTypeId ? true : false });
                }
            }
            if (order.StatusId != 0)
            {
                var orderStatuses = await this._customOrderService.GetOrderStatuses();
                model.OrderStatus = orderStatuses.Where(m => m.Id == order.StatusId).FirstOrDefault()?.Name;

            }


            return model;
        }
        public async Task<CustomOrderCustomerSectionModel> PrepareCustomerSection(int orderId, int customerId = 0, bool byCustomerId = false)
        {
            var customer = new Customer();
            var order = new MWT.Nop.Core.Domain.CustomOrders.CustomOrder();
            if (!byCustomerId)
                order = await _customOrderService.GetById(orderId);
            CustomOrderCustomerSectionModel model = new CustomOrderCustomerSectionModel();
            model.isEditable = await this.IsOrderEditable(order);
            if ((!byCustomerId && (order.CustomerId == null || order.CustomerId == 0)) || (byCustomerId && customerId == 0))
                model.EnableCustomerSearch = true;
            else
            {
                customer = await this._customerService.GetCustomerByIdAsync(Convert.ToInt32(!byCustomerId ? order.CustomerId : customerId));
                if (customer == null || customer.ShippingAddressId == null || customer.ShippingAddressId == 0 || customer.BillingAddressId == null || customer.BillingAddressId == 0)
                    model.EnableCustomerSearch = true;
                else
                {
                    if (!byCustomerId)
                    {
                        if (order.StatusId == 0)
                            model.EnableCustomerSearch = true;
                        else
                        {
                            var orderTypes = await _customOrderService.GetOrderTypes();
                            var statusIdAlreadyPaidoBJ = orderTypes.Where(m => String.Compare(m.Name,
                                MWT.Nop.Core.Domain.CustomOrders.OrderStatus.SavedDraft.ToString(), StringComparison.OrdinalIgnoreCase) == 0).FirstOrDefault();
                            if (statusIdAlreadyPaidoBJ == null)
                                model.EnableCustomerSearch = true;
                        }
                    }
                }
            }


            //address Model

            if (customer != null && customer.Id != 0)
            {
                model.FirstName = customer.FirstName;
                model.LastName = customer.LastName;
                string phone = "";
                string email = customer.Email;
                phone = customer.Phone;
                model.Email = string.IsNullOrEmpty(email) ? "" : email;
                model.Id = customer.Id;
                if (customer.ShippingAddressId != null && customer.ShippingAddressId != 0)
                    model.ShippingAddress = await _customerModelFactory.PrepareCustomAddressModelAsync(customer, MWT.Nop.Core.Domain.Address.AddressType.IsShippingAddress, Convert.ToInt32(customer.ShippingAddressId),
                        null, prePopulateNewAddressWithCustomerFields: true);
                else
                    model.ShippingAddress = await _customerModelFactory.PrepareCustomAddressModelAsync(customer, MWT.Nop.Core.Domain.Address.AddressType.IsShippingAddress, 0,
                           null, prePopulateNewAddressWithCustomerFields: true);

                if (customer.BillingAddressId != null && customer.BillingAddressId != 0)
                    model.BillingAddress = await _customerModelFactory.PrepareCustomAddressModelAsync(customer, MWT.Nop.Core.Domain.Address.AddressType.IsBillingAddress, Convert.ToInt32(customer.BillingAddressId),
                        null, prePopulateNewAddressWithCustomerFields: true);
                else
                    model.BillingAddress = await _customerModelFactory.PrepareCustomAddressModelAsync(customer, MWT.Nop.Core.Domain.Address.AddressType.IsBillingAddress, 0,
                           null, prePopulateNewAddressWithCustomerFields: true);

                if (string.IsNullOrEmpty(model.ShippingAddress?.PhoneNumber) && !string.IsNullOrEmpty(phone))
                {
                    if (model.ShippingAddress == null)
                    {
                        model.ShippingAddress = new AddressModel();
                        model.ShippingAddress.PhoneNumber = phone;
                    }
                    else
                        model.ShippingAddress.PhoneNumber = phone;
                }
            }
            else
            {
                model.ShippingAddress = await _customerModelFactory.PrepareCustomAddressModelAsync(customer, MWT.Nop.Core.Domain.Address.AddressType.IsShippingAddress, 0,
                              null, prePopulateNewAddressWithCustomerFields: true);
                model.BillingAddress = await _customerModelFactory.PrepareCustomAddressModelAsync(customer, MWT.Nop.Core.Domain.Address.AddressType.IsBillingAddress, 0,
           null, prePopulateNewAddressWithCustomerFields: true);
            }


            //end 

            return model;
        }
        public async Task<List<CustomOrderOrderTypeModel>> PrepareOrderTypesModel()
        {
            var orderTypes = await _customOrderService.GetOrderTypes();
            return orderTypes.Select(orderType =>
            {
                return orderType.ToModel<CustomOrderOrderTypeModel>();

            }).ToList();
        }
        public async Task<List<CustomerModel>> PrepareCustomerListModelAsync(string searchTerm)
        {
            var customers = await _customerService.CustomGetAllCustomersByStoreProcedureAsync(searchTerm);

            List<CustomerModel> lstModel = new List<CustomerModel>();
            foreach (var customer in customers)
            {
                CustomerModel model = new CustomerModel();
                model.Id = customer.Id;
                string email = customer.Email;

                model.Email = string.IsNullOrEmpty(email) ? "" : email;
                model.FullName = await _customerService.GetCustomerFullNameAsync(customer);
                model.FullName = model.FullName == null ? "" : model.FullName;
                lstModel.Add(model);
            }
            return lstModel;
        }
        public async Task UpdateOrderType(int orderId, int typeId, int SubOrderTypeId)
        {
            var order = await _customOrderService.GetById(orderId);
            if (order != null)
            {
                order.OrderTypeId = typeId;
                order.SubOrderTypeId = SubOrderTypeId;
                var orderTypes = await this._customOrderService.GetOrderTypes();
                var orderType = orderTypes.Where(m => m.Id == typeId).FirstOrDefault().Name;
                if (orderType == OrderTypes.CustomOrder.ToString())
                {
                    order.HouzzFee = 0;
                    order.HouzzFeeType = "";
                    order.SubOrderTypeId = null;
                    order.PurchaseOrderNumber = "";
                }
                else if (orderType == OrderTypes.AlreadyPaid.ToString())
                {
                    order.HouzzFee = 0;
                    order.HouzzFeeType = "";

                }
                else if (orderType == OrderTypes.EbayOrder.ToString())
                {
                    order.HouzzFee = 0;
                    order.HouzzFeeType = "";
                    order.AlreadyFee = 0;
                    order.SubOrderTypeId = null;
                    order.PurchaseOrderNumber = "";
                }
                else if (orderType == OrderTypes.HouzzOrder.ToString())
                {
                    order.AlreadyFee = 0;
                    order.SubOrderTypeId = null;
                    order.PurchaseOrderNumber = "";
                }
                await _customOrderService.UpdateAsync(order);
            }
        }
        public async Task UpdateCustomer(int customerId, int orderId)
        {
            var order = await _customOrderService.GetById(orderId);
            if (order != null)
            {
                order.CustomerId = customerId;
                await _customOrderService.UpdateAsync(order);
            }
            await UpdateOrderTotal(orderId, true);
        }
        public async Task UpdateOrderTypeDetails(CustomOrderOrderTypeSectionModel model, bool isReset)
        {
            var order = await this._customOrderService.GetById(model.Id);
            if (isReset)
            {
                order.PromiseDayDate = null;
                order.AlreadyFee = null;
                order.PurchaseOrderNumber = "";
                order.HouzzFee = 0;
                order.HouzzFeeType = "";
                order.SubOrderTypeId = null;
            }
            else
            {
                var orderTypes = await this._customOrderService.GetOrderTypes();
                var orderType = orderTypes.Where(m => m.Id == order.OrderTypeId).FirstOrDefault().Name;
                if (orderType == OrderTypes.CustomOrder.ToString())
                {
                    order.HouzzFee = 0;
                    order.HouzzFeeType = "";
                    order.AlreadyFee = model.AlreadyFee;
                    order.PromiseDayDate = model.PromiseDayDate;
                    order.SubOrderTypeId = null;
                    order.PurchaseOrderNumber = "";
                }
                else if (orderType == OrderTypes.AlreadyPaid.ToString())
                {
                    order.HouzzFee = 0;
                    order.HouzzFeeType = "";
                    order.AlreadyFee = model.AlreadyFee;
                    order.PromiseDayDate = model.PromiseDayDate;
                    order.PurchaseOrderNumber = model.PurchaseOrderNumber;
                    //order.SubOrderTypeId = model.SubOrderTypeId;
                }
                else if (orderType == OrderTypes.EbayOrder.ToString())
                {
                    order.HouzzFee = 0;
                    order.HouzzFeeType = "";
                    order.AlreadyFee = 0;
                    order.PromiseDayDate = model.PromiseDayDate;
                    order.SubOrderTypeId = null;
                    order.PurchaseOrderNumber = "";
                }
                else if (orderType == OrderTypes.HouzzOrder.ToString())
                {
                    order.HouzzFee = model.HouzzFee;
                    order.HouzzFeeType = model.HouzzFeeType.ToLower() == DiscountType.Percentage.ToString().ToLower() ? DiscountType.Percentage.ToString()
                        : DiscountType.Fixed.ToString();
                    order.AlreadyFee = 0;
                    order.PromiseDayDate = model.PromiseDayDate;
                    order.SubOrderTypeId = null;
                    order.PurchaseOrderNumber = model.PurchaseOrderNumber;
                }

            }
            await _customOrderService.UpdateAsync(order);
        }



        #endregion

        #region Product Steps
        public async Task<List<ProductOverviewModel>> PrepareProductListModelAsync(string searchTerm)
        {

            var products = await _productService.OverriddenSearchProductsAsync(pageIndex: 0,
                                                                   pageSize: 5,
                                                                   searchSku: true,
                                                                   keywords: searchTerm,
                                                                   showHidden: true);


            List<ProductOverviewModel> listModel = new List<ProductOverviewModel>();
            foreach (var product in products)
            {
                ProductOverviewModel model = new ProductOverviewModel();
                model.Id = product.Id;
                model.Stock = product.StockQuantity;
                model.Sku = product.Sku;
                model.ProductPrice = await _priceFormatter.FormatPriceAsync(product.OldPrice <= 0 ? product.Price : product.OldPrice);
                model.Name = product.Name;
                model.DefaultPictureModel = (await PrepareProductOverviewPictureModelAsync(product, null))?.ImageUrl;
                model.Combinations = await this.PrepareProductAttributeCombinations(product);
                model.ProductAttributes = await this.PrepareProductAttributes(model.Combinations);
                listModel.Add(model);
            }
            return listModel;
        }
        public async Task<CustomOrderShoppingCartItemModel> PrepareCartModel(int orderId)
        {
            CustomOrderShoppingCartItemModel model = new CustomOrderShoppingCartItemModel();
            decimal total = 0;
            var items = await _customOrderService.GetOrderItems(orderId);
            var order = await _customOrderService.GetById(orderId);
            model.isEditable = await this.IsOrderEditable(order);
            foreach (var _item in items)
            {

                ItemModel itemModel = new ItemModel();
                itemModel.AttributesDescription = _item.AttributesDescription;
                itemModel.AttributesXml = _item.AttributesXml;
                itemModel.CreatedOnUtc = _item.CreatedOnUtc;
                itemModel.CustomAttributesDescription = _item.CustomAttributesDescription;
                itemModel.Id = _item.Id;
                itemModel.Notes = _item.Notes;
                itemModel.Quantity = _item.Quantity;

                // adjustment
                try
                {
                    var _priceAdjustment = await _customOrderService.GetPriceAdjustmentsByCartId(_item.Id);
                    if (_priceAdjustment != null)
                    {
                        itemModel.Price = await _priceFormatter.FormatPriceAsync(
                            _priceAdjustment.ShoppingCartProductPrice == null ? 0 : Convert.ToDecimal(_priceAdjustment.ShoppingCartProductPrice));
                        itemModel.ItemTotal = await _priceFormatter.FormatPriceAsync(
                (_priceAdjustment.ShoppingCartProductPrice == null ? 0 : Convert.ToDecimal(_priceAdjustment.ShoppingCartProductPrice)) * _item.Quantity);

                        itemModel.ChargeType = _priceAdjustment.Chargestype;
                        itemModel.DiscountAmount = _priceAdjustment.Discountamount == null ? 0 : Convert.ToDecimal(_priceAdjustment.Discountamount);
                        itemModel.DiscountPercentage = _priceAdjustment.DiscountPercentage == null ? 0 : Convert.ToDecimal(_priceAdjustment.DiscountPercentage);
                        itemModel.DiscountType = _priceAdjustment.Discounttype;
                        decimal totalAdjustment = (itemModel.DiscountType == DiscountType.Percentage.ToString() && itemModel.DiscountPercentage != 0 ?
                             (_priceAdjustment.ShoppingCartProductPrice == null ? 0 : Convert.ToDecimal(_priceAdjustment.ShoppingCartProductPrice)
                             * Convert.ToDecimal(_priceAdjustment.DiscountPercentage)) / 100
                             : (itemModel.DiscountType == DiscountType.Fixed.ToString() && itemModel.DiscountAmount != 0
                             ? itemModel.DiscountAmount : 0)) * _item.Quantity;

                        itemModel.TotalAdjustment = (totalAdjustment < 0 ? "-" : "") + await _priceFormatter.FormatPriceAsync(totalAdjustment);
                        decimal itemTotal = 0;
                        if (_priceAdjustment.Chargestype == ChargeType.Subtract.ToString())
                            itemTotal = ((
            _priceAdjustment.ShoppingCartProductPrice == null ? 0 : Convert.ToDecimal(_priceAdjustment.ShoppingCartProductPrice)) * _item.Quantity)
            - totalAdjustment;
                        else if (_priceAdjustment.Chargestype == ChargeType.Add.ToString())
                            itemTotal = ((
         _priceAdjustment.ShoppingCartProductPrice == null ? 0 : Convert.ToDecimal(_priceAdjustment.ShoppingCartProductPrice)) * _item.Quantity)
         + totalAdjustment;

                        else
                            itemTotal = ((
      _priceAdjustment.ShoppingCartProductPrice == null ? 0 : Convert.ToDecimal(_priceAdjustment.ShoppingCartProductPrice)) * _item.Quantity);

                        itemModel.Total = (itemTotal < 0 ? "-" : "") + await _priceFormatter.FormatPriceAsync(itemTotal);
                        total += itemTotal;

                    }

                    // end
                    var product = await _productService.GetProductByIdAsync(_item.ProductId);
                    itemModel.Name = product.Name;
                    itemModel.Sku = product.Sku;
                    itemModel.Picture = (await PrepareProductOverviewPictureModelAsync(product))?.ImageUrl;
                    itemModel.UpdatedOnUtc = _item.UpdatedOnUtc;
                    model.Items.Add(itemModel);
                }
                catch (Exception ex)
                {

                }
            }
            model.Total = await _priceFormatter.FormatPriceAsync(total);
            model.Id = orderId;

            return model;
        }
        public async Task AddUpdateCartItems(List<ItemModel> itemsModel, int orderId, bool isItemNotes = false)
        {
            var order = await _customOrderService.GetById(orderId);
            var items = await _customOrderService.GetOrderItems(orderId);
            foreach (var itemModel in itemsModel)
            {
                var item = items.Where(m => m.Id == itemModel.Id).FirstOrDefault();
                if (item != null)
                {
                    if (isItemNotes)
                    {
                        item.Notes = itemModel.Notes;
                        item.UpdatedOnUtc = itemModel.UpdatedOnUtc;
                        await _customOrderService.UpdateCartItemAsync(item);
                    }
                    else
                    {
                        item.Quantity = itemModel.Quantity;
                        item.UpdatedOnUtc = itemModel.UpdatedOnUtc;
                        await _customOrderService.UpdateCartItemAsync(item);

                        var priceAdj = await _customOrderService.GetPriceAdjustmentsByCartId(itemModel.Id);
                        if (priceAdj != null)
                        {
                            priceAdj.Discountamount = itemModel.ChargeType == ChargeType.Subtract.ToString() ? itemModel.DiscountAmount > priceAdj.ShoppingCartProductPrice ? priceAdj.ShoppingCartProductPrice : itemModel.DiscountAmount : itemModel.DiscountAmount;
                            priceAdj.DiscountPercentage = itemModel.DiscountPercentage > 100 ? 100 : itemModel.DiscountPercentage;
                            priceAdj.Discounttype = itemModel.DiscountType;
                            priceAdj.Chargestype = itemModel.ChargeType;
                            await _customOrderService.UpdatePriceAdjustmentAsync(priceAdj);
                        }
                    }
                }
                else
                {
                    bool isUpdated = false;
                    if (itemModel.CustomAttributesDescription == null)
                    {
                        item = items.Where(m => m.ProductId == itemModel.ProductId && (m.AttributesDescription == null ? "" : m.AttributesDescription) == (itemModel.AttributesDescription == null ? "" : itemModel.AttributesDescription)).FirstOrDefault();
                        if (item != null)
                        {
                            item.Quantity = item.Quantity + itemModel.Quantity;
                            item.UpdatedOnUtc = itemModel.UpdatedOnUtc;
                            await _customOrderService.UpdateCartItemAsync(item);
                            isUpdated = true;
                        }
                    }
                    else
                    {
                        item = items.Where(m => m.ProductId == itemModel.ProductId && (m.CustomAttributesDescription == null ? "" : m.CustomAttributesDescription) == (itemModel.CustomAttributesDescription == null ? "" : itemModel.CustomAttributesDescription)).FirstOrDefault();
                        if (item != null)
                        {
                            item.Quantity = item.Quantity + itemModel.Quantity;
                            item.UpdatedOnUtc = itemModel.UpdatedOnUtc;
                            await _customOrderService.UpdateCartItemAsync(item);
                            isUpdated = true;
                        }
                    }

                    if (!isUpdated)
                    {
                        CustomOrderShoppingCartItem cartItem = new CustomOrderShoppingCartItem();
                        cartItem.AttributesDescription = itemModel.AttributesDescription;
                        cartItem.AttributesXml = itemModel.AttributesXml;
                        cartItem.CreatedOnUtc = DateTime.UtcNow;
                        cartItem.CustomAttributesDescription = itemModel.CustomAttributesDescription;
                        cartItem.CustomerId = order.CustomerId;
                        cartItem.Notes = itemModel.Notes;
                        cartItem.OrderId = orderId;
                        cartItem.ProductId = itemModel.ProductId;
                        cartItem.Quantity = itemModel.Quantity;
                        cartItem.ShoppingCartTypeId = 1;
                        cartItem.UpdatedOnUtc = DateTime.Now;
                        await _customOrderService.InsertOrderItemAsync(cartItem);

                        CustomOrderPriceAdjustment priceAdj = new CustomOrderPriceAdjustment();
                        priceAdj.Chargestype = itemModel.DiscountAmount != 0 ? itemModel.ChargeType : "";
                        priceAdj.Discountamount = itemModel.ChargeType == ChargeType.Subtract.ToString() ? itemModel.DiscountAmount > priceAdj.ShoppingCartProductPrice ? priceAdj.ShoppingCartProductPrice : itemModel.DiscountAmount : itemModel.DiscountAmount; ;
                        priceAdj.DiscountPercentage = itemModel.DiscountPercentage > 100 ? 100 : itemModel.DiscountPercentage;
                        priceAdj.Discounttype = itemModel.DiscountAmount != 0 ? itemModel.DiscountType : "";
                        priceAdj.OrderId = orderId;
                        decimal price = 0;
                        decimal.TryParse(itemModel.Price, out price);

                        priceAdj.ShoppingCartProductPrice = price;// code to rmeove currency
                        priceAdj.ShoppingCartRecID = cartItem.Id;
                        await _customOrderService.InsertPriceAdjustmentAsync(priceAdj);
                    }
                }
            }

            await UpdateOrderTotal(orderId, true);
        }
        public async Task DeleteCartItem(ItemModel item, int orderId)


        {
            await _customOrderService.DeleteOrderItemAsync(item.Id, orderId);
            await UpdateOrderTotal(orderId, true);
        }

        #endregion end Product Steps

        #region Order Summary 

        public async Task<CustomOrderModel> PrepareBriefSummaryOfOrder(int orderId)
        {
            var order = await _customOrderService.GetById(orderId);
            CustomOrderModel model = new CustomOrderModel();
            if (order != null)
            {
                var customer = new Customer();
                if (order.CustomerId != null)
                    customer = await this._customerService.GetCustomerByIdAsync(Convert.ToInt32(order.CustomerId));

                model.Id = order.Id;
                model.customOrderSummaryModel = await this.PrepareOderSummaryModel(orderId);
                model.customOrderShoppingCartItemModel = await this.PrepareCartModel(order.Id);
                model.ShippingAddress = await _customerModelFactory.PrepareCustomAddressModelAsync(customer, MWT.Nop.Core.Domain.Address.AddressType.IsShippingAddress, 0,
                          null, prePopulateNewAddressWithCustomerFields: true);
                model.BillingAddress = await _customerModelFactory.PrepareCustomAddressModelAsync(customer, MWT.Nop.Core.Domain.Address.AddressType.IsBillingAddress, 0,
           null, prePopulateNewAddressWithCustomerFields: true);
            }
            return model;
        }
        public async Task<CustomOrderSummaryModel> PrepareOderSummaryModel(int orderId, bool isCustomerPaying = false)
        {
            CustomOrderSummaryModel model = new CustomOrderSummaryModel();
            var order = await _customOrderService.GetById(orderId);
            if (order != null)
            {
                model.ApplyTax = order.ApplyTax;
                var items = await _customOrderService.GetOrderItems(orderId);
                var orderStatuses = await _customOrderService.GetOrderStatuses();
                model.Id = orderId;
                model.OrderStatus = orderStatuses.Where(o => o.Id == order.StatusId).FirstOrDefault()?.Name;
                model.FullPaid = order.FullPaid;
                if (items.Count == 0 &&
                    model.OrderStatus != Nop.Core.Domain.CustomOrders.OrderStatus.Paid.ToString())
                    return null;
                await this.UpdateOrderTotal(orderId);
                decimal orderTotal = order.OrderTotal == null ? 0 : (Convert.ToDecimal(order.OrderTotal));
                decimal subtotal = order.SubTotal == null ? 0 : (Convert.ToDecimal(order.SubTotal));
                decimal houzzFee = 0;
                decimal initialPayment = 0;
                decimal pendingPayment = 0;
                decimal payableAmount = orderTotal;
                var orderTypes = await _customOrderService.GetOrderTypes();
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
                model.NormalWgsCharges = (await this.GetWgsCharges(order, subtotal, await _customOrderService.IsSurchargeApplicable(order))).wgsCharges;

                if (order.Shipping != null && order.Shipping > 0)
                    model.Shipping = await _priceFormatter.FormatPriceAsync(Convert.ToDecimal(order.Shipping));

                // Discount 
                var orderSummaryAdj = await _customOrderService.GetOrderSummaryAdjustment(order.Id);
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

                    var subTotalDiscountDetails = new DiscountDetails();
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

                    var shippingDiscountDetails = new DiscountDetails();
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
                (model.TaxInfo, model.HtmlTaxInfo) = await this.GetTaxDetails(order.TaxInfo);
                model.InvoiceNotes = order.InvoiceNote;
                model.SpecialInstructionsfromBuyer = order.SpecialInstructionsfromBuyer;
                model.PrivateNotes = order.PrivateOrderNotes;

                #region Customer zipcode
                string zipCode = "";
                int billingAddressId = 0;
                int shippingAddressId = 0;
                string email = "";
                Address billingAddress = new Address();
                Address shippingAddress = new Address();
                if (order.LiveOrderNumber != null && order.LiveOrderNumber > 0)
                {
                    var _order = await _orderService.GetOrderByIdAsync(Convert.ToInt32(order.LiveOrderNumber));
                    billingAddressId = _order.BillingAddressId == null ? 0 : Convert.ToInt32(_order.BillingAddressId);
                    shippingAddressId = _order.ShippingAddressId == null ? 0 : Convert.ToInt32(_order.ShippingAddressId);
                    if (billingAddressId > 0)
                        billingAddress = await _addressService.GetAddressByIdAsync(billingAddressId);
                    if (shippingAddressId > 0)
                        shippingAddress = await _addressService.GetAddressByIdAsync(shippingAddressId);
                    if (billingAddress == null)
                        billingAddress = shippingAddress;
                    email = string.IsNullOrEmpty(order.CustomerCCEmail) ? billingAddress?.Email : order.CustomerCCEmail;
                    zipCode = string.IsNullOrEmpty(shippingAddress?.ZipPostalCode) ? billingAddress?.ZipPostalCode ?? "" :
                        shippingAddress?.ZipPostalCode ?? "";
                }
                else
                {
                    billingAddressId = customer.BillingAddressId == null ? 0 : Convert.ToInt32(customer.BillingAddressId);
                    shippingAddressId = customer.ShippingAddressId == null ? 0 : Convert.ToInt32(customer.ShippingAddressId);
                    if (billingAddressId > 0)
                        billingAddress = await _addressService.GetAddressByIdAsync(billingAddressId);
                    if (shippingAddressId > 0)
                        shippingAddress = await _addressService.GetAddressByIdAsync(shippingAddressId);
                    if (billingAddress == null)
                        billingAddress = shippingAddress;
                    email = string.IsNullOrEmpty(billingAddress?.Email) ? shippingAddress?.Email ?? "" : billingAddress?.Email ?? "";
                    zipCode = string.IsNullOrEmpty(shippingAddress?.ZipPostalCode) ? billingAddress?.ZipPostalCode ?? "" :
                        shippingAddress?.ZipPostalCode ?? "";
                }
                model.ZipCode = zipCode;
                model.Email = email;
                #endregion

                // Payment Section
                var paymentMethods = await (await _paymentPluginManager
             .LoadActivePluginsAsync(model.Customer, 0, 0))
             .Where(pm => pm.PaymentMethodType == PaymentMethodType.Standard)
             .ToListAsync();



                List<IPaymentMethod> pms = new List<IPaymentMethod>();
                foreach (var pm in paymentMethods)
                {

                    if ((await _localizationService.GetLocalizedFriendlyNameAsync(pm, (await _workContext.GetWorkingLanguageAsync()).Id)).ToLower().IndexOf("affirm") >= 0)
                    {
                        List<ShoppingCartItem> cartItems = new List<ShoppingCartItem>();

                        foreach (var item in items)
                        {
                            cartItems.Add(new ShoppingCartItem()
                            {
                                AttributesXml = item.AttributesXml,
                                ProductId = item.ProductId,
                                Quantity = item.Quantity,
                                CustomerId = customer.Id
                            });
                        }

                        if (!await pm.HidePaymentMethodAsync(cartItems))
                        {
                            pms.Add(pm);
                        }
                    }
                    else
                    {
                        pms.Add(pm);
                    }
                }

                foreach (var pm in pms)
                {

                    var pmModel = new CheckoutPaymentMethodModel.PaymentMethodModel
                    {
                        Name = await _localizationService.GetLocalizedFriendlyNameAsync(pm, (await _workContext.GetWorkingLanguageAsync()).Id),
                        Description = _paymentSettings.ShowPaymentMethodDescriptions ? await pm.GetPaymentMethodDescriptionAsync() : string.Empty,
                        PaymentMethodSystemName = pm.PluginDescriptor.SystemName,
                        LogoUrl = await _paymentPluginManager.GetPluginLogoUrlAsync(pm)
                    };

                    model.PaymentMethods.Add(pmModel);
                }
                // end
                // Payment Details

                if (model.OrderStatus == Nop.Core.Domain.CustomOrders.OrderStatus.Paid.ToString() && order.LiveOrderNumber != null)
                {
                    var liveOrder = await _orderService.GetOrderByIdAsync(Convert.ToInt32(order.LiveOrderNumber));
                    if (liveOrder != null)
                    {
                        var paymentDetails = new PaymentDetails();
                        paymentDetails.PaymentMethod = liveOrder.PaymentMethodSystemName;
                        paymentDetails.PaymentDate = liveOrder.CreatedOnUtc;
                        paymentDetails.TransactionId = string.IsNullOrEmpty(liveOrder.AuthorizationTransactionId) ?
                            liveOrder.CaptureTransactionId : liveOrder.AuthorizationTransactionId;
                        paymentDetails.Card = String.IsNullOrEmpty(liveOrder.MaskedCreditCardNumber) ? "" : _encryptionService.DecryptText(liveOrder.MaskedCreditCardNumber);
                        var paidStatusId = orderStatuses.Where(s => s.Name == Nop.Core.Domain.CustomOrders.OrderStatus.Paid.ToString()).FirstOrDefault()?.Id;
                        if (paidStatusId != null)
                        {
                            var logs = await _customOrderService.GetOrderStatusLogs(order.Id);
                            var paidStatusLog = logs.Where(l => l.StatusId == Convert.ToInt32(paidStatusId)).FirstOrDefault();
                            if (paidStatusLog != null && paidStatusLog.UserId != 0)
                                paymentDetails.PaidBy = (await _customerService.GetCustomerByIdAsync(paidStatusLog.UserId))?.Email;
                        }
                        paymentDetails.LiveOrderNumber = Convert.ToInt32(order.LiveOrderNumber);
                        model.PaymentDetails = paymentDetails;
                    }
                }
                else if (order.ParentOrderID != 0)
                {
                    if (order.PromiseDayDate != null)
                    {
                        if (order.PromiseDayDate < DateTime.Now)
                        {
                            model.IsOrderExpired = true;
                            model.Message = await _localizationService.GetResourceAsync("admin.CheckoutCustomOrder.Isexpired");
                        }
                    }
                    else
                    {
                        (bool isValid, string message) = await _manageService.ValidateWgsOrder(order.ParentOrderID);
                        if (!isValid)
                        {
                            model.IsOrderExpired = true;
                            model.Message = message;
                        }
                    }
                }
                model.ParentOrderID = order.ParentOrderID;

                // end
            }


            return model;

        }

        public async Task<ApiCustomOrderSummaryModel> PrepareApiOderSummaryModel(int orderId, bool isCustomerPaying = false)
        {
            ApiCustomOrderSummaryModel model = new ApiCustomOrderSummaryModel();
            var order = await _customOrderService.GetById(orderId);
            if (order != null)
            {
                var liveOrder = await _orderService.GetOrderByIdAsync(order.LiveOrderNumber ?? 0);
                model.ApplyTax = order.ApplyTax;
                var items = await _customOrderService.GetOrderItems(orderId);
                var orderStatuses = await _customOrderService.GetOrderStatuses();
                model.Id = orderId;
                model.OrderStatus = orderStatuses.Where(o => o.Id == order.StatusId).FirstOrDefault()?.Name;
                model.FullPaid = order.FullPaid;
                if (items.Count == 0 &&
                    model.OrderStatus != Nop.Core.Domain.CustomOrders.OrderStatus.Paid.ToString())
                    return null;
                await this.UpdateOrderTotal(orderId);
                decimal orderTotal = order.OrderTotal == null ? 0 : (Convert.ToDecimal(order.OrderTotal));
                decimal subtotal = order.SubTotal == null ? 0 : (Convert.ToDecimal(order.SubTotal));
                decimal houzzFee = 0;
                decimal initialPayment = 0;
                decimal pendingPayment = 0;
                decimal payableAmount = orderTotal;
                var orderTypes = await _customOrderService.GetOrderTypes();
                model.OrderType = (orderTypes.Where(o => o.Id == order.OrderTypeId)).FirstOrDefault()?.Name;
                model.ShippingMethod = order.ShippingMethod;
                model.WgsAdjustmentNotes = order.WgsAdjustmentNotes;

                var customer = new Customer();
                if (order.CustomerId != null)
                    customer = await this._customerService.GetCustomerByIdAsync(Convert.ToInt32(order.CustomerId));


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

                model.SubTotal = subtotal;
                model.OrderTotal = orderTotal;
                model.PayableAmount = payableAmount;
                if (initialPayment > 0)
                    model.InitialPayment = initialPayment;
                if (pendingPayment > 0)
                    model.PendingPayment = pendingPayment;
                if (houzzFee > 0)
                    model.HouzzFee = houzzFee;

                if (order.Wgs != null && order.Wgs > 0)
                    model.Wgs = Convert.ToDecimal(order.Wgs);
                model.NormalWgsCharges = (await this.GetWgsCharges(order, subtotal, await _customOrderService.IsSurchargeApplicable(order))).wgsCharges;
                model.ComplementryWgsFree = order.ComplementryWgsFree;

                if (order.Shipping != null && order.Shipping > 0)
                    model.Shipping = Convert.ToDecimal(order.Shipping);

                // Discount 
                var orderSummaryAdj = await _customOrderService.GetOrderSummaryAdjustment(order.Id);
                if (orderSummaryAdj != null)
                {
                    #region SubTotalAdj

                    string subTotalDiscountType = orderSummaryAdj.SubTotalDiscountType == null ? "" :
                       (orderSummaryAdj.SubTotalDiscountType == DiscountType.Percentage.ToString() ? DiscountType.Percentage.ToString() : DiscountType.Fixed.ToString());
                    decimal totalAdjustment = 0;
                    decimal discountAmount = orderSummaryAdj.SubtotalDiscount == null ? 0 : Convert.ToDecimal(orderSummaryAdj.SubtotalDiscount);
                    if (discountAmount != 0)
                    {
                        totalAdjustment =
                            subTotalDiscountType == DiscountType.Percentage.ToString() ?
                            (subtotal * discountAmount) / 100
                            : discountAmount;
                    }

                    var subTotalDiscountDetails = new ApiDiscountDetails();
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
                    totalAdjustment = 0;
                    if (discountAmount != 0)
                    {
                        totalAdjustment = shippingDiscountType == DiscountType.Percentage.ToString() ?
                            ((order.Shipping == null ? 0 : Convert.ToDecimal(order.Shipping)) * discountAmount) / 100
                            : discountAmount;
                    }

                    var shippingDiscountDetails = new ApiDiscountDetails();
                    shippingDiscountDetails.DiscountType = shippingDiscountType;
                    shippingDiscountDetails.DiscountAmount = discountAmount;
                    shippingDiscountDetails.TotalAdjustment = totalAdjustment;
                    shippingDiscountDetails.ChargeType = orderSummaryAdj.ShippingChargeType == null ? "" : (orderSummaryAdj.ShippingChargeType == ChargeType.Subtract.ToString() ?
                        ChargeType.Subtract.ToString() : ChargeType.Add.ToString());
                    shippingDiscountDetails.Notes = orderSummaryAdj.ShippingAdjustmentNotes;

                    model.ShippingDiscountDetails = shippingDiscountDetails;


                    #endregion
                }

                model.Tax = order.OrderTax == null ? 0 : Convert.ToDecimal(order.OrderTax);


                model.InvoiceNotes = order.InvoiceNote;
                model.PrivateNotes = order.PrivateOrderNotes;

                // customduty
                if (order.CustomDuty > 0)
                {
                    model.CustomDutyPercentage = order.CustomDutyPercentage;
                    model.CustomDuty = order.CustomDuty;
                }

                if (liveOrder != null)
                {
                    var taxes = _orderService.GetTaxDetails(liveOrder);
                    if (taxes.Where(t => t.TaxType != TaxType.Tax && t.TaxRate > 0).Any())
                    {
                        foreach (var taxInfo in taxes.Where(t => t.TaxRate > 0))
                        {
                            model.TaxInfo.Add(new TaxInfoModel()
                            {
                                Amount = taxInfo.Amount,
                                TaxType = taxInfo.TaxType.ToString(),
                                TaxRate = taxInfo.TaxRate
                            });
                        }
                    }
                }
                // end
            }


            return model;

        }
        public async Task UpdateOrderSections(CustomOrderModel model, OrderTypeUpdate orderTypeUpdate, int customerId = 0)
        {
            var order = await _customOrderService.GetById(model.Id);
            if (order != null)
            {
                if (orderTypeUpdate == OrderTypeUpdate.Notes)
                {
                    order.PrivateOrderNotes = model.PrivateOrderNotes;
                    order.InvoiceNote = model.InvoiceNote;

                    if (
                       (string.IsNullOrEmpty(order.SpecialInstructionsfromBuyer) ? "" : order.SpecialInstructionsfromBuyer).Trim() !=
                              (string.IsNullOrEmpty(model.SpecialInstructionsfromBuyer) ? "" : model.SpecialInstructionsfromBuyer).Trim()
                       )
                    {
                        await this._customOrderService.InsertOrderNotesLogAsync(new CustomOrderNotesLog()
                        {
                            CreatedOn = DateTime.UtcNow,
                            OrderId = order.Id,
                            UserId = customerId == 0 ? (await _workContext.GetCurrentCustomerAsync()).Id : customerId,
                            SpecialInstructions = model.SpecialInstructionsfromBuyer

                        });
                    }
                    order.SpecialInstructionsfromBuyer = model.SpecialInstructionsfromBuyer;
                    var orderStatuses = await _customOrderService.GetOrderStatuses();

                    if (order.StatusId == 0)
                    {
                        order.StatusId = orderStatuses.Where(s => s.Name == MWT.Nop.Core.Domain.CustomOrders.OrderStatus.SavedDraft.ToString()).Any() ?
                               orderStatuses.Where(s => s.Name == MWT.Nop.Core.Domain.CustomOrders.OrderStatus.SavedDraft.ToString()).FirstOrDefault().Id : order.StatusId;
                    }

                    await _customOrderService.UpdateAsync(order);

                    await this._customOrderService.InsertOrderStatusLogAsync(new CustomorderOrderStatusLog()
                    {
                        CreatedOn = DateTime.UtcNow,
                        OrderId = order.Id,
                        StatusId = orderStatuses.Where(s => s.Name == MWT.Nop.Core.Domain.CustomOrders.OrderStatus.OrderSaved.ToString()).Any() ?
                        orderStatuses.Where(s => s.Name == MWT.Nop.Core.Domain.CustomOrders.OrderStatus.OrderSaved.ToString()).FirstOrDefault().Id : order.StatusId,
                        UserId = customerId == 0 ? (await _workContext.GetCurrentCustomerAsync()).Id : customerId
                    });



                }
                else if (orderTypeUpdate == OrderTypeUpdate.Wgs)
                {
                    order.Wgs = model.Wgs < 0 ? 0 : model.Wgs;
                    order.WgsAdjustmentNotes = model.WgsAdjustmentNotes;
                    order.ComplementryWgsFree = model.ComplementryWgsFree;
                    await _customOrderService.UpdateAsync(order);
                }
                else if (orderTypeUpdate == OrderTypeUpdate.Shipping)
                {
                    //decimal shipping = model.Shipping;
                    //decimal shippingDiscount = 0;
                    //DiscountType shippingDiscountType = DiscountType.Percentage;
                    //ChargeType shippingChargeType = ChargeType.Subtract;
                    //string shippingMethod = "";
                    //var shippingMethods = await this.GetShippingMethods();
                    //if (shippingMethods.Count() != 0)
                    //{
                    //    if (shippingMethods.Where(m => string.Compare(m.Key, model.ShippingMethod, false) == 0).Any())
                    //    {
                    //        var shippingMethodObj = shippingMethods.Where(m => string.Compare(m.Key, model.ShippingMethod, false) == 0).FirstOrDefault();
                    //        shippingMethod = shippingMethodObj.Key;
                    //        if (shippingMethodObj.Value)
                    //            shippingDiscount = 100;
                    //    }
                    //    else
                    //        shipping = 0;
                    //}
                    //else
                    //    shipping = 0;

                    var shipping = order.Shipping == null ? 0 : Convert.ToInt32(order.Shipping);
                    decimal shippingDiscount = Convert.ToDecimal(model.ShippingDiscount == null ? 0 : model.ShippingDiscount);
                    var shippingChargeType = ChargeType.Add;
                    if (shippingDiscount < 0)
                        shippingChargeType = ChargeType.Subtract;
                    if (shippingChargeType == ChargeType.Subtract)
                    {

                        if (model.ShippingDiscountType == DiscountType.Fixed.ToString())
                            shippingDiscount = shippingDiscount < 0 ? (shipping < -shippingDiscount ? -shipping : shippingDiscount)
                            : (shipping < shippingDiscount ? shipping : shippingDiscount);

                        else
                            shippingDiscount = shippingDiscount < 0 ? (-shippingDiscount > 100 ? 100 : shippingDiscount) : (
                           shippingDiscount > 100 ? 100 : shippingDiscount);

                    }




                    var orderSummaryAdjObj = await _customOrderService.GetOrderSummaryAdjustment(model.Id);
                    if (orderSummaryAdjObj != null)
                    {
                        orderSummaryAdjObj.ShippingChargeType = shippingChargeType.ToString();
                        orderSummaryAdjObj.ShippingDiscount = shippingDiscount == 0 ? null : (shippingDiscount < 0 ? -shippingDiscount : shippingDiscount);
                        orderSummaryAdjObj.ShippingDiscountType = model.ShippingDiscountType.ToString();
                        orderSummaryAdjObj.ShippingAdjustmentNotes = model.ShippingAdjustmentNotes;
                        await _customOrderService.UpdateCustomOrderOrderSummaryAdjustmentAsync(orderSummaryAdjObj);
                    }
                    else
                    {
                        orderSummaryAdjObj = new CustomOrderOrderSummaryAdjustment();
                        orderSummaryAdjObj.ShippingChargeType = shippingChargeType.ToString();
                        orderSummaryAdjObj.ShippingDiscount = shippingDiscount == 0 ? null : (shippingDiscount < 0 ? -shippingDiscount : shippingDiscount);
                        orderSummaryAdjObj.ShippingDiscountType = model.ShippingDiscountType.ToString();
                        orderSummaryAdjObj.OrderId = model.Id;
                        orderSummaryAdjObj.ShippingAdjustmentNotes = model.ShippingAdjustmentNotes;
                        await _customOrderService.InsertCustomOrderOrderSummaryAdjustmentAsync(orderSummaryAdjObj);
                    }

                    await this.UpdateOrderTotal(model.Id);



                }
                else if (orderTypeUpdate == OrderTypeUpdate.Discount)
                {

                    var subTotal = await this.GetCartItemTotal(model.Id);
                    if ((model.SubTotalDiscountType == DiscountType.Fixed.ToString() || model.SubTotalDiscountType == DiscountType.Percentage.ToString())
                        && model.SubTotalDiscount != null)
                    {
                        decimal subTotalDiscount = Convert.ToDecimal(model.SubTotalDiscount);

                        if (model.SubTotalDiscountType == DiscountType.Fixed.ToString())
                            subTotalDiscount = subTotalDiscount < 0 ? (subTotal < -subTotalDiscount ? -subTotal : subTotalDiscount)
                            : (subTotal < subTotalDiscount ? subTotal : subTotalDiscount);
                        else
                        {
                            subTotalDiscount = subTotalDiscount < 0 ? (-subTotalDiscount > 100 ? 100 : subTotalDiscount) : (
                                subTotalDiscount > 100 ? 100 : subTotalDiscount);
                        }

                        var orderSummaryAdjObj = await _customOrderService.GetOrderSummaryAdjustment(model.Id);
                        if (orderSummaryAdjObj != null)
                        {
                            orderSummaryAdjObj.SubtotalChargeType = ChargeType.Subtract.ToString();
                            orderSummaryAdjObj.SubtotalDiscount = subTotalDiscount < 0 ? -subTotalDiscount : subTotalDiscount;
                            orderSummaryAdjObj.SubTotalDiscountType = model.SubTotalDiscountType;
                            orderSummaryAdjObj.SubTotalAdjustmentNotes = model.SubTotalAdjustmentNotes;
                            await _customOrderService.UpdateCustomOrderOrderSummaryAdjustmentAsync(orderSummaryAdjObj);
                        }
                        else
                        {
                            orderSummaryAdjObj = new CustomOrderOrderSummaryAdjustment();
                            orderSummaryAdjObj.SubtotalChargeType = ChargeType.Subtract.ToString();
                            orderSummaryAdjObj.SubtotalDiscount = subTotalDiscount < 0 ? -subTotalDiscount : subTotalDiscount;
                            orderSummaryAdjObj.SubTotalDiscountType = model.SubTotalDiscountType;
                            orderSummaryAdjObj.OrderId = model.Id;
                            orderSummaryAdjObj.SubTotalAdjustmentNotes = model.SubTotalAdjustmentNotes;
                            await _customOrderService.InsertCustomOrderOrderSummaryAdjustmentAsync(orderSummaryAdjObj);
                        }

                        await this.UpdateOrderTotal(model.Id);
                    }


                }
                else if (orderTypeUpdate == OrderTypeUpdate.PartialOrderPaymentLink)
                {
                    await _workflowMessageService.CustomOrder_SendPartialPaymentLinkNotificationAsync(order, (await _workContext.GetWorkingLanguageAsync()).Id, model.CustomerEmail);
                    if (order.CustomerId != null)
                    {
                        var customer = await _customerService.GetCustomerByIdAsync(Convert.ToInt32(order.CustomerId));
                        if (customer != null)
                            await _customerActivityService.InsertActivityAsync(customer, "CustomerRelatedActivity",
                   $"Invoice sent for Order no {order.Id}");
                    }
                }
                else if (orderTypeUpdate == OrderTypeUpdate.TaxUpdate)
                {
                    order.ApplyTax = model.ApplyTax;
                    await _customOrderService.UpdateAsync(order);
                }
                else if (orderTypeUpdate == OrderTypeUpdate.StatusUpdate)
                {
                    if (model.OrderStatus == MWT.Nop.Core.Domain.CustomOrders.OrderStatus.SavedDraft.ToString())
                    {
                        var orderStatuses = await _customOrderService.GetOrderStatuses();
                        if (order.CreatedOn == null)
                            order.CreatedOn = DateTime.UtcNow;
                        order.StatusId = orderStatuses.Where(s => s.Name == MWT.Nop.Core.Domain.CustomOrders.OrderStatus.SavedDraft.ToString()).Any() ?
                            orderStatuses.Where(s => s.Name == MWT.Nop.Core.Domain.CustomOrders.OrderStatus.SavedDraft.ToString()).FirstOrDefault().Id : order.StatusId;
                        await this._customOrderService.UpdateAsync(order);

                        await this._customOrderService.InsertOrderStatusLogAsync(new CustomorderOrderStatusLog()
                        {
                            CreatedOn = DateTime.UtcNow,
                            OrderId = order.Id,
                            StatusId = orderStatuses.Where(s => s.Name == MWT.Nop.Core.Domain.CustomOrders.OrderStatus.SavedDraft.ToString()).Any() ?
                           orderStatuses.Where(s => s.Name == MWT.Nop.Core.Domain.CustomOrders.OrderStatus.SavedDraft.ToString()).FirstOrDefault().Id : order.StatusId,
                            UserId = customerId == 0 ? (await _workContext.GetCurrentCustomerAsync()).Id : customerId
                        });

                        if (order.CustomerId != null)
                        {
                            var customer = await _customerService.GetCustomerByIdAsync(Convert.ToInt32(order.CustomerId));
                            if (customer != null)
                                await _customerActivityService.InsertActivityAsync(customer, "CustomerRelatedActivity",
                       $"Order status for  {order.Id} changed to Draft");
                        }
                    }
                    else if (model.OrderStatus == MWT.Nop.Core.Domain.CustomOrders.OrderStatus.InvoiceSent.ToString())
                    {
                        var orderStatuses = await _customOrderService.GetOrderStatuses();
                        if (order.CreatedOn == null)
                            order.CreatedOn = DateTime.UtcNow;

                        order.StatusId = orderStatuses.Where(s => s.Name == MWT.Nop.Core.Domain.CustomOrders.OrderStatus.InvoiceSent.ToString()).Any() ?
                            orderStatuses.Where(s => s.Name == MWT.Nop.Core.Domain.CustomOrders.OrderStatus.InvoiceSent.ToString()).FirstOrDefault().Id : order.StatusId;


                        var customer = new Customer();
                        if (order.CustomerId != null)
                        {
                            customer = await _customerService.GetCustomerByIdAsync(Convert.ToInt32(order.CustomerId));
                            if (customer == null)
                                throw new Exception("Please select Customer");

                        }
                        else
                            throw new Exception("Please select Customer");


                        #region Order Zoho Lead
                        var items = await _customOrderService.GetOrderItems(order.Id);

                        string description = "";

                        foreach (var item in items)
                        {
                            var product = await _productService.GetProductByIdAsync(item.ProductId);

                            description += "ProductId:" + item.ProductId + ";SKU:" + product?.Sku ?? "" + "|";
                        }

                        var iPAddress = _httpContextAccessor.HttpContext.Connection.RemoteIpAddress == null ? "" : _httpContextAccessor.HttpContext.Connection.RemoteIpAddress.ToString();



                        order.ZohoPotentialId = await _zohoService.CreateUpdateOrderContactPotential(order.LiveOrderNumber ?? order.Id, order.ZohoPotentialId, customer, MWT.Nop.Core.Domain.CustomOrders.OrderStatus.InvoiceSent.ToString(), description, iPAddress, string.Empty, (order.SubTotal ?? 0) + (order.TotalDiscount ?? 0), order.CreatedBy, $"{_storeContext.GetCurrentStore().Url}checkoutCustomOrder?orderid={order.Id}&customerid={order.CustomerId}", true);



                        #endregion

                        await this._customOrderService.UpdateAsync(order);
                        // send mail


                        var notificationIds = await _workflowMessageService.CustomOrder_SendPaymentLinkNotificationAsync(order, (await _workContext.GetWorkingLanguageAsync()).Id, model.CustomerEmail);
                        if (customer != null)
                            await _customerActivityService.InsertActivityAsync(customer, "CustomerRelatedActivity",
                           $"Invoice sent for Order no {order.Id}");

                        await this._customOrderService.InsertOrderStatusLogAsync(new CustomorderOrderStatusLog()
                        {
                            CreatedOn = DateTime.UtcNow,
                            InvoiceSendTo = model.CustomerEmail,
                            OrderId = order.Id,
                            StatusId = orderStatuses.Where(s => s.Name == MWT.Nop.Core.Domain.CustomOrders.OrderStatus.InvoiceSent.ToString()).Any() ?
                                 orderStatuses.Where(s => s.Name == MWT.Nop.Core.Domain.CustomOrders.OrderStatus.InvoiceSent.ToString()).FirstOrDefault().Id : order.StatusId,
                            UserId = customerId == 0 ? (await _workContext.GetCurrentCustomerAsync()).Id : customerId,
                            NotificationId = notificationIds.Any() ? notificationIds.First() : 0
                        });
                    }
                }
            }
        }

        #endregion


        #region Custom Order Bin
        public async Task ArchiveCustomOrderAsync(int orderId)
        {

            var customOrder = await _customOrderService.GetById(orderId);
            customOrder.IsDeleted = true;
            customOrder.DeletedBy = (await _workContext.GetCurrentCustomerAsync())?.Id ?? 0;
            customOrder.DeletedOn = DateTime.UtcNow;
            await _customOrderService.UpdateAsync(customOrder);
        }
        public async Task RestoreCustomOrderAsync(int orderId)
        {
            //  var _customOrderService = EngineContext.Current.Resolve<ICustomOrderService>();
            var customOrder = await _customOrderService.GetById(orderId);
            customOrder.IsDeleted = false;
            await _customOrderService.UpdateAsync(customOrder);
        }

        #endregion

        #endregion

        #region Common Methods

        public async Task<bool> IsOrderPaid(MWT.Nop.Core.Domain.CustomOrders.CustomOrder order)
        {
            var orderTypes = (await _customOrderService.GetOrderTypes()).Where(o => o.ParentId == 0);
            var isOrderPaid = orderTypes.Where(m => m.Id == order.StatusId && m.Name == MWT.Nop.Core.Domain.CustomOrders.OrderStatus.Paid.ToString()).Any();
            return isOrderPaid;
        }
        public async Task<bool> ProductSearchEnableDisable(MWT.Nop.Core.Domain.CustomOrders.CustomOrder order)
        {
            var customer = new Customer();
            bool enableProductSearch = false;
            if (order.OrderTypeId != 0 && order.OrderTypeId == null)
                enableProductSearch = false;
            else if (order.CustomerId == null || order.CustomerId == 0)
                enableProductSearch = false;

            else
            {
                var orderTypes = await _customOrderService.GetOrderTypes();
                var statusIdAlreadyPaidoBJ = orderTypes.Where(m => String.Compare(m.Name,
                    MWT.Nop.Core.Domain.CustomOrders.OrderStatus.Paid.ToString(), StringComparison.OrdinalIgnoreCase) == 0).FirstOrDefault();
                if (statusIdAlreadyPaidoBJ == null)
                    enableProductSearch = true;

            }
            return enableProductSearch;
        }
        public async Task<bool> CustomerSearchEnableDisable(MWT.Nop.Core.Domain.CustomOrders.CustomOrder order)
        {
            var customer = new Customer();
            bool enableCustomerSearch = false;
            if (order.OrderTypeId != 0 && order.OrderTypeId == null)
                enableCustomerSearch = false;
            else if (order.CustomerId == null || order.CustomerId == 0)
                enableCustomerSearch = true;

            else
            {
                customer = await this._customerService.GetCustomerByIdAsync(Convert.ToInt32(order.CustomerId));
                if (customer == null || customer.ShippingAddressId == null || customer.ShippingAddressId == 0 || customer.BillingAddressId == null || customer.BillingAddressId == 0)
                    enableCustomerSearch = true;
                else
                {
                    if (order.StatusId == 0)
                        enableCustomerSearch = true;
                    else
                    {
                        var orderTypes = await _customOrderService.GetOrderTypes();
                        var statusIdAlreadyPaidoBJ = orderTypes.Where(m => String.Compare(m.Name,
                            MWT.Nop.Core.Domain.CustomOrders.OrderStatus.SavedDraft.ToString(), StringComparison.OrdinalIgnoreCase) == 0).FirstOrDefault();
                        if (statusIdAlreadyPaidoBJ == null)
                            enableCustomerSearch = true;
                    }
                }
            }
            return enableCustomerSearch;
        }

        #endregion

        #region FrontEndMethods

        public async Task<bool> ValidateOrder(int orderId, int customerId)
        {
            var order = await _customOrderService.GetById(orderId);
            if (order == null)
                return false;
            return order.CustomerId == customerId;

        }
        public async Task<bool> ValidateOrderCustomerDetails(int orderId, string emailAddress, string zipCode)
        {
            var order = await _customOrderService.GetById(orderId);
            if (order == null)
                return false;
            if (order.CustomerId == null || order.CustomerId == 0)
                return false;
            var customer = await _customerService.GetCustomerByIdAsync(Convert.ToInt32(order.CustomerId));
            if (customer == null || customer.ShippingAddressId == null || customer.ShippingAddressId == 0)
                return false;
            string email = customer.Email;
      
            if (String.Compare(email.Trim(), emailAddress.Trim(), StringComparison.OrdinalIgnoreCase) != 0 && String.Compare((order.CustomerCCEmail ?? string.Empty).Trim(), emailAddress.Trim(), StringComparison.OrdinalIgnoreCase) != 0)
                return false;
            else
            {
                var address = await this._customerService.GetCustomerShippingAddressAsync(customer);
                if (String.Compare(zipCode.Trim(), address.ZipPostalCode.Trim(), StringComparison.OrdinalIgnoreCase) != 0)
                    return false;

            }
            return true;
        }

        #endregion



        #endregion


        #region Utilities
        public async Task<(List<TaxInfo>, string)> GetTaxDetails(string taxInfo)
        {
            string html = "";
            var taxdetails = new List<TaxInfo>();
            if (string.IsNullOrEmpty(taxInfo))
            {
                return (taxdetails, html);
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
            if (taxdetails.Where(t => t.TaxType != TaxType.Tax && t.TaxRate > 0).Any())
            {
                foreach (var tax in taxdetails)
                {
                    html += $"<tr class=\"border-0\"><td scope=\"row\" class=\"border-0 text-uppercase\">{string.Format(await _localizationService.GetResourceAsync("Canada.Tax.Label"), tax.TaxType.ToString(), tax.TaxRate.ToString("F2"))}</td><td clas=\"border-0 text-uppercase\" ></td><td class=\"border-0 text-uppercase\">" +
                        $"{await _priceFormatter.FormatPriceAsync(tax.Amount)}</td></tr>";
                }
            }
            return (taxdetails, html);
        }

        public async Task<bool> IsOrderEditable(MWT.Nop.Core.Domain.CustomOrders.CustomOrder order)
        {
            var orderStatuses = await this._customOrderService.GetOrderStatuses();
            var status = orderStatuses.Where(s => s.Id == order.StatusId).FirstOrDefault();
            if (status == null || String.Compare(status.Name,
                            MWT.Nop.Core.Domain.CustomOrders.OrderStatus.Paid.ToString(), StringComparison.OrdinalIgnoreCase) != 0)
                return true;
            return false;

        }
        public async Task UpdateOrderTotal(int orderId, bool updateWgsCharges = false)
        {

            var order = await _customOrderService.GetById(orderId);
            var orderStatuses = await this._customOrderService.GetOrderStatuses();
            var status = orderStatuses.Where(m => m.Id == order.StatusId).FirstOrDefault()?.Name;
            if (status != MWT.Nop.Core.Domain.CustomOrders.OrderStatus.Paid.ToString())
            {


                if (order != null)
                {
                    var subTotal = await GetCartItemTotal(orderId);

                    // wgs pending
                    // already feed

                    decimal total = 0;
                    decimal subTotalDiscount = 0;
                    decimal shippingDiscount = 0;


                    var shipping = await this.GetShipping(orderId);
                    var wgs = order.Wgs == null ? 0 : Convert.ToDecimal(order.Wgs);
                    decimal tax = 0;
                    if (updateWgsCharges && string.IsNullOrEmpty(order.WgsAdjustmentNotes))
                    {
                        wgs = (await this.GetWgsCharges(order, subTotal, await _customOrderService.IsSurchargeApplicable(order))).wgsCharges;
                        order.Wgs = wgs;

                    }

                    var orderSummryAdj = await _customOrderService.GetOrderSummaryAdjustment(orderId);

                    if (orderSummryAdj != null)
                    {

                        if (orderSummryAdj.SubtotalDiscount != null)
                        {
                            if (orderSummryAdj.SubTotalDiscountType == DiscountType.Fixed.ToString())
                                subTotalDiscount = Convert.ToDecimal(orderSummryAdj.SubtotalDiscount);
                            else
                                subTotalDiscount = (subTotal * Convert.ToDecimal(orderSummryAdj.SubtotalDiscount)) / 100;
                            if (orderSummryAdj.SubtotalChargeType == ChargeType.Subtract.ToString())
                                subTotalDiscount = -subTotalDiscount;
                        }

                        if (orderSummryAdj.ShippingDiscount != null)
                        {
                            if (orderSummryAdj.ShippingDiscountType == DiscountType.Fixed.ToString())
                                shippingDiscount = Convert.ToDecimal(orderSummryAdj.ShippingDiscount);
                            else
                                shippingDiscount = (shipping * Convert.ToDecimal(orderSummryAdj.ShippingDiscount)) / 100;
                            if (orderSummryAdj.ShippingChargeType == ChargeType.Subtract.ToString())
                                shippingDiscount = -shippingDiscount;
                        }
                    }

                    //Added Check For if it was addational servcie than the tax would be 0
                    decimal taxRate = 0;
                    decimal customDuty = order.CustomDuty;
                    decimal customDutyPercentage = 0;
                    List<TaxInfo> taxes = new List<TaxInfo>();

                    if (status != MWT.Nop.Core.Domain.CustomOrders.OrderStatus.Paid.ToString())
                        order.Shipping = shipping;
                    order.SubTotal = subTotal;

                    var customer = await this._customerService.GetCustomerByIdAsync(Convert.ToInt32(order.CustomerId));
                    bool includeShipping = false;
                    if (subTotal > 0 && order.CustomerId != null && order.CustomerId != 0 &&
         (status != MWT.Nop.Core.Domain.CustomOrders.OrderStatus.Paid.ToString()))
                    {

                        if (customer != null && customer.ShippingAddressId != null && customer.ShippingAddressId > 0)
                        {
                            var address = await _customerService.GetCustomerShippingAddressAsync(customer);
                            if (address.CountryId != null && address.StateProvinceId != null)
                            {
                                if (order.ApplyTax)
                                {
                                    includeShipping = await _settingService.GetSettingByKeyAsync<bool>($"ShippingIsTaxable_{address?.CountryId ?? 0}");
                                    var taxTotalResult = await GetOrderTax(customer, subTotal + subTotalDiscount,
                                         shipping + shippingDiscount + wgs);
                                    tax = taxTotalResult?.TaxTotal ?? 0;
                                    taxRate = taxTotalResult?.TaxRates.FirstOrDefault().Key ?? 0;
                                    taxes = taxTotalResult.Taxes;
                                }

                                (customDutyPercentage, customDuty) = await _orderTotalCalculationService.GetCustomDuty(new List<ShoppingCartItem>(), true, customer, subTotal + subTotalDiscount);
                            }
                        }

                    }

                    string taxInfo = "";
                    foreach (var _tax in taxes.Where(t => t.TaxRate > 0))
                    {
                        taxInfo += $"{_tax.TaxType.ToString()}:{_tax.TaxRate.ToString()}:{_tax.Amount};";
                    }
                    order.TaxInfo = status == MWT.Nop.Core.Domain.CustomOrders.OrderStatus.Paid.ToString() ? order.TaxInfo : taxInfo;

                    order.TaxRate = status == MWT.Nop.Core.Domain.CustomOrders.OrderStatus.Paid.ToString() ? order.TaxRate : taxRate;
                    order.OrderTax = status == MWT.Nop.Core.Domain.CustomOrders.OrderStatus.Paid.ToString() ?
                        order.OrderTax : tax;

                    order.CustomDuty = status == MWT.Nop.Core.Domain.CustomOrders.OrderStatus.Paid.ToString() ?
                   order.CustomDuty : customDuty;

                    order.CustomDutyPercentage = status == MWT.Nop.Core.Domain.CustomOrders.OrderStatus.Paid.ToString() ?
                   order.CustomDutyPercentage : customDutyPercentage;

                    order.TotalDiscount = subTotalDiscount;
                    order.OrderTotal = subTotal + subTotalDiscount + shipping + shippingDiscount + wgs + tax + customDuty;


                    await _customOrderService.UpdateAsync(order);
                }

            }
        }
        protected async Task<decimal> GetCartItemTotal(int orderId)
        {
            var items = await _customOrderService.GetOrderItems(orderId);
            decimal total = 0;
            foreach (var _item in items)
            {
                var _priceAdjustment = await _customOrderService.GetPriceAdjustmentsByCartId(_item.Id);
                if (_priceAdjustment != null)
                {
                    decimal totalAdjustment = (_priceAdjustment.Discounttype == DiscountType.Percentage.ToString() && _priceAdjustment.DiscountPercentage != null &&
                         _priceAdjustment.DiscountPercentage > 0 ?
                        (_priceAdjustment.ShoppingCartProductPrice == null ? 0 : Convert.ToDecimal(_priceAdjustment.ShoppingCartProductPrice)
                        * Convert.ToDecimal(_priceAdjustment.DiscountPercentage)) / 100
                        : (_priceAdjustment.Discounttype == DiscountType.Fixed.ToString() && _priceAdjustment.Discountamount != null && _priceAdjustment.Discountamount > 0
                        ? Convert.ToDecimal(_priceAdjustment.Discountamount) : 0)) * _item.Quantity;

                    if (_priceAdjustment.Chargestype == ChargeType.Subtract.ToString())
                        total += ((
        _priceAdjustment.ShoppingCartProductPrice == null ? 0 : Convert.ToDecimal(_priceAdjustment.ShoppingCartProductPrice)) * _item.Quantity)
        - totalAdjustment;
                    else if (_priceAdjustment.Chargestype == ChargeType.Add.ToString())
                        total += ((
        _priceAdjustment.ShoppingCartProductPrice == null ? 0 : Convert.ToDecimal(_priceAdjustment.ShoppingCartProductPrice)) * _item.Quantity)
        + totalAdjustment;
                    else
                        total += (
        _priceAdjustment.ShoppingCartProductPrice == null ? 0 : Convert.ToDecimal(_priceAdjustment.ShoppingCartProductPrice * _item.Quantity));
                }

            }
            return total;
        }
        protected async Task<decimal> GetShipping(int orderId)
        {
            var items = await _customOrderService.GetOrderItems(orderId);
            decimal shipping = 0;
            foreach (var _item in items)
            {
                var product = await _productService.GetProductByIdAsync(_item.ProductId);
                shipping = shipping + (product.ShippingPrice == null ? 0 : (product.ShippingPrice < 0 ? 0 : Convert.ToDecimal(product.ShippingPrice)));
            }
            return shipping;
        }
        protected async Task<List<MWT.Plugin.Misc.MwtStorefront.Areas.CustomOrder.Models.Products.ProductAttribute>> PrepareProductAttributes(List<AttributeCombination> attributeCombinations)
        {
            List<MWT.Plugin.Misc.MwtStorefront.Areas.CustomOrder.Models.Products.ProductAttribute> attributes = new List<Models.Products.ProductAttribute>();

            if (attributeCombinations.Count > 0)
            {

                foreach (var combination in attributeCombinations)
                {
                    foreach (var prdAttr in combination.Combinations)
                    {
                        var obj = attributes.Where(m => m.ParentAttributeId == prdAttr.ParentAttributeId).FirstOrDefault();
                        if (obj == null)
                        {
                            attributes.Add(new Models.Products.ProductAttribute()
                            {
                                ParentAttributeId = prdAttr.ParentAttributeId,
                                ParentAtributeName = prdAttr.ParentAtributeName,
                                Values = new List<Models.Products.Attribute>()
                            });
                            obj = attributes.Where(m => m.ParentAttributeId == prdAttr.ParentAttributeId).FirstOrDefault();
                        }

                        if (obj != null)
                        {
                            var childAttr = obj.Values.Where(m => m.AttributeId == prdAttr.AttributeId).FirstOrDefault();
                            if (childAttr == null)
                                obj.Values.Add(new Models.Products.Attribute()
                                {
                                    AttributeId = prdAttr.AttributeId,
                                    AttributeName = prdAttr.AttributeName
                                });
                        }

                    }

                }
            }
            return attributes;
        }
        protected async Task<List<AttributeCombination>> PrepareProductAttributeCombinations(Product product)
        {
            List<AttributeCombination> prdCombinationsModel = new List<AttributeCombination>();
            var prdCombinations = await _productAttributeService.GetAllProductAttributeCombinationsAsync(product.Id);

            foreach (var prdCombination in prdCombinations)
            {
                AttributeCombination model = new AttributeCombination();
                model.Stock = prdCombination.StockQuantity;
                List<MWT.Plugin.Misc.MwtStorefront.Areas.CustomOrder.Models.Products.Attribute> _productAttributes = new List<Models.Products.Attribute>();
                decimal price =
                    prdCombination.OverriddenOldPrice == null || prdCombination.OverriddenOldPrice <= 0 ? (
                    prdCombination.OverriddenPrice == null ? product.Price : Convert.ToDecimal(prdCombination.OverriddenPrice)
                    ) : Convert.ToDecimal(prdCombination.OverriddenOldPrice);

                foreach (var attribute in await _productAttributeParser.ParseProductAttributeMappingsAsync(prdCombination.AttributesXml))
                {
                    var productAttribute = await _productAttributeService.GetProductAttributeByIdAsync(attribute.ProductAttributeId);
                    if (productAttribute != null)
                    {
                        MWT.Plugin.Misc.MwtStorefront.Areas.CustomOrder.Models.Products.Attribute _productAttribute = new Models.Products.Attribute();
                        var attributeName = await _localizationService.GetLocalizedAsync(productAttribute, a => a.Name, (await _workContext.GetWorkingLanguageAsync()).Id);
                        _productAttribute.ParentAttributeId = productAttribute.Id;
                        _productAttribute.ParentAtributeName = productAttribute.Name;

                        if (attribute.ShouldHaveValues())
                        {
                            foreach (var attributeValue in await _productAttributeParser.ParseProductAttributeValuesAsync(prdCombination.AttributesXml, attribute.Id))
                            {
                                _productAttribute.AttributeId = attributeValue.Id;
                                _productAttribute.AttributeName = attributeValue.Name;
                                _productAttributes.Add(_productAttribute);
                                price = price + attributeValue.PriceAdjustment;
                                break;
                            }

                        }
                    }
                }
                if (_productAttributes.Count > 0)
                {
                    model.Price = await _priceFormatter.FormatPriceAsync(price);
                    model.Combinations = _productAttributes;
                    prdCombinationsModel.Add(model);
                }

            }
            return prdCombinationsModel;
        }
        protected virtual async Task<PictureModel> PrepareProductOverviewPictureModelAsync(Product product, int? productThumbPictureSize = null)
        {
            if (product == null)
                throw new ArgumentNullException(nameof(product));

            var productName = await _localizationService.GetLocalizedAsync(product, x => x.Name);
            //If a size has been set in the view, we use it in priority
            var pictureSize = productThumbPictureSize ?? _mediaSettings.ProductThumbPictureSize;

            //prepare picture model



            var picture = (await _pictureService.GetPicturesByProductIdAsync(product.Id, 1)).FirstOrDefault();
            string fullSizeImageUrl, imageUrl;
            (imageUrl, picture) = await _pictureService.GetPictureUrlAsync(picture, pictureSize);
            (fullSizeImageUrl, picture) = await _pictureService.GetPictureUrlAsync(picture);

            var pictureModel = new PictureModel
            {
                ImageUrl = imageUrl,
                FullSizeImageUrl = fullSizeImageUrl,
                //"title" attribute
                Title = (picture != null && !string.IsNullOrEmpty(picture.TitleAttribute))
                    ? picture.TitleAttribute
                    : string.Format(await _localizationService.GetResourceAsync("Media.Product.ImageLinkTitleFormat"),
                        productName),
                //"alt" attribute
                AlternateText = (picture != null && !string.IsNullOrEmpty(picture.AltAttribute))
                    ? picture.AltAttribute
                    : string.Format(await _localizationService.GetResourceAsync("Media.Product.ImageAlternateTextFormat"),
                        productName)
            };


            return pictureModel;
        }
        protected async Task<CustomOrderModel> PrepareCustomOrderModelByOrderDomain(Order order)
        {
            CustomOrderModel model = new CustomOrderModel();
            model.OrderTotal = order.OrderTotal > 0 ? await _priceFormatter.FormatPriceAsync(order.OrderTotal) : "";
            model.OrderTax = order.OrderTax > 0 ? await _priceFormatter.FormatPriceAsync(order.OrderTax) : ""; ;
            model.CreatedOn = order.CreatedOnUtc;
            model.LiveOrderNumber = order.Id;
            model.CustomerId = order.CustomerId;
            model.isShipped = order.ShippingStatus == ShippingStatus.Shipped ? true : false;
            model.isDelivered = order.ShippingStatus == ShippingStatus.Delivered ? true : false;
            model.Items = await this.BindOfOrderItemsByOrderItemDomain(order.Id);
            return model;
        }
        protected virtual async Task<CustomOrderModel> PrepareCustomerOrderModel(MWT.Nop.Core.Domain.CustomOrders.CustomOrder order, int alreadyPaidStatusId)
        {
            var customOrderModel = new CustomOrderModel();
            try
            {


                customOrderModel.Id = order.Id;
                customOrderModel.CreatedOn = order.CreatedOn;
                customOrderModel.FormattedCreatedOn = order.CreatedOn == null ? "" : Convert.ToDateTime(order.CreatedOn).ToString("yyyy/MM/dd");
                //binding exp date by nikhil
                customOrderModel.PromiseDayDate = order.PromiseDayDate;
                customOrderModel.FormattedPromiseDayDate = order.PromiseDayDate == null ? "" : Convert.ToDateTime(order.PromiseDayDate).ToString("yyyy/MM/dd");
                customOrderModel.DeletedOn = order.DeletedOn;
                customOrderModel.IsDeleted = order.IsDeleted;
                if (order.DeletedBy > 0)
                {
                    var customer = await _customerService.GetCustomerByIdAsync(order.DeletedBy);
                    if (customer != null)
                    {
                        customOrderModel.DeletedBy = await _customerService.GetCustomerFullNameAsync(customer);
                    }
                }

                if (order.CustomerId != null && order.CustomerId > 0)
                {
                    var customer = await _customerService.GetCustomerByIdAsync(Convert.ToInt32(order.CustomerId));
                    if (customer != null)
                    {
                        var FirstName = customer.FirstName;
                        var LastName = customer.LastName;
                        var email = await _genericAttributeService.GetAttributeAsync<string>(customer, "Email");
                        customOrderModel.CustomerName = (FirstName == null ? "" : FirstName) + " " +
                            (LastName == null ? "" : LastName);


                        customOrderModel.CustomerEmail = customer != null ? (customer.Email == null ? email == null ? "" : email : customer.Email) : "";
                    }
                }
                else
                {
                    customOrderModel.CustomerEmail = "";
                    customOrderModel.CustomerName = "";
                }


                customOrderModel.OrderTotal = order.OrderTotal == null ? "" : await this._priceFormatter.FormatPriceAsync(Convert.ToDecimal(order.OrderTotal));
                customOrderModel.NoOfItems = (await _customOrderService.GetOrderItems(order.Id)).Count;

                if (order.AlreadyFee != null && order.AlreadyFee > 0 && !order.FullPaid)
                {
                    customOrderModel.IsPartialOrder = true;
                    customOrderModel.Pending = await this._priceFormatter.FormatPriceAsync(
                        Convert.ToDecimal(order.OrderTotal) - ((Convert.ToDecimal(order.OrderTotal) * Convert.ToDecimal(order.AlreadyFee)) / 100));
                }

                if (order.LiveOrderNumber != null && order.LiveOrderNumber > 0)
                {
                    if (order.AlreadyFee != null && order.AlreadyFee > 0 && !order.FullPaid && order.StatusId != alreadyPaidStatusId)
                        customOrderModel.OrderStatus = await this._localizationService.GetResourceAsync("CustomOrder.Status.PartiallyPaid");
                    else
                        customOrderModel.OrderStatus = await this._localizationService.GetResourceAsync("CustomOrder.Status.Paid");
                }
                else
                    customOrderModel.OrderStatus = await this._localizationService.GetResourceAsync("CustomOrder.Status.Pending");

            }
            catch (Exception exp)
            {

            }

            return customOrderModel;

        }
        protected async Task<PictureModel> PrepareCartItemPictureModelAsync(string attributesXml, Product product, int pictureSize, bool showDefaultPicture, string productName)
        {
            //shopping cart item picture
            var sciPicture = await _pictureService.GetProductPictureAsync(product, attributesXml);

            return new PictureModel
            {
                ImageUrl = (await _pictureService.GetPictureUrlAsync(sciPicture, pictureSize, showDefaultPicture)).Url,
                Title = string.Format(await _localizationService.GetResourceAsync("Media.Product.ImageLinkTitleFormat"), productName),
                AlternateText = string.Format(await _localizationService.GetResourceAsync("Media.Product.ImageAlternateTextFormat"), productName),
            };
        }
        protected async Task<Dictionary<string, bool>> GetShippingMethods()
        {
            Dictionary<string, bool> shippingMethods = new Dictionary<string, bool>();
            try
            {
                var shippingMethodsSettings = await _settingService.GetSettingAsync("customorder.shipping.methods");

                if (shippingMethodsSettings != null)
                {
                    string shippingMethodsRaw = shippingMethodsSettings.Value;
                    foreach (var method in shippingMethodsRaw.Split('|'))
                    {
                        try
                        {
                            if (method.Split('@').Length > 1)
                            {
                                bool isFree = false;
                                bool.TryParse(method.Split('@')[1], out isFree);
                                shippingMethods.Add(method.Split('@')[0], isFree);
                            }
                            else
                            {
                                if (!shippingMethods.Where(m => m.Key == method).Any())
                                    shippingMethods.Add(method, false);
                            }
                        }
                        catch
                        {

                        }
                    }
                }
            }
            catch
            {

            }
            return shippingMethods;

        }
        public async Task<(decimal wgsCharges, decimal defaultWgsCharges, decimal surchargeAmount)> GetWgsCharges(MWT.Nop.Core.Domain.CustomOrders.CustomOrder order, decimal subTotal, bool IsSurchargeApplicable = false)
        {
            decimal wgsCharges = 0;
            decimal defaultWgsCharges = 0;
            decimal surchargeAmount = 0;
            List<CustomOrderShoppingCartItem> items = await this._customOrderService.GetOrderItems(order.Id);
            if (items.Count > 0)
            {

                if (order.CustomerId != null && order.CustomerId != 0)
                {
                    var customer = await _customerService.GetCustomerByIdAsync(Convert.ToInt32(order.CustomerId));
                    var storeId = (await this._storeContext.GetCurrentStoreAsync()).Id;
                    var shippingRateComputationMethods = await _shippingPluginManager
                .LoadActivePluginsAsync(customer, storeId, "");
                    if (shippingRateComputationMethods.Any())
                    {
                        await this.AddItemsToCart(order, customer);
                        var result = new GetShippingOptionResponse();

                        var cart = await this._shoppingCartService.GetShoppingCartAsync(customer, ShoppingCartType.ShoppingCart, storeId);
                        //create a package
                        var (shippingOptionRequests, shippingFromMultipleLocations) = await CreateShippingOptionRequestsAsync(cart, await _customerService.GetCustomerShippingAddressAsync(customer)
                            , storeId);
                        if (IsSurchargeApplicable)
                        {
                            foreach (var shippingOptionRequest in shippingOptionRequests)
                            {
                                shippingOptionRequest.IsSurchargeApplicable = IsSurchargeApplicable;
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
                    var cartItems = await _shoppingCartService.GetShoppingCartAsync(customer, ShoppingCartType.ShoppingCart, (await _storeContext.GetCurrentStoreAsync()).Id);
                    foreach (var cartItem in cartItems)
                    {
                        await _shoppingCartService.DeleteShoppingCartItemAsync(cartItem);
                    }
                }
            }
            return (wgsCharges, defaultWgsCharges, surchargeAmount);
        }

        public virtual async Task<(IList<GetShippingOptionRequest> shipmentPackages, bool shippingFromMultipleLocations)> CreateShippingOptionRequestsAsync(IList<ShoppingCartItem> cart,
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

                var product = await _productService.GetProductByIdAsync(sci.ProductId);
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
                        warehouse = await this._warehouseService.GetWarehouseByIdAsync(product.WarehouseId);
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
        protected async Task AddItemsToCart(MWT.Nop.Core.Domain.CustomOrders.CustomOrder order, Customer customer)
        {
            var storID = (await _storeContext.GetCurrentStoreAsync()).Id;
            var cartItems = await _shoppingCartService.GetShoppingCartAsync(customer, ShoppingCartType.ShoppingCart, storID);
            foreach (var cartItem in cartItems)
            {
                await _shoppingCartService.DeleteShoppingCartItemAsync(cartItem);
            }

            var items = await _customOrderService.GetOrderItems(order.Id);
            foreach (var _item in items)
            {
                var product = await _productService.GetProductByIdAsync(_item.ProductId);
                var _priceAdjustment = await _customOrderService.GetPriceAdjustmentsByCartId(_item.Id);
                decimal price = 0;
                if (_priceAdjustment != null)
                {
                    decimal discountAmount = _priceAdjustment.Discountamount == null ? 0 : Convert.ToDecimal(_priceAdjustment.Discountamount);
                    decimal discountPercentage = _priceAdjustment.DiscountPercentage == null ? 0 : Convert.ToDecimal(_priceAdjustment.DiscountPercentage);
                    price = _priceAdjustment.ShoppingCartProductPrice == null ? 0 : Convert.ToDecimal(_priceAdjustment.ShoppingCartProductPrice);
                    decimal totalAdjustment = (_priceAdjustment.Discounttype == DiscountType.Percentage.ToString() && discountPercentage != 0 ?
                        (_priceAdjustment.ShoppingCartProductPrice == null ? 0 : Convert.ToDecimal(_priceAdjustment.ShoppingCartProductPrice)
                        * Convert.ToDecimal(_priceAdjustment.DiscountPercentage)) / 100
                        : (_priceAdjustment.Discounttype == DiscountType.Fixed.ToString() && discountAmount != 0
                        ? discountAmount : 0));


                    if (_priceAdjustment.Chargestype == ChargeType.Subtract.ToString())
                        price = ((
        _priceAdjustment.ShoppingCartProductPrice == null ? 0 : Convert.ToDecimal(_priceAdjustment.ShoppingCartProductPrice)))
        - totalAdjustment;
                    else if (_priceAdjustment.Chargestype == ChargeType.Add.ToString())
                        price = ((
     _priceAdjustment.ShoppingCartProductPrice == null ? 0 : Convert.ToDecimal(_priceAdjustment.ShoppingCartProductPrice)))
     + totalAdjustment;

                    else
                        price = ((
  _priceAdjustment.ShoppingCartProductPrice == null ? 0 : Convert.ToDecimal(_priceAdjustment.ShoppingCartProductPrice)));



                }
                await _shoppingCartService.CustomAddToCartAsync(customer, product, ShoppingCartType.ShoppingCart, storID,
               String.IsNullOrEmpty(_item.AttributesDescription) ?
                _item.CustomAttributesDescription : _item.AttributesDescription, price, null, null, _item.Quantity, false);
            }
        }



        #endregion


        #region Additioal Services


        public async Task AdditionalServiceSendInvoice(MWT.Nop.Core.Domain.CustomOrders.CustomOrder order, decimal previousWgsAmount, decimal previousWgsDiscount, decimal currentWgsAmount, decimal currentWgsDiscount, decimal defaultWgsCharges = 0, decimal surchargeAmount = 0)
        {
            if (order != null)
            {
                var orderStatuses = await _customOrderService.GetOrderStatuses();
                if (order.CreatedOn == null)
                    order.CreatedOn = DateTime.UtcNow;

                order.StatusId = orderStatuses.Where(s => s.Name == MWT.Nop.Core.Domain.CustomOrders.OrderStatus.InvoiceSent.ToString()).Any() ?
                    orderStatuses.Where(s => s.Name == MWT.Nop.Core.Domain.CustomOrders.OrderStatus.InvoiceSent.ToString()).FirstOrDefault().Id : order.StatusId;
                await this._customOrderService.UpdateAsync(order);

                var customer = new Customer();
                if (order.CustomerId != null)
                {
                    customer = await _customerService.GetCustomerByIdAsync(Convert.ToInt32(order.CustomerId));
                    if (customer == null)
                        throw new Exception("Please select Customer");

                }
                else
                    throw new Exception("Please select Customer");

                // send mail


                var notificationIds = await _workflowMessageService.CustomOrder_SendPaymentLinkNotificationAsync(order, (await _workContext.GetWorkingLanguageAsync()).Id);
                if (customer != null)
                    await _customerActivityService.InsertActivityAsync(customer, "CustomerRelatedActivity",
                   $"Invoice sent for Order no {order.Id}");

                await this._customOrderService.InsertOrderStatusLogAsync(new CustomorderOrderStatusLog()
                {
                    CreatedOn = DateTime.UtcNow,
                    InvoiceSendTo = string.IsNullOrEmpty(order.CustomerCCEmail) ? customer.Email : order.CustomerCCEmail,
                    OrderId = order.Id,
                    StatusId = orderStatuses.Where(s => s.Name == MWT.Nop.Core.Domain.CustomOrders.OrderStatus.InvoiceSent.ToString()).Any() ?
                         orderStatuses.Where(s => s.Name == MWT.Nop.Core.Domain.CustomOrders.OrderStatus.InvoiceSent.ToString()).FirstOrDefault().Id : order.StatusId,
                    UserId = (await _workContext.GetCurrentCustomerAsync()).Id,
                    Comments = (currentWgsAmount <= 0 ? $"WGS created, Order Total {await _priceFormatter.FormatPriceAsync(previousWgsAmount - previousWgsDiscount)}" :
                    previousWgsAmount == currentWgsAmount && previousWgsDiscount == currentWgsDiscount ? $"WGS Invoice Sent Order Total {await _priceFormatter.FormatPriceAsync(previousWgsAmount - previousWgsDiscount)}" : (

                    ($"WGS had a past price of {await _priceFormatter.FormatPriceAsync(previousWgsAmount)} with a discount of {await _priceFormatter.FormatPriceAsync(previousWgsDiscount)}, " +
                     $"and is now changed to {await _priceFormatter.FormatPriceAsync(currentWgsAmount)} with a discount of {await _priceFormatter.FormatPriceAsync(currentWgsDiscount)}.")))
                     + $"{(defaultWgsCharges > 0 ? "(Amount " + await _priceFormatter.FormatPriceAsync(defaultWgsCharges) + ", and Surcharge " + await _priceFormatter.FormatPriceAsync(surchargeAmount) + ")" : "")}",
                    NotificationId = notificationIds.Any() ? notificationIds.First() : 0
                });
            }
        }


        public async Task<AdditionalServiceModel> PrepareAdditionalServiceModel(AdditionalServiceModel model, MWT.Nop.Core.Domain.CustomOrders.CustomOrder Customorder = null,
            Order order = null,

            MWT.Nop.Core.Domain.CustomOrders.OrderStatus status = MWT.Nop.Core.Domain.CustomOrders.OrderStatus.SavedDraft)
        {
            if (order != null)
            {


                (Dictionary<int, dynamic> pairedOrders, string validPairedOrders, decimal subTotal, decimal orderTotal, bool IsSurchargeApplicable) =
                    await this.GetAdditionalServiceOrderInfo(model.Status, order, await _customOrderService.GetByOrderNumber(order.Id), model.PairedOrderIds);
                model.OrderTotal = await _priceFormatter.FormatPriceAsync(orderTotal);
                model.OrderSubTotal = await _priceFormatter.FormatPriceAsync(subTotal);
                model.PairedOrderIds = validPairedOrders;
                model.PairedOrders = pairedOrders;

                await BindingAddress(model, order);
            }
            if (Customorder != null)
                await BindPaymentDetails(model, Customorder);

            model.CustomerId = Customorder?.CustomerId ?? 0;
            model.ServiceProducts = (List<Product>)await _productService.GetServiceTypeProducts();
            if (Customorder != null && order != null)
            {
                if (model.Status == MWT.Nop.Core.Domain.CustomOrders.OrderStatus.Paid)
                    model.Tax = Math.Round(Customorder.OrderTax ?? 0, 2);
                else
                {
                    if (model.ApplyTax)
                    {
                        var customer = await _customerService.GetCustomerByIdAsync(order.CustomerId);
                        if (customer != null)
                        {
                            var taxTotalResult = await this.GetOrderTax(customer, (model.ServicePrice ?? 0) - (model.DiscountPrice ?? 0), 0);
                            model.Tax = Math.Round(taxTotalResult?.TaxTotal ?? 0, 2);
                        }
                    }
                }
            }
            return model;
        }

        public async Task<(Dictionary<int, dynamic> pairedOrders, string validPairedOrders, decimal orderSubTotal, decimal orderTotal, bool isSurchargeApplicable)> GetAdditionalServiceOrderInfo
            (MWT.Nop.Core.Domain.CustomOrders.OrderStatus status, Order order, MWT.Nop.Core.Domain.CustomOrders.CustomOrder customOrder, string pairedOrderIds)
        {
            decimal orderTotal = order.OrderTotal;
            decimal orderSubtotal = 0;
            bool IsSurchargeApplicable = false;
            Dictionary<int, dynamic> pairedOrders = new Dictionary<int, dynamic>();
            dynamic orderInfo = new ExpandoObject();

            if (customOrder != null)
            {
                IsSurchargeApplicable = await _customOrderService.IsSurchargeApplicable(customOrder);
            }
            else
            {
                IsSurchargeApplicable = await _orderService.IsSurchargeApplicable(order);
            }
            List<(int orderID, string serviceName)> systemPairOrders = new List<(int orderId, string serviceName)>();
            if (status != MWT.Nop.Core.Domain.CustomOrders.OrderStatus.Paid)
            {
                systemPairOrders = await _manageService.GetPairedOrders(order.Id);
            }

            string validOrderIds = string.Empty;
            if (!string.IsNullOrEmpty(pairedOrderIds) || systemPairOrders.Count > 0)
            {
                int[] _pairedOrderIds = (pairedOrderIds ?? string.Empty).Split(',').Where(s => int.TryParse(s, out int _out)).Select(s => int.Parse(s)).Where(s => s != order.Id).Distinct().ToArray();
                if (_pairedOrderIds.Length > 0 || systemPairOrders.Count > 0)
                {
                    orderInfo.SubTotal = await _priceFormatter.FormatPriceAsync(customOrder == null ? order.OrderSubtotalInclTax : (customOrder.SubTotal == null ? 0 : Convert.ToDecimal(customOrder.SubTotal)));
                    orderInfo.OrderTotal = await _priceFormatter.FormatPriceAsync(order.OrderTotal);
                    orderInfo.IsPairOrder = false;
                    orderInfo.IsFound = true;
                    orderInfo.IsPaid = false;
                    orderInfo.IsSelected = false;
                    pairedOrders.Add(order.Id, orderInfo);
                }


                foreach (var orderId in _pairedOrderIds)
                {
                    var _order = await _orderService.GetOrderByIdAsync(orderId);
                    if (_order != null)
                    {
                        var _customOrder = await _customOrderService.GetByOrderNumber(orderId);

                        if (!IsSurchargeApplicable)
                        {
                            if (_customOrder != null)
                            {
                                IsSurchargeApplicable = await _customOrderService.IsSurchargeApplicable(_customOrder);
                            }
                            else
                            {
                                IsSurchargeApplicable = await _orderService.IsSurchargeApplicable(_order);
                            }
                        }

                        dynamic _orderInfo = new ExpandoObject();
                        _orderInfo.SubTotal = await _priceFormatter.FormatPriceAsync(_customOrder == null ? _order.OrderSubtotalInclTax : (_customOrder.SubTotal == null ? 0 : Convert.ToDecimal(_customOrder.SubTotal)));
                        _orderInfo.OrderTotal = await _priceFormatter.FormatPriceAsync(_order.OrderTotal);
                        _orderInfo.IsPairOrder = true;
                        _orderInfo.IsFound = systemPairOrders.Where(s => s.orderID == _order.Id).Any();
                        _orderInfo.IsPaid = systemPairOrders.Where(s => s.orderID == _order.Id && s.serviceName == "WGS Service").Any();
                        _orderInfo.IsSelected = true;
                        pairedOrders.Add(_order.Id, _orderInfo);


                        orderSubtotal = orderSubtotal + (_customOrder == null ? _order.OrderSubtotalInclTax : (_customOrder.SubTotal == null ? 0 : Convert.ToDecimal(_customOrder.SubTotal)));
                        orderTotal += _order.OrderTotal;
                        validOrderIds = validOrderIds + orderId.ToString() + ",";
                    }
                }

            }
            if (customOrder != null)
            {
                orderSubtotal += customOrder.SubTotal == null ? 0 : (Convert.ToDecimal(customOrder.SubTotal));
            }
            else
            {
                orderSubtotal += order.OrderSubtotalInclTax;
            }

            foreach (var pairOrder in systemPairOrders)
            {
                if (!pairedOrders.Where(p => p.Key == pairOrder.orderID).Any())
                {
                    var _order = await _orderService.GetOrderByIdAsync(pairOrder.orderID);
                    if (_order != null)
                    {
                        var _customOrder = await _customOrderService.GetByOrderNumber(pairOrder.orderID);


                        dynamic _orderInfo = new ExpandoObject();
                        _orderInfo.SubTotal = await _priceFormatter.FormatPriceAsync(_customOrder == null ? _order.OrderSubtotalInclTax : (_customOrder.SubTotal == null ? 0 : Convert.ToDecimal(_customOrder.SubTotal)));
                        _orderInfo.OrderTotal = await _priceFormatter.FormatPriceAsync(_order.OrderTotal);
                        _orderInfo.IsPairOrder = true;
                        _orderInfo.IsFound = true;
                        _orderInfo.IsPaid = systemPairOrders.Where(s => s.orderID == _order.Id && s.serviceName == "WGS Service").Any();
                        _orderInfo.IsSelected = false;
                        pairedOrders.Add(_order.Id, _orderInfo);
                    }
                }
            }

            return (pairedOrders.OrderBy(o => o.Key).ThenBy(o => (bool)o.Value.IsPairOrder).ToDictionary(o => o.Key, o => o.Value), validOrderIds, orderSubtotal, orderTotal, IsSurchargeApplicable);
        }

        #endregion

        #region Utilities

        public async Task BindingAddress(AdditionalServiceModel model, Order order)
        {
            var customer = await _customerService.GetCustomerByIdAsync(order.CustomerId);
            if (customer != null)
            {
                if (order.ShippingAddressId != null && order.ShippingAddressId != 0)
                    model.ShippingAddress = await _customerModelFactory.PrepareCustomAddressModelAsync(customer, MWT.Nop.Core.Domain.Address.AddressType.IsShippingAddress, Convert.ToInt32(order.ShippingAddressId),
                        null, prePopulateNewAddressWithCustomerFields: true);
                model.ShippingAddress.IsShippingAddress = true;

                if (order.BillingAddressId != null && order.BillingAddressId != 0)
                    model.BillingAddress = await _customerModelFactory.PrepareCustomAddressModelAsync(customer, MWT.Nop.Core.Domain.Address.AddressType.IsBillingAddress, Convert.ToInt32(order.BillingAddressId),
                        null, prePopulateNewAddressWithCustomerFields: true);

                if (order.BillingAddressId == 0)
                {
                    model.BillingAddress = model.ShippingAddress;
                    model.BillingAddress.IsShippingAddress = true;
                }

            }
        }


        public async Task BindPaymentDetails(AdditionalServiceModel model, MWT.Nop.Core.Domain.CustomOrders.CustomOrder order)
        {
            var orderStatuses = await this._customOrderService.GetOrderStatuses();
            var liveOrder = await _orderService.GetOrderByIdAsync(Convert.ToInt32(order.LiveOrderNumber));
            if (liveOrder != null)
            {
                var paymentDetails = new PaymentDetails();
                paymentDetails.PaymentMethod = liveOrder.PaymentMethodSystemName;
                paymentDetails.PaymentDate = liveOrder.CreatedOnUtc;
                paymentDetails.TransactionId = string.IsNullOrEmpty(liveOrder.AuthorizationTransactionId) ?
                    liveOrder.CaptureTransactionId : liveOrder.AuthorizationTransactionId;
                paymentDetails.Card = String.IsNullOrEmpty(liveOrder.MaskedCreditCardNumber) ? "" : _encryptionService.DecryptText(liveOrder.MaskedCreditCardNumber);
                var paidStatusId = orderStatuses.Where(s => s.Name == MWT.Nop.Core.Domain.CustomOrders.OrderStatus.Paid.ToString()).FirstOrDefault()?.Id;
                if (paidStatusId != null)
                {
                    var logs = await _customOrderService.GetOrderStatusLogs(order.Id);
                    var paidStatusLog = logs.Where(l => l.StatusId == Convert.ToInt32(paidStatusId)).FirstOrDefault();
                    if (paidStatusLog != null && paidStatusLog.UserId != 0)
                        paymentDetails.PaidBy = (await _customerService.GetCustomerByIdAsync(paidStatusLog.UserId))?.Email;
                }
                paymentDetails.LiveOrderNumber = Convert.ToInt32(order.LiveOrderNumber);
                model.PaymentDetails = paymentDetails;
            }
        }


        public async Task<TaxTotalResult> GetOrderTax(Customer customer, decimal total, decimal wgs)
        {
            var activeTaxProvider = await _taxPluginManager.LoadPrimaryPluginAsync(customer, (await _storeContext.GetCurrentStoreAsync()).Id);
            if (activeTaxProvider == null)
                return null;
            var taxTotalRequest = new TaxTotalRequest
            {
                ShoppingCart = new List<ShoppingCartItem>(),
                Customer = customer,
                StoreId = (await _storeContext.GetCurrentStoreAsync()).Id,
                UsePaymentMethodAdditionalFee = false,
                IsCustomorder = true,
                Total = total,
                ShippingCharges = wgs
            };
            var taxTotalResult = await activeTaxProvider.GetTaxTotalAsync(taxTotalRequest);
            return taxTotalResult;
        }
        #endregion
    }
}