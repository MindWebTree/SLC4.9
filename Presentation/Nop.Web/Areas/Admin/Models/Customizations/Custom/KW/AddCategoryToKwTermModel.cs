

using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;
using System.Collections.Generic;

namespace Nop.Web.Areas.Admin.Models.Customization.Custom.KW
{
    public partial record AddCategoryToKwTermModel : BaseNopModel
    {
        #region Ctor

        public AddCategoryToKwTermModel()
        {
            SelectedCategoryIds = new List<int>();
        }
        #endregion

        #region Properties

        public int KwTermId { get; set; }

        public IList<int> SelectedCategoryIds { get; set; }

        #endregion
    }

}
