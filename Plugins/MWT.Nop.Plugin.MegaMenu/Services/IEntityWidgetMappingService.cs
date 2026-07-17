using MWT.Nop.Plugin.MegaMenu.Domain;
using MWT.Nop.Plugin.MegaMenu.Domain.Enums;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MWT.Nop.Plugin.MegaMenu.Services
{
    public interface IEntityWidgetMappingService
    {
        Task<EntityWidgetMapping> GetEntityWidgetMappingByIdAsync(int id);

        Task UpdateEntityWidgetMappingAsync(EntityWidgetMapping widgetMapping);

        Task DeleteEntityWidgetMappingAsync(EntityWidgetMapping widgetMapping);

        Task InsertEntityWidgetMappingAsync(EntityWidgetMapping widgetMapping);

        Task DeleteEntityWidgetMappingByEntityTypeAsync(EntityType entityType);

        Task<IList<EntityWidgetMapping>> GetAllEntityWidgetMappingsByEntityTypeAsync(
          EntityType entityType);

        Task<IList<EntityWidgetMapping>> GetAllEntityWidgetMappingsByEntityTypeAndEntityIdAsync(
          EntityType entityType,
          int entityId);

        Task<EntityWidgetMapping> GetEntityWidgetMappingByEntityTypeAndEntityIdAsync(
          EntityType entityType,
          int entityId);

        Task<EntityWidgetMapping> GetEntityWidgetMappingByEntityTypeEntityIdAndWidgetZoneAsync(
          EntityType entityType,
          int entityId,
          string widgetZone);

        Task CopyEntityWidgetMappingsAsync(EntityType entityType, int entityId, int newEntityId);
    }
}
