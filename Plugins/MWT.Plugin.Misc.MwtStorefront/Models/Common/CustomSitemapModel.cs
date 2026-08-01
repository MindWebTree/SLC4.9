using Nop.Web.Framework.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MWT.Plugin.Misc.MwtStorefront.Models.Common
{
    public partial record CustomSitemapModel : BaseNopModel
    {
        #region Ctor

        public CustomSitemapModel()
        {
            Items = new List<SitemapItemModel>();
            PageModel = new CustomSitemapPageModel();
        }

        #endregion

        #region Properties

        public List<SitemapItemModel> Items { get; set; }

        public CustomSitemapPageModel PageModel { get; set; }

        #endregion

        #region Nested classes

        public record SitemapItemModel
        {
            public int Id { get; set; }
            public string GroupTitle { get; set; }
            public string Url { get; set; }
            public string Name { get; set; }

            public int ParentId { get; set; }

        }

        #endregion
    }
}
