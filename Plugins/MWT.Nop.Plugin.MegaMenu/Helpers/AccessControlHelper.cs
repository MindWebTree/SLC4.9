using Nop.Services.Security;
using System.Threading.Tasks;
namespace MWT.Nop.Plugin.MegaMenu.Helpers
{
    public class AccessControlHelper : IAccessControlHelper
    {
        private readonly IPermissionService _permissionService;

        public AccessControlHelper(IPermissionService permissionService) => this._permissionService = permissionService;

        public async Task<bool> HasManagePluginsPermissionAsync() => await this._permissionService.AuthorizeAsync(StandardPermission.Configuration.MANAGE_SETTINGS);

        public async Task<bool> HasAdminAccessAsync() => await this._permissionService.AuthorizeAsync(StandardPermission.Configuration.MANAGE_SETTINGS);

        public async Task<bool> HasManagePluginPermissionAsync(string pluginSystemName) => await this._permissionService.AuthorizeAsync("Manage" + pluginSystemName);
    }
}