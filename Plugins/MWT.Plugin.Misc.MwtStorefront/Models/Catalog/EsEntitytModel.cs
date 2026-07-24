namespace MWT.Plugin.Misc.MwtStorefront.Models.Catalog
{
    public partial class EsEntitytModel
    {
        public string EntityID { get; set; }
        public string EntityType { get; set; }
        public string EntityName { get; set; }
        public string SEName { get; set; }
        public string SEKeywords { get; set; }
        public string ParentEntityID { get; set; }
        public string ParentEntityName { get; set; }
        public int DisplayOrder { get; set; }
        public bool Published { get; set; }
        public bool ShowOutOfStock { get; set; }
    }
}
