using FluentMigrator.Builders.Create.Table;
using Nop.Data.Extensions;
using Nop.Data.Mapping.Builders;
using MWT.Nop.Plugin.MegaMenu.Domain;
using System.Data;

namespace MWT.Nop.Plugin.MegaMenu.Data.Mapping.Builders
{
    public class MenuItemBuilder : NopEntityBuilder<MenuItem>
    {
        public override void MapEntity(CreateTableExpressionBuilder table) => FluentMigratorExtensions.ForeignKey<Menu>(table.WithColumn("MenuId").AsInt32().Nullable(), (string)null, (string)null, Rule.Cascade).WithColumn("Type").AsInt32().WithColumn("CatalogTemplate").AsInt32().WithColumn("Width").AsDecimal(18, 2);
    }
}
