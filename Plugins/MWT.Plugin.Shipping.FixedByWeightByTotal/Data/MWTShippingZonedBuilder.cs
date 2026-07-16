using FluentMigrator.Builders.Create.Table;
using Nop.Data.Mapping.Builders;
using MWT.Plugin.Shipping.FixedByWeightByTotal.Domain;

namespace MWT.Plugin.Shipping.FixedByWeightByTotal.Data
{
    public class MWTShippingZonedBuilder : NopEntityBuilder<MWTShippingZone>
    {
        public override void MapEntity(CreateTableExpressionBuilder table)
        {
            table.WithColumn(nameof(MWTShippingZone.Name))
                  .AsString(100)
                .NotNullable()
                .WithColumn(nameof(MWTShippingZone.ZipCodes))
                .AsString()
                .Nullable();
        }
    }
}