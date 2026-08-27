

using Nop.Web.Framework.Models;
using System.Collections.Generic;

namespace Nop.Web.Areas.Admin.Models.Customization.Custom
{
    public partial record GroupedProductConfigurationModel: BaseNopEntityModel
    {
        public GroupedProductConfigurationModel()
        {
            Configurations = new List<GrpConfiguration>();
        }
        public int ProductAttributeOptionId { get; set; }
        public string ProductAttributeOptionName { get; set; }
        public int ProductId { get; set; }
        public List<GrpConfiguration> Configurations { get; set; }
    }

    public class GrpConfiguration
    {
        public string Sku { get; set; }
        public int ProductId { get; set; }
        public int Quantity { get; set; }
    }
}

