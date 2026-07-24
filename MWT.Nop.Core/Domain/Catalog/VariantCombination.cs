using Nop.Core;
using Nop.Core.Domain.Seo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MWTNop.Core.Domain.Catalog
{
    public class VariantCombination : BaseEntity
    {
        public int VariantId { get; set; }
        public int ProductId { get; set; }
        public string Combination { get; set; }
        public decimal? OldPrice { get; set; }
        public decimal? Msrp { get; set; }
        public decimal Price { get; set; }
        public DateTime? CreatedOn { get; set; }
        public DateTime? UpdatedOn { get; set; }
        public string ProductAttributeValueIds { get; set; }
        public string Title { get; set; }
        public bool WgsRequired { get; set; }
        public bool EnableSurcharge { get; set; }
        public string QueryParameter { get; set; }
        public decimal Weight { get; set; }
        public string Dimension { get; set; }
        public string ManufacturerPartNumber { get; set; }
        public string EstimatedDeliveryDate { get; set; }
        public int DimensionPictureId { get; set; }
        public string SeName { get; set; }
    }
}