using Nop.Web.Framework.Models;
using Nop.Web.Models.Catalog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MWT.Plugin.Misc.MwtStorefront.Models.Catalog
{
    public record class CustomSpecificationFilterModel : BaseNopModel
    {
     #region Properties

        /// <summary>
        /// Gets or sets a value indicating whether filtering is enabled
        /// </summary>
        public bool Enabled { get; set; }

        /// <summary>
        /// Gets or sets the filtrable specification attributes
        /// </summary>
        public IList<CustomSpecificationAttributeFilterModel> Attributes { get; set; }

        #endregion

        #region Ctor

        public CustomSpecificationFilterModel()
        {
            Attributes = new List<CustomSpecificationAttributeFilterModel>();
        }

        #endregion
    }
}
