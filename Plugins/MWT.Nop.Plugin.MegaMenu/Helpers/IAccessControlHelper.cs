

using System.Threading.Tasks;

namespace MWT.Nop.Plugin.MegaMenu.Helpers
{
    public interface IAccessControlHelper
    {
        Task<bool> HasManagePluginsPermissionAsync();

        Task<bool> HasAdminAccessAsync();

        Task<bool> HasManagePluginPermissionAsync(string pluginSystemName);
    }
}
