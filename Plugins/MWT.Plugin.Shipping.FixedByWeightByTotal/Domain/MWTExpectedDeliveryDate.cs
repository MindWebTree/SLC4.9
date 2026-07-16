

using Nop.Core;
using System;

namespace MWT.Plugin.Shipping.FixedByWeightByTotal.Domain
{
    /// <summary>
    /// Represents a shipping by weight record
    /// </summary>
    public partial class MWTExpectedDeliveryDate : BaseEntity
    {
        /// <summary>
        /// Gets or sets the store identifier
        /// </summary>
        public int ZoneID { get; set; }

        /// <summary>
        /// Gets or sets the warehouse identifier
        /// </summary>
        public int ExpectedMinNoOfDays { get; set; }

        /// <summary>
        /// Gets or sets the country identifier
        /// </summary>
        public int ExpectedMaxNoOfDays { get; set; }

        public  DateTime CreatedOnUtc { get; set; }
        public DateTime UpdatedOnUtc { get; set; }
    }
}