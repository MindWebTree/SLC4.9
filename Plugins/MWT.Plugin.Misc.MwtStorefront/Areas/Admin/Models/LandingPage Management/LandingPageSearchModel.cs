using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace MWT.Plugin.Misc.MwtStorefront.Area.Admin.Models.Catalog.LandingPage_Management
{
    /// <summary>
    /// Represents a category search model
    /// </summary>
    public partial record LandingPageSearchModel : BaseSearchModel
    {
        #region Ctor

        public LandingPageSearchModel()
        {
            AvailableStores = new List<SelectListItem>();
            AvailablePublishedOptions = new List<SelectListItem>();
        }

        #endregion

        #region Properties

        [NopResourceDisplayName("Admin.Catalog.LandingPages.List.SearchLandingPageName")]
        public string SearchLandingPageName { get; set; }

        [NopResourceDisplayName("Admin.Catalog.LandingPages.List.SearchPublished")]
        public int SearchPublishedId { get; set; }

        public IList<SelectListItem> AvailablePublishedOptions { get; set; }

        [NopResourceDisplayName("Admin.Catalog.LandingPages.List.SearchStore")]
        public int SearchStoreId { get; set; }

        public IList<SelectListItem> AvailableStores { get; set; }

        public bool HideStoresList { get; set; }

        #endregion
    }
}