
using Nop.Web.Models.Catalog;
using System.Collections.Generic;

namespace MWT.Nop.Plugin.MegaMenu.Models
{
    public class MegaMenuCategoryModel
    {
        public MegaMenuCategoryModel()
        {
            this.CategoryModel = new CategoryModel();
            this.SubCategories = (IList<MegaMenuCategoryModel>)new List<MegaMenuCategoryModel>();
            this.CategoryManufacturers = (IList<ManufacturerModel>)new List<ManufacturerModel>();
            this.BestSellerProducts = (IEnumerable<ProductOverviewModel>)new List<ProductOverviewModel>();
        }

        public string Title { get; set; }

        public bool ShouldShowViewAllLink { get; set; }

        public CategoryModel CategoryModel { get; set; }

        public IList<MegaMenuCategoryModel> SubCategories { get; set; }

        public IList<ManufacturerModel> CategoryManufacturers { get; set; }

        public IEnumerable<ProductOverviewModel> BestSellerProducts { get; set; }
    }
}
