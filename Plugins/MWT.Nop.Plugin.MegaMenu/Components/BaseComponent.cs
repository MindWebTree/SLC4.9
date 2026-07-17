using Microsoft.Extensions.DependencyInjection;
using Nop.Core.Caching;
using Nop.Core.Infrastructure;
using Nop.Services.Customers;
using Nop.Services.Localization;
using Nop.Services.Seo;
using Nop.Web.Framework.Components;

namespace MWT.Nop.Plugin.MegaMenu.Components
{
    public abstract class BaseComponent : NopViewComponent
    {
        private ILocalizationService _localizationService;
        private IUrlRecordService _urlRecordService;
        private ICustomerService _customerService;
        private IStaticCacheManager _staticCacheManager;

        protected ILocalizationService LocalizationService
        {
            get
            {
                if (this._localizationService == null)
                    this._localizationService = EngineContext.Current.Resolve<ILocalizationService>();
                return this._localizationService;
            }
        }

        protected IUrlRecordService UrlRecordService
        {
            get
            {
                if (this._urlRecordService == null)
                    this._urlRecordService = EngineContext.Current.Resolve<IUrlRecordService>();
                return this._urlRecordService;
            }
        }

        protected ICustomerService CustomerService
        {
            get
            {
                if (this._customerService == null)
                    this._customerService = EngineContext.Current.Resolve<ICustomerService>();
                return this._customerService;
            }
        }

        protected IStaticCacheManager StaticCacheManager
        {
            get
            {
                if (this._staticCacheManager == null)
                    this._staticCacheManager = EngineContext.Current.Resolve<IStaticCacheManager>();
                return this._staticCacheManager;
            }
        }
    }
}

