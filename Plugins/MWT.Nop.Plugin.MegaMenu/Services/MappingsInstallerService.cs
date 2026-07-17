
using MWT.Nop.Plugin.MegaMenu.Domain.Enums;
using System.Threading.Tasks;


namespace MWT.Nop.Plugin.MegaMenu.Services
{
    public class MappingsInstallerService : IMappingInstallerService
    {
        private readonly IEntityMappingService _entityMappingService;
        private readonly IEntityWidgetMappingService _entityWidgetMappingService;

        public MappingsInstallerService(
          IEntityMappingService entityMappingService,
          IEntityWidgetMappingService entityWidgetMappingService)
        {
            this._entityMappingService = entityMappingService;
            this._entityWidgetMappingService = entityWidgetMappingService;
        }

        public async Task InstallMappingsForEntityAsync(EntityType entityType)
        {
            await this._entityMappingService.DeleteEntityMappingsByEntityTypeAsync(entityType);
            await this._entityWidgetMappingService.DeleteEntityWidgetMappingByEntityTypeAsync(entityType);
        }

        public async Task UnInstallMappingsForEntityAsync(EntityType entityType)
        {
            await this._entityMappingService.DeleteEntityMappingsByEntityTypeAsync(entityType);
            await this._entityWidgetMappingService.DeleteEntityWidgetMappingByEntityTypeAsync(entityType);
        }
    }
}
