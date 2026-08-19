using Nop.Web.Framework.Mvc.ModelBinding;

namespace MWT.Plugin.Misc.MwtStorefront.Areas.CustomOrder.Models.Orders
{
    public partial record CustomerStatsModel
    {
        [NopResourceDisplayName("CustomOrder.CustomerStats.Fields.LatestOrder")]
        public CustomOrderModel LatestOrder { get; set; }

        [NopResourceDisplayName("CustomOrder.CustomerStats.Fields.NoOforders")]
        public int NoOforders { get; set; }

        [NopResourceDisplayName("CustomOrder.CustomerStats.Fields.Spent")]
        public string Spent { get; set; }
    }
}
