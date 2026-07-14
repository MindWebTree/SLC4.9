using FluentMigrator;
using Nop.Data.Migrations;
using Nop.Data.Extensions;
using Nop.Plugin.Misc.Redirect.Domain;


namespace Nop.Plugin.Misc.Redirect.Data
{
    [NopMigration(
     "2026/07/14 09:09:17",
     "MWT.Plugin.Misc.Redirect base schema",
     MigrationProcessType.Installation)]
    public class SchemaMigration : AutoReversingMigration
    { 
        public override void Up()
        {
            if (!Schema.Table(nameof(RedirectionRule)).Exists())
            {
                Create.TableFor<RedirectionRule>();
            }
            else
            {
                // Table exists → check and add new column if missing
                if (!Schema.Table(nameof(RedirectionRule)).Column(nameof(RedirectionRule.IsPermanent)).Exists())
                {
                    Alter.Table(nameof(RedirectionRule))
                        .AddColumn(nameof(RedirectionRule.IsPermanent)).AsBoolean().Nullable();
                }
            }
        }
    }
}