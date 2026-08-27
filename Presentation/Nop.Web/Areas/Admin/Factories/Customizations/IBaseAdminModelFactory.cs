using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models.Translation;

namespace Nop.Web.Areas.Admin.Factories;

/// <summary>
/// Represents the base model factory that implements a most common admin model factories methods
/// </summary>
public partial interface IBaseAdminModelFactory
{
    Task PrepareKWTemplatesAsync(IList<SelectListItem> items, bool withSpecialDefaultItem = true, string defaultItemText = null);

}