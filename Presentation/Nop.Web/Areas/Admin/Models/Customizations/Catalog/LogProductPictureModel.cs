using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;
using System;
using System.ComponentModel.DataAnnotations;

namespace Nop.Web.Areas.Admin.Models.Customization.Catalog
{
    public partial record LogProductPictureModel : BaseNopEntityModel
    {
        public int ProductId { get; set; }


        [NopResourceDisplayName("Admin.Catalog.Products.Pictures.Fields.Picture")]
        public int PictureId { get; set; }

        [NopResourceDisplayName("Admin.Catalog.Products.Pictures.Fields.Picture")]
        public string PictureUrl { get; set; }

        [NopResourceDisplayName("Admin.Catalog.Products.Pictures.Fields.DisplayOrder")]
        public int DisplayOrder { get; set; }

        [NopResourceDisplayName("Admin.Catalog.Products.Pictures.Fields.OverrideAltAttribute")]
        public string OverrideAltAttribute { get; set; }

        [NopResourceDisplayName("Admin.Catalog.Products.Pictures.Fields.OverrideTitleAttribute")]
        public string OverrideTitleAttribute { get; set; }

        [NopResourceDisplayName("Admin.Catalog.Products.Pictures.Fields.DisplayOnListingModules")]
        public bool DisplayOnListingModules { get; set; }

        [NopResourceDisplayName("Admin.Catalog.Products.Pictures.Fields.DisplayOnCategoryPage")]
        public bool DisplayOnCategoryPage { get; set; }

        [NopResourceDisplayName("Admin.Catalog.Products.Pictures.Fields.HideOnProductPage")]
        public bool HideOnProductPage { get; set; }

        [NopResourceDisplayName("Admin.Catalog.Products.Pictures.Fields.UserName")]
        public string UserName { get; set; }
        [NopResourceDisplayName("Admin.Catalog.Products.Pictures.Fields.Action")]
        public string Action { get; set; }

        [NopResourceDisplayName("Admin.Catalog.Products.Pictures.Fields.CreatedOn")]
        public DateTime CreatedOn { get; set; }
    }
}
