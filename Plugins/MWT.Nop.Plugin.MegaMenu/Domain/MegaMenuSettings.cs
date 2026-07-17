using Nop.Core;
using Nop.Core.Configuration;

namespace MWT.Nop.Plugin.MegaMenu.Domain
{
    public class MegaMenuSettings : BaseEntity,ISettings
    {
        public bool Enabled { get; set; }

        public int CategoryImageSize { get; set; }

        public int ManufacturerImageSize { get; set; }

        public int VendorImageSize { get; set; }

        public int NumberOfCategories { get; set; }

        public int NumberOfManufacturers { get; set; }

        public int NumberOfVendors { get; set; }

        public int NumberOfTopics { get; set; }

        public int NumberOfProductTags { get; set; }

        public int NumberOfCategoriesPerRow { get; set; }

        public int NumberOfManufacturersPerRow { get; set; }

        public int NumberOfVendorsPerRow { get; set; }
    }
}
