using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Nop.Core;
using Nop.Services.Security;
using OfficeOpenXml.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MWT.Plugin.Misc.MwtStorefront.Infrastructure.Filters
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = true)]
    public class CategoryPermissionAttribute : TypeFilterAttribute
    {
        public CategoryPermissionAttribute(string permissionRecordSystemName)
            : base(typeof(AuthorizePermissionFilter))
        {
            Arguments = new object[] { permissionRecordSystemName };
        }

        private class AuthorizePermissionFilter : IAsyncAuthorizationFilter
        {
            private readonly string _permissionRecordSystemName;
            private readonly IPermissionService _permissionService;
            private readonly IWorkContext _workContext;

            public AuthorizePermissionFilter(
                string permissionRecordSystemName,
                IPermissionService permissionService,
                IWorkContext workContext)
            {
                _permissionRecordSystemName = permissionRecordSystemName;
                _permissionService = permissionService;
                _workContext = workContext;
            }

            // Need to confirm
            public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
            {
                //var customer = await _workContext.GetCurrentCustomerAsync();
                //var authorized = await _permissionService.CustomAuthorizeAsync(_permissionRecordSystemName, customer, context);

                //if (!authorized)
                //{
                //    context.Result = new RedirectToActionResult("AccessDenied", "Security", new { area = "Admin" });
                //}
            }
        }
    }
}
