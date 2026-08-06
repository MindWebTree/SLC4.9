using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MWT.Nop.Core.Services.ElasticSearch
{
    public partial interface IFuzzySearchService
    {
        Task<List<(int categoryId, double score)>> SearchCategoryGenricKeyWords(string searchTerm);

        Task<List<(int categoryId, double score)>> SearchCategories(string searchTerm);
    }
}
