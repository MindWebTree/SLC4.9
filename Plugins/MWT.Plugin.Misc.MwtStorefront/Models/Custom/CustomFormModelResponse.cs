using Nop.Web.Models.Catalog;
using System.Collections.Generic;

namespace MWT.Plugin.Misc.MwtStorefront.Models.Custom
{
    public partial record CustomFormModelResponse
    {
        public bool ShowInPopup { get; set; }
        public string ThankYouHtml { get; set; }
        public string ThankYouPageLink { get; set; }
        public string ErrorMessage { get; set; }
    }

}
