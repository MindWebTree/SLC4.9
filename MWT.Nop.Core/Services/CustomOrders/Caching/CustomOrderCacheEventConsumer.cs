using MWT.Nop.Core.Domain.CustomOrders;
using Nop.Services.Caching;

namespace Nop.Services.Customizations.Phone_Order.Caching
{
    public partial class CustomOrderCacheEventConsumer : CacheEventConsumer<CustomOrder>
    {
        protected override async Task ClearCacheAsync(CustomOrder entity, EntityEventType entityEventType)
        {
            await base.ClearCacheAsync(entity, entityEventType);
        }
    }
}