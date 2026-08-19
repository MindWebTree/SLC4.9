using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;
using System;
using System.Collections.Generic;

namespace MWT.Plugin.Misc.MwtStorefront.Areas.CustomOrder.Models.Orders
{
    public partial record CustomOrderOrderTypeSectionModel : BaseNopEntityModel
    {
        public CustomOrderOrderTypeSectionModel()
        {
            subOrderTypes = new List<SelectListItem>();
        }
        public List<SelectListItem> subOrderTypes { get; set; }

        [NopResourceDisplayName("CustomOrder.Order.Fields.AlreadyFee")]
        public decimal? AlreadyFee { get; set; }

        [NopResourceDisplayName("CustomOrder.Order.Fields.PromiseDayDate")]
        public DateTime? PromiseDayDate { get; set; }

        [NopResourceDisplayName("CustomOrder.Order.Fields.HouzzFee")]
        public decimal? HouzzFee { get; set; }

        [NopResourceDisplayName("CustomOrder.Order.Fields.HouzzFeeType")]
        public string HouzzFeeType { get; set; }
        [NopResourceDisplayName("CustomOrder.Order.Fields.PurchaseOrderNumber")]
        public string PurchaseOrderNumber { get; set; }

        [NopResourceDisplayName("CustomOrder.Order.Fields.LiveOrderNumber")]
        public int? LiveOrderNumber { get; set; }
        public string OrderType { get; set; }
        public string OrderStatus { get; set; }
        [NopResourceDisplayName("CustomOrder.Order.Fields.SubOrderTypeId")]
        public int? SubOrderTypeId { get; set; }

        

    }
}
