using FluentMigrator;
using MWT.Nop.Plugin.MegaMenu.Domain;
using Nop.Data.Extensions;
using Nop.Data.Migrations;
using System.Collections.Generic;

namespace MWT.Nop.Plugin.MegaMenu.Data.Migrations
{
    [NopMigration("2020/03/26 09:00:00:0000000", "Mega Menu base schema", MigrationProcessType.Installation)]
    public class SchemaMigration : AutoReversingMigration
    {

            
        public override void Up()
        {
            // Create tables using the native NopCommerce 4.90 builder
            Create.TableFor<EntityMapping>();
            Create.TableFor<EntityWidgetMapping>();
            Create.TableFor<Menu>();
            Create.TableFor<MenuItem>();

            if (!Schema.Table(nameof(EntityMapping)).Exists())
            {
                Create.TableFor<EntityMapping>();
            }
            if (!Schema.Table(nameof(EntityWidgetMapping)).Exists())
            {
                Create.TableFor<EntityWidgetMapping>();
            }
            if (!Schema.Table(nameof(Menu)).Exists())
            {
                Create.TableFor<Menu>();
            }
            if (!Schema.Table(nameof(MenuItem)).Exists())
            {
                Create.TableFor<MenuItem>();
            }



            Create.ForeignKey("FK_MWT_MM_MenuItem_MWT_MM_Menu_MenuId")
                .FromTable("MWT_MM_MenuItem").ForeignColumn("MenuId")
                .ToTable("MWT_MM_Menu").PrimaryColumn("Id")
                .OnDelete(System.Data.Rule.Cascade);
        }
         
    }
}
