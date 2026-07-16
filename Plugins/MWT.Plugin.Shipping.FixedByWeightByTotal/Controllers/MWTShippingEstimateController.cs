using Microsoft.AspNetCore.Mvc;
using MWT.Plugin.Shipping.FixedByWeightByTotal.Services;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Web.Framework.Controllers;

namespace MWT.Plugin.Shipping.FixedByWeightByTotal.Controllers
{

    public class MWTShippingEstimateController : BaseController
    {
        #region Fields

        private readonly IMWTExpectedDeliveryDateService _mwtExpectedDeliveryDateService;
        private readonly ILocalizationService _localizationService;
        private readonly ISettingService _settingService;

        #endregion

        #region Ctor

        public MWTShippingEstimateController(IMWTExpectedDeliveryDateService mwtExpectedDeliveryDateService,
                                             ILocalizationService localizationService,
                                             ISettingService settingService)
        {
            this._mwtExpectedDeliveryDateService = mwtExpectedDeliveryDateService;
            this._localizationService = localizationService;
            this._settingService = settingService;

        }

        #endregion

        #region Methods

        [HttpPost]
        [IgnoreAntiforgeryToken]
        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task<IActionResult> GetEstimateDeliverydate(int productId, string postalCode)
        {
            return Content(await _mwtExpectedDeliveryDateService.GetShippingEstimateDate(productId, postalCode));
        }


        #endregion
    }
}
