using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Mvc.ModelBinding;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
namespace MWT.Nop.Plugin.MegaMenu.Areas.Admin.Models
{
    public class StoreMappingModel
    {
        public StoreMappingModel()
        {
            this.SelectedStoreIds = (IList<int>)new List<int>();
            this.AvailableStores = (IList<SelectListItem>)new List<SelectListItem>();
        }

        [NopResourceDisplayName("Admin.Catalog.Categories.Fields.LimitedToStores")]
        [UIHint("MultiSelect")]
        public IList<int> SelectedStoreIds { get; set; }

        public IList<SelectListItem> AvailableStores { get; set; }
    }
}