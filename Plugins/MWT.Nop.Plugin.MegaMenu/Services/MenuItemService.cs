using MWT.Nop.Plugin.MegaMenu.Domain;
using Nop.Core.Caching;
using Nop.Core.Events;
using Nop.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;



namespace MWT.Nop.Plugin.MegaMenu.Services
{
    public class MenuItemService : IMenuItemService
    {
        private readonly  IRepository<MenuItem> _menuItemRepository;
        private readonly IEventPublisher _eventPublisher;

        public MenuItemService(IRepository<MenuItem> menuItemRepository, IEventPublisher eventPublisher)
        {
            this._menuItemRepository = menuItemRepository;
            this._eventPublisher = eventPublisher;
        }

        public async Task<IList<MenuItem>> GetAllItemsAsync() => (IList<MenuItem>)await AsyncIQueryableExtensions.ToListAsync<MenuItem>(this._menuItemRepository.Table.Select<MenuItem, MenuItem>((Expression<Func<MenuItem, MenuItem>>)(mi => mi)));

        public async Task<IList<MenuItem>> GetAllItemsForMenuAsync(int menuId) => (IList<MenuItem>)await AsyncIQueryableExtensions.ToListAsync<MenuItem>(this._menuItemRepository.Table.Where<MenuItem>((Expression<Func<MenuItem, bool>>)(mi => mi.MenuId == (int?)menuId)));

        public async Task<IList<MenuItem>> GetAllChildrenForMenuItemAsync(
          int menuItemId)
        {
            return (IList<MenuItem>)await AsyncIQueryableExtensions.ToListAsync<MenuItem>(this._menuItemRepository.Table.Where<MenuItem>((Expression<Func<MenuItem, bool>>)(mi => mi.ParentMenuItemId == menuItemId)));
        }

        public async Task<MenuItem> GetMenuItemByIdAsync(int id) => await this._menuItemRepository.GetByIdAsync(new int?(id), (Func<ICacheKeyService, CacheKey>)null, true);

        public async Task CreateAsync(MenuItem menuItem)
        {
            if (menuItem == null)
                throw new ArgumentNullException(nameof(menuItem));
            await this._menuItemRepository.InsertAsync(menuItem, true);
            await EventPublisherExtensions.EntityInsertedAsync<MenuItem>(this._eventPublisher, menuItem);
        }

        public async Task UpdateAsync(MenuItem menuItem)
        {
            if (menuItem == null)
                throw new ArgumentNullException(nameof(menuItem));
            await this._menuItemRepository.UpdateAsync(menuItem, true);
            await EventPublisherExtensions.EntityUpdatedAsync<MenuItem>(this._eventPublisher, menuItem);
        }

        public async Task DeleteAsync(MenuItem menuItem)
        {
            if (menuItem == null)
                throw new ArgumentNullException(nameof(menuItem));
            await this._menuItemRepository.DeleteAsync(menuItem, true);
            await EventPublisherExtensions.EntityDeletedAsync<MenuItem>(this._eventPublisher, menuItem);
        }
    }
}
