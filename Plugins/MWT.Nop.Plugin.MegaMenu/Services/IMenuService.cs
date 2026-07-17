
using MWT.Nop.Plugin.MegaMenu.Domain;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MWT.Nop.Plugin.MegaMenu.Services
{
    public interface IMenuService
    {
        Task<IList<Menu>> GetAllMenusAsync();

        Task<IList<Menu>> GetMenusByWidgetZoneAsync(string widgetZone);

        Task<Menu> GetMenuByIdAsync(int id);

        Task CreateMenuAsync(Menu menu);

        Task UpdateMenuAsync(Menu menu);

        Task DeleteMenuAsync(Menu menu);
    }
}
