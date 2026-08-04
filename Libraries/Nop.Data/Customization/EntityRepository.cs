using LinqToDB.Data;
using Nop.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nop.Data;


public partial class EntityRepository<TEntity> : IRepository<TEntity> where TEntity : BaseEntity
{
    public virtual async Task<IList<TEntity>> EntityFromSqlAsync(string procedureName, params DataParameter[] parameters)
    {
        return await _dataProvider.QueryProcAsync<TEntity>(procedureName, parameters?.ToArray());
    }

}

