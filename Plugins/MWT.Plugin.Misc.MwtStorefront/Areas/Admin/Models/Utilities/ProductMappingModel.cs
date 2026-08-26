using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nop.Web.Areas.Admin.Models.Customization.Utilities
{
    public record ProductMappingModel : BaseNopEntityModel
    {
        public ProductMappingModel()
        {
            ProductTemplates = new List<SelectListItem>();
            Actions = new List<SelectListItem>();
            CustomizationFormTemplates = new List<SelectListItem>();
            Actions.Add(new SelectListItem()
            {
                Text = "Product Category Department Mapping",
                Value = "Product Category Department Mapping"
            });

            Actions.Add(new SelectListItem()
            {
                Text = "Category Products Publish/Unpublish",
                Value = "Category Products Publish/Unpublish"
            });

            Actions.Add(new SelectListItem()
            {
                Text = "Product Quantity Update",
                Value = "Product Quantity Update"
            });
            Actions.Add(new SelectListItem()
            {
                Text = "Product Template Mapping",
                Value = "Product Template Mapping"
            });
            Actions.Add(new SelectListItem()
            {
                Text = "Product Listing Section",
                Value = "Product Listing Section"
            });
            Actions.Add(new SelectListItem()
            {
                Text = "Customization Form Template",
                Value = "Customization Form Template"
            });

            ProductListingTypes = new List<SelectListItem>

        {
            new SelectListItem { Value = "People Also Viewed", Text = "People Also Viewed" },
            new SelectListItem { Value = "FbtProducts", Text = "FbtProducts" },
            new SelectListItem { Value = "You May Also Like", Text = "You May Also Like" },
            new SelectListItem { Value = "Pick Products You Like", Text = "Pick Products You like" },
            new SelectListItem { Value = "Pair With Products", Text = "Pair With Products" }
        };



        }
        [NopResourceDisplayName("Admin.Utilities.ProductMapping.Field.ProductIds")]
        public string ProductIds { get; set; }
        [NopResourceDisplayName("Admin.Utilities.ProductMapping.Field.CategoryIds")]
        public string CategoryIds { get; set; }
        [NopResourceDisplayName("Admin.Utilities.ProductMapping.Field.SpecificationAttribute")]
        public string SpecificationAttributeIds { get; set; }

        [NopResourceDisplayName("Admin.Utilities.ProductMapping.Field.Published")]
        public bool Published { get; set; }

        [NopResourceDisplayName("Admin.Utilities.ProductMapping.Field.Add.remove")]
        public bool Remove { get; set; }

        [NopResourceDisplayName("Admin.Utilities.ProductMapping.Field.Inventory")]
        public int Inventory { get; set; }

        [NopResourceDisplayName("Admin.Utilities.ProductMapping.Field.ProductId")]
        public int ProductId { get; set; }
        [NopResourceDisplayName("Admin.Utilities.ProductMapping.Field.ProductListingTypes")]
        public string ProductListingType { get; set; }

        public List<SelectListItem> ProductListingTypes { get; set; }


        [NopResourceDisplayName("Admin.Utilities.ProductMapping.Field.Action")]
        public string Action { get; set; }
        public List<SelectListItem> Actions { get; set; }

        public List<SelectListItem> ProductTemplates { get; set; }

        [NopResourceDisplayName("Admin.Utilities.ProductMapping.Field.ProductTemplate")]
        public int ProductTemplateId { get; set; }

        public List<SelectListItem> CustomizationFormTemplates { get; set; }

        [NopResourceDisplayName("Admin.Utilities.ProductMapping.Field.CustomizationFormTemplate")]
        public int CustomizationFormTemplateId { get; set; }
    }
}
