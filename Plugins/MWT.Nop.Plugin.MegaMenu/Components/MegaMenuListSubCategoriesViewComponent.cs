
using Microsoft.AspNetCore.Mvc;
using MWT.Nop.Plugin.MegaMenu.Models;
using System.Collections.Generic;

namespace MWT.Nop.Plugin.MegaMenu.Components
{
    public class MegaMenuListSubCategoriesViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke(
          IList<MegaMenuCategoryModel> subCategories)
        {
            return View("CategoryMenuTemplate.List.SubCategories", subCategories);
        }
    }
}
