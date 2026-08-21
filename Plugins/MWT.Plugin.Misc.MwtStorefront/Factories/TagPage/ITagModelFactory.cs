using MWT.Nop.Core.Domain.TagPage;
using MWT.Plugin.Misc.MwtStorefront.Models.Catalog;
using Nop.Core.Domain.Catalog;
using Nop.Web.Models.Catalog;

namespace MWT.Plugin.Misc.MwtStorefront.Factories.TagPage
{
    public partial interface ITagModelFactory
    {
        Task<CustomCategoryModel> PrepareTagModelAsync(TagSlugMapping tag, Category category,string categorySlug, CustomCatalogProductsCommand command, string queryString, int pictureSize);
        Task<CustomCatalogProductsModel> PrepareTagProductsModelAsync(TagSlugMapping tag, Category category, CustomCatalogProductsCommand command,
          string queryString = "", int categoryId = 0, bool forSections = false, int pictureSize = 0);

        //Task<string> PrepareTagSitemapXmlAsync(int? id);
    }
}
