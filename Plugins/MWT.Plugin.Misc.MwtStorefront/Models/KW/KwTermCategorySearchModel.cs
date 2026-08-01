using Nop.Web.Framework.Models;

namespace MWT.Plugin.Misc.MwtStorefront.Models.KW
{
    public partial record KwTermCategorySearchModel : BaseSearchModel
    {
        #region Properties

        public int KwTermId { get; set; }

        #endregion
    }
}
