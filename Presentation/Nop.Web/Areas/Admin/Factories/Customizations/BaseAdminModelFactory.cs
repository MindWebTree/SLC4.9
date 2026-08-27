using Microsoft.AspNetCore.Mvc.Rendering;
using MWT.Nop.Core.Services.KW;
using Nop.Core.Infrastructure;

namespace Nop.Web.Areas.Admin.Factories;

/// <summary>
/// Represents the implementation of the base model factory that implements a most common admin model factories methods
/// </summary>
public partial class BaseAdminModelFactory : IBaseAdminModelFactory
{

    public virtual async Task PrepareKWTemplatesAsync(IList<SelectListItem> items, bool withSpecialDefaultItem = true, string defaultItemText = null)
    {
        if (items == null)
            throw new ArgumentNullException(nameof(items));
        
        //prepare available category templates
        var _kwTemplatetService = EngineContext.Current.Resolve<IKwTemplateService>();
        var availableTemplates = await _kwTemplatetService.GetAllKwTemplatesAsync();
        foreach (var template in availableTemplates)
        {
            items.Add(new SelectListItem { Value = template.Id.ToString(), Text = template.Name });
        }

        //insert special item for the default value
        await PrepareDefaultItemAsync(items, withSpecialDefaultItem, defaultItemText);
    }
}