using FluentMigrator;
using Nop.Data.Migrations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MWT.Nop.Plugin.Misc.ProductBundle.Data
{
    [SkipMigrationOnUpdate]
    [NopMigration("2026/05/20 00:00:00", "Bundle Configuration")]
    public class SchemaMigration : AutoReversingMigration
    {
        protected IMigrationManager _migrationManager;

        public SchemaMigration(IMigrationManager migrationManager)
        {
            _migrationManager = migrationManager;
        }

        public override void Up()
        {
            if (!Schema.Table(nameof(BundleAuditLogMappingBuilder)).Exists())
            {
                _migrationManager.BuildTable<BundleAuditLogMappingBuilder>(Create);
            }
            if (!Schema.Table(nameof(BundleConfigurationMappingBuilder)).Exists())
            {
                _migrationManager.BuildTable<BundleConfigurationMappingBuilder>(Create);
            }
            if (!Schema.Table(nameof(BundleItemMappingBuilder)).Exists())
            {
                _migrationManager.BuildTable<BundleItemMappingBuilder>(Create);
            }
            if (!Schema.Table(nameof(VariantPriceBackupMappingBuilder)).Exists())
            {
                _migrationManager.BuildTable<VariantPriceBackupMappingBuilder>(Create);
            }
        }
    }
}
