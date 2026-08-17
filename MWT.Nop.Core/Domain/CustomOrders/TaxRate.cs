using Nop.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MWT.Nop.Core.Domain.CustomOrders
{
    public partial class TaxRate : BaseEntity
    {
        public int? StoreId { get; set; }
        public int? TaxCategoryId { get; set; }
        public int CountryId { get; set; }
        public int StateProvinceId { get; set; }

        public string Zip { get; set; }
        public decimal Percentage { get; set; }
    }
}
