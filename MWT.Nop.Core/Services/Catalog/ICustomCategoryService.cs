using Nop.Services.Catalog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MWT.Nop.Core.Services.Catalog
{
    public partial interface ICustomCategoryService : ICategoryService
    {

        ValueTask<bool> IsMwtWidgetApplied(string widget, int entityId, string entityType, bool isMobileDevice);

    }
}
