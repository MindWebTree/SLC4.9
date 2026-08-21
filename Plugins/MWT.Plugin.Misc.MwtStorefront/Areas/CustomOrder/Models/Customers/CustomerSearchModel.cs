using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace MWT.Plugin.Misc.MwtStorefront.Areas.CustomOrder.Models.Customers
{
    public partial record CustomerSearchModel : BaseSearchModel
    {

        #region Properties

        [NopResourceDisplayName("CustomOrder.Customers.Customers.List.Search")]
        public string SearchTerm { get; set; }

        #endregion
    }
}
