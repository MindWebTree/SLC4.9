using Nop.Core;
using System;

namespace MWT.Plugin.Shipping.FixedByWeightByTotal.Domain
{
    /// <summary>
    /// Represents a shipping by weight record
    /// </summary>
    public partial class MWTExpectedDeliveryDate_Entity_Mapping : BaseEntity
    {
        /// <summary>
        /// Gets or sets the store identifier
        /// </summary>
        public int MWTExpectedDeliveryDateID { get; set; }

        /// <summary>
        /// Gets or sets the warehouse identifier
        /// </summary>
        public string EntityType { get; set; }

        /// <summary>
        /// Gets or sets the country identifier
        /// </summary>
        public int EntityID { get; set; }

        public DateTime CreatedOnUtc { get; set; }
        public DateTime UpdatedOnUtc { get; set; }

    }
}