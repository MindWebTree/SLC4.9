using MWT.Plugin.Misc.MwtStorefront.Models.Catalog;
using Nop.Core.Domain.Catalog;
using Nop.Web.Factories;
using Nop.Web.Models.Catalog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MWT.Plugin.Misc.MwtStorefront.Factories.Catalog
{
    public interface ICustomCatalogModelFactory : ICatalogModelFactory
    {
        #region Categories
        Task<List<CategoryModel>> CustomPrepareHomepageCategoryModelsAsync();
        #endregion
        Task<CustomCategoryModel> PrepareCustomCategoryModelAsync(Category category, CustomCatalogProductsCommand command, string queryString, int pictureSize);
        Task<CustomCatalogProductsModel> PrepareCustomCategoryProductsModelAsync(Category category, CustomCatalogProductsCommand command, string queryString = "", int categoryId = 0, bool forSections = false, int pictureSize = 0);
        Task<CustomSearchModel> PrepareCustomSearchModelAsync(CustomSearchModel model, CustomCatalogProductsCommand command, string queryString);
        Task<(string templateViewPath, string listingViewPath, string filterViewPath, string FilterViewPathForMobile, int pictureSize, bool isHorizontal)> CustomPrepareCategoryTemplateViewPathAsync(int templateId);
        Task<CustomCatalogProductsModel> PrepareCustomSearchProductsModelAsync(CustomSearchModel searchModel, CustomCatalogProductsCommand command, string queryString);


    }
}
