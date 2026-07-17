using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Nop.Services.Security;
using MWT.Nop.Plugin.MegaMenu.Helpers;
using System;
using System.Threading.Tasks;


#nullable enable
namespace MWT.Nop.Plugin.MegaMenu.Areas.Admin.Attributes
{
    public class ManagePluginsAdminAuthorizeAttribute : TypeFilterAttribute
    {
        public ManagePluginsAdminAuthorizeAttribute(string permissionSystemName = "", bool ignore = false)
          : base(typeof(ManagePluginsAdminAuthorizeAttribute.ManagePluginsAdminAuthorizeFilter))
        {
            this.Arguments = new object[2]
            {
        (object) permissionSystemName,
        (object) ignore
            };
        }

        private class ManagePluginsAdminAuthorizeFilter : IAsyncAuthorizationFilter, IFilterMetadata
        {
            private readonly bool _ignoreFilter;
            private readonly string _pluginSystemName;
            private readonly IAccessControlHelper _accessControlHelper;

            public ManagePluginsAdminAuthorizeFilter(
              string pluginSystemName,
              bool ignoreFilter,
              IAccessControlHelper accessControlHelper)
            {
                this._pluginSystemName = pluginSystemName;
                this._ignoreFilter = ignoreFilter;
                this._accessControlHelper = accessControlHelper;
            }

            public async Task OnAuthorizationAsync(AuthorizationFilterContext filterContext)
            {
                if (this._ignoreFilter)
                    return;
                if (filterContext == null)
                    throw new ArgumentNullException(nameof(filterContext));
                //if (!await this._accessControlHelper.HasAdminAccessAsync())
                //    return;
                bool flag = !string.IsNullOrEmpty(this._pluginSystemName);
                if (flag)
                    flag = !await this._accessControlHelper.HasManagePluginPermissionAsync(this._pluginSystemName);
                if (!flag)
                    return;
                filterContext.Result = (IActionResult)new RedirectToActionResult("AccessDenied", "Security", (object)filterContext.RouteData.Values);
            }
        }
    }
}
