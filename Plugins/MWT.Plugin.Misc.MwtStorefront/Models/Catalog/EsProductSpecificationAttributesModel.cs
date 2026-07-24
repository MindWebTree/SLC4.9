namespace MWT.Plugin.Misc.MwtStorefront.Models.Catalog
{
    public class EsProductSpecificationAttributesModel
    {
        public string SpecificationAttributeOptionID { get; set; }
        public string Name { get; set; }
        public string SEName { get; set; }
        public string SpecificationAttributeID { get; set; }
        public string ParentName { get; set; }
        public int DisplayOrder { get; set; }
        public int ParentDisplayOrder { get; set; }
        public string ColorSquaresRgb { get; set; }

        public string keyword { get; set; }
        public bool ShowOutOfStock { get; set; }
        public bool Published { get; set; }
    }
}
