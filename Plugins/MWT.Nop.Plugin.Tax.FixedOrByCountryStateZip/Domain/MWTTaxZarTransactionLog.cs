using Nop.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MWT.Tax.FixedOrByCountryStateZip.Domain
{
    public class MWTTaxZarTransactionLog : BaseEntity
    {
        /// <summary>
        /// Gets or sets the response status code
        /// </summary>
        public int StatusCode { get; set; }

        /// <summary>
        /// Gets or sets the requested URL
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// Gets or sets the request message
        /// </summary>
        public string RequestMessage { get; set; }

        /// <summary>
        /// Gets or sets the response message
        /// </summary>
        public string ResponseMessage { get; set; }

        /// <summary>
        /// Gets or sets the customer identifier
        /// </summary>
        public int CustomerId { get; set; }

        /// <summary>
        /// Gets or sets the date and time of creation
        /// </summary>
        public DateTime CreatedDateUtc { get; set; }

        public int OrderId { get; set; }

        public string TaxRateInfo { get; set; }
    }
}
