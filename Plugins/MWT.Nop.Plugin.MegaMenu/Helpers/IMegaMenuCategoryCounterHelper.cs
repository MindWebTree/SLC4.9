using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MWT.Nop.Plugin.MegaMenu.Helpers
{
    public interface IMegaMenuCategoryCounterHelper
    {
        int GetCategoryCount(int menuId);
    }
}
