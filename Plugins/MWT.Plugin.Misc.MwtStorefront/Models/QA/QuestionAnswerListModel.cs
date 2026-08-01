using Nop.Web.Framework.Models;
using Nop.Web.Framework.UI.Paging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MWT.Plugin.Misc.MwtStorefront.Models.QA
{
    
    public partial record QuestionAnswerListModel : BasePageableModel
    {
        public string NoResultMessage { get; set; }
        public bool UseAjaxLoading { get; set; }
        public IList<QuestionAnswerModel> QuestionAnswers { get; set; }
    }
}
