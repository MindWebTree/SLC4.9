using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MWT.Plugin.Misc.MwtStorefront.Models.Catalog
{
    public partial record CustomizationFormDataModel
    {
        public string FullName { get; set; }

        public string Email { get; set; }

        public string Phone { get; set; }

        public string ZipCode { get; set; }

        public int ProductId { get; set; }

        public string ProductName { get; set; }

        public string Dimensions { get; set; }

        public string Message { get; set; }

        public string SKU { get; set; }

        public Dictionary<string, string> customProps { get; set; }

        public decimal Width { get; set; }

        public decimal Height { get; set; }

        public decimal Length { get; set; }

        public string Picture { get; set; }

        public Dictionary<string, bool> Attachments { get; set; }

        public decimal Total { get; set; }

    }
}
