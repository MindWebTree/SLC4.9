
using MWT.Nop.Plugin.MegaMenu.Domain;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Events;
using Nop.Data;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;


#nullable enable
namespace MWT.Nop.Plugin.MegaMenu.Services
{
    public class MenuService : IMenuService
    {
        private readonly IEventPublisher _eventPublisher;
        private readonly IRepository<Menu> _menuRepository;
        private readonly IRepository<EntityWidgetMapping> _entityWidgetMappingRepository;

        public MenuService(
          IRepository<Menu> menuRepository,
          IEventPublisher eventPublisher,
          IRepository<EntityWidgetMapping> entityWidgetMappingRepository)
        {
            this._menuRepository = menuRepository;
            this._eventPublisher = eventPublisher;
            this._entityWidgetMappingRepository = entityWidgetMappingRepository;
        }

        public async Task<IList<Menu>> GetAllMenusAsync()
        {


            var query = from a in _menuRepository.Table
                        orderby a.Id
                        select a;
            return await query.ToListAsync();
        }

        public async Task<IList<Menu>> GetMenusByWidgetZoneAsync(string widgetZone)
        {

            if (string.IsNullOrWhiteSpace(widgetZone))
                return new List<Menu>();

            var query = from menu in _menuRepository.Table
                        join widgetMapping in _entityWidgetMappingRepository.Table
                        on menu.Id equals widgetMapping.EntityId
                        where widgetMapping.WidgetZone == widgetZone
                        orderby menu.Id
                        select menu;
            return await query.ToListAsync();
        }

        public async Task<Menu> GetMenuByIdAsync(int id)
        {
            return await _menuRepository.GetByIdAsync(id, cache => default);
        }

        public async Task CreateMenuAsync(Menu menu)
        {
            if (menu == null)
                throw new ArgumentNullException(nameof(menu));
            await this._menuRepository.InsertAsync(menu, true);
            await EventPublisherExtensions.EntityInsertedAsync<Menu>(this._eventPublisher, menu);
        }

        public async Task UpdateMenuAsync(Menu menu)
        {
            if (menu == null)
                throw new ArgumentNullException(nameof(menu));
            await this._menuRepository.UpdateAsync(menu, true);
            await EventPublisherExtensions.EntityUpdatedAsync<Menu>(this._eventPublisher, menu);
        }

        public async Task DeleteMenuAsync(Menu menu)
        {
            if (menu == null)
                throw new ArgumentNullException(nameof(menu));
            await this._menuRepository.DeleteAsync(menu, true);
            await EventPublisherExtensions.EntityDeletedAsync<Menu>(this._eventPublisher, menu);
        }
    }
}
