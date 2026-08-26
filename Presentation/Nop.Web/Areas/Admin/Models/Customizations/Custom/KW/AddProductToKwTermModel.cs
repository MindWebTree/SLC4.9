

using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;
using System.Collections.Generic;

namespace Nop.Web.Areas.Admin.Models.Customization.Custom.KW
{
    public partial record AddProductToKwTermModel : BaseNopModel
    {
        #region Ctor

        public AddProductToKwTermModel()
        {
            SelectedProductIds = new List<int>();
        }
        #endregion

        #region Properties

        public int KwTermId { get; set; }

        public IList<int> SelectedProductIds { get; set; }

        #endregion
    }

}
