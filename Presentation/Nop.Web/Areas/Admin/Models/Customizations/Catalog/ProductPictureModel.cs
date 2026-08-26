using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Web.Areas.Admin.Models.Catalog
{
    public partial record ProductPictureModel : BaseNopEntityModel
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
