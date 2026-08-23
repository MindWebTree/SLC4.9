using MWT.Plugin.Misc.MwtStorefront.Areas.CustomOrder.Models.Orders;
using MWT.Plugin.Misc.MwtStorefront.Models.Common;
using Nop.Web.Framework.Models;
using Nop.Web.Models.Common;
using NopOrder = Nop.Core.Domain.Orders.Order;

namespace MWT.Plugin.Misc.MwtStorefront.Models.Order
{
    public partial record CustomOrderConversionModel : BaseNopModel
    {
        public CustomOrderConversionModel()
        {
            ShippingAddress = new AddressModel();
        }
        public NopOrder order { get; set; }
        public List<ItemModel> Items { get; set; }
        public AddressModel ShippingAddress { get; set; }
    }
}
