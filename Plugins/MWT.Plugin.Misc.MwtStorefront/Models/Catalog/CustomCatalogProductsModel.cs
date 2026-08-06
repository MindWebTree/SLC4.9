using MWT.Plugin.Misc.MwtStorefront.Models.Catalog;
using Nop.Web.Models.Catalog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MWT.Plugin.Misc.MwtStorefront.Models.Catalog
{
    public record class CustomCatalogProductsModel: CatalogProductsModel
    {
        public new IList<CustomProductOverviewModel> Products = new List<CustomProductOverviewModel>();
        public new CustomSpecificationFilterModel SpecificationFilter { get; set; }
        public int defaultPageSize { get; set; }
        public string CategoryName { get; set; }
        public string Sename { get; set; }
        public string Description { get; set; }
        public int CategoryID { get; set; }
        public string KwTermName { get; set; }
        public int KwTermId { get; set; }
        public string QuestionAnswerName { get; set; }
        public int QuestionAnswerId { get; set; }
        public bool ShowBanner { get; set; } = true;
    }
}
