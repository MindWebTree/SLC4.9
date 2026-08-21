using Nop.Web.Framework.Models;

namespace MWT.Plugin.Misc.MwtStorefront.Areas.CustomOrder.Models.Orders
{
    public record CustomOrderOrderTypeModel:BaseNopEntityModel
    {
        public string Name { get; set; } = null!;
        public string IconPath { get; set; }
        public int ParentId { get; set; }
    }
}
