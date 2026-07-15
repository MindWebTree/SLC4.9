using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nop.Services.Tax
{
    /// <summary>
    /// Represents a result of tax total calculation
    /// </summary>
    public partial class TaxTotalResult
    {
        public List<TaxInfo> Taxes { get; set; }
        public bool TaxRetrieved { get; set; }
    }
    public class TaxInfo
    {
        public decimal TaxRate { get; set; }
        public TaxType TaxType { get; set; }
        public decimal Amount { get; set; }
    }

    public enum TaxType
    {
        Tax,
        GST,
        PST,
        QST,
        HST
    }
}