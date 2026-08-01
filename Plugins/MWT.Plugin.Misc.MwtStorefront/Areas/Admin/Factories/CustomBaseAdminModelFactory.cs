using Microsoft.AspNetCore.Mvc.Rendering;
using MWT.Nop.Core.Services.KW;
using MWT.Nop.Core.Services.QA;
using Nop.Core.Caching;
using Nop.Core.Domain.Translation;
using Nop.Core.Infrastructure;
using Nop.Services.Catalog;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.Helpers;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Messages;
using Nop.Services.Plugins;
using Nop.Services.Shipping;
using Nop.Services.Shipping.Date;
using Nop.Services.Stores;
using Nop.Services.Tax;
using Nop.Services.Topics;
using Nop.Services.Vendors;
using Nop.Web.Areas.Admin.Factories;

namespace MWT.Plugin.Misc.MwtStorefront.Area.Admin.Factories
{
    /// <summary>
    /// Represents the implementation of the base model factory that implements a most common admin model factories methods
    /// </summary>
    public partial class CustomBaseAdminModelFactory : BaseAdminModelFactory, ICustomBaseAdminModelFactory
    {
        public CustomBaseAdminModelFactory(ICategoryService categoryService, ICategoryTemplateService categoryTemplateService, ICountryService countryService, ICurrencyService currencyService, ICustomerActivityService customerActivityService, ICustomerService customerService, IDateRangeService dateRangeService, IDateTimeHelper dateTimeHelper, IEmailAccountService emailAccountService, ILanguageService languageService, ILocalizationService localizationService, IManufacturerService manufacturerService, IManufacturerTemplateService manufacturerTemplateService, INewsLetterSubscriptionTypeService newsLetterSubscriptionTypeService, IPluginService pluginService, IProductTemplateService productTemplateService, ISpecificationAttributeService specificationAttributeService, IStateProvinceService stateProvinceService, IStaticCacheManager staticCacheManager, IStoreService storeService, ITaxCategoryService taxCategoryService, ITopicTemplateService topicTemplateService, IVendorService vendorService, IWarehouseService warehouseService, TranslationSettings translationSettings) : base(categoryService, categoryTemplateService, countryService, currencyService, customerActivityService, customerService, dateRangeService, dateTimeHelper, emailAccountService, languageService, localizationService, manufacturerService, manufacturerTemplateService, newsLetterSubscriptionTypeService, pluginService, productTemplateService, specificationAttributeService, stateProvinceService, staticCacheManager, storeService, taxCategoryService, topicTemplateService, vendorService, warehouseService, translationSettings)
        {
        }

        #region Methods

        public virtual async Task PrepareKWTemplatesAsync(IList<SelectListItem> items, bool withSpecialDefaultItem = true, string defaultItemText = null)
        {
            if (items == null)
                throw new ArgumentNullException(nameof(items));

            //prepare available category templates
            var _kwTemplatetService = EngineContext.Current.Resolve<IKwTemplateService>();
            var availableTemplates = await _kwTemplatetService.GetAllKwTemplatesAsync();
            foreach (var template in availableTemplates)
            {
                items.Add(new SelectListItem { Value = template.Id.ToString(), Text = template.Name });
            }

            //insert special item for the default value
            await PrepareDefaultItemAsync(items, withSpecialDefaultItem, defaultItemText);
        }

        public virtual async Task PrepareQuestionAnswerTemplatesAsync(IList<SelectListItem> items, bool withSpecialDefaultItem = true, string defaultItemText = null)
        {
            if (items == null)
                throw new ArgumentNullException(nameof(items));

            //prepare available category templates
            var _questionAnswerTemplatetService = EngineContext.Current.Resolve<IQuestionAnswerTemplateService>();
            var availableTemplates = await _questionAnswerTemplatetService.GetAllQuestionAnswerTemplatesAsync();
            foreach (var template in availableTemplates)
            {
                items.Add(new SelectListItem { Value = template.Id.ToString(), Text = template.Name });
            }

            //insert special item for the default value
            await PrepareDefaultItemAsync(items, withSpecialDefaultItem, defaultItemText);
        }
        #endregion
    }
}
