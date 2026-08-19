using Nop.Web.Framework.Models;
using Nop.Web.Models.Sitemap;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MWT.Plugin.Misc.MwtStorefront.Models.Sitemap
{
    public partial record SitemapExtendedModel : SitemapModel
    {

        #region Properties

        public new List<SitemapItemExtendedModel> Items = new List<SitemapItemExtendedModel>();

        public new SitemapPageExtendedModel PageModel = new SitemapPageExtendedModel();

        #endregion

        #region Nested classes

        public record SitemapItemExtendedModel: SitemapModel.SitemapItemModel
        {
            public int Id { get; set; }
            public int ParentId { get; set; }

        }

        #endregion
    }
}
