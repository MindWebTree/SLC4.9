using Nop.Web.Framework.Models;

namespace MWT.Plugin.Misc.MwtStorefront.Areas.CustomOrder.Models.Products
{
    public partial record ProductOverviewModel : BaseNopEntityModel
    {
        public string DefaultPictureModel { get; set; }
        public string Name { get; set; }
        public string Sku { get; set; }
        public int Stock { get; set; }
        public string ProductPrice { get; set; }
        public List<ProductAttribute> ProductAttributes { get; set;  }
        public List<AttributeCombination> Combinations { get; set; }

    }
    public class AttributeCombination
    {
        public string Price { get; set; }
        public int Stock { get; set; }
        public List<Attribute> Combinations { get; set; }
    }

    public class ProductAttribute
    {
        public string ParentAtributeName { get; set; }
        public int ParentAttributeId { get; set; }
        public List<Attribute> Values { get; set; }
    }
    public class Attribute
    {
        public string ParentAtributeName { get; set; }
        public int ParentAttributeId { get; set; }
        public int AttributeId { get; set; }
        public string AttributeName { get; set; }
    }

}