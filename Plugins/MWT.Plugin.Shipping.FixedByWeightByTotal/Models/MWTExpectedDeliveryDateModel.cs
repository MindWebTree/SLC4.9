using Microsoft.AspNetCore.Mvc.Rendering;
using MWT.Plugin.Shipping.FixedByWeightByTotal.Validators;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace MWT.Plugin.Shipping.FixedByWeightByTotal.Models
{
    public record MWTExpectedDeliveryDateModel : BaseNopEntityModel
    {
        public MWTExpectedDeliveryDateModel()
        {
            AvailableCategories = new List<SelectListItem>();
            AvailableZones = new List<SelectListItem>();
            SelectedCategoryIds = new List<int>();
            SelectedProductIds = new List<int>();
        }
        [NopResourceDisplayName("MWT.Plugins.Shipping.MWTExpectedDeliveryDate.Fields.Zone")]
        public int ZoneID { get; set; }

        public string ZoneName { get; set; }

        [NopResourceDisplayName("MWT.Plugins.Shipping.MWTExpectedDeliveryDate.Fields.ExpectedMinNoOfDays")]
        public int ExpectedMinNoOfDays { get; set; }

        [NopResourceDisplayName("MWT.Plugins.Shipping.MWTExpectedDeliveryDate.Fields.ExpectedMaxNoOfDays")]
        public int ExpectedMaxNoOfDays { get; set; }

        [NopResourceDisplayName("MWT.Plugins.Shipping.MWTExpectedDeliveryDate.Fields.Products")]
        public string  ProductIDs { get; set; }
        public string CategoryIds { get; set; }
        public IList<int> SelectedProductIds { get; set; }

        [NopResourceDisplayName("MWT.Plugins.Shipping.MWTExpectedDeliveryDate.Fields.Categories")]
        public IList<int> SelectedCategoryIds { get; set; }

        public IList<SelectListItem> AvailableCategories { get; set; }

        public IList<SelectListItem> AvailableZones { get; set; }
    }
}
