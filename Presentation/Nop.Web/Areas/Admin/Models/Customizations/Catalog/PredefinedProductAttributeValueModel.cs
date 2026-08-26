using Nop.Web.Framework.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nop.Web.Areas.Admin.Models.Catalog
{

    public partial record PredefinedProductAttributeValueModel : BaseNopEntityModel, ILocalizedModel<PredefinedProductAttributeValueLocalizedModel>
    {
        #region Properties
        public string Image { get; set; }

        #endregion
    }
}
