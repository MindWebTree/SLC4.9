using Nop.Web.Framework.Mvc.ModelBinding;
using Nop.Web.Models.Catalog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MWT.Plugin.Misc.MwtStorefront.Models.Catalog
{
    public record class CustomCatalogProductsCommand: CatalogProductsCommand
    {
        #region Properties

        public int ViewAll { get; set; }

        public Dictionary<int, List<int>> specificationAttributes { get; set; }

        [NopResourceDisplayName("SortOption.Query.Param")]
        public string SortBy { get; set; }

        public List<int> openTabs { get; set; }

        public List<int> popupOpenTabs { get; set; }

        public List<int> topOpenTabs { get; set; }

        public int KwTermId { get; set; }

        public int QuestionAnswerId { get; set; }
        #endregion
    }
}
