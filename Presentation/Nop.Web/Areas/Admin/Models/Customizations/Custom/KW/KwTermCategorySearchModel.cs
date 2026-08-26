using Nop.Web.Framework.Models;

namespace Nop.Web.Areas.Admin.Models.Customization.Custom.KW
{
    public partial record KwTermCategorySearchModel : BaseSearchModel
    {
        #region Properties

        public int KwTermId { get; set; }

        #endregion
    }
}
