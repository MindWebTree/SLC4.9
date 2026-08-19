using Nop.Web.Framework.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MWT.Nop.Plugin.Misc.ProductBundle.Models.BundleLogs
{
    public record BundleAuditLogModel : BaseNopEntityModel
    {
        public int BundleId { get; set; }
        public int ProductId { get; set; }
        public int? VariantId { get; set; }
        public string ActionType { get; set; }
        public string UserIdentifier { get; set; }
        public decimal? OldPrice { get; set; }
        public decimal? NewPrice { get; set; }
        public bool IsValid { get; set; }
        public int RequiredPieces { get; set; }
        public int ActualQuantity { get; set; }
        public string LogMessage { get; set; }
        public DateTime CreatedOnUtc { get; set; }
    }
}
