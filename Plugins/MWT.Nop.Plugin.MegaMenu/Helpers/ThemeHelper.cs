
using Microsoft.Extensions.DependencyInjection;
using Nop.Core;
using Nop.Core.Infrastructure;
using Nop.Services.Configuration;
using Nop.Web.Framework.Themes;
using System.IO;
using System.Threading.Tasks;


#nullable enable
namespace MWT.Nop.Plugin.MegaMenu.Helpers
{
    public class ThemeHelper
    {
        private static readonly string DefaultThemeName = "DefaultClean";

        public static async Task<string> GetCurrentDesktopThemeAsync() => await ThemeHelper.ThemeContext.GetWorkingThemeNameAsync();

        public static async Task<string> GetCurrentAdminDesktopThemeAsync(int storeId) => await EngineContext.Current.Resolve<ISettingService>().GetSettingByKeyAsync<string>("storeinformationsettings.defaultstoretheme", (string)null, storeId, true);

        internal static IThemeContext ThemeContext => EngineContext.Current.Resolve<IThemeContext>();

        public static async Task<string> GetPluginThemeAsync(string pluginFolderName) => await ThemeHelper.GetPluginThemeInternalAsync(pluginFolderName);

        private static async Task<string> GetPluginThemeInternalAsync(string pluginFolderName)
        {
            INopFileProvider inopFileProvider = CommonHelper.DefaultFileProvider;
            string str = pluginFolderName;
            string path = inopFileProvider.MapPath("~/Plugins/" + str + "/Themes/" + await ThemeHelper.ThemeContext.GetWorkingThemeNameAsync());
             return path != null && Directory.Exists(path) ? await ThemeHelper.ThemeContext.GetWorkingThemeNameAsync() : ThemeHelper.DefaultThemeName;
        }

        public static async Task<string> GetPluginViewPathAsync(
          string pluginFolderName,
          string viewPath)
        {
            return await ThemeHelper.GetPluginViewPathInternalAsync(pluginFolderName, viewPath);
        }

        private static async Task<string> GetPluginViewPathInternalAsync(
          string pluginFolderName,
          string view)
        {
            string str1 = "Themes/" + await ThemeHelper.ThemeContext.GetWorkingThemeNameAsync();
            string str2 = string.Format(view, (object)str1);
            string str3 = string.Format(view, (object)string.Empty);
            string path = CommonHelper.DefaultFileProvider.MapPath("~/Plugins/" + pluginFolderName + "/" + str1);
            return path == null || !Directory.Exists(path) ? str3 : (!File.Exists(CommonHelper.DefaultFileProvider.MapPath(str2)) ? str3 : str2);
        }
    }
}
