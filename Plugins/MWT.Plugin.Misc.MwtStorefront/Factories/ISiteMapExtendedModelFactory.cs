
using MWT.Plugin.Misc.MwtStorefront.Models.Sitemap;
using Nop.Web.Factories;

namespace MWT.Plugin.Misc.MwtStorefront.Factories
{
    public partial interface ISiteMapExtendedModelFactory: ISitemapModelFactory
    {
        Task<SitemapExtendedModel> PrepareSitemapExtendedModelAsync(SitemapPageExtendedModel pageModel);
        Task<string> PrepareSitemapExtendedIndexXml();
        Task<string> PreparePageSitemapXml();
        Task<string> PrepareProductSitemapXml();
        Task<string> PrepareCategorySitemapXml();
        Task<string> PrepareQuestionAnswerSitemapXml();
        Task<string> PrepareKwTermSitemapXml();
        Task<string> PreparePictureSitemapXmlAsync(int? id);
        Task<string> PrepareTagSitemapXmlAsync(int? id);
    }
}
