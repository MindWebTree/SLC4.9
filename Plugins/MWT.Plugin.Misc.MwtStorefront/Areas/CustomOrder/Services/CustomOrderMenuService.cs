using MWT.Nop.Core.Services.Customizations.CustomOrders;
using Nop.Core;
using Nop.Services.Events;
using Nop.Services.Localization;
using Nop.Services.Security;
using Nop.Web.Framework.Events;
using Nop.Web.Framework.Menu;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Intrinsics.X86;
using System.Text;
using System.Threading.Tasks;
using static MWT.Plugin.Misc.MwtStorefront.Areas.CustomOrder.Services.CustomOrderMenuService;
using static Nop.Services.Security.StandardPermission;

namespace MWT.Plugin.Misc.MwtStorefront.Areas.CustomOrder.Services
{
    public class CustomOrderMenuService : ICustomOrderMenuService
    {
        private readonly IPermissionService _permissionService;
        private readonly ILocalizationService _localizationService;

        public CustomOrderMenuService(IPermissionService permissionService,
            ILocalizationService localizationService)
        {
            _permissionService = permissionService;
            _localizationService = localizationService;
        }

        public async Task<IList<AdminMenuItem>> GetMenuItemsAsync()
        {
            var items = new List<AdminMenuItem>();


            items.Add(new AdminMenuItem
            {
                SystemName = "Orders",
                Title = await _localizationService.GetResourceAsync("CustomOrder.Orders"),
                Url = "/CustomOrder/Order/Index",
                Visible = true,
                PermissionNames = new List<string>
            {
                CustomPermission.CUSTOM_ACCESS_CUSTOMORDER
            },
                IconClass = "/css/Customizations/Areas/CustomOrder/images/Orders.png"
            });

            items.Add(new AdminMenuItem
            {
                SystemName = "PartialPayment",
                Title = await _localizationService.GetResourceAsync("CustomOrder.PartialPayment"),
                Url = "/CustomOrder/Order/List?IsPartialOrderScreen=true",
                Visible = true,
                PermissionNames = new List<string>
            {
                CustomPermission.CUSTOM_ACCESS_CUSTOMORDER
            },
                IconClass = "/css/Customizations/Areas/CustomOrder/images/part-payment.png"
            });

            items.Add(new AdminMenuItem
            {
                SystemName = "AdditionalService",
                Title = await _localizationService.GetResourceAsync("CustomOrder.AdditionalService"),
                Url = "/CustomOrder/AdditionalService",
                Visible = true,
                PermissionNames = new List<string>
            {
                CustomPermission.CUSTOM_ACCESS_CUSTOMORDER
            },
                IconClass = "/css/Customizations/Areas/CustomOrder/images/additional-service.png"
            });


            items.Add(new AdminMenuItem
            {
                SystemName = "Customers",
                Title = await _localizationService.GetResourceAsync("CustomOrder.Customers"),
                Url = "/CustomOrder/Customer/Index",
                Visible = true,
                PermissionNames = new List<string>
                {
                    Customers.CUSTOMERS_CREATE_EDIT_DELETE
                },
                IconClass = "/css/Customizations/Areas/CustomOrder/images/customers.png"
            });


            return items;
        }
    }
}
