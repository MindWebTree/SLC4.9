using MWT.Plugin.Misc.MwtStorefront.Models.Catalog;
using Nop.Web.Framework.Models;
using Nop.Web.Models.Catalog;
using Nop.Web.Models.Media;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MWT.Plugin.Misc.MwtStorefront.Models.QA
{

    public partial record QuestionAnswerModel : BaseNopEntityModel
    {
        public QuestionAnswerModel()
        {
            PictureModel = new PictureModel();
            QuestionAnswerBreadcrumb = new List<QuestionAnswerModel>();
            CatalogProductsModel = new CustomCatalogProductsModel();
        }

        public string Name { get; set; }
        public string Description { get; set; }
        public string MetaKeywords { get; set; }
        public string MetaDescription { get; set; }
        public string MetaTitle { get; set; }
        public string SeName { get; set; }
        public PictureModel PictureModel { get; set; }
        public int PictureId { get; set; }
        public bool DisplayCategoryBreadcrumb { get; set; }
        public string ListingTitle { get; set; }
        public string ListingLink { get; set; }

        public DateTime PublishedOn { get; set; }
        public IList<QuestionAnswerModel> QuestionAnswerBreadcrumb { get; set; }
        public CustomCatalogProductsModel CatalogProductsModel { get; set; }
        public string GridLineViewPath { get; set; }
        public string FilterViewPath { get; set; }
        public string FilterViewPathForMobile { get; set; }
        public bool EnableInfiniteScroll { get; set; }

        public DateTime CreatedOn { get; set; }
        public string BackgroundColor { get; set; }
        public string AuthorName { get; set; }
        public int AuthorPictureId { get; set; }
        public string AuthorPictureUrl { get; set; }
    }
}