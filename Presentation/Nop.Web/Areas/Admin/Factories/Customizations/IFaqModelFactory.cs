using Nop.Web.Areas.Admin.Models.FAQModule;

namespace Nop.Web.Areas.Admin.Factories
{
    public partial interface IFaqModelFactory
    {
        Task<FaqListModel> PrepareFaqListModelAsync(FaqSearchModel searchModel);
    }
}
