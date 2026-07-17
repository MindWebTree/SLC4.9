using Microsoft.AspNetCore.Mvc;
using MWT.Nop.Plugin.MegaMenu.Areas.Admin.Models;
using MWT.Nop.Plugin.MegaMenu.Models;

namespace MWT.Nop.Plugin.MegaMenu.Components
{
    public class MegaMenuMenuItemViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke(MenuItemModel menuItem) { return View("MegaMenuMenuItem", menuItem); }
    }
}
