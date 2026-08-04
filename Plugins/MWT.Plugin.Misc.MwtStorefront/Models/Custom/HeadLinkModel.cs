
using System.Collections.Generic;

namespace MWT.Plugin.Misc.MwtStorefront.Models.Custom
{
    public partial record HeadLinkModel
    {
        public HeadLinkModel()
        {
            Links = new List<CollectionLinkModel>();
        }
        public List<CollectionLinkModel> Links { get; set; }
    }

    public partial record CollectionLinkModel
    {
        public string Title { get; set; }
        public string Link { get; set; }
        public bool IsActive { get; set; }
    }
}
