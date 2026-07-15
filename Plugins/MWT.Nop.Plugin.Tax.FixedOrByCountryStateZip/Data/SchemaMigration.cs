using FluentMigrator;
using MWT.Tax.FixedOrByCountryStateZip.Domain;
using Nop.Data.Extensions;
using Nop.Data.Migrations;

namespace MWT.Tax.FixedOrByCountryStateZip.Data
{ 
    [NopMigration("2026/07/14 09:09:17", "MWT.Tax.FixedOrByCountryStateZip base schema", MigrationProcessType.Installation)]
    public class SchemaMigration : AutoReversingMigration
    { 
        public override void Up()
        {
            Create.TableFor<MWTTaxRate>();
            Create.TableFor<MWTTaxZarTransactionLog>();
        }
    }
}