using Nop.Web.Framework.Models;

namespace Nop.Web.Areas.Admin.Models.Customization.Catalog
{
    public partial record VariantSearchModel:BaseSearchModel
    {
        public int ProductId { get; set; }
    }
}
