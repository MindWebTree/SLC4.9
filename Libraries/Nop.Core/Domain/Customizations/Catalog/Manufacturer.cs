using System;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Discounts;
using Nop.Core.Domain.Localization;
using Nop.Core.Domain.Security;
using Nop.Core.Domain.Seo;
using Nop.Core.Domain.Stores;


namespace Nop.Core.Domain.Catalog
{
    /// <summary>
    /// Represents a manufacturer
    /// </summary>
    public partial class Manufacturer : BaseEntity, ILocalizedEntity, ISlugSupported, IAclSupported, IStoreMappingSupported, IDiscountSupported<DiscountManufacturerMapping>, ISoftDeletedEntity
    {
        public string Country { get; set; }
    }
}

