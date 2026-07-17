using FluentMigrator.Builders.Create.Table;
using MWT.Nop.Plugin.MegaMenu.Domain;
using Nop.Data.Mapping.Builders;


namespace MWT.Nop.Plugin.MegaMenu.Data.Mapping.Builders
{
    public class EntityWidgetMappingBuilder : NopEntityBuilder<EntityWidgetMapping>
    {
        public override void MapEntity(CreateTableExpressionBuilder table) => table.WithColumn("EntityType").AsInt32();
    }
}


