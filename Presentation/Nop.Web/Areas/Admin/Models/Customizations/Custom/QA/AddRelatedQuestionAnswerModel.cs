using Nop.Web.Framework.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nop.Web.Areas.Admin.Models.Customization.Custom.QA
{
    public partial record AddRelatedQuestionAnswerModel : BaseNopModel
    {
        #region Ctor

        public AddRelatedQuestionAnswerModel()
        {
            SelectedQuestionAnswerIds = new List<int>();
        }
        #endregion

        #region Properties

        public int QuestionAnswerId { get; set; }

        public IList<int> SelectedQuestionAnswerIds { get; set; }

        #endregion
    }
}
