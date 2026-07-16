using FluentMigrator.Builders.Create.Table;
using Nop.Data.Mapping.Builders;
using MWT.Plugin.Shipping.FixedByWeightByTotal.Domain;
using Nop.Data.Extensions;
namespace MWT.Plugin.Shipping.FixedByWeightByTotal.Data
{
    public class MWTShippingByWeightByTotalRecordBuilder : NopEntityBuilder<MWTShippingByWeightByTotalRecord>
    {
        public override void MapEntity(CreateTableExpressionBuilder table)
        {
            table 
                .WithColumn(nameof(MWTShippingByWeightByTotalRecord.WeightFrom))
                .AsDecimal(18, 2)
                .WithColumn(nameof(MWTShippingByWeightByTotalRecord.WeightTo))
                .AsDecimal(18, 2)
                .WithColumn(nameof(MWTShippingByWeightByTotalRecord.OrderSubtotalFrom))
                .AsDecimal(18, 2)
                .WithColumn(nameof(MWTShippingByWeightByTotalRecord.OrderSubtotalTo))
                .AsDecimal(18, 2)
                .WithColumn(nameof(MWTShippingByWeightByTotalRecord.AdditionalFixedCost))
                .AsDecimal(18, 2)
                .WithColumn(nameof(MWTShippingByWeightByTotalRecord.Amount))
                .AsDecimal(18, 2)
                .WithColumn(nameof(MWTShippingByWeightByTotalRecord.RatePerWeightUnit))
                .AsDecimal(18, 2)
                .WithColumn(nameof(MWTShippingByWeightByTotalRecord.LowerWeightLimit))
                .AsDecimal(18, 2)
                .WithColumn(nameof(MWTShippingByWeightByTotalRecord.ZoneId))
                .AsInt32()
                .NotNullable();
        }
    }
}