
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MWT.Nop.Plugin.MegaMenu.Helpers
{
    public interface IInstallHelper
    {
        Task InstallLocaleResourcesAsync(
          string pluginFolderName,
          string fileNameWithoutCultureAndExtension = "Resources",
          bool updateExistingResources = true);

        Task InstallDefaultPluginSettingsAsync(
          string pluginFolderName,
          bool overrideExistingSettings = true);

        Task<IEnumerable<string>> GetSupportedWidgetZonesAsync(string pluginFolderName);

        Task<IEnumerable<string>> GetSupportedWidgetZonesAsync(
          string pluginFolderName,
          int storeId);

        Task<IEnumerable<string>> GetSupportedWidgetZonesAsync(
          string pluginFolderName,
          string fileName);

        Task<IEnumerable<string>> GetSupportedWidgetZonesAsync(
          string pluginFolderName,
          string fileName,
          int storeId);
    }
}
