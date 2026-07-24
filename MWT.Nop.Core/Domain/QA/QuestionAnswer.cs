using Nop.Core;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Discounts;
using Nop.Core.Domain.Localization;
using Nop.Core.Domain.Security;
using Nop.Core.Domain.Seo;
using Nop.Core.Domain.Stores;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MWT.Nop.Core.Domain.QA
{
    public partial class QuestionAnswer : BaseEntity, ILocalizedEntity, ISlugSupported, IAclSupported, IStoreMappingSupported, IDiscountSupported<DiscountCategoryMapping>, ISoftDeletedEntity
    {
        public QuestionAnswer()
        {
            BackgroundColor = "#f6f6f5";
        }
        public string Name { get; set; }
        public string Description { get; set; }
        public int QuestionAnswerTemplateId { get; set; }
        public string MetaKeywords { get; set; }
        public string MetaDescription { get; set; }
        public string MetaTitle { get; set; }
        public int PictureId { get; set; }
        public int PageSize { get; set; }
        public bool AllowCustomersToSelectPageSize { get; set; }
        public string PageSizeOptions { get; set; }
        public bool SubjectToAcl { get; set; }
        public bool LimitedToStores { get; set; }
        public bool Published { get; set; }
        public bool Deleted { get; set; }
        public int DisplayOrder { get; set; }
        public bool EnableInfiniteScroll { get; set; }
        public DateTime CreatedOnUtc { get; set; }
        public DateTime UpdatedOnUtc { get; set; }
        public DateTime PublishedOn { get; set; }
        public string ListingLink { get; set; }
        public string ListingTitle { get; set; }
        public string BackgroundColor { get; set; }
        public string AuthorName { get; set; }
        public int AuthorPictureId { get; set; }

    }
}
