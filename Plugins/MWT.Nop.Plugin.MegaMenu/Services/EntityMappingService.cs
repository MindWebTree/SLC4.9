
using Nop.Core.Caching;
using Nop.Core.Events;
using Nop.Data;
using MWT.Nop.Plugin.MegaMenu.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using MWT.Nop.Plugin.MegaMenu.Domain;
namespace MWT.Nop.Plugin.MegaMenu.Services
{
    public class EntityMappingService : IEntityMappingService
    {
        private readonly IEventPublisher _eventPublisher;
        private readonly IRepository<EntityMapping> _entityMappingRepository;

        public EntityMappingService(
          IEventPublisher eventPublisher,
          IRepository<EntityMapping> entityMappingRepository)
        {
            this._eventPublisher = eventPublisher;
            this._entityMappingRepository = entityMappingRepository;
        }

        public async Task<IList<EntityMapping>> GetAllEntityMappingsAsync() => (IList<EntityMapping>)await AsyncIQueryableExtensions.ToListAsync<EntityMapping>(this._entityMappingRepository.Table.Select<EntityMapping, EntityMapping>((Expression<Func<EntityMapping, EntityMapping>>)(entityMapping => entityMapping)));

        public async Task<EntityMapping> GetEntityMappingByIdAsync(int id) => await this._entityMappingRepository.GetByIdAsync(new int?(id), (Func<ICacheKeyService, CacheKey>)null, true);

        public async Task InsertEntityMappingAsync(EntityMapping entityMapping)
        {
            if (entityMapping == null)
                throw new ArgumentNullException(nameof(entityMapping));
            await this._entityMappingRepository.InsertAsync(entityMapping, true);
            await EventPublisherExtensions.EntityInsertedAsync<EntityMapping>(this._eventPublisher, entityMapping);
        }

        public async Task UpdateEntityMappingAsync(EntityMapping entityMapping)
        {
            if (entityMapping == null)
                throw new ArgumentNullException(nameof(entityMapping));
            await this._entityMappingRepository.UpdateAsync(entityMapping, true);
            await EventPublisherExtensions.EntityUpdatedAsync<EntityMapping>(this._eventPublisher, entityMapping);
        }

        public async Task DeleteEntityMappingAsync(EntityMapping entityMapping)
        {
            if (entityMapping == null)
                throw new ArgumentNullException(nameof(entityMapping));
            await this._entityMappingRepository.DeleteAsync(entityMapping, true);
            await EventPublisherExtensions.EntityDeletedAsync<EntityMapping>(this._eventPublisher, entityMapping);
        }

        public async Task DeleteEntityMappingsByEntityTypeAsync(EntityType entityType)
        {
            if (!Enum.IsDefined(typeof(EntityType), (object)entityType))
                return;
            IQueryable<EntityMapping> table = this._entityMappingRepository.Table;
            Expression<Func<EntityMapping, bool>> predicate = (Expression<Func<EntityMapping, bool>>)(mapping => (int)mapping.EntityType == (int)entityType);
            foreach (EntityMapping entityMapping in await AsyncIQueryableExtensions.ToListAsync<EntityMapping>(table.Where<EntityMapping>(predicate)))
                await this.DeleteEntityMappingAsync(entityMapping);
        }

        public async Task DeleteEntityMappingsByEntityTypeAndEntityIdAsync(
          EntityType entityType,
          int entityId)
        {
            if (entityId <= 0)
                throw new ArgumentNullException(nameof(entityId));
            IQueryable<EntityMapping> table = this._entityMappingRepository.Table;
            Expression<Func<EntityMapping, bool>> predicate = (Expression<Func<EntityMapping, bool>>)(entityMapping => entityMapping.EntityId == entityId && (int)entityMapping.EntityType == (int)entityType);
            foreach (EntityMapping entityMapping in (IEnumerable<EntityMapping>)await AsyncIQueryableExtensions.ToListAsync<EntityMapping>(table.Where<EntityMapping>(predicate)))
                await this.DeleteEntityMappingAsync(entityMapping);
        }

        public async Task RemoveDeletedEntitiesFromEntityMappingsAsync(
          IQueryable<EntityMapping> mappings,
          IEnumerable<int> availableIds)
        {
            IQueryable<EntityMapping> source = mappings;
            Expression<Func<EntityMapping, bool>> predicate = (Expression<Func<EntityMapping, bool>>)(m => !availableIds.Contains<int>(m.MappedEntityId));
            foreach (EntityMapping entityMapping in await AsyncIQueryableExtensions.ToListAsync<EntityMapping>(source.Where<EntityMapping>(predicate)))
                await this.DeleteEntityMappingAsync(entityMapping);
        }

        public async Task<IList<EntityMapping>> GetAllMappingsByEntityTypeAsync(
          EntityType entityType)
        {
            return (IList<EntityMapping>)await AsyncIQueryableExtensions.ToListAsync<EntityMapping>(this._entityMappingRepository.Table.Where<EntityMapping>((Expression<Func<EntityMapping, bool>>)(entityMapping => (int)entityMapping.EntityType == (int)entityType)));
        }

        public IQueryable<EntityMapping> GetAllMappingsByEntityTypeEntityIdAndMappingType(
          EntityType entityType,
          int entityId,
          MappingType mappingType)
        {
            return this._entityMappingRepository.Table.Where<EntityMapping>((Expression<Func<EntityMapping, bool>>)(entityMapping => (int)entityMapping.EntityType == (int)entityType && entityMapping.EntityId == entityId && (int)entityMapping.MappingType == (int)mappingType));
        }

        public async Task<EntityMapping> GetMappingByEntityTypeEntityIdMappedEntityIdAndMappingTypeAsync(
          EntityType entityType,
          int entityId,
          int mappedEntityId,
          MappingType mappingType)
        {
            return await AsyncIQueryableExtensions.FirstOrDefaultAsync<EntityMapping>(this._entityMappingRepository.Table.Where<EntityMapping>((Expression<Func<EntityMapping, bool>>)(entityMapping => (int)entityMapping.EntityType == (int)entityType && entityMapping.EntityId == entityId && entityMapping.MappedEntityId == mappedEntityId && (int)entityMapping.MappingType == (int)mappingType)), (Expression<Func<EntityMapping, bool>>)null);
        }

        public async Task<IList<EntityMapping>> GetAllEntityMappingsByEntityTypeAndEntityIdAsync(
          EntityType entityType,
          int entityId)
        {
            return (IList<EntityMapping>)await AsyncIQueryableExtensions.ToListAsync<EntityMapping>(this._entityMappingRepository.Table.Where<EntityMapping>((Expression<Func<EntityMapping, bool>>)(entityMapping => (int)entityMapping.EntityType == (int)entityType && entityMapping.EntityId == entityId)));
        }

        public async Task CopyEntityMappingsAsync(
          EntityType entityType,
          int entityId,
          int newEntityId)
        {
            foreach (EntityMapping entityMapping in (IEnumerable<EntityMapping>)await this.GetAllEntityMappingsByEntityTypeAndEntityIdAsync(entityType, entityId))
                await this.InsertEntityMappingAsync(new EntityMapping()
                {
                    EntityId = newEntityId,
                    EntityType = entityMapping.EntityType,
                    MappedEntityId = entityMapping.MappedEntityId,
                    DisplayOrder = entityMapping.DisplayOrder,
                    MappingType = entityMapping.MappingType
                });
        }
    }
}
