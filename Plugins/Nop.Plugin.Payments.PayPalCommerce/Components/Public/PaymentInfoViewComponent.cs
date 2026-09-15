using DocumentFormat.OpenXml.EMMA;
using Microsoft.AspNetCore.Mvc;
using MWT.Nop.Core.Domain.CustomOrders;
using MWT.Nop.Core.Services.Customizations.CustomOrders;
using Nop.Plugin.Payments.PayPalCommerce.Domain;
using Nop.Plugin.Payments.PayPalCommerce.Factories;
using Nop.Plugin.Payments.PayPalCommerce.Models.Public;
using Nop.Plugin.Payments.PayPalCommerce.Services;
using Nop.Services.Catalog;
using Nop.Web.Framework.Components;

namespace Nop.Plugin.Payments.PayPalCommerce.Components.Public;

/// <summary>
/// Represents the view component to display payment info in the public store
/// </summary>
public class PaymentInfoViewComponent : NopViewComponent
{

    #region Fields

    private readonly IProductService _productService;
    private readonly PayPalCommerceModelFactory _modelFactory;
    private readonly PayPalCommerceServiceManager _serviceManager;
    private readonly PayPalCommerceSettings _settings;
    private readonly ICustomOrderService _customOrderService;

    #endregion

    #region Ctor

    public PaymentInfoViewComponent(IProductService productService,
        PayPalCommerceModelFactory modelFactory,
        PayPalCommerceServiceManager serviceManager,
        PayPalCommerceSettings settings,
        ICustomOrderService customOrderService)
    {
        _productService = productService;
        _modelFactory = modelFactory;
        _serviceManager = serviceManager;
        _settings = settings;
        _customOrderService = customOrderService;
    }

    #endregion

    #region Methods


    public async Task<IViewComponentResult> InvokeAsync(string widgetZone, dynamic additionalData)
    {
        int invoiceId = 0;
        if (additionalData != null)
            int.TryParse(Convert.ToString(additionalData.Id), out invoiceId);
        var (active, _) = await _serviceManager.IsActiveAsync(_settings);
        if (!active)
            return Content(string.Empty);
        int CustomerId = 0;
        CustomOrder customOrder = null;
        if (invoiceId > 0)
        {
            customOrder = await _customOrderService.GetById(invoiceId);
            int.TryParse(Convert.ToString(customOrder.CustomerId), out CustomerId);

        }
        PaymentInfoModel model = await _modelFactory.PreparePaymentInfoModelAsync(ButtonPlacement.PaymentMethod, null, customOrder);
        model.InvoiceId = invoiceId;
        model.CustomerId = CustomerId;
        return View("~/Plugins/Payments.PayPalCommerce/Views/Public/PaymentInfo.cshtml", model);
    }

    #endregion
}