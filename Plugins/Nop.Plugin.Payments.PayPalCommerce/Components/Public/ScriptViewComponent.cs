using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewComponents;
using Microsoft.AspNetCore.Routing;
using Nop.Core;
using Nop.Core.Domain.Orders;
using Nop.Core.Http;
using Nop.Plugin.Payments.PayPalCommerce.Domain;
using Nop.Plugin.Payments.PayPalCommerce.Factories;
using Nop.Plugin.Payments.PayPalCommerce.Models.Public;
using Nop.Plugin.Payments.PayPalCommerce.Services;
using Nop.Services.Catalog;
using Nop.Services.Payments;
using Nop.Web.Framework.Components;
using Nop.Web.Framework.Infrastructure;
using Nop.Web.Framework.Mvc.Routing;
using Nop.Web.Models.Catalog;

namespace Nop.Plugin.Payments.PayPalCommerce.Components.Public;
public class ScriptViewComponent : NopViewComponent
{
    #region Fields
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IPaymentPluginManager _paymentPluginManager;
    private readonly IStoreContext _storeContext;
    private readonly IWorkContext _workContext;
    private readonly PayPalCommerceSettings _settings;
    private readonly PayPalCommerceServiceManager _serviceManager;
    private readonly OrderSettings _orderSettings;
    private readonly INopUrlHelper _nopUrlHelper;

    #endregion

    #region Ctor

    public ScriptViewComponent(IPaymentPluginManager paymentPluginManager,
        IStoreContext storeContext,
        IWorkContext workContext,
        PayPalCommerceSettings settings,
        PayPalCommerceServiceManager serviceManager,
        IHttpContextAccessor httpContextAccessor,
        OrderSettings orderSettings,
        INopUrlHelper nopUrlHelper)
    {
        _paymentPluginManager = paymentPluginManager;
        _storeContext = storeContext;
        _workContext = workContext;
        _settings = settings;
        _serviceManager = serviceManager;
        _httpContextAccessor = httpContextAccessor;
        _orderSettings = orderSettings;
        _nopUrlHelper = nopUrlHelper;
    }

    #endregion

    #region Methods

    /// <summary>
    /// Invoke view component
    /// </summary>
    /// <param name="widgetZone">Widget zone name</param>
    /// <param name="additionalData">Additional data</param>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains the view component result
    /// </returns>
    public async Task<IViewComponentResult> InvokeAsync(string widgetZone, object additionalData)
    {


        var customer = await _workContext.GetCurrentCustomerAsync();
        var store = await _storeContext.GetCurrentStoreAsync();
        if (!await _paymentPluginManager.IsPluginActiveAsync(PayPalCommerceDefaults.SystemName, customer, store?.Id ?? 0))
            return Content(string.Empty);
        var (active, _) = await _serviceManager.IsActiveAsync(_settings);
        if (!active)
            return Content(string.Empty);

        if (!widgetZone.Equals(PublicWidgetZones.CheckoutPaymentInfoTop) && !widgetZone.Equals(PublicWidgetZones.OpcContentBefore)
            && !widgetZone.Equals(PublicWidgetZones.OrderSummaryContentBefore))
        {
            return Content(string.Empty);
        }

        int invoiceId = 0;
        var path = _httpContextAccessor.HttpContext?.Request.Path.Value;
        var isOnePageCheckout =
            path?.Equals(_orderSettings.OnePageCheckoutEnabled ? "/onepagecheckout" : "/checkout", StringComparison.OrdinalIgnoreCase) == true;
        if (!isOnePageCheckout)
        {
            isOnePageCheckout = path?.Equals("/checkoutCustomOrder", StringComparison.OrdinalIgnoreCase) == true;
            if (isOnePageCheckout)
                int.TryParse(_httpContextAccessor.HttpContext?.Request.Query["orderid"].ToString(), out invoiceId);
        }
        if (!isOnePageCheckout)
        {
            return Content(string.Empty);
        }



        var ((script, clientToken, userToken), _) = await _serviceManager.PreparePaymentScriptsAsync(_settings, ButtonPlacement.PaymentMethod, null, invoiceId);


        var contentBuilder = new HtmlContentBuilder();

        // existing script

        contentBuilder.AppendHtml(new HtmlString($"<script src=\"{script}\" data-page-type=\"checkout\" data-client-token=\"{clientToken}\" data-user-id-token=\"{userToken}\" data-partner-attribution-id=\"{PayPalCommerceDefaults.PartnerHeader.Value}\"></script>"));
        if (_settings.UseApplePay)
        {
            contentBuilder.AppendHtml(new HtmlString($"<script src=\"{PayPalCommerceDefaults.ApplePayScriptUrl}\"></script>"));
        }
        if (_settings.UseGooglePay)
        {
            contentBuilder.AppendHtml(new HtmlString($"<script  src=\"{PayPalCommerceDefaults.GooglePayScriptUrl}\"></script>"));
        }
        if (_settings.UseSandbox || _settings.ConfiguratorSupported)
        {
            contentBuilder.AppendHtml(new HtmlString($"<script  src=\"{PayPalCommerceDefaults.MerchantConfiguratorScriptUrl}\"></script>"));
        }
        return new HtmlContentViewComponentResult(contentBuilder);
    }

    #endregion
}
