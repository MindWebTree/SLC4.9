using Nop.Web.Framework.Mvc.ModelBinding;
using Nop.Web.Models.Catalog;
using Nop.Web.Models.Media;
using System.Collections.Generic;

namespace MWT.Plugin.Misc.MwtStorefront.Models.Catalog
{
    public partial record CustomizationFormModel
    {
        public CustomizationFormModel()
        {
            LstInteresedIn = new List<string>();
        }
        [NopResourceDisplayName("CustomizationForm.Fields.FullName")]
        public string FullName { get; set; }

        [NopResourceDisplayName("CustomizationForm.Fields.Email")]
        public string Email { get; set; }

        [NopResourceDisplayName("CustomizationForm.Fields.Phone")]
        public string Phone { get; set; }

        [NopResourceDisplayName("CustomizationForm.Fields.ZipCode")]
        public string ZipCode { get; set; }

        [NopResourceDisplayName("CustomizationForm.Fields.InteresedIn")]
        public string InteresedIn { get; set; }

        [NopResourceDisplayName("CustomizationForm.Fields.Message")]
        public string Message { get; set; }

        public string Dimensions { get; set; }

        public int ProductId { get; set; }

        public List<string> LstInteresedIn { get; set; }

        public IList<ProductDetailsModel.ProductAttributeModel> ProductAttributes { get; set; }

        public ProductSpecificationModel ProductSpecificationModel { get; set; }

        public PictureModel DefaultPictureModel { get; set; }
        public IList<PictureModel> PictureModels { get; set; }

        public string DefaultPicture { get; set; }
        public string ProductName { get; set; }

        public string Sku { get; set; }

        public bool DisplayCaptcha { get; set; }

        [NopResourceDisplayName("CustomizationForm.Fields.Attachment")]
        public string Attachment { get; set; }

    }
}
