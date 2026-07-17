using MWT.Nop.Plugin.MegaMenu.Domain.Enums;
using System.Threading.Tasks;

namespace MWT.Nop.Plugin.MegaMenu.Services
{
    public interface IMappingInstallerService
    {
        Task InstallMappingsForEntityAsync(EntityType entityType);

        Task UnInstallMappingsForEntityAsync(EntityType entityType);
    }
}
