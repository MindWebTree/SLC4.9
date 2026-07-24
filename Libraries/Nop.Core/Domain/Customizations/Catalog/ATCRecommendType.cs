using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nop.Core.Domain.Customization.Catalog
{
    public enum ATCRecommendType
    {
        /// <summary>
        /// None
        /// </summary>
        None = 0,
        /// <summary>
        /// SimilarItems
        /// </summary>
        SimilarItems = 1,

        /// <summary>
        /// Collection Items
        /// </summary>
        Collection = 2,
        /// <summary>
        /// Other input product 
        /// </summary>
        Other = 3,
    }
}
