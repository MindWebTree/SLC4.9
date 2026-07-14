using FluentMigrator.Builders.Create.Table;
using Nop.Data.Mapping.Builders;
using Nop.Plugin.Misc.Redirect.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MWT.Plugin.Misc.Redirect.Data.Mapping.Builder
{
    public class RedirectionRuleBuilder : NopEntityBuilder<RedirectionRule>
    {
        /// <summary>
        /// Apply entity configuration
        /// </summary>
        /// <param name="table">Create table expression builder</param>
        public override void MapEntity(CreateTableExpressionBuilder table)
        {
            table.WithColumn(nameof(RedirectionRule.Pattern)).AsString(400).NotNullable();
            table.WithColumn(nameof(RedirectionRule.RedirectUrl)).AsString(400).NotNullable();
            table.WithColumn(nameof(RedirectionRule.Type)).AsInt32().NotNullable();
            table.WithColumn(nameof(RedirectionRule.StoreId)).AsInt32().NotNullable();
            table.WithColumn(nameof(RedirectionRule.UseQueryString)).AsBoolean().NotNullable();
            table.WithColumn(nameof(RedirectionRule.IsPermanent)).AsBoolean().NotNullable();
        }
    }
}
