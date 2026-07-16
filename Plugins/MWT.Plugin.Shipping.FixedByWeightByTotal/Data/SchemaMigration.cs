using FluentMigrator;
using Nop.Data.Migrations;
using MWT.Plugin.Shipping.FixedByWeightByTotal.Domain;
using Nop.Data.Extensions;

namespace MWT.Plugin.Shipping.FixedByWeightByTotal.Data
{ 
    [NopMigration("2026/01/31 11:24:16:2551771", "MWT.Shipping.FixedByWeightByTotal base schema", 
        MigrationProcessType.Installation)]
    public class SchemaMigration : AutoReversingMigration
    { 
        public override void Up()
        {
            Create.TableFor<MWTShippingZone>();
            Create.TableFor<MWTShippingByWeightByTotalRecord>();
            Create.TableFor<MWTEstimationDeliveryDateNotification>();
        }
    }
}