using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MWT.Nop.Core.Services.ElasticSearch
{
    public partial interface IElasticSearchService
    {
        Task<string> InsertDataToElasticSearch(string json, string index, string ID);
        Task<string> DeleteEntity(string json, string index, string ID);
    }
}
