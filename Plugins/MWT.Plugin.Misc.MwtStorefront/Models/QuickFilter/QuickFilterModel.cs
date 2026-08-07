

using iTextSharp.text;
using System.Collections.Generic;

namespace MWT.Plugin.Misc.MwtStorefront.Models.QuickFilter
{
    public partial record QuickFilterModel
    {
        public string Link { get; set; }
        public string TermName { get; set; }
        public string PictureUrl { get; set; }

    }

    public partial record QuickFilterListModel
    {
        public QuickFilterListModel()
        {
            Filters = new List<QuickFilterModel>();
        }
        public string Name { get; set; }
        public List<QuickFilterModel> Filters { get; set; }
    }

}
