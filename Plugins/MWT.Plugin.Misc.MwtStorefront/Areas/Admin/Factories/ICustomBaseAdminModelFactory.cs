using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Areas.Admin.Factories;

namespace MWT.Plugin.Misc.MwtStorefront.Area.Admin.Factories
{
    public interface ICustomBaseAdminModelFactory : IBaseAdminModelFactory
    {
      
        Task PrepareQuestionAnswerTemplatesAsync(IList<SelectListItem> items, bool withSpecialDefaultItem = true, string defaultItemText = null);

    }
}
