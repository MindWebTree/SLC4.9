using Nop.Core.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MWT.Nop.Core.Services.Catalog
{
    public class TagAutomationSettings : ISettings
    {
        /// <summary>
        /// Name of the New Arrival tag
        /// Default: "New Arrival"
        /// </summary>
        public string NewArrivalTagName { get; set; } = "New Arrival";

        /// <summary>
        /// Name of the Bestseller tag
        /// Default: "Bestseller"
        /// </summary>
        public string BestsellerTagName { get; set; } = "Bestseller";

        /// <summary>
        /// Number of days a product qualifies as New Arrival
        /// Default: 180
        /// </summary>
        public int NewArrivalDays { get; set; } = 180;

        /// <summary>
        /// Number of days to look back for Bestseller order count
        /// Default: 180
        /// </summary>
        public int BestsellerDays { get; set; } = 180;

        /// <summary>
        /// Minimum number of orders required to qualify as Bestseller
        /// Default: 3
        /// </summary>
        public int BestsellerMinOrders { get; set; } = 3;

        /// <summary>
        /// Maximum number of Bestseller products per category
        /// Default: 7
        /// </summary>
        public int MaxBestsellerPerCategory { get; set; } = 7;

        /// <summary>
        /// Maximum number of products per category on the Bestseller page pool
        /// Default: 5
        /// </summary>
        public int MaxBestsellerPoolPerCategory { get; set; } = 5;

        public int BestsellerTagId { get; set; } = 0;

        public string SkipProductIds { get; set; }
    }
}
