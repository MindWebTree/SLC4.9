
using FluentMigrator.Builders.Create.Table;
using Nop.Data.Mapping.Builders;
using MWT.Nop.Plugin.MegaMenu.Domain;

namespace MWT.Nop.Plugin.MegaMenus.Data.Mapping.Builders
{
    public class EntityMappingBuilder : NopEntityBuilder<EntityMapping>
    {
        public override void MapEntity(CreateTableExpressionBuilder table) => table.WithColumn("EntityType").AsInt32().WithColumn("MappingType").AsInt32();
    }
}
