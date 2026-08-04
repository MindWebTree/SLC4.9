using LinqToDB.Data;
using Nop.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nop.Data;

/// <summary>
/// Represents an entity repository
/// </summary>
/// <typeparam name="TEntity">Entity type</typeparam>
public partial interface IRepository<TEntity> where TEntity : BaseEntity
{
    Task<IList<TEntity>> EntityFromSqlAsync(string procedureName, params DataParameter[] parameters);
}

