using Nop.Web.Framework.Models;

namespace Nop.Web.Areas.Admin.Models.Customization.Custom.KW
{
    /// <summary>
    /// Represents a category list model
    /// </summary>
    public partial record KwTermListModel : BasePagedListModel<KwTermModel>
    {
    }
}