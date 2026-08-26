using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Nop.Web.Areas.Admin.Models.Customization.Custom.LandingPage_Management
{
    /// <summary>
    /// Represents a Landing Page model for admin area
    /// </summary>
    public partial record LandingPageModel : BaseNopEntityModel, IAclSupportedModel,
        ILocalizedModel<LandingPageLocalizedModel>, IStoreMappingSupportedModel
    {
        #region Ctor

        public LandingPageModel()
        {
            Locales = new List<LandingPageLocalizedModel>();
            SelectedCustomerRoleIds = new List<int>();
            AvailableCustomerRoles = new List<SelectListItem>();

            SelectedStoreIds = new List<int>();
            AvailableStores = new List<SelectListItem>();
        }

        #endregion

        #region Properties

        [NopResourceDisplayName("Admin.LandingPage.Fields.Name")]
        public string Name { get; set; }

        [NopResourceDisplayName("Admin.LandingPage.Fields.Description")]
        public string Description { get; set; }

        [NopResourceDisplayName("Admin.LandingPage.Fields.MetaKeywords")]
        public string MetaKeywords { get; set; }

        [NopResourceDisplayName("Admin.LandingPage.Fields.MetaTitle")]
        public string MetaTitle { get; set; }

        [NopResourceDisplayName("Admin.LandingPage.Fields.MetaDescription")]
        public string MetaDescription { get; set; }

        [UIHint("Picture")]
        [NopResourceDisplayName("Admin.LandingPage.Fields.Picture")]
        public int PictureId { get; set; }


        [NopResourceDisplayName("Admin.LandingPage.Fields.PageContent")]
        public string PageContent { get; set; }

        [NopResourceDisplayName("Admin.LandingPage.Fields.DisplayOrder")]
        public int DisplayOrder { get; set; }

        [NopResourceDisplayName("Admin.LandingPage.Fields.SubjectToAcl")]
        public bool SubjectToAcl { get; set; }

        [NopResourceDisplayName("Admin.LandingPage.Fields.LimitedToStores")]
        public bool LimitedToStores { get; set; }

        [NopResourceDisplayName("Admin.LandingPage.Fields.Published")]
        public bool Published { get; set; }
        [NopResourceDisplayName("Admin.LandingPage.Fields.SeName")]
        public string SeName { get; set; }

        [NopResourceDisplayName("Admin.LandingPage.Fields.Deleted")]
        public bool Deleted { get; set; }

        [NopResourceDisplayName("Admin.LandingPage.Fields.CreatedOnUtc")]
        public DateTime CreatedOnUtc { get; set; }

        [NopResourceDisplayName("Admin.LandingPage.Fields.UpdatedOnUtc")]
        public DateTime UpdatedOnUtc { get; set; }

        // ACL
        [NopResourceDisplayName("Admin.LandingPage.Fields.AclCustomerRoles")]
        public IList<int> SelectedCustomerRoleIds { get; set; }
        public IList<SelectListItem> AvailableCustomerRoles { get; set; }

        // Store Mapping
        [NopResourceDisplayName("Admin.LandingPage.Fields.LimitedToStores")]
        public IList<int> SelectedStoreIds { get; set; }
        public IList<SelectListItem> AvailableStores { get; set; }
        public IList<LandingPageLocalizedModel> Locales { get; set; }

        #endregion
    }
    public partial record LandingPageLocalizedModel : ILocalizedLocaleModel
    {
        public int LanguageId { get; set; }

        [NopResourceDisplayName("Admin.Catalog.Categories.Fields.Name")]
        public string Name { get; set; }

        [NopResourceDisplayName("Admin.Catalog.Categories.Fields.Description")]
        public string Description { get; set; }

        [NopResourceDisplayName("Admin.Catalog.Categories.Fields.MetaKeywords")]
        public string MetaKeywords { get; set; }

        [NopResourceDisplayName("Admin.Catalog.Categories.Fields.MetaTitle")]
        public string MetaTitle { get; set; }

        [NopResourceDisplayName("Admin.Catalog.Categories.Fields.MetaDescription")]
        public string MetaDescription { get; set; }

        [NopResourceDisplayName("Admin.Catalog.Categories.Fields.SeName")]
        public string SeName { get; set; }

    }
}
