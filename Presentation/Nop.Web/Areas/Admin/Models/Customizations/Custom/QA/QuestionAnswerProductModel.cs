using Nop.Web.Framework.Models;
namespace Nop.Web.Areas.Admin.Models.Customization.Custom.QA
{
    /// <summary>
    /// Represents a category product model
    /// </summary>
    public partial record QuestionAnswerProductModel : BaseNopEntityModel
    {
        public int ProductId { get; set; }
        public int QuestionAnswerId { get; set; }

        public string ProductName { get; set; }
        public string PictureThumbnailUrl { get; set; }

       
        public int MobileDisplayOrder { get; set; }
        public int DisplayOrder { get; set; }
      
    }
    public partial record QuestionAnswerProductListModel : BasePagedListModel<QuestionAnswerProductModel>
    {
    }
}
