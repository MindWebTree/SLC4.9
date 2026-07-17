using Nop.Core.Caching;
using Nop.Core.Events;
using Nop.Data;
using MWT.Nop.Plugin.MegaMenu.Domain.Enums;
using MWT.Nop.Plugin.MegaMenu.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;


#nullable enable
namespace MWT.Nop.Plugin.MegaMenu.Services
{
    public class EntityWidgetMappingService : IEntityWidgetMappingService
    {
        private readonly    IEventPublisher _eventPublisher;
        private readonly IRepository<EntityWidgetMapping> _entityWidgetMappingRepository;

        public EntityWidgetMappingService(
          IEventPublisher eventPublisher,
          IRepository<EntityWidgetMapping> entityWidgetMappingRepository)
        {
            this._eventPublisher = eventPublisher;
            this._entityWidgetMappingRepository = entityWidgetMappingRepository;
        }

        public async Task<EntityWidgetMapping> GetEntityWidgetMappingByIdAsync(
          int id)
        {
            return await _entityWidgetMappingRepository.GetByIdAsync(id, cache => default);
        }

        public async Task UpdateEntityWidgetMappingAsync(EntityWidgetMapping widgetMapping)
        {
            if (widgetMapping == null)
                throw new ArgumentNullException("entityMapping");
            await this._entityWidgetMappingRepository.UpdateAsync(widgetMapping, true);
            await EventPublisherExtensions.EntityUpdatedAsync<EntityWidgetMapping>(this._eventPublisher, widgetMapping);
        }

        public async Task DeleteEntityWidgetMappingAsync(EntityWidgetMapping widgetMapping)
        {
            if (widgetMapping == null)
                throw new ArgumentNullException("entityMapping");
            await this._entityWidgetMappingRepository.DeleteAsync(widgetMapping, true);
            await EventPublisherExtensions.EntityDeletedAsync<EntityWidgetMapping>(this._eventPublisher, widgetMapping);
        }

        public async Task InsertEntityWidgetMappingAsync(EntityWidgetMapping widgetMapping)
        {
            if (widgetMapping == null)
                throw new ArgumentNullException("entityMapping");
            await this._entityWidgetMappingRepository.InsertAsync(widgetMapping, true);
            await EventPublisherExtensions.EntityInsertedAsync<EntityWidgetMapping>(this._eventPublisher, widgetMapping);
        }

        public async Task DeleteEntityWidgetMappingByEntityTypeAsync(EntityType entityType)
        {
            if (!Enum.IsDefined(typeof(EntityType), (object)entityType))
                return;
            IQueryable<EntityWidgetMapping> table = this._entityWidgetMappingRepository.Table;
            Expression<Func<EntityWidgetMapping, bool>> predicate = (Expression<Func<EntityWidgetMapping, bool>>)(widgetMapping => (int)widgetMapping.EntityType == (int)entityType);
            foreach (EntityWidgetMapping widgetMapping in await AsyncIQueryableExtensions.ToListAsync<EntityWidgetMapping>(table.Where<EntityWidgetMapping>(predicate)))
                await this.DeleteEntityWidgetMappingAsync(widgetMapping);
        }

        public async Task<IList<EntityWidgetMapping>> GetAllEntityWidgetMappingsByEntityTypeAsync(
          EntityType entityType)
        {
            return (IList<EntityWidgetMapping>)await AsyncIQueryableExtensions.ToListAsync<EntityWidgetMapping>(this._entityWidgetMappingRepository.Table.Where<EntityWidgetMapping>((Expression<Func<EntityWidgetMapping, bool>>)(entityWidgetMapping => (int)entityWidgetMapping.EntityType == (int)entityType)));
        }

        public async Task<IList<EntityWidgetMapping>> GetAllEntityWidgetMappingsByEntityTypeAndEntityIdAsync(
          EntityType entityType,
          int entityId)
        {
            return (IList<EntityWidgetMapping>)await AsyncIQueryableExtensions.ToListAsync<EntityWidgetMapping>(this._entityWidgetMappingRepository.Table.Where<EntityWidgetMapping>((Expression<Func<EntityWidgetMapping, bool>>)(entityWidgetMapping => (int)entityWidgetMapping.EntityType == (int)entityType && entityWidgetMapping.EntityId == entityId)));
        }

        public async Task<EntityWidgetMapping> GetEntityWidgetMappingByEntityTypeAndEntityIdAsync(
          EntityType entityType,
          int entityId)
        {
            return await AsyncIQueryableExtensions.FirstOrDefaultAsync<EntityWidgetMapping>(this._entityWidgetMappingRepository.Table.Where<EntityWidgetMapping>((Expression<Func<EntityWidgetMapping, bool>>)(entityWidgetMapping => (int)entityWidgetMapping.EntityType == (int)entityType && entityWidgetMapping.EntityId == entityId)), (Expression<Func<EntityWidgetMapping, bool>>)null);
        }

        public async Task<EntityWidgetMapping> GetEntityWidgetMappingByEntityTypeEntityIdAndWidgetZoneAsync(
          EntityType entityType,
          int entityId,
          string widgetZone)
        {
            return await AsyncIQueryableExtensions.FirstOrDefaultAsync<EntityWidgetMapping>(this._entityWidgetMappingRepository.Table.Where<EntityWidgetMapping>((Expression<Func<EntityWidgetMapping, bool>>)(entityWidgetMapping => (int)entityWidgetMapping.EntityType == (int)entityType && entityWidgetMapping.EntityId == entityId && entityWidgetMapping.WidgetZone == widgetZone)), (Expression<Func<EntityWidgetMapping, bool>>)null);
        }

        public async Task CopyEntityWidgetMappingsAsync(
          EntityType entityType,
          int entityId,
          int newEntityId)
        {
            foreach (EntityWidgetMapping entityWidgetMapping in (IEnumerable<EntityWidgetMapping>)await this.GetAllEntityWidgetMappingsByEntityTypeAndEntityIdAsync(entityType, entityId))
                await this.InsertEntityWidgetMappingAsync(new EntityWidgetMapping()
                {
                    EntityId = newEntityId,
                    EntityType = entityWidgetMapping.EntityType,
                    WidgetZone = entityWidgetMapping.WidgetZone,
                    DisplayOrder = entityWidgetMapping.DisplayOrder
                });
        }
    }
}
