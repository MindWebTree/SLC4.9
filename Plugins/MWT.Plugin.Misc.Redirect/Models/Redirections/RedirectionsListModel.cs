using Nop.Web.Framework.Models;

namespace Nop.Plugin.Misc.Redirect.Models.Redirections
{
    /// <summary>
    /// Represents a tax transaction log list model
    /// </summary>
    public record RedirectionsListModel : BasePagedListModel<RedirectionModel>    
    {
    }
}