using Nop.Web.Areas.Admin.Models.Catalog;
using Nop.Web.Areas.Admin.Models.Customization.Custom.PostDelivery;
using System.Threading.Tasks;

namespace Nop.Web.Areas.Admin.Factories.Customization.PostDelivery
{
    public partial interface IPostDeliveryQueueEmailModelFactory
    {
        Task<PostDeliveryQueueEmailListModel> PreparePostDeliveryQueueEmailListModelAsync(PostDeliveryQueueEmailSearchModel searchModel);
    }
}
