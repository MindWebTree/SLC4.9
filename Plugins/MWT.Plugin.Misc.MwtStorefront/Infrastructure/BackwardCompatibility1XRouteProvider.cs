using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Nop.Web.Framework.Mvc.Routing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MWT.Nop.Plugin.Widgets.Catalog.Infrastructure
{
    public class BackwardCompatibility1XRouteProvider : IRouteProvider
    {
        #region Methods
        public void RegisterRoutes(IEndpointRouteBuilder endpointRouteBuilder)
        {
            endpointRouteBuilder.MapControllerRoute("", "checkoutcustomorder.aspx",
                new { controller = "AspdotnetStoreFrontCompaitable", action = "HandleCustomOrderRedirect" });
        }
        #endregion

        #region Properties

        /// <summary>
        /// Gets a priority of route provider
        /// </summary>
        public int Priority => -999;

        #endregion
    }
}
