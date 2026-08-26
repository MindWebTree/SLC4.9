using Nop.Web.Framework.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nop.Web.Areas.Admin.Models.Customization.Catalog
{
    public partial record ProductDimensionPictureModel:BaseNopModel
    {
        public string Url { get; set; }
        public DateTime CreatedOn { get; set; }
    }
}
