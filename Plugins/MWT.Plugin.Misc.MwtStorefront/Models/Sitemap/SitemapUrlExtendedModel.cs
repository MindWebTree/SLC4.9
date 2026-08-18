
using Nop.Web.Models.Sitemap;

namespace MWT.Plugin.Misc.MwtStorefront.Models.Sitemap
{
    public record SitemapUrlExtendedModel : SitemapUrlModel
    {
        public SitemapUrlExtendedModel(string location, IList<string> alternateLocations, UpdateFrequency frequency, DateTime updatedOn):base(location, alternateLocations, frequency, updatedOn)
        {
            Location = location;
            AlternateLocations = alternateLocations ?? new List<string>();
            UpdateFrequency = frequency;
            UpdatedOn = updatedOn;
        }

        /// <summary>
        /// Initializes a new instance of the sitemap URL model based on the passed model
        /// </summary>
        /// <param name="location">URL of the page</param>
        /// <param name="sitemapUrl">The another sitemap url</param>
        public SitemapUrlExtendedModel(string location, SitemapUrlModel sitemapUrl) : base(location, sitemapUrl)
        {
            Location = location;
            AlternateLocations = sitemapUrl.AlternateLocations;
            UpdateFrequency = sitemapUrl.UpdateFrequency;
            UpdatedOn = sitemapUrl.UpdatedOn;
        }
        public ImageSitemap Image { get; set; }
    }
    public class ImageSitemap
    {
        public string url { get; set; }
        public string Title { get; set; }
        public string Caption { get; set; }
    }
}
