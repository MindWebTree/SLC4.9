using Nop.Web.Framework.Models;
namespace Nop.Web.Areas.Admin.Models.Customization.Custom.KW
{
    /// <summary>
    /// Represents a category product model
    /// </summary>
    public partial record KwTermProductModel : BaseNopEntityModel
    {
        public int ProductId { get; set; }
        public int KwTermId { get; set; }

        public string ProductName { get; set; }
        public string PictureThumbnailUrl { get; set; }

       
        public int MobileDisplayOrder { get; set; }
        public int DisplayOrder { get; set; }
      
    }
    public partial record KwTermProductListModel : BasePagedListModel<KwTermProductModel>
    {
    }
}
