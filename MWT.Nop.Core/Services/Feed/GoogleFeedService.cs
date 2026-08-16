using LinqToDB.Data;
using MWT.Nop.Core.Domain.Feed;
using Nop.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MWT.Nop.Core.Services.Feed
{
    public partial class GoogleFeedService : IGoogleFeedService
    {
        #region Fields

        private readonly IRepository<GoogleCategory> _googleCategoryrepository;

        #endregion

        #region Ctor

        public GoogleFeedService(IRepository<GoogleCategory> googleCategoryrepository)
        {
            this._googleCategoryrepository = googleCategoryrepository;
        }

        #endregion


        #region Methods
        public async Task<GoogleCategory> GetFeedCategoryAsync(int categoryId, string productType)
        {
            return (await _googleCategoryrepository.EntityFromSqlAsync("uspgetDetailOverCategoryId",
                      new DataParameter()
                      {
                          DataType = LinqToDB.DataType.Int32,
                          Value = categoryId,
                          Direction = System.Data.ParameterDirection.Input,
                          Name = "@CategoryId",

                      },
                     new DataParameter()
                     {
                         DataType = LinqToDB.DataType.VarChar,
                         Value = productType,
                         Direction = System.Data.ParameterDirection.Input,
                         Name = "@ProductType",

                     })).FirstOrDefault();
        }

        #endregion
    }
}
