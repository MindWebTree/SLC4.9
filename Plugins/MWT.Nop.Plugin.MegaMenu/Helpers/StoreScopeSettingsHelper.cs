using Nop.Core.Configuration;
using Nop.Services.Configuration;
using System;
using System.Linq.Expressions;
using System.Threading.Tasks;


#nullable enable
namespace MWT.Nop.Plugin.MegaMenu.Helpers
{
    public class StoreScopeSettingsHelper<TSettings> where TSettings :
  ISettings, new()
    {
        private readonly TSettings _settings;
        private readonly int _storeScope;
        private readonly ISettingService _settingService;

        public int StoreId => this._storeScope;

        public StoreScopeSettingsHelper(
          TSettings settings,
          int storeScope,
          ISettingService settingService)
        {
            this._settings = settings;
            this._storeScope = storeScope;
            this._settingService = settingService;
        }

        public async Task SaveStoreSettingAsync<TPropType>(
          bool overrideForStoreProperty,
          Expression<Func<TSettings, TPropType>> keySelector)
        {
            if (overrideForStoreProperty || this._storeScope == 0)
            {
                await this._settingService.SaveSettingAsync<TSettings, TPropType>(this._settings, keySelector, this._storeScope, false);
            }
            else
            {
                if (this._storeScope <= 0)
                    return;
                await this._settingService.DeleteSettingAsync<TSettings, TPropType>(this._settings, keySelector, this._storeScope);
            }
        }

        public async Task<bool> SettingExistsAsync<TPropType>(
          Expression<Func<TSettings, TPropType>> keySelector)
        {
            return await this._settingService.SettingExistsAsync<TSettings, TPropType>(this._settings, keySelector, this._storeScope);
        }
    }
}
