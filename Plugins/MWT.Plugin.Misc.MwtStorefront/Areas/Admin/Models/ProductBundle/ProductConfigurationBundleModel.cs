using MWT.Nop.Core.Domain.ProductBundle;
using MWT.Plugin.Misc.MwtStorefront.Models.Catalog;
using Nop.Web.Framework.Models;

namespace MWT.Plugin.Misc.MwtStorefront.Area.Admin.Models
{
    //public partial record ProductConfigurationBundleModel : BaseNopModel
    //{
    //    public ProductConfigurationBundleModel()
    //    {
    //        ProductOverviewModels = new List<ProductOverviewModel>();
    //    }
    //    public string BundleName { get; set; }
    //    public string Imageurl { get; set; }
    //    public int BundleId { get; set; }
    //    public string AttributeName { get; set; }
    //    public int VariantId { get; set; }
    //    public string WidgetZone { get; set; } 
    //    public int ProductPrice { get; set; }
    //    public bool IsPreSelected { get; set; } 
    //    public IList<ProductOverviewModel> ProductOverviewModels { get; set; }

    //    public decimal BundlePrice { get; set; }
    //}
    public partial record ProductConfigurationBundleModel : BaseNopModel
    {
        public ProductConfigurationBundleModel()
        {
            ProductOverviewModels = new List<CustomProductOverviewModel>();
            InitialItems = new List<BundleItem>();
        }

        public string BundleName { get; set; }
        public string Imageurl { get; set; }
        public int BundleId { get; set; }
        public string AttributeName { get; set; }
        public int VariantId { get; set; }
        public string WidgetZone { get; set; }
        public bool IsPreSelected { get; set; }

        public IList<CustomProductOverviewModel> ProductOverviewModels { get; set; }

        public List<int> InitialProductIds { get; set; }
        public string BundleManifestJson { get; set; }
        public string DefaultBundlePrice { get; set; }
        public string TotalSaving { get; set; }
        public string Offer { get; set; }
        public string BMSMDiscount { get; set; }
        public string BundleDiscount { get; set; }
        public List<BundleItem> InitialItems { get; set; }
        public int SelectedVariantId { get; set; }
        public bool ShowShadeName { get; set; } = false;
        public decimal BuyMoreSaveMoreDiscount { get; set; }
        public decimal SingleItemThreshold { get; set; }
        public string DiscountType { get; set; }
        public bool EnableQuickView { get; set; }

    }


}