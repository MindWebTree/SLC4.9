using System.Linq;
using System.Threading.Tasks;
using Nop.Core.Caching;
using Nop.Core.Domain.Tax;
using Nop.Core.Events;
using MWT.Tax.FixedOrByCountryStateZip.Domain;
using MWT.Tax.FixedOrByCountryStateZip.Services;
using Nop.Services.Configuration;
using Nop.Services.Events;
using Nop.Core.Domain.Orders;
using Nop.Data;
using Nop.Services.Orders;
using Nop.Services.Common;
 
//using Nop.Core.Domain.Customization.PhoneOrder;
//using Nop.Services.Customizations.Phone_Order;
using Nop.Services.Customers;

namespace MWT.Tax.FixedOrByCountryStateZip.Infrastructure.Cache
{
    /// <summary>
    /// Model cache event consumer (used for caching of presentation layer models)
    /// </summary>
    public partial class ModelCacheEventConsumer :
        //tax rates
        IConsumer<EntityInsertedEvent<MWTTaxRate>>,
        IConsumer<EntityUpdatedEvent<MWTTaxRate>>,
        IConsumer<EntityDeletedEvent<MWTTaxRate>>,
        IConsumer<EntityInsertedEvent<Order>>
        //IConsumer<EntityInsertedEvent<CustomOrder>>,
        //IConsumer<EntityUpdatedEvent<CustomOrder>>

    {
        #region Constants

        /// <summary>
        /// Key for caching all tax rates
        /// </summary>
        public static CacheKey ALL_TAX_RATES_MODEL_KEY = new CacheKey("Nop.plugins.MWT.tax.fixedorbycountrystateziptaxrate.all");
        public static CacheKey TAXRATE_ALL_KEY = new CacheKey("Nop.plugins.MWT.tax.fixedorbycountrystateziptaxrate.taxrate.all");
        public static CacheKey  TAXRATE_ZAR_RESPONSE = new CacheKey("Nop.plugins.MWT.tax.fixedorbycountrystateziptaxrate.taxrate.{0}.{1}");
        public const string TAXRATE_PATTERN_KEY = "Nop.plugins.MWT.tax.fixedorbycountrystateziptaxrate.";

        #endregion

        #region Fields

        private readonly IMWTCountryStateZipService _taxRateService;
        private readonly ISettingService _settingService;
        private readonly IStaticCacheManager _staticCacheManager;
        private readonly IOrderService _orderService;
        private readonly IAddressService _addressService;
        private readonly ITaxLogService _transactionLogService;
        private readonly ICustomerService _customerService;
       // private readonly ICustomOrderService _customOrderService;
        #endregion

        #region Ctor

        public ModelCacheEventConsumer(IMWTCountryStateZipService taxRateService,
            ISettingService settingService,
            IStaticCacheManager staticCacheManager,
            IOrderService orderService,
            IAddressService addressService,
            ITaxLogService transactionLogService,
            ICustomerService customerService
            //,
          //  ICustomOrderService customOrderService
            )
        {
            _taxRateService = taxRateService;
            _settingService = settingService;
            _staticCacheManager = staticCacheManager;
            _orderService = orderService;
            _addressService = addressService;
            _transactionLogService = transactionLogService;
            _customerService = customerService;
       //     this._customOrderService = customOrderService;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Handle tax rate inserted event
        /// </summary>
        /// <param name="eventMessage">Event message</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task HandleEventAsync(EntityInsertedEvent<MWTTaxRate> eventMessage)
        {
            //clear cache
            await _staticCacheManager.RemoveByPrefixAsync(TAXRATE_PATTERN_KEY);
        }

        /// <summary>
        /// Handle tax rate updated event
        /// </summary>
        /// <param name="eventMessage">Event message</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task HandleEventAsync(EntityUpdatedEvent<MWTTaxRate> eventMessage)
        {
            //clear cache
            await _staticCacheManager.RemoveByPrefixAsync(TAXRATE_PATTERN_KEY);
        }

        /// <summary>
        /// Handle tax rate deleted event
        /// </summary>
        /// <param name="eventMessage">Event message</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task HandleEventAsync(EntityDeletedEvent<MWTTaxRate> eventMessage)
        {
            //clear cache
            await _staticCacheManager.RemoveByPrefixAsync(TAXRATE_PATTERN_KEY);
        }

        public async Task HandleEventAsync(EntityInsertedEvent<Order> eventMessage)
        {
            string zipCode = "";
            string city = "";
            int stateId = 0;
            string address1 = "";
            string address2 = "";
            CacheKey zarResponsecacheKey;
            if (eventMessage.Entity.ShippingAddressId != null)
            {
                var address= (await _addressService.GetAddressByIdAsync(Convert.ToInt32(eventMessage.Entity.ShippingAddressId)));
                zipCode = address?.ZipPostalCode;
                city = address?.City ?? "";
                stateId = address?.StateProvinceId ?? 0;
                address1 = address?.Address1??"";
                address2 = address?.Address2??"";
                if (!string.IsNullOrEmpty(zipCode))
                {
                    zarResponsecacheKey = _staticCacheManager.PrepareKeyForDefaultCache(ModelCacheEventConsumer.TAXRATE_ZAR_RESPONSE,
               eventMessage.Entity.CustomerId, zipCode + "-" + city + "-" + stateId + "-" + address1 + "-" + address2);
                    (bool isTaxZarRequestProcessed, bool isShippingChargable, decimal taxRate, string taxZarResponse,string taxRateInfo, int statusCode,
                    string url) =
                   await _staticCacheManager.GetAsync(zarResponsecacheKey, async () =>
                    {
                        decimal _taxRate = 0;
                        bool _isTaxZarRequestProcessed = false;
                        bool _isShippingChargable = false;
                        string _taxZarResponse = "";
                        string _taxRateInfo = "";
                        return (_isTaxZarRequestProcessed, _isShippingChargable, _taxRate, _taxZarResponse, _taxRateInfo, 0, "");
                    });
                    if (!string.IsNullOrEmpty(taxZarResponse))
                    {
                        await _transactionLogService.InsertLog(new MWTTaxZarTransactionLog()
                        {
                            CreatedDateUtc = DateTime.UtcNow,
                            CustomerId = eventMessage.Entity.CustomerId,
                            ResponseMessage = taxZarResponse,
                            StatusCode = statusCode,
                            RequestMessage = zipCode,
                            Url = url,
                            OrderId = eventMessage.Entity.Id,
                            TaxRateInfo= taxRateInfo
                        });
                    }

                }
            }
            zarResponsecacheKey = _staticCacheManager.PrepareKeyForDefaultCache(ModelCacheEventConsumer.TAXRATE_ZAR_RESPONSE,
             eventMessage.Entity.CustomerId, zipCode + "-" + city + "-" + stateId + "-" + address1 + "-" + address2);
            await _staticCacheManager.RemoveAsync(zarResponsecacheKey);

        }

        //public async Task HandleEventAsync(EntityInsertedEvent<CustomOrder> eventMessage)
        //{
        //    //await UpdateTaxRate(eventMessage.Entity);

        //}
        //public async Task HandleEventAsync(EntityUpdatedEvent<CustomOrder> eventMessage)
        //{
        //    //await UpdateTaxRate(eventMessage.Entity);

        //}

        #endregion

   
    }
}