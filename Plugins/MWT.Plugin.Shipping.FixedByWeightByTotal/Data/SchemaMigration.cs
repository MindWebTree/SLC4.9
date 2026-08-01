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
          //  if (!Schema.Table(nameof(MWTShippingZone)).Exists())
         //   {
                Create.TableFor<MWTShippingZone>();
          //  }

         //   if (!Schema.Table(nameof(MWTShippingByWeightByTotalRecord)).Exists())
        //    {
                Create.TableFor<MWTShippingByWeightByTotalRecord>();
        //    }

        //    if (!Schema.Table(nameof(MWTEstimationDeliveryDateNotification)).Exists())
        //    {
                Create.TableFor<MWTEstimationDeliveryDateNotification>();
       //     }
        }
    }
}