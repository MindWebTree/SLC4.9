using Nop.Web.Framework.Menu;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MWT.Plugin.Misc.MwtStorefront.Areas.CustomOrder.Services
{
    public interface ICustomOrderMenuService
    {
        Task<IList<AdminMenuItem>> GetMenuItemsAsync();
    }
}
