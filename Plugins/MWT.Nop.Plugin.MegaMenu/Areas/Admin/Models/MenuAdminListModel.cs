using Nop.Web.Framework.Models;
using System.Text;

namespace MWT.Nop.Plugin.MegaMenu.Areas.Admin.Models
{
    public record MenuAdminListModel : BasePagedListModel<MenuAdminModel>
    {  
        public MenuAdminListModel()
        {
        }
    }
}
