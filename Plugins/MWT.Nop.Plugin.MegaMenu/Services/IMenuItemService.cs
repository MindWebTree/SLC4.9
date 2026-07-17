
using MWT.Nop.Plugin.MegaMenu.Domain;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MWT.Nop.Plugin.MegaMenu.Services
{
    public interface IMenuItemService
    {
        Task<IList<MenuItem>> GetAllItemsAsync();

        Task<IList<MenuItem>> GetAllItemsForMenuAsync(int menuId);

        Task<IList<MenuItem>> GetAllChildrenForMenuItemAsync(int menuItemId);

        Task<MenuItem> GetMenuItemByIdAsync(int id);

        Task CreateAsync(MenuItem menuItem);

        Task UpdateAsync(MenuItem menuItem);

        Task DeleteAsync(MenuItem menuItem);
    }
}
