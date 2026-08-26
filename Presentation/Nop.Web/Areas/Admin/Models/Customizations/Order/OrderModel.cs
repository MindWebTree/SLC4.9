using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Web.Areas.Admin.Models.Orders
{
    /// <summary>
    /// Represents an order model
    /// </summary>
    public partial record OrderModel : BaseNopEntityModel
    {
        #region Properties
        [NopResourceDisplayName("Admin.Orders.Fields.ParentOrderID")]
        public int ParentOrderID { get; set; }

        public string OrderType { get; set; }

        #endregion
    }
}
