using Microsoft.AspNetCore.Mvc;
using MWT.Plugin.Shipping.FixedByWeightByTotal.Models;
using Nop.Core;
using Nop.Services.Customers;
using Nop.Web.Framework.Components;
using Nop.Web.Framework.Models;

namespace MWT.Plugin.Shipping.FixedByWeightByTotal.Components
{

    [ViewComponent(Name = "WidgetsProductExpectedDeliverydate")]
    public class WidgetsProductExpectedDeliverydateViewComponent : NopViewComponent
    {
        private readonly IWorkContext _workContext;
        private readonly ICustomerService _customService;
        public WidgetsProductExpectedDeliverydateViewComponent(IWorkContext workContext,
                   ICustomerService customService)
        {
            this._workContext = workContext;
            this._customService = customService;
        }

        public async Task<IViewComponentResult> InvokeAsync(string widgetZone, object additionalData)
        {
            int id = ((BaseNopEntityModel)additionalData).Id;
            if (id == 0)
                return Content("");
            else
            {
                EstimatedShippingDateRequestModel model = new EstimatedShippingDateRequestModel();
                model.ProductId = id;
                var customer = await _workContext.GetCurrentCustomerAsync();
                if (customer != null)
                {
                    var address = await this._customService.GetCustomerShippingAddressAsync(customer);
                    if (address != null && !String.IsNullOrEmpty(address.ZipPostalCode))
                        model.Zipcode = address.ZipPostalCode;
                }
                return View("~/Plugins/MWT.Shipping.FixedByWeightByTotal/Views/PublicInfo.cshtml", model);
            }

        }
    }

}