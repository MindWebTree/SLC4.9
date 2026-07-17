
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Nop.Core;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Security;
using Nop.Core.Infrastructure;
using Nop.Data;
using Nop.Services.Localization;
using Nop.Services.Plugins;
using Nop.Services.Security;
using Nop.Web.Framework.Menu;
using MWT.Nop.Plugin.MegaMenu.Helpers;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;



namespace MWT.Nop.Plugin.MegaMenu.Plugin
{
    public abstract class BasePluginMwt : BasePlugin
    {
        protected string PluginFolderName;
        private ILocalizationService _localizationService;
        private IWorkContext _workContext;
        private IWebHelper _webHelper;
        private IPermissionService _permissionService;
        private INopFileProvider _nopFileProvider;
        private IAccessControlHelper _accessControlHelper;

        protected ILocalizationService LocalizationService
        {
            get
            {
                if (this._localizationService == null)
                    this._localizationService = EngineContext.Current.Resolve<ILocalizationService>();
                return this._localizationService;
            }
        }

        protected IWorkContext WorkContext
        {
            get
            {
                if (this._workContext == null)
                    this._workContext = EngineContext.Current.Resolve<IWorkContext>();
                return this._workContext;
            }
        }

        protected IWebHelper WebHelper
        {
            get
            {
                if (this._webHelper == null)
                    this._webHelper = EngineContext.Current.Resolve<IWebHelper>();
                return this._webHelper;
            }
        }

        protected IPermissionService PermissionService
        {
            get
            {
                if (this._permissionService == null)
                    this._permissionService = EngineContext.Current.Resolve<IPermissionService>();
                return this._permissionService;
            }
        }

        protected INopFileProvider NopFileProvider
        {
            get
            {
                if (this._nopFileProvider == null)
                    this._nopFileProvider = EngineContext.Current.Resolve<INopFileProvider>();
                return this._nopFileProvider;
            }
        }

        protected IAccessControlHelper AccessControlHelper
        {
            get
            {
                if (this._accessControlHelper == null)
                    this._accessControlHelper = EngineContext.Current.Resolve<IAccessControlHelper>();
                return this._accessControlHelper;
            }
        }

        protected BasePluginMwt(string pluginFolderName) => this.PluginFolderName = pluginFolderName;

        public async Task<bool> AuthenticateAsync()
        {
            bool flag = await this.AccessControlHelper.HasManagePluginsPermissionAsync();
            if (!flag)
                flag = await this.AccessControlHelper.HasManagePluginPermissionAsync(this.PluginFolderName);
            return flag;
        }

        public virtual async Task InstallAsync()
        {
            IInstallHelper installHelper = EngineContext.Current.Resolve<IInstallHelper>();
            await installHelper.InstallDefaultPluginSettingsAsync(this.PluginFolderName);
            await installHelper.InstallLocaleResourcesAsync(this.PluginFolderName);
            await this.InstallAdditionalSettingsAsync();
            //await this.SetupPermissionRecordsAsync();
            await base.InstallAsync();
            installHelper = (IInstallHelper)null;
        }

        public virtual async Task UninstallAsync()
        {
            await this.UninstallAdditionalSettingsAsync();
            await this.DeletePluginPermissionRecordAsync();
            await base.UninstallAsync();
        }

        protected virtual async Task InstallAdditionalSettingsAsync() => await Task.CompletedTask;

        protected virtual async Task UninstallAdditionalSettingsAsync() => await Task.CompletedTask;

        protected async Task AttachMenuItemAsync(
          AdminMenuItem rootNode,
          string parentNodeSystemName,
          AdminMenuItem childMenuItem,
          bool ignorePermissions = false)
        {
            //bool flag = !ignorePermissions;
            //if (flag)
            //    flag = !await this.AuthenticateAsync();
            //if (flag)
            //    ;
            //else
            //{
            //    SiteMapNode siteMapNode = ((IEnumerable<SiteMapNode>)(await this.EnsureMainMenuInitializedAsync(rootNode)).ChildNodes).FirstOrDefault<SiteMapNode>((Func<SiteMapNode, bool>)(x => x.SystemName == parentNodeSystemName));
            //    if (siteMapNode == null)
            //        ;
            //    else
            //    {
            var node = new AdminMenuItem()
            {
                Title = parentNodeSystemName,
                SystemName = parentNodeSystemName,
                Visible = false,
                IconClass = "icon-plugins"

            };

            node.Visible = true;
            node.ChildNodes.Add(childMenuItem);
            rootNode.ChildNodes.Add(node);
            //}
            //}
        }

        private async Task<AdminMenuItem> EnsureMainMenuInitializedAsync(
          AdminMenuItem rootNode)
        {
            ILocalizationService ilocalizationService = this.LocalizationService;
            string resourceAsync1 = await ilocalizationService.GetResourceAsync("Mwt.Plugin.Admin.Menu.MainMenu", ((BaseEntity)await this.WorkContext.GetWorkingLanguageAsync()).Id, true, "Nop-Templates", false);
            ilocalizationService = (ILocalizationService)null;
            string mainMenuTitle = resourceAsync1;
            ilocalizationService = this.LocalizationService;
            string resourceAsync2 = await ilocalizationService.GetResourceAsync("Mwt.Plugin.Admin.Menu.Plugins.MenuName", ((BaseEntity)await this.WorkContext.GetWorkingLanguageAsync()).Id, true, "Plugins", false);
            ilocalizationService = (ILocalizationService)null;
            string pluginsTitle = resourceAsync2;
            ilocalizationService = this.LocalizationService;
            string resourceAsync3 = await ilocalizationService.GetResourceAsync("Mwt.Plugin.Admin.Menu.Themes.MenuName", ((BaseEntity)await this.WorkContext.GetWorkingLanguageAsync()).Id, true, "Themes", false);
            ilocalizationService = (ILocalizationService)null;
            string templatesTitle = resourceAsync3;
            ilocalizationService = this.LocalizationService;
            string resourceAsync4 = await ilocalizationService.GetResourceAsync("Mwt.Plugin.Admin.Menu.PluginsAccessControl.MenuName", ((BaseEntity)await this.WorkContext.GetWorkingLanguageAsync()).Id, true, "Plugins Access Control", false);
            ilocalizationService = (ILocalizationService)null;
            string accessControlTitle = resourceAsync4;
            ilocalizationService = this.LocalizationService;
            string resourceAsync5 = await ilocalizationService.GetResourceAsync("Mwt.Plugin.Admin.Menu.Warnings.MenuName", ((BaseEntity)await this.WorkContext.GetWorkingLanguageAsync()).Id, true, "Warnings", false);
            ilocalizationService = (ILocalizationService)null;
            string warningsTitle = resourceAsync5;
            ilocalizationService = this.LocalizationService;
            string resourceAsync6 = await ilocalizationService.GetResourceAsync("Mwt.Plugin.Admin.Menu.Information.MenuName", ((BaseEntity)await this.WorkContext.GetWorkingLanguageAsync()).Id, true, "Information", false);
            ilocalizationService = (ILocalizationService)null;
            string str = resourceAsync6;
            AdminMenuItem mainMenuNode = ((IEnumerable<AdminMenuItem>)rootNode.ChildNodes).FirstOrDefault<AdminMenuItem>((Func<AdminMenuItem, bool>)(x => x.SystemName == "nopTemplates"));
            if (mainMenuNode == null)
            {
                mainMenuNode = new AdminMenuItem()
                {
                    SystemName = "nopTemplates",
                    Title = mainMenuTitle,
          //          RouteValues = new RouteValueDictionary()
          //{
          //  {
          //    "area",
          //    (object) "admin"
          //  }
          //},
                    IconClass = "fa icon-nop-templates",
                    Visible = true
                };





            }
            AdminMenuItem siteMapNode = mainMenuNode;
            return siteMapNode;
        }

        protected async Task DeletePluginPermissionRecordAsync()
        {
            string permissionRecordSystemName = "Manage" + this.PluginFolderName;
            PermissionRecord permissionRecord = ((IEnumerable<PermissionRecord>)await this.PermissionService.GetAllPermissionRecordsAsync()).FirstOrDefault<PermissionRecord>((Func<PermissionRecord, bool>)(x => x.SystemName.Equals(permissionRecordSystemName)));
            if (permissionRecord == null)
                ;
            else
                await EngineContext.Current.Resolve<IRepository<PermissionRecord>>().DeleteAsync(permissionRecord, true);
        }

        public virtual async Task SetupPermissionRecordsAsync()
        {
            //    BasePlugin7Spikes basePlugin7Spikes = this;
            //    Base7SpikesPermissionProvider permissionProvider = new Base7SpikesPermissionProvider(basePlugin7Spikes.PluginFolderName, basePlugin7Spikes.PluginDescriptor.FriendlyName, NopCustomerDefaults.AdministratorsRoleName);
            //    await basePlugin7Spikes.PermissionService.InstallPermissionsAsync((IPermissionProvider)permissionProvider);

        }

        public abstract string GetConfigurationPageUrl();
    }
}
