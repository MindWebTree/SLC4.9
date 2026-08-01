using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MWT.Plugin.Misc.MwtStorefront.Areas.Admin.Models.QA
{
    public partial record RelatedQuestionAnswerModel: BaseNopEntityModel
    {
        #region Properties

        public int QuestionAnswerId2 { get; set; }

        [NopResourceDisplayName("Admin.Catalog.Products.RelatedQuestionAnswers.Fields.Product")]
        public string QuestionAnswer2Name { get; set; }

        [NopResourceDisplayName("Admin.Catalog.Products.RelatedQuestionAnswers.Fields.DisplayOrder")]
        public int DisplayOrder { get; set; }

        #endregion
    }
}
