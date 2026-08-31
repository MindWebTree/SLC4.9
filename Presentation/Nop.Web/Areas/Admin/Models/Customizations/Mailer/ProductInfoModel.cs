using iTextSharp.text;

using Nop.Web.Framework.Models;
using Nop.Web.Models.Catalog;
using System.Collections.Generic;

namespace Nop.Web.Areas.Models.Customizations.Mailer
{
    public partial record ProductInfoModel : BaseNopEntityModel
    {
        public ProductInfoModel()
        {
            Products = new List<ProductOverviewModel>();
        }
        public string ModuleType { get; set; }
        public List<ProductOverviewModel> Products { get; set; }

    }
}

