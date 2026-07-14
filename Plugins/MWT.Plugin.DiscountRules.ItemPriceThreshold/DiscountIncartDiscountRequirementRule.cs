
using System.Globalization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Nop.Core;
using Nop.Core.Domain.Orders;
using Nop.Services.Catalog;
using Nop.Services.Configuration;
using Nop.Services.Directory;
using Nop.Services.Discounts;
using Nop.Services.Localization;
using Nop.Services.Orders;
using Nop.Services.Plugins;
using Nop.Services.Tax;

namespace Nop.Plugin.DiscountRules.ItemsBelowPrice
{
    public partial class ItemsBelowPriceDiscountRequirementRule : BasePlugin, IDiscountRequirementRule
    {
        private readonly IActionContextAccessor _actionContextAccessor;
        private readonly IDiscountService _discountService;
        private readonly ILocalizationService _localizationService;
        private readonly ISettingService _settingService;
        private readonly IUrlHelperFactory _urlHelperFactory;
        private readonly IWebHelper _webHelper;
        private readonly IShoppingCartService _shoppingCartService;
        private readonly IWorkContext _workContext;
        private readonly IStoreContext _storeContext;
        private readonly IProductService _productService;
        private readonly ITaxService _taxService;
        private readonly ICurrencyService _currencyService;
        public ItemsBelowPriceDiscountRequirementRule(IActionContextAccessor actionContextAccessor,

            IDiscountService discountService,
            ILocalizationService localizationService,
            ISettingService settingService,
            IUrlHelperFactory urlHelperFactory,
            IWebHelper webHelper,
            IShoppingCartService shoppingCartService,
            IWorkContext workContext,
            IStoreContext storeContext,
            IProductService productService,
            ITaxService taxService,
            ICurrencyService currencyService)
        {
            _actionContextAccessor = actionContextAccessor;
            _discountService = discountService;
            _localizationService = localizationService;
            _settingService = settingService;
            _urlHelperFactory = urlHelperFactory;
            _webHelper = webHelper;
            _shoppingCartService = shoppingCartService;
            _workContext = workContext;
            _storeContext = storeContext;
            _productService = productService;
            _taxService = taxService;
            _currencyService = currencyService;
        }

        /// <summary>
        /// Check discount requirement
        /// </summary>
        /// <param name="request">Object that contains all information required to check the requirement (Current customer, discount, etc)</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the result
        /// </returns>
        public async Task<DiscountRequirementValidationResult> CheckRequirementAsync(DiscountRequirementValidationRequest request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            //invalid by default
            var result = new DiscountRequirementValidationResult();
            var store = await _storeContext.GetCurrentStoreAsync();
            var cartItems = await _shoppingCartService.GetShoppingCartAsync(await _workContext.GetCurrentCustomerAsync(), ShoppingCartType.ShoppingCart, store.Id);

            if (cartItems.Count == 0)
            {
                return result;
            }

            var cartAmountRequirement = await _settingService.GetSettingByKeyAsync<string>($"DiscountRequirement.ItemsBelowPrice-{request.DiscountRequirementId}");

            decimal ItemsBelowPriceThreashold = 0;

            if (cartAmountRequirement.Contains('-'))
            {
                string[] splt = cartAmountRequirement.Split(new string[] { "-" }, StringSplitOptions.RemoveEmptyEntries);


                if (splt.Length == 1)
                {
                    decimal.TryParse(splt[0], NumberStyles.Currency, CultureInfo.CurrentCulture.NumberFormat, out ItemsBelowPriceThreashold);
                }
                else
                {
                    //unvalid
                    result.UserError = await _localizationService.GetResourceAsync("Plugins.DiscountRules.ItemsBelowPrice.Fields.Amount.Required");
                    return result;
                }
            }
            else
            {
                decimal.TryParse(cartAmountRequirement, NumberStyles.Currency, CultureInfo.CurrentCulture.NumberFormat, out ItemsBelowPriceThreashold);
            }

            if (ItemsBelowPriceThreashold == decimal.Zero)
            {
                //valid
                result.IsValid = true;
                return result;
            }

            result.IsValid = true;
            foreach (var sci in cartItems)
            {
                var product = await _productService.GetProductByIdAsync(sci.ProductId);
                var (shoppingCartUnitPriceWithDiscountBase, _) = await _taxService.GetProductPriceAsync(product, (await _shoppingCartService.GetUnitPriceAsync(sci, true)).unitPrice);
                var shoppingCartUnitPriceWithDiscount = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(shoppingCartUnitPriceWithDiscountBase, await _workContext.GetWorkingCurrencyAsync());
                if (shoppingCartUnitPriceWithDiscount > ItemsBelowPriceThreashold)
                {
                    result.IsValid = false;
                    result.UserError = await _localizationService.GetResourceAsync("Plugins.DiscountRules.ItemsBelowPrice.NotEnough");
                    break;
                }
            }

            return result;
        }

        /// <summary>
        /// Get URL for rule configuration
        /// </summary>
        /// <param name="discountId">Discount identifier</param>
        /// <param name="discountRequirementId">Discount requirement identifier (if editing)</param>
        /// <returns>URL</returns>
        public string GetConfigurationUrl(int discountId, int? discountRequirementId)
        {
            var urlHelper = _urlHelperFactory.GetUrlHelper(_actionContextAccessor.ActionContext);

            return urlHelper.Action("Configure", "ItemsBelowPrice",
                new { discountId = discountId, discountRequirementId = discountRequirementId }, _webHelper.GetCurrentRequestProtocol());
        }

        /// <summary>
        /// Install the plugin
        /// </summary>
        /// <returns>A task that represents the asynchronous operation</returns>
        public override async Task InstallAsync()
        {
            //locales
            await _localizationService.AddOrUpdateLocaleResourceAsync(new Dictionary<string, string>
            {
                ["Plugins.DiscountRules.ItemsBelowPrice.Fields.Amount"] = "Maximum allowed price per cart item",
                ["Plugins.DiscountRules.ItemsBelowPrice.Fields.Amount.Hint"] = "The discount applies only if all items in the cart are priced below this amount.",
                ["Plugins.DiscountRules.ItemsBelowPrice.NotEnough"] = "Sorry, the discount cannot be applied because one or more items exceed the allowed price.",
                ["Plugins.DiscountRules.ItemsBelowPrice.Fields.Amount.Required"] = "Maximum item price cannot be empty",
                ["Plugins.DiscountRules.ItemsBelowPrice.Fields.DiscountId.Required"] = "Discount ID is required"
            });

            await base.InstallAsync();
        }

        /// <summary>
        /// Uninstall the plugin
        /// </summary>
        /// <returns>A task that represents the asynchronous operation</returns>
        public override async Task UninstallAsync()
        {
            //discount requirements
            var discountRequirements = (await _discountService.GetAllDiscountRequirementsAsync())
                .Where(discountRequirement => discountRequirement.DiscountRequirementRuleSystemName == DiscountRequirementDefaults.SYSTEM_NAME);
            foreach (var discountRequirement in discountRequirements)
            {
                await _discountService.DeleteDiscountRequirementAsync(discountRequirement, false);
            }

            //locales
            await _localizationService.DeleteLocaleResourcesAsync("Plugins.DiscountRules.ItemsBelowPrice");

            await base.UninstallAsync();
        }
    }
}