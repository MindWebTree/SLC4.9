

using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MWT.Plugin.Misc.MwtStorefront.Areas.Admin.Models.QA
{
    /// <summary>
    /// Represents a category model
    /// </summary>
    public partial record QuestionAnswerModel : BaseNopEntityModel, IAclSupportedModel, IDiscountSupportedModel,
        ILocalizedModel<QuestionAnswerLocalizedModel>, IStoreMappingSupportedModel
    {
        #region Ctor

        public QuestionAnswerModel()
        {
            if (PageSize < 1)
            {
                PageSize = 5;
            }

            Locales = new List<QuestionAnswerLocalizedModel>();
            AvailableQuestionAnswerTemplates = new List<SelectListItem>();
            AvailableQuestionAnswer = new List<SelectListItem>();
            AvailableDiscounts = new List<SelectListItem>();
            SelectedDiscountIds = new List<int>();

            SelectedCustomerRoleIds = new List<int>();
            AvailableCustomerRoles = new List<SelectListItem>();

            SelectedStoreIds = new List<int>();
            AvailableStores = new List<SelectListItem>();

            QuestionAnswerProductSearchModel = new QuestionAnswerProductSearchModel();
            RelatedQuestionAnswerSearchModel = new RelatedQuestionAnswerSearchModel();
            PublishedOn = DateTime.UtcNow;
        }

        #endregion

        #region Properties

        [NopResourceDisplayName("Admin.Catalog.Categories.Fields.Name")]
        public string Name { get; set; }

        [NopResourceDisplayName("Admin.Catalog.Categories.Fields.Description")]
        public string Description { get; set; }

        [NopResourceDisplayName("Admin.Catalog.Categories.Fields.QuestionAnswerTemplate")]
        public int QuestionAnswerTemplateId { get; set; }
        public IList<SelectListItem> AvailableQuestionAnswerTemplates { get; set; }

        [NopResourceDisplayName("Admin.Catalog.Categories.Fields.MetaKeywords")]
        public string MetaKeywords { get; set; }

        [NopResourceDisplayName("Admin.Catalog.Categories.Fields.MetaDescription")]
        public string MetaDescription { get; set; }

        [NopResourceDisplayName("Admin.Catalog.Categories.Fields.MetaTitle")]
        public string MetaTitle { get; set; }

        [NopResourceDisplayName("Admin.Catalog.Categories.Fields.SeName")]
        public string SeName { get; set; }


        [UIHint("Picture")]
        [NopResourceDisplayName("Admin.Catalog.Categories.Fields.Picture")]
        public int PictureId { get; set; }

        [NopResourceDisplayName("Admin.Catalog.Categories.Fields.PageSize")]
        public int PageSize { get; set; }

        [NopResourceDisplayName("Admin.Catalog.Categories.Fields.AllowCustomersToSelectPageSize")]
        public bool AllowCustomersToSelectPageSize { get; set; }

        [NopResourceDisplayName("Admin.Catalog.Categories.Fields.PageSizeOptions")]
        public string PageSizeOptions { get; set; }


        [NopResourceDisplayName("Admin.Catalog.Categories.Fields.Published")]
        public bool Published { get; set; }

        [NopResourceDisplayName("Admin.Catalog.Categories.Fields.Deleted")]
        public bool Deleted { get; set; }

        [NopResourceDisplayName("Admin.Catalog.Categories.Fields.DisplayOrder")]
        public int DisplayOrder { get; set; }

        public IList<QuestionAnswerLocalizedModel> Locales { get; set; }

        public string Breadcrumb { get; set; }

        //ACL (customer roles)
        [NopResourceDisplayName("Admin.Catalog.Categories.Fields.AclCustomerRoles")]
        public IList<int> SelectedCustomerRoleIds { get; set; }
        public IList<SelectListItem> AvailableCustomerRoles { get; set; }

        //store mapping
        [NopResourceDisplayName("Admin.Catalog.Categories.Fields.LimitedToStores")]
        public IList<int> SelectedStoreIds { get; set; }
        public IList<SelectListItem> AvailableStores { get; set; }

        public IList<SelectListItem> AvailableQuestionAnswer { get; set; }

        //discounts
        [NopResourceDisplayName("Admin.Catalog.Categories.Fields.Discounts")]
        public IList<int> SelectedDiscountIds { get; set; }
        public IList<SelectListItem> AvailableDiscounts { get; set; }

        public QuestionAnswerProductSearchModel QuestionAnswerProductSearchModel { get; set; }
        public RelatedQuestionAnswerSearchModel RelatedQuestionAnswerSearchModel { get; set; }
        public string PrimaryStoreCurrencyCode { get; set; }


        public bool EnableInfiniteScroll { get; set; }
        [NopResourceDisplayName("Admin.Catalog.Categories.Fields.ListingLink")]
        public string ListingLink { get; set; }

        [NopResourceDisplayName("Admin.Catalog.Categories.Fields.ListingTitle")]
        public string ListingTitle { get; set; }
        [NopResourceDisplayName("Admin.Catalog.Categories.Fields.PublishedOn")]
        public DateTime PublishedOn { get; set; }

        [NopResourceDisplayName("Admin.Catalog.Categories.Fields.BackgroundColor")]
        public string BackgroundColor { get; set; }

        [NopResourceDisplayName("Admin.Catalog.Categories.Fields.AuthorName")]
        public string AuthorName { get; set; }

        [UIHint("Picture")]
        [NopResourceDisplayName("Admin.Catalog.Categories.Fields.AuthorId")]
        public int AuthorPictureId { get; set; }

        #endregion
    }

    public partial record QuestionAnswerLocalizedModel : ILocalizedLocaleModel
    {
        public int LanguageId { get; set; }

        [NopResourceDisplayName("Admin.Catalog.Categories.Fields.Name")]
        public string Name { get; set; }

        [NopResourceDisplayName("Admin.Catalog.Categories.Fields.Description")]
        public string Description { get; set; }

        [NopResourceDisplayName("Admin.Catalog.Categories.Fields.MetaKeywords")]
        public string MetaKeywords { get; set; }

        [NopResourceDisplayName("Admin.Catalog.Categories.Fields.MetaDescription")]
        public string MetaDescription { get; set; }

        [NopResourceDisplayName("Admin.Catalog.Categories.Fields.MetaTitle")]
        public string MetaTitle { get; set; }

        [NopResourceDisplayName("Admin.Catalog.Categories.Fields.SeName")]
        public string SeName { get; set; }

    }

    public partial record QuestionAnswerProductSearchModel : BaseSearchModel
    {
        #region Properties

        public int QuestionAnswerId { get; set; }

        #endregion
    }
}
