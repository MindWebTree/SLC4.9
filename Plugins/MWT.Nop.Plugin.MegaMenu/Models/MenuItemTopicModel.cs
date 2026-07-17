using MWT.Nop.Plugin.MegaMenu.Areas.Admin.Models;
using Nop.Web.Models.Topics;
using System.Collections.Generic;

namespace MWT.Nop.Plugin.MegaMenu.Models
{
    public class MenuItemTopicModel
    {
        public MenuItemTopicModel() => this.Topics = (IList<TopicModel>)new List<TopicModel>();

        public MenuItemModel Item { get; set; }

        public IList<TopicModel> Topics { get; set; }
    }
}
