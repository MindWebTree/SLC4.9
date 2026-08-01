using Nop.Web.Areas.Admin.Models.Catalog;
using Nop.Web.Framework.Mvc.ModelBinding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MWT.Plugin.Misc.MwtStorefront.Area.Admin.Models.Catalog
{
    public record class CustomProductPictureModel : ProductPictureModel
    {
        [NopResourceDisplayName("Admin.Catalog.Products.Pictures.Fields.DisplayOnListingModules")]
        public bool DisplayOnListingModules { get; set; }

        [NopResourceDisplayName("Admin.Catalog.Products.Pictures.Fields.DisplayOnCategoryPage")]
        public bool DisplayOnCategoryPage { get; set; }

        [NopResourceDisplayName("Admin.Catalog.Products.Pictures.Fields.HideOnProductPage")]
        public bool HideOnProductPage { get; set; }


        [NopResourceDisplayName("Admin.Catalog.Products.Pictures.Fields.IsDimention")]
        public bool IsDimensionImage { get; set; }
    }
}
