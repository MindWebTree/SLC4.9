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

            if (!Schema.Table(nameof(MWTTaxZarTransactionLog)).Exists())
            {
                Create.TableFor<MWTTaxZarTransactionLog>();
            }
            if (!Schema.Table(nameof(MWTTaxRate)).Exists())
            {
                Create.TableFor<MWTTaxRate>();
            }
        }
    }
}