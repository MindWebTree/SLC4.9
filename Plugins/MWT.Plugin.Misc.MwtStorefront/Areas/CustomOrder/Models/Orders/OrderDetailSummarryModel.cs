namespace MWT.Plugin.Misc.MwtStorefront.Areas.CustomOrder.Models.Orders
{
    public class OrderDetailSummarryModel
    {
        public string Orderid { get; set; }

        public int LiveOrderNumber { get; set; }

        public string OrderStatus { get; set; }

        public DateTime? LastUpdatedOn { get; set; }

        public string LastUpdatedBy { get; set; }

        public List<CustomOrderStatusLogModel> Logs { get; set; }
    }
}
