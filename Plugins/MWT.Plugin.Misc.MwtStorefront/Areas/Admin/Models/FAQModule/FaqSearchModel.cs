using Nop.Web.Framework.Models;
namespace MWT.Plugin.Misc.MwtStorefront.Areas.Admin.Models.FAQModule
{
    /// <summary>
    /// Represents a related product list model
    /// </summary>
    public partial record FaqSearchModel : BaseSearchModel
    {
        #region Properties

        public string EntityType { get; set; }
        public int EntityId { get; set; }

        #endregion
    }
}