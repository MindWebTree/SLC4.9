using MWT.Nop.Core.Services.Zoho;
using MWT.Plugin.Misc.MwtStorefront.Areas.CustomOrder.Models.Orders;


namespace MWT.Plugin.Misc.MwtStorefront.Areas.CustomOrder.Models.Common
{
    public class AjaxReponseModel
    {
        public int statuscode { get; set; }
        public string message { get; set; }
        public string html { get; set; }
        public string json { get; set; }
        public string goto_section { get; set; }
        public bool closeContainer { get; set; }
        public bool isPopup { get; set; }
        public bool showContainer { get; set; }
        public string bindSectionId { get; set; }
        public bool EnableDisableSections { get; set; }
        public bool? EnableProductSearch { get; set; }
        public bool? EnableCartSummary { get; set; }
        public bool? DisplayCartSummary { get; set; }
        public bool? EnableCustomerSearch { get; set; }
        public CustomOrderSummaryModel orderSummary { get; set; }
        public string orderSummaryHtml { get; set; }
        public string orderDetails { get; set; }
        public string notificationMessage { get; set; }
        public bool redirect { get; set; }

        public ZohoDto ContactDetails { get; set; }
        public string receipt { get; set; }

        public string NotesLog { get; set; }
    }
}
