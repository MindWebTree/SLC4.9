using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Areas.Admin.Factories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MWT.Plugin.Misc.MwtStorefront.Area.Admin.Factories
{
    public interface ICustomBaseAdminModelFactory : IBaseAdminModelFactory
    {
        Task PrepareKWTemplatesAsync(IList<SelectListItem> items, bool withSpecialDefaultItem = true, string defaultItemText = null);

        Task PrepareQuestionAnswerTemplatesAsync(IList<SelectListItem> items, bool withSpecialDefaultItem = true, string defaultItemText = null);

    }
}
