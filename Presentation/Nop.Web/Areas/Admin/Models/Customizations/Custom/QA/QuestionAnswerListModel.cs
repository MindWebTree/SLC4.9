using Nop.Web.Framework.Models;

namespace Nop.Web.Areas.Admin.Models.Customization.Custom.QA
{
    /// <summary>
    /// Represents a category list model
    /// </summary>
    public partial record QuestionAnswerListModel : BasePagedListModel<QuestionAnswerModel>
    {
    }
}