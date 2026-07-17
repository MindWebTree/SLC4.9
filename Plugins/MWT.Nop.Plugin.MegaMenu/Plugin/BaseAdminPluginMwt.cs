
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using MWT.Nop.Plugin.MegaMenu.Areas.Admin.Models;
using Nop.Core;
using Nop.Core.Domain.Localization;
using Nop.Core.Infrastructure;
using Nop.Services.Configuration;
using Nop.Services.Events;
using Nop.Services.Localization;
using Nop.Services.Plugins;
using Nop.Web.Areas.Admin.Models.Common;
using Nop.Web.Framework.Events;
using Nop.Web.Framework.Menu;
using Nop.Web.Framework.Security;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;


#nullable enable
namespace MWT.Nop.Plugin.MegaMenu.Plugin
{
    public abstract class BaseAdminPluginMwt : BasePluginMwt, IPlugin, IConsumer<AdminMenuCreatedEvent>
    {
        private readonly
    List<MenuItemMwt> _menuItems;
        private readonly string _pluginAdminMenuResourceKey;
        private readonly bool _isTrialVersion;
        private readonly string _pluginUrlInStore;

        protected string StoreLocation => EngineContext.Current.Resolve<IWebHelper>().GetStoreLocation(new bool?());
        protected INopFileProvider FileProvider => EngineContext.Current.Resolve<INopFileProvider>();
        protected BaseAdminPluginMwt(
          List<MenuItemMwt> menuItems,
          string pluginAdminMenuResourceKey,
          string pluginFolderName,
          bool isTrialVersion = false,
          string pluginUrlInStore = "")
          : base(pluginFolderName)
        {
            this._menuItems = menuItems;
            this._pluginAdminMenuResourceKey = pluginAdminMenuResourceKey;
            this._isTrialVersion = isTrialVersion;
            this._pluginUrlInStore = pluginUrlInStore;
        }

        public abstract override string GetConfigurationPageUrl();

        public virtual async Task<WarningsModel> WarningsAsync()
        {
   WarningsModel warningsModel1 = new WarningsModel();
            WarningsModel warningsModel2 = warningsModel1;
            warningsModel2.PluginName = await LocalizationService.GetResourceAsync(_pluginAdminMenuResourceKey);
            warningsModel2 = (WarningsModel)null;
            string path1 = NopFileProvider.MapPath(string.Format("~/Plugins/{0}/Resources/", (object)PluginFolderName));
            if (!Directory.Exists(path1))
            {
                ((ICollection<SystemWarningModel>)warningsModel1.Warnings).Add(new SystemWarningModel()
                {
                    Level = (SystemWarningLevel)3,
                    Text = string.Format("The resource directory does not exists! - {0}", (object)path1)
                });
            }
            else
            {
                string path2 = NopFileProvider.MapPath(string.Format("~/Plugins/{0}/Resources/Resources.en-us.xml", (object)PluginFolderName));
                if (File.Exists(path2) && !FilePermissionHelper.CheckPermissions(FileProvider, path2, true, false, false, false))
                    ((ICollection<SystemWarningModel>)warningsModel1.Warnings).Add(new SystemWarningModel()
                    {
                        Level = (SystemWarningLevel)3,
                        Text = string.Format("The file {0} does not have the correct permissions! You need to add read permissions to the file !", (object)path2)
                    });
            }
            WarningsModel warningsModel = warningsModel1;
            warningsModel1 = (WarningsModel)null;
            return warningsModel;
        }

        protected void CheckIfDirectoryExistsAndHasCorrectPermissions(
          string directoryPath,
          WarningsModel warningsModel,
          bool checkRead = true,
          bool checkWrite = true)
        {
            if (!Directory.Exists(directoryPath))
            {
                ((ICollection<SystemWarningModel>)warningsModel.Warnings).Add(new SystemWarningModel()
                {
                    Level = (SystemWarningLevel)4,
                    Text = string.Format("The directory does not exists, please create it manually! - {0}", (object)directoryPath)
                });
            }
            else
            {
                if (FilePermissionHelper.CheckPermissions(FileProvider , directoryPath, checkRead, checkWrite, false, false))
                    return;
                ((ICollection<SystemWarningModel>)warningsModel.Warnings).Add(new SystemWarningModel()
                {
                    Level = (SystemWarningLevel)4,
                    Text = string.Format("The directory {0} does not have the correct permissions! You need to add read/write permissions to the directory.", (object)directoryPath)
                });
            }
        }

        private async Task<string> GetResourceAsync(string resourceName)
        {
            string resourceAsync = await this.LocalizationService.GetResourceAsync(resourceName);
            return !string.IsNullOrEmpty(resourceAsync) ? resourceAsync : resourceName;
        }

        public virtual async Task<AdminMenuItem> BuildMenuItemAsync()
        {

            ILocalizationService ilocalizationService = LocalizationService;
            string str = _pluginAdminMenuResourceKey;
            LocaleStringResource resourceByNameAsync = await ilocalizationService.GetLocaleStringResourceByNameAsync(str, ((BaseEntity)await WorkContext.GetWorkingLanguageAsync()).Id, true);
            ilocalizationService = (ILocalizationService)null;
            str = (string)null;
            LocaleStringResource localeStringResource = resourceByNameAsync;
            string pluginMenuName = _pluginAdminMenuResourceKey;
            if (localeStringResource != null)
                pluginMenuName = localeStringResource.ResourceValue;
            AdminMenuItem siteMapNode1 = new AdminMenuItem();
            siteMapNode1.Title = pluginMenuName;
            AdminMenuItem siteMapNode2 = siteMapNode1;
            siteMapNode2.Visible = await AuthenticateAsync();
            siteMapNode1.SystemName = PluginFolderName;
            siteMapNode1.IconClass = "fa fa-genderless";
            AdminMenuItem nextMenuNode = siteMapNode1;
            siteMapNode2 = (AdminMenuItem)null;
            siteMapNode1 = (AdminMenuItem)null;
            IList<AdminMenuItem> siteMapNodeList;
            foreach (MenuItemMwt menuItem1 in _menuItems)
            {
                MenuItemMwt menuItem = menuItem1;
                siteMapNodeList = nextMenuNode.ChildNodes;
                siteMapNode2 = new AdminMenuItem();
                siteMapNode1 = siteMapNode2;
                siteMapNode1.Title = await GetResourceAsync(menuItem.SubMenuName);
                siteMapNode2.Url = WebHelper.GetStoreLocation(new bool?()) + "admin/" + menuItem.SubMenuRelativePath;
                siteMapNode2.Visible = true;
                siteMapNode2.SystemName = menuItem.SubMenuName;
                siteMapNode2.IconClass = "fa fa-genderless";
                ((ICollection<AdminMenuItem>)siteMapNodeList).Add(siteMapNode2);
                siteMapNodeList = (IList<AdminMenuItem>)null;
                siteMapNode1 = (AdminMenuItem)null;
                siteMapNode2 = (AdminMenuItem)null;
                menuItem = new MenuItemMwt();
            }
            AdminMenuItem siteMapNode = nextMenuNode;
            pluginMenuName = (string)null;
            nextMenuNode = (AdminMenuItem)null;
            return siteMapNode;
        }

        //public virtual async Task ManageSiteMapAsync(SiteMapNode rootNode)
        //{

        //    if (!await AuthenticateAsync())
        //        return;
        //    AdminMenuItem childMenuItem = await BuildMenuItemAsync();
        //    if (childMenuItem == null)
        //        return;
        //    await AttachMenuItemAsync(rootNode, ParentNodeSystemName, childMenuItem);
        //}

        //public virtual async Task ManageSiteMapAsync(AdminMenuItem rootNode)
        //{
        //    if (!await AuthenticateAsync())
        //        return;
        //    AdminMenuItem childMenuItem = await BuildMenuItemAsync();
        //    if (childMenuItem == null)
        //        return;
        //    //    await AttachMenuItemAsync(rootNode, ParentNodeSystemName, childMenuItem);
        //    rootNode.ChildNodes.Add(childMenuItem);
        //}

        public async Task HandleEventAsync(AdminMenuCreatedEvent eventMessage)
        {
            if (!await AuthenticateAsync())
                return;

            AdminMenuItem childMenuItem = await BuildMenuItemAsync();
            if (childMenuItem == null)
                return;

            var parentNode = eventMessage.RootMenuItem.ChildNodes.FirstOrDefault(x => x.SystemName == ParentNodeSystemName);
            if (parentNode == null)
            { 
                parentNode = new AdminMenuItem
                {
                    Title = "MWT Plugins",
                    SystemName = ParentNodeSystemName,
                    Visible = true,
                    IconClass = "fa-plug"
                };
                eventMessage.RootMenuItem.ChildNodes.Add(parentNode);
            }
            parentNode.ChildNodes.Add(childMenuItem);
        }

        protected virtual string ParentNodeSystemName => "MWT Plugins";
    }
}
