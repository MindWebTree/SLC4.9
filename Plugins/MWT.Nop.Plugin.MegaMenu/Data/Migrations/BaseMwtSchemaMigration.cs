//using FluentMigrator;
//using Nop.Data.Migrations;
//using System.Collections.Generic;

//namespace MWT.Nop.Plugin.MegaMenu.Data.Migrations
//{
//    public abstract class BaseMwtSchemaMigration : Migration
//    {
//        protected IMigrationManager _migrationManager;

//        public BaseMwtSchemaMigration(IMigrationManager migrationManager) => this._migrationManager = migrationManager;

//        protected virtual void CreateOrUpdateTableIfExist<TEntityType>(
//          string tableName,
//          params ForeignKeyInfo[] foreignKeys)
//        {
//            if (!this.Schema.Table(tableName).Exists())
//            {
//                this._migrationManager.BuildTable<TEntityType>(this.Create);
//            }
//            else
//            {
//                foreach (ForeignKeyInfo foreignKey in foreignKeys)
//                {
//                    foreach (string oldForeignKeyName in (IEnumerable<string>)foreignKey.OldForeignKeyNames)
//                    {
//                        if (this.Schema.Table(foreignKey.FromTable).Constraint(oldForeignKeyName).Exists())
//                            this.Delete.ForeignKey(oldForeignKeyName).OnTable(foreignKey.FromTable);
//                    }
//                    if (foreignKey.CreateNew)
//                        this.Create.ForeignKey().FromTable(foreignKey.FromTable).ForeignColumn(foreignKey.ForeignColumn).ToTable(foreignKey.ToTable).PrimaryColumn(foreignKey.PrimaryColumn).OnDelete(foreignKey.OnDelete);
//                }
//            }
//        }

//        protected virtual void DeleteTablesIfExist(params string[] tableNames)
//        {
//            foreach (string tableName in tableNames)
//            {
//                if (this.Schema.Table(tableName).Exists())
//                    this.Delete.Table(tableName);
//            }
//        }
//    }
//}
