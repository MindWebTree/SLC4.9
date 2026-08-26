using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;
using System.Collections.Generic;

namespace Nop.Web.Areas.Admin.Models.Customization.Custom.QA
{
    public partial record AddProductToQuestionAnswerModel : BaseNopModel
    {
        #region Ctor

        public AddProductToQuestionAnswerModel()
        {
            SelectedProductIds = new List<int>();
        }
        #endregion

        #region Properties

        public int QuestionAnswerId { get; set; }

        public IList<int> SelectedProductIds { get; set; }

        #endregion
    }

}
