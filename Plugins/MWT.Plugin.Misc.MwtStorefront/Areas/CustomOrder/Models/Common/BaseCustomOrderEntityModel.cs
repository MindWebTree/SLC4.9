using Nop.Web.Framework.Models;
namespace MWT.Plugin.Misc.MwtStorefront.Areas.CustomOrder.Models.Common
{
    public record BaseCustomOrderEntityModel: BaseNopEntityModel
    {
        public bool isEditable { get; set; }
    }
}
