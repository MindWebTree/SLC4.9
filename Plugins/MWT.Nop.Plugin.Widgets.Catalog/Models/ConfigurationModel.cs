using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MWT.Nop.Plugin.Widgets.Catalog.Models
{
    public record ConfigurationModel : BaseSearchModel
    {

        public ConfigurationModel()
        {
            AvailableCategories = new List<SelectListItem>();
        }
        public IList<SelectListItem> AvailableCategories { get; set; }

        [NopResourceDisplayName("MWT.Plugins.Shipping.Catalog.Fields.Search")]
        public string SearchText { get; set; }

        [NopResourceDisplayName("MWT.Plugins.Widgets.Catalog.Fields.Category")]
        public int SearchCategoryId { get; set; }
    }
}
