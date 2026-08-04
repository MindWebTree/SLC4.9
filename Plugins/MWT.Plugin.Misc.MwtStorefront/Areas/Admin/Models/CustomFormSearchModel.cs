
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace MWT.Plugin.Misc.MwtStorefront.Areas.Admin.Models
{
    /// <summary>
    /// Represents a product tag search model
    /// </summary>
    public partial record CustomFormSearchModel : BaseSearchModel
    {
        [NopResourceDisplayName("Admin.Catalog.CustomForm.Fields.SearchByName")]
        public string Name { get; set; }
        public int Id { get; set; }
    }
}