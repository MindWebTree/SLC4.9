using Nop.Core.Domain.Customization.Custom;
using Nop.Web.Framework.Models;

namespace Nop.Web.Areas.Admin.Models.Customization.Custom
{
    /// <summary>
    /// Represents a product tag list model
    /// </summary>
    public partial record CustomFormEntryMetaListModel : BasePagedListModel<CustomFormEntryMetaModel>
    {
    }
}