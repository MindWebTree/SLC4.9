using MWT.Plugin.Misc.MwtStorefront.Areas.Admin.Models.FAQModule;
using System.Threading.Tasks;
namespace MWT.Plugin.Misc.MwtStorefront.Areas.Admin.Factories
{
    public partial interface IFaqModelFactory
    {
        Task<FaqListModel> PrepareFaqListModelAsync(FaqSearchModel searchModel);
    }
}
