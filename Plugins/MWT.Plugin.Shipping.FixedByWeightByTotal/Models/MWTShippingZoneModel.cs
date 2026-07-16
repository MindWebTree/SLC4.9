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

    public record MWTShippingZoneModel : BaseNopEntityModel
    {
        [NopResourceDisplayName("MWT.Plugins.Shipping.FixedByWeightByTotal.Fields.Store")]
        public int StoreId { get; set; }

        [NopResourceDisplayName("MWT.Plugins.Shipping.FixedByWeightByTotal.Fields.Name")]
        public string Name { get; set; }

        [NopResourceDisplayName("MWT.Plugins.Shipping.FixedByWeightByTotal.Fields.ZipCodes")]
        public string ZipCodes { get; set; }

        [NopResourceDisplayName("MWT.Plugins.Shipping.FixedByWeightByTotal.Fields.Store")]
        public string StoreName { get; set; }
    }
}
