using Nop.Core.Domain.Catalog;
using Nop.Web.Framework.Models;
using Nop.Web.Models.Catalog; 
using Nop.Web.Models.Media;
using System.Collections.Generic;


namespace MWT.Plugin.Misc.MwtStorefront.Models.Catalog
{
    public partial record CustomProductOverviewModel : ProductOverviewModel
    {
        public string VariantTitle { get; set; }

        public int DefaultVariantId { get; set; }
        public int VariantId { get; set; }

        public string VariantSename { get; set; }
        public string ProductRelation { get; set; }
        public IList<PictureModel> PictureModels { get; set; }
        public ProductDetailsModel.AddToCartModel AddToCart { get; set; }
        public IList<ProductDetailsModel.ProductAttributeModel> ProductAttributes { get; set; }

        public List<List<int>> AllCombinations { get; set; }
        public bool AllowAddingOnlyExistingAttributeCombinations { get; set; }
        public List<ProductTag> Tags { get; set; }
        public partial record CustomProductPriceModel : ProductPriceModel
        {
            public string MembershipPrice { get; set; }
            public decimal MembershipPriceValue { get; set; }
            public string Msrp { get; set; }
            public decimal MsrpValue { get; set; }
            public decimal OldPriceValue { get; set; }
            public string CurrencyCode { get; set; }
            public string PriceWithDiscount { get; set; }
            public bool CustomerEntersPrice { get; set; }
            public bool CallForPrice { get; set; }
            public int ProductId { get; set; }
            public bool HidePrices { get; set; }
            public string RentalPrice { get; set; }

            public bool IsVariantProduct { get; set; }
            public decimal MinOldPriceValue { get; set; }
            public decimal MaxOldPriceValue { get; set; }
            public decimal MinPriceValue { get; set; }
            public decimal MaxPriceValue { get; set; }
            public decimal MinMsrpValue { get; set; }
            public decimal MaxMsrpValue { get; set; }

            public decimal MinMembershipPriceValue { get; set; }
            public decimal MaxMembershipPriceValue { get; set; }


            public string MinOldPrice { get; set; }
            public string MaxOldPrice { get; set; }
            public string MinPrice { get; set; }
            public string MaxPrice { get; set; }
            public string MinMsrp { get; set; }
            public string MaxMsrp { get; set; }
            public string MinMembershipPrice { get; set; }
            public string MaxMembershipPrice { get; set; }
            public string OfferText { get; set; }

        }
        public PictureModel AlternatePictureModel { get; set; }
        public List<ProductAttributeModelWithImage> Attribute { get; set; }
        public partial record ProductAttributeModelWithImage : BaseNopModel
        {
            public string Name { get; set; }
            public string PictureUrl { get; set; }
        }
        public string CollectionMessage { get; set; }
        public string SizeShadeMessage { get; set; }
        public bool IsMainCollection { get; set; }
        public List<EsProductSuggestedKeyWordModel> suggestedwords { get; set; }
        public List<EsProductSpecificationAttributesModel> filters { get; set; }

        public List<EsEntitytModel> entities { get; set; }
        public int Inventory { get; set; }
        public bool EnableCustomizationModule { get; set; }
        public bool Published { get; set; }
        public string MetaKeywords { get; set; }
        public int NoOfSales { get; set; }
        public decimal ConversionValue { get; set; }
        public bool IsSimilarProduct { get; set; }
    }
}
