using LinqToDB.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using MWT.Nop.Core.Domain.AbandonedCarts;
using MWT.Nop.Core.Service.Catalog;
using MWT.Nop.Core.Services.Customers;
using MWT.Nop.Core.Services.Message;
using MWT.Nop.Core.Services.Orders;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Logging;
using Nop.Core.Domain.Orders;
using Nop.Data;
using Nop.Services.Catalog;
using Nop.Services.Directory;
using Nop.Services.Logging;
using Nop.Services.Orders;
using Nop.Services.Stores;
using System.Net;


namespace MWT.Nop.Core.Services.AbandonedCarts
{
    public class AbandonedCartService : IAbandonedCartService
    {
        private readonly IRepository<AbandonedCartReminderSchedule> _abandonedCartReminderScheduleRepository;
        private readonly IRepository<AbandonedReminder> _abandonedReminderRepository;
        private readonly IRepository<AbandonedCartItem> _abandonedCartItemRepository;
        private readonly IRepository<AbandonedReminderHistory> _abandonedReminderHistoryRepository;
        private readonly IRepository<AbandonedCart> _abandonedCartRepository;
        private readonly IRepository<AbandonedShoppingCart> _abandonedShoppingCartRepository;
        private readonly ICustomerExtendedService _customerService;
        private readonly ILogger _loggerService;
        private readonly IShoppingCartService _shoppingCartService;
        private readonly IStoreContext _storeContext;
        private readonly IOrderTotalCalculationExtendedService _orderTotalCalculationService;
        private readonly ICurrencyService _currencyService;
        private readonly IWorkContext _workContext;
        protected readonly IStaticCacheManager _staticCacheManager;
        private readonly IProductExtendedService _productService;
        private readonly ICustomWorkflowMessageService _workflowMessageService;
        private readonly IStoreService _storeService;
        private readonly IUrlHelperFactory _urlHelperFactory;
        private readonly IActionContextAccessor _actionContextAccessor;
        public AbandonedCartService(IRepository<AbandonedCartItem> abandonedCartItemRepository,
            IRepository<AbandonedShoppingCart> abandonedShoppingCartRepository,
             IRepository<AbandonedCart> abandonedCartRepository,
              ICustomerExtendedService customerService,
              ILogger loggerService, IShoppingCartService shoppingCartService,
              IStoreContext storeContext, IOrderTotalCalculationExtendedService orderTotalCalculationService,
              ICurrencyService currencyService, IWorkContext workContext,
               IRepository<AbandonedReminderHistory> abandonedReminderHistoryRepository,
               IRepository<AbandonedCartReminderSchedule> abandonedCartReminderScheduleRepository,
               IStaticCacheManager staticCacheManage,
               IRepository<AbandonedReminder> abandonedReminderRepository,
               IProductExtendedService productService,
               ICustomWorkflowMessageService workflowMessageService,
               IStoreService storeService,
               IUrlHelperFactory urlHelperFactory,
               IActionContextAccessor actionContextAccessor)
        {
            _abandonedCartItemRepository = abandonedCartItemRepository;
            _abandonedShoppingCartRepository = abandonedShoppingCartRepository;
            _abandonedCartRepository = abandonedCartRepository;
            _customerService = customerService;
            _loggerService = loggerService;
            _shoppingCartService = shoppingCartService;
            _storeContext = storeContext;
            _orderTotalCalculationService = orderTotalCalculationService;
            _currencyService = currencyService;
            _workContext = workContext;
            _abandonedReminderHistoryRepository = abandonedReminderHistoryRepository;
            _abandonedCartReminderScheduleRepository = abandonedCartReminderScheduleRepository;
            _staticCacheManager = staticCacheManage;
            _abandonedReminderRepository = abandonedReminderRepository;
            _productService = productService;
            _workflowMessageService = workflowMessageService;
            _storeService = storeService;
            _urlHelperFactory = urlHelperFactory;
            _actionContextAccessor = actionContextAccessor;
        }

        public async Task<IList<AbandonedCartItem>> SyncSalesForceAbandonedCartItems(DateTime StartDate)
        {

            var abandonedItems = await _abandonedCartItemRepository.EntityFromSqlAsync("V2GetAbandonedItems", new DataParameter[] { new DataParameter()
                       {
                           DataType = LinqToDB.DataType.VarChar,
                           Value = StartDate,
                           Direction = System.Data.ParameterDirection.Input,
                           Name = "@StartDate",

                       } });

            return abandonedItems;

        }
        public async Task<IList<AbandonedCartItem>> SyncAbandonedCartItems()
        {

            var abandonedItems = await _abandonedCartItemRepository.EntityFromSqlAsync("GetAbandonedItems", new DataParameter[] { });

            return abandonedItems;

        }
        public async Task DeleteItem(int shoppingCartRecid)
        {
            try
            {
                var abandonedItem = await _abandonedShoppingCartRepository.Table.Where(item => item.ShoppingCartRecID == shoppingCartRecid).FirstOrDefaultAsync();

                if (abandonedItem != null)
                {
                    await _abandonedShoppingCartRepository.DeleteAsync(abandonedItem);
                }
            }
            catch (Exception exp)
            {
                await _loggerService.InsertLogAsync(LogLevel.Error, "SalesForce: Failed to DeleteItem", exp.Message);
            }
        }


        public async Task MarkItemAsOld(int shoppingCartRecid)
        {
            var abandonedItem = await _abandonedShoppingCartRepository.Table.Where(item => item.ShoppingCartRecID == shoppingCartRecid).FirstOrDefaultAsync();
            if (abandonedItem != null)
            {
                abandonedItem.IsNew = false;
                await _abandonedShoppingCartRepository.UpdateAsync(abandonedItem);
            }
        }

        public async Task<AbandonedCart> GetAbandonedInvoiceByGuid(Guid guid)
        {
            return await _abandonedCartRepository.Table.Where(a => a.Guid == guid).FirstOrDefaultAsync();
        }

        public async Task MarkAbandonedInvoiceAsPaid(int customerId, int[] shoppingCartRecIds, int ordernumber, decimal total)
        {
            try
            {
                var invoice = await _abandonedCartRepository.Table.Where(a => a.CustomerId == customerId
                  && (a.OrderNumber == null || a.OrderNumber == 0)).FirstOrDefaultAsync();
                if (invoice != null)
                {
                    var customer = await this._customerService.GetCustomerByIdAsync(customerId);
                    var items = await this._abandonedShoppingCartRepository.Table.Where(a => a.CartInvoiceID == invoice.Id).ToListAsync();
                    foreach (var item in items)
                    {
                        if (!shoppingCartRecIds.Contains(item.ShoppingCartRecID))
                        {
                            await this._abandonedShoppingCartRepository.DeleteAsync(item);
                        }
                    }
                    invoice.OrderNumber = ordernumber;
                    await this._abandonedCartRepository.UpdateAsync(invoice);
                }
              
            }
            catch (Exception exp)
            {
                await _loggerService.InsertLogAsync(LogLevel.Error, "SalesForce: Failed to mark order as Paid", exp.Message + exp.InnerException?.Message ?? "");
            }
        }

        
        public async Task<List<AbandonedCart>> AbandonedCarts()
        {
            var abandCarts = await ((from abandCart in _abandonedCartRepository.Table
                                     join abandCartItem in _abandonedShoppingCartRepository.Table
                                     on abandCart.Id equals abandCartItem.CartInvoiceID
                                     select abandCart).OrderByDescending(o => o.CreatedOn)).Take(100).ToListAsync();
            return abandCarts;
        }

        public async Task AbandonedCardForCustomerOnPaymentFail(Customer customer)
        {
            try
            {
                var items = await _abandonedCartItemRepository.EntityFromSqlAsync("Sp_AbandonedCard_For_Customer", new DataParameter[] { new DataParameter()
                       {
                           DataType = LinqToDB.DataType.VarChar,
                           Value = customer.Id,
                           Direction = System.Data.ParameterDirection.Input,
                           Name = "@CustomerId",

                       } });

                foreach (var customerid in items.Select(a => a.CustomerID).Distinct())
                {

                    string email = await this._customerService.GetCustomerEmailAsync(customer);
                    if (customer != null && !string.IsNullOrEmpty(email))
                    {
                        var item = items.Where(i => i.CustomerID == customerid).FirstOrDefault();
                        var cart = await _shoppingCartService.GetShoppingCartAsync(customer, ShoppingCartType.ShoppingCart, (await _storeContext.GetCurrentStoreAsync()).Id);
                        if (cart.Count > 0)
                        {
                            var (orderSubTotalDiscountAmountBase, appliedDiscounts, subTotalWithoutDiscountBase, _, _, discountAmountsApplied) =
                await _orderTotalCalculationService.GetCustomShoppingCartTotalAsync(cart, true);
                            if (orderSubTotalDiscountAmountBase.HasValue)
                            {
                                orderSubTotalDiscountAmountBase = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(orderSubTotalDiscountAmountBase.Value, await _workContext.GetWorkingCurrencyAsync());

                            }

                            
                            
                        }
                    }
                }
            }
            catch (Exception exp)
            {
                await _loggerService.InsertLogAsync(LogLevel.Error, "SalesForce: Failed to sync pay now event", exp.Message + exp.InnerException?.Message ?? "");
            }
        }

        #region V3 Version

        public async Task<IList<AbandonedCartReminderSchedule>> GetAbandonedCartReminderSchedules()
        {
            var query = from ar in _abandonedCartReminderScheduleRepository.Table
                        where ar.Published
                        orderby ar.Number
                        select ar;
            var schedules = await _staticCacheManager.GetAsync(_staticCacheManager.PrepareKeyForDefaultCache(CustomNopCatalogDefaults.AbandonedCartReminderCacheKey), async () => await query.ToListAsync());
            return schedules;
        }

        public async Task<IList<AbandonedReminder>> GetAbandonedReminders()
        {
            return await _abandonedReminderRepository.EntityFromSqlAsync("Sp_GetAbandonedReminders");
        }
        public async Task SyncAbandonedCarts(DateTime StartDate)
        {
            await _abandonedCartItemRepository.EntityFromSqlAsync("SP_V3SyncAbandonedCarts", new DataParameter[] { new DataParameter()
                       {
                           DataType = LinqToDB.DataType.VarChar,
                           Value = StartDate,
                           Direction = System.Data.ParameterDirection.Input,
                           Name = "@StartDate",
           } });
        }

        public async Task InsertAbandonedReminderHistory(AbandonedReminderHistory reminder, bool trackReminder = true)
        {
            if (trackReminder)
            {
                await this._abandonedReminderHistoryRepository.InsertAsync(reminder);
            }
            if (reminder.ReminderNumber >= (await GetAbandonedCartReminderSchedules()).Max(ar => ar.Number))
            {
                var abandonedcart = await _abandonedCartRepository.GetByIdAsync(reminder.AbandonedCartInvoiceID);
                if (abandonedcart != null)
                {
                    abandonedcart.IsCompleted = true;
                    await _abandonedCartRepository.UpdateAsync(abandonedcart);
                }
            }
        }

        public async Task SendAbandonedCartReminder(AbandonedReminder reminder, Customer customer, string name, string email, string phone, int messageTemplateId, IList<ShoppingCartItem> cart, string utmSource)
        {
            int counter = 0;
            List<Product> similarProducts = new List<Product>();
            foreach (var item in cart.Skip(1))
            {

                if (similarProducts.Count < 3)
                {
                    var product = await _productService.GetProductByIdAsync(item.ProductId);
                    if (product != null)
                    {
                        similarProducts.Add(product);
                        counter++;
                    }
                }
            }
            if (counter < 3)
            {
                var relatedItems = await _productService.GetCustomRelatedProductsByProductId1Async(cart[0].ProductId, 5, false);
                foreach (var relatedItem in relatedItems)
                {
                    counter++;
                    if (similarProducts.Count < 3)
                    {
                        var product = await _productService.GetProductByIdAsync(relatedItem);
                        if (product != null)
                        {
                            similarProducts.Add(product);
                        }
                    }
                }
            }

            var response = await _workflowMessageService.SendAbandonedCartReminderNotificationAsync(customer, name, email, messageTemplateId, await RouteUrlAsync(routeName: "RebuildCart",
                       routeValues: new { invoiceId = reminder.Guid }), await _productService.GetProductByIdAsync(cart[0].ProductId), similarProducts, (await _workContext.GetWorkingCurrencyAsync())?.Id ?? 0, utmSource);

            if (response)
            {
                await InsertAbandonedReminderHistory(new AbandonedReminderHistory()
                {
                    AbandonedCartInvoiceID = reminder.Id,
                    Isdeleted = false,
                    ReminderDate = DateTime.Now,
                    ReminderNumber = reminder.ReminderNumber
                }, true);

            }

        }

        #endregion

        #region Utilities

        protected virtual async Task<string> RouteUrlAsync(int storeId = 0, string routeName = null, object routeValues = null)
        {
            //try to get a store by the passed identifier
            var store = await _storeService.GetStoreByIdAsync(storeId) ?? await _storeContext.GetCurrentStoreAsync()
                ?? throw new Exception("No store could be loaded");

            //ensure that the store URL is specified
            if (string.IsNullOrEmpty(store.Url))
                throw new Exception("URL cannot be null");

            //generate a URL with an absolute path
            //var urlHelper = _urlHelperFactory.GetUrlHelper(_actionContextAccessor.ActionContext);
            //var url = new PathString(urlHelper.RouteUrl(routeName, routeValues));
            var urlHelper = _urlHelperFactory.GetUrlHelper(_actionContextAccessor.ActionContext);
            var url = new PathString(urlHelper.RouteUrl(routeName, routeValues));
            //remove the application path from the generated URL if exists
            var pathBase = _actionContextAccessor.ActionContext?.HttpContext?.Request?.PathBase ?? PathString.Empty;
            url.StartsWithSegments(pathBase, out url);

            //compose the result
            return Uri.EscapeDataString(WebUtility.UrlDecode($"{store.Url.TrimEnd('/')}{url}"));
        }
        #endregion

    }
}
