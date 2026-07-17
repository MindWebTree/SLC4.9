using Nop.Core;
using MWT.Nop.Plugin.MegaMenu.Domain.Enums;



namespace MWT.Nop.Plugin.MegaMenu.Domain
{
    public class EntityMapping : BaseEntity
    {
        public EntityType EntityType { get; set; }

        public int EntityId { get; set; }

        public int MappedEntityId { get; set; }

        public int DisplayOrder { get; set; }

        public MappingType MappingType { get; set; }
    }
}
