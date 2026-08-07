using MWT.Nop.Core.Domain.KW;
using MWT.Plugin.Misc.MwtStorefront.Models.Catalog;
using MWT.Plugin.Misc.MwtStorefront.Models.KW;
using Nop.Web.Models.Catalog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MWT.Plugin.Misc.MwtStorefront.Factories
{
    public partial interface IKwTermModelFactory
    {
        Task<(string templateViewPath, string listingViewPath, string filterViewPath, string FilterViewPathForMobile, int pictureSize, bool isHorizontal)> PrepareKwTermTemplateViewPathAsync(int templateId);
        Task<KwTermModel> PrepareKwTermModelAsync(KwTerm kwTerm, CustomCatalogProductsCommand command, string queryString, int pictureSize);
        Task<CustomCatalogProductsModel> PrepareKwTermProductsModelAsync(KwTerm kwTerm, CustomCatalogProductsCommand command, string queryString = "", int categoryId = 0, bool forSections = false, int pictureSize = 0);
        Task<IList<CategoryModel.SubCategoryModel>> PrepareKwTermCategoriesModelAsync(int kwTermId);
    }
}
