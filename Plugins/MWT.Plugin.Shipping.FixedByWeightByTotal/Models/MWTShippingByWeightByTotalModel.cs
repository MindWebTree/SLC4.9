using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace MWT.Plugin.Shipping.FixedByWeightByTotal.Models
{
    public record MWTShippingByWeightByTotalModel : BaseNopEntityModel
    {
        public MWTShippingByWeightByTotalModel()
        {
            AvailableCountries = new List<SelectListItem>();
            AvailableStates = new List<SelectListItem>();
            AvailableShippingMethods = new List<SelectListItem>();
            AvailableStores = new List<SelectListItem>();
            AvailableWarehouses = new List<SelectListItem>();
            AvailableZones= new List<SelectListItem>();
        }

        [NopResourceDisplayName("MWT.Plugins.Shipping.FixedByWeightByTotal.Fields.Store")]
        public int StoreId { get; set; }
        [NopResourceDisplayName("MWT.Plugins.Shipping.FixedByWeightByTotal.Fields.Store")]
        public string StoreName { get; set; }

        [NopResourceDisplayName("MWT.Plugins.Shipping.FixedByWeightByTotal.Fields.Warehouse")]
        public int WarehouseId { get; set; }
        [NopResourceDisplayName("MWT.Plugins.Shipping.FixedByWeightByTotal.Fields.Warehouse")]
        public string WarehouseName { get; set; }

        [NopResourceDisplayName("MWT.Plugins.Shipping.FixedByWeightByTotal.Fields.Country")]
        public int CountryId { get; set; }
        [NopResourceDisplayName("MWT.Plugins.Shipping.FixedByWeightByTotal.Fields.Country")]
        public string CountryName { get; set; }

        [NopResourceDisplayName("MWT.Plugins.Shipping.FixedByWeightByTotal.Fields.StateProvince")]
        public int StateProvinceId { get; set; }
        [NopResourceDisplayName("MWT.Plugins.Shipping.FixedByWeightByTotal.Fields.StateProvince")]
        public string StateProvinceName { get; set; }
         
        [NopResourceDisplayName("MWT.Plugins.Shipping.FixedByWeightByTotal.Fields.ZoneId")]
        public int ZoneId { get; set; }


        [NopResourceDisplayName("MWT.Plugins.Shipping.FixedByWeightByTotal.Fields.ZoneName")]
        public string ZoneName { get; set; }

        [NopResourceDisplayName("MWT.Plugins.Shipping.FixedByWeightByTotal.Fields.ShippingMethod")]
        public int ShippingMethodId { get; set; }
        [NopResourceDisplayName("MWT.Plugins.Shipping.FixedByWeightByTotal.Fields.ShippingMethod")]
        public string ShippingMethodName { get; set; }

        [UIHint("Int32Nullable")]
        [NopResourceDisplayName("MWT.Plugins.Shipping.FixedByWeightByTotal.Fields.TransitDays")]
        public int? TransitDays { get; set; }

        [NopResourceDisplayName("MWT.Plugins.Shipping.FixedByWeightByTotal.Fields.WeightFrom")]
        public decimal WeightFrom { get; set; }

        [NopResourceDisplayName("MWT.Plugins.Shipping.FixedByWeightByTotal.Fields.WeightTo")]
        public decimal WeightTo { get; set; }

        [NopResourceDisplayName("MWT.Plugins.Shipping.FixedByWeightByTotal.Fields.OrderSubtotalFrom")]
        public decimal OrderSubtotalFrom { get; set; }

        [NopResourceDisplayName("MWT.Plugins.Shipping.FixedByWeightByTotal.Fields.OrderSubtotalTo")]
        public decimal OrderSubtotalTo { get; set; }

        [NopResourceDisplayName("MWT.Plugins.Shipping.FixedByWeightByTotal.Fields.AdditionalFixedCost")]
        public decimal AdditionalFixedCost { get; set; }

        [NopResourceDisplayName("MWT.Plugins.Shipping.FixedByWeightByTotal.Fields.Amount")]
        public decimal Amount { get; set; }

        [NopResourceDisplayName("MWT.Plugins.Shipping.FixedByWeightByTotal.Fields.PercentageRateOfSubtotal")]
        public decimal PercentageRateOfSubtotal { get; set; }


        [NopResourceDisplayName("MWT.Plugins.Shipping.FixedByWeightByTotal.Fields.WgsSurcharge")]
        public decimal Surcharge { get; set; }

        [NopResourceDisplayName("MWT.Plugins.Shipping.FixedByWeightByTotal.Fields.RatePerWeightUnit")]
        public decimal RatePerWeightUnit { get; set; }

        [NopResourceDisplayName("MWT.Plugins.Shipping.FixedByWeightByTotal.Fields.LowerWeightLimit")]
        public decimal LowerWeightLimit { get; set; }

        [NopResourceDisplayName("MWT.Plugins.Shipping.FixedByWeightByTotal.Fields.DataHtml")]
        public string DataHtml { get; set; }

        public string PrimaryStoreCurrencyCode { get; set; }
        public string BaseWeightIn { get; set; }

        public IList<SelectListItem> AvailableCountries { get; set; }
        public IList<SelectListItem> AvailableStates { get; set; }
        public IList<SelectListItem> AvailableShippingMethods { get; set; }
        public IList<SelectListItem> AvailableStores { get; set; }
        public IList<SelectListItem> AvailableWarehouses { get; set; }
        public IList<SelectListItem> AvailableZones { get; set; }
    }
}