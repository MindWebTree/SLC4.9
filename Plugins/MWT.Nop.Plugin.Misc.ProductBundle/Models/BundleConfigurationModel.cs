using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MWT.Nop.Plugin.Misc.ProductBundle.Models
{
    public record BundleConfigurationModel : BaseNopEntityModel
    {
        public BundleConfigurationModel()
        {
            BundleItemSearchModel = new BundleItemSearchModel();
        }

        public int ProductId { get; set; }
        public int BundleId { get; set; }

        [Required]
        [NopResourceDisplayName("MWT.Plugin.ProductBundle.BundleName")]
        public string Name { get; set; }

        [NopResourceDisplayName("MWT.Plugin.ProductBundle.ProductAttributeValueName")]
        public string VariantName { get; set; }

        [NopResourceDisplayName("MWT.Plugin.ProductBundle.ProductName")]
        public string ProductName { get; set; }

        [Required]
        [NopResourceDisplayName("MWT.Plugin.ProductBundle.ConfigurationValue")]
        public int VariantId { get; set; }

        [NopResourceDisplayName("MWT.Plugin.ProductBundle.NoOfPieces")]
        public int NoOfPieces { get; set; }
        public BundleItemSearchModel BundleItemSearchModel { get; set; }


        [NopResourceDisplayName("MWT.Plugin.ProductBundle.CreatedOnUtc")]
        public DateTime? CreatedOnUtc { get; set; }
        [NopResourceDisplayName("MWT.Plugin.ProductBundle.UpdatedOnUtc")]
        public DateTime? UpdatedOnUtc { get; set; }

        [UIHint("Picture")]
        [NopResourceDisplayName("MWT.Plugin.ProductBundle.PictureId")]
        public int PictureId { get; set; }
        public IFormFile BundleImage { get; set; }
        [NopResourceDisplayName("MWT.Plugin.ProductBundle.DisplayOnProductPage")]
        public bool DisplayOnProductPage { get; set; }

    } 
}