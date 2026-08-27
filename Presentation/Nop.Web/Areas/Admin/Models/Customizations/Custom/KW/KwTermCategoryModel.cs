using Nop.Web.Framework.Models;
namespace Nop.Web.Areas.Admin.Models.Customization.Custom.KW
{
    /// <summary>
    /// Represents a category product model
    /// </summary>
    public partial record KwTermCategoryModel : BaseNopEntityModel
    {
        public int CategoryId { get; set; }
        public int KwTermId { get; set; }
        public string CategoryName { get; set; }
        public int DisplayOrder { get; set; }
      
    }
    public partial record KwTermCategoryListModel : BasePagedListModel<KwTermCategoryModel>
    {
    }
}
