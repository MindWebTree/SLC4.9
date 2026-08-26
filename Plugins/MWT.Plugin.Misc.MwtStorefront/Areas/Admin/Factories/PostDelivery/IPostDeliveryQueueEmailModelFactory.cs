using MWT.Plugin.Misc.MwtStorefront.Area.Admin.Models.PostDelivery;

namespace MWT.Plugin.Misc.MwtStorefront.Area.Admin.Factories.PostDelievery
{
    public partial interface IPostDeliveryQueueEmailModelFactory
    {
        Task<PostDeliveryQueueEmailListModel> PreparePostDeliveryQueueEmailListModelAsync(PostDeliveryQueueEmailSearchModel searchModel);
    }
}
