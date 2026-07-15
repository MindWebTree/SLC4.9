using Nop.Web.Framework.Mvc.ModelBinding;
using Nop.Web.Framework.Models;
using System;

namespace MWT.Tax.FixedOrByCountryStateZip.Models
{
    public record LogModel : BaseNopEntityModel
    {

        [NopResourceDisplayName("Plugins.MWT.Tax.FixedOrByCountryStateZip.Fields.StatusCode")]
        public int StatusCode { get; set; }

        /// <summary>
        /// Gets or sets the requested URL
        /// </summary>
        [NopResourceDisplayName("Plugins.MWT.Tax.FixedOrByCountryStateZip.Fields.Url")]
        public string Url { get; set; }

        /// <summary>
        /// Gets or sets the request message
        /// </summary>
        [NopResourceDisplayName("Plugins.MWT.Tax.FixedOrByCountryStateZip.Fields.Request")]
        public string RequestMessage { get; set; }

        /// <summary>
        /// Gets or sets the response message
        /// </summary>
        [NopResourceDisplayName("Plugins.MWT.Tax.FixedOrByCountryStateZip.Fields.Response")]
        public string ResponseMessage { get; set; }

        /// <summary>
        /// Gets or sets the customer identifier
        /// </summary>
        [NopResourceDisplayName("Plugins.MWT.Tax.FixedOrByCountryStateZip.Fields.CustomerId")]
        public int CustomerId { get; set; }

        /// <summary>
        /// Gets or sets the date and time of creation
        /// </summary>
        [NopResourceDisplayName("Plugins.MWT.Tax.FixedOrByCountryStateZip.Fields.CreatedDateUtc")]
        public DateTime CreatedDateUtc { get; set; }

        [NopResourceDisplayName("Plugins.MWT.Tax.FixedOrByCountryStateZip.Fields.OrderId")]
        public int OrderId { get; set; }

        [NopResourceDisplayName("Plugins.MWT.Tax.FixedOrByCountryStateZip.Fields.TaxRateInfo")]
        public string TaxRateInfo { get; set; }
    }
}