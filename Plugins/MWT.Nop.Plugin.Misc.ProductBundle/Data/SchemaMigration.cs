using FluentMigrator;
using MWT.Nop.Plugin.Misc.ProductBundle.Domain;
using Nop.Data.Extensions;
using Nop.Data.Migrations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MWT.Nop.Plugin.Misc.ProductBundle.Data
{
 
    [NopMigration("2026/05/20 00:00:00", "Bundle Configuration")]
    public class SchemaMigration : AutoReversingMigration
    {
        protected IMigrationManager _migrationManager;
         
        public override void Up()
        {
            if (!Schema.Table(nameof(BundleAuditLog)).Exists())
            {
                Create.TableFor<BundleAuditLog>();
            }

            if (!Schema.Table(nameof(BundleConfiguration)).Exists())
            {
                Create.TableFor<BundleConfiguration>();
            }

            if (!Schema.Table(nameof(BundleItem)).Exists())
            {
                Create.TableFor<BundleItem>();
            }

            if (!Schema.Table(nameof(VariantPriceBackup)).Exists())
            {
                Create.TableFor<VariantPriceBackup>();
            }

        }
    }
}
