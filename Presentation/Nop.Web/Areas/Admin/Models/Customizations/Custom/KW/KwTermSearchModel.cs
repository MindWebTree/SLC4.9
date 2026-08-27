using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Web.Areas.Admin.Models.Customization.Custom.KW
{
    /// <summary>
    /// Represents a category search model
    /// </summary>
    public partial record KwTermSearchModel : BaseSearchModel
    {
        #region Ctor

        public KwTermSearchModel()
        {
            AvailableStores = new List<SelectListItem>();
            AvailablePublishedOptions = new List<SelectListItem>();
        }

        #endregion

        #region Properties

        [NopResourceDisplayName("Admin.Catalog.KwTerms.List.SearchKwTermName")]
        public string SearchKwTermName { get; set; }

        [NopResourceDisplayName("Admin.Catalog.KwTerms.List.SearchPublished")]
        public int SearchPublishedId { get; set; }

        public IList<SelectListItem> AvailablePublishedOptions { get; set; }

        [NopResourceDisplayName("Admin.Catalog.KwTerms.List.SearchStore")]
        public int SearchStoreId { get; set; }

        public IList<SelectListItem> AvailableStores { get; set; }

        public bool HideStoresList { get; set; }

        #endregion
    }
}