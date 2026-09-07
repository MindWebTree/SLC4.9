using Nop.Web.Framework.Menu;

namespace MWT.Plugin.Misc.MwtStorefront.Areas.CustomOrder.Services
{
    public interface ICustomOrderMenuService
    {
        Task<IList<AdminMenuItem>> GetMenuItemsAsync();
    }
}
