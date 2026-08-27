using Nop.Web.Framework.Models;

namespace MWT.Plugin.Misc.MwtStorefront.Areas.Admin.Models.Utilities
{
    public partial record TestimonialSearchModel : BaseSearchModel
    {
        #region Properties

        public int OrderId { get; set; }

        #endregion
    }
}
