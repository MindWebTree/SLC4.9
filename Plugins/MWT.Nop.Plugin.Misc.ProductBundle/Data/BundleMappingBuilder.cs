using FluentMigrator.Builders.Create.Table;
using MWT.Nop.Plugin.Misc.ProductBundle.Domain;
using Nop.Data.Mapping.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MWT.Nop.Plugin.Misc.ProductBundle.Data
{
    public class BundleAuditLogMappingBuilder : NopEntityBuilder<BundleAuditLog>
    {
        public override void MapEntity(CreateTableExpressionBuilder table)
        {
            table
                .WithColumn(nameof(BundleAuditLog.ActionType))
                    .AsString(200)
               .WithColumn(nameof(BundleAuditLog.LogMessage))
                    .AsCustom("nVARCHAR(MAX)");


        }
    }

    public class BundleConfigurationMappingBuilder : NopEntityBuilder<BundleConfiguration>
    {
        public override void MapEntity(CreateTableExpressionBuilder table)
        {
            table
                .WithColumn(nameof(BundleConfiguration.IsActive))
                    .AsBoolean().NotNullable().WithDefaultValue(true);
        }
    }
    public class BundleItemMappingBuilder : NopEntityBuilder<BundleItem>
    {
        public override void MapEntity(CreateTableExpressionBuilder table)
        {
            table
                .WithColumn(nameof(BundleItem.IsActive))
                    .AsBoolean().NotNullable().WithDefaultValue(true);
        }
    }
    public class VariantPriceBackupMappingBuilder : NopEntityBuilder<VariantPriceBackup>
    {
        public override void MapEntity(CreateTableExpressionBuilder table)
        {
           
        }
    }
}
