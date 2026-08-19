using Nop.Web.Framework.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MWT.Nop.Plugin.Misc.ProductBundle.Models
{
    public partial record ProductAttributeMappingBundleSearchModel : BaseSearchModel
    {
        #region Properties 
        public int ProductId { get; set; } 
        #endregion
    }
}
