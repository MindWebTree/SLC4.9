using MWT.Nop.Plugin.MegaMenu.Domain.Enums;
using MWT.Nop.Plugin.MegaMenu.Domain;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MWT.Nop.Plugin.MegaMenu.Services
{
    public interface IEntityMappingService
    {
        Task<IList<EntityMapping>> GetAllEntityMappingsAsync();

        Task<EntityMapping> GetEntityMappingByIdAsync(int id);

        Task InsertEntityMappingAsync(EntityMapping entityMapping);

        Task UpdateEntityMappingAsync(EntityMapping entityMapping);

        Task DeleteEntityMappingAsync(EntityMapping entityMapping);

        Task DeleteEntityMappingsByEntityTypeAsync(EntityType entityType);

        Task DeleteEntityMappingsByEntityTypeAndEntityIdAsync(EntityType entityType, int entityId);

        Task RemoveDeletedEntitiesFromEntityMappingsAsync(
          IQueryable<EntityMapping> mappings,
          IEnumerable<int> availableIds);

        Task<IList<EntityMapping>> GetAllMappingsByEntityTypeAsync(
          EntityType entityType);

        IQueryable<EntityMapping> GetAllMappingsByEntityTypeEntityIdAndMappingType(
          EntityType entityType,
          int entityId,
          MappingType mappingType);

        Task<EntityMapping> GetMappingByEntityTypeEntityIdMappedEntityIdAndMappingTypeAsync(
          EntityType entityType,
          int entityId,
          int mappedEntityId,
          MappingType mappingType);

        Task<IList<EntityMapping>> GetAllEntityMappingsByEntityTypeAndEntityIdAsync(
          EntityType entityType,
          int entityId);

        Task CopyEntityMappingsAsync(EntityType entityType, int entityId, int newEntityId);
    }
}
