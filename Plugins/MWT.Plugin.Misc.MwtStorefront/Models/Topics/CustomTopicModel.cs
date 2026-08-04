using Nop.Web.Areas.Admin.Models.Topics;

namespace MWT.Plugin.Misc.MwtStorefront.Models.Topics;

public partial record CustomTopicModel : TopicModel
{
    public bool HideDefaultTitle { get; set; }
}