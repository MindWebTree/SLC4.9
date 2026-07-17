using MWT.Nop.Plugin.MegaMenu.Domain.Enums;
using Nop.Core;

namespace MWT.Nop.Plugin.MegaMenu.Domain
{
    public class EntityWidgetMapping : BaseEntity
    {
        public EntityType EntityType { get; set; }

        public int EntityId { get; set; }

        public string WidgetZone { get; set; }

        public int DisplayOrder { get; set; }
    }
}
