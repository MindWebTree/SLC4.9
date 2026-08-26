using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Web.Areas.Admin.Models.Customization.Custom.QA
{
    /// <summary>
    /// Represents a category search model
    /// </summary>
    public partial record QuestionAnswerSearchModel : BaseSearchModel
    {
        #region Ctor

        public QuestionAnswerSearchModel()
        {
            AvailableStores = new List<SelectListItem>();
            AvailablePublishedOptions = new List<SelectListItem>();
        }

        #endregion

        #region Properties

        [NopResourceDisplayName("Admin.Catalog.QuestionAnswers.List.SearchQuestionAnswerName")]
        public string SearchQuestionAnswerName { get; set; }

        [NopResourceDisplayName("Admin.Catalog.QuestionAnswers.List.SearchPublished")]
        public int SearchPublishedId { get; set; }

        public IList<SelectListItem> AvailablePublishedOptions { get; set; }

        [NopResourceDisplayName("Admin.Catalog.QuestionAnswers.List.SearchStore")]
        public int SearchStoreId { get; set; }

        public IList<SelectListItem> AvailableStores { get; set; }

        public bool HideStoresList { get; set; }

        #endregion
    }
}